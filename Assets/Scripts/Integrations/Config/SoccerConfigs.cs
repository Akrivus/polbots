using System;

public class SoccerConfigs : IConfig
{
    public string Type => "soccer";
    public int WaitTimeBetweenMatches { get; set; } = 60;
}