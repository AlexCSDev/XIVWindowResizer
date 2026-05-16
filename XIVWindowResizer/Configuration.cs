using Dalamud.Configuration;

namespace XIVWindowResizer;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool OpenAutomaticallyInGPose { get; set; } = true;
}
