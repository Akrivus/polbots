using System;

public class SplashConfigs : IConfig
{
    public string Type => "splash";
    public string[] Splashes { get; set; }
}