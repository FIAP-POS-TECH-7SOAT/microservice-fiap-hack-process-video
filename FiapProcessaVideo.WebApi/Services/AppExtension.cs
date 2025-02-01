using Serilog;

namespace FiapProcessaVideo.WebApi.Services
{
    public static class AppExtension
    {
        public static void SerilogConfiguration(this IHostBuilder host)
        {
            //  Directory for txt log files
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            // Ensure the directory exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            host.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine(logDirectory, $"log-{DateTime.Now:yyyy-MM-dd}.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
                    //.CreateLogger(); // Keeps only the last 7 log files
            });
        }
    }
}
