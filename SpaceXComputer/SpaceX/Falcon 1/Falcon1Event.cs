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
    class Falcon1Event : Event
    {
        protected Connection connection;

        protected F1FirstStage firstStage;
        protected F1SecondStage secondStage;

        public Falcon1Event(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            firstStage = new F1FirstStage(vessel, RocketBody.F1_FIRST_STAGE);

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Contains("Falcon 1") && vesselTarget.Type.Equals(VesselType.Probe))
                {
                    firstStage.firstStage = vesselTarget;
                    firstStage.firstStage.Name = "F1 Full";
                    Console.WriteLine("FALCON 1 : Falcon 1 accisition signal.");
                    break;
                }
            }

            firstStage.F1Startup();
            while (firstStage.firstStage.Flight(null).MeanAltitude < 300)
            {
                Thread.Sleep(100);
            }

            Thread GravityTurn = new Thread(gravityTurn);
            GravityTurn.Start();
            stageSep();

            secondStage = new F1SecondStage(vessel);

            foreach (Vessel vesselTargetSecond in connection.SpaceCenter().Vessels)
            {
                if ((vesselTargetSecond.Name.Contains("F1") || vesselTargetSecond.Name.Contains("Falcon 1")) && vesselTargetSecond.Type.Equals(VesselType.Probe))
                {
                    secondStage.secondStage = vesselTargetSecond;
                    secondStage.secondStage.Name = "Falcon 1 Second Stage";
                    Console.WriteLine("STAGE 2 : Second stage accisition signal.");
                    break;
                }
            }

            secondStage.SecondStageStartup();
        }

        public void gravityTurn()
        {
            var Ft = firstStage.firstStage.Thrust;
            var Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var TWRstart = TWR;
            var pit = 90f;

            while (pit > 50)
            {
                Ft = firstStage.firstStage.Thrust;
                Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
                TWR = Ft / Fw;

                var difSup = (90 * TWR) / TWRstart;
                var dif = difSup - 90;
                pit = 90 - dif;
                firstStage.firstStage.AutoPilot.TargetPitch = pit;
            }
        }

        public void stageSep()
        {
            var thrust = firstStage.firstStage.Thrust;

            while (true)
            {
                thrust = firstStage.firstStage.Parts.Engines[0].Thrust;

                if (thrust == 0)
                {
                    Console.WriteLine("FALCON 1 : MECO.");
                    Thread.Sleep(1500);
                    firstStage.firstStage.Parts.Decouplers[0].Decouple();
                    Console.WriteLine("FALCON 1 : Stage separation.");
                    break;
                }
            }
        }
    }
}
