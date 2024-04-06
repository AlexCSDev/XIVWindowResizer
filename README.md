# XIVWindowResizer

Game window resizing plugin mostly intended for users who want to take screenshots at resolutions higher than their display resolution without going fullscreen and using DSR and similar features.

The window resizing code is a fully managed implementation of the code from the [XIVR](https://github.com/ProjectMimer/xivr) plugin.

## Usage
**Make sure your game is in borderless or windowed mode, this won't work in full screen**

The plugin comes with the following commands:

`/wresize set <width> <height>` - Set window size.

`/wresize reset` - Reset window size back to the original size.

`/wresize update` - Update window size used by `/wresize reset` command. Use if you have changed game's screen resolution without restarting the game.

There are 3 recommended usage patterns:
1) Move your chat window to top left of your screen so you can use it no matter which resolution you choose.
2) Create macro for `/wresize set <width> <height>` and `/wresize reset` and bind them to some key combinations or use some plugin to do similar thing.
3) Switch to windowed mode, run `/wresize set <width> <height>` and maximize window to keep it completely on your screen while keeping increased internal resolution. Image on the screen can become distorted, but it will look fine on screenshots.

## Installation

XIVWindowResizer is written as a Dalamud plugin and as such, requires that you use [FFXIVQuickLauncher](https://github.com/goatcorp/FFXIVQuickLauncher) to start your game.
<br/>
Once you have installed FFXIVQuickLauncher and Dalamud, you can install the plugin via the Plugin Installer.