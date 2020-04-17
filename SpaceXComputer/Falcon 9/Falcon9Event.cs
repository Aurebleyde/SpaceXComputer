using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class Falcon9Event : Event
    {
        protected Connection connection;
        protected Connection connectionFirstStage;
        protected F9FirstStage firstStage;
        protected F9SecondStage secondStage;
        protected F9FirstStage droneShip;
        protected F9SecondStage satTarget;
        Event dragonFlightEvent;


        public Falcon9Event(Vessel vessel, Connection connectionLink, Connection connectionFirstStageLink)
        {
            connection = connectionLink;
            connectionFirstStage = connectionFirstStageLink;
            firstStage = new F9FirstStage(vessel, RocketBody.F9_FIRST_STAGE);
            var Falcon9 = firstStage;

            Console.WriteLine("Second Stage accisition signal.");

            Console.WriteLine("First Stage accisition signal.");

            //Thread.Sleep(5000);

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (Startup.GetInstance().GetFlightInfo().getDragon() == false)
                {
                    if (vesselTarget.Name.Contains("Dragon SpX-C2") && vesselTarget.Type.Equals(VesselType.Probe))
                    {
                        firstStage.firstStage = vesselTarget;
                        firstStage.firstStage.Name = "Falcon 9 Full";
                        Console.WriteLine("FALCON 9 : Falcon 9 accisition signal.");
                        break;
                    }
                }
                else if (Startup.GetInstance().GetFlightInfo().getDragon() == true)
                {
                    if (vesselTarget.Name.Equals("Falcon 9 Dragon COTS-1") && vesselTarget.Type.Equals(VesselType.Probe))
                    {
                        firstStage.firstStage = vesselTarget;
                        firstStage.firstStage.Name = "Falcon 9 Dragon";
                        Console.WriteLine("FALCON 9 : Falcon 9 accisition signal.");
                        break;
                    }
                }
            }

            if (Startup.GetInstance().GetFlightInfo().getLZ() == "OCISLY")
            {
                droneShip = new F9FirstStage(vessel, RocketBody.OCISLY);
                foreach (Vessel vesselTarget in connectionFirstStage.SpaceCenter().Vessels)
                {
                    if (vesselTarget.Name.Contains("Of Course I Still Love You") && vesselTarget.Type.Equals(VesselType.Relay))
                    {
                        firstStage.droneShip = vesselTarget;
                        firstStage.droneShip.Name = "Of Course I Still Love You";
                        Console.WriteLine("FIRST STAGE : OCISLY accisition signal.");
                        Console.WriteLine($"Lat = {firstStage.droneShip.Flight(firstStage.droneShip.SurfaceReferenceFrame).Latitude} | Long = {firstStage.droneShip.Flight(firstStage.droneShip.SurfaceReferenceFrame).Longitude}");
                        //break;
                    }
                }
            }

            //Falcon9.F9Startup(connection, connectionFirstStageLink);
            /*Thread Abort = new Thread(FlightAbort);
            Abort.Start();*/

            /*while (firstStage.firstStage.Flight(null).SurfaceAltitude < 100)
            {
                Thread.Sleep(100);
            }

            Thread GravityTurn = new Thread(gravityTurn);
            GravityTurn.Start();
            stageSep();

            foreach (Vessel vesselTargetFirst in connectionFirstStage.SpaceCenter().Vessels)
            {
                if (vesselTargetFirst.Name.Contains("Falcon 9.1") && vesselTargetFirst.Name.Contains("NRS-F") && vesselTargetFirst.Type.Equals(VesselType.Probe))
                {
                    secondStage = new F9SecondStage(vessel, RocketBody.F9_SECOND_STAGE);
                    firstStage.secondStage = secondStage;
                    secondStage.secondStage.Control.Forward = 1;
                    secondStage.secondStage.Name = "F9 Second Stage";
                    secondStage.rocketBody = RocketBody.F9_SECOND_STAGE;
                    Console.WriteLine("FALCON 9 : Second Stage as configured.");
                }
            }

            
            firstStage.switchToSecondStage = true;

            /*foreach (Vessel vesselTargetSecond in connection.SpaceCenter().Vessels)
            {
                if (Startup.GetInstance().GetFlightInfo().getDragon() == false)
                {
                    if (vesselTargetSecond.Name.Equals("Falcon 9 Block 6.1") && vesselTargetSecond.Type.Equals(VesselType.Probe))
                    {
                        secondStage.secondStage = vesselTargetSecond;
                        secondStage.secondStage.Name = "Falcon 9 Second Stage";
                        Console.WriteLine("FALCON 9 : Second stage accisition signal.");
                        break;
                    }
                }
                else if (Startup.GetInstance().GetFlightInfo().getDragon() == true)
                {
                    if (vesselTargetSecond.Name.Equals("Falcon 9 Dragon COTS-1") && vesselTargetSecond.Type.Equals(VesselType.Probe))
                    {
                        secondStage.secondStage = vesselTargetSecond;
                        secondStage.secondStage.Name = "Falcon 9 Second Stage";
                        Console.WriteLine("FALCON 9 : Second stage accisition signal.");
                        break;
                    }
                }
            }*/
            foreach (Vessel vesselTargetFirst in connectionFirstStage.SpaceCenter().Vessels)
            {
                if (vesselTargetFirst.Name.Contains("Falcon 9.1") && vesselTargetFirst.Name.Contains("Sonde") && vesselTargetFirst.Type.Equals(VesselType.Probe))
                {
                    firstStage.firstStage = vesselTargetFirst;
                    firstStage.firstStage.Name = "F9 First Stage";
                    firstStage.rocketBody = RocketBody.F9_FIRST_STAGE;
                    Console.WriteLine("FALCON 9 : First Stage as configured.");
                }
            }

            if (Startup.GetInstance().GetFlightInfo().getLZ() == "OCISLY")
            {
                droneShip = new F9FirstStage(vessel, RocketBody.OCISLY);
                foreach (Vessel vesselTarget in connectionFirstStage.SpaceCenter().Vessels)
                {
                    if (vesselTarget.Name.Contains("Of Course I Still Love You") && vesselTarget.Type.Equals(VesselType.Relay))
                    {
                        firstStage.droneShip = vesselTarget;
                        firstStage.droneShip.Name = "Of Course I Still Love You";
                        Console.WriteLine("FIRST STAGE : OCISLY accisition signal.");
                        break;
                    }
                }
            }
            //connectionFirstStage = new Connection(address: IPAddress.Parse("192.168.1.88"), rpcPort: 50000, streamPort: 50001);
            firstStage.ConnectionF91stStage(connection);
            Thread Boostback = new Thread(firstStage.boostbackStart);
            Boostback.Start();
            /*secondStage.SecondStageStartup();
            Thread FairingSep = new Thread(secondStage.fairingSep);
            FairingSep.Start();
            secondStage.SECO(vessel, connection);
            secondStage.satSep();
            

            /*satTarget = new F9SecondStage(vessel, RocketBody.SATTARGET);
            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Equals("Polar Constellation 1") && vesselTarget.Type.Equals(VesselType.Relay))
                {
                    secondStage.satTarget = vesselTarget;
                    Console.WriteLine("SECOND STAGE : Sat target accisition signal.");
                    break;
                }
            }*/
            /*secondStage.circularisation(vessel, connection);

            if (Startup.GetInstance().GetFlightInfo().getGrap() == true)
            { secondStage.grapSep(); }
            else
            {
                if (Startup.GetInstance().GetFlightInfo().getStarlink() == true)
                { secondStage.StarlinkSep(); }
                else
                { secondStage.satSep(); }
            }

            if (Startup.GetInstance().GetFlightInfo().getDragon() == true)
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                dragonFlightEvent = new DragonEvent(spaceCenter.ActiveVessel, connection);
            }*/

            Console.WriteLine("Stop ?");
            while (Console.ReadLine() != "stop")
            {

            }
        }

        public bool Abort;
        public void FlightAbort()
        {
            while (true)
            {
                if (firstStage.firstStage.Parts.WithTag("MainCentral")[0].Engine.Thrust <= 1000)
                {
                    Abort = true;
                    connection = null;
                    connectionFirstStage = null;
                }
                else
                { 
                    Abort = false;
                }
            }
        }

        public void gravityTurn()
        {
            firstStage.firstStage.AutoPilot.TargetRoll = 0;
            var Ft = firstStage.firstStage.Thrust;
            var Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var TWRstart = TWR;
            var pit = 90f;

            while (pit > 35 && firstStage.firstStage.Orbit.ApoapsisAltitude < 120000)
            {
                Ft = firstStage.firstStage.Thrust;
                Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
                TWR = Ft / Fw;

                var difSup = ((90 * TWR) / TWRstart);
                double dif = (difSup - 90) / 1.8; //1.8 ASDS //2.9 RTLS
                float dif2 = Convert.ToSingle(dif);
                pit = 90 - dif2;
                firstStage.firstStage.AutoPilot.TargetPitch = pit;

                if (TWR == 0)
                {
                    firstStage.firstStage.AutoPilot.TargetPitch = 90;
                    break;
                }
            }

            firstStage.firstStage.AutoPilot.TargetPitch = 30;
        }

        public void gravityTurn2()
        {
            var pit = 90f;

            while (pit > 35 && firstStage.firstStage.Orbit.ApoapsisAltitude < 120000)
            {
                pit = Convert.ToSingle(90 - (Math.Sqrt(firstStage.firstStage.Flight(firstStage.firstStage.SurfaceReferenceFrame).SurfaceAltitude) / 5));
                firstStage.firstStage.AutoPilot.TargetPitch = pit;

                firstStage.firstStage.AutoPilot.TargetRoll = 0;
                var Ft = firstStage.firstStage.Thrust;
                var Fw = firstStage.firstStage.Mass * firstStage.firstStage.Orbit.Body.SurfaceGravity;
                var TWR = Ft / Fw;

                if (TWR == 0)
                {
                    firstStage.firstStage.AutoPilot.TargetPitch = 90;
                    break;
                }
            }

            firstStage.firstStage.AutoPilot.TargetPitch = 30;
        }

        public void stageSep()
        {
            var thrust = firstStage.firstStage.Thrust;

            while (true)
            {
                //thrust = firstStage.firstStage.Thrust;
                //var vesselFuel = (connection.AddStream(() => firstStage.firstStage.Resources.Amount("LiquidFuel")));
                //RTLS = 42%, ASDS = 33.5%, Exp = 1%
                //var percentage = (33.5 * (9336.4 * 3 + 4668.2)) / 100;
                firstStage.firstStage.Control.Throttle = 1;

                if (firstStage.firstStage.Flight(firstStage.firstStage.SurfaceReferenceFrame).TrueAirSpeed > 2050 /*RTLS = > 1600 | ASDS = > 2050*/ /*firstStage.firstStage.Thrust < 50*/)
                {
                    Console.WriteLine("FALCON 9 : MECO.");
                    firstStage.firstStage.Control.Throttle = 0;
                    firstStage.firstStage.Control.RCS = true;
                    firstStage.firstStage.Control.ToggleActionGroup(4);
                    Thread.Sleep(1500);
                    if (Startup.GetInstance().GetFlightInfo().getDragon() == true)
                    {
                        firstStage.firstStage.Parts.Decouplers[3].Decouple();
                    }
                    else
                    {
                        //firstStage.firstStage.Parts.Decouplers[1].Decouple();
                        /*firstStage.firstStage.Parts.WithTag("SepEngine")[0].Engine.Active = true;
                        firstStage.firstStage.Parts.WithTag("SepEngine")[1].Engine.Active = true;
                        firstStage.firstStage.Parts.WithTag("SepEngine")[2].Engine.Active = true;
                        firstStage.firstStage.Parts.WithTag("SepEngine")[3].Engine.Active = true;*/

                        firstStage.firstStage.Control.ToggleActionGroup(7);
                        //firstStage.firstStage.Parts.WithTag("StageSep")[0].Decoupler.Decouple();
                    }
                    Console.WriteLine("FALCON 9 : Stage separation.");
                    Thread gt = new Thread(gravityTurn);
                    gt.Abort();
                    break;
                }
            }
        }
    }
}
