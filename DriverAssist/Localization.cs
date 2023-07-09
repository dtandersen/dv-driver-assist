using System.Globalization;
using UnityEngine;

namespace DriverAssist.Localization
{
    public interface Translation
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
        string STAT_AMPS { get; }
        string STAT_RPM { get; }
        string STAT_HORSEPOWER { get; }
    }

    public class TranslationManager
    {
        public static Translation Current { get; internal set; }

        public static void Init()
        {
            string language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Init(language);
        }

        public static void Init(string language)
        {
            switch (language)
            {
                case "en":
                    Current = new TranslationEN();
                    break;
                case "de":
                    Current = new TranslationDE();
                    break;
                default:
                    Current = new TranslationEN();
                    break;
            }
        }
    }

    public class TranslationEN : Translation
    {
        public string CC_SETPOINT => "Setpoint";
        public string CC_STATUS => "Status";
        public string CC_COASTING => "Coast";
        public string CC_DECELERATING => "Decelerating to {0} km/h";
        public string CC_ACCELERATING => "Accelerating to {0} km/h";
        public string CC_CHANGING_DIRECTION => "Direction change";
        public string CC_STOPPING => "Stop";
        public string CC_WARNING_NEUTRAL => "Idle: Reverser is in neutral";
        public string CC_DISABLED => "Disabled";
        public string CC_UNSUPPORTED => "No settings found for {0}";

        public string STAT_LOCOMOTIVE => "Locomotive";
        public string STAT_MASS => "Mass (t)";
        public string STAT_SPEED => "Speed (km/h)";
        public string STAT_ACCELERATION => "Acceleration (m/s^2)";
        public string STAT_TORQUE => "Torque";
        public string STAT_POWER => "Power (kW)";
        public string STAT_HORSEPOWER => "Horsepower";
        public string STAT_THROTTLE => "Throttle";
        public string STAT_TEMPERATURE => "Temperature";
        public string STAT_AMPS => "Amps";
        public string STAT_RPM => "RPM";
    }

    public class TranslationDE : Translation
    {
        public string CC_SETPOINT => "Sollwert";
        public string CC_STATUS => "Status";
        public string CC_COASTING => "Rollt";
        public string CC_DECELERATING => "Verlangsamung auf {0} km/h";
        public string CC_ACCELERATING => "Beschleunigung auf {0} km/h";
        public string CC_CHANGING_DIRECTION => "Richtung ändern";
        public string CC_STOPPING => "Anhalten";
        public string CC_WARNING_NEUTRAL => "Leerlauf: Reversierer steht im Leerlauf";
        public string CC_DISABLED => "Behinderte";
        public string CC_UNSUPPORTED => "Keine Einstellungen für {0} gefunden";

        public string STAT_LOCOMOTIVE => "Lokomotive";
        public string STAT_MASS => "Masse (t)";
        public string STAT_SPEED => "Geschwindigkeit (km/h)";
        public string STAT_ACCELERATION => "Beschleunigung (m/s^2)";
        public string STAT_TORQUE => "Drehmoment";
        public string STAT_POWER => "Leistung (kW)";
        public string STAT_HORSEPOWER => "Pferdestärken";
        public string STAT_THROTTLE => "Schubhebel";
        public string STAT_TEMPERATURE => "Temperatur";
        public string STAT_AMPS => "Amperemeter";
        public string STAT_RPM => "U/min";
    }
}
