using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using XIVWindowResizer.Helpers;
using XIVWindowResizer.UI;
using System.Drawing;
using Dalamud.Game.Text;
using System;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;

namespace XIVWindowResizer;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "XIVWindowResizer";
    private const string CommandName = "/wresize";

    [PluginService]
    private ICommandManager _commandManager { get; init; }

    [PluginService]
    private IChatGui _chatGui { get; init; }

    [PluginService]
    private IClientState _clientState { get; init; }

    [PluginService]
    private IFramework _framework { get; init; }

    [PluginService]
    private IDalamudPluginInterface _pluginInterface { get; init; }

    private bool _lastGposeState;

    private Size _originalWindowSize { get; set; }

    private WindowSizeHelper _windowSizeHelper { get; init; }

    private readonly WindowSystem _windowSystem = new("XIVWindowResizer");

    private GPoseResizerWindow _gposeWindow;
    
    private Configuration _config;

    public Plugin()
    {
        _windowSizeHelper = new WindowSizeHelper(new WindowSearchHelper());
        _originalWindowSize = _windowSizeHelper.GetWindowSize();

        _config = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Window resizer.\r\nUsage:\r\n/wresize - Toggle the resizer window.\r\n/wresize set <width> <height> - Set window size.\r\n/wresize reset - Reset window size back to the original size.\r\n/wresize update - Update window size used by /wresize reset command. Use if you have changed game's screen resolution without restarting the game or reloading the plugin."
        });

        _gposeWindow = new GPoseResizerWindow(_windowSizeHelper, () => _originalWindowSize, _config, () => _pluginInterface.SavePluginConfig(_config));
        _lastGposeState = _clientState.IsGPosing;
        _gposeWindow.IsOpen = _lastGposeState && _config.OpenAutomaticallyInGPose;
        _windowSystem.AddWindow(_gposeWindow);

        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        _pluginInterface.UiBuilder.DisableGposeUiHide = true;

        _framework.Update += OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        bool current = _clientState.IsGPosing;
        if (current && !_lastGposeState && _config.OpenAutomaticallyInGPose)
            _gposeWindow.IsOpen = true;
        _lastGposeState = current;
    }

    private void OnOpenMainUi() => _gposeWindow.IsOpen = true;

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
        _pluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
        _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _windowSystem.RemoveAllWindows();
        _commandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        string[] splitArgs = args.ToLowerInvariant().Split(' ');
        switch(splitArgs[0])
        {
            case "":
                _gposeWindow.IsOpen = !_gposeWindow.IsOpen;
                break;
            case "set":
                int width = 0;
                int height = 0;

                bool isInvalidSize = false;
                try
                {
                    width = int.Parse(splitArgs[1]);
                    height= int.Parse(splitArgs[2]);
                }
                catch(Exception ex)
                {
                    isInvalidSize = true;
                }

                if(width <= 0 || height <= 0)
                    isInvalidSize = true;

                if(isInvalidSize)
                {
                    PrintInChat("Invalid width or height");
                    return;
                }

                _windowSizeHelper.SetWindowSize(width, height);
                PrintInChat($"Window size is set to {width}x{height}");
                break;
            case "reset":
                _windowSizeHelper.SetWindowSize(_originalWindowSize.Width, _originalWindowSize.Height);
                PrintInChat($"Window size is reset to {_originalWindowSize.Width}x{_originalWindowSize.Height}");
                break;
            case "update":
                _originalWindowSize = _windowSizeHelper.GetWindowSize();
                PrintInChat($"Updated saved window size to {_originalWindowSize.Width}x{_originalWindowSize.Height}");
                break;
				default:
                PrintInChat($"Unknown command: {splitArgs[0]}");
                break;					
        }
    }

    private void PrintInChat(string message)
    {
        var xivChat = new XivChatEntry()
        {
            Message = message
        };

        _chatGui.Print(xivChat);
    }
}
