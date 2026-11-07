import { createReadStream, existsSync, statSync } from "node:fs";
import { createServer } from "node:http";
import { extname, join, normalize, resolve } from "node:path";

const root = resolve(process.argv[2]);
const port = Number(process.argv[3] ?? 8765);
const contentTypes = {
  ".html": "text/html; charset=utf-8",
  ".js": "application/javascript",
  ".css": "text/css; charset=utf-8",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".png": "image/png",
  ".ico": "image/x-icon",
};

const server = createServer((request, response) => {
  const requestedPath = decodeURIComponent(new URL(request.url, "http://127.0.0.1").pathname);
  const relativePath = requestedPath === "/" ? "index.html" : requestedPath.slice(1);
  const filePath = resolve(join(root, normalize(relativePath)));
  if (!filePath.startsWith(root) || !existsSync(filePath) || statSync(filePath).isDirectory()) {
    response.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
    response.end("Not found");
    return;
  }

  const compressed = filePath.endsWith(".gz");
  const logicalPath = compressed ? filePath.slice(0, -3) : filePath;
  response.setHeader("Content-Type", contentTypes[extname(logicalPath)] ?? "application/octet-stream");
  response.setHeader("Cache-Control", "no-store");
  if (compressed) response.setHeader("Content-Encoding", "gzip");
  response.writeHead(200);
  createReadStream(filePath).pipe(response);
});

server.listen(port, "127.0.0.1", () => {
  process.stdout.write(`Serving ${root} at http://127.0.0.1:${port}\n`);
});
