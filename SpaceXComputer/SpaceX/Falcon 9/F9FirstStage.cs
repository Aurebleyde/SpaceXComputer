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
using System.IO;
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

        public double VHour = 14;
        public double VMinute = 09;
        public double VSecond = 30;

        protected double InitLat; //LZ-1
        protected double InitLon;
        protected double InitAlt;

        //protected double InitAlt = 49; //OCISLY

        public void F9Startup(Connection connectionLink, Connection connectionFirstStageLink)
        {
            connection = connectionLink;
            connectionFirstStage = connectionFirstStageLink;
            firstStage.AutoPilot.Engage();

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

            InitLat = landingZonePosition().Item1;
            InitLon = landingZonePosition().Item2;

            firstStage.Control.Forward = -1;
            Thread.Sleep(10000);
            firstStage.Control.Forward = 0;

            boostbackBurn(firstStage, connectionFirstStage);
            EntryBurn = true;

            firstStage.AutoPilot.Disengage();
            NewEntryBurn(connectionFirstStage);
            SuicidBurn();
        }

        public void boostbackBurn(Vessel firstStage, Connection connectionFirstStage)
        {
            Console.WriteLine("Falcon 9 first stage accisition signal.");
            firstStage.AutoPilot.Engage();
            firstStage.AutoPilot.TargetRoll = 0;

            Thread.Sleep(4000);

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
                firstStage.Control.Throttle = 1;
            }

            firstStage.Control.Throttle = 0;
            Console.WriteLine("FIRST STAGE : Boostback burn shutdown.");
            firstStage.Control.RCS = false;
            firstStage.Control.Brakes = true;
        }

        public void NewEntryBurn(Connection connectionFirstStage)
        {
            firstStage.Control.Brakes = true;
            firstStage.Control.RCS = true;
            firstStage.Control.SpeedMode = SpeedMode.Surface;
            firstStage.Control.RCS = true;

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

            while (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed >= 350) { }

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
                if (verticalSpeed > 420)
                {
                    verticalSpeed = 420;
                }

                double trueRadar = firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude - landedAltitude - 30; //15

                double g = 9.81;
                double maxDecelCentral = ((firstStage.Parts.WithTag("MainCentral")[0].Engine.AvailableThrust) / firstStage.Mass) - g;
                double stopDistCentral = Math.Pow(Math.Abs(verticalSpeed), 2) / (2 * maxDecelCentral);
                double impactTime = trueRadar / Math.Abs(verticalSpeed);

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ - (firstStage.Flight(firstStage.SurfaceReferenceFrame).TrueAirSpeed * 1.25) <= stopDistCentral && suicideBurnText == false)
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

        public Tuple<double, double> DifImpactPos(Vessel firstStage, Connection connection)
        {
            double difPosLat = landingZonePosition().Item1 - connection.Trajectories().ImpactPos().Item1;
            double  difPosLong = landingZonePosition().Item2 - connection.Trajectories().ImpactPos().Item2;

            return Tuple.Create(difPosLat, difPosLong);
        }

        public float circleDistance()
        {
            Tuple<double, double> p1 = Tuple.Create(landingZonePosition().Item1, landingZonePosition().Item2);     //...this point...
            Tuple<double, double> p2 = Tuple.Create(firstStage.Flight(null).Latitude, firstStage.Flight(null).Longitude);     //...to this point...
            double radius = firstStage.Orbit.Body.EquatorialRadius + firstStage.Flight(firstStage.SurfaceReferenceFrame).SurfaceAltitude; //...around a body of this radius. (note: if you are flying you may want to use ship:body:radius + altitude).
            double A = Math.Pow(Math.Pow(Math.Sin((p1.Item1 - p2.Item1) / 2), 2) + Math.Cos(p1.Item1) * Math.Cos(p2.Item1) * Math.Sin((p1.Item2 - p2.Item2) / 2), 2);

            return Convert.ToSingle(radius * Math.PI * Math.Atan2(Math.Sqrt(A), Math.Sqrt(1 - A)) / 90);
        }

        public Tuple<Double, Double, Double> landingZonePosition()
        {
            if (Startup.GetInstance().GetFlightInfo().getLZ() == "LZ-1")
            {
                var landingZonePosition = Tuple.Create(Convert.ToDouble(28.49289896), Convert.ToDouble(-80.51653638), Convert.ToDouble(47.6120));
                return landingZonePosition;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "LZ-2")
            {
                var landingZonePosition = Tuple.Create(Convert.ToDouble(28.493734), Convert.ToDouble(-80.519386), Convert.ToDouble(47.6120));
                return landingZonePosition;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "OCISLY")
            {
                var landingZonePosition = Tuple.Create(droneShip.Flight(droneShip.SurfaceReferenceFrame).Latitude, droneShip.Flight(droneShip.SurfaceReferenceFrame).Longitude, droneShip.Flight(droneShip.SurfaceReferenceFrame).SurfaceAltitude + 5);
                return landingZonePosition;
                //return null;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "LZ-4")
            {
                var landingZonePosition = Tuple.Create(Convert.ToDouble(34.7362686474572), Convert.ToDouble(-120.278871365682), Convert.ToDouble(47.6120));
                return landingZonePosition;
            }
            else if (Startup.GetInstance().GetFlightInfo().getLZ() == "FHLZ")
            {
                var landingZonePosition = Tuple.Create(0.0, 0.0, 0.0);
                if (rocketBody == RocketBody.FH_SIDEBOOSTER_A) //LZ-1
                {
                    landingZonePosition = Tuple.Create(Convert.ToDouble(28.49289896), Convert.ToDouble(-80.51653638), Convert.ToDouble(47.6120));
                }
                else if (rocketBody == RocketBody.FH_SIDEBOOSTER_B) //LZ-2
                {
                    landingZonePosition = Tuple.Create(Convert.ToDouble(28.493734), Convert.ToDouble(-80.519386), Convert.ToDouble(47.6120));
                }
                return landingZonePosition;
            }

            else
            {
                return null;
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
