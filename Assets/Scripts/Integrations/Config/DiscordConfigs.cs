using System;

public class DiscordConfigs : IConfig
{
    public string Type => "discord";
    public string WebhookURL { get; set; }
}