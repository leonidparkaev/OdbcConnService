using Microsoft.Extensions.Logging;

namespace WinLogger
{
    class cWinLogger
    {
        public static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddEventLog(eventLogSettings => eventLogSettings.SourceName = "ODBC_Serv"));
        public static ILogger Logger = factory.CreateLogger("ODBC_Serv_log");
    }
}
