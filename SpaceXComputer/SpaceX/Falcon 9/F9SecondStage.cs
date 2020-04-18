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

namespace SpaceXComputer
{
    public class F9SecondStage
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel secondStage;
        public Vessel satTarget;

        public F9SecondStage(Vessel vessel, RocketBody rocketBody)
        {
            secondStage = vessel;
            this.rocketBody = rocketBody;
        }

        public void SecondStageStartup()
        {
            secondStage.Control.RCS = true;
            secondStage.AutoPilot.Engage();
            secondStage.AutoPilot.TargetRoll = 0;
            Thread.Sleep(4000);
            secondStage.AutoPilot.TargetPitchAndHeading(35, Startup.GetInstance().GetFlightInfo().getHead());
            Thread.Sleep(4000);
            secondStage.Control.Throttle = 0.01f;
            //secondStage.Parts.Engines[0].Active = true;
            secondStage.Parts.WithTag("Second")[0].Engine.Active = true;
            Thread.Sleep(10000);
            secondStage.Control.Throttle = 0.3f;
            Thread.Sleep(5000);
            secondStage.Control.Throttle = 1;
            secondStage.Control.Forward = 0;
            Console.WriteLine("STAGE 2 : Second engine startup.");
        }

        public void fairingSep()
        {
            while (true)
            {
                if (Startup.GetInstance().GetFlightInfo().getOrion() == true)
                {
                    if (secondStage.Flight(null).MeanAltitude > 60000)
                    {
                        secondStage.Control.ToggleActionGroup(5);
                        Console.WriteLine("STAGE 2 : Solar protection separation.");
                        Thread.Sleep(5000);
                        secondStage.Control.ToggleActionGroup(9);
                        Console.WriteLine("STAGE 2 : LES Jettison.");
                        break;
                    }
                }
                else if (Startup.GetInstance().GetFlightInfo().getDragon() == false)
                {
                    if (secondStage.Flight(null).MeanAltitude > 90000)
                    {
                        secondStage.Control.ToggleActionGroup(5);
                        Console.WriteLine("STAGE 2 : Fairing separation.");
                        break;
                    }
                }
                else
                {
                    if (secondStage.Flight(null).MeanAltitude > 45000)
                    {
                        secondStage.Control.ToggleActionGroup(5);
                        Console.WriteLine("STAGE 2 : Nose cone sep.");
                        break;
                    }
                }
            }
        }

        public void SECO(Vessel vessel, Connection connection)
        {
            while (true)
            {
                secondStage.AutoPilot.TargetPitch = 35;
                secondStage.Control.Throttle = 1;
                if (secondStage.Orbit.ApoapsisAltitude >= Startup.GetInstance().GetFlightInfo().getPeriapsisTarget())
                {
                    secondStage.Control.Throttle = 0;
                    Console.WriteLine("STAGE 2 : Second engine cutoff.");
                    SESSECO2();
                    //circularisation(vessel, connection);
                    break;
                }
            }
        }

        public void SESSECO2()
        {
            secondStage.AutoPilot.TargetPitchAndHeading(0, 47);
            while (secondStage.Orbit.TimeToApoapsis > 50) { }
            secondStage.Control.RCS = true;
            secondStage.Control.Forward = 1;
            while (secondStage.Orbit.TimeToApoapsis > 30) { }
            secondStage.Control.Throttle = 1;
            Console.WriteLine("STAGE 2 : Second Engine Startup.");
            var pit = 0;
            float throttle = 1;

            Thread.Sleep(10000);

            while (true)
            {
                float head = 47;

                if (Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() != Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
                {

                }
                else
                {
                    if (secondStage.Orbit.ApoapsisAltitude >= Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
                    {
                        if (secondStage.Orbit.TimeToApoapsis > 200)
                        {
                            pit = -5;
                        }
                        else
                        {
                            pit = 5;
                        }
                    }
                    else if (secondStage.Orbit.ApoapsisAltitude <= Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
                    {
                        if (secondStage.Orbit.TimeToApoapsis > 20)
                        {
                            pit = 5;
                        }
                        else
                        {
                            pit = -5;
                        }
                    }

                    if (secondStage.Orbit.PeriapsisAltitude < 0)
                    {
                        throttle = 1;
                    }
                    else
                    {
                        var Peri = Convert.ToSingle(secondStage.Orbit.PeriapsisAltitude);

                        throttle = Convert.ToSingle(((1 * Startup.GetInstance().GetFlightInfo().getPeriapsisTarget()) / Peri) / 10);
                    }

                    secondStage.AutoPilot.TargetPitchAndHeading(pit, head);
                    secondStage.Control.Throttle = throttle + 0.1f;
                }

                secondStage.Control.Throttle = throttle;

                if (Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() == Startup.GetInstance().GetFlightInfo().getApoapsisTarget() || secondStage.Thrust == 0)
                {
                    if (secondStage.Orbit.PeriapsisAltitude >= Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() - 2000)
                    {
                        secondStage.Control.Throttle = 0;
                        Console.WriteLine("STAGE 2 : Second Engine CutOff");
                        break;
                    }
                }
                else
                {
                    if (secondStage.Orbit.ApoapsisAltitude >= (Startup.GetInstance().GetFlightInfo().getApoapsisTarget() - 2000) || secondStage.Thrust == 0/* && secondStage.Orbit.PeriapsisAltitude > Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() - 70000*/)
                    {
                        secondStage.Control.Throttle = 0;
                        Console.WriteLine("STAGE 2 : Second Engine CutOff");
                        break;
                    }
                }

                secondStage.AutoPilot.TargetPitchAndHeading(pit, Startup.GetInstance().GetFlightInfo().getHead());
            }

            secondStage.Control.Forward = 0;
        }

        public void circularisation(Vessel vessel, Connection connection)
        {
            var ut = connection.AddStream(() => connection.SpaceCenter().UT);
            secondStage.AutoPilot.Engage();

            /*foreach (Vessel v in connection.SpaceCenter().Vessels)
            {
                if (v.Name == "Polar Constellation 1")
                {
                    connection.SpaceCenter().TargetVessel = v;
                }
            }*/



            // --- Correction d'inclinaison au niveau de l'équateur.
            /*var longitude = connection.AddStream(() => secondStage.Flight(secondStage.SurfaceReferenceFrame).Longitude);
            var utn = connection.SpaceCenter().UT + 10f;
            while (secondStage.Orbit.Body.LatitudeAtPosition(secondStage.Orbit.PositionAt(utn, secondStage.SurfaceReferenceFrame), secondStage.SurfaceReferenceFrame) > 1)
            {
                utn = utn + 10;
            }
            var ut_an = utn;

            var nodeSpeed = secondStage.Orbit.OrbitalSpeedAt(ut_an);
            var orbit = secondStage.Orbit;
            double i = Startup.GetInstance().GetFlightInfo().getInclination() - secondStage.Orbit.Inclination;
            double v = nodeSpeed;
            //double v = orbit.Speed;
            double normal = v * Math.Sin(i);
            double prograde = v * Math.Cos(i) - v;
            var ut_node = connection.SpaceCenter().UT + ut_an;
            var deltaV = normal + prograde;
            //var deltaV = 2 * nodeSpeed * Math.Sin((i2 - i1) / 2);

            var node = secondStage.Control.AddNode(ut_node, normal: Convert.ToSingle(normal), prograde: Convert.ToSingle(prograde));

            var ISP = secondStage.Parts.WithTag("Second")[0].Engine.SpecificImpulse;
            var P = secondStage.Parts.WithTag("Second")[0].Engine.AvailableThrust;
            var F = P * 4;
            var Isp = ISP * 9.82;
            var m0 = secondStage.Mass;
            var m1 = m0 / Math.Exp(deltaV / Isp);
            var flowRate = F / Isp;
            var burnTime = (m0 - m1) / flowRate;

            secondStage.AutoPilot.ReferenceFrame = node.ReferenceFrame;
            secondStage.AutoPilot.TargetDirection = Tuple.Create(0.0, 1.0, 0.0);
            secondStage.AutoPilot.Wait();

            /*Console.WriteLine("Ready for burn ?");
            while (Console.ReadLine() != "true") { }*/

            /*while (ut_an - ut.Get() - (Math.Abs(burnTime) / 2.0) > 2)
            {
            }
            
            secondStage.Control.Throttle = 1;
            Console.WriteLine("STAGE 2 : SES-2.");

            System.Threading.Thread.Sleep((int)((Math.Abs(burnTime) - 0.1) * 1000));

            if (secondStage.Orbit.LongitudeOfAscendingNode <= 0)
            {
                while (secondStage.Orbit.LongitudeOfAscendingNode <= 0 - 1) { }
            }
            else
            {
                while (secondStage.Orbit.LongitudeOfAscendingNode >= 0 + 1) { }
            }

            node.Remove();*/

            var mu = secondStage.Orbit.Body.GravitationalParameter;
            var r = secondStage.Orbit.Apoapsis;
            var a1 = secondStage.Orbit.SemiMajorAxis;
            var SMA = ((Startup.GetInstance().GetFlightInfo().getApoapsisTarget() + 600000) + (Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() + 600000)) / 2;
            var a2 = SMA;
            var v1 = Math.Sqrt(mu * ((2.0 / r) - (1.0 / a1)));
            var v2 = Math.Sqrt(mu * ((2.0 / r) - (1.0 / a2)));
            var deltaV = v2 - v1;
            var node = secondStage.Control.AddNode(ut.Get() + secondStage.Orbit.TimeToApoapsis, prograde: (float)deltaV);

            var F = secondStage.AvailableThrust;
            var Isp = secondStage.SpecificImpulse * 9.82;
            var m0 = secondStage.Mass;
            var m1 = m0 / Math.Exp(deltaV / Isp);
            var flowRate = F / Isp;
            var burnTime = (m0 - m1) / flowRate;

            secondStage.AutoPilot.ReferenceFrame = node.ReferenceFrame;
            secondStage.AutoPilot.TargetDirection = Tuple.Create(0.0, 1.0, 0.0);
            //secondStage.AutoPilot.Wait();

            var timeToApoapsis = connection.AddStream(() => secondStage.Orbit.TimeToApoapsis);
            while (timeToApoapsis.Get() - (burnTime / 2.0) > 0)
            {
            }
            secondStage.Control.Throttle = 1;
            Console.WriteLine("STAGE 2 : SES-2.");
            System.Threading.Thread.Sleep((int)((burnTime - 0.1) * 1000));
            //secondStage.Control.Throttle = 0.3f;
            /*var remainingBurn = connection.AddStream(() => node.RemainingBurnVector(node.ReferenceFrame));
            while (remainingBurn.Get().Item1 > 0)
            {
            }*/
            if (Startup.GetInstance().GetFlightInfo().getApoapsisTarget() == Startup.GetInstance().GetFlightInfo().getPeriapsisTarget())
            {
                while (secondStage.Orbit.PeriapsisAltitude <= Startup.GetInstance().GetFlightInfo().getApoapsisTarget() - 500) { }
            }
            else
            {
                while (secondStage.Orbit.ApoapsisAltitude <= Startup.GetInstance().GetFlightInfo().getApoapsisTarget() - 500) { }
            }
            secondStage.Control.Throttle = 0;
            Console.WriteLine("STAGE 2 : SECO-2.");
            node.Remove();
        }

        public void satSep()
        {
            secondStage.AutoPilot.Engage();
            secondStage.AutoPilot.TargetPitch = 0;
            Thread.Sleep(120000);
            //secondStage.Parts.Decouplers[0].Decouple();
            secondStage.Parts.WithTag("SatSep")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload deployed.");
            Thread.Sleep(10000);
            Console.WriteLine("ENDFLIGHT FOR FALCON 9 SECOND STAGE.");
        }

        public void grapSep()
        {
            secondStage.AutoPilot.Engage();
            secondStage.AutoPilot.TargetPitch = 90;
            secondStage.AutoPilot.TargetRoll = 0;
            Thread.Sleep(120000);
            secondStage.Parts.WithTag("Grap 1")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 1 deployed.");
            //secondStage.Parts.WithTag("Grap 2")[0].Decoupler.Decouple();
            //Console.WriteLine("STAGE 2 : Payload on the Grape 2 deployed.");
            Thread.Sleep(60000);
            secondStage.Parts.WithTag("Grap 3")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 3 deployed.");
            secondStage.Parts.WithTag("Grap 4")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 4 deployed.");
            /*Thread.Sleep(60000);
            secondStage.Parts.WithTag("Grap 5")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 5 deployed.");
            secondStage.Parts.WithTag("Grap 6")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 6 deployed.");
            Thread.Sleep(60000);
            secondStage.Parts.WithTag("Grap 8")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 7 deployed.");
            secondStage.Parts.WithTag("Grap 9")[0].Decoupler.Decouple();
            Console.WriteLine("STAGE 2 : Payload on the Grape 8 deployed.");*/
            Thread.Sleep(60000);

            Console.WriteLine("STAGE 2 : The grap as been correctly deployed.");
            Console.WriteLine("ENDFLIGHT FOR FALCON 9 SECOND STAGE.");
        }

        public void StarlinkSep()
        {
            secondStage.AutoPilot.Engage();
            secondStage.AutoPilot.TargetPitchAndHeading(0, 90 + 55);
            secondStage.AutoPilot.TargetRoll = 0;

            Thread.Sleep(60000);

            secondStage.AutoPilot.Disengage();
            secondStage.Control.Pitch = 1;

            Thread.Sleep(1000);

            secondStage.Control.Pitch = 0;

            Thread.Sleep(10000);

            secondStage.Control.ToggleActionGroup(0);

            secondStage.Control.SAS = true;

            Console.WriteLine("STAGE 2 : Starlink's satellite has been sucessfully deployed.");
            Console.WriteLine("ENDFLIGHT FOR FALCON 9 SECOND STAGE.");
        }

        public Vessel GetVessel()
        {
            return secondStage;
        }

        public void SetVessel(Vessel vessel)
        {
            secondStage = vessel;
        }
    }
}
