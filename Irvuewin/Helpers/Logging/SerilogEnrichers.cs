using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Irvuewin.Helpers.Logging
{
    public class CompactSourceContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!logEvent.Properties.TryGetValue("SourceContext", out var propertyValue) ||
                propertyValue is not ScalarValue { Value: string sourceContext }) return;

            string shortContext;
            var parts = sourceContext.Split('.');
            if (parts.Length > 1)
            {
                shortContext = string.Join(".", parts.Take(parts.Length - 1).Select(p => p[0])) + "." + parts.Last();
            }
            else
            {
                shortContext = sourceContext;
            }
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CompactContext", shortContext));
        }
    }

    public class LineNumberEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            // Skip frames from Serilog and mscorlib, finding the first frame from our application
            // This is a heuristic and might need tuning based on the actual stack properties
            foreach (var frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();
                var declaringType = method?.DeclaringType;
                
                if (declaringType == null) continue;
                if (declaringType == typeof(LineNumberEnricher)) continue;

                var assemblyName = declaringType.Assembly.GetName().Name;
                if (assemblyName != null && 
                    (assemblyName.StartsWith("Serilog") || 
                     assemblyName.StartsWith("mscorlib") || 
                     assemblyName.StartsWith("System"))) continue;

                // We found the user code frame
                var lineNumber = frame.GetFileLineNumber();
                if (lineNumber > 0)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LineNumber", lineNumber));
                }
                break;
            }
        }
    }
}
