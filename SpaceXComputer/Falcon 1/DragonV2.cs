using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.Trajectories;

namespace SpaceXComputer
{
    class DragonV2
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel dragonV2;

        public DragonV2(Vessel vessel, RocketBody rocketBody)
        {
            dragonV2 = vessel;
            this.rocketBody = rocketBody;
        }

        public void abortEngage()
        {
            var intAp = dragonV2.Orbit.ApoapsisAltitude;
            var intPit = 90 - 15;

            dragonV2.AutoPilot.Engage();
            dragonV2.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHead();
            dragonV2.AutoPilot.TargetPitch = intPit;

            dragonV2.Parts.WithTag("Draco")[0].Engine.Active = true;
            dragonV2.Parts.WithTag("Draco")[1].Engine.Active = true;
            dragonV2.Parts.WithTag("Draco")[2].Engine.Active = true;
            dragonV2.Parts.WithTag("Draco")[3].Engine.Active = true;

            while (dragonV2.Orbit.ApoapsisAltitude < intAp + 700)
            { dragonV2.Control.Throttle = 1; }

            while (/*dragonV2.Orbit.ApoapsisAltitude < intAp + 10000 || */dragonV2.Thrust > 100)
            { dragonV2.Control.Throttle = 1; }

            dragonV2.Control.Throttle = 0;
            dragonV2.AutoPilot.Disengage();
            dragonV2.Control.SAS = true;
            dragonV2.Control.SASMode = SASMode.Prograde;
            Thread.Sleep(1000);
            dragonV2.Control.SASMode = SASMode.Prograde;

            while (dragonV2.Orbit.TimeToApoapsis > 5)
            { }

            dragonV2.Parts.WithTag("DragonTrunk")[0].Decoupler.Decouple();
            dragonV2.Control.SAS = false;
            dragonV2.Parts.WithTag("Drogue")[0].Parachute.Deploy();
            dragonV2.Parts.WithTag("Drogue")[1].Parachute.Deploy();

            while (dragonV2.Flight(dragonV2.SurfaceReferenceFrame).SurfaceAltitude > 1500 && dragonV2.Flight(dragonV2.SurfaceReferenceFrame).MeanAltitude > 1500) { }

            dragonV2.Parts.WithTag("Chute")[0].Parachute.Deploy();
            dragonV2.Parts.WithTag("Chute")[1].Parachute.Deploy();
            dragonV2.Parts.WithTag("Chute")[2].Parachute.Deploy();
            dragonV2.Parts.WithTag("Chute")[3].Parachute.Deploy();

            /*var drogues = dragonV2.Parts.WithTag("Drogue");
            for (int i = 0; i <2; i++)
            {
                var modules = drogues[i].Modules;
                for (int j = 0; j < 2; j++)
                {
                    var m = modules[j];
                    if (m.Name == "ModuleParachute")
                    {
                        m.TriggerEvent("Cut Parachute");
                    }
                }
            }*/

            dragonV2.Control.ToggleActionGroup(0);
        }

        public Vessel GetVessel()
        {
            return dragonV2;
        }

        public void SetVessel(Vessel vessel)
        {
            dragonV2 = vessel;
        }
    }
}
