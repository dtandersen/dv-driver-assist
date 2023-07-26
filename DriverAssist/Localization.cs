using System.Globalization;
using UnityEngine;

namespace DriverAssist.Localization
{
    // #pragma warning disable IDE1006
    public interface Translation
    // #pragma warning restore IDE1006
    {
        string CC_SETPOINT { get; }
        string CC_STATUS { get; }
        string CC_COASTING { get; }
        string CC_DECELERATING { get; }
        string CC_ACCELERATING { get; }
        string CC_CHANGING_DIRECTION { get; }
        string CC_STOPPING { get; }
        string CC_WARNING_NEUTRAL { get; }
        string CC_DISABLED { get; }
        string CC_UNSUPPORTED { get; }

        string STAT_LOCOMOTIVE { get; }
        string STAT_MASS { get; }
        string STAT_SPEED { get; }
        string STAT_ACCELERATION { get; }
        string STAT_TORQUE { get; }
        string STAT_POWER { get; }
        string STAT_THROTTLE { get; }
        string STAT_TEMPERATURE { get; }
        string STAT_TEMPERATURE_CHANGE { get; }
        string STAT_AMPS { get; }
        string STAT_RPM { get; }
        string STAT_HORSEPOWER { get; }
        string STAT_CURRENT { get; }
        string STAT_CHANGE { get; }

        string TRAIN { get; }
        string LOCO_ABBV { get; }
        string LOCOMOTIVE { get; }
        string CARGO { get; }
    }

    public class TranslationManager
    {
        static Translation instance;

        static TranslationManager()
        {
            instance = Init(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        }

        public static Translation Current
        {
            get
            {
                // PluginLoggerSingleton.Instance.Info($"Detected language {CultureInfo.CurrentCulture.DisplayName}");
                return instance;
            }
        }

        public static void SetLangage(string language)
        {
            instance = Init(language);
        }

        static Translation Init(string language)
        {
            Translation translation = language switch
            {
                "en" => new TranslationEN(),
                "English" => new TranslationEN(),
                "de" => new TranslationDE(),
                "German" => new TranslationDE(),
                _ => new TranslationEN(),
            };

            return translation;
        }
    }

    public class TranslationEN : Translation
    {
        public string CC_SETPOINT => "Setpoint";
        public string CC_STATUS => "Status";
        public string CC_COASTING => "Coasting";
        public string CC_DECELERATING => "Decelerating to {0} km/h";
        public string CC_ACCELERATING => "Accelerating to {0} km/h";
        public string CC_CHANGING_DIRECTION => "Changing direction";
        public string CC_STOPPING => "Stopping";
        public string CC_WARNING_NEUTRAL => "Idle: Engage reverser to start";
        public string CC_DISABLED => "Disabled";
        public string CC_UNSUPPORTED => "Idle: {0} is not supported";

        public string STAT_LOCOMOTIVE => "Locomotive";
        public string STAT_MASS => "Mass";
        public string STAT_SPEED => "Speed (km/h)";
        public string STAT_ACCELERATION => "Acceleration (m/s^2)";
        public string STAT_TORQUE => "Torque";
        public string STAT_POWER => "Power (kW)";
        public string STAT_HORSEPOWER => "Horsepower";
        public string STAT_THROTTLE => "Throttle";
        public string STAT_TEMPERATURE => "Temperature";
        public string STAT_TEMPERATURE_CHANGE => "Temperature Change";
        public string STAT_AMPS => "Amps";
        public string STAT_RPM => "RPM";

        public string LOCOMOTIVE => "Locomotive";
        public string CARGO => "Cargo";

        public string STAT_CURRENT => "Current";
        public string STAT_CHANGE => "Δ";
        public string TRAIN => "Train";
        public string LOCO_ABBV => "Loco";
    }

    public class TranslationDE : Translation
    {
        public string CC_SETPOINT => "Sollwert";
        public string CC_STATUS => "Status";
        public string CC_COASTING => "Rollen";
        public string CC_DECELERATING => "Bremsen auf {0} km/h";
        public string CC_ACCELERATING => "Beschleunigen auf {0} km/h";
        public string CC_CHANGING_DIRECTION => "Richtung ändern";
        public string CC_STOPPING => "Anhalten";
        public string CC_WARNING_NEUTRAL => "Leerlauf: Richtungshebel steht im Leerlauf";
        public string CC_DISABLED => "Abgeschaltet";
        public string CC_UNSUPPORTED => "Keine Einstellungen für {0} gefunden";

        public string STAT_LOCOMOTIVE => "Lokomotive";
        public string STAT_MASS => "Masse";
        public string STAT_SPEED => "Geschwindigkeit";
        public string STAT_ACCELERATION => "Beschleunigung (m/s^2)";
        public string STAT_TORQUE => "Drehmoment";
        public string STAT_POWER => "Leistung (kW)";
        public string STAT_HORSEPOWER => "Pferdestärken";
        public string STAT_THROTTLE => "Schubhebel";
        public string STAT_TEMPERATURE => "Temperatur";
        public string STAT_TEMPERATURE_CHANGE => "Temperaturänderung";
        public string STAT_AMPS => "Amperemeter";
        public string STAT_RPM => "U/min";

        public string LOCOMOTIVE => "Lokomotive";
        public string CARGO => "Ladung";

        public string STAT_CURRENT => "Aktueller ";
        public string STAT_CHANGE => "Δ";
        public string TRAIN => "Zug";
        public string LOCO_ABBV => "Loko";
    }
}
