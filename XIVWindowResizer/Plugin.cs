using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using XIVWindowResizer.Helpers;
using System.Drawing;
using Dalamud.Game.Text;
using Dalamud.Game.Gui;
using System;

namespace XIVWindowResizer
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "XIVWindowResizer";
        private const string CommandName = "/wresize";
        private CommandManager _commandManager { get; init; }
        private ChatGui _chatGui { get; init; }

        private Size _originalWindowSize;

        private WindowSizeHelper _windowSizeHelper;

        public Plugin(
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ChatGui chatGui)
        {
            _commandManager = commandManager;
            _chatGui = chatGui;

            _windowSizeHelper = new WindowSizeHelper(new WindowSearchHelper());
            _originalWindowSize = _windowSizeHelper.GetWindowSize();

            _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Set window size.\r\nUsage:\r\n/wresize set <width> <height> - Set window size.\r\n/wresize reset - Reset window size back to the original size.\r\n/wresize update - Update stored original window size. Use to update stored resolution when you have changed game's screen resolution without restarting the game."
            });
        }

        public void Dispose()
        {
            _commandManager.RemoveHandler(CommandName);
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
                    PrintInChat($"Window size is set to {_originalWindowSize.Width}x{_originalWindowSize.Height}");
                    break;
                case "update":
                    _originalWindowSize = _windowSizeHelper.GetWindowSize();
                    PrintInChat($"Updated saved window size to {_originalWindowSize.Width}x{_originalWindowSize.Height}");
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
