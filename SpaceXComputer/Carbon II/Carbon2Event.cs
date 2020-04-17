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
    class Carbon2Event : Event
    {
        protected Connection connection;

        protected C2FirstStage firstStage;
        protected C2SecondStage secondStage;
        protected C2ThirdStage thirdStage;
        

        public Carbon2Event(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            firstStage = new C2FirstStage(vessel, RocketBody.C2_FIRST_STAGE);
            var CarbonII = firstStage;

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Contains("Carbon II") && vesselTarget.Type.Equals(VesselType.Probe))
                {
                    firstStage.firstStage = vesselTarget;
                    firstStage.firstStage.Name = "Carbon II Full";
                    Console.WriteLine("CARBON II : Carbon II accisition signal.");
                    break;
                }
            }

            CarbonII.C2Startup();
            while (firstStage.firstStage.Flight(null).MeanAltitude < 300)
            {
                Thread.Sleep(100);
            }

            Thread GravityTurn = new Thread(gravityTurn);
            GravityTurn.Start();
            stageSep();

            secondStage = new C2SecondStage(vessel);

            foreach (Vessel vesselTargetSecond in connection.SpaceCenter().Vessels)
            {
                if ((vesselTargetSecond.Name.Contains("Carbon II") || vesselTargetSecond.Name.Contains("C2")) && vesselTargetSecond.Type.Equals(VesselType.Probe))
                {
                    secondStage.secondStage = vesselTargetSecond;
                    secondStage.secondStage.Name = "Carbon II Second Stage";
                    Console.WriteLine("CARBON II : Second stage accisition signal.");
                    break;
                }
            }

            secondStage.SecondStageStartup();
            Thread FairingSep = new Thread(secondStage.fairingSep);
            FairingSep.Start();
            Thread SECO = new Thread(secondStage.SECO);
            SECO.Start();
            stage2Sep();

            thirdStage = new C2ThirdStage(vessel);
            foreach (Vessel vesselTargetThird in connection.SpaceCenter().Vessels)
            {
                if ((vesselTargetThird.Name.Contains("Carbon II") != vesselTargetThird.Name.Contains("Full")) && vesselTargetThird.Type.Equals(VesselType.Probe))
                {
                    secondStage.secondStage = vesselTargetThird;
                    thirdStage.thirdStage = vesselTargetThird;
                    thirdStage.thirdStage.Name = "Carbon II Third Stage";
                    Console.WriteLine("CARBON II : Third stage accisition signal.");
                }
            }

        }

        public void gravityTurn()
        {
            var Ft = firstStage.firstStage.Thrust;
            var Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var TWRstart = TWR;
            var pit = 90f;

            while (pit > 40)
            {
                Ft = firstStage.firstStage.Thrust;
                Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
                TWR = Ft / Fw;

                var difSup = ((90 * TWR) / TWRstart);
                var dif = (difSup - 90) / 1.8;
                float dif2 = Convert.ToSingle(dif);
                pit = 90 - dif2;
                firstStage.firstStage.AutoPilot.TargetPitch = pit;

                if (TWR == 0)
                {
                    break;
                }
            }
        }

        public void stageSep()
        {
            var thrust = firstStage.firstStage.Thrust;

            while (true)
            {
                thrust = firstStage.firstStage.Thrust;

                if (thrust == 0)
                {
                    Console.WriteLine("CARBON II : MECO.");
                    Thread.Sleep(1500);
                    firstStage.firstStage.Parts.Decouplers[1].Decouple();
                    Console.WriteLine("CARBON II : Stage separation.");
                    Thread gt = new Thread(gravityTurn);
                    gt.Abort();
                    break;
                }
            }
        }

        public void stage2Sep()
        {
            var thrust = secondStage.secondStage.Thrust;

            while (true)
            {
                thrust = secondStage.secondStage.Thrust;
                if (secondStage.secondStage.Control.Throttle == 0)
                {
                    while (secondStage.secondStage.Thrust == 0)
                    {
                        Thread.Sleep(1000);
                    }
                }

                if (thrust == 0 && secondStage.secondStage.Control.Throttle == 1)
                {
                    secondStage.secondStage.Control.Throttle = 0;
                    Console.WriteLine("CARBON II : SECO.");
                    Thread SECO = new Thread(secondStage.SECO);
                    Thread SES = new Thread(secondStage.SESSECO2);
                    SECO.Abort();
                    SES.Abort();
                    Thread.Sleep(1500);
                    secondStage.secondStage.Parts.Decouplers[0].Decouple();
                    Console.WriteLine("CARBON II : Stage 2 separation.");
                    break;
                }
            }
        }
    }
}
