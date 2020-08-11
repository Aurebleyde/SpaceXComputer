using System;
using System.Threading;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class FalconheavyEvent : Event
    {

        protected Connection connection;
        protected Connection connectionFirstStage;
        protected FHCenterCore centerCore;
        protected F9FirstStage sideBoosterA;
        protected F9FirstStage sideBoosterB;
        protected FHCenterCore droneShip;
        protected F9SecondStage secondStage;
        protected F9SecondStage satTarget;

        public FalconheavyEvent(Vessel vessel, Connection connectionLink)
        {

            connection = connectionLink;
            centerCore = new FHCenterCore(vessel, RocketBody.FH_CENTERCORE);
            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Equals("Falcon Heavy") && vesselTarget.Type.Equals(VesselType.Probe))
                {
                    centerCore.centerCore = vesselTarget;
                    centerCore.centerCore.Name = "Falcon Heavy Full";
                    Console.WriteLine("FH : Falcon Heavy accisition signal.");
                    break;
                }
            }

            centerCore.FHStartup(connection);
            Thread GT = new Thread(gravityTurn);
            GT.Start();
            BECO();

            sideBoosterA = new F9FirstStage(vessel, RocketBody.FH_SIDEBOOSTER_A);
            foreach (Vessel vesselTargetFirst in connection.SpaceCenter().Vessels)
            {
                if (vesselTargetFirst.Name.Equals("FH Side Booster B") && vesselTargetFirst.Type.Equals(VesselType.Probe))
                {
                    sideBoosterA.firstStage = vesselTargetFirst;
                    sideBoosterA.firstStage.Name = "FH Side Booster B";
                    Console.WriteLine("FH : SideBoosterB as configured.");
                }
            }
            sideBoosterB = new F9FirstStage(vessel, RocketBody.FH_SIDEBOOSTER_B);
            foreach (Vessel vesselTargetFirst in connection.SpaceCenter().Vessels)
            {
                if (vesselTargetFirst.Name.Equals("FH Side Booster A") && vesselTargetFirst.Type.Equals(VesselType.Probe))
                {
                    sideBoosterA.firstStage = vesselTargetFirst;
                    sideBoosterA.firstStage.Name = "FH Side Booster A";
                    Console.WriteLine("FH : SideBoosterA as configured.");
                }
            }
            sideBoosterA.FHsignal(connectionLink, connectionFirstStage);
            sideBoosterB.FHsignal(connectionLink, connectionFirstStage);
            Thread BoostbackA = new Thread(sideBoosterA.boostbackStart);
            BoostbackA.Start();
            Thread BoostbackB = new Thread(sideBoosterB.boostbackStart);
            //BoostbackB.Start();

            /*stageSep();

            secondStage = new F9SecondStage(vessel, RocketBody.F9_SECOND_STAGE);

            foreach (Vessel vesselTargetSecond in connection.SpaceCenter().Vessels)
            {
                if (vesselTargetSecond.Name.Equals("Falcon Heavy Block 6") && vesselTargetSecond.Type.Equals(VesselType.Probe))
                {
                    secondStage.secondStage = vesselTargetSecond;
                    secondStage.secondStage.Name = "Falcon Heavy Second Stage";
                    Console.WriteLine("FH : Second stage accisition signal.");
                    break;
                }
            }

            secondStage.SecondStageStartup();
            Thread FairingSep = new Thread(secondStage.fairingSep);
            FairingSep.Start();
            secondStage.SECO(vessel, connection);
            secondStage.satSep();*/
        }

        public void gravityTurn()
        {
            centerCore.centerCore.AutoPilot.TargetRoll = 90;
            var Ft = centerCore.centerCore.Thrust;
            var Fw = centerCore.centerCore.Mass * centerCore.centerCore.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var TWRstart = TWR;
            var pit = 90f;

            while (pit > 35 && centerCore.centerCore.Orbit.ApoapsisAltitude < 120000)
            {
                Ft = centerCore.centerCore.Thrust;
                Fw = centerCore.centerCore.Mass * centerCore.centerCore.Orbit.Body.SurfaceGravity;
                TWR = Ft / Fw;

                var difSup = ((90 * TWR) / TWRstart);
                double dif = (difSup - 90) / 3; //1.8 ASDS //2.9 RTLS
                float dif2 = Convert.ToSingle(dif);
                pit = 90 - dif2;
                centerCore.centerCore.AutoPilot.TargetPitch = pit;

                if (TWR == 0)
                {
                    centerCore.centerCore.AutoPilot.TargetPitch = 90;
                    break;
                }
            }

            centerCore.centerCore.AutoPilot.TargetPitch = 30;
        }

        public void BECO()
        {
            var thrust = centerCore.centerCore.Thrust;

            while (true)
            {
                thrust = centerCore.centerCore.Thrust;
                /*var vesselFuel = (connection.AddStream(() => centerCore.centerCore.Resources.Amount("LiquidFuel")));
                //RTLS = 30%, ASDS = 20%, Exp = 1%
                var percentage = (30 * (9336.4 * 3 + 4668.2)) / 100;*/
                centerCore.centerCore.Control.Throttle = 1;

                if (/*vesselFuel.Get()/3 - ((4668.2 + 1167)) <= percentage*/ centerCore.centerCore.Flight(centerCore.centerCore.SurfaceReferenceFrame).TrueAirSpeed > 1550) //RTLS : 1600
                {
                    Console.WriteLine("FH : BECO.");
                    centerCore.centerCore.Control.ToggleActionGroup(3);

                    centerCore.centerCore.Parts.WithTag("MainCentral")[0].Engine.ThrustLimit = 1;
                    centerCore.centerCore.Parts.WithTag("MainSecond")[0].Engine.ThrustLimit = 1;
                    centerCore.centerCore.Parts.WithTag("MainSecond")[1].Engine.ThrustLimit = 1;
                    for (int i = 0; i < 6; i++)
                    {
                        centerCore.centerCore.Parts.WithTag("Main")[i].Engine.ThrustLimit = 1;
                    }

                    Thread.Sleep(500);
                    //centerCore.centerCore.Parts.Decouplers[1].Decouple();
                    centerCore.centerCore.Control.ToggleActionGroup(6);
                    Console.WriteLine("FH : Boosters separation.");



                    Thread gt = new Thread(gravityTurn);
                    gt.Abort();
                    centerCore.centerCore.AutoPilot.TargetPitch = 20;
                    break;
                }
            }
        }

        public void stageSep()
        {
            var thrust = centerCore.centerCore.Thrust;

            while (true)
            {
                thrust = centerCore.centerCore.Thrust;
                var vesselFuel = (connection.AddStream(() => centerCore.centerCore.Resources.Amount("LiquidFuel")));
                //RTLS = 30%, ASDS = 20%, Exp = 1%
                var percentage = (20 * (9336.4 * 3 + 4668.2)) / 100;
                centerCore.centerCore.Control.Throttle = 1;

                if (vesselFuel.Get() - (4668.2 + 1167) <= percentage)
                {
                    Console.WriteLine("FH : MECO.");
                    centerCore.centerCore.Control.Throttle = 0;
                    Thread.Sleep(1500);
                    if (Startup.GetInstance().GetFlightInfo().getDragon() == true)
                    {
                        centerCore.centerCore.Parts.Decouplers[3].Decouple();
                    }
                    else
                    {
                        //firstStage.firstStage.Parts.Decouplers[1].Decouple();
                        centerCore.centerCore.Parts.WithTag("StageSep")[0].Decoupler.Decouple();
                    }
                    Console.WriteLine("FH : Stage separation.");
                    break;
                }
            }
        }
    }
}
