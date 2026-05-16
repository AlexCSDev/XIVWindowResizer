using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using XIVWindowResizer.Helpers;

namespace XIVWindowResizer.UI;

public class GPoseResizerWindow : Window
{
    private static readonly float[] Scales = { 1.00f, 1.25f, 1.50f, 1.75f, 2.00f };
    private static readonly string[] ScaleLabels = { "1.00x", "1.25x", "1.50x", "1.75x", "2.00x" };
    private readonly WindowSizeHelper _windowSizeHelper;
    private readonly Func<Size> _getOriginalSize;
    private readonly Configuration _config;
    private readonly Action _saveConfig;
    private int _selectedScale = 0;

    public GPoseResizerWindow(
        WindowSizeHelper windowSizeHelper,
        Func<Size> getOriginalSize,
        Configuration config,
        Action saveConfig)
        : base("XIVWindowResizer###xivwindowresizer_gpose",
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize)
    {
        _windowSizeHelper = windowSizeHelper;
        _getOriginalSize = getOriginalSize;
        _config = config;
        _saveConfig = saveConfig;
    }

    public override void PreDraw()
    {
        base.PreDraw();
        Position = new Vector2(20f, 20f) * ImGuiHelpers.GlobalScale;
        PositionCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        _selectedScale = MatchCurrentScale();
        ImGui.SetNextItemWidth(80f);
        if (ImGui.Combo("##scale", ref _selectedScale, ScaleLabels, ScaleLabels.Length))
        {
            ApplyScale(Scales[_selectedScale]);
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset"))
        {
            _selectedScale = 0;
            ApplyScale(Scales[0]);
        }

        bool openAuto = _config.OpenAutomaticallyInGPose;
        if (ImGui.Checkbox("Open automatically in GPose", ref openAuto))
        {
            _config.OpenAutomaticallyInGPose = openAuto;
            _saveConfig();
        }
    }

    private void ApplyScale(float scale)
    {
        var orig = _getOriginalSize();
        int w = (int)Math.Round(orig.Width * scale);
        int h = (int)Math.Round(orig.Height * scale);
        _windowSizeHelper.SetWindowSize(w, h);
    }

    private int MatchCurrentScale()
    {
        var orig = _getOriginalSize();
        var current = _windowSizeHelper.GetWindowSize();
        for (int i = 0; i < Scales.Length; i++)
        {
            int w = (int)Math.Round(orig.Width * Scales[i]);
            int h = (int)Math.Round(orig.Height * Scales[i]);
            if (w == current.Width && h == current.Height)
                return i;
        }
        return -1;
    }
}
