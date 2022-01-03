namespace Yadex.Retirement.Models;

public static class DecimalExtensions
{
    public static string ToKilo(this decimal value)
    {
        return $"{(int)value / 1000}k";
    }
}
