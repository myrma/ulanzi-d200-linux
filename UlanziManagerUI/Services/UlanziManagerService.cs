using System.Diagnostics;
using System.Text.Json;

namespace UlanziManagerUI.Services;

public class UlanziManagerService
{
    private readonly ILogger<UlanziManagerService> _logger;
    private readonly string _configPath;
    private Process? _daemonProcess;
    
    public event Action<string>? OnStatusChanged;
    public event Action<DeviceStatus>? OnDeviceStatusChanged;
    
    public UlanziManagerService(ILogger<UlanziManagerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _configPath = configuration["ConfigPath"] ?? Path.Combine(homeDir, ".config", "ulanzi", "config.yaml");
    }
    
    public Task<bool> StartDaemonAsync()
    {
        try
        {
            if (_daemonProcess != null && !_daemonProcess.HasExited)
            {
                _logger.LogWarning("Daemon is already running");
                return Task.FromResult(false);
            }
            
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var daemonPath = Path.Combine(homeDir, ".local", "bin", "ulanzi-daemon");
            
            if (!File.Exists(daemonPath))
            {
                _logger.LogError("Daemon executable not found at {Path}", daemonPath);
                OnStatusChanged?.Invoke("Daemon nicht gefunden");
                return Task.FromResult(false);
            }
            
            var psi = new ProcessStartInfo
            {
                FileName = daemonPath,
                Arguments = _configPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            _daemonProcess = Process.Start(psi);
            
            if (_daemonProcess != null)
            {
                OnStatusChanged?.Invoke("Daemon gestartet");
                _ = Task.Run(() => MonitorDaemon(_daemonProcess));
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start daemon");
            OnStatusChanged?.Invoke($"Fehler: {ex.Message}");
            return Task.FromResult(false);
        }
    }
    
    public async Task<bool> StopDaemonAsync()
    {
        try
        {
            if (_daemonProcess == null || _daemonProcess.HasExited)
            {
                return false;
            }
            
            _daemonProcess.Kill();
            await _daemonProcess.WaitForExitAsync();
            OnStatusChanged?.Invoke("Daemon gestoppt");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop daemon");
            return false;
        }
    }
    
    public bool IsDaemonRunning()
    {
        return _daemonProcess != null && !_daemonProcess.HasExited;
    }
    
    public async Task<ConfigData?> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                _logger.LogWarning("Config file not found at {Path}", _configPath);
                return null;
            }
            
            var yaml = await File.ReadAllTextAsync(_configPath);
            // Hier würde normalerweise YAML geparst werden
            // Für jetzt geben wir eine einfache Struktur zurück
            return new ConfigData
            {
                ConfigPath = _configPath,
                Content = yaml
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load config");
            return null;
        }
    }
    
    public async Task<bool> SaveConfigAsync(string content)
    {
        try
        {
            await File.WriteAllTextAsync(_configPath, content);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save config");
            return false;
        }
    }
    
    public async Task<bool> ValidateConfigAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var managerPath = Path.Combine(homeDir, ".local", "ulanzi", "venv", "bin", "ulanzi-manager");
            
            _logger.LogInformation("Validating config at: {ConfigPath}", _configPath);
            _logger.LogInformation("Using manager at: {ManagerPath}", managerPath);
            
            if (!File.Exists(managerPath))
            {
                _logger.LogError("ulanzi-manager not found at {Path}", managerPath);
                OnStatusChanged?.Invoke($"✗ ulanzi-manager nicht gefunden bei {managerPath}");
                return false;
            }
            
            if (!File.Exists(_configPath))
            {
                _logger.LogError("Config file not found at {Path}", _configPath);
                OnStatusChanged?.Invoke($"✗ Konfigurationsdatei nicht gefunden bei {_configPath}");
                return false;
            }
            
            var psi = new ProcessStartInfo
            {
                FileName = managerPath,
                Arguments = $"validate {_configPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            if (process == null)
            {
                _logger.LogError("Failed to start validation process");
                OnStatusChanged?.Invoke("✗ Fehler beim Starten der Validierung");
                return false;
            }
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            _logger.LogInformation("Validation exit code: {ExitCode}", process.ExitCode);
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                _logger.LogInformation("Validation output: {Output}", output);
                OnStatusChanged?.Invoke(output);
            }
            
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logger.LogWarning("Validation error output: {Error}", error);
                OnStatusChanged?.Invoke($"Validierungsfehler: {error}");
            }
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate config");
            OnStatusChanged?.Invoke($"✗ Exception: {ex.Message}");
            return false;
        }
    }
    
    public async Task<ValidationResult> ValidateConfigWithDetailsAsync()
    {
        var result = new ValidationResult { IsValid = true };
        
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var managerPath = Path.Combine(homeDir, ".local", "ulanzi", "venv", "bin", "ulanzi-manager");
            
            _logger.LogInformation("Validating config at: {ConfigPath}", _configPath);
            
            if (!File.Exists(managerPath))
            {
                _logger.LogError("ulanzi-manager not found at {Path}", managerPath);
                result.IsValid = false;
                result.Errors.Add(new ValidationError 
                { 
                    Message = $"ulanzi-manager nicht gefunden bei {managerPath}"
                });
                return result;
            }
            
            if (!File.Exists(_configPath))
            {
                _logger.LogError("Config file not found at {Path}", _configPath);
                result.IsValid = false;
                result.Errors.Add(new ValidationError 
                { 
                    Message = $"Konfigurationsdatei nicht gefunden bei {_configPath}"
                });
                return result;
            }
            
            var psi = new ProcessStartInfo
            {
                FileName = managerPath,
                Arguments = $"validate {_configPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            if (process == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Message = "Fehler beim Starten der Validierung" });
                return result;
            }
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            _logger.LogInformation("Validation exit code: {ExitCode}", process.ExitCode);
            
            if (process.ExitCode == 0)
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                
                // Parse error output
                var lines = error.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("ERROR:"))
                    {
                        var errorMessage = line.Substring(line.IndexOf("ERROR:") + 6).Trim();
                        
                        // Parse button errors
                        if (errorMessage.Contains("Button"))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(
                                errorMessage, 
                                @"Button (\d+):\s*(.+)");
                            
                            if (match.Success)
                            {
                                var buttonId = int.Parse(match.Groups[1].Value);
                                var message = match.Groups[2].Value;
                                
                                result.Errors.Add(new ValidationError
                                {
                                    ButtonId = buttonId,
                                    Message = message,
                                    Type = "ERROR"
                                });
                            }
                            else
                            {
                                result.Errors.Add(new ValidationError { Message = errorMessage });
                            }
                        }
                        else if (!errorMessage.Contains("Configuration errors:"))
                        {
                            result.Errors.Add(new ValidationError { Message = errorMessage });
                        }
                    }
                }
                
                // Falls keine Errors geparsed wurden, füge generische Nachricht hinzu
                if (!result.Errors.Any() && !string.IsNullOrWhiteSpace(error))
                {
                    result.Errors.Add(new ValidationError { Message = error });
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate config");
            result.IsValid = false;
            result.Errors.Add(new ValidationError { Message = $"Exception: {ex.Message}" });
            return result;
        }
    }
    
    public async Task<bool> ApplyConfigAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var managerPath = Path.Combine(homeDir, ".local", "ulanzi", "venv", "bin", "ulanzi-manager");
            
            var psi = new ProcessStartInfo
            {
                FileName = managerPath,
                Arguments = $"configure {_configPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            if (process == null) return false;
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                OnStatusChanged?.Invoke("Konfiguration angewendet");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply config");
            return false;
        }
    }
    
    public async Task<DeviceStatus> GetDeviceStatusAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var managerPath = Path.Combine(homeDir, ".local", "ulanzi", "venv", "bin", "ulanzi-manager");
            
            if (!File.Exists(managerPath))
            {
                _logger.LogWarning("ulanzi-manager not found at {Path}", managerPath);
                return new DeviceStatus { Connected = false };
            }
            
            var psi = new ProcessStartInfo
            {
                FileName = managerPath,
                Arguments = "status",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            using var process = Process.Start(psi);
            if (process == null)
            {
                return new DeviceStatus { Connected = false };
            }
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            _logger.LogDebug("Device status check - Exit code: {ExitCode}", process.ExitCode);
            _logger.LogDebug("Output: {Output}", output);
            
            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                // Gerät gefunden
                var status = new DeviceStatus
                {
                    Connected = true,
                    Model = "Ulanzi D200",
                    Buttons = 15 // D200 hat 15 Tasten (14 + Uhr)
                };
                OnDeviceStatusChanged?.Invoke(status);
                return status;
            }
            else
            {
                // Gerät nicht verbunden oder Fehler
                _logger.LogInformation("Device not connected. Exit code: {ExitCode}, Error: {Error}", 
                    process.ExitCode, error);
                var status = new DeviceStatus { Connected = false };
                OnDeviceStatusChanged?.Invoke(status);
                return status;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device status");
            var status = new DeviceStatus { Connected = false };
            OnDeviceStatusChanged?.Invoke(status);
            return status;
        }
    }
    
    private async Task MonitorDaemon(Process process)
    {
        try
        {
            await process.WaitForExitAsync();
            OnStatusChanged?.Invoke("Daemon wurde beendet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring daemon");
        }
    }
}

public class ConfigData
{
    public string ConfigPath { get; set; } = "";
    public string Content { get; set; } = "";
}

public class DeviceStatus
{
    public bool Connected { get; set; }
    public string Model { get; set; } = "";
    public int Buttons { get; set; }
}

public class ValidationError
{
    public int? ButtonId { get; set; }
    public string Message { get; set; } = "";
    public string Type { get; set; } = "ERROR";
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}
