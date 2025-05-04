using System.Text.RegularExpressions;

namespace mazharenko.aoc_mcp.Client;

internal class AoCClient(string sessionToken, IHttpClientFactory httpClientFactory) : IAoCClient
{
	private HttpClient CreateHttpClient()
	{
		var httpClient = httpClientFactory.CreateClient();
		httpClient.BaseAddress = new Uri("https://adventofcode.com");
		httpClient.DefaultRequestHeaders.Add("cookie", "session=" + sessionToken);
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"aoc-mcp/{ThisAssembly.Info.Version} (+via github.com/mazharenko/aoc-mcp by mazharenko.a@gmail.com)");
		return httpClient;
	}

	private static readonly Regex DayRegex = new(
		"""
		aria-label="Day (?<day>\d+)(, (?<stars>((one star)|(two stars))?))?"
		""");
	
	public async Task<Stats> GetDayResults(int year)
	{
		var stats = new Stats();
		await foreach (var (day, part) in _GetDayResults(year)) 
			stats.Solved(day, part);
		return stats;
	}
	
	private async IAsyncEnumerable<(int day, int part)> _GetDayResults(int year)
	{
		using var httpClient = CreateHttpClient();
		var response = await httpClient.GetStringAsync($"/{year}");
		var statParsed = DayRegex.Matches(response);
		
		foreach (Match match in statParsed)
		{
			var day = int.Parse(match.Groups["day"].Value);
			switch (match.Groups["stars"].Value)
			{
				case "":
					break;
				case "one star":
					yield return (day, 1);
					break;
				case "two stars":
					yield return (day, 1);
					yield return (day, 2);
					break;
				default:
					throw new InvalidOperationException($"Could not interpret day results: {match.Value}");
			}
		}
	}
}
