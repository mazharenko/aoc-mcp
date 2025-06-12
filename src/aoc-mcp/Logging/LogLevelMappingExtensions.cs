using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace mazharenko.aoc_mcp.Logging;

public static class LogLevelMappingExtensions
{
	public static LogLevel ToLogLevel(this LoggingLevel loggingLevel)
	{
		return loggingLevel switch
		{
			LoggingLevel.Debug => LogLevel.Debug,
			LoggingLevel.Info => LogLevel.Information,
			LoggingLevel.Notice => LogLevel.Information,
			LoggingLevel.Warning => LogLevel.Warning,
			LoggingLevel.Error => LogLevel.Error,
			LoggingLevel.Critical => LogLevel.Critical,
			LoggingLevel.Alert => LogLevel.Critical,
			LoggingLevel.Emergency => LogLevel.Critical
		};
	}

	public static LoggingLevel? ToLoggingLevel(this LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Trace => LoggingLevel.Debug,
			LogLevel.Debug => LoggingLevel.Debug,
			LogLevel.Information => LoggingLevel.Info,
			LogLevel.Warning => LoggingLevel.Warning,
			LogLevel.Error => LoggingLevel.Error,
			LogLevel.Critical => LoggingLevel.Critical,
			LogLevel.None => null,
		};
	}
}