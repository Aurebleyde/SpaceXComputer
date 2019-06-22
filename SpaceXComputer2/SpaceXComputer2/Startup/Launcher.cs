
namespace SpaceXComputer2
{
    public class Launcher
    {
        private string rocket = "Starship";
        private string version = "Hopper";
        private bool reuse;
        private LandingType landingType;
        private string landingZone;
        private double apoapsis;
        private double periapsis;
        private double heading;
        private string launchSite;

        public void launcher(string rocket, string version, bool reuse, LandingType landingType, double apoapsis, double periapsis, double heading, string launchSite)
        {
            this.rocket = rocket;
            this.version = version;
            this.reuse = reuse;
            this.landingType = landingType;
            this.apoapsis = apoapsis;
            this.periapsis = periapsis;
            this.heading = heading;
            this.launchSite = launchSite;
        }

        public enum LandingType
        {
            ASDS,
            RTLS,
        }
         

        public string getRocket() { return rocket; }
        public string getVersion() { return version; }
    }
}
