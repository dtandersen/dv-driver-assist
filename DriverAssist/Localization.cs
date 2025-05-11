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

        string JOB_TITLE { get; }
        string JOB_ID { get; }
        string JOB_ORIGIN { get; }
        string JOB_DESTINATION { get; }
    }

    public class TranslationManager
    {
        static Translation instance;
        private static readonly Logger logger = LogFactory.GetLogger(typeof(TranslationManager));

        static TranslationManager()
        {
            instance = Init(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        }

        public static Translation Current
        {
            get
            {
                return instance;
            }
        }

        public static void SetLangage(string language)
        {
            logger.Info($"Using language {language}");

            instance = Init(language);
        }

        static Translation Init(string language)
        {
            Translation translation = language switch
            {
                "English" => new TranslationEN(),
                "German" => new TranslationDE(),
                "French" => new TranslationFR(),
                "Polish" => new TranslationPL(),
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

        public string JOB_TITLE => "Jobs";
        public string JOB_ID => "Job";
        public string JOB_ORIGIN => "Origin";
        public string JOB_DESTINATION => "Destination";
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

        public string JOB_TITLE => "Arbeitsplätze";
        public string JOB_ID => "Arbeit";
        public string JOB_ORIGIN => "Herkunft";
        public string JOB_DESTINATION => "Ziel";
    }

    public class TranslationFR : Translation
    {
        public string CC_SETPOINT => "Point de départ";
        public string CC_STATUS => "Status";
        public string CC_COASTING => "Roue libre";
        public string CC_DECELERATING => "Décéleration à {0} km/h";
        public string CC_ACCELERATING => "Accélération à {0} km/h";
        public string CC_CHANGING_DIRECTION => "Changement de direction";
        public string CC_STOPPING => "Arrêt";
        public string CC_WARNING_NEUTRAL => "Neutre : le levier de direction est au point mort";
        public string CC_DISABLED => "Désactivé";
        public string CC_UNSUPPORTED => "Neutre: {0} n'est pas supporté";

        public string STAT_LOCOMOTIVE => "Locomotive";
        public string STAT_MASS => "Masse";
        public string STAT_SPEED => "Vitesse (km/h)";
        public string STAT_ACCELERATION => "Accéleration (m/s^2)";
        public string STAT_TORQUE => "Couple";
        public string STAT_POWER => "Puissance (kW)";
        public string STAT_HORSEPOWER => "Cheveaux";
        public string STAT_THROTTLE => "Accélérateur";
        public string STAT_TEMPERATURE => "Température";
        public string STAT_TEMPERATURE_CHANGE => "Changement de température";
        public string STAT_AMPS => "Ampères";
        public string STAT_RPM => "Tr/min";

        public string LOCOMOTIVE => "Locomotive";
        public string CARGO => "Chargement";

        public string STAT_CURRENT => "Courant";
        public string STAT_CHANGE => "Δ";
        public string TRAIN => "Train";
        public string LOCO_ABBV => "Loco";

        public string JOB_TITLE => "Emploi";
        public string JOB_ID => "Travail";
        public string JOB_ORIGIN => "Origine";
        public string JOB_DESTINATION => "Destination";
    }

    public class TranslationPL : Translation
    {
        public string CC_SETPOINT => "Punkt wyjścia";
        public string CC_STATUS => "Status";
        public string CC_COASTING => "Jazda na luzie";
        public string CC_DECELERATING => "Zwalnianie do {0} km/h";
        public string CC_ACCELERATING => "Przyspieszanie do {0} km/h";
        public string CC_CHANGING_DIRECTION => "Zmienianie kierunku";
        public string CC_STOPPING => "Zatrzymywanie";
        public string CC_WARNING_NEUTRAL => "Neutralny: Nastawnik kierunkowy znajdue się w położeniu neutralnym";
        public string CC_DISABLED => "Wyłączony";
        public string CC_UNSUPPORTED => "Neutralny: {0} nie jest wspierane";

        public string STAT_LOCOMOTIVE => "Lokomotywa";
        public string STAT_MASS => "Masa";
        public string STAT_SPEED => "Prędkość (km/h)";
        public string STAT_ACCELERATION => "Przyspieszenie (m/s^2)";
        public string STAT_TORQUE => "Moment obrotowy";
        public string STAT_POWER => "Moc (kW)";
        public string STAT_HORSEPOWER => "Konie mechaniczne";
        public string STAT_THROTTLE => "Nastawnik jazdy";
        public string STAT_TEMPERATURE => "Temperatura";
        public string STAT_TEMPERATURE_CHANGE => "Zmiana temperatury";
        public string STAT_AMPS => "Ampery";
        public string STAT_RPM => "Obr/Min";

        public string LOCOMOTIVE => "Lokomotywa";
        public string CARGO => "Ładunek";

        public string STAT_CURRENT => "Aktualny";
        public string STAT_CHANGE => "Δ";
        public string TRAIN => "Pociąg";
        public string LOCO_ABBV => "Loka";

        public string JOB_TITLE => "Zlecenia";
        public string JOB_ID => "Zlecenie";
        public string JOB_ORIGIN => "Pochodzenie";
        public string JOB_DESTINATION => "Miejsce docelowe";
    }
}
