using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace mazharenko.aoc_mcp.Logging;

public class SetLoggingLevelHandler : IConfigureOptions<LoggerFilterOptions>
{
	private LogLevel _currentLevel = LogLevel.Debug;

	public ValueTask<EmptyResult> UpdateLogLevel(RequestContext<SetLevelRequestParams> ctx)
	{
		if (ctx.Params?.Level is null)
			throw new McpException("Missing required argument 'level'", McpErrorCode.InvalidParams);

		_currentLevel = ctx.Params.Level.ToLogLevel();
		return ValueTask.FromResult(new EmptyResult());
	}

	public void Configure(LoggerFilterOptions options)
	{
		options.MinLevel = _currentLevel;
	}
}