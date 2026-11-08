import { createClient } from "@supabase/supabase-js";

const CODE_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
const CODE_PATTERN = /^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{8}$/;
const PIN_PATTERN = /^\d{6}$/;
const DEVICE_FINGERPRINT_PATTERN = /^[a-f0-9]{64}$/;
const MAX_BODY_BYTES = 160 * 1024;
const MAX_SAVE_BYTES = 128 * 1024;
const PBKDF2_ITERATIONS = 120_000;

const corsHeaders = {
  "access-control-allow-origin": "*",
  "access-control-allow-headers": "content-type",
  "access-control-allow-methods": "POST, OPTIONS",
  "cache-control": "no-store",
};

function json(status: number, body: Record<string, unknown>): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { ...corsHeaders, "content-type": "application/json; charset=utf-8" },
  });
}

function bytesToBase64(bytes: Uint8Array): string {
  return btoa(String.fromCharCode(...bytes));
}

function base64ToBytes(value: string): Uint8Array {
  return Uint8Array.from(atob(value), (character) => character.charCodeAt(0));
}

async function sha256(value: string): Promise<string> {
  const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(value));
  return Array.from(new Uint8Array(digest), (byte) => byte.toString(16).padStart(2, "0")).join("");
}

async function deriveDeviceKey(serviceKey: string, fingerprint: string): Promise<string> {
  const key = await crypto.subtle.importKey(
    "raw",
    new TextEncoder().encode(serviceKey),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign"],
  );
  const signature = await crypto.subtle.sign("HMAC", key, new TextEncoder().encode(`farm-device:${fingerprint}`));
  return Array.from(new Uint8Array(signature), (byte) => byte.toString(16).padStart(2, "0")).join("");
}

async function derivePinHash(pin: string, salt: Uint8Array): Promise<Uint8Array> {
  const key = await crypto.subtle.importKey(
    "raw",
    new TextEncoder().encode(pin),
    "PBKDF2",
    false,
    ["deriveBits"],
  );
  const bits = await crypto.subtle.deriveBits(
    { name: "PBKDF2", hash: "SHA-256", salt, iterations: PBKDF2_ITERATIONS },
    key,
    256,
  );
  return new Uint8Array(bits);
}

function constantTimeEqual(left: Uint8Array, right: Uint8Array): boolean {
  if (left.length !== right.length) return false;
  let difference = 0;
  for (let index = 0; index < left.length; index += 1) difference |= left[index] ^ right[index];
  return difference === 0;
}

function randomCode(): string {
  const random = crypto.getRandomValues(new Uint8Array(8));
  return Array.from(random, (byte) => CODE_ALPHABET[byte % CODE_ALPHABET.length]).join("");
}

function randomPin(): string {
  const random = crypto.getRandomValues(new Uint32Array(1))[0] % 1_000_000;
  return random.toString().padStart(6, "0");
}

function normalizeCode(value: unknown): string {
  return typeof value === "string" ? value.toUpperCase().replace(/[\s-]/g, "") : "";
}

function readServiceKey(): string | null {
  const legacy = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
  if (legacy) return legacy;
  const keys = Deno.env.get("SUPABASE_SECRET_KEYS");
  if (!keys) return null;
  try {
    const parsed = JSON.parse(keys);
    if (Array.isArray(parsed)) {
      const key = parsed.find((candidate) => typeof candidate === "string" && candidate.length > 20);
      return key ?? null;
    }
  } catch {
    return keys.length > 20 ? keys : null;
  }
  return null;
}

function parseSaveJson(value: unknown): { parsed: Record<string, unknown>; text: string } | null {
  if (typeof value !== "string" || new TextEncoder().encode(value).length > MAX_SAVE_BYTES) return null;
  try {
    const parsed = JSON.parse(value);
    if (!parsed || Array.isArray(parsed) || typeof parsed !== "object") return null;
    const version = (parsed as Record<string, unknown>).version;
    if (!Number.isInteger(version) || (version as number) < 1 || (version as number) > 3) return null;
    return { parsed: parsed as Record<string, unknown>, text: JSON.stringify(parsed) };
  } catch {
    return null;
  }
}

Deno.serve(async (request: Request) => {
  if (request.method === "OPTIONS") return new Response("ok", { headers: corsHeaders });
  if (request.method !== "POST") return json(405, { ok: false, message: "POST 요청만 지원합니다." });

  try {
    const rawBody = await request.text();
    if (new TextEncoder().encode(rawBody).length > MAX_BODY_BYTES) {
      return json(413, { ok: false, message: "저장 데이터가 너무 큽니다." });
    }

    let body: Record<string, unknown>;
    try {
      body = JSON.parse(rawBody);
    } catch {
      return json(400, { ok: false, message: "요청 형식이 올바르지 않습니다." });
    }

    const serviceKey = readServiceKey();
    const supabaseUrl = Deno.env.get("SUPABASE_URL");
    if (!serviceKey || !supabaseUrl) return json(503, { ok: false, message: "동기화 서버를 사용할 수 없습니다." });
    const supabase = createClient(supabaseUrl, serviceKey, {
      auth: { persistSession: false, autoRefreshToken: false },
    });

    const forwarded = request.headers.get("x-forwarded-for")?.split(",")[0]?.trim();
    const clientIp = forwarded || request.headers.get("cf-connecting-ip") || "unknown";
    const ipKey = await sha256(`ip:${clientIp}`);
    const { data: ipAllowed, error: ipRateError } = await supabase.rpc("consume_cloud_sync_rate_limit", {
      p_key_hash: ipKey,
      p_limit: 30,
      p_window_seconds: 60,
    });
    if (ipRateError || !ipAllowed) return json(429, { ok: false, message: "요청이 너무 많습니다. 잠시 후 다시 시도해 주세요." });

    const action = body.action;
    if (action === "device-connect" || action === "device-save") {
      const fingerprint = typeof body.deviceFingerprint === "string" ? body.deviceFingerprint : "";
      if (!DEVICE_FINGERPRINT_PATTERN.test(fingerprint)) {
        return json(400, { ok: false, message: "기기 식별 정보가 올바르지 않습니다." });
      }

      const deviceKey = await deriveDeviceKey(serviceKey, fingerprint);
      const { data: deviceAllowed, error: deviceRateError } = await supabase.rpc("consume_cloud_sync_rate_limit", {
        p_key_hash: await sha256(`device-rate:${deviceKey}`),
        p_limit: 12,
        p_window_seconds: 60,
      });
      if (deviceRateError || !deviceAllowed) {
        return json(429, { ok: false, message: "요청이 너무 많습니다. 잠시 후 다시 시도해 주세요." });
      }

      const { data: deviceRow, error: deviceReadError } = await supabase
        .from("cloud_saves")
        .select("sync_code,save_json,revision")
        .eq("device_key", deviceKey)
        .maybeSingle();
      if (deviceReadError) throw deviceReadError;

      if (action === "device-save") {
        const revision = body.revision;
        const save = parseSaveJson(body.saveJson);
        if (!deviceRow || !Number.isSafeInteger(revision) || (revision as number) < 1 || !save) {
          return json(400, { ok: false, message: "자동 저장 요청이 올바르지 않습니다." });
        }
        const { data: nextRevision, error: updateError } = await supabase.rpc("update_cloud_save_if_revision", {
          p_sync_code: deviceRow.sync_code,
          p_expected_revision: revision,
          p_save_json: save.parsed,
        });
        if (updateError) throw updateError;
        if (nextRevision === 0) {
          return json(409, { ok: false, conflict: true, message: "다른 브라우저에 더 최신 저장이 있습니다. 먼저 불러와 주세요." });
        }
        return json(200, { ok: true, autoLinked: true, syncCode: deviceRow.sync_code, revision: nextRevision });
      }

      if (deviceRow) {
        return json(200, {
          ok: true,
          autoLinked: true,
          syncCode: deviceRow.sync_code,
          revision: deviceRow.revision,
          saveJson: JSON.stringify(deviceRow.save_json),
        });
      }

      const save = parseSaveJson(body.saveJson);
      if (!save) return json(400, { ok: false, message: "저장 데이터가 올바르지 않습니다." });
      const newPin = randomPin();
      const salt = crypto.getRandomValues(new Uint8Array(16));
      const pinHash = await derivePinHash(newPin, salt);
      for (let attempt = 0; attempt < 6; attempt += 1) {
        const syncCode = randomCode();
        const { error } = await supabase.from("cloud_saves").insert({
          sync_code: syncCode,
          pin_salt: bytesToBase64(salt),
          pin_hash: bytesToBase64(pinHash),
          device_key: deviceKey,
          save_json: save.parsed,
          revision: 1,
        });
        if (!error) {
          return json(200, { ok: true, autoLinked: true, syncCode, pin: newPin, revision: 1 });
        }
        if (error.code !== "23505") throw error;
        const { data: racedRow } = await supabase
          .from("cloud_saves")
          .select("sync_code,save_json,revision")
          .eq("device_key", deviceKey)
          .maybeSingle();
        if (racedRow) {
          return json(200, {
            ok: true,
            autoLinked: true,
            syncCode: racedRow.sync_code,
            revision: racedRow.revision,
            saveJson: JSON.stringify(racedRow.save_json),
          });
        }
      }
      return json(503, { ok: false, message: "자동 저장 공간을 만들지 못했습니다. 다시 시도해 주세요." });
    }

    if (action === "create") {
      const save = parseSaveJson(body.saveJson);
      if (!save) return json(400, { ok: false, message: "저장 데이터가 올바르지 않습니다." });
      const pin = randomPin();
      const salt = crypto.getRandomValues(new Uint8Array(16));
      const pinHash = await derivePinHash(pin, salt);

      for (let attempt = 0; attempt < 6; attempt += 1) {
        const syncCode = randomCode();
        const { error } = await supabase.from("cloud_saves").insert({
          sync_code: syncCode,
          pin_salt: bytesToBase64(salt),
          pin_hash: bytesToBase64(pinHash),
          save_json: save.parsed,
          revision: 1,
        });
        if (!error) return json(200, { ok: true, syncCode, pin, revision: 1 });
        if (error.code !== "23505") throw error;
      }
      return json(503, { ok: false, message: "동기화 코드를 만들지 못했습니다. 다시 시도해 주세요." });
    }

    const syncCode = normalizeCode(body.syncCode);
    const pin = typeof body.pin === "string" ? body.pin : "";
    if (!CODE_PATTERN.test(syncCode) || !PIN_PATTERN.test(pin)) {
      return json(400, { ok: false, message: "코드 또는 PIN 형식이 올바르지 않습니다." });
    }

    const codeKey = await sha256(`code:${syncCode}`);
    const { data: codeAllowed, error: codeRateError } = await supabase.rpc("consume_cloud_sync_rate_limit", {
      p_key_hash: codeKey,
      p_limit: 12,
      p_window_seconds: 60,
    });
    if (codeRateError || !codeAllowed) return json(429, { ok: false, message: "요청이 너무 많습니다. 잠시 후 다시 시도해 주세요." });

    const { data: row, error: readError } = await supabase
      .from("cloud_saves")
      .select("pin_salt,pin_hash,save_json,revision")
      .eq("sync_code", syncCode)
      .maybeSingle();
    if (readError) throw readError;
    if (!row) return json(401, { ok: false, message: "코드 또는 PIN이 올바르지 않습니다." });

    const candidateHash = await derivePinHash(pin, base64ToBytes(row.pin_salt));
    if (!constantTimeEqual(candidateHash, base64ToBytes(row.pin_hash))) {
      return json(401, { ok: false, message: "코드 또는 PIN이 올바르지 않습니다." });
    }

    if (action === "load") {
      return json(200, {
        ok: true,
        syncCode,
        revision: row.revision,
        saveJson: JSON.stringify(row.save_json),
      });
    }

    if (action === "save") {
      const revision = body.revision;
      const save = parseSaveJson(body.saveJson);
      if (!Number.isSafeInteger(revision) || (revision as number) < 1 || !save) {
        return json(400, { ok: false, message: "저장 요청이 올바르지 않습니다." });
      }
      const { data: nextRevision, error: updateError } = await supabase.rpc("update_cloud_save_if_revision", {
        p_sync_code: syncCode,
        p_expected_revision: revision,
        p_save_json: save.parsed,
      });
      if (updateError) throw updateError;
      if (nextRevision === 0) {
        return json(409, { ok: false, conflict: true, message: "다른 브라우저에 더 최신 저장이 있습니다. 먼저 불러와 주세요." });
      }
      return json(200, { ok: true, syncCode, revision: nextRevision });
    }

    return json(400, { ok: false, message: "지원하지 않는 동기화 작업입니다." });
  } catch (_error) {
    return json(500, { ok: false, message: "동기화 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요." });
  }
});
