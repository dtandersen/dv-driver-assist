# Driver Assist for Derail Valley

The Driver Assist plugin adds cruise control to Derail Valley.

# Features

* Maintain speed with cruise control. It handles acceleration, braking, and also reversing.
* (idea) DM3 shifting
* (idea) Handbrake set/release
* (idea) Automatic sander

# Community

Please join the [Just Another Snake Cult Discord](https://discord.gg/KNmFpwyzYf) if you have suggestions about the mod or need help.

# Installation

This mod requires [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). Here's some helpful links on how to install it.

* [Installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
* [YouTube Tutorial](https://youtu.be/PXwa4WMUie4)
* [Altfuture Discord](https://discord.gg/altfuture)

Once BepInEx is installed, download the [release](https://github.com/dtandersen/dv-driver-assist/releases), unzip it, and copy `DriverAssist.dll` into the `BepInEx\plugins` folder.

An easy way to do this is to download the ZIP, double-click it, copy the DLL to the clipboard, and then paste it into the plugins folder. The standard location of the plugins folder is `c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins`.

# Configuration

The `DriverAssist.cfg` file is in the `BepInEx\config` folder. However, it's recommended to use the [Configuration Manager plugin](https://github.com/BepInEx/BepInEx.ConfigurationManager) to change the settings. If you install Configuration Manager, change the keyboard shortcut to it to `F10`, as the default `F1` key interfers with the first person camera.

## Cruise control

There are many settings to play with for the cruise control. The first group of settings determines how close to the setpoint the locomotive will travel.

* `Diff` - Try to stay in the speed range of setpoint +/- diff. For example, if the setpoint is 60 km/h and the diff is 2.5 km/h, cruise control will keep the train in the range of 57.5 to 62.5 km/h (default: 2.5 km/h).
* `Offset` - Add this value to the setpoint. In the above example if `Offset` is -2.5 km/h the speed range would be 55 km/h to 60 km/h (default: 0 km/h).

### Locomotive Profiles

Currently the DE2, DE6, and DH4 are supported. Each locomotive type has its own settings.

* `MinTorque` - Throttle up if torque is below this threshold (only if torque is going down)
* `MinAmps` - Throttle up if amps are below this threshold.
* `MaxAmps` - Throttle down if amps are above this threshold.
* `MaxTemperature` - Throttle down if temperture is above this threshold.
* `OverdriveTemperature` - If the train is slowing down use this heat threshold.

### Braking

* `DecelerationTime` - Cruise control will attempt to decelerate in this amount of time. Lower values may lead to overshoot due to the delay in pressurizing the brake lines.

### Key bindings

The default key bindings are:

* `Faster` - Increase speed (default: `PAGE UP`)
* `Slower` - Decrease speed (default: `PAGE DOWN`)
* `Toggle` - Toggle cruise control on/off (default: `RIGHT CTRL`)

# Usage

Put the reverser into neutral. Press `PAGE DOWN` to increase the set point to 10 km/h. Press `RIGHT CTRL` to the cruise control. Now press `PAGE DOWN` and decrease the setpoing to -10 km/h. The train should no come to a halt and reverse.

# Development

Install the BepInEx [ScriptEngine plugin](https://github.com/BepInEx/BepInEx.Debug) and run `build.bat`. This builds the solution with `dotnet build` and copies the DLL to `BepInEx\scripts`.

Press `F6` in game to load the plugin.

Use `dotnet test` to run the unit tests.
