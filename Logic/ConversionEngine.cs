using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace WiiConverterDesktop.Logic
{
    public class ConversionEngine
    {
        private readonly string _dolphinToolPath;
        private readonly string _witPath;

        public ConversionEngine(string dolphinToolPath, string witPath)
        {
            _dolphinToolPath = dolphinToolPath;
            _witPath = witPath;
        }

        public async Task<string> ConvertRvzToWbfs(string inputPath, string outputFolder, Action<string, double> onProgress)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string tempIsoPath = Path.Combine(Path.GetTempPath(), $"{fileName}_{Guid.NewGuid()}.iso");
            string finalWbfsPath = Path.Combine(outputFolder, $"{fileName}.wbfs");
            
            string actualRvzPath = inputPath;
            string extractTempDir = null;
            bool isZip = Path.GetExtension(inputPath).ToLower() == ".zip";

            try
            {
                if (isZip)
                {
                    onProgress?.Invoke("Extracting ZIP archive...", 5);
                    extractTempDir = Path.Combine(Path.GetTempPath(), $"WiiConv_{Guid.NewGuid()}");
                    Directory.CreateDirectory(extractTempDir);
                    ZipFile.ExtractToDirectory(inputPath, extractTempDir);
                    
                    var extractedRvz = Directory.GetFiles(extractTempDir, "*.rvz", SearchOption.AllDirectories).FirstOrDefault();
                    if (extractedRvz == null)
                        throw new Exception("No .rvz file found inside the ZIP archive.");

                    actualRvzPath = extractedRvz;
                    fileName = Path.GetFileNameWithoutExtension(actualRvzPath);
                    
                    // Update final WBFS path to match the extracted RVZ name, not the ZIP name
                    finalWbfsPath = Path.Combine(outputFolder, $"{fileName}.wbfs");
                    tempIsoPath = Path.Combine(Path.GetTempPath(), $"{fileName}_{Guid.NewGuid()}.iso");
                }

                // Step 1: RVZ -> ISO
                onProgress?.Invoke("Decompressing RVZ to temporary ISO...", 20);
                await RunProcess(_dolphinToolPath, $"convert -i \"{actualRvzPath}\" -o \"{tempIsoPath}\" -f iso");

                if (!File.Exists(tempIsoPath))
                    throw new Exception("DolphinTool failed to create the temporary ISO file.");

                // Step 2: ISO -> WBFS
                onProgress?.Invoke("Converting ISO to WBFS...", 60);
                await RunProcess(_witPath, $"copy \"{tempIsoPath}\" \"{finalWbfsPath}\" --wbfs");

                onProgress?.Invoke("Conversion complete!", 100);
                return finalWbfsPath;
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempIsoPath))
                {
                    try { File.Delete(tempIsoPath); } catch { /* Ignore cleanup errors */ }
                }
                
                if (extractTempDir != null && Directory.Exists(extractTempDir))
                {
                    try { Directory.Delete(extractTempDir, true); } catch { /* Ignore cleanup errors */ }
                }
            }
        }

        private Task RunProcess(string exePath, string arguments)
        {
            var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                if (process.ExitCode == 0)
                    tcs.SetResult(true);
                else
                {
                    string error = process.StandardError.ReadToEnd();
                    tcs.SetException(new Exception($"Process failed with exit code {process.ExitCode}: {error}"));
                }
                process.Dispose();
            };

            try
            {
                if (!process.Start())
                    throw new Exception($"Failed to start process: {exePath}");
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }
}
