# Driver Assist for Derail Valley

The Driver Assist plugin adds cruise control to Derail Valley.

## Installation

Put the DLL in the ZIP file into the `BepInEx\plugins` folder. An easy way to do this is to download the ZIP, double-click it, copy the DLL to the clipboard, and then pasting it into the plugins folder. The standard directory is `c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins`.

## Configuration

Use the [Configuration Manager plugin](https://github.com/BepInEx/BepInEx.ConfigurationManager) to modify the configuration.

* `PAGE UP` - Increase speed
* `PAGE DOWN` - Decrease speed
* `CTRL+C` - Toggle cruise control

# Development

Install the BepInEx [ScriptEngine plugin](https://github.com/BepInEx/BepInEx.Debug) and run `build.bat`. This builds the solution with `dotnet build` and copies the DLL to `BepInEx\scripts`.

Press `F6` in game to load the plugin.

Use `dotnet test` to run the unit tests.
