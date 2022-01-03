namespace Yadex.Retirement.Models;

public record RetirementAge(int Age, int Year)
{
    public override string ToString()
    {
        return $"{Age} Years Old in {Year}";
    }
}
