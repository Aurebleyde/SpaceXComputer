using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.Trajectories;
//using System.Numerics;
using UnityEngine;

namespace SpaceXComputer
{
    public class Starship
    {
        public Connection connection;
        public Vessel starship;

        protected double InitLat = 25.9104119486221;
        protected double InitLon = -97.1562937661744;
        protected double LandingAlt = 30;
        protected double InitAlt;

        double LandLat;
        double LandLon;

        protected Engine Raptor;

        Thread Guid;

        public Starship(Vessel vessel, RocketBody rocketBody)
        {
            starship = vessel;
            rocketBody = RocketBody.STARSHIP;
        }

        public void startup(Connection Connection)
        {
            connection = Connection;

            Console.WriteLine("Starship : Starship is in startup.");

            Console.WriteLine("LAt : " + starship.Flight(starship.SurfaceReferenceFrame).Latitude + " | LONG : " + starship.Flight(starship.SurfaceReferenceFrame).Longitude);

            Thread.Sleep(5000);

            InitAlt = starship.Flight(starship.SurfaceReferenceFrame).MeanAltitude;
            Raptor = starship.Parts.Engines[0];
            double trueRadar = starship.Flight(starship.SurfaceReferenceFrame).SurfaceAltitude - 30;
            Console.WriteLine("true radar : " + trueRadar);

            Ignition();
            Thread.Sleep(5000);
            Thread OffSet = new Thread(OffSetCorrections);
            OffSet.Start();
            Guid = new Thread(Guidance);
            
            Thread.Sleep(15000);
            Assent();
            Guid.Start();
            Console.WriteLine("Descent");
            Descent();
            Console.WriteLine($"Land Lat = {LandLat} | Land Long = {LandLon}");
            Console.WriteLine($"Distance = {Distance(InitLat, LandLat, InitLon, LandLon)}");
            Thread.Sleep(10000);
            Verification();
            Guid?.Abort();
        }

        private void Ignition()
        {
            Console.WriteLine("Ignition");

            float aThrust = starship.AvailableThrust;

            starship.AutoPilot.Engage();
            starship.AutoPilot.TargetPitchAndHeading(90, Startup.GetInstance().GetFlightInfo().getHead());

            Raptor.Active = true;
            /*starship.Parts.Engines[1].Active = true;
            starship.Parts.Engines[2].Active = true;*/
            starship.Control.Throttle = ThrottleToTWR(0.1f);

            Thread.Sleep(5000);
            Console.WriteLine("Throttle Up");
            starship.Control.Throttle = ThrottleToTWR(0.8f);

            Thread.Sleep(4000);

            if (TWR() >= 0.75f)
            {
                Console.WriteLine("Liftoff");
                starship.Control.Throttle = ThrottleToTWR(1.05f);

                starship.Control.ToggleActionGroup(4);
                starship.Control.RCS = true;

                Thread.Sleep(1000);

                foreach (LaunchClamp clamp in starship.Parts.LaunchClamps)
                {
                    clamp.Release();
                }
            }
            else
            {
                Console.WriteLine($"Abort !\nThrust : {0}\nTWR : {1}", starship.Thrust, TWR());
                Raptor.Active = false;
                starship.Control.Throttle = 0;

                Thread.Sleep(999999999);
            }
        }

        private void Assent()
        {
            double pastAp = starship.Orbit.ApoapsisAltitude - InitAlt;

            while (starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed > 0 || starship.Flight(starship.SurfaceReferenceFrame).SurfaceAltitude < Startup.GetInstance().GetFlightInfo().getMaxAltitude())
            {
                double Ap = starship.Orbit.ApoapsisAltitude - InitAlt;

                float TWRTarget;

                if (Ap >= Startup.GetInstance().GetFlightInfo().getMaxAltitude())
                {
                    TWRTarget = 0.6f;
                }
                else
                {
                    TWRTarget = 1.2f;
                }

                if (starship.Flight(starship.SurfaceReferenceFrame).SurfaceAltitude >= Startup.GetInstance().GetFlightInfo().getMaxAltitude())
                {
                    TWRTarget = 0.4f;
                    starship.Control.Throttle = TWRTarget;
                    break;
                }

                TWRTarget = ThrottleToTWR(TWRTarget);
                if (TWRTarget < 0.001f)
                {
                    TWRTarget = 0.001f;
                }

                starship.Control.Throttle = TWRTarget;

                starship.AutoPilot.TargetHeading = (float)RetroHead();
                starship.AutoPilot.TargetPitch = 90 - ((float)PitchRetro() / 4f);
            }
        }

        private void Descent()
        {
            float TWRTarget = ThrottleToTWR(0.55f);
            if (TWRTarget < 0.01f)
            {
                TWRTarget = 0.01f;
            }

            starship.Control.Throttle = TWRTarget;
            SuicidBurn();

            Console.WriteLine("Starship has landed");
        }

        private void Verification()
        {
            double Pitch = starship.Flight(starship.SurfaceReferenceFrame).Pitch;
            Console.WriteLine("Pitch : " + Pitch);
            if (Pitch > 85)
            {
                Console.WriteLine("Starship is vertical");
            }
            else if (Pitch > 70)
            {
                Console.WriteLine("Starship isn't vertical but stable");
            }
            else
            {
                Console.WriteLine("Starship is falling");
            }

            LandLat = starship.Flight(starship.SurfaceReferenceFrame).Latitude;
            LandLon = starship.Flight(starship.SurfaceReferenceFrame).Longitude;
            Console.WriteLine($"Actual Lat = {LandLat} | Actual Long = {LandLon}");

            Console.WriteLine($"Distance = {Distance(InitLat, LandLat, InitLon, LandLon)}");
        }

        private void SuicidBurn()
        {
            bool suicideBurnText = false;
            bool suicideBurn = false;

            float throt = 0;

            while (true)
            {

                if (suicideBurnText == false)
                {
                    throt = ThrottleToTWR(0.7f);
                }

                double trueRadar = starship.Flight(starship.SurfaceReferenceFrame).SurfaceAltitude - 20;
                double verticalSpeed = starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed;
                //Console.WriteLine("true radar : " + trueRadar);

                double g = 9.81;
                double maxDecel = ((starship.MaxThrust / starship.Mass) - g);
                double stopDist = Math.Pow(verticalSpeed, 2) / (1.5f * maxDecel);
                double impactTime = trueRadar / Math.Abs(verticalSpeed);

                double IdealThrottle = stopDist / trueRadar;
                throt = (float)IdealThrottle;

                /*Console.WriteLine("MaxDecel : " + maxDecel);
                Console.WriteLine("Vertical Speed : " + verticalSpeed);
                Console.WriteLine("stopDist : " + stopDist);
                Console.WriteLine("IdealThrottle : " + IdealThrottle);*/

                if (Math.Abs(impactTime) < 4)
                {
                    starship.Control.Gear = true;
                }

                if (verticalSpeed > -3 && trueRadar < 80)
                {
                    throt = ThrottleToTWR(0.93f);
                }


                if (starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed > -0.2 && trueRadar < 10 && suicideBurn == false || connection.Trajectories().HasImpact() == false)
                {
                    starship.Control.Throttle = 0;
                    throt = 0;
                    Raptor.Active = false;
                    Console.WriteLine("Starship has landed.");
                    suicideBurn = true;
                    suicideBurnText = false;

                    LandLat = starship.Flight(starship.SurfaceReferenceFrame).Latitude;
                    LandLon = starship.Flight(starship.SurfaceReferenceFrame).Longitude;

                    Console.WriteLine($"Land Lat = {LandLat} | Land Long = {LandLon}");
                    Console.WriteLine($"Distance = {Distance(InitLat, LandLat, InitLon, LandLon)}");
                    break;
                }
                

                if (throt <= 0.01f)
                {
                    throt = 0.01f;
                }

                starship.Control.Throttle = throt;
            }
        }

        private void OffSetCorrections()
        {
            float PastPitchPosition = starship.Flight(starship.SurfaceReferenceFrame).Pitch;
            float PastHeadPosition = starship.Flight(starship.SurfaceReferenceFrame).Heading;
            float PastPitchControl = starship.Control.Pitch;
            float PastYawControl = starship.Control.Yaw;

            float TargetedPicth;
            float TargetedHead;
            float PitchControl;
            float YawControl;

            while (true)
            {
                TargetedPicth = starship.AutoPilot.TargetPitch;
                TargetedHead = starship.AutoPilot.TargetHeading;
                PitchControl = starship.Control.Pitch;
                YawControl = starship.Control.Yaw;

                starship.Control.Yaw = YawControl * 1.5f;
                starship.Control.Pitch = PitchControl * 1.1f;

                //if (Math.Abs(PitchControl) >= Math.Abs(PastPitchControl) && ())

                //starship.Control.Yaw = starship.Control.Pitch + 0.38f;
            }
        }

        double PastLatImpact;
        double PastLonImpact;
        double LatImpact;
        double LonImpact;

        private void Guidance()
        {
            PastLatImpact = ImpactPos().Item1;
            PastLonImpact = ImpactPos().Item2;

            double trueRadar = starship.Flight(starship.SurfaceReferenceFrame).SurfaceAltitude - 20;
            double verticalSpeed = starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed;
            //Console.WriteLine("true radar : " + trueRadar);

            double g = 9.81;
            double maxDecel = ((starship.MaxThrust / starship.Mass) - g);
            double impactTime = trueRadar / Math.Abs(verticalSpeed);
            impactTime = 999;

            while (impactTime > 7)
            {
                LatImpact = ImpactPos().Item1;
                LonImpact = ImpactPos().Item2;

                HeadingGuidance();
                PitchGuidance();

                Console.WriteLine("Head : " + starship.AutoPilot.TargetHeading);
                Console.WriteLine("Pitch : " + starship.AutoPilot.TargetPitch);

                PastLatImpact = ImpactPos().Item1;
                PastLonImpact = ImpactPos().Item2;

                Thread.Sleep(100);

                trueRadar = starship.Flight(starship.SurfaceReferenceFrame).SurfaceAltitude - 20;
                verticalSpeed = starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed;
                maxDecel = ((starship.MaxThrust / starship.Mass) - g);
                impactTime = trueRadar / Math.Abs(verticalSpeed);

                if (starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed > 0)
                {
                    impactTime = 999;
                }
            }

            Console.WriteLine("Retrograde");
            while (true)
            {
                if (starship.Flight(starship.Orbit.Body.ReferenceFrame).HorizontalSpeed > 2)
                {
                    starship.AutoPilot.TargetHeading = (float)RetroHead();
                    starship.AutoPilot.TargetPitch = 90 - ((float)PitchRetro() / 3.2f);

                    Console.WriteLine("Pitch : " + starship.AutoPilot.TargetPitch);
                }
                else
                {
                    starship.AutoPilot.TargetPitchAndHeading(90, starship.Flight(starship.SurfaceReferenceFrame).Heading);
                }

                //Console.WriteLine("Pitch : " + pitch_for());
            }
        }
        private double compass_for()
        {
            ReferenceFrame frame = starship.SurfaceReferenceFrame;
            Vector3 pointing = new Vector3((float)starship.Flight(frame).Retrograde.Item1, (float)starship.Flight(frame).Retrograde.Item2, (float)starship.Flight(frame).Retrograde.Item3);

            Vector3 horizonVector = new Vector3(0, pointing.y, pointing.z);

            Vector3 north = new Vector3(0.0f, 1.0f, 0.0f);

            double heading = Vector3.Angle(north, horizonVector);
            if (horizonVector.z < 0)
            {
                heading = 360 - heading;
            }
            return heading;
        }
        private double pitch_for()
        {
            ReferenceFrame frame = starship.SurfaceReferenceFrame;
            Vector3 pointing = new Vector3((float)starship.Flight(frame).Retrograde.Item1, (float)starship.Flight(frame).Retrograde.Item2, (float)starship.Flight(frame).Retrograde.Item3);
            Vector3 up = new Vector3(1f, 0f, 0f);

            double pitch = 90 - Vector3.Angle(up, pointing);
            return pitch;
        }
        private float RetroHead()
        {
            Vector2 Dir = new Vector2((float)(ImpactPos().Item2 - starship.Flight(starship.SurfaceReferenceFrame).Longitude), (float)(ImpactPos().Item1 - starship.Flight(starship.SurfaceReferenceFrame).Latitude));
            Vector2 North = new Vector2(0.0f, 1.0f);

            double angle = Vector2.Angle(North, Dir);

            float Head = 0;
            if (Dir.x >= 0)
            {
                Head = 180 + (float)angle;
                Console.WriteLine("1");
            }
            else
            {
                Head = (float)angle;
                Console.WriteLine("2");
            }

            return Head;
        }
        private float PitchRetro()
        {
            double vSpeed = Math.Abs(starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed);
            double hSpeed = starship.Flight(starship.Orbit.Body.ReferenceFrame).HorizontalSpeed;

            double angle = (hSpeed / vSpeed);
            angle = ToDegrees(Math.Atan(angle));

            return (float)angle;
        }

        private void HeadingGuidance()
        {
            try
            {
                Vector2 Impact = new Vector2((float)(LonImpact - InitLon), (float)(LatImpact - InitLat));
                Vector2 Zone = new Vector2((float)0, (float)1);

                double angle = Vector2.Angle(Zone, Impact);

                float Head = 0;
                if (Impact.x >= 0)
                {
                    Head = 180 + ((float)angle * 1.15f);
                    Console.WriteLine("1");
                }
                else
                {
                    Head = 180 - ((float)angle * 1.15f);
                    Console.WriteLine("2");
                }

                starship.AutoPilot.TargetHeading = Head;
            }
            catch
            {
                starship.AutoPilot.TargetHeading = 0;
            }

            //Console.WriteLine("Heading : " + starship.AutoPilot.TargetHeading);
        }

        private void PitchGuidance()
        {
            double Pitch = 120;

            try
            {
                if (connection.Trajectories().HasImpact())
                {
                    if (starship.AutoPilot.HeadingError < 90)
                    {
                        starship.AutoPilot.TargetPitch = (float)Pitch;

                        double vSpeed = Math.Abs(starship.Flight(starship.Orbit.Body.ReferenceFrame).VerticalSpeed);
                        double distance = Distance(starship.Flight(starship.SurfaceReferenceFrame).Latitude, InitLat, starship.Flight(starship.SurfaceReferenceFrame).Longitude, InitLon);

                        double angle = (distance / vSpeed);
                        angle = ToDegrees(Math.Atan(angle));
                        angle = (angle / 20) * 10 /*(Distance(ImpactPos().Item1, InitLat, ImpactPos().Item2, InitLon) / 50)*/;
                        if (angle > 25)
                        {
                            angle = 25;
                        }

                        if (distance > Distance(starship.Flight(starship.SurfaceReferenceFrame).Latitude, ImpactPos().Item1, starship.Flight(starship.SurfaceReferenceFrame).Longitude, ImpactPos().Item2) - 5)
                        {
                            starship.AutoPilot.TargetPitch = 90 - (float)angle;
                            Console.WriteLine("distance : " + distance);
                            Console.WriteLine("L'autre : " + Distance(starship.Flight(starship.SurfaceReferenceFrame).Latitude, ImpactPos().Item1, starship.Flight(starship.SurfaceReferenceFrame).Longitude, ImpactPos().Item2));
                        }
                        else
                        {
                            starship.AutoPilot.TargetPitch = 90 + (float)angle;
                            Console.WriteLine("Ici");
                        }
                    }
                    else
                    {
                        starship.AutoPilot.TargetPitch = 88;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Ground has touched");
            }


            //Console.WriteLine("Pitch : " + starship.AutoPilot.TargetPitch);
        }

        public Tuple<double, double> ImpactPos()
        {
            try
            {
                return connection.Trajectories().ImpactPos();
            }
            catch
            {
                return new Tuple<double, double>(0, 0);
            }
        }

        private float TWR()
        {
            float Thrust = starship.Thrust;
            float Mass = starship.Mass * starship.Orbit.Body.SurfaceGravity;
            return Thrust / Mass;
        }

        private float ThrottleToTWR(float twr)
        {
            float Mass = starship.Mass * starship.Orbit.Body.SurfaceGravity;
            float T = twr * Mass;
            float Throttle = (T - (33079f)) / (starship.MaxThrust - (33079f));
            return Throttle;
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
        public double ToDegrees(double val)
        {
            return (val * 180) / Math.PI;
        }

        public Vessel GetVessel()
        {
            return starship;
        }

        public void SetVessel(Vessel vessel)
        {
            starship = vessel;
        }
    }
}
