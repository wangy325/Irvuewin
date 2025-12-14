using Serilog;
using System.IO;

namespace Irvuewin.Helpers.Logging
{
    public static class LogHelper
    {
        public static void Init()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Irvuewin",
                "logs",
                "log-.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.With(new CompactSourceContextEnricher())
                .Enrich.With(new LineNumberEnricher())
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CompactContext}:{LineNumber}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{CompactContext}:{LineNumber}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
                
            Log.Information("Logging initialized.");
        }

        public static void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }
    }
}
