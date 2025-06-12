using mazharenko.aoc_mcp.Client;
using mazharenko.aoc_mcp.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var session = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable("SESSION_COOKIE");
if (session == null) throw new ArgumentNullException(nameof(session));

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Services
	.AddHttpClient()
	.AddSingleton<IAoCClient>(serviceProvider =>
		new AoCClient(session, serviceProvider.GetRequiredService<IHttpClientFactory>(), serviceProvider.GetRequiredService<ILogger<AoCClient>>())
	);


builder.Services.AddSingleton<IConfigureOptions<LoggerFilterOptions>, SetLoggingLevelHandler>();

builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithToolsFromAssembly()
	.WithSetLoggingLevelHandler((ctx, ct) 
		=> ctx.Services!.GetRequiredService<SetLoggingLevelHandler>().UpdateLogLevel(ctx));


var host = builder.Build();
await host.RunAsync();