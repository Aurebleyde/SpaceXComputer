using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using System.Windows.Forms;
using System.Threading.Tasks;

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
        protected 
        Event dragonFlightEvent;

        [STAThread]
        private static void LoadSupervisor()
        {
            Application.EnableVisualStyles();
            Application.Run(new FalconSupervisor());
        }

        public Falcon9Event(Vessel vessel, Connection connectionLink, Connection connectionFirstStageLink)
        {
            connection = connectionLink;
            connectionFirstStage = connectionFirstStageLink;
            firstStage = new F9FirstStage(vessel, RocketBody.F9_FIRST_STAGE);
            var Falcon9 = firstStage;

            Console.WriteLine("Second Stage accisition signal.");

            Console.WriteLine("First Stage accisition signal.");

            Console.WriteLine("Loading...");
            Task.Run(LoadSupervisor);
            while (FalconSupervisor.Instance == null) { Thread.Sleep(1000); }
            Console.WriteLine("Loaded !");

            Task.Run(() => { 

                while(true)
                {
                    FalconSupervisor.Execute(() =>
                    {
                        try
                        {
                            #region FirstStageThrust
                            FalconSupervisor.Instance.lb_PowerCentral.Text = Math.Round(firstStage.firstStage.Parts.WithTag("MainCentral")[0].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerSecond1.Text = Math.Round(firstStage.firstStage.Parts.WithTag("MainSecond")[1].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerSecond0.Text = Math.Round(firstStage.firstStage.Parts.WithTag("MainSecond")[0].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerMain1.Text = Math.Round(firstStage.firstStage.Parts.WithTag("Main")[1].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerMain2.Text = Math.Round(firstStage.firstStage.Parts.WithTag("Main")[2].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerMain3.Text = Math.Round(firstStage.firstStage.Parts.WithTag("Main")[3].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerMain4.Text = Math.Round(firstStage.firstStage.Parts.WithTag("Main")[4].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerMain5.Text = Math.Round(firstStage.firstStage.Parts.WithTag("Main")[5].Engine.Thrust).ToString() + "kN";
                            FalconSupervisor.Instance.lb_PowerMain0.Text = Math.Round(firstStage.firstStage.Parts.WithTag("Main")[0].Engine.Thrust).ToString() + "kN";
                            #endregion

                            #region CenterEngine
                            if (firstStage.firstStage.Parts.WithTag("MainCentral")[0].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("MainCentral")[0].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_CenterEngine.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("MainCentral")[0].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("MainCentral")[0].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_CenterEngine.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_CenterEngine.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }
                            #endregion

                            #region SecondsEngines
                            if (firstStage.firstStage.Parts.WithTag("MainSecond")[0].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("MainSecond")[0].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Second0.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("MainSecond")[0].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("MainSecond")[0].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Second0.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Second0.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }

                            if (firstStage.firstStage.Parts.WithTag("MainSecond")[1].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("MainSecond")[1].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Second1.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("MainSecond")[1].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("MainSecond")[1].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Second1.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Second1.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }
                            #endregion

                            #region MainEngines
                            if (firstStage.firstStage.Parts.WithTag("Main")[0].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[0].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Main0.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("Main")[0].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[0].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Main0.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Main0.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }

                            if (firstStage.firstStage.Parts.WithTag("Main")[1].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[1].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Main1.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("Main")[1].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[1].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Main1.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Main1.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }

                            if (firstStage.firstStage.Parts.WithTag("Main")[2].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[2].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Main2.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("Main")[2].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[2].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Main2.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Main2.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }

                            if (firstStage.firstStage.Parts.WithTag("Main")[3].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[3].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Main3.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("Main")[3].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[3].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Main3.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Main3.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }

                            if (firstStage.firstStage.Parts.WithTag("Main")[4].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[4].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Main4.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("Main")[4].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[4].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Main4.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Main4.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }

                            if (firstStage.firstStage.Parts.WithTag("Main")[5].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[5].Engine.Thrust < 700000)
                            {
                                FalconSupervisor.Instance.pb_Main5.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Yellow.png");
                            }
                            else if (firstStage.firstStage.Parts.WithTag("Main")[5].Engine.Thrust > 0 && firstStage.firstStage.Parts.WithTag("Main")[5].Engine.Thrust >= 700000)
                            {
                                FalconSupervisor.Instance.pb_Main5.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Green.png");
                            }
                            else
                            {
                                FalconSupervisor.Instance.pb_Main5.Image = Image.FromFile("C:/Users/Utilisateur/Documents/SpaceX Computer/SpaceXComputer/SpaceXComputer/SpaceX/Falcon 9/Engine-Red.png");
                            }
                            #endregion



                            FalconSupervisor.Instance.lb_Debug.Text = "Speed : " + Math.Round(firstStage.firstStage.Flight(firstStage.firstStage.SurfaceReferenceFrame).TrueAirSpeed).ToString();
                        }
                        catch
                        {

                        }
                    });

                    Thread.Sleep(200);
                }

            });

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
            //Boostback.Start();
            /*secondStage.SecondStageStartup();
            Thread FairingSep = new Thread(secondStage.fairingSep);
            FairingSep.Start();
            secondStage.SECO(vessel, connection);
            secondStage.satSep();*/

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
                double dif = (difSup - 90) / 2.9; //1.8 ASDS //2.9 RTLS
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

                if (firstStage.firstStage.Flight(firstStage.firstStage.SurfaceReferenceFrame).TrueAirSpeed > 1600 /*RTLS = > 1600 | ASDS = > 2050*/ /*firstStage.firstStage.Thrust < 50*/)
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
