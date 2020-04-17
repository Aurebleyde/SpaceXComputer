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

namespace SpaceXComputer
{
    public class F1FirstStage
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel firstStage;

        public F1FirstStage(Vessel vessel, RocketBody rocket)
        {
            firstStage = vessel;
            this.rocketBody = rocketBody;
        }

        public void F1Startup()
        {
            firstStage.AutoPilot.Engage();

            firstStage.Control.Throttle = 1;
            firstStage.AutoPilot.TargetPitchAndHeading(90, Startup.GetInstance().GetFlightInfo().getHead());

            firstStage.Parts.Engines[1].Active = true;

            Console.WriteLine("FALCON 1 : Main engine startup.");
            var during = 0;
            var thrust = 0f;
            while (during <= 100)
            {
                thrust = firstStage.Parts.Engines[1].Thrust;
                during = during + 1;
            }

            if (thrust < firstStage.Parts.Engines[1].AvailableThrust)
            {
                firstStage.Control.Throttle = 0;
                firstStage.Parts.Engines[1].Active = false;
                Console.WriteLine("FALCON 1 : Engine Abort.");
            }
            else
            {
                foreach (LaunchClamp clamp in firstStage.Parts.LaunchClamps)
                {
                    clamp.Release();
                }
                Console.WriteLine("FALCON 1 : Liftoff.");
            }
        }

        public Vessel GetVessel()
        {
            return firstStage;
        }

        public void SetVessel(Vessel vessel)
        {
            firstStage = vessel;
        }
    }
}
