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

namespace SpaceXComputer
{
    public class C2FirstStage
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel firstStage;

        public C2FirstStage(Vessel vessel, RocketBody rocket)
        {
            firstStage = vessel;
            this.rocketBody = rocketBody;
        }

        public void C2Startup()
        {
            firstStage.AutoPilot.Engage();

            firstStage.Control.Throttle = 1;
            firstStage.AutoPilot.TargetPitchAndHeading(90, Startup.GetInstance().GetFlightInfo().getHead());

            firstStage.Parts.Engines[2].Active = true;

            Console.WriteLine("CARBON II : Main engine startup.");
            var during = 0;
            float thrust;
            while (during <= 1000)
            {
                thrust = firstStage.Parts.Engines[1].Thrust;
                during = during + 1;
            }
            thrust = firstStage.Thrust;
            if (thrust < firstStage.AvailableThrust - 5)
            {
                Console.WriteLine("CARBON II : Engine Abort.");
                Console.WriteLine("Trust : " + thrust);
                Console.WriteLine("Available Thrust : " + firstStage.AvailableThrust);
                firstStage.Control.Throttle = 0;
                firstStage.Parts.Engines[2].Active = false;
            }
            else
            {
                foreach (LaunchClamp clamp in firstStage.Parts.LaunchClamps)
                {
                    clamp.Release();
                }
                Console.WriteLine("CARBON II : Liftoff.");
            }
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
            firstStage.Parts.Engines[0].Active = true;
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

            firstStage.Parts.Engines[0].Active = false;
            firstStage.Control.Throttle = 0;
            Console.WriteLine("STAGE 1 : Carbon has landed.");
            
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

                Tuple<Double,Double> impactPoint = Tuple.Create<Double,Double>(Longitude, Latitude);

                Console.WriteLine("Longitude : " + (impactPoint.Item1) + " Latitude : " + (27.3948765184433 - impactPoint.Item2));
                return impactPoint;
            }
            catch (Exception e)
            {
            }
            return null;
        }

        public void LandingTarget()
        {
            //LZ-1 = Logitude : -80.5379863249586 Latitude : 27.3948765184433
            while (firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).MeanAltitude > 200 || firstStage.Flight(firstStage.Orbit.Body.ReferenceFrame).SurfaceAltitude > 200)
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
                    if (trueTWR < 0.8)
                    {
                        if (impactPoint().Item1 > -80.5379863249586)
                        {
                            firstStage.AutoPilot.TargetHeading = 90;
                        }
                        else if (impactPoint().Item1 < -80.5379863249586)
                        {
                            firstStage.AutoPilot.TargetHeading = 270;
                        }

                        if (impactPoint().Item2 > 27.3948765184433)
                        {
                            firstStage.AutoPilot.TargetHeading = 0;
                        }
                        else if (impactPoint().Item2 < 27.3948765184433)
                        {
                            firstStage.AutoPilot.TargetHeading = 180;
                        }
                        firstStage.AutoPilot.TargetPitch = 40;
                    }
                    else
                    {
                        if (impactPoint().Item1 > -80.5379863249586)
                        {
                            firstStage.AutoPilot.TargetHeading = 270;
                        }
                        else if (impactPoint().Item1 < -80.5379863249586)
                        {
                            firstStage.AutoPilot.TargetHeading = 90;
                        }

                        if (impactPoint().Item2 > 27.3948765184433)
                        {
                            firstStage.AutoPilot.TargetHeading = 180;
                        }
                        else if (impactPoint().Item2 < 27.3948765184433)
                        {
                            firstStage.AutoPilot.TargetHeading = 0;
                        }
                        firstStage.AutoPilot.TargetPitch = 85;
                    }
                }
                catch (Exception e)
                {

                }
            }

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
