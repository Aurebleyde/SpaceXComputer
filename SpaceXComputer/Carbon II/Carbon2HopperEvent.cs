using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class Carbon2HopperEvent : Event
    {
        protected Connection connection;

        protected C2FirstStage firstStage;
        private double ut;

        public Carbon2HopperEvent(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            firstStage = new C2FirstStage(vessel, RocketBody.C2_FIRST_STAGE);
            var CarbonII = firstStage;

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Contains("Carbon II Hopper") && vesselTarget.Type.Equals(VesselType.Probe))
                {
                    firstStage.firstStage = vesselTarget;
                    firstStage.firstStage.Name = "Carbon II Hopper";
                    Console.WriteLine("CARBON Hopper : Carbon accisition signal.");
                    break;
                }
            }
            
            firstStage.firstStage.AutoPilot.Engage();
            firstStage.firstStage.AutoPilot.TargetPitch = 90;
            firstStage.firstStage.Control.Throttle = 1;
            firstStage.firstStage.Parts.Engines[0].Active = true;
            Console.WriteLine("CARBON Hopper : Liftoff.");

            while (firstStage.firstStage.Flight(null).SurfaceAltitude < 1000) { }

            firstStage.firstStage.Control.Throttle = 0;

            while (firstStage.firstStage.Flight(firstStage.firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > 0) { }
            firstStage.firstStage.Control.Brakes = true;
            firstStage.firstStage.AutoPilot.TargetPitch = 90;
            Thread LZ = new Thread(firstStage.LandingTarget);
            LZ.Start();
            firstStage.landingBurn();
            Thread.Sleep(9999999);
        }
    }
}
