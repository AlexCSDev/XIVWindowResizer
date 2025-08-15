using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JetBrains.Annotations;
using System;
using System.Drawing;
using XIVWindowResizer.Helpers;

namespace XIVWindowResizer
{
    public sealed class Plugin : IDalamudPlugin
    {
        [UsedImplicitly]
        public string Name => "XIVWindowResizer";
        private const string CommandName = "/wresize";

        [UsedImplicitly]
        [PluginService]
        private ICommandManager CommandManager { get; init; }

        [UsedImplicitly]
        [PluginService]
        private IChatGui ChatGui { get; init; }

        private Size OriginalWindowSize { get; set; }

        private WindowSizeHelper WindowSizeHelper { get; init; }

        public Plugin()
        {
            WindowSizeHelper = new WindowSizeHelper(new WindowSearchHelper());
            OriginalWindowSize = WindowSizeHelper.GetWindowSize();

            CommandManager!.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Set window size.\r\nUsage:\r\n/wresize set <width> <height> - Set window size.\r\n/wresize reset - Reset window size back to the original size.\r\n/wresize update - Update window size used by /wresize reset command. Use if you have changed game's screen resolution without restarting the game or reloading the plugin."
            });
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            var splitArgs = args.ToLowerInvariant().Split(' ');
            switch(splitArgs[0])
            {
                case "set":
                    var width = 0;
                    var height = 0;

                    var isInvalidSize = false;
                    try
                    {
                        width = int.Parse(splitArgs[1]);
                        height= int.Parse(splitArgs[2]);
                    }
                    catch (Exception)
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

                    WindowSizeHelper.SetWindowSize(width, height);
                    PrintInChat($"Window size is set to {width}x{height}");
                    break;
                case "reset":
                    WindowSizeHelper.SetWindowSize(OriginalWindowSize.Width, OriginalWindowSize.Height);
                    PrintInChat($"Window size is reset to {OriginalWindowSize.Width}x{OriginalWindowSize.Height}");
                    break;
                case "update":
                    OriginalWindowSize = WindowSizeHelper.GetWindowSize();
                    PrintInChat($"Updated saved window size to {OriginalWindowSize.Width}x{OriginalWindowSize.Height}");
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

            ChatGui.Print(xivChat);
        }
    }
}
