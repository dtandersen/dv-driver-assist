using System.Globalization;
using UnityEngine;

namespace DriverAssist.Localization
{
    public interface Translation
    {
        string CC_SETPOINT { get; }
        string CC_STATUS { get; }

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

        internal static void Init()
        {
            string language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
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
