#!/usr/bin/env node
import { createServer } from "node:http";
import { stat, readdir, access } from "node:fs/promises";
import { constants as FS } from "node:fs";
import { createReadStream } from "node:fs";
import { resolve, join, extname, normalize, relative, sep, posix } from "node:path";
import { fileURLToPath } from "node:url";

/* ---------- env / cwd ---------- */
const __dirname = fileURLToPath(new URL(".", import.meta.url));
const ROOT = resolve(process.cwd());
const ICONS_DIR = join(ROOT, "icons");

/* ---------- CLI args ---------- */
function parseArgs(argv) {
  const out = {};
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--port" || a === "-p") { out.port = argv[++i]; continue; }
    if (a === "--host" || a === "-H") { out.host = argv[++i]; continue; }
    if (/^\d+$/.test(a)) { out.port = a; continue; } // fallback: blank numeric arg
  }
  return out;
}
const ARGS = parseArgs(process.argv.slice(2));

/* ---------- Port/Host robust bestimmen ---------- */
function pickPort() {
  const raw = (process.env.PORT && String(process.env.PORT)) || (ARGS.port && String(ARGS.port));
  if (!raw || raw === "") return 8080;
  if (raw.toLowerCase() === "auto") return 0;
  const p = parseInt(raw.trim(), 10);
  if (Number.isFinite(p) && p >= 0 && p < 65536) return p;
  console.warn(`[dev-server] Ungültiger Port "${raw}" → fallback 8080`);
  return 8080;
}
function pickHost() {
  const raw = (process.env.HOST && String(process.env.HOST)) || (ARGS.host && String(ARGS.host));
  return raw && raw.trim() ? raw.trim() : "127.0.0.1";
}
const PORT = pickPort();
const HOST = pickHost();

/* ---------- MIME ---------- */
const MIME = {
  ".html": "text/html; charset=utf-8",
  ".htm":  "text/html; charset=utf-8",
  ".js":   "application/javascript; charset=utf-8",
  ".mjs":  "application/javascript; charset=utf-8",
  ".css":  "text/css; charset=utf-8",
  ".map":  "application/json; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".svg":  "image/svg+xml",
  ".png":  "image/png",
  ".jpg":  "image/jpeg",
  ".jpeg": "image/jpeg",
  ".webp": "image/webp",
  ".gif":  "image/gif",
  ".ico":  "image/x-icon",
  ".txt":  "text/plain; charset=utf-8",
  ".wasm": "application/wasm"
};
const contentType = p => MIME[extname(p).toLowerCase()] || "application/octet-stream";

/* ---------- sichere Pfade ---------- */
function safePath(urlPath) {
  const decoded = decodeURIComponent((urlPath || "/").split("?")[0]);
  const full = normalize(join(ROOT, decoded));
  if (!full.startsWith(ROOT)) return null;
  return full;
}

/* ---------- Icons rekursiv auflisten ---------- */
async function scanIcons(root = ICONS_DIR) {
  // Wenn es den Ordner nicht gibt: leere Liste
  try { await access(root, FS.F_OK); } catch { return { entries: [] }; }

  const entries = [];
  async function walk(dir) {
    let ents;
    try { ents = await readdir(dir, { withFileTypes: true }); } catch { return; }
    for (const e of ents) {
      const full = join(dir, e.name);
      if (e.isDirectory()) {
        await walk(full);
      } else {
        const ext = extname(e.name).toLowerCase();
        if ([".png", ".jpg", ".jpeg", ".webp", ".svg", ".gif"].includes(ext)) {
          const relFs = relative(root, full).split(sep).join(posix.sep);
          const label = e.name.replace(/\.[^.]+$/, "");
          const url = "/icons/" + relFs.split("/").map(encodeURIComponent).join("/");
          entries.push({ path: relFs, label, url });
        }
      }
    }
  }
  await walk(root);
  entries.sort((a, b) => a.path.localeCompare(b.path, "de"));
  return { entries };
}

/* ---------- Response helpers ---------- */
function send(res, code, body, headers = {}) {
  res.writeHead(code, headers);
  res.end(body);
}
function notFound(res, msg = "Not found") {
  send(res, 404, msg, { "Content-Type": "text/plain; charset=utf-8" });
}

/* ---------- HTTP Server ---------- */
const server = createServer(async (req, res) => {
  try {
    res.setHeader("Access-Control-Allow-Origin", "*");
    res.setHeader("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
    res.setHeader("Access-Control-Allow-Headers", "Content-Type");
    res.setHeader("Cache-Control", "no-store");

    if (req.method === "OPTIONS") { res.writeHead(204).end(); return; }

    const url = req.url || "/";

    // Health
    if (url === "/__ping") {
      send(res, 200, "ok", { "Content-Type": "text/plain; charset=utf-8" });
      return;
    }

    // Icons-API
    if (url === "/icons/index.json" || url === "/icons/__list") {
      const json = await scanIcons();
      const body = JSON.stringify(json);
      send(res, 200, body, { "Content-Type": "application/json; charset=utf-8" });
      return;
    }

    // Statisches Serving
    let filePath = safePath(url);
    if (!filePath) { send(res, 400, "Bad request", { "Content-Type": "text/plain; charset=utf-8" }); return; }

    let st;
    try {
      st = await stat(filePath);
      if (st.isDirectory()) {
        filePath = join(filePath, "index.html");
        st = await stat(filePath);
      }
    } catch {
      // SPA-Fallback: jede GET-Route ohne Dateiendung → index.html
      const hasExt = !!extname(url.split("?")[0]);
      if (req.method === "GET" && !hasExt) {
        const fallback = join(ROOT, "index.html");
        try { await stat(fallback); }
        catch { return notFound(res, "Not found (no index.html)"); }
        filePath = fallback;
      } else {
        return notFound(res);
      }
    }

    const type = contentType(filePath);
    res.writeHead(200, { "Content-Type": type });
    if (req.method === "HEAD") { res.end(); return; }
    createReadStream(filePath).pipe(res);

  } catch (err) {
    console.error("[dev-server] Internal error:", err);
    send(res, 500, "Internal Server Error", { "Content-Type": "text/plain; charset=utf-8" });
  }
});

/* ---------- Start ---------- */
server.listen(PORT, HOST, () => {
  const addr = server.address();
  const actualPort = typeof addr === "object" && addr ? addr.port : PORT;
  console.log("=====================================================================");
  console.log(" Kolonie-Game Dev Server ");
  console.log(` Root     : ${ROOT}`);
  console.log(` Host/URL : http://${HOST}:${actualPort}/`);
  console.log(` Health   : http://${HOST}:${actualPort}/__ping`);
  console.log(` IconsAPI : http://${HOST}:${actualPort}/icons/index.json`);
  console.log("=====================================================================");
});
