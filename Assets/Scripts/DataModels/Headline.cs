using System;

public class Headline
{
    public string Title { get; set; }
    public string Topic { get; set; }
    public string[] Names { get; set; }
    public float Duration { get; set; }

    public DateTime Time { get; set; }

    public bool ShouldSerializeTime() => Time != null;

    public Headline At(DateTime time)
    {
        Time = time;
        return this;
    }
}