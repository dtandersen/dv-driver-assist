# Driver Assist for Derail Valley

The Driver Assist plugin adds cruise control to Derail Valley.

# Community

Please join the [Just Another Snake Cult Discord](https://discord.gg/KNmFpwyzYf) if you have suggestions about the mod or need help.

# Installation

This mod requires [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). Here's some helpful links on how to install it.

* [Installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
* [YouTube Tutorial](https://youtu.be/PXwa4WMUie4)
* [Altfuture Discord](https://discord.gg/altfuture)

Once you get BepInEx install, unzip the [release](https://github.com/dtandersen/dv-driver-assist/releases) and copy the `DriverAssist.dll` into the  `BepInEx\plugins` folder.

An easy way to do this is to download the ZIP, double-click it, copy the DLL to the clipboard, and then paste it into the plugins folder. The standard location of the plugins folder is `c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins`.

## Configuration

The `DriverAssist.cfg` file is in the `BepInEx\config` folder. 

You can also use the [Configuration Manager plugin](https://github.com/BepInEx/BepInEx.ConfigurationManager) to change the settings. If you install it, change the keyboard shortcut to it to `F10`, as the default `F1` key interfers with the first person camera.

## Key bindings

* `PAGE UP` - Increase speed
* `PAGE DOWN` - Decrease speed
* `RIGHT CONTROL` - Toggle cruise control on/off

## Cruise control settings

* `MinTorque` - Upshift torque threshold
* `MinAmps` - Upshift amperage threshold
* `MaxAmps` - Downshift amperage threshold
* `MaxTemperature` - Downshift temperature threshhold
* `OverdriveTemperature` - Downshift temperature threshhold during overdrive
* `Overdrive` - Enable overdrive when train is slowing down

# Development

Install the BepInEx [ScriptEngine plugin](https://github.com/BepInEx/BepInEx.Debug) and run `build.bat`. This builds the solution with `dotnet build` and copies the DLL to `BepInEx\scripts`.

Press `F6` in game to load the plugin.

Use `dotnet test` to run the unit tests.
