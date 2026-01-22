using System.Diagnostics;
using System.Text;

namespace UlanziInstaller.Services;

public class InstallationService
{
    private readonly ILogger<InstallationService> _logger;
    
    public event Action<string>? OnOutput;
    public event Action<InstallationStep>? OnStepChanged;
    
    public InstallationService(ILogger<InstallationService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> InstallAsync(InstallationOptions options)
    {
        try
        {
            // Step 1: Validierung
            UpdateStep(InstallationStep.Validating);
            LogOutput("1. Validiere Setup...");
            await Task.Delay(500);
            
            if (!File.Exists("setup.py"))
            {
                LogOutput("✗ Fehler: setup.py nicht gefunden.");
                return false;
            }
            LogOutput("   ✓ Ready to install");
            
            // Step 2: Udev Regel installieren
            UpdateStep(InstallationStep.InstallingUdevRule);
            LogOutput("2. Installiere udev Regel...");
            
            if (await InstallUdevRuleAsync())
            {
                LogOutput("   ✓ Udev Regel installiert");
            }
            else
            {
                LogOutput("   ⚠ Udev Regel erfordert sudo. Bitte manuell ausführen:");
                LogOutput("     sudo cp 99-ulanzi.rules /etc/udev/rules.d/");
                LogOutput("     sudo udevadm control --reload-rules");
                LogOutput("     sudo udevadm trigger");
            }
            
            // Step 3: Verzeichnisse erstellen
            UpdateStep(InstallationStep.CreatingDirectories);
            LogOutput("3. Erstelle Konfigurationsverzeichnisse...");
            
            var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".config", "ulanzi");
            var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".local", "share", "ulanzi");
            var localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".local", "ulanzi");
            var binDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".local", "bin");
            
            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(localDir);
            Directory.CreateDirectory(binDir);
            LogOutput("   ✓ Verzeichnisse erstellt");
            
            // Step 4: Virtual Environment erstellen
            UpdateStep(InstallationStep.CreatingVirtualEnv);
            LogOutput("4. Richte ~/.local/ulanzi mit virtueller Umgebung ein...");
            LogOutput("   Erstelle virtuelle Umgebung...");
            
            var venvPath = Path.Combine(localDir, "venv");
            if (!await CreateVirtualEnvironmentAsync(venvPath))
            {
                LogOutput("   ✗ Fehler beim Erstellen der virtuellen Umgebung");
                return false;
            }
            
            LogOutput("   Installiere Paket...");
            if (!await InstallPackageAsync(venvPath))
            {
                LogOutput("   ✗ Fehler beim Installieren des Pakets");
                return false;
            }
            
            // Wrapper Script erstellen
            var wrapperPath = Path.Combine(binDir, "ulanzi-daemon");
            await CreateWrapperScriptAsync(wrapperPath, venvPath);
            LogOutput("   ✓ Virtual Environment Setup abgeschlossen in ~/.local/ulanzi");
            LogOutput("   ✓ Wrapper-Skript installiert in ~/.local/bin/ulanzi-daemon");
            
            // Step 5: Konfiguration generieren
            UpdateStep(InstallationStep.GeneratingConfig);
            LogOutput("5. Generiere Beispielkonfiguration...");
            
            var configPath = Path.Combine(configDir, "config.yaml");
            if (!File.Exists(configPath))
            {
                await GenerateConfigAsync(venvPath, configPath);
                LogOutput($"   ✓ Konfiguration generiert in {configPath}");
            }
            else
            {
                LogOutput("   ✓ Konfiguration existiert bereits");
            }
            
            // Step 6: GUI installieren (optional)
            if (options.InstallGui)
            {
                UpdateStep(InstallationStep.InstallingGui);
                LogOutput("6. Installiere Anwendungs-GUI...");
                
                var guiInstallPath = Path.Combine(localDir, "gui");
                Directory.CreateDirectory(guiInstallPath);
                
                // Kopiere GUI-Dateien
                await InstallGuiAsync(guiInstallPath);
                LogOutput($"   ✓ GUI installiert in {guiInstallPath}");
                
                // GUI-Starter erstellen
                var guiWrapperPath = Path.Combine(binDir, "ulanzi-manager-ui");
                await CreateGuiWrapperAsync(guiWrapperPath, guiInstallPath);
                LogOutput("   ✓ GUI-Starter erstellt in ~/.local/bin/ulanzi-manager-ui");
            }
            
            UpdateStep(InstallationStep.Completed);
            LogOutput("");
            LogOutput("╔════════════════════════════════════════════════════════════╗");
            LogOutput("║              ✓ Installation Abgeschlossen!                ║");
            LogOutput("╚════════════════════════════════════════════════════════════╝");
            LogOutput("");
            LogOutput("Nächste Schritte:");
            LogOutput("1. Ulanzi D200 Gerät neu verbinden (falls noch nicht verbunden)");
            LogOutput($"2. Konfiguration bearbeiten: nano {configPath}");
            LogOutput($"3. Validieren: ulanzi-manager validate {configPath}");
            LogOutput($"4. Gerät konfigurieren: ulanzi-manager configure {configPath}");
            
            if (options.InstallGui)
            {
                LogOutput("5. GUI starten: ulanzi-manager-ui");
            }
            else
            {
                LogOutput($"5. Daemon starten: ulanzi-daemon {configPath}");
            }
            
            LogOutput("");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Installation failed");
            LogOutput($"✗ Fehler: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> InstallUdevRuleAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sudo",
                Arguments = "cp 99-ulanzi.rules /etc/udev/rules.d/",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            if (process == null) return false;
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                // Reload rules
                await RunCommandAsync("sudo", "udevadm control --reload-rules");
                await RunCommandAsync("sudo", "udevadm trigger");
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> CreateVirtualEnvironmentAsync(string venvPath)
    {
        return await RunCommandAsync("python3", $"-m venv {venvPath}");
    }
    
    private async Task<bool> InstallPackageAsync(string venvPath)
    {
        var pipPath = Path.Combine(venvPath, "bin", "pip");
        return await RunCommandAsync(pipPath, "install -q -e .");
    }
    
    private async Task CreateWrapperScriptAsync(string wrapperPath, string venvPath)
    {
        var daemonPath = Path.Combine(venvPath, "bin", "ulanzi-daemon");
        var wrapper = $@"#!/bin/bash
# Wrapper for ulanzi-daemon using ~/.local/ulanzi/venv
exec {daemonPath} ""$@""
";
        await File.WriteAllTextAsync(wrapperPath, wrapper);
        await RunCommandAsync("chmod", $"+x {wrapperPath}");
    }
    
    private async Task GenerateConfigAsync(string venvPath, string configPath)
    {
        var managerPath = Path.Combine(venvPath, "bin", "ulanzi-manager");
        await RunCommandAsync(managerPath, $"generate-config {configPath}");
    }
    
    private async Task InstallGuiAsync(string guiInstallPath)
    {
        // Kopiere GUI-Dateien aus UlanziManagerUI zum Installationsverzeichnis
        var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "UlanziManagerUI");
        
        if (Directory.Exists(sourcePath))
        {
            await CopyDirectoryAsync(sourcePath, guiInstallPath);
        }
    }
    
    private async Task CreateGuiWrapperAsync(string wrapperPath, string guiInstallPath)
    {
        var wrapper = $@"#!/bin/bash
# Wrapper for Ulanzi Manager UI
cd {guiInstallPath}
dotnet UlanziManagerUI.dll
";
        await File.WriteAllTextAsync(wrapperPath, wrapper);
        await RunCommandAsync("chmod", $"+x {wrapperPath}");
    }
    
    private async Task<bool> RunCommandAsync(string command, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            if (process == null) return false;
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            if (!string.IsNullOrWhiteSpace(output))
                LogOutput(output);
            
            if (!string.IsNullOrWhiteSpace(error))
                LogOutput(error);
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run command: {Command} {Arguments}", command, arguments);
            return false;
        }
    }
    
    private async Task CopyDirectoryAsync(string sourcePath, string destPath)
    {
        Directory.CreateDirectory(destPath);
        
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destPath, fileName);
            File.Copy(file, destFile, true);
        }
        
        foreach (var dir in Directory.GetDirectories(sourcePath))
        {
            var dirName = Path.GetFileName(dir);
            // Skip obj, bin, and .vs folders
            if (dirName == "obj" || dirName == "bin" || dirName == ".vs")
                continue;
                
            var destDir = Path.Combine(destPath, dirName);
            await CopyDirectoryAsync(dir, destDir);
        }
    }
    
    private void LogOutput(string message)
    {
        OnOutput?.Invoke(message);
    }
    
    private void UpdateStep(InstallationStep step)
    {
        OnStepChanged?.Invoke(step);
    }
}

public enum InstallationStep
{
    NotStarted,
    Validating,
    InstallingUdevRule,
    CreatingDirectories,
    CreatingVirtualEnv,
    GeneratingConfig,
    InstallingGui,
    Completed,
    Failed
}

public class InstallationOptions
{
    public bool InstallGui { get; set; } = true;
    public string InstallPath { get; set; } = "";
}
