# Ulanzi D200 Manager - GUI Projekt Übersicht

## Projektstruktur

```
ulanzi-d200-linux/
├── UlanziInstaller/              # Installer-GUI (Blazor)
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Home.razor       # Installations-UI
│   │   └── App.razor            # Root-Komponente
│   ├── Services/
│   │   └── InstallationService.cs  # Installationslogik
│   └── Program.cs
│
├── UlanziManagerUI/              # Manager-GUI (Blazor)
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Home.razor       # Haupt-Dashboard
│   │   └── App.razor
│   ├── Services/
│   │   └── UlanziManagerService.cs  # Backend-Integration
│   └── Program.cs
│
├── ulanzi_manager/               # Python Backend
│   ├── cli.py
│   ├── daemon.py
│   ├── device.py
│   └── ...
│
├── docs/
│   ├── GUI_DOCUMENTATION.md     # Vollständige GUI-Dokumentation
│   └── ...
│
├── install-with-gui.sh          # Erweitertes Installations-Script
├── GUI_QUICKSTART.md            # Schnelleinstieg für GUI
└── README.md                    # Haupt-README (aktualisiert)
```

## Komponenten-Übersicht

### 1. UlanziInstaller (Installations-GUI)

**Zweck**: Benutzerfreundliche Installation des gesamten Systems

**Features**:
- Schritt-für-Schritt Installations-Wizard
- Fortschrittsanzeige mit Echtzeit-Updates
- Automatische Installation von:
  - udev-Regeln
  - Python Virtual Environment
  - Ulanzi Manager Paket
  - Konfigurationsdateien
  - Manager-GUI (optional)
- Detailliertes Installationsprotokoll
- Fehlerbehandlung und Rückmeldung

**Technologie**:
- Blazor Server (ASP.NET Core 8.0)
- Bootstrap 5 UI
- Bootstrap Icons
- C# Services für Systeminteraktion

**Aufruf**:
```bash
ulanzi-installer
# oder
cd ~/.local/ulanzi/installer && dotnet UlanziInstaller.dll
```

**Port**: 5000 (konfigurierbar)

### 2. UlanziManagerUI (Manager-GUI)

**Zweck**: Verwaltung und Steuerung des Ulanzi D200 Geräts

**Features**:
- **Dashboard**:
  - Daemon-Status (Läuft/Gestoppt)
  - Gerätestatus (Verbunden/Getrennt)
  - Geräte-Informationen (Modell, Tasten)

- **Daemon-Steuerung**:
  - Start/Stop mit einem Klick
  - Status-Updates in Echtzeit
  - Automatische Status-Überwachung

- **Konfigurations-Editor**:
  - Integrierter YAML-Editor
  - Syntax-Highlighting (monospace Font)
  - Speichern/Neu laden
  - Live-Bearbeitung

- **Schnellaktionen**:
  - Konfiguration validieren
  - Konfiguration anwenden
  - Konfigurationsordner öffnen

**Technologie**:
- Blazor Server (ASP.NET Core 8.0)
- Bootstrap 5 UI
- Bootstrap Icons
- Process Management für Python Backend
- File I/O für Konfiguration

**Aufruf**:
```bash
ulanzi-manager-ui
# oder
cd ~/.local/ulanzi/manager-ui && dotnet UlanziManagerUI.dll
```

**Port**: 5001 (konfigurierbar)

### 3. Python Backend

**Zweck**: Kernfunktionalität für Gerätesteuerung

**Komponenten**:
- `ulanzi-daemon`: Hintergrund-Daemon
- `ulanzi-manager`: CLI-Tool
- `device.py`: Gerätekommunikation
- `config.py`: Konfigurationsverwaltung

**Integration mit GUI**:
- GUI startet/stoppt Daemon via Process API
- GUI liest/schreibt config.yaml direkt
- GUI ruft CLI-Befehle zur Validierung auf

### 4. Installations-Script

**install-with-gui.sh**:
- Erweiterte Version von install.sh
- Installiert zusätzlich die GUIs
- Prüft .NET SDK Verfügbarkeit
- Baut und deployed beide Blazor-Apps
- Erstellt Starter-Scripts
- Erstellt Desktop-Eintrag
- Flag `--no-gui` für Installation ohne GUI

## Dateifluss

### Konfiguration
```
config.yaml
    ↓ (read/write)
UlanziManagerUI
    ↓ (validate via CLI)
ulanzi-manager validate
    ↓ (apply via CLI)
ulanzi-manager configure
    ↓
Ulanzi D200 Device
```

### Daemon-Steuerung
```
UlanziManagerUI
    ↓ (Process.Start)
ulanzi-daemon
    ↓ (USB communication)
Ulanzi D200 Device
```

## Verzeichnis-Layout nach Installation

```
~/.local/ulanzi/
├── venv/                    # Python Virtual Environment
│   ├── bin/
│   │   ├── ulanzi-daemon
│   │   └── ulanzi-manager
│   └── lib/
├── installer/               # Installer-GUI Deployment
│   ├── UlanziInstaller.dll
│   └── ...
└── manager-ui/              # Manager-GUI Deployment
    ├── UlanziManagerUI.dll
    └── ...

~/.local/bin/
├── ulanzi-daemon           # Wrapper für Python-Daemon
├── ulanzi-installer        # Starter für Installer-GUI
└── ulanzi-manager-ui       # Starter für Manager-GUI

~/.config/ulanzi/
└── config.yaml             # Benutzer-Konfiguration

~/.local/share/applications/
└── ulanzi-manager.desktop  # Desktop-Eintrag
```

## Technologie-Stack

### Frontend (GUIs)
- **Framework**: Blazor Server
- **Runtime**: .NET 8.0
- **UI Library**: Bootstrap 5.3
- **Icons**: Bootstrap Icons 1.11.3
- **Sprache**: C# 12

### Backend (Python)
- **Runtime**: Python 3.8+
- **USB**: pyusb, hidapi
- **Config**: PyYAML
- **OBS**: obs-websocket-py
- **Images**: Pillow

### System-Integration
- **udev**: USB-Gerätezugriff
- **systemd**: Daemon-Management
- **xdg**: Desktop-Integration

## Design-Entscheidungen

### Warum Blazor Server?

**Vorteile**:
- ✅ Volle .NET-Integration für Systemaufrufe
- ✅ Process Management ohne Komplexität
- ✅ Direkter Dateisystem-Zugriff
- ✅ Einfache lokale Installation
- ✅ Keine separate API nötig

**Nachteile**:
- ❌ Erfordert .NET Runtime
- ❌ SignalR-Overhead für lokale App
- ❌ Nicht multi-user fähig

**Alternativen erwogen**:
- Blazor WebAssembly: Zu eingeschränkt für Systemzugriff
- Electron: Zu groß, Node.js-Abhängigkeit
- GTK/Qt: Komplexer, weniger moderne UI
- Web + API: Unnötige Komplexität für lokale App

### Warum separate GUIs?

**Installer vs Manager**:
- Installer: Einmalige Verwendung, Installation-fokussiert
- Manager: Tägliche Nutzung, Feature-reich

**Vorteile der Trennung**:
- ✅ Klare Verantwortlichkeiten
- ✅ Kleinere Deployments
- ✅ Unabhängige Updates möglich
- ✅ Manager kann ohne Installer verwendet werden

### Backend-Integration

**Prozess-basiert statt API**:
- ✅ Einfacher: Nutzt existierende CLI
- ✅ Zuverlässig: Keine Netzwerk-Issues
- ✅ Sicher: Keine Ports nach außen
- ❌ Performance: Prozess-Overhead
- ❌ Echtzeit: Kein Live-Feedback von Daemon

## Entwicklungs-Workflow

### Neue Features hinzufügen

**Manager-GUI erweitern**:
1. Service-Methode in `UlanziManagerService.cs` hinzufügen
2. UI-Element in `Home.razor` erstellen
3. Event-Handler für Interaktion implementieren
4. Testen im Development-Modus
5. Dokumentation aktualisieren

**Beispiel** - Neue Schnellaktion:
```csharp
// UlanziManagerService.cs
public async Task<bool> MyNewActionAsync()
{
    // Implementierung
    return true;
}

// Home.razor
<button class="btn btn-primary" @onclick="OnMyAction">
    Neue Aktion
</button>

@code {
    private async Task OnMyAction()
    {
        await ManagerService.MyNewActionAsync();
    }
}
```

### Build und Test

**Development**:
```bash
cd UlanziManagerUI
dotnet run --launch-profile http
# App läuft auf http://localhost:5001
```

**Production Build**:
```bash
dotnet publish -c Release -o ./publish
```

**Integration Test**:
```bash
./install-with-gui.sh
ulanzi-manager-ui
# Teste alle Features
```

## Deployment-Szenarien

### Szenario 1: Lokale Entwicklung
```bash
git clone <repo>
cd ulanzi-d200-linux
dotnet run --project UlanziManagerUI
```

### Szenario 2: Benutzer-Installation
```bash
./install-with-gui.sh
# Alles wird nach ~/.local installiert
```

### Szenario 3: System-weite Installation (nicht implementiert)
```bash
# Würde nach /opt oder /usr/local installieren
# Erfordert sudo
# Nicht empfohlen für Single-User-Gerät
```

## Performance-Überlegungen

### Blazor Server Overhead
- SignalR Connection: ~50KB RAM
- UI Update Latency: <50ms (lokal)
- Startup Time: ~1-2s

### Backend-Interaktion
- Process Start: ~100-200ms
- File Read/Write: <10ms
- Config Validation: ~500ms

### Optimierungen
- Lazy Loading für große Konfigurations-Dateien
- Debouncing für Editor-Updates
- Caching von Device-Status

## Sicherheit

### Lokaler Zugriff
- Nur localhost (127.0.0.1)
- Keine externe Netzwerk-Exposition
- Keine Authentifizierung nötig

### Dateisystem
- Nur Zugriff auf ~/.config/ulanzi und ~/.local/ulanzi
- Keine sudo-Rechte nach Installation
- udev-Regeln einmalig mit sudo

### Prozess-Isolation
- Daemon läuft als User-Prozess
- Keine privilegierten Operationen
- Process-Sandbox durch OS

## Zukünftige Erweiterungen

### Geplant (Roadmap)
1. **Button Designer**: Visueller Editor für Buttons
2. **Icon Generator Integration**: Direkter Zugriff auf Icon-Generator
3. **OBS Management**: UI für OBS-Konfiguration
4. **Profile Manager**: Mehrere Konfigurationsprofile
5. **Plugin System**: Erweiterbarkeit
6. **Auto-Update**: Automatische Updates für GUI

### In Erwägung
- **Mobile App**: React Native für Remote-Control
- **WebSocket API**: Für Plugins und Extensions
- **Theme Support**: Dark/Light Mode
- **Multi-Language**: i18n Support

## Dokumentation

Vollständige Dokumentation:
- [GUI_QUICKSTART.md](../GUI_QUICKSTART.md) - Schnelleinstieg
- [GUI_DOCUMENTATION.md](GUI_DOCUMENTATION.md) - Detaillierte Dokumentation
- [README.md](../README.md) - Projekt-Übersicht

## Support und Beitragen

Issues und Pull Requests auf GitHub willkommen!

---

**Erstellt**: Januar 2026  
**Version**: 1.0.0  
**Technologie**: C# 12, .NET 8.0, Blazor Server, Python 3.8+
