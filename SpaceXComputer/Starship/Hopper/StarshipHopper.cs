using System;
using System.Threading;
using System.Threading.Tasks;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using UnityEngine;
using UnityEngineInternal;
using Trajectories;
using systemAlias = global::System;
using Google.Protobuf;
using KRPC.Client.Services.Trajectories;
using System.Linq;

namespace SpaceXComputer
{
    public class StarshipHopper
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel starship;

        public StarshipHopper(Vessel vessel, RocketBody rocketBody)
        {
            starship = vessel;
            this.rocketBody = rocketBody;
        }

        public void StarshipStartup(Connection connection)
        {
            starship.Control.Throttle = 0;
            starship.AutoPilot.Engage();
            starship.AutoPilot.TargetPitchAndHeading(90, 0);
            starship.AutoPilot.TargetRoll = 0;

            starship.Parts.Engines[0].Active = true;
            starship.Parts.Engines[1].Active = true;
            starship.Parts.Engines[2].Active = true;

            double twr = TWR();
            Console.WriteLine("TWR = {0}", starship.Mass * starship.Orbit.Body.SurfaceGravity / starship.MaxThrust);
            Console.WriteLine("throttle set = {0}", 1.02f * starship.Mass * starship.Orbit.Body.SurfaceGravity / starship.MaxThrust);

            var lat = starship.Flight(starship.SurfaceReferenceFrame).Latitude;
            var lon = starship.Flight(starship.SurfaceReferenceFrame).Longitude;

            Console.WriteLine("Latitude at start : {0} // Longitude at start : {1}", lat, lon);

            float throt = 1.02f * starship.Mass * starship.Orbit.Body.SurfaceGravity / starship.MaxThrust;
            Console.WriteLine("Calcul = " + throt);

            starship.Control.Throttle = Convert.ToSingle(0.70);

            Thread.Sleep(5000);
            Console.WriteLine("Throttle of Starship: " + starship.Control.Throttle);

            var latF = connection.Trajectories().ImpactPos().Item1;
            var lonF = connection.Trajectories().ImpactPos().Item2;

            Console.WriteLine("Latitude in flight : {0} // Longitude in flight : {1}", latF, lonF);

            starship.Control.Throttle = 0.50f * 1 / Convert.ToSingle(twr);

            while (starship.Flight(starship.SurfaceReferenceFrame).VerticalSpeed > -0.5) { }
            while (starship.Flight(starship.SurfaceReferenceFrame).VerticalSpeed > 0.3 || starship.Flight(starship.SurfaceReferenceFrame).VerticalSpeed < -0.3)
            { }

            var latE = starship.Flight(starship.SurfaceReferenceFrame).Latitude;
            var lonE = starship.Flight(starship.SurfaceReferenceFrame).Longitude;

            Console.WriteLine("Latitude at end : {0} // Longitude at end : {1}", latE, lonE);

            starship.Control.Throttle = 0;
            starship.Parts.Engines[0].Active = false;
            starship.Parts.Engines[1].Active = false;
            starship.Parts.Engines[2].Active = false;
        }

        public double TWR()
        {
            var Ft = starship.MaxThrust;
            var Fw = starship.Mass * starship.Orbit.Body.SurfaceGravity;
            return Ft / Fw;
            //return twr;
        }

        public Vessel GetVessel()
        {
            return starship;
        }

        public void SetVessel(Vessel vessel)
        {
            starship = vessel;
        }
    }
}
