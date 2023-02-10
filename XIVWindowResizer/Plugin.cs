using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using XIVWindowResizer.Helpers;
using System.Drawing;
using Dalamud.Game.Text;
using Dalamud.Game.Gui;
using System;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game;

namespace XIVWindowResizer
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "XIVWindowResizer";
        private const string CommandName = "/wresize";
        private CommandManager _commandManager { get; init; }
        private ChatGui _chatGui { get; init; }
        private KeyState _keyState { get; init; }
        private Framework _framework { get; init; }

        private Size _originalWindowSize { get; set; }

        private WindowSizeHelper _windowSizeHelper { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] KeyState keyState,
            [RequiredVersion("1.0")] Framework framework)
        {
            _commandManager = commandManager;
            _chatGui = chatGui;
            _keyState = keyState;
            _framework = framework;

            _windowSizeHelper = new WindowSizeHelper(new WindowSearchHelper());
            _originalWindowSize = _windowSizeHelper.GetWindowSize();

            _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Set window size.\r\nUsage:\r\n/wresize set <width> <height> - Set window size.\r\n/wresize reset - Reset window size back to the original size.\r\n/wresize update - Update window size used by /wresize reset command. Use if you have changed game's screen resolution without restarting the game or reloading the plugin."
            });

            _framework.Update += FrameworkUpdate;
        }

        private void FrameworkUpdate(Framework framework)
        {
            if (_keyState[VirtualKey.F9] && _keyState[VirtualKey.SHIFT])
            {
                //TODO: Load this and the desired hotkey from a configuration that can be set with a slash command or something?
                _windowSizeHelper.SetWindowSize(5160, 2160); 
            }
            else if (_keyState[VirtualKey.F10] && _keyState[VirtualKey.SHIFT])
            {
                _windowSizeHelper.SetWindowSize(_originalWindowSize.Width, _originalWindowSize.Height);
            }
        }

        public void Dispose()
        {
            _commandManager.RemoveHandler(CommandName);
            _framework.Update -= FrameworkUpdate;
        }

        private void OnCommand(string command, string args)
        {
            string[] splitArgs = args.ToLowerInvariant().Split(' ');
            switch(splitArgs[0])
            {
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

            _chatGui.PrintChat(xivChat);
        }
    }
}
