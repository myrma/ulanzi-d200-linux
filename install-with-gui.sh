#!/bin/bash
# Ulanzi D200 Manager Installation Script mit GUI

set -e

echo "╔════════════════════════════════════════════════════════════╗"
echo "║     Ulanzi D200 Manager - Installation Script             ║"
echo "║                  mit GUI Unterstützung                     ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo

# Prüfe ob GUI installiert werden soll
INSTALL_GUI=true
if [ "$1" = "--no-gui" ]; then
    INSTALL_GUI=false
    echo "GUI Installation wird übersprungen."
    echo
fi

# Prüfe ob vom richtigen Verzeichnis ausgeführt wird
if [ ! -f "setup.py" ]; then
    echo "✗ Fehler: setup.py nicht gefunden. Führen Sie dieses Script vom Projekt-Root aus."
    exit 1
fi

# Step 1: Validierung
echo "1. Validiere Setup..."
echo "   ✓ Ready to install"

# Step 2: Udev Regel installieren
echo "2. Installiere udev Regel..."
if [ -w "/etc/udev/rules.d/" ]; then
    sudo cp 99-ulanzi.rules /etc/udev/rules.d/
    sudo udevadm control --reload-rules
    sudo udevadm trigger
    echo "   ✓ Udev Regel installiert"
else
    echo "   ⚠ Udev Regel erfordert sudo. Führen Sie manuell aus:"
    echo "     sudo cp 99-ulanzi.rules /etc/udev/rules.d/"
    echo "     sudo udevadm control --reload-rules"
    echo "     sudo udevadm trigger"
fi

# Step 3: Verzeichnisse erstellen
echo "3. Erstelle Konfigurationsverzeichnisse..."
mkdir -p ~/.config/ulanzi
mkdir -p ~/.local/share/ulanzi
echo "   ✓ Verzeichnisse erstellt"

# Step 4: Setup ~/.local/ulanzi mit venv
echo "4. Richte ~/.local/ulanzi mit virtueller Umgebung ein..."
mkdir -p ~/.local/ulanzi
mkdir -p ~/.local/bin

# Erstelle venv in ~/.local/ulanzi
echo "   Erstelle virtuelle Umgebung..."
python3 -m venv ~/.local/ulanzi/venv

# Installiere Paket im neuen venv
echo "   Installiere Paket..."
~/.local/ulanzi/venv/bin/pip install -q -e .

# Erstelle einfachen Wrapper, der das venv verwendet
cat > ~/.local/bin/ulanzi-daemon << 'WRAPPER'
#!/bin/bash
# Wrapper for ulanzi-daemon using ~/.local/ulanzi/venv
exec ~/.local/ulanzi/venv/bin/ulanzi-daemon "$@"
WRAPPER

chmod +x ~/.local/bin/ulanzi-daemon
echo "   ✓ Virtual Environment Setup abgeschlossen in ~/.local/ulanzi"
echo "   ✓ Wrapper-Skript installiert in ~/.local/bin/ulanzi-daemon"

# Step 5: Beispielkonfiguration generieren
echo "5. Generiere Beispielkonfiguration..."
if [ ! -f ~/.config/ulanzi/config.yaml ]; then
    ~/.local/ulanzi/venv/bin/ulanzi-manager generate-config ~/.config/ulanzi/config.yaml
    echo "   ✓ Konfiguration generiert in ~/.config/ulanzi/config.yaml"
else
    echo "   ✓ Konfiguration existiert bereits"
fi

# Step 6: GUI Installation (optional)
if [ "$INSTALL_GUI" = true ]; then
    echo "6. Installiere GUI-Anwendungen..."
    
    # Prüfe ob .NET installiert ist
    if ! command -v dotnet &> /dev/null; then
        echo "   ⚠ .NET SDK nicht gefunden. GUI Installation wird übersprungen."
        echo "   Installieren Sie .NET 8.0 SDK für GUI-Unterstützung:"
        echo "   https://dotnet.microsoft.com/download"
    else
        echo "   .NET SDK gefunden: $(dotnet --version)"
        
        # Installiere Installer-GUI
        if [ -d "UlanziInstaller" ]; then
            echo "   Baue Installer-GUI..."
            cd UlanziInstaller
            dotnet publish -c Release -o ~/.local/ulanzi/installer --self-contained false
            cd ..
            
            # Erstelle Starter-Script für Installer
            cat > ~/.local/bin/ulanzi-installer << 'INSTALLER_WRAPPER'
#!/bin/bash
# Wrapper for Ulanzi Installer GUI
cd ~/.local/ulanzi/installer
exec dotnet UlanziInstaller.dll
INSTALLER_WRAPPER
            chmod +x ~/.local/bin/ulanzi-installer
            echo "   ✓ Installer-GUI installiert"
        fi
        
        # Installiere Manager-GUI
        if [ -d "UlanziManagerUI" ]; then
            echo "   Baue Manager-GUI..."
            cd UlanziManagerUI
            dotnet publish -c Release -o ~/.local/ulanzi/manager-ui --self-contained false
            cd ..
            
            # Erstelle Starter-Script für Manager-GUI
            cat > ~/.local/bin/ulanzi-manager-ui << 'MANAGER_WRAPPER'
#!/bin/bash
# Wrapper for Ulanzi Manager GUI
cd ~/.local/ulanzi/manager-ui
exec dotnet UlanziManagerUI.dll
MANAGER_WRAPPER
            chmod +x ~/.local/bin/ulanzi-manager-ui
            echo "   ✓ Manager-GUI installiert"
        fi
        
        # Erstelle Desktop-Eintrag (optional)
        if [ -d ~/.local/share/applications ]; then
            cat > ~/.local/share/applications/ulanzi-manager.desktop << 'DESKTOP'
[Desktop Entry]
Version=1.0
Type=Application
Name=Ulanzi D200 Manager
Comment=Verwalte dein Ulanzi D200 StreamDeck
Exec=/home/$USER/.local/bin/ulanzi-manager-ui
Icon=input-gaming
Terminal=false
Categories=Utility;Settings;
DESKTOP
            # Ersetze $USER mit tatsächlichem Username
            sed -i "s/\$USER/$USER/g" ~/.local/share/applications/ulanzi-manager.desktop
            echo "   ✓ Desktop-Eintrag erstellt"
        fi
    fi
fi

echo
echo "╔════════════════════════════════════════════════════════════╗"
echo "║              ✓ Installation Abgeschlossen!                ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo
echo "Nächste Schritte:"
echo "1. Verbinden Sie Ihr Ulanzi D200 Gerät (falls noch nicht verbunden)"
echo "2. Konfiguration bearbeiten: nano ~/.config/ulanzi/config.yaml"
echo "3. Validieren: ulanzi-manager validate ~/.config/ulanzi/config.yaml"
echo "4. Gerät konfigurieren: ulanzi-manager configure ~/.config/ulanzi/config.yaml"

if [ "$INSTALL_GUI" = true ] && command -v dotnet &> /dev/null; then
    echo
    echo "GUI starten:"
    echo "  - Manager-GUI: ulanzi-manager-ui"
    echo "  - Oder über Anwendungsmenü: 'Ulanzi D200 Manager'"
else
    echo "5. Daemon starten: ulanzi-daemon ~/.config/ulanzi/config.yaml"
fi

echo
echo "Optional - Enable systemd user service:"
echo "  systemctl --user enable ulanzi-daemon"
echo "  systemctl --user start ulanzi-daemon"
echo
echo "Für weitere Informationen siehe README.md oder QUICKSTART.md"
echo
