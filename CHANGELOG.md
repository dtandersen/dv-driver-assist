0.7.0 - 7/19/2023

- Added experimental DM3 support.
- Fixed a bug where cruise control became confused during rapid acceleration. This could occur in a DE2 without cars attached.
- Updated German localization.


0.6.0 - 7/15/2023

- Restored support for DH4 after 2023-07-15 game patch.
- Cruise control considers it safe to reverse if speed is less than .01 km/h instead of 0 km/h.
- Overdrive is now active when acceleration is less than .05 m/s^2 instead of 0.
- Cruise control adjusts the throttle less frequently.
- Cruise control may throttle up if temperature is less than 90% of `MaxTemperature`.
- Cruise control reduces throttle gradually if it's above `MaxTemperature` and acceleration is above 0.1 m/s^2.
- Cruise control won't try to accelerate faster than 0.25 m/s^2 to reduce chance of wheel slip.
- Modified the acceleration calculation to hopefully be more accurate.
- Various internal code refactoring.
- Updated German localization.

0.5.0 - 7/9/2023

- Added German localization
- The setting `MinBrake` can be used to prevent the brakes from completely disengaging during deceleration
- The setting `BrakeReleaseFactor` controls how quickly the brakes will be released

0.4.0 - 7/8/2023

- Overdrive works again!

0.3.0 - 7/8/2023

- Added support for DE6 and DH4.
- Each locomotive's cruise control settings can be tuned.
- Cleaned up the stats a bit. They can be disabled.
- Fixed a bug with braking prediction. Braking overshoots less now and is smoother.
- Braking prediction time is now configurable with the `DecelerationTime` setting.
- Locomotives now brake when torque decreases instead of amps (DH4 has no amps)
- Removed `OverdriveEnabled` setting. If you don't want to use overdrive set `OverdriveTemperature` to the same as `MaxTemperature`.

0.2.0 - 7/7/2023

- Fixed some crashes with non-DE2 locos

0.1.0 - 7/7/2023

- Initial Release
