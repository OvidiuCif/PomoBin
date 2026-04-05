namespace PomoApp.Core;

public class Settings
{
    public double WorkMinutes { get; set; } = 25;
    public double BreakMinutes { get; set; } = 5;
    public double LongBreakMinutes { get; set; } = 15;
    public int SessionsBeforeLongBreak { get; set; } = 4;
}
