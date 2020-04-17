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
using UnityEngine;
using UnityEngineInternal;
using Trajectories;
using systemAlias = global::System;
using Google.Protobuf;
using KRPC.Client.Services.Trajectories;
using SpaceXComputer;
using KRPC.Client.Services.Impact;
using System.Numerics;
using System.IO;
using MathNet.Numerics.Providers.LinearAlgebra;
using System.Windows;
using PIDLoop;

namespace SpaceXComputer
{
    public class F9FirstStage
    {
        public Connection connection;
        public Connection connectionFirstStage;
        public RocketBody rocketBody;
        public Vessel firstStage;
        public Vessel droneShip;

        public Boolean EntryBurn = false;
        public Boolean EntryBurnDone = false;
        public Boolean FinalPhase;
    

        public void ConnectionF91stStage(Connection connectionTrue)
        {
            connection = connectionTrue;
        }

        public F9FirstStage(Vessel vessel, RocketBody rocketBody)
        {
            firstStage = vessel;
            this.rocketBody = rocketBody;
        }

        public void FHsignal(Connection connectionLink, Connection connectionFirstStageLink)
        {
            connection = connectionLink;
            connectionFirstStage = connectionFirstStageLink;
        }

        public double VHour = 21;
        public double VMinute = 45;
        public double VSecond = 30;

        protected double InitLat = 28.49289896; //LZ-1
        protected double InitLon = -80.51653638;
        //protected double InitAlt = 47.6120;

        protected double InitAlt = 49; //OCISLY

        public void F9Startup(Connection connectionLink, Connection connectionFirstStageLink)
        {
            connection = connectionLink;
            connectionFirstStage = connectionFirstStageLink;
            firstStage.AutoPilot.Engage();

            //firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceReferenceFrame;
            /*firstStage.AutoPilot.TargetDirection = Tuple.Create(1.0, 0.0, 0.0);
            Console.WriteLine(firstStage.AutoPilot.TargetDirection);
            firstStage.Control.SpeedMode = SpeedMode.Surface;
            firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
            while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed >= 0) { }
            firstStage.Control.Brakes = true;
            while (true)
            {
                firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                Console.WriteLine(firstStage.Flight(firstStage.SurfaceVelocityReferenceFrame).Retrograde);
                firstStage.AutoPilot.TargetDirection = firstStage.Flight(firstStage.SurfaceVelocityReferenceFrame).Retrograde;
                Console.WriteLine(firstStage.AutoPilot.TargetDirection);
            }*/
            
            //Thread.Sleep(999999);

            firstStage.Control.Throttle = 0;
            firstStage.AutoPilot.TargetPitchAndHeading(90, Startup.GetInstance().GetFlightInfo().getHead());

            /*for (int i = 1; i < 10; i++)
            {
                firstStage.Parts.Engines[i].Active = true;
            }*/

            bool T = false;

            while (T == false)
            {
                double hour = VHour - DateTime.Now.Hour;
                double minute = VMinute - DateTime.Now.Minute;
                double second = VSecond - DateTime.Now.Second;

                second = second + VSecond - 1;

                if (hour <= 0 && minute <= 0 && second <= 7) { T = true; }
            }

            Thread Rec = new Thread(FlightRecord);
            //Rec.Start();

            firstStage.Control.Throttle = 1;
            firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = true;
            for (int i = 0; i < 6; i++)
            {
                firstStage.Parts.WithTag("Main")[i].Engine.Active = true;
            }


            Console.WriteLine("FIRST STAGE : Main engine startup.");
            //var during = 0;
            float thrust;
            thrust = firstStage.Parts.Engines[1].Thrust;
            firstStage.Control.Throttle = 1;
            Thread.Sleep(7000);
            firstStage.Control.Throttle = 1;
            thrust = firstStage.Thrust;
            if ((thrust * 100) / firstStage.AvailableThrust < 50)
            {
                Console.WriteLine("FIRST STAGE : Engine Abort.");
                Console.WriteLine("Trust : " + thrust);
                Console.WriteLine("Available Thrust : " + firstStage.AvailableThrust);
                firstStage.Control.Throttle = 0;
                firstStage.Parts.Engines[2].Active = false;
                firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = false;
                firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = false;
                firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = false;
                for (int i = 0; i < 6; i++)
                {
                    firstStage.Parts.WithTag("Main")[i].Engine.Active = false;
                }
                firstStage.Control.Lights = false;
            }
            else if (Startup.GetInstance().GetFlightInfo().getStaticFire() == true)
            {
                Thread.Sleep(7000);
                Console.WriteLine("FIRST STAGE : Static Fire ended.");
                Console.WriteLine("Trust : " + thrust);
                Console.WriteLine("Available Thrust : " + firstStage.AvailableThrust);
                firstStage.Control.Throttle = 0;
                firstStage.Parts.Engines[2].Active = false;
                firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = false;
                firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = false;
                firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = false;
                for (int i = 0; i < 6; i++)
                {
                    firstStage.Parts.WithTag("Main")[i].Engine.Active = false;
                }
            }
            else
            {
                foreach (LaunchClamp clamp in firstStage.Parts.LaunchClamps)
                {
                    clamp.Release();
                }
                firstStage.Control.ToggleActionGroup(6);
                Console.WriteLine("FIRST STAGE : Liftoff.");
            }
        }

        public F9SecondStage secondStage;
        public bool switchToSecondStage;
        public void FlightRecord()
        {
            using (StreamWriter Record = new StreamWriter($@"C:\Users\Utilisateur\Documents\KSPRP\Flight\SpaceX\Falcon9\F9Record_{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.csv", true))
            {
                Record.WriteLine("'Time', 'Speed (m/s)', 'Altitude (m)', 'TWR', 'Pitch', 'Head', 'RealPitch', 'RealHead', 'ImpactLat', 'ImpactLong', 'LZLat', 'LZLong', 'ControlPitch', 'ControlYaw', 'DirectionPitch', 'DirectionHead'");

                while (true)
                {
                    if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed <= 0.5)
                    {
                        Thread.Sleep(250);
                        Record.WriteLine($"'{Hour(connection.SpaceCenter().UT)}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).MeanAltitude}', '{(firstStage.Thrust / firstStage.Mass) / 10}', '{firstStage.AutoPilot.TargetPitch}', '{firstStage.AutoPilot.TargetHeading}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Pitch}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Heading}', ' ', ' ', '{landingZonePosition().Item1}', '{landingZonePosition().Item2}', '{firstStage.Control.Pitch}', '{firstStage.Control.Yaw}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item1}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item2}'");
                    }
                    else
                    {
                        Thread.Sleep(250);
                        Record.WriteLine($"'{Hour(connection.SpaceCenter().UT)}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).MeanAltitude}', '{(firstStage.Thrust / firstStage.Mass) / 10}', '{firstStage.AutoPilot.TargetPitch}', '{firstStage.AutoPilot.TargetHeading}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Pitch}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Heading}', '{connection.Trajectories().ImpactPos().Item1}', '{connection.Trajectories().ImpactPos().Item2}', '{landingZonePosition().Item1}', '{landingZonePosition().Item2}', '{firstStage.Control.Pitch}', '{firstStage.Control.Yaw}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item1}', '{firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item2}'");
                    }

                    if (switchToSecondStage == true)
                    {
                        break;
                    }
                }

                while (true)
                {
                    if (secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).TrueAirSpeed <= 0.5)
                    {
                        Thread.Sleep(250);
                        Record.WriteLine($"'{Hour(connection.SpaceCenter().UT)}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).TrueAirSpeed}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).MeanAltitude}', '{(secondStage.secondStage.Thrust / secondStage.secondStage.Mass) / 10}', '{secondStage.secondStage.AutoPilot.TargetPitch}', '{secondStage.secondStage.AutoPilot.TargetHeading}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Pitch}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Heading}', ' ', ' ', ' ', ' ', '{secondStage.secondStage.Control.Pitch}', '{secondStage.secondStage.Control.Yaw}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Retrograde.Item1}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Retrograde.Item2}'");
                    }
                    else
                    {
                        Thread.Sleep(250);
                        Record.WriteLine($"'{Hour(connection.SpaceCenter().UT)}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).TrueAirSpeed}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).MeanAltitude}', '{(secondStage.secondStage.Thrust / secondStage.secondStage.Mass) / 10}', '{secondStage.secondStage.AutoPilot.TargetPitch}', '{secondStage.secondStage.AutoPilot.TargetHeading}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Pitch}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Heading}', ' ', ' ', ' ', ' ', '{secondStage.secondStage.Control.Pitch}', '{secondStage.secondStage.Control.Yaw}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Retrograde.Item1}', '{secondStage.secondStage.Flight(secondStage.secondStage.SurfaceReferenceFrame).Retrograde.Item2}'");
                    }
                }
            }
        }

        public static string Hour(double uT)
        {
            return $"{uT}";
        }

        public void boostbackStart()
        {
            //connection = new Connection(address: IPAddress.Parse("192.168.1.88"), rpcPort: 50000, streamPort: 50001);

            firstStage.Control.Throttle = 0;

            if (Startup.GetInstance().GetFlightInfo().getRocket() == "FH")
            {
                firstStage.Parts.WithTag("MainCentralSB")[0].Tag = "MainCentral";
                firstStage.Parts.WithTag("MainSecondSB")[0].Tag = "MainSecond";
                firstStage.Parts.WithTag("MainSecondSB")[0].Tag = "MainSecond";

                for (int i = 0; i < 6; i++)
                {
                    firstStage.Parts.WithTag("MainSB")[0].Tag = "Main";
                }
            }

            Console.WriteLine("Communication done with the first stage.");

            firstStage.Control.Forward = -1;
            Thread.Sleep(10000);
            firstStage.Control.Forward = 0;

            //boostbackBurn(firstStage, connectionFirstStage);
            //firstStage.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHeadDesorbitation();
            //firstStage.AutoPilot.TargetPitch = 90;
            //Thread Guidance = new Thread(NewLandingBurnGuidance);
            EntryBurn = true;
            //Guidance.Start();
            firstStage.AutoPilot.Disengage();

            NewEntryBurn(connectionFirstStage);

            //landingBurn2(connectionFirstStage);
            SuicidBurn();
        }

        public void boostbackBurn(Vessel firstStage, Connection connectionFirstStage)
        {
            //connectionFirstStage = new Connection(address: IPAddress.Parse("192.168.1.88"), rpcPort: 50000, streamPort: 50001);
            Console.WriteLine("Falcon 9 first stage accisition signal.");
            firstStage.AutoPilot.Engage();
            firstStage.AutoPilot.TargetRoll = 0;



            Thread.Sleep(4000);


            //firstStage.Parts.WithTag("RCS")[0].Engine.Active = true;
            firstStage.Control.RCS = true;
            firstStage.AutoPilot.Engage();
            
            firstStage.AutoPilot.TargetPitchAndHeading(0, Startup.GetInstance().GetFlightInfo().getHeadDesorbitation());
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    firstStage.Parts.WithTag("Main")[i].Engine.Active = false;
                }
            }
            catch
            {
                firstStage.Control.ToggleActionGroup(1);
            }


            firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = true;

            //while (firstStage.AutoPilot.RollError >= 10) { firstStage.AutoPilot.TargetRoll = 0; }
            //firstStage.AutoPilot.AttenuationAngle = 0;
            firstStage.Control.Pitch = 1;
            firstStage.Control.Up = 1;
            Thread.Sleep(15000);
            
            firstStage.Control.Up = 0;
            //while (firstStage.AutoPilot.HeadingError > 45) { }
            Thread.Sleep(10000);
            firstStage.AutoPilot.TargetRoll = 180;
            firstStage.Control.Up = 0;
            firstStage.Control.Pitch = 0;
            firstStage.Control.Forward = 1;
            Thread.Sleep(4000);
            firstStage.Control.Forward = 0;
            firstStage.Control.Throttle = 1;
            //while (firstStage.AutoPilot.PitchError > 30) { /*firstStage.Control.Pitch = 1;*/ }
            /*firstStage.Control.Pitch = 0;
            firstStage.Control.Up = 0;
            while (firstStage.AutoPilot.HeadingError > 17) { }*/
            firstStage.AutoPilot.Disengage();
            firstStage.AutoPilot.Engage();

            bool impact = connection.Trajectories().HasImpact();
            double latLand = firstStage.connection.Trajectories().ImpactPos().Item1;
            firstStage.AutoPilot.TargetPitchAndHeading(5, Startup.GetInstance().GetFlightInfo().getHeadDesorbitation() - (Convert.ToSingle(latLand) - Convert.ToSingle(landingZonePosition().Item1)));

            Console.WriteLine("FIRST STAGE : Boostback burn started. " + rocketBody);
            firstStage.Control.Throttle = 1;
            firstStage.Control.RCS = false;

            while (connection.Trajectories().ImpactPos().Item2 > landingZonePosition().Item2)
            {
                /*var IP = connection.Trajectories().ImpactPos().Item1;
                float DesorbitHead = Startup.GetInstance().GetFlightInfo().getHeadDesorbitation();

                var Head = (DesorbitHead * landingZonePosition().Item1) / IP;
                float Head2 = Convert.ToSingle(Head);

                firstStage.AutoPilot.TargetHeading = Head2;*/

                //firstStage.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHeadDesorbitation() - ((Convert.ToSingle(connection.Trajectories().ImpactPos().Item1) - Convert.ToSingle(landingZonePosition().Item1)) * 100);
                firstStage.Control.Throttle = 1;
            }

            firstStage.Control.Throttle = 0;
            Console.WriteLine("FIRST STAGE : Boostback burn shutdown.");
            firstStage.Control.RCS = false;
            firstStage.Control.Brakes = true;

            //firstStage.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHeadDesorbitation();
            //firstStage.AutoPilot.TargetPitch = 90;
        }

        public void NewEntryBurn(Connection connectionFirstStage)
        {
            //firstStage.AutoPilot.Engage();
            firstStage.Control.Brakes = true;
            firstStage.Control.RCS = true;
            firstStage.Control.SpeedMode = SpeedMode.Surface;
            //firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, 0.0, -1.0); //Radial, Normal, Prograde

            //Thread.Sleep(30000);
            firstStage.Control.RCS = true;

            /*while(firstStage.Flight(firstStage.SurfaceReferenceFrame).Pitch <= 80) { }
            firstStage.Control.RCS = true;

            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).Pitch <= 89) { }
            firstStage.Control.RCS = false;*/

            while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > 0) { }

            firstStage.Control.RCS = true;

            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).MeanAltitude >= 60000/* && firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed <= 2200*/) { } //55000 RTLS | 60000 ASDS

            try
            {
                for (int i = 0; i < 6; i++)
                {
                    firstStage.Parts.WithTag("Main")[i].Engine.Active = false;
                }
            }
            catch
            {
                firstStage.Control.ToggleActionGroup(1);
            }


            firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = true;

            firstStage.Control.RCS = false;
            firstStage.Control.Throttle = 1;
            Console.WriteLine("FIRST STAGE : Entry Burn Started.");

            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed >= 350)
            {
                /*if (firstStage.Flight(firstStage.SurfaceReferenceFrame).HorizontalSpeed >= 150)
                {
                    firstStage.Control.SASMode = SASMode.Retrograde;
                    while (firstStage.Control.SASMode != SASMode.Retrograde) { firstStage.Control.SASMode = SASMode.Retrograde; }
                }
                else
                {
                    firstStage.Control.SASMode = SASMode.AntiRadial;
                    while (firstStage.Control.SASMode != SASMode.AntiRadial) { firstStage.Control.SASMode = SASMode.AntiRadial; }
                }*/
            }

            Console.WriteLine("FIRST STAGE : Entry Burn Shutdown.");
            firstStage.Control.RCS = true;
            firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = true;
            firstStage.Control.Throttle = 0;

            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = false;
            firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = false;
        }

        public void SuicidBurn()
        {
            bool suicideBurnText = false;
            float throt = ThrottleToTWR(0.0f);
            bool suicideBurn = false;

            double landedAltitude = InitAlt;

            while (true)
            {
                if (suicideBurnText == false)
                {
                    throt = ThrottleToTWR(0.0f);
                }

                double verticalSpeed = firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed;
                if (verticalSpeed < -450)
                {
                    verticalSpeed = -450;
                }

                double trueRadar = firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude - landedAltitude + 15;

                double g = 9.81;
                double maxDecelCentral = ((firstStage.Parts.WithTag("MainCentral")[0].Engine.AvailableThrust) / firstStage.Mass) - g;
                double stopDistCentral = Math.Pow(Math.Abs(verticalSpeed), 2) / (2 * maxDecelCentral);
                double impactTime = trueRadar / Math.Abs(verticalSpeed);

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ - (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed * 1.75) <= stopDistCentral && suicideBurnText == false)
                {
                    Console.WriteLine("FIRST STAGE : Landing Burn started.");
                    suicideBurnText = true;

                    throt = 1;
                }

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ <= stopDistCentral && suicideBurnText == true)
                {
                    throt = 1;
                }

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ > stopDistCentral && suicideBurnText == true)
                {
                    throt = ThrottleToTWR(0.90f);
                }

                if (Math.Abs(impactTime) < 4)
                {
                    firstStage.Control.Gear = true;
                }

                if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > -5 && suicideBurnText == true)
                {
                    throt = ThrottleToTWR(0.85f);
                }

                if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > -0.1 && suicideBurn == false)
                {
                    firstStage.Control.Throttle = 0;
                    throt = 0;
                    Console.WriteLine("firstStage has landed.");
                    suicideBurn = true;

                    double LandLat = firstStage.Flight(firstStage.SurfaceReferenceFrame).Latitude;
                    double LandLon = firstStage.Flight(firstStage.SurfaceReferenceFrame).Longitude;

                    Console.WriteLine($"Land Lat = {LandLat} | Land Long = {LandLon}");
                    Console.WriteLine($"Distance = {Distance(InitLat, LandLat, InitLon, LandLon)}");

                    break;
                }

                if (throt <= 0.001f && suicideBurnText == true)
                {
                    throt = 0.01f;
                }

                firstStage.Control.Throttle = throt;
            }
        }

        public double Distance(double lat1, double lat2, double lon1, double lon2/*, double el1, double el2*/) //Lat1 & 2 and Lon1 & 2 = Pose Initial and Final. Eli1 & 2 = Altitude Init and Final.
        {

            int R = 6371; // Radius of the earth

            double latDistance = ToRadians(lat2 - lat1);
            double lonDistance = ToRadians(lon2 - lon1);
            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c * 1000; // convert to meters

            /*double height = el1 - el2;*/

            distance = Math.Pow(distance, 2)/* + Math.Pow(height, 2)*/;

            return Math.Sqrt(distance);
        }
        public double ToRadians(double val)
        {
            return (Math.PI / 180) * val;
        }

        public float ThrottleToTWR(float twr)
        {
            float Mass = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
            //Console.WriteLine(Mass);
            float T = twr * Mass;
            //Console.WriteLine(T);
            float Throttle = (T - 306125) / (firstStage.AvailableThrust - 306125);
            //Console.WriteLine(Throttle);
            return Throttle;
        }

        public void NewLandingBurnGuidance()
        {
            firstStage.AutoPilot.Disengage();
            firstStage.Control.Brakes = true;
            //firstStage.Control.SAS = true;
            //firstStage.Control.SASMode = SASMode.Retrograde;
            //while (firstStage.Control.SASMode != SASMode.Retrograde) { firstStage.Control.SASMode = SASMode.Retrograde; }
            firstStage.Control.RCS = true;

            while (EntryBurn == true)
            {
                //firstStage.Control.SASMode = SASMode.Retrograde;
                //while (firstStage.Control.SASMode != SASMode.Retrograde) { firstStage.Control.SASMode = SASMode.Retrograde; }
            }

            for (int i = 0; i < 6; i++)
            {
                firstStage.Parts.WithTag("Main")[i].Engine.Active = false;
            }


            firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = true;
            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = false;
            firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = false;
        }

        public void entryBurn2(Connection connectionFirstStage)
        {
            /*var target_position = droneShip.Position(droneShip.Orbit.Body.ReferenceFrame);
            target_position = (connection.SpaceCenter().TransformDirection(target_position, droneShip.Orbit.Body.ReferenceFrame, droneShip.SurfaceReferenceFrame));
            //custom value for target position to ensure a trajectory that intercepts the target tail on instead of severe angle;
            target_position[0] += 10000.0 - 10000.0 * numpy.exp(-1.0 * vessel.flight(vessel.orbit.body.reference_frame).mean_altitude / 30000);
            target_position = (target_position[0], target_position[1], target_position[2]);
            target_position = conn.space_center.transform_direction(target_position, target.surface_reference_frame, target.orbit.body.reference_frame);
            target_position = numpy.asarray(target_position);

            vessel_position = numpy.asarray(vessel.position(vessel.orbit.body.reference_frame));
            target_position = numpy.add(target_position, numpy.multiply(vessel_position, -1.0));
            target_position = (target_position[0], target_position[1], target_position[2]);
            target_position = conn.space_center.transform_direction(target_position, target.orbit.body.reference_frame, vessel.reference_frame);
            target_position = numpy.asarray(target_position);*/

            for (int i = 0; i < 6; i++)
            {
                firstStage.Parts.WithTag("Main")[i].Engine.Active = false;
            }

            firstStage.AutoPilot.Engage();
            EntryBurn = true;

            /*var body_ref_frame = firstStage.Orbit.Body.NonRotatingReferenceFrame;
            var angvel = firstStage.AngularVelocity(body_ref_frame);

            firstStage.AutoPilot.StoppingTime = Tuple.Create(0.1, 0.1, 0.1);
            firstStage.AutoPilot.DecelerationTime = Tuple.Create(1.0, 1.0, 1.0);

            firstStage.AutoPilot.TimeToPeak = Tuple.Create(1.0,1.0,1.0);
            firstStage.AutoPilot.Overshoot = Tuple.Create(0.1, 0.1, 0.1);*/

            firstStage.Control.RCS = true;
            firstStage.AutoPilot.TargetPitch = 90;
            firstStage.Control.Pitch = -1;
            firstStage.Control.Up = -1;
            firstStage.Control.SpeedMode = SpeedMode.Surface;
            Thread.Sleep(5000);
            firstStage.Control.RCS = false;

            while (firstStage.AutoPilot.PitchError > 20)
            {
                firstStage.Control.Pitch = 0;
                firstStage.Control.Up = 0;
            }
            firstStage.Control.Up = 0;
            firstStage.AutoPilot.Disengage();
            firstStage.AutoPilot.Engage();
            firstStage.Control.RCS = true;
            firstStage.AutoPilot.TargetPitch = 90;

            /* body_ref_frame = firstStage.Orbit.Body.NonRotatingReferenceFrame;
             angvel = firstStage.AngularVelocity(body_ref_frame);

             firstStage.AutoPilot.StoppingTime = Tuple.Create(0.1, 0.1, 0.1);
             firstStage.AutoPilot.DecelerationTime = Tuple.Create(1.0, 1.0, 1.0);

             firstStage.AutoPilot.TimeToPeak = Tuple.Create(0.1, 1.0, 0.1);
             firstStage.AutoPilot.Overshoot = Tuple.Create(0.1, 0.1, 0.1);

             firstStage.AutoPilot.AttenuationAngle = Tuple.Create(1.0, 180.0, 1.0);*/


            //var CreateRelative = ReferenceFrame.CreateRelative(connection, firstStage.SurfaceReferenceFrame, (firstStage.Flight().Retrograde), null, null);
            firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;

            var difPosLat = connection.Impact().GetImpactPos(firstStage).Item1 - landingZonePosition().Item1;
            var difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2;
            var difPos = Math.Abs(difPosLat) + Math.Abs(difPosLong);

            /*difPosLat = connection.Trajectories().ImpactPos().Item1 - positionOCISLY().Item1;
            if (connection.Trajectories().ImpactPos().Item1 > positionOCISLY().Item1)
            {
                difPosLong = connection.Trajectories().ImpactPos().Item2 - positionOCISLY().Item2;
                if (connection.Trajectories().ImpactPos().Item2 > landingZonePosition().Item2)
                {
                    firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create(-0.2, -1.0, 0.1);
                }
                else if (connection.Trajectories().ImpactPos().Item2 < landingZonePosition().Item2)
                {
                    firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create(0.2, -1.0, 0.1);
                }
            }
            else if (connection.Trajectories().ImpactPos().Item1 < positionOCISLY().Item1)
            {
                double retrograde = (firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item3) + 10;

                difPosLong = connection.Trajectories().ImpactPos().Item2 - positionOCISLY().Item2;
                if (connection.Trajectories().ImpactPos().Item2 > landingZonePosition().Item2)
                {
                    firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create(-0.2, -1.0, -0.1);
                }
                else if (connection.Trajectories().ImpactPos().Item2 < landingZonePosition().Item2)
                {
                    firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create(0.2, -1.0, -0.1);
                }
            }*/

            while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > 0) { }
            while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude > 130000) { }

            firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
            firstStage.Control.RCS = true;

            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed < 800) { firstStage.Control.RCS = true; }

            while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude > 50000) { }

            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 1100) { }

            firstStage.Control.RCS = false;
            firstStage.Control.Throttle = 1;
            Console.WriteLine("FIRST STAGE : Entry burn started.");


            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 350)
            {
                /*difPosLat = connection.Trajectories().ImpactPos().Item1 - positionOCISLY().Item1;

                difPosLat = connection.Trajectories().ImpactPos().Item1 - positionOCISLY().Item1;
                if (connection.Trajectories().ImpactPos().Item1 > landingZonePosition().Item1)
                {
                    difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2;
                    if (connection.Trajectories().ImpactPos().Item2 > landingZonePosition().Item2)
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(-0.2, -1.0, 0.2);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 < landingZonePosition().Item2)
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.2, -1.0, 0.2);
                    }
                }
                else if (connection.Trajectories().ImpactPos().Item1 < landingZonePosition().Item1)
                {
                    double retrograde = (firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item3) + 10;

                    difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2;
                    if (connection.Trajectories().ImpactPos().Item2 > landingZonePosition().Item2)
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(-0.2, -1.0, -0.2);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 < landingZonePosition().Item2)
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.OrbitalReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.2, -1.0, -0.2);
                    }
                }*/
            }

            firstStage.Control.Throttle = 0;
            EntryBurnDone = true;
            Console.WriteLine("FIRST STAGE : Entry burn shutdown.");
            EntryBurn = false;
        }

        public void landingBurn()
        {
            firstStage.AutoPilot.Engage();
            firstStage.AutoPilot.TargetPitch = 90;
            while (true)
            {
                impactPoint();
                var Ft = firstStage.AvailableThrust;
                var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                var TWR = Ft / Fw;
                var thrust = Math.Abs(firstStage.AvailableThrust);
                var mass = firstStage.Mass;
                var gravitySurface = 9.813352;
                var verticalSpeed = Math.Abs(firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed);
                var at = (thrust / (mass * gravitySurface));
                var burnAltitude = (Math.Pow(verticalSpeed, at) / (1 * Math.Abs(at))) / 1.20 * TWR;

                if (firstStage.Flight(null).SurfaceAltitude <= Math.Abs(burnAltitude) || firstStage.Flight(null).MeanAltitude <= Math.Abs(burnAltitude))
                {
                    break;
                }
            }

            firstStage.Control.Throttle = 1;
            firstStage.Parts.WithTag("MainCentral")[0].Engine.Active = false;
            float newThrottle = 0;
            while (true)
            {
                impactPoint();
                var Ft = firstStage.AvailableThrust;
                var trueFT = firstStage.Thrust;
                var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                var TWR = Ft / Fw;
                var trueTWR = trueFT / Fw;

                var thrust = Math.Abs(firstStage.AvailableThrust);
                var mass = firstStage.Mass;
                var gravitySurface = 9.813352;
                var verticalSpeed = Math.Abs(firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed);
                var at = (thrust / (mass * gravitySurface));
                var burnAltitude = (Math.Pow(verticalSpeed, at) / (1 * Math.Abs(at))) / 1.2 * TWR;

                if (trueTWR < 1 && (burnAltitude > firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude || burnAltitude > firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude))
                {
                    firstStage.Control.Throttle = firstStage.Control.Throttle + 0.01f;
                }
                if (trueTWR > 1 && (burnAltitude < firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude || burnAltitude < firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude))
                {
                    firstStage.Control.Throttle = firstStage.Control.Throttle - 0.01f;
                }

                if ((burnAltitude > firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude || burnAltitude > firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude))
                {
                    firstStage.Control.Throttle = firstStage.Control.Throttle + 0.05f;
                }
                if ((burnAltitude < firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude || burnAltitude < firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude))
                {
                    firstStage.Control.Throttle = firstStage.Control.Throttle - 0.05f;
                }

                if (trueTWR < 1.2 && trueTWR > 1)
                {
                    firstStage.Control.Throttle = firstStage.Control.Throttle + 0.01f;
                }
                if (trueTWR < 0.9)
                {
                    firstStage.Control.Throttle = firstStage.Control.Throttle + 0.01f;
                }

                /*if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude < 1000 || firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude < 1000)
                {
                    break;
                }*/

                if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > -1)
                {
                    break;
                }
            }

            /*while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed < -1)
            {
                var Ft = firstStage.AvailableThrust;
                var trueFT = firstStage.Thrust;
                var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                var TWR = Ft / Fw;
                var trueTWR = trueFT / Fw;

                if (firstStage.Control.Throttle < 0.1) { firstStage.Control.Throttle = 0.2f; }

                if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed < -6)
                {
                    newThrottle = (firstStage.Control.Throttle * 1.4f) / TWR;
                    firstStage.Control.Throttle = newThrottle;
                }
                else if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > -6)
                {
                    newThrottle = (firstStage.Control.Throttle * 0.8f) / TWR;
                    firstStage.Control.Throttle = newThrottle;
                }
            }*/
            try
            {
                firstStage.Parts.Engines[0].Active = false;
                firstStage.Control.Throttle = 0;
                Console.WriteLine("FIRST STAGE : Falcon has landed.");
            }
            catch
            { }
        }

        public void landingBurn2(Connection connectionFirstStage)
        {
            firstStage.Control.RCS = true;
            firstStage.Control.Brakes = true;
            var suicideBurnText = false;
            var suicideBurn = false;
            //firstStage.Control.SASMode = SASMode.Retrograde;

            firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = false;
            firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = false;
            //firstStage.Parts.Engines[8].Active = true;
            //firstStage.Control.SASMode = SASMode.Retrograde;
            float throt = 0;

            bool threeEngine = false;
            bool finalPhase = false;

            //Console.WriteLine(firstStage.Orbit.Body.SurfaceGravity);
            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).MeanAltitude > 15000 || firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude > 15000)
            {
                
            }

            while (true)
            {
                var landedAltitude = 20;
                var verticalSpeed = firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed;
                if (verticalSpeed < -350)
                {
                    verticalSpeed = -350;
                }
                var trueRadar = firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude - landedAltitude * 2;
                //var g = firstStage.Orbit.Body.SurfaceGravity;
                var g = 9.81;
                var maxDecelCentral = ((firstStage.Parts.WithTag("MainCentral")[0].Engine.AvailableThrust) / firstStage.Mass) - g;
                var stopDistCentral = Math.Pow(Math.Abs(verticalSpeed), 2) / (2 * maxDecelCentral);
                var maxDecelThree = ((firstStage.Parts.WithTag("MainSecond")[0].Engine.AvailableThrust * 3) / firstStage.Mass) - g;
                var stopDistThree = Math.Pow(Math.Abs(verticalSpeed), 2) / (2 * maxDecelThree);
                var impactTime = trueRadar / Math.Abs(verticalSpeed);
                //Console.WriteLine("Central : " + stopDistCentral);
                //Console.WriteLine("Three : " + stopDistThree);

                var Ft = firstStage.AvailableThrust;
                var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                var TWR = Ft / Fw;

                if (trueRadar <= stopDistCentral)
                {
                    if (suicideBurnText == false)
                    {
                        Console.WriteLine("FIRST STAGE : Landing Burn started.");
                        suicideBurnText = true;
                        firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = false;
                        firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = false;
                        throt = 1;
                    }
                }

                if (Math.Abs(impactTime) < 6)
                {
                    firstStage.Control.Gear = true;
                }

                if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed < 55)
                {
                    if (finalPhase == false)
                    {
                        Console.WriteLine("FIRST STAGE : Final phase started.");
                        finalPhase = true;
                        FinalPhase = true;
                    }
                    //firstStage.Control.SASMode = SASMode.Radial;
                    /*firstStage.Parts.Engines[1].Active = false;
                    firstStage.Parts.Engines[2].Active = false;
                    */
                    threeEngine = false;
                    Ft = firstStage.AvailableThrust;
                    Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                    TWR = Ft / Fw;
                    //firstStage.AutoPilot.TargetPitchAndHeading(90, 0);
                    if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed >= -5)
                    {
                        throt = 0.98f / TWR;
                    }
                    /*firstStage.AutoPilot.Disengage();
                    firstStage.Control.SAS = true;
                    firstStage.Control.SASMode = SASMode.Radial;*/
                    firstStage.AutoPilot.TargetPitch = 90;
                }
                if (Math.Abs(stopDistCentral) <= trueRadar)
                {
                    threeEngine = false;
                    if (throt > 1) { throt = 1; }
                    throt = Convert.ToSingle(stopDistCentral / trueRadar);
                }
                else if (Math.Abs(stopDistCentral) < trueRadar && stopDistThree <= trueRadar)
                {
                    throt = Convert.ToSingle(stopDistThree / trueRadar);
                    threeEngine = true;
                }
                else if (stopDistThree > trueRadar && finalPhase == false)
                {
                    threeEngine = false;

                    /*if (Math.Abs(stopDistThree) <= trueRadar-200)
                    {
                        if (throt > 1) { throt = 1; }
                        throt = Convert.ToSingle(stopDistCentral / trueRadar);
                    }
                    else if (Math.Abs(stopDistThree) > trueRadar - 200)
                    {
                        throt = Convert.ToSingle(stopDistThree / trueRadar);
                    }*/
                }
                if (stopDistThree > trueRadar)
                {
                    throt = Convert.ToSingle(stopDistCentral / trueRadar);
                }
                else if (stopDistCentral > trueRadar)
                {
                    throt = Convert.ToSingle(stopDistCentral / trueRadar);
                }
                

                if (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed > 0.01 && suicideBurn == false)
                {
                    firstStage.Control.Throttle = 0;
                    throt = 0;
                    Console.WriteLine("FIRST STAGE : Falcon has landed.");
                    var difPosLat1 = firstStage.Flight(firstStage.SurfaceReferenceFrame).Latitude - landingZonePosition().Item1;
                    var difPosLong1 = firstStage.Flight(firstStage.SurfaceReferenceFrame).Longitude - (landingZonePosition().Item2);
                    Console.WriteLine("DifPoseLat = " + difPosLat1);
                    Console.WriteLine("DifPoseLong = " + difPosLong1);
                    suicideBurn = true;

                    break;
                }

                if (throt == 0)
                {
                    throt = 0.01f;
                }

                firstStage.Control.Throttle = throt;
                firstStage.Parts.WithTag("MainSecond")[0].Engine.Active = threeEngine;
                firstStage.Parts.WithTag("MainSecond")[1].Engine.Active = threeEngine;
                if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 70 && (firstStage.Flight(firstStage.SurfaceReferenceFrame).MeanAltitude < 400 || firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 70 && firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude < 400))
                {
                    Console.WriteLine("FIRST STAGE : First stage landing failed.");
                    break;
                }

                else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed <= 50)
                {
                    firstStage.AutoPilot.TargetHeading = 0;
                    firstStage.AutoPilot.TargetPitch = 90;
                }
            }
        }

        public Tuple<Double, Double> impactPoint()
        {
            try
            {
                var refFrame = firstStage.SurfaceReferenceFrame;
                var Flight = firstStage.Flight(refFrame);
                double radius = firstStage.Orbit.Body.EquatorialRadius + Flight.MeanAltitude;
                double TA = firstStage.Orbit.TrueAnomalyAtRadius(radius);
                TA = -1 * TA;
                double impactTime = firstStage.Orbit.UTAtTrueAnomaly(TA);
                Tuple<Double, Double, Double> impactPointRef = firstStage.Orbit.PositionAt(impactTime, firstStage.SurfaceReferenceFrame);

                double Longitude = (firstStage.Flight(refFrame).Longitude - impactPointRef.Item1);
                double Latitude = firstStage.Flight(refFrame).Latitude - impactPointRef.Item2;

                Tuple<Double, Double> impactPoint;
                impactPoint = Tuple.Create<Double, Double>(Longitude, Latitude);
                Console.WriteLine("Longitude : " + (impactPoint.Item1) + " Latitude : " + (27.3948765184433 - impactPoint.Item2));
                return impactPoint;
            }
            catch (Exception e)
            {
            }
            return null;
        }

        /*public Tuple<Double, Double> positionOCISLY()
        {
                var refFrame = droneShip.SurfaceReferenceFrame;
                double Longitude = droneShip.Flight(refFrame).Longitude;
                double Latitude = droneShip.Flight(refFrame).Latitude;

                Tuple<Double, Double> positionOCISLY = Tuple.Create<Double, Double>(Latitude, Longitude);
                return positionOCISLY;
        }*/

            public void NewGuidance()
        {
            while (true)
            {

            }
        }

        public void guidanceStart()
        {
            GuidanceSystemPerfect(firstStage, connectionFirstStage);
            //ControlFirstStage();
        }

        public void ControlFirstStage()
        {
            while(EntryBurnDone == false) { }
            Thread Roll = new Thread(ControlRoll);
            Roll.Start();
            StageControl();
        }

        public Tuple<double, double> DifImpactPos(Vessel firstStage, Connection connection)
        {
            double difPosLat = landingZonePosition().Item1 - connection.Trajectories().ImpactPos().Item1;
            double  difPosLong = landingZonePosition().Item2 - connection.Trajectories().ImpactPos().Item2;

            return Tuple.Create(difPosLat, difPosLong);
        }

        public void ControlRoll()
        {
            while (true)
            {
                double Roll = firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).Roll;
                double Heading = firstStage.Flight(firstStage.SurfaceReferenceFrame).Heading;

                double TargetRoll = Roll - (Heading - 90);

                //double DifRoll = Roll - Heading;

                if (Heading - 90 == 0)
                {
                    TargetRoll = -Roll;
                }

                firstStage.Control.Roll = Convert.ToSingle(TargetRoll);
            }
        }

        public double TargetHeading()
        {
            double Lat = DifImpactPos(firstStage, connection).Item1;
            double Long = DifImpactPos(firstStage, connection).Item2;
            double Head = 0;

            if (Lat < 0)
            {
                double LongPercent = (Math.Abs(Long) * 100) / Lat;
                Head = 0 + (LongPercent * 100) / 90;
                if (Head < 0)
                {
                    Head = 360 + Head;
                }
            }
            else if (Lat >= 0)
            {
                double LongPercent = (Math.Abs(Long) * 100) / Lat;
                Head = 180 + (LongPercent * 100) / 90;
            }

            return Head;
        }

        public double TargetPitch()
        {
            double Lat = DifImpactPos(firstStage, connection).Item1 * 10000;
            double Long = DifImpactPos(firstStage, connection).Item2 * 10000;

            double difPos = Math.Abs(Lat) + Math.Abs(Long);

            return 90 - difPos;
        }

        public void StageControl()
        {
            while(true)
            {
                double TargetHead = TargetHeading();
                double TargPitch = TargetPitch();

                double Head = firstStage.Flight(firstStage.SurfaceReferenceFrame).Heading;
                double Pitch = firstStage.Flight(firstStage.SurfaceReferenceFrame).Pitch;

                double difHead = Head - TargetHead * 2;
                double difPitch = Pitch - TargPitch * 2;
                double difRotate = difHead + difPitch;

                double CosinusHead = Math.Cos(TargetHead);
                double SinusHead = Math.Sin(TargetHead);

                double CosinusDifHead = Math.Cos(difHead);
                double SinusDifHead = Math.Sin(difHead);

                double CosHead = CosinusHead - CosinusDifHead;
                double SinHead = SinusHead - SinusDifHead;

                double GoPitch = SinHead * 180 / Math.PI;
                double GoYaw = SinHead * 180 / Math.PI;

                firstStage.Control.Pitch = Convert.ToSingle(GoPitch/* + TargetPitch()*/);
                firstStage.Control.Yaw = Convert.ToSingle(GoYaw/* + TargetPitch()*/);
            }
        }

        public struct Vector2
        {
            public double X { get; set; }
            public double Z { get; set; }

            public Vector2(double x, double z)
            {
                this.X = x;
                this.Z = z;
            }

            public double Magnitude
            {
                get
                {
                    return Math.Sqrt(this.X * this.X + this.Z * this.Z);
                }
            }

            public static double CrossProduct(Vector v1, Vector v2)
            {
                return (v1.X * v2.Z) - (v1.Z * v2.X);
            }
        }

        public struct Vector
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Vector(double x, double y, double z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }
            
            public double Magnitude
            {
                get
                {
                    return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
                }
            }
                        
            public Vector Normalized
            {
                get
                {
                    return this / Magnitude;
                }
            }

            public static Vector operator /(Vector v, double n)
            {
                return new Vector(v.X / n, v.Y / n, v.Z / n);
            }
            public static Vector operator *(Vector v, double n)
            {
                return new Vector(v.X * n, v.Y * n, v.Z * n);
            }
            public static implicit operator Vector(Tuple<double, double, double> t)
            {
                return new Vector(t.Item1, t.Item2, t.Item3);
            }
            public static implicit operator Tuple<double, double, double>(Vector v)
            {
                return Tuple.Create(v.X, v.Y, v.Z);
            }

            public static double CrossProduct(Vector v1, Vector v2)
            {
                return (v1.X * v2.Y) - (v1.Y * v2.X);
            }
        }

        public void ThePerfectLandingsManeuver(Vessel firstStage, Connection connectionFirstStage)
        {
            List<Boolean> q = new List<Boolean>();
            List<Boolean> w = new List<Boolean>();
            List<Vector> e = new List<Vector>();
            List<Vector> r = new List<Vector>();
            List<Double> smode1 = new List<Double>();

            var i = false;
            var o = false;

            Vector Dir = new Vector(0, -1, 0);
            Dir = connectionFirstStage.SpaceCenter().TransformDirection(Dir, firstStage.SurfaceVelocityReferenceFrame, firstStage.SurfaceReferenceFrame);
            Dir.X = 0;
            Dir *= 1.0 / Dir.Magnitude;

            var mode = 3;
            var smode = 0;

            q.Insert(0, i);
            w.Insert(0, o);
            e.Insert(0, Dir);
            smode1.Insert(0, smode);
        }

        public void SteeringLogic(Vessel firstStage, Connection connectionFirstStage)
        {
            //var altitude = connectionFirstStage.AddStream(() => firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude);
            //var vspeed = connectionFirstStage.AddStream(() => firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed);

            Vector SeekDir = new Vector(0.0, Math.PI * 0.5, 0.0);
            Vector CurrentDir = new Vector(0.0, 0.0, 0.0);
            Vector CurrentRotVel = new Vector(0.0, 0.0, 0.0);

            var RotKp = 1.0;
            var RotKi = 0.1;
            var RotKd = 7.0;

            Vector Max = new Vector(0.0, 0.0, 0.0);
            Max.X = 10.0 * Math.PI / 180.0;
            Max.Y = 10.0 * Math.PI / 180.0;
            Max.Z = 10.0 * Math.PI / 180.0;
            Vector Min = Max * (-1.0);

            var RotPitch = new PID(0.0, Time.time, 0.0, RotKp, RotKi, RotKd, 0.0, Min.X, Max.X);
            var RotHeading = new PID(0.0, Time.time, 0.0, RotKp, RotKi, RotKd, 0.0, Min.Y, Max.Y);
            var RotRoll = new PID(0.0, Time.time, 0.0, RotKp, RotKi, RotKd, 0.0, Min.Z, Max.Z);

            Vector Fore = new Vector(0, 1, 0);
            Vector Top = new Vector(0, 0, -1);
            Vector Star = new Vector(1, 0, 0);

            var TorTu = 8.0;
            var TorKu = 5.0;
            Vector TorKi = new Vector(0.0, 0.0, 0.0);
            Vector TorKp = new Vector(0.0, 0.0, 0.0);
            Vector TorKd = new Vector(0.0, 0.0, 0.0);
            TorKp.X = 0.45 * TorKu;
            TorKp.Y = 0.45 * TorKu;
            TorKp.Z = 0.45 * TorKu;
            TorKd = TorKp * 10.0;

            double r2d(double x)
            {
                return x * 180.0 / Math.PI;
            }
            double d2r(double x)
            {
                return x * Math.PI / 180.0;
            }
            double simp(double x)
            {
                return Math.Round(x, 2);
            }
            double vang(Vector v1, Vector v2)
            {
                return Math.Acos(Vector.CrossProduct(v1, v2));
            }
            /*Vector vexclude(Vector v1,Vector v2)
            {
                Vector CrossProd = Tuple.Create(Vector.CrossProduct(v1, v2),0.0,0.0);
                Vector n = Vector.CrossProduct(Vector.CrossProduct(v1, v2) / CrossProd.Magnitude, v1);
            return n/;
            }*/

            //var h = altitude + (firstStage.Parts.WithTag("MainCenter")[0].Position(firstStage.ReferenceFrame)).Item2;

            Vector SeekDir1 = Tuple.Create(0.0, 1.0, 0.0);

            Vector Toggle(bool inQ, Vector inE, Vector inMode)
            {
                while (true)
                {
                    double altitude = firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude;
                    double vspeed = firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).VerticalSpeed;

                    bool i = inQ;
                    double sign = vspeed / Math.Abs(vspeed);

                    SeekDir1.X = inE.X;
                    double mode = inMode.X;

                    if (i == true)
                    {
                        firstStage.Control.SAS = false;

                        if (mode == 1)
                        {
                            SeekDir1 = connection.SpaceCenter().TransformDirection(SeekDir1, firstStage.SurfaceVelocityReferenceFrame, firstStage.SurfaceVelocityReferenceFrame);
                            TorKu = 5.0;
                            TorKi = Tuple.Create(0.0, 0.0, 0.0);
                            TorKp = Tuple.Create(0.0, 0.0, 0.0);
                            TorKd = Tuple.Create(0.0, 0.0, 0.0);
                            TorKp.X = 0.45 * TorKu;
                            TorKp.Y = 0.45 * TorKu;
                            TorKp.Z = 0.45 * TorKu;
                            TorKd = TorKp * 10.0;
                        }
                        else
                        {
                            TorKu = 5.0;
                            TorKi = Tuple.Create(0.0, 0.0, 0.0);
                            TorKp = Tuple.Create(0.0, 0.0, 0.0);
                            TorKd = Tuple.Create(0.0, 0.0, 0.0);
                            TorKp.X = 0.45 * TorKu;
                            TorKp.Y = 0.45 * TorKu;
                            TorKp.Z = 0.45 * TorKu * 0.25 * 40;
                            TorKd = TorKp * 10.0;
                        }

                        SeekDir.X = 0.5 * Math.PI - Math.Acos(SeekDir1.X);
                        SeekDir.Y = Math.Atan2(SeekDir1.Z, SeekDir1.X);

                        if ((SeekDir.Y * 180.0 / Math.PI) < 0.0)
                        {
                            SeekDir.Y += 2.0 * Math.PI;
                        }
                        else
                        {
                            SeekDir.Y += 0.0;
                        }
                        SeekDir.Z = 0.0;

                        Vector SeekDir1Top = Tuple.Create(0.0, 0.0, 0.0);
                        SeekDir1Top.X = Math.Cos(-SeekDir.X);

                        if ((SeekDir.X * 180.0 / Math.PI) <= 0.0)
                        {
                            SeekDir1Top.Y = Math.Sin(SeekDir.X) * Math.Cos(SeekDir.Y);
                            SeekDir1Top.Z = Math.Sin(SeekDir.X) * Math.Sin(SeekDir.Y);
                        }
                        else
                        {
                            SeekDir1Top.Y = -Math.Sin(SeekDir.X) * Math.Cos(SeekDir.Y);
                            SeekDir1Top.Z = -Math.Sin(SeekDir.X) * Math.Cos(SeekDir.Y);
                        }

                        SeekDir1 = connection.SpaceCenter().TransformDirection(SeekDir1, firstStage.SurfaceReferenceFrame, firstStage.ReferenceFrame);
                        SeekDir1Top = connection.SpaceCenter().TransformDirection(SeekDir1Top, firstStage.SurfaceReferenceFrame, firstStage.ReferenceFrame);

                        var CurrentDirMag = vang(Fore, SeekDir1);

                        CurrentDir = Tuple.Create(0.0, 0.0, 0.0);
                        //CurrentDir.X = vang(Fore, )
                    }
                }
            }
        }

        public float anglePitchLZ()
        {
            double altStage = firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude;
            if (altStage < 0) { altStage = firstStage.Flight(firstStage.SurfaceReferenceFrame).MeanAltitude; }

            var Ft = firstStage.AvailableThrust;
            var trueFT = firstStage.Thrust;
            var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var trueTWR = trueFT / Fw;

            var body = firstStage.Orbit.Body;

            double difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
            double difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2;

            float difPos = Convert.ToSingle(difPosLat + difPosLong);

            return difPos * 10;
        }

        public float angleHeadLZ()
        {
            double LZlat = landingZonePosition().Item1;
            double LZlong = landingZonePosition().Item2;

            double StageLat = firstStage.Flight(firstStage.SurfaceReferenceFrame).Latitude;
            double StageLong = firstStage.Flight(firstStage.SurfaceReferenceFrame).Longitude;

            var difPosLat = LZlat - StageLat;
            var difPosLong = LZlong - StageLong;

            var difPos = difPosLat + difPosLong;
            var latInflu = (difPosLat * 100) / difPos;
            var longInflu = (difPosLong * 100) / difPos;

            float latHead;
            float longHead;

            if (difPosLat >= 0) { latHead = 0; } else { latHead = 180; }
            if (difPosLong >= 0) { longHead = 90; } else { longHead = 270; }

            latInflu = (90 * latInflu) / 100;
            longInflu = (90 * longInflu) / 100;
            float angleHead;

            if (latHead < longHead) { angleHead = longHead - latHead; } else { angleHead = longHead + latHead; }

            return angleHead;
        }

        public float pitchSpeed()
        {
            var hSpeed = firstStage.Flight(firstStage.SurfaceReferenceFrame).HorizontalSpeed;
            var vSpeed = firstStage.Flight(firstStage.SurfaceReferenceFrame).VerticalSpeed;

            double speed = hSpeed + Math.Abs(vSpeed);

            double vSpeedInflu = (Math.Abs(vSpeed) * 100) / speed;

            float PitchSpeed = Convert.ToSingle((vSpeedInflu * 90) / 100);
            return PitchSpeed;
        }

        public void guidanceSystem(Vessel firstStage,Connection connectionFirstStage)
        {
            while (true)
            {
                try
                {
                    var Ft = firstStage.AvailableThrust;
                    var trueFT = firstStage.Thrust;
                    var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                    var TWR = Ft / Fw;
                    var trueTWR = trueFT / Fw;

                    var body = firstStage.Orbit.Body;





                    if (FinalPhase == true)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 * 2;
                        var difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 * 2;
                        Console.WriteLine("Thread abord");
                        firstStage.AutoPilot.TargetPitchAndHeading(90, 0);
                        Thread.CurrentThread.Abort();

                    }

                    if (EntryBurn == true)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);

                        firstStage.AutoPilot.TargetHeading = angleHeadLZ();
                        firstStage.AutoPilot.TargetPitch = anglePitchLZ();

                        Console.WriteLine("Pitch = " + anglePitchLZ());
                        Console.WriteLine("Head = " + angleHeadLZ());
                    }
                    else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 200)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);

                        firstStage.AutoPilot.TargetPitch = anglePitchLZ() - (pitchSpeed() - anglePitchLZ());
                        firstStage.AutoPilot.TargetHeading = angleHeadLZ();

                        Console.WriteLine("Pitch = " + (anglePitchLZ() - (pitchSpeed() - anglePitchLZ())));
                        Console.WriteLine("Head = " + angleHeadLZ());
                    }
                    else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed < 200)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);

                        firstStage.AutoPilot.TargetPitch = anglePitchLZ() + (pitchSpeed() - anglePitchLZ());
                        firstStage.AutoPilot.TargetHeading = angleHeadLZ() + 180;

                        Console.WriteLine("Pitch = " + (anglePitchLZ() + (pitchSpeed() - anglePitchLZ())));
                        Console.WriteLine("Head = " + (angleHeadLZ() + 180));
                    }

                    firstStage.AutoPilot.TargetRoll = 0;
                }
                catch (Exception e)
                {

                }
            }
        }

        public void guidanceSystem2(Vessel firstStage, Connection connection)
        {
            while (true)
            {
                try
                {
                    var Ft = firstStage.AvailableThrust;
                    var trueFT = firstStage.Thrust;
                    var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                    var TWR = Ft / Fw;
                    var trueTWR = trueFT / Fw;

                    var body = firstStage.Orbit.Body;





                    if (FinalPhase == true)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 * 2;
                        var difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 * 2;
                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, -1.0, 0.0);
                        Console.WriteLine("Thread abord");
                        firstStage.AutoPilot.TargetPitchAndHeading(90, 0);
                        Thread.CurrentThread.Abort();

                    }

                    if (EntryBurn == true)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                        /*if (difPosLat > 0.1) { difPosLat = 0.3; }
                        else if (difPosLat < -0.1) { difPosLat = -0.3; }
                        else { difPosLat = 0; }
                        if (difPosLong > 0.1) { difPosLong = 0.4; }
                        else if (difPosLong < -0.1) { difPosLong = -0.1; }
                        else { difPosLong = 0.3; }*/

                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create((difPosLat), -1.0, (-difPosLong + 0.1));
                    }
                    else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 200)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                        /*if (difPosLat > 0.1) { difPosLat = 0.4; }
                        else if (difPosLat < -0.1) { difPosLat = -0.4; }
                        else { difPosLat = difPosLat * 15; }
                        if (difPosLong > 0.1) { difPosLong = 0.4; }
                        else if (difPosLong < -0.1) { difPosLong = -0.4; }
                        else { difPosLong = difPosLong * 15; }*/

                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create((difPosLat * 40), -1.0, (difPosLong * 40));
                    }
                    else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed < 200)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                        /*if (difPosLat > 0.02) { difPosLat = 0.1; }
                        else if (difPosLat < -0.02) { difPosLat = -0.1; }
                        else { difPosLat = difPosLat; }
                        if (difPosLong > 0.02) { difPosLong = 0.1; }
                        else if (difPosLong < -0.02) { difPosLong = -0.1; }
                        else { difPosLong = difPosLong; }*/

                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create((difPosLat), -1.0, (-difPosLong));
                    }

                    firstStage.AutoPilot.TargetRoll = 0;

                    /*var difPosLat1 = connectionFirstStage.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                    var difPosLong1 = (connectionFirstStage.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                    var distLat = landingZonePosition().Item1 * Math.PI / 180 - connectionFirstStage.Trajectories().ImpactPos().Item1 * Math.PI / 180;
                    var distLong = landingZonePosition().Item2 * Math.PI / 180 - connectionFirstStage.Trajectories().ImpactPos().Item2 * Math.PI / 180;
                    var dist = Math.Pow(distLat, 2) + Math.Pow(distLong, 2);
                    dist = Math.Sqrt(dist);
                    Console.WriteLine("DifPoseLat = " + difPosLat1);
                    Console.WriteLine("DifPoseLong = " + difPosLong1);
                    Console.WriteLine("Dist = " + dist);*/
                }
                catch (Exception e)
                {

                }
            }
        }

        public void GuidanceSystemPerfect(Vessel firstStage, Connection connectionFirstStage)
        {
            firstStage.AutoPilot.StoppingTime = Tuple.Create(0.5, 0.1, 0.5);
            firstStage.AutoPilot.DecelerationTime = Tuple.Create(1.0, 0.5, 1.0);
            firstStage.AutoPilot.AttenuationAngle = Tuple.Create(0.1, 0.1, 0.1);
            firstStage.AutoPilot.AutoTune = false;
            firstStage.AutoPilot.PitchPIDGains = Tuple.Create(100.0, 2.0, 0.1);
            firstStage.AutoPilot.RollPIDGains = Tuple.Create(5.0, 1.0, 0.1);
            firstStage.AutoPilot.YawPIDGains = Tuple.Create(100.0, 2.0, 0.1);


            while (true)
            {
                if (FinalPhase == true)
                {
                    var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 * 2;
                    var difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 * 2;
                    Console.WriteLine("Thread abord");
                    firstStage.AutoPilot.TargetPitchAndHeading(90, 0);
                    Thread.CurrentThread.Abort();

                }

                if (firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude < 130000 && firstStage.Flight(firstStage.SurfaceVelocityReferenceFrame).TrueAirSpeed >= 150 && EntryBurn == true)
                {
                    float angle = circleDistance() / 12;

                    if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 >-0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle + 10, 45), 225);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle + 10, 45), 315);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > 0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > 0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 135);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > 0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 45);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 270);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > 0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle + 10, 45), 90);
                    }
                    else if (connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > 0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 180);
                    }
                    else if (connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 0);
                    }
                    else
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, -1.0, 0.0);
                    }

                    /*var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                    var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                    /*if (difPosLat > 0.1) { difPosLat = 0.3; }
                    else if (difPosLat < -0.1) { difPosLat = -0.3; }
                    else { difPosLat = 0; }
                    if (difPosLong > 0.1) { difPosLong = 0.4; }
                    else if (difPosLong < -0.1) { difPosLong = -0.1; }
                    else { difPosLong = 0.3; }*/

                    /*firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create((-difPosLat), -1.0, (-difPosLong + 0.1));*/
                }
                else if (firstStage.Flight(firstStage.SurfaceVelocityReferenceFrame).TrueAirSpeed > 150 && EntryBurn == false)
                {
                    float angle = circleDistance() / 4;

                    if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 225);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 315);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 135);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 45);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 270);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 90);
                    }
                    else if (connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 180);
                    }
                    else if (connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 0);
                    }
                    else
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, -1.0, 0.0);
                    }

                    var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                    var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                    /*if (difPosLat > 0.02) { difPosLat = 0.1; }
                    else if (difPosLat < -0.02) { difPosLat = -0.1; }
                    else { difPosLat = difPosLat; }
                    if (difPosLong > 0.02) { difPosLong = 0.1; }
                    else if (difPosLong < -0.02) { difPosLong = -0.1; }
                    else { difPosLong = difPosLong; }*/

                    /*firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create((difPosLat * 3), -1.0, (difPosLong * 3));*/
                }
                else if (firstStage.Flight(firstStage.SurfaceVelocityReferenceFrame).TrueAirSpeed <= 150 && EntryBurn == false)
                {
                    float angle = circleDistance() / 8;

                    if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 225);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 315);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 135);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > -0.0001 && connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 45);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 270);
                    }
                    else if (connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 90);
                    }
                    else if (connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 > -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 180);
                    }
                    else if (connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 < -0.0001)
                    {
                        firstStage.AutoPilot.TargetPitchAndHeading(Math.Max(90 - angle, 45), 0);
                    }
                    else
                    {
                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, -1.0, 0.0);
                    }

                    /*var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                    var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                    /*if (difPosLat > 0.02) { difPosLat = 0.1; }
                    else if (difPosLat < -0.02) { difPosLat = -0.1; }
                    else { difPosLat = difPosLat; }
                    if (difPosLong > 0.02) { difPosLong = 0.1; }
                    else if (difPosLong < -0.02) { difPosLong = -0.1; }
                    else { difPosLong = difPosLong; }*/

                    /*firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                    firstStage.AutoPilot.TargetDirection = Tuple.Create((-difPosLat * 2), -1.0, (-difPosLong * 2));*/
                }
            }
        }

        public float circleDistance()
        {
            Tuple<double, double> p1 = landingZonePosition();     //...this point...
            Tuple<double, double> p2 = Tuple.Create(firstStage.Flight(null).Latitude, firstStage.Flight(null).Longitude);     //...to this point...
            double radius = firstStage.Orbit.Body.EquatorialRadius + firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude; //...around a body of this radius. (note: if you are flying you may want to use ship:body:radius + altitude).
            double A = Math.Pow(Math.Pow(Math.Sin((p1.Item1 - p2.Item1) / 2), 2) + Math.Cos(p1.Item1) * Math.Cos(p2.Item1) * Math.Sin((p1.Item2 - p2.Item2) / 2), 2);

            return Convert.ToSingle(radius * Math.PI * Math.Atan2(Math.Sqrt(A), Math.Sqrt(1 - A)) / 90);
        }

        public Tuple<Double, Double> landingZonePosition()
        {
            if (Startup.GetInstance().GetFlightInfo().getLZ() == "LZ-1")
            {
                var landingZonePosition = Tuple.Create(Convert.ToDouble(28.49289896), Convert.ToDouble(-80.51653638));
                return landingZonePosition;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "OCISLY")
            {
                var landingZonePosition = Tuple.Create(droneShip.Flight(droneShip.SurfaceReferenceFrame).Latitude, droneShip.Flight(droneShip.SurfaceReferenceFrame).Longitude);
                return landingZonePosition;
                //return null;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "LZ-4")
            {
                var landingZonePosition = Tuple.Create(Convert.ToDouble(34.7362686474572), Convert.ToDouble(-120.278871365682));
                return landingZonePosition;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "FHLZ")
            {
                var landingZonePosition = Tuple.Create(0.0,0.0);
                if (rocketBody == RocketBody.FH_SIDEBOOSTER_A) //LZ-1
                {
                    landingZonePosition = Tuple.Create(Convert.ToDouble(26.8058981888411), Convert.ToDouble(-80.5380493910099));
                }
                else if (rocketBody == RocketBody.FH_SIDEBOOSTER_B) //LZ-2
                {
                    landingZonePosition = Tuple.Create(Convert.ToDouble(26.8400145977634), Convert.ToDouble(-80.5606280072286));
                }
                return landingZonePosition;
            }

            else
            {
                return null;
            }
        }

        public void LandingTarget()
        {
            //LZ-1 = Logitude : -80.5379863249586 // Latitude : 27.3948765184433
            //OCISLY = Longitude : positionOCISLY().Item2 // Latitude : positionOCISLY().Item1
            //while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude > 200 || firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude > 200)
            while (true)
            {
                try
                {
                    var Ft = firstStage.AvailableThrust;
                    var trueFT = firstStage.Thrust;
                    var Fw = firstStage.Mass * firstStage.Orbit.Body.SurfaceGravity;
                    var TWR = Ft / Fw;
                    var trueTWR = trueFT / Fw;

                    var body = firstStage.Orbit.Body;

                    /*Tuple<Double, Double, Double> LAT = Tuple.Create<Double, Double>(body.LatitudeAtPosition());

                    var worldImpactPos = (Vector3)impactVect + body.position;
                    var lat = body.GetLatitude(worldImpactPos);
                    var test = ve
                    */

                    if (FinalPhase == true)
                    {
                        //Console.WriteLine("Balistic Position.");
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 * 2;
                        var difPosLong = connection.Trajectories().ImpactPos().Item2 - landingZonePosition().Item2 * 2;
                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, -1.0, 0.0);
                        Console.WriteLine("Thread abord");
                        firstStage.AutoPilot.TargetPitchAndHeading(90, 0);
                        Thread.CurrentThread.Abort();

                    }

                    if (EntryBurn == true)
                    {
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                        if (difPosLat > 0.2) { difPosLat = -0.2; }
                        else if (difPosLat < -0.2) { difPosLat = 0.2; }
                        if (difPosLong > 0.1) { difPosLong = -0.1; }
                        else if (difPosLong < -0.3) { difPosLong = 0.3; }

                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create((difPosLat), -1.0, -(difPosLong));
                    }
                    else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed > 150 && firstStage.Parts.WithTag("MainSecond")[0].Engine.Active == false)
                    {
                        if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed >= 150)
                        {
                            //Console.WriteLine("Balistic Position.");
                            var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1;
                            var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2);
                            if (difPosLat > 0.5) { difPosLat = 0.5; }
                            else if (difPosLat < -0.5) { difPosLat = -0.5; }
                            if (difPosLong > 0.6) { difPosLong = 0.6; }
                            else if (difPosLong < -0.6) { difPosLong = -0.6; }

                            //Console.WriteLine("Lat : " + difPosLat);
                            //Console.WriteLine("Long : " + difPosLong);

                            /*if (Math.Abs(difPosLat) > 0.01)
                            {
                                if (connection.Trajectories().ImpactPos().Item1 > positionOCISLY().Item1)
                                {
                                    if (connection.Trajectories().ImpactPos().Item2 > positionOCISLY().Item2)
                                    {
                                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                        firstStage.AutoPilot.TargetDirection = Tuple.Create(-difPosLat, -1.0, difPosLong);
                                    }
                                    else if (connection.Trajectories().ImpactPos().Item2 < positionOCISLY().Item2)
                                    {
                                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                        firstStage.AutoPilot.TargetDirection = Tuple.Create(difPosLat, -1.0, difPosLong);
                                    }
                                }
                                else if (connection.Trajectories().ImpactPos().Item1 < positionOCISLY().Item1)
                                {
                                    double retrograde = (firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item3) + 10;

                                    if (connection.Trajectories().ImpactPos().Item2 > positionOCISLY().Item2)
                                    {
                                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                        firstStage.AutoPilot.TargetDirection = Tuple.Create(difPosLat, -1.0, -difPosLong);
                                    }
                                    else if (connection.Trajectories().ImpactPos().Item2 < positionOCISLY().Item2)
                                    {
                                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                        firstStage.AutoPilot.TargetDirection = Tuple.Create(-difPosLat, -1.0, -difPosLong);
                                    }
                                }
                            }
                            else
                            {
                                firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                firstStage.AutoPilot.TargetDirection = Tuple.Create(0.0, -1.0, 0.0);
                            }*/

                            firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                            firstStage.AutoPilot.TargetDirection = Tuple.Create((difPosLat), -1.0, -(difPosLong));
                        }
                    }
                    else if (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed < 150 || firstStage.Parts.WithTag("MainSecond")[0].Engine.Active == true)
                    {
                        //Console.WriteLine("Pre landing Position.");
                        var difPosLat = connection.Trajectories().ImpactPos().Item1 - landingZonePosition().Item1 / 2;
                        var difPosLong = (connection.Trajectories().ImpactPos().Item2) - (landingZonePosition().Item2) / 2;

                        if (difPosLat > 0.2) { difPosLat = 0.2; }
                        else if (difPosLat < -0.2) { difPosLat = -0.2; }
                        if (difPosLong > 0.1) { difPosLong = 0.1; }
                        else if (difPosLong < -0.4) { difPosLong = -0.4; }

                        /*if (Math.Abs(difPosLat) > 0.05)
                        {
                            if (connection.Trajectories().ImpactPos().Item1 > positionOCISLY().Item1)
                            {
                                if (connection.Trajectories().ImpactPos().Item2 > positionOCISLY().Item2)
                                {
                                    firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                    firstStage.AutoPilot.TargetDirection = Tuple.Create(difPosLat, -1.0, -difPosLong);
                                }
                                else if (connection.Trajectories().ImpactPos().Item2 < positionOCISLY().Item2)
                                {
                                    firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                    firstStage.AutoPilot.TargetDirection = Tuple.Create(-difPosLat, -1.0, -difPosLong);
                                }
                            }
                            else if (connection.Trajectories().ImpactPos().Item1 < positionOCISLY().Item1)
                            {
                                double retrograde = (firstStage.Flight(firstStage.SurfaceReferenceFrame).Retrograde.Item3) + 10;

                                if (connection.Trajectories().ImpactPos().Item2 > positionOCISLY().Item2)
                                {
                                    firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                    firstStage.AutoPilot.TargetDirection = Tuple.Create(-difPosLat, -1.0, difPosLong);
                                }
                                else if (connection.Trajectories().ImpactPos().Item2 < positionOCISLY().Item2)
                                {
                                    firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                                    firstStage.AutoPilot.TargetDirection = Tuple.Create(difPosLat, -1.0, difPosLong);
                                }
                            }
                        }*/

                        firstStage.AutoPilot.ReferenceFrame = firstStage.SurfaceVelocityReferenceFrame;
                        firstStage.AutoPilot.TargetDirection = Tuple.Create(-(difPosLat), -1.0, (difPosLong));
                    }
                }
                catch (Exception e)
                {

                }
            }
            Console.WriteLine("Lol sa march pa.");
            firstStage.AutoPilot.TargetPitchAndHeading(90, 0);
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
