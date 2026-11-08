using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FarmGame2D.Core
{
    public sealed class CloudSaveClient
    {
        private const string Endpoint = "https://eqvrgccwzfnjrdekxamj.supabase.co/functions/v1/farm-save-sync";
        private const string CodeKey = "farmgame2d.cloud.code";
        private const string PinKey = "farmgame2d.cloud.pin";
        private const string RevisionKey = "farmgame2d.cloud.revision";
        private const string ModeKey = "farmgame2d.cloud.mode";
        private const string FingerprintKey = "farmgame2d.cloud.device";

        public bool IsLinked => IsDeviceLinked || (IsValidCode(SyncCode) && IsValidPin(Pin));
        public bool IsDeviceLinked => PlayerPrefs.GetString(ModeKey, string.Empty) == "device"
            && IsValidDeviceFingerprint(DeviceFingerprint)
            && Revision > 0;
        public string SyncCode => PlayerPrefs.GetString(CodeKey, string.Empty);
        private string Pin => PlayerPrefs.GetString(PinKey, string.Empty);
        private string DeviceFingerprint => PlayerPrefs.GetString(FingerprintKey, string.Empty);
        private int Revision => PlayerPrefs.GetInt(RevisionKey, 0);

        public IEnumerator AutoConnect(string saveJson, Action<CloudSyncResult> completed)
        {
            var fingerprint = CreateDeviceFingerprint();
            yield return Send(new CloudSyncRequest
            {
                action = "device-connect",
                deviceFingerprint = fingerprint,
                saveJson = saveJson
            }, result =>
            {
                if (result.Success)
                {
                    StoreDeviceLink(fingerprint, result.SyncCode, result.Pin, result.Revision);
                }

                completed(result);
            });
        }

        public IEnumerator Create(string saveJson, Action<CloudSyncResult> completed)
        {
            var request = new CloudSyncRequest { action = "create", saveJson = saveJson };
            yield return Send(request, result =>
            {
                if (result.Success)
                {
                    StoreManualLink(result.SyncCode, result.Pin, result.Revision);
                }

                completed(result);
            });
        }

        public IEnumerator LinkAndLoad(string code, string pin, Action<CloudSyncResult> completed)
        {
            var normalizedCode = NormalizeCode(code);
            var normalizedPin = NormalizePin(pin);
            if (!IsValidCode(normalizedCode) || !IsValidPin(normalizedPin))
            {
                completed(CloudSyncResult.Failed("동기화 코드 8자리와 PIN 6자리를 확인해 주세요."));
                yield break;
            }

            yield return Send(new CloudSyncRequest
            {
                action = "load",
                syncCode = normalizedCode,
                pin = normalizedPin
            }, result =>
            {
                if (result.Success)
                {
                    StoreManualLink(normalizedCode, normalizedPin, result.Revision);
                }

                completed(result);
            });
        }

        public IEnumerator Load(string fallbackSaveJson, Action<CloudSyncResult> completed)
        {
            if (!IsLinked)
            {
                completed(CloudSyncResult.Failed("먼저 동기화 코드를 연결해 주세요."));
                yield break;
            }

            var request = IsDeviceLinked
                ? new CloudSyncRequest
                {
                    action = "device-connect",
                    deviceFingerprint = DeviceFingerprint,
                    saveJson = fallbackSaveJson
                }
                : new CloudSyncRequest
            {
                action = "load",
                syncCode = SyncCode,
                pin = Pin
            };
            yield return Send(request, result =>
            {
                if (result.Success)
                {
                    PlayerPrefs.SetInt(RevisionKey, result.Revision);
                    PlayerPrefs.Save();
                }

                completed(result);
            });
        }

        public IEnumerator Save(string saveJson, Action<CloudSyncResult> completed)
        {
            if (!IsLinked)
            {
                completed(CloudSyncResult.Failed("먼저 동기화 코드를 연결해 주세요."));
                yield break;
            }

            var request = IsDeviceLinked
                ? new CloudSyncRequest
                {
                    action = "device-save",
                    deviceFingerprint = DeviceFingerprint,
                    revision = Revision,
                    saveJson = saveJson
                }
                : new CloudSyncRequest
            {
                action = "save",
                syncCode = SyncCode,
                pin = Pin,
                revision = Revision,
                saveJson = saveJson
            };
            yield return Send(request, result =>
            {
                if (result.Success)
                {
                    PlayerPrefs.SetInt(RevisionKey, result.Revision);
                    PlayerPrefs.Save();
                }

                completed(result);
            });
        }

        public void Disconnect()
        {
            PlayerPrefs.DeleteKey(CodeKey);
            PlayerPrefs.DeleteKey(PinKey);
            PlayerPrefs.DeleteKey(RevisionKey);
            PlayerPrefs.DeleteKey(ModeKey);
            PlayerPrefs.DeleteKey(FingerprintKey);
            PlayerPrefs.Save();
        }

        public static string NormalizeCode(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.ToUpperInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);
        }

        public static string NormalizePin(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var digits = string.Empty;
            foreach (var character in value)
            {
                if (character >= '0' && character <= '9')
                {
                    digits += character;
                }
            }

            return digits;
        }

        public static bool IsValidCode(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 8)
            {
                return false;
            }

            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            foreach (var character in value)
            {
                if (alphabet.IndexOf(character) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidPin(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 6)
            {
                return false;
            }

            foreach (var character in value)
            {
                if (character < '0' || character > '9')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidDeviceFingerprint(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 64)
            {
                return false;
            }

            foreach (var character in value)
            {
                if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f')))
                {
                    return false;
                }
            }

            return true;
        }

        public static string HashDeviceSignature(string signature)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(signature ?? string.Empty));
            var builder = new StringBuilder(hash.Length * 2);
            foreach (var value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }

        private static string CreateDeviceFingerprint()
        {
            var resolution = Screen.currentResolution;
            var timeZoneMinutes = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
            var signature = string.Join("|",
                SystemInfo.operatingSystemFamily,
                SystemInfo.processorCount,
                resolution.width,
                resolution.height,
                Application.systemLanguage,
                timeZoneMinutes);
            return HashDeviceSignature(signature);
        }

        private static IEnumerator Send(CloudSyncRequest payload, Action<CloudSyncResult> completed)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
            using var request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(bytes),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 15
            };
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            CloudSyncResponse response = null;
            try
            {
                response = JsonUtility.FromJson<CloudSyncResponse>(request.downloadHandler.text);
            }
            catch (Exception)
            {
                // The generic network message below deliberately avoids exposing server details.
            }

            if (request.result == UnityWebRequest.Result.Success && response?.ok == true)
            {
                completed(CloudSyncResult.Succeeded(response));
                yield break;
            }

            var message = !string.IsNullOrWhiteSpace(response?.message)
                ? response.message
                : "클라우드 저장 서버에 연결하지 못했습니다.";
            completed(CloudSyncResult.Failed(message, response?.conflict == true));
        }

        private static void StoreManualLink(string code, string pin, int revision)
        {
            PlayerPrefs.SetString(CodeKey, code);
            PlayerPrefs.SetString(PinKey, pin);
            PlayerPrefs.SetInt(RevisionKey, revision);
            PlayerPrefs.SetString(ModeKey, "manual");
            PlayerPrefs.DeleteKey(FingerprintKey);
            PlayerPrefs.Save();
        }

        private static void StoreDeviceLink(string fingerprint, string code, string pin, int revision)
        {
            PlayerPrefs.SetString(FingerprintKey, fingerprint);
            PlayerPrefs.SetString(ModeKey, "device");
            PlayerPrefs.SetString(CodeKey, code ?? string.Empty);
            if (!string.IsNullOrEmpty(pin))
            {
                PlayerPrefs.SetString(PinKey, pin);
            }
            PlayerPrefs.SetInt(RevisionKey, revision);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class CloudSyncRequest
        {
            public string action;
            public string syncCode;
            public string pin;
            public int revision;
            public string saveJson;
            public string deviceFingerprint;
        }

        [Serializable]
        internal sealed class CloudSyncResponse
        {
            public bool ok;
            public bool conflict;
            public string message;
            public string syncCode;
            public string pin;
            public int revision;
            public string saveJson;
            public bool autoLinked;
        }

        public sealed class CloudSyncResult
        {
            public bool Success { get; private set; }
            public bool Conflict { get; private set; }
            public string Message { get; private set; }
            public string SyncCode { get; private set; }
            public string Pin { get; private set; }
            public int Revision { get; private set; }
            public string SaveJson { get; private set; }

            internal static CloudSyncResult Succeeded(CloudSyncResponse response)
            {
                return new CloudSyncResult
                {
                    Success = true,
                    SyncCode = response.syncCode,
                    Pin = response.pin,
                    Revision = response.revision,
                    SaveJson = response.saveJson
                };
            }

            public static CloudSyncResult Failed(string message, bool conflict = false)
            {
                return new CloudSyncResult { Message = message, Conflict = conflict };
            }
        }
    }
}
