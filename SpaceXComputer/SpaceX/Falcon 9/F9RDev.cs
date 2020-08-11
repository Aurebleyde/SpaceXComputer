using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.Trajectories;
using KRPC.Client.Services.KIPC;

namespace SpaceXComputer
{
    public class F9RDev
    {
        public Connection connection;
        public Vessel grassHopper;

        protected Engine Merlin;

        protected double InitLat;
        protected double InitLon;
        protected double InitAlt;

        protected float minThrottle = 0.1f;

        public F9RDev(Vessel vessel, RocketBody rocketBody)
        {
            grassHopper = vessel;
            rocketBody = RocketBody.F9RDev;
        }

        public void startup(Connection connectionLink)
        {
            connection = connectionLink;

            Merlin = grassHopper.Parts.WithTag("MainCentral")[0].Engine;

            InitLat = 28.49289896; //LZ-1
            InitLon = -80.51653638;
            InitAlt = 47.6120;
            /*InitLat = 28.507917;
            InitLon = -80.520953;*/

            Console.WriteLine($"Init Lat = {InitLat} | Init Long = {InitLon}");

            /*Thread Guid = new Thread(GuidanceLanding);
            Guid.Start();*/

            Console.WriteLine("Ignition");
            //Assent();

            Console.WriteLine("Descent");
            grassHopper.AutoPilot.Disengage();
            grassHopper.Control.Brakes = true;
            Descent();
            Console.WriteLine("Landing");

            double LandLat = grassHopper.Flight(grassHopper.SurfaceReferenceFrame).Latitude;
            double LandLon = grassHopper.Flight(grassHopper.SurfaceReferenceFrame).Longitude;

            Console.WriteLine($"Land Lat = {LandLat} | Land Long = {LandLon}");

            Console.WriteLine($"Distance = {Distance(InitLat, LandLat, InitLon, LandLon)}");

            grassHopper.Control.Brakes = false;
        }

        public void Assent()
        {
            grassHopper.AutoPilot.Engage();
            grassHopper.AutoPilot.TargetPitchAndHeading(90, 90);

            Merlin.Active = true;
            grassHopper.Control.Throttle = ThrottleToTWR(0.80f);

            Thread.Sleep(4000);
            /*if (TWR() < 0.75)
            {
                Console.WriteLine($"TWR = {TWR()} (0.74 required for liftoff)");
                Merlin.Active = false;
                grassHopper.Control.Throttle = 0;
                Console.WriteLine("Abort");
            }
            else
            {*/
                Console.WriteLine("Liftoff");
                grassHopper.Control.Throttle = ThrottleToTWR(1.10f);
            //}

            Thread.Sleep(10000);
            grassHopper.AutoPilot.TargetPitchAndHeading(90f, 90);
            grassHopper.Control.Gear = false;

            while (grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt < Startup.GetInstance().GetFlightInfo().getMaxAltitude())
            {
                grassHopper.Control.Throttle = ThrottleToTWR(1.20f);
                grassHopper.AutoPilot.TargetPitchAndHeading(89.5f, 120);
            }

            grassHopper.Control.RCS = true;
            //GuidanceEvent();
            
            AngleLimit = LimitOfAngle(grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt, Distance(InitLat, ImpactPos().Item1, InitLon, ImpactPos().Item2));

            while ((grassHopper.Flight(grassHopper.Orbit.Body.ReferenceFrame).VerticalSpeed > 0))
            {
                grassHopper.Control.Throttle = ThrottleToTWR(0.0f);
            }
        }

        public void Descent()
        {
            //kOSGuidance();

            AngleLimit = LimitOfAngle(grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt, Distance(InitLat, ImpactPos().Item1, InitLon, ImpactPos().Item2));

            grassHopper.Control.Throttle = ThrottleToTWR(0.0f);

            SuicidBurn();
        }

        public float TWR()
        {
            float Thrust = grassHopper.Thrust;
            float Mass = grassHopper.Mass * grassHopper.Orbit.Body.SurfaceGravity;
            float TWR = Thrust / Mass;
            return TWR;
        }

        public float ThrottleToTWR(float twr)
        {
            float Mass = grassHopper.Mass * grassHopper.Orbit.Body.SurfaceGravity;
            //Console.WriteLine(Mass);
            float T = twr * Mass;
            //Console.WriteLine(T);
            float Throttle = (T - 320125) / (grassHopper.AvailableThrust - 320125);
            //Console.WriteLine(Throttle);
            return Throttle;
        }

        public void kOSGuidance()
        {
            Processor proc = connection.KIPC().GetProcessor(grassHopper.Parts.WithTag("kOS")[0])[0];
            proc.SendMessage("Falcon is better launcher on Earth");
            grassHopper.AutoPilot.Disengage();
        }

        public void GuidanceEvent()
        {
            Thread Head = new Thread(HeadingGuidance);
            Head.Start();
            Thread Pitch = new Thread(PitchGuidance);
            Pitch.Start();
        }

        public void HeadingGuidance()
        {
            while (true)
            {
                try
                {
                    if (connection.Trajectories().HasImpact() == true)
                    {
                        if (grassHopper.Control.Throttle > 0)
                        {
                            if (ImpactPos().Item1 - InitLat >= 0 && ImpactPos().Item2 - InitLon >= 0) //Too North and East
                            {
                                double Total = (ImpactPos().Item1 - InitLat) + (ImpactPos().Item2 - InitLon);
                                double LatPercent = ((ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 270 - Head;

                                //Console.WriteLine("N & E");
                            }
                            else if (ImpactPos().Item1 - InitLat >= 0 && ImpactPos().Item2 - InitLon < 0) //Too North and West
                            {
                                double Total = (ImpactPos().Item1 - InitLat) + Math.Abs(ImpactPos().Item2 - InitLon);
                                double LatPercent = ((ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 90 + Head;

                                //Console.WriteLine("N & W");
                            }
                            else if (ImpactPos().Item1 - InitLat < 0 && ImpactPos().Item2 - InitLon >= 0) //Too South and East
                            {
                                double Total = Math.Abs(ImpactPos().Item1 - InitLat) + (ImpactPos().Item2 - InitLon);
                                double LatPercent = (Math.Abs(ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 270 + Head;

                                //Console.WriteLine("S & E");
                            }
                            else if (ImpactPos().Item1 - InitLat < 0 && ImpactPos().Item2 - InitLon < 0) //Too South and West
                            {
                                double Total = Math.Abs(ImpactPos().Item1 - InitLat) + Math.Abs(ImpactPos().Item2 - InitLon);
                                double LatPercent = (Math.Abs(ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 90 - Head;

                                //Console.WriteLine("S & W");
                            }
                        }
                        else
                        {
                            if (ImpactPos().Item1 - InitLat >= 0 && ImpactPos().Item2 - InitLon >= 0) //Too North and East
                            {
                                double Total = (ImpactPos().Item1 - InitLat) + (ImpactPos().Item2 - InitLon);
                                double LatPercent = ((ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 270 - Head;

                                //Console.WriteLine("N & E");
                            }
                            else if (ImpactPos().Item1 - InitLat >= 0 && ImpactPos().Item2 - InitLon < 0) //Too North and West
                            {
                                double Total = (ImpactPos().Item1 - InitLat) + Math.Abs(ImpactPos().Item2 - InitLon);
                                double LatPercent = ((ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 90 + Head;

                                //Console.WriteLine("N & W");
                            }
                            else if (ImpactPos().Item1 - InitLat < 0 && ImpactPos().Item2 - InitLon >= 0) //Too South and East
                            {
                                double Total = Math.Abs(ImpactPos().Item1 - InitLat) + (ImpactPos().Item2 - InitLon);
                                double LatPercent = (Math.Abs(ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 270 + Head;

                                //Console.WriteLine("S & E");
                            }
                            else if (ImpactPos().Item1 - InitLat < 0 && ImpactPos().Item2 - InitLon < 0) //Too South and West
                            {
                                double Total = Math.Abs(ImpactPos().Item1 - InitLat) + Math.Abs(ImpactPos().Item2 - InitLon);
                                double LatPercent = (Math.Abs(ImpactPos().Item1 - InitLat) * 100) / Total;
                                float Head = Convert.ToSingle((LatPercent * 90) / 100);

                                grassHopper.AutoPilot.TargetHeading = 90 - Head;

                                //Console.WriteLine("S & W");
                            }

                            AngleLimit = LimitOfAngle(grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt, Distance(InitLat, ImpactPos().Item1, InitLon, ImpactPos().Item2)) * 2;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Ground has touched");
                }
            }
        }

        public double AngleLimit;

        public void PitchGuidance()
        {
            while (true)
            {
                try
                {
                    if (connection.Trajectories().HasImpact())
                    {
                        double Total = (Math.Abs(ImpactPos().Item1 - InitLat) + Math.Abs(ImpactPos().Item2 - InitLon)) * 100000;

                        double diffLat = ImpactPos().Item1 - InitLat;
                        double diffLon = ImpactPos().Item2 - InitLon;


                        if (Total > AngleLimit)
                        {
                            Total = AngleLimit;
                        }

                        Total = 90 - Total;

                        grassHopper.AutoPilot.TargetPitch = Convert.ToSingle(Total);
                    }
                }
                catch
                {
                    Console.WriteLine("Ground has touched");
                }
            }
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

                double verticalSpeed = grassHopper.Flight(grassHopper.Orbit.Body.ReferenceFrame).VerticalSpeed;
                if (verticalSpeed < -320)
                {
                    verticalSpeed = -320;
                }

                double trueRadar = grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - landedAltitude - 5;

                double g = 9.81;
                double maxDecelCentral = ((grassHopper.Parts.WithTag("MainCentral")[0].Engine.AvailableThrust) / grassHopper.Mass) - g;
                double stopDistCentral = Math.Pow(Math.Abs(verticalSpeed), 2) / (1.5 * maxDecelCentral);
                double impactTime = trueRadar / Math.Abs(verticalSpeed);

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ - (grassHopper.Flight(grassHopper.SurfaceReferenceFrame).TrueAirSpeed * 2.5) <= stopDistCentral && suicideBurnText == false)
                {
                    Console.WriteLine("FIRST STAGE : Landing Burn started.");
                    suicideBurnText = true;

                    Merlin.Active = true;

                    throt = 1;

                    if (trueRadar < 100)
                    {
                        AngleLimit = 5; //3
                    }
                    else
                    {
                        try
                        {
                            AngleLimit = LimitOfAngle(grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt, Distance(InitLat, ImpactPos().Item1, InitLon, ImpactPos().Item2));
                        }
                        catch
                        {
                            Console.WriteLine("Ground touched");
                        }
                    }
                }

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ <= stopDistCentral && suicideBurnText == true)
                {
                    throt = 1;

                    if (trueRadar < 100)
                    {
                        AngleLimit = 3;
                    }
                    else
                    {
                        try
                        {
                            AngleLimit = LimitOfAngle(grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt, Distance(InitLat, ImpactPos().Item1, InitLon, ImpactPos().Item2));
                        }
                        catch
                        {
                            Console.WriteLine("Ground touched");
                        }
                    }
                }

                if (trueRadar /*- ((1 * trueRadar) / 100)*/ > stopDistCentral && suicideBurnText == true)
                {
                    throt = ThrottleToTWR(0.90f);

                    if (trueRadar < 40)
                    {
                        AngleLimit = 3;
                    }
                    else
                    {
                        try
                        {
                            AngleLimit = LimitOfAngle(grassHopper.Flight(grassHopper.SurfaceReferenceFrame).SurfaceAltitude - InitAlt, Distance(InitLat, ImpactPos().Item1, InitLon, ImpactPos().Item2));
                        }
                        catch
                        {
                            Console.WriteLine("Ground touched");
                        }
                    }
                }

                if (Math.Abs(impactTime) < 4)
                {
                    grassHopper.Control.Gear = true;
                }

                if (grassHopper.Flight(grassHopper.Orbit.Body.ReferenceFrame).VerticalSpeed > -10 && suicideBurnText == true)
                {
                    throt = ThrottleToTWR(0.95f);
                    Console.WriteLine("Yes faut ralentir lo");
                }

                if (grassHopper.Flight(grassHopper.Orbit.Body.ReferenceFrame).VerticalSpeed > 0 && suicideBurn == false)
                {
                    grassHopper.Control.Throttle = 0;
                    throt = 0;
                    Console.WriteLine("Grasshopper has landed.");
                    suicideBurn = true;

                    break;
                }

                if (throt <= 0.001f && suicideBurnText == true)
                {
                    throt = 0.01f;
                }

                grassHopper.Control.Throttle = throt;
            }
        }

        public Tuple<double, double> ImpactPos()
        {
            return connection.Trajectories().ImpactPos();
        }

        public double LimitOfAngle(double alt, double distance)
        {
            double Limit = Math.Atan(distance / alt) * 100;
            if (Limit > 15)
            {
                Limit = 15;
            }

            return Limit;
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

        public Vessel GetVessel()
        {
            return grassHopper;
        }

        public void SetVessel(Vessel vessel)
        {
            grassHopper = vessel;
        }
    }
}
