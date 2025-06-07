# Driver Assist for Derail Valley

The Driver Assist plugin adds cruise control to Derail Valley.

# Features

* Maintain speed with cruise control. It handles acceleration, braking, and also reversing.
* (idea) DM3 shifting
* (idea) Handbrake set/release
* (idea) Automatic sander
* (idea) Wheel slip detection

# Community

Please join the [Just Another Snake Cult Discord](https://discord.gg/KNmFpwyzYf) if you have suggestions about the mod or need help.

# Installation

This mod requires [Unity Mod Manager](https://www.nexusmods.com/site/mods/21?tab=files) (UMM). If you need help installing it, please join the [Altfuture Discord](https://discord.gg/altfuture).

Download UMM and unzip it (`Right-click -> Extract all...`). Open the folder where it was unzipped and run the installer `UnityModManager.exe`.

Choose the game Derail Valley and install it into the Derail Valley folder (`C:\Program Files (x86)\Steam\steamapps\common\Derail Valley`). Select Doorstop Proxy as the installation method. Now click the install button to complete the installation of UMM.

Download the [Driver Assist mod](https://www.nexusmods.com/derailvalley/mods/663?tab=files) from NexusMods. Go back to the UMM installer and open the Mods tab. Drag the downloaded zip file onto `Drop zip files here`.

You can now close the installer and run Derail Valley.

# Configuration

Press `CTRL+F10` to open the UMM config screen. Select Driver Assist to change the settings.

## Cruise control

There are many settings to play with for the cruise control. The first group of settings determines how close to the setpoint the locomotive will travel.

* `Diff` - Try to stay in the speed range of setpoint +/- diff. For example, if the setpoint is 60 km/h and the diff is 2.5 km/h, cruise control will keep the train in the range of 57.5 to 62.5 km/h (default: 2.5 km/h).
* `Offset` - Add this value to the setpoint. In the above example if `Offset` is -2.5 km/h the speed range would be 55 km/h to 60 km/h (default: 0 km/h).

### Locomotive Profiles

Currently the DE2, DE6, DH4, DM3 (experimental) are supported. Each locomotive type has its own settings.

The DM3 currently upshifts at 800 RPM and downshifts at 600 RPM. This will be configurable in a future update.

All locomotives

* `MinTorque` - Throttle up if torque is below this threshold (only if torque is going down)
* `MaxTemperature` - Throttle down if temperture is above this threshold.
* `OverdriveTemperature` - If the train is slowing down use this heat threshold.

The DE2 and DE6 have an electrical traction motor. `MaxAmps` forces the locomotive to throttle down in order to avoid blowing up the traction motor.

* `MaxAmps` - Throttle down if amps are above this threshold.

### Braking

* `DecelerationTime` - Cruise control attempts to decelerate in this amount of time. A low time may lead to braking overshoot due to the delay in pressurizing/depressurzing the brake lines (default: 10 seconds).
* `BrakeReleaseFactor` - Brake = Brake - BrakeReleaseFactor * Brake.
* `MinBrake` - Minimum braking (0=0%, 1=100%)

### Key bindings

The default key bindings are:

* `Faster` - Increase speed (default: `PAGE UP`).
* `Slower` - Decrease speed (default: `PAGE DOWN`).
* `Toggle` - Toggle cruise control on/off (default: `RIGHT CTRL`)
* `Upshift` - Shifts up one gear (default: `HOME`)
* `Downshift` - Shifts down one gear (default: `END`)

The current throttle setting is reapplied after shifting.

# Usage

Put the reverser into neutral to avoid speeding away. Press `PAGE UP` to increase the setpoint to 10 km/h. Press `RIGHT CTRL` to enable cruise control. Now press `PAGE DOWN` and decrease the setpoint to -10 km/h. The train should no come to a halt and reverse. Finally press `PAGE UP` to raise the setpoint to 0. The locomotive will come to a full stop.

# Development

See [DEVELOPMENT.md](DEVELOPMENT.md).

# Translation

If you would like to provide a translation for another language please join the Discord server.

* Deutsch - 𝓩𝓮𝓬𝓴𝓮_𝓓𝓲𝓻𝓴
* Dutch - NicoLodbrok 
* Français - Kronos
* Polish - FemboyGaymer
* Slovak - OndikSBG

# Credits

* Kotenuki - b99.4 conversion
