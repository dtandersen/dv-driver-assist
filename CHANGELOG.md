0.10.0 - May 11, 2025

- Update to build 99.4 courtesy of Kotenuki.
- Polish translation courtesy of FemboyGaymer.

0.9.4 - 8/3/2023

- French translation. Thanks Kronos!

0.9.3 - 7/29/2023

- Fixed an issue that prevented shift completion when the throttle was applied after shifting.
- Brake cylinder is released after braking. Should see less overshoots.
- Fixed bug in acceleration calculation when locomotive is in reverse.

0.9.0 - 7/29/2023

- Throttle down when the wheels start slipping.
- Hide the cruise control window in photo mode.
- Don't reapply throttle in DM3 unless RPM is below 750 to reduce gear grinding.
- DH4 max temperature is now 118C instead of 119C.
- Improved the settings UI.
- Removed the MinAmps setting since it was no longer used.

0.8.1 - 7/25/2023

- Fixed a bug entering cloned cars when the interior was not loaded.
- Fixed a bug choosing the correct localization.

0.8.0 - 7/24/2023

- Fixed a bug where DM3 gears couldn't be shifted manually.
- DM3 won't downshift at low RPM as long as RPM is rising. This should prevent some unnecessary gear seeking.
- DM3 no longer tries to apply MinBrake. This caused it to exit cruise control.
- DM3 now waits a gear shift to complete before shifting to another gear. This resulted in sluggish acceleration because the previous throttle setting wasn't reapplied.
- Acceleration is now averaged over 1/2 second instead of 1 second.
- Single locomotive trains now use the independent brake.

0.7.1 - 7/22/2023

- Added support for Unity Mod Manager.
- Cruise control no longer tries to shift gears on trains with no shifter.

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
