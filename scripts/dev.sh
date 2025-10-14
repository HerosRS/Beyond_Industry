#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

PORT="${PORT:-8080}"
HOST="${HOST:-127.0.0.1}"

if ! command -v node >/dev/null 2>&1; then
  echo "❌ Node.js nicht gefunden. Installiere: https://nodejs.org" >&2
  exit 1
fi

node scripts/dev-server.mjs --port "$PORT" --host "$HOST" & SERVER_PID=$!

# kurz warten und Browser öffnen (macOS: open, Linux: xdg-open)
sleep 0.5
URL="http://${HOST}:${PORT}/"
if command -v open >/dev/null 2>&1; then open "$URL"; fi
if command -v xdg-open >/dev/null 2>&1; then xdg-open "$URL" >/dev/null 2>&1 || true; fi

wait $SERVER_PID
