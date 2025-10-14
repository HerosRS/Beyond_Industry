# Datei-Name: Start Kolonie Server.command
#!/usr/bin/env bash
set -euo pipefail

# In den Ordner dieser Datei wechseln (Projekt-Root)
cd "$(dirname "$0")"

# Server-Script lokalisieren (scripts/dev-server.mjs oder dev-server.mjs im Root)
if [ -f "scripts/dev-server.mjs" ]; then
  SERVER="scripts/dev-server.mjs"
elif [ -f "dev-server.mjs" ]; then
  SERVER="dev-server.mjs"
else
  osascript -e 'display alert "Startfehler" message "Konnte dev-server.mjs nicht finden.\nErwarte scripts/dev-server.mjs oder ./dev-server.mjs im Projekt-Root." as critical buttons {"OK"}'
  exit 1
fi

# Standard-Host/Port – per Umgebungsvariablen überschreibbar
PORT="${PORT:-8080}"
HOST="${HOST:-127.0.0.1}"

# Prüfen, ob Node existiert
if ! command -v node >/dev/null 2>&1; then
  osascript -e 'display alert "Node.js fehlt" message "Bitte Node.js installieren: https://nodejs.org" as critical buttons {"OK"}'
  exit 1
fi

# Server starten
node "$SERVER" --port "$PORT" --host "$HOST" & SERVER_PID=$!

# Kurz warten und Browser öffnen
sleep 0.6
open "http://${HOST}:${PORT}/" || true

# Bis zum Ende warten (Fenster bleibt offen, Strg+C/Schließen beendet)
wait $SERVER_PID
