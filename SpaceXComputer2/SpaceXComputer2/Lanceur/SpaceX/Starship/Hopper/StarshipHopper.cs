using System;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using System.Threading;

namespace SpaceXComputer2
{
    public class StarshipHopper
    {
        private Connection connection;

        public Vessel starshipHopper;

        public StarshipHopper(Vessel vessel)
        {
            starshipHopper = vessel;
        }

        private double initAltitude;
        private Tuple<Double, Double> initCoordinate;
        private float TWR;
        private double maxAltitude;

        public void Hopp()
        {
            Console.WriteLine("Starship Hopper will do a hopp in 10s.");
            Thread maxAlti = new Thread(MaxAltitude);

            Thread.Sleep(10000);

            initAltitude = starshipHopper.Flight(null).SurfaceAltitude;
            initCoordinate = Tuple.Create(starshipHopper.Flight(null).Latitude, starshipHopper.Flight(null).Longitude);

            TWR = starshipHopper.Mass * starshipHopper.Orbit.Body.SurfaceGravity / starshipHopper.AvailableThrust;
            starshipHopper.Control.Throttle = 1.20f * TWR;
            starshipHopper.AutoPilot.Engage();
            starshipHopper.AutoPilot.TargetPitch = 90;

            for (int i = 0; i < 3; i++)
            {
                starshipHopper.Parts.WithTag("Raptor")[i].Engine.Active = true;
            }

            Console.WriteLine("Liftoff of Starship Hopper.");
            maxAlti.Start();

            while (starshipHopper.Flight(null).SurfaceAltitude - initAltitude <= 5) { }

            TWR = starshipHopper.Mass * starshipHopper.Orbit.Body.SurfaceGravity / starshipHopper.AvailableThrust;

            while (starshipHopper.Flight(starshipHopper.Orbit.Body.ReferenceFrame).VerticalSpeed > -1) { TWR = starshipHopper.Mass * starshipHopper.Orbit.Body.SurfaceGravity / starshipHopper.AvailableThrust; starshipHopper.Control.Throttle = 0.85f * TWR; }
            while (starshipHopper.Flight(starshipHopper.Orbit.Body.ReferenceFrame).VerticalSpeed < 0) { TWR = starshipHopper.Mass * starshipHopper.Orbit.Body.SurfaceGravity / starshipHopper.AvailableThrust; starshipHopper.Control.Throttle = 0.97f * TWR; }

            for (int i = 0; i < 3; i++)
            {
                starshipHopper.Parts.WithTag("Raptor")[i].Engine.Active = false;
            }
            starshipHopper.Control.Throttle = 0;
            Console.WriteLine("Landing of Starship Hopper.");

            Thread.Sleep(1000);
            EndTestFlight();
        }

        public void EndTestFlight()
        {
            Console.WriteLine("Initial coordinate : \nLatitude = {0} \nLongitude = {1} \nFinal coordinate : \nLatitude = {2} \nLongitude = {3} \nMaximum altitude = {4}", initCoordinate.Item1, initCoordinate.Item2, starshipHopper.Flight(null).Latitude, starshipHopper.Flight(null).Longitude, maxAltitude);
            Console.ReadKey();
        }

        public void MaxAltitude()
        {
            while (starshipHopper.Flight(null).VerticalSpeed >= 0)
            {
                maxAltitude = starshipHopper.Flight(null).SurfaceAltitude - initAltitude;
            }
        }
    }
}
