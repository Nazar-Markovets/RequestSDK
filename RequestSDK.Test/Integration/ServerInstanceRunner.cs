using System.Diagnostics;

namespace RequestSDK.Test.Integration
{
    public class ServerInstanceRunner : IDisposable
    {
        private Process? _serverProcess;

        internal bool IsAlive { get; private set; }
        internal string BaseUrl { get; private set; } = $"https://localhost:{defaultPort}";
        private const int defaultPort = 8080;
        internal void Run(int port = defaultPort)
        {
            if(IsAlive) return;

            string moduleName = typeof(API.Program).Assembly.ManifestModule.Name.Replace(".dll", string.Empty)!;

            DirectoryInfo directory = Directory.GetParent(typeof(API.Program).Assembly.Location)!;
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent!;
            }
            string arg = Directory.GetFiles(Path.Combine(directory!.FullName, moduleName), "*.csproj")?.FirstOrDefault()
                          ?? throw new FileNotFoundException("Can't fount API accembly file");

            BaseUrl = $"https://localhost:{port}";
            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"run --project {arg} --urls {BaseUrl}",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            _serverProcess = Process.Start(startInfo);
            ArgumentNullException.ThrowIfNull(_serverProcess);

            IsAlive = true;
        }

        public void Dispose()
        {
            _serverProcess?.Kill();
            _serverProcess?.Dispose();
            IsAlive = false;
        }
    }
}
