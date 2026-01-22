# Ulanzi D200 Manager - GUI Dokumentation

## Übersicht

Das Ulanzi D200 Manager Projekt verfügt über zwei grafische Benutzeroberflächen (GUIs), die mit C# und Blazor entwickelt wurden:

1. **Installer-GUI** (`UlanziInstaller`) - Führt den Installationsprozess durch
2. **Manager-GUI** (`UlanziManagerUI`) - Verwaltet die Ulanzi D200 Anwendung

## Voraussetzungen

- .NET 8.0 SDK oder höher
- Linux mit X11 oder Wayland
- Python 3.8+ (für das Backend)

### .NET SDK Installation

#### Ubuntu/Debian
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

#### Fedora/RHEL
```bash
sudo dnf install dotnet-sdk-8.0
```

#### Arch Linux
```bash
sudo pacman -S dotnet-sdk
```

## Installation

### Automatische Installation (empfohlen)

Das neue Installationsscript installiert automatisch sowohl das Python-Backend als auch die GUIs:

```bash
./install-with-gui.sh
```

Um nur das Backend ohne GUIs zu installieren:
```bash
./install-with-gui.sh --no-gui
```

### Manuelle Installation

#### 1. Backend installieren
```bash
./install.sh
```

#### 2. GUIs bauen und installieren

**Installer-GUI:**
```bash
cd UlanziInstaller
dotnet publish -c Release -o ~/.local/ulanzi/installer --self-contained false
```

**Manager-GUI:**
```bash
cd UlanziManagerUI
dotnet publish -c Release -o ~/.local/ulanzi/manager-ui --self-contained false
```

## Verwendung

### Installer-GUI

Die Installer-GUI führt Sie durch den gesamten Installationsprozess:

```bash
ulanzi-installer
```

oder manuell:
```bash
cd ~/.local/ulanzi/installer
dotnet UlanziInstaller.dll
```

Die GUI wird standardmäßig auf `http://localhost:5000` gestartet.

**Features:**
- Schritt-für-Schritt Installation
- Fortschrittsanzeige
- Automatische Konfiguration
- Fehlerbehandlung
- Installationsprotokoll

### Manager-GUI

Die Manager-GUI ist die Hauptanwendung zur Verwaltung Ihres Ulanzi D200:

```bash
ulanzi-manager-ui
```

oder manuell:
```bash
cd ~/.local/ulanzi/manager-ui
dotnet UlanziManagerUI.dll
```

Die GUI wird standardmäßig auf `http://localhost:5001` gestartet.

**Features:**
- Daemon-Steuerung (Start/Stop)
- Gerätestatus-Überwachung
- Konfigurationseditor mit Syntax-Highlighting
- Konfigurationsvalidierung
- Schnelle Konfigurationsanwendung
- Direkter Zugriff auf Konfigurationsordner

## Projektstruktur

### UlanziInstaller

```
UlanziInstaller/
├── Components/
│   ├── Pages/
│   │   └── Home.razor          # Hauptseite der Installation
│   └── App.razor               # App-Root
├── Services/
│   └── InstallationService.cs  # Installation Logik
├── Program.cs                  # App-Konfiguration
└── UlanziInstaller.csproj      # Projekt-Datei
```

### UlanziManagerUI

```
UlanziManagerUI/
├── Components/
│   ├── Pages/
│   │   └── Home.razor          # Hauptdashboard
│   └── App.razor               # App-Root
├── Services/
│   └── UlanziManagerService.cs # Backend-Integration
├── Program.cs                  # App-Konfiguration
└── UlanziManagerUI.csproj      # Projekt-Datei
```

## Entwicklung

### Voraussetzungen für Entwicklung

- .NET 8.0 SDK
- Visual Studio Code (empfohlen) oder Visual Studio
- C# Dev Kit Extension (für VS Code)

### Projekte starten (Entwicklungsmodus)

**Installer-GUI:**
```bash
cd UlanziInstaller
dotnet run
```

**Manager-GUI:**
```bash
cd UlanziManagerUI
dotnet run
```

Hot-Reload ist standardmäßig aktiviert. Änderungen werden automatisch übernommen.

### Debugging

In VS Code:
1. Öffnen Sie das Projekt
2. Drücken Sie F5
3. Wählen Sie ".NET Core Launch (web)"

### Anpassungen

#### Ports ändern

Bearbeiten Sie `Properties/launchSettings.json` in jedem Projekt:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5000"
    }
  }
}
```

#### Styling anpassen

Die GUIs verwenden Bootstrap 5 und Bootstrap Icons. Anpassungen können in `wwwroot/app.css` vorgenommen werden.

## Architektur

### Installer-GUI

Die Installer-GUI verwendet Blazor Server und führt folgende Schritte aus:

1. **Validierung** - Prüft Voraussetzungen
2. **Udev-Regel** - Installiert USB-Gerätezugriff
3. **Verzeichnisse** - Erstellt Konfigurationsverzeichnisse
4. **Virtual Environment** - Richtet Python venv ein
5. **Paketinstallation** - Installiert Python-Paket
6. **Konfiguration** - Generiert Standardkonfiguration
7. **GUI-Installation** - Installiert Manager-GUI (optional)

### Manager-GUI

Die Manager-GUI ist eine Blazor Server-Anwendung mit folgenden Komponenten:

- **UlanziManagerService**: Backend-Integration
  - Daemon-Steuerung (Start/Stop/Status)
  - Konfigurationsverwaltung (Laden/Speichern/Validieren)
  - Gerätestatusabfrage
  
- **Home.razor**: Hauptdashboard
  - Status-Panel (Daemon, Gerät)
  - Konfigurationseditor
  - Schnellaktionen

## Integration mit Python-Backend

Die GUIs kommunizieren mit dem Python-Backend über:

1. **Prozessausführung**: Starten/Stoppen von `ulanzi-daemon`
2. **Dateioperationen**: Lesen/Schreiben von `config.yaml`
3. **CLI-Tools**: Aufrufen von `ulanzi-manager` Befehlen

## Deployment

### Standalone-Deployment

Erstellen Sie selbstständige Anwendungen (inkl. .NET Runtime):

```bash
# Installer
cd UlanziInstaller
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish

# Manager
cd UlanziManagerUI
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish
```

### Framework-Dependent Deployment

Kleinere Deployments (erfordert .NET Runtime auf Zielsystem):

```bash
dotnet publish -c Release --self-contained false -o ./publish
```

## Fehlerbehebung

### GUI startet nicht

**Problem**: Port bereits in Verwendung
```bash
# Finde Prozess auf Port 5000/5001
sudo lsof -i :5000
# Beende Prozess oder ändere Port in launchSettings.json
```

**Problem**: .NET nicht gefunden
```bash
# Prüfe Installation
dotnet --version
# Füge ~/.dotnet zu PATH hinzu
export PATH="$PATH:$HOME/.dotnet"
```

### Backend-Kommunikation fehlgeschlagen

**Problem**: Python venv nicht gefunden
```bash
# Prüfe Installation
ls -la ~/.local/ulanzi/venv
# Neuinstallation falls nötig
./install.sh
```

**Problem**: Konfiguration nicht gefunden
```bash
# Prüfe Konfigurationsdatei
ls -la ~/.config/ulanzi/config.yaml
# Generiere neu falls nötig
~/.local/ulanzi/venv/bin/ulanzi-manager generate-config ~/.config/ulanzi/config.yaml
```

## Roadmap

Geplante Features für zukünftige Versionen:

- [ ] Button-Editor mit Drag & Drop
- [ ] Icon-Generator Integration
- [ ] OBS-Integration Management
- [ ] Live-Vorschau der Buttons
- [ ] Profilverwaltung
- [ ] Plugin-System
- [ ] Automatische Updates
- [ ] Mehrsprachige Unterstützung

## Beitragen

Contributions sind willkommen! Bitte:

1. Forken Sie das Repository
2. Erstellen Sie einen Feature-Branch
3. Committen Sie Ihre Änderungen
4. Pushen Sie zum Branch
5. Erstellen Sie einen Pull Request

## Lizenz

Siehe Haupt-README für Lizenzinformationen.

## Support

Bei Problemen oder Fragen:

- Erstellen Sie ein Issue auf GitHub
- Siehe Debug-Dokumentation: [DEBUG.md](DEBUG.md)
- Konsultieren Sie die Hauptdokumentation: [README.md](README.md)
