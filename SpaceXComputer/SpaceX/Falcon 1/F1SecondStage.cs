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
    public class F1SecondStage
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel secondStage;

        public F1SecondStage(Vessel vessel)
        {
            secondStage = vessel;
        }

        public void SecondStageStartup()
        {
            secondStage.AutoPilot.Engage();
            secondStage.AutoPilot.TargetPitchAndHeading(45, Startup.GetInstance().GetFlightInfo().getHead());
            Thread.Sleep(800);
            secondStage.Control.Throttle = 1;
            secondStage.Parts.Engines[0].Active = true;
            Console.WriteLine("STAGE 2 : Second engine startup.");
        }

        public void fairingSep()
        {
            while (true)
            {
                if (secondStage.Flight(null).MeanAltitude > 60000)
                {
                    secondStage.Parts.Fairings[0].Jettison();
                    Console.WriteLine("STAGE 2 : Fairing separation.");
                    break;
                }
            }
        }



        public Vessel GetVessel()
        {
            return secondStage;
        }

        public void SetVessel(Vessel vessel)
        {
            secondStage = vessel;
        }
    }
}
