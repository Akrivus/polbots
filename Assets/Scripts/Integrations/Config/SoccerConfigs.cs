using System;
using System.Collections.Generic;

public class SoccerConfigs : IConfig
{
    public string Type => "soccer";
    public int WaitTimeBetweenMatches { get; set; } = 60;
    public Dictionary<string, string[]> Lines { get; set; } = new Dictionary<string, string[]>();
}