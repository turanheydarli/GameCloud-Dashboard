namespace GameCloud.Dashboard.Models;

public class ViewConfiguration
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> MetaTags { get; set; } = new();
    public List<string> Styles { get; set; } = new();
    public List<string> Scripts { get; set; } = new();
}
