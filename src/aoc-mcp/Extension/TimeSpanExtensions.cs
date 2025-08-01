using System.Globalization;

namespace mazharenko.aoc_mcp.Extension;
internal static class TimeSpanExtensions
{
	public static string ToHumanReadable(this TimeSpan timeSpan)
	{
		if (timeSpan.TotalMinutes > 1)
			return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
		if (timeSpan.TotalSeconds > 1)
			return timeSpan.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture) + "s";
		return timeSpan.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture) + "ms";
	}
}
