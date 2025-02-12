using System;
using System.Collections.Generic;

public class SoccerConfigs : IConfig
{
    public string Type => "soccer";
    public Dictionary<string, string[]> Lines { get; set; } = new Dictionary<string, string[]>();
    public int MatchTimeLimit { get; set; } = 10;
    public int MatchTimeLimitExtraTime { get; set; } = 30;
    public int MatchRestLimit { get; set; } = 60;
    public int MatchGoalLimit { get; set; } = 7;
}