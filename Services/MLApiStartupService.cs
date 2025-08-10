using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Service that ensures the ML API is running before the application starts
    /// </summary>
    public class MLApiStartupService : IHostedService
    {
        private readonly ILogger<MLApiStartupService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly string _mlApiDirectory;
        private readonly string _projectRoot;
        private Process? _mlApiProcess;

        public MLApiStartupService(
            ILogger<MLApiStartupService> logger, 
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _apiBaseUrl = configuration["PredictionApi:BaseUrl"] ?? "http://localhost:5001";
            
            // Get the project root directory
            _projectRoot = Directory.GetCurrentDirectory();
            _mlApiDirectory = Path.Combine(_projectRoot, "ml_api");
            
            // Configure timeout for health checks
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🤖 ML API Startup Service: Initializing...");
            
            // Check if ML API is already running
            if (await IsMLApiRunningAsync())
            {
                _logger.LogInformation("✅ ML API is already running and healthy");
                return;
            }

            _logger.LogWarning("🔄 ML API not detected. Starting ML API...");
            
            // Start the ML API
            bool started = await StartMLApiAsync();
            
            if (started)
            {
                _logger.LogInformation("✅ ML API started successfully and is healthy");
            }
            else
            {
                _logger.LogError("❌ Failed to start ML API. Application will continue with fallback mode.");
                // Don't throw exception - let the app start with fallback mode
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 ML API Startup Service: Shutting down...");
            
            // Only stop the process if we started it
            if (_mlApiProcess != null && !_mlApiProcess.HasExited)
            {
                try
                {
                    _logger.LogInformation("🔄 Stopping ML API process...");
                    _mlApiProcess.Kill();
                    await _mlApiProcess.WaitForExitAsync(cancellationToken);
                    _logger.LogInformation("✅ ML API process stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error stopping ML API process");
                }
                finally
                {
                    _mlApiProcess?.Dispose();
                    _mlApiProcess = null;
                }
            }
        }

        private async Task<bool> IsMLApiRunningAsync()
        {
            try
            {
                _logger.LogDebug("🔍 Checking ML API health at: {ApiUrl}", $"{_apiBaseUrl}/api/health");
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/health");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("📊 API health response: {Response}", content);
                    return true;
                }
                
                _logger.LogDebug("⚠️ API health check failed. Status: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "🔍 ML API health check exception (API likely not running)");
                return false;
            }
        }

        private async Task<bool> StartMLApiAsync()
        {
            try
            {
                // Check if ml_api directory exists
                if (!Directory.Exists(_mlApiDirectory))
                {
                    _logger.LogError("❌ ML API directory not found at: {Directory}", _mlApiDirectory);
                    return false;
                }

                // Check if app.py exists
                string appPyPath = Path.Combine(_mlApiDirectory, "app.py");
                if (!File.Exists(appPyPath))
                {
                    _logger.LogError("❌ app.py not found at: {Path}", appPyPath);
                    return false;
                }

                _logger.LogInformation("🚀 Starting ML API from: {Directory}", _mlApiDirectory);

                // Prepare the Python command
                string pythonCommand = "python";
                string arguments = "app.py";

                // Check if we're on Windows and adjust command if needed
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Try to find Python executable
                    if (await IsPythonAvailableAsync("python"))
                    {
                        pythonCommand = "python";
                    }
                    else if (await IsPythonAvailableAsync("python3"))
                    {
                        pythonCommand = "python3";
                    }
                    else if (await IsPythonAvailableAsync("py"))
                    {
                        pythonCommand = "py";
                    }
                    else
                    {
                        _logger.LogError("❌ Python not found. Please install Python and ensure it's in PATH.");
                        return false;
                    }
                }

                // Start the ML API process
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pythonCommand,
                    Arguments = arguments,
                    WorkingDirectory = _mlApiDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _logger.LogInformation("🔄 Executing: {Command} {Arguments} in {Directory}", 
                    pythonCommand, arguments, _mlApiDirectory);

                _mlApiProcess = Process.Start(processStartInfo);

                if (_mlApiProcess == null)
                {
                    _logger.LogError("❌ Failed to start ML API process");
                    return false;
                }

                _logger.LogInformation("🔄 ML API process started with PID: {ProcessId}", _mlApiProcess.Id);

                // Wait for the API to become available (up to 30 seconds)
                int maxAttempts = 30;
                int attempt = 0;

                while (attempt < maxAttempts)
                {
                    await Task.Delay(1000); // Wait 1 second between attempts
                    attempt++;

                    if (await IsMLApiRunningAsync())
                    {
                        _logger.LogInformation("✅ ML API is healthy after {Seconds} seconds", attempt);
                        return true;
                    }

                    // Check if process is still running
                    if (_mlApiProcess.HasExited)
                    {
                        string output = await _mlApiProcess.StandardOutput.ReadToEndAsync();
                        string error = await _mlApiProcess.StandardError.ReadToEndAsync();
                        
                        _logger.LogError("❌ ML API process exited unexpectedly. Exit code: {ExitCode}", _mlApiProcess.ExitCode);
                        _logger.LogError("📜 Output: {Output}", output);
                        _logger.LogError("📜 Error: {Error}", error);
                        return false;
                    }

                    if (attempt % 5 == 0)
                    {
                        _logger.LogInformation("🔄 Still waiting for ML API... ({Attempt}/{MaxAttempts})", attempt, maxAttempts);
                    }
                }

                _logger.LogError("❌ ML API failed to become healthy within {Seconds} seconds", maxAttempts);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception starting ML API");
                return false;
            }
        }

        private async Task<bool> IsPythonAvailableAsync(string pythonCommand)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pythonCommand,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    return process.ExitCode == 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
