# Ulanzi D200 Manager - GUI Schnellstart

## Installation in 3 Schritten

### 1. Voraussetzungen installieren

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install python3 python3-pip python3-venv dotnet-sdk-8.0 xdotool libhidapi-hidraw0
```

**Fedora/RHEL:**
```bash
sudo dnf install python3 python3-pip dotnet-sdk-8.0 xdotool hidapi
```

### 2. Installation ausführen

```bash
git clone https://github.com/IHR-REPO/ulanzi-d200-linux.git
cd ulanzi-d200-linux
./install-with-gui.sh
```

Das Script installiert automatisch:
- ✅ Python Backend mit ulanzi-manager
- ✅ Installer-GUI für zukünftige Installationen
- ✅ Manager-GUI zur Verwaltung
- ✅ Desktop-Eintrag im Anwendungsmenü

### 3. Anwendung starten

**Option A: Über Anwendungsmenü**
- Öffnen Sie Ihr Anwendungsmenü
- Suchen Sie nach "Ulanzi D200 Manager"
- Klicken Sie auf das Icon

**Option B: Über Terminal**
```bash
ulanzi-manager-ui
```

Die GUI öffnet sich im Browser unter `http://localhost:5001`

## Was macht die GUI?

### 🎯 Manager-GUI Features

**Dashboard:**
- 📊 Status-Übersicht (Daemon, Gerät)
- ▶️ Daemon Start/Stop Steuerung
- 🔄 Live-Status-Updates

**Konfiguration:**
- 📝 Integrierter Editor für config.yaml
- ✅ Validierung der Konfiguration
- 💾 Speichern mit einem Klick
- 📁 Direkter Zugriff auf Konfigurationsordner

**Schnellaktionen:**
- ⚡ Konfiguration validieren
- 🚀 Konfiguration anwenden
- 📂 Ordner öffnen

## Erste Schritte nach Installation

1. **Gerät verbinden**
   - Schließen Sie Ihr Ulanzi D200 an
   - Status sollte "Verbunden" anzeigen

2. **Konfiguration anpassen**
   - Öffnen Sie die Manager-GUI
   - Bearbeiten Sie die Konfiguration im Editor
   - Klicken Sie auf "Speichern"

3. **Konfiguration anwenden**
   - Klicken Sie auf "Konfiguration anwenden"
   - Warten Sie auf Bestätigung

4. **Daemon starten**
   - Klicken Sie auf "Daemon starten"
   - Status sollte "Läuft" anzeigen

## Verzeichnisstruktur

Nach der Installation finden Sie:

```
~/.config/ulanzi/
  └── config.yaml              # Ihre Konfiguration

~/.local/ulanzi/
  ├── venv/                    # Python Virtual Environment
  ├── installer/               # Installer-GUI
  └── manager-ui/              # Manager-GUI

~/.local/bin/
  ├── ulanzi-daemon            # Daemon-Starter
  ├── ulanzi-installer         # Installer-GUI-Starter
  └── ulanzi-manager-ui        # Manager-GUI-Starter

~/.local/share/applications/
  └── ulanzi-manager.desktop   # Desktop-Eintrag
```

## Befehle

### GUI starten
```bash
ulanzi-manager-ui           # Manager-GUI
ulanzi-installer            # Installer-GUI
```

### CLI (Backend)
```bash
ulanzi-daemon ~/.config/ulanzi/config.yaml     # Daemon starten
ulanzi-manager validate ~/.config/ulanzi/config.yaml  # Validieren
ulanzi-manager configure ~/.config/ulanzi/config.yaml # Anwenden
```

## Problemlösung

### GUI startet nicht

**"Port bereits in Verwendung"**
```bash
sudo lsof -i :5001
# Beenden Sie den Prozess oder ändern Sie den Port
```

**".NET nicht gefunden"**
```bash
dotnet --version
# Installieren Sie .NET 8.0 SDK falls nicht vorhanden
```

### Daemon startet nicht

**"Gerät nicht gefunden"**
1. Überprüfen Sie USB-Verbindung
2. Prüfen Sie udev-Regeln:
```bash
ls -la /etc/udev/rules.d/99-ulanzi.rules
```
3. Gerät neu verbinden

**"Konfiguration ungültig"**
1. Öffnen Sie Manager-GUI
2. Klicken Sie auf "Konfiguration validieren"
3. Korrigieren Sie Fehler in der Konfiguration

## Deinstallation

```bash
# GUIs entfernen
rm -rf ~/.local/ulanzi/installer
rm -rf ~/.local/ulanzi/manager-ui
rm ~/.local/bin/ulanzi-installer
rm ~/.local/bin/ulanzi-manager-ui
rm ~/.local/share/applications/ulanzi-manager.desktop

# Komplette Deinstallation (inkl. Backend)
rm -rf ~/.local/ulanzi
rm -rf ~/.config/ulanzi
rm ~/.local/bin/ulanzi-daemon
sudo rm /etc/udev/rules.d/99-ulanzi.rules
```

## Weitere Dokumentation

- 📖 [Vollständige GUI-Dokumentation](docs/GUI_DOCUMENTATION.md)
- 🐛 [Debugging & Troubleshooting](docs/DEBUG.md)
- 🚀 [Quick Start Guide](docs/QUICKSTART.md)
- 📋 [Hauptdokumentation](README.md)

## Support

Bei Problemen:
1. Prüfen Sie die Logs in der Manager-GUI
2. Konsultieren Sie die Dokumentation
3. Erstellen Sie ein Issue auf GitHub
