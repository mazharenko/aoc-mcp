namespace mazharenko.aoc_mcp.Client;

public abstract record SubmissionResult
{
	private SubmissionResult(){}
	public record Correct : SubmissionResult;
	public record Incorrect : SubmissionResult;
	public record TooLow : SubmissionResult;
	public record TooHigh : SubmissionResult;
	public record TooRecently(TimeSpan LeftToWait) : SubmissionResult;
}

public class Stats(ISet<(int, int)> solvedParts)
{
	public Stats() : this(new HashSet<(int, int)>())
	{
	}

	public bool IsSolved(int day, int part)
	{
		return solvedParts.Contains((day, part));
	}

	public Stats Solved(int day, int part)
	{
		solvedParts.Add((day, part));
		return this;
	}

	public void Add(int day, int part, bool solved)
	{
		if (solved)
			Solved(day, part);
		else 
			NotSolved(day, part);
	}

	public Stats NotSolved(int day, int part)
	{
		solvedParts.Remove((day, part));
		return this;
	}

	public IEnumerable<(int day, int part)> GetSolved() => solvedParts;

	public int Stars => solvedParts.Count;

	public bool AllComplete() => Stars == 50;
}

public interface IAoCClient 
{
	Task<Stats> GetDayResults(int year);
	Task<SubmissionResult> SubmitAnswer(int year, int day, int part, string answer);
}
