namespace mazharenko.aoc_mcp.Client;

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
	Task<Stats> GetDayResults(int year, CancellationToken cancellationToken);
	Task<string> SubmitAnswer(int year, int day, int part, string answer, CancellationToken cancellationToken);
}
