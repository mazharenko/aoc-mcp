using mazharenko.aoc_mcp.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Services
	.AddHttpClient()
	.AddSingleton<IAoCClient>(serviceProvider =>
		new AoCClient(args[0], serviceProvider.GetRequiredService<IHttpClientFactory>())
	);

builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithToolsFromAssembly();

await builder.Build().RunAsync();