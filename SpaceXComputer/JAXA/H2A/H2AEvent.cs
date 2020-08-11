using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.UI;
using System.IO;

namespace SpaceXComputer
{
    class H2AEvent : Event
    {
        public Vessel H2A;
        public Connection connection;

        private double initSurfaceAlt;
        private bool T4;
        private bool T0;
        private bool Hold;
        private bool BoosterSep;

        public StreamWriter sw;

        public H2AEvent(Vessel vessel, Connection connectionLink)
        {
            H2A = vessel;
            connection = connectionLink;

            initSurfaceAlt = H2A.Flight(H2A.SurfaceReferenceFrame).SurfaceAltitude;
            T4 = true;
            T0 = false;
            Hold = false;
            BoosterSep = false;

            /*Thread timer = new Thread(CountDown);
            timer.Start();*/

            Console.WriteLine("H2A is in startup.");

            Thread Log = new Thread(LogDoc);
            //Log.Start();

            Thread.Sleep(200000);

            Liftoff();
            while (H2A.Flight(H2A.SurfaceReferenceFrame).MeanAltitude <= 200) { }

            H2A.AutoPilot.TargetRoll = 90;
            while (H2A.Flight(H2A.SurfaceReferenceFrame).MeanAltitude <= 300) { }

            H2A.AutoPilot.TargetPitch = 85;

            Thread GT = new Thread(GravityTurn);
            GT.Start();
            BoosterSeparation();
            Thread Fairing = new Thread(FairingSep);
            Fairing.Start();
            StageSep();
            Thread.Sleep(4000);
            SecondEngineStartup();
            waitGoodPe();
            waitGoodOrbit();
            Thread.Sleep(60000);
            satelliteDeployment();

            while(true) { }
        }

        public void Liftoff()
        {
            while (T4 == false) { }

            Console.WriteLine("HII-A : Ignition.");
            H2A.Parts.WithTag("First")[0].Engine.Active = true;
            H2A.Control.Throttle = 1;
            H2A.AutoPilot.Engage();
            H2A.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHead();
            H2A.AutoPilot.TargetPitch = 90;

            //while (T0 == false) { }
            Thread.Sleep(12000);

            if (H2A.Thrust <= 80000) //un 0 en plus
            {
                Console.WriteLine("HII-A : Engine Abort.");
                Console.WriteLine("Trust : " + H2A.Thrust);
                Console.WriteLine("Available Thrust : " + H2A.Parts.WithTag("First")[0].Engine.AvailableThrust);
                H2A.Control.Throttle = 0;
                H2A.Parts.WithTag("First")[0].Engine.Active = false;
                Hold = true;
            }
            else
            {
                try
                {
                    H2A.Parts.WithTag("Booster")[0].Engine.Active = true;
                    H2A.Parts.WithTag("Booster")[1].Engine.Active = true;
                    H2A.Parts.WithTag("Booster")[2].Engine.Active = true;
                    H2A.Parts.WithTag("Booster")[3].Engine.Active = true; 
                }
                catch
                {
                    H2A.Control.Throttle = 0;
                    Console.WriteLine("HII-A : Booster Ignition Failed, Launch is abort.");
                    Hold = true;
                    while (true) { }
                }

                foreach (LaunchClamp clamp in H2A.Parts.LaunchClamps)
                {
                    clamp.Release();
                }
                Console.WriteLine("HII-A : Liftoff.");
                //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Liftoff");
            }
        }

        public void GravityTurn()
        {
            double BoosterMass = 21000;
            bool Booster = false;
            double FirstTWR = H2A.Parts.WithTag("First")[0].Engine.AvailableThrust / H2A.Mass;

            //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Gravity turn start");

            while (true)
            {
                double Thrust = H2A.Parts.WithTag("First")[0].Engine.AvailableThrust;
                double Mass = H2A.Mass - BoosterMass * 2;
                double TWR = Thrust / Mass;

                double Pitch = (TWR * 85) / FirstTWR;
                Pitch = 90 - ((Pitch - 90) / 2.3);

                if (BoosterSep == true && Booster == false)
                {
                    Thread.Sleep(1000);
                    BoosterMass = 0;
                    Booster = true;
                }

                H2A.AutoPilot.TargetPitch = Convert.ToSingle(Pitch);
                H2A.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHead();
                H2A.AutoPilot.TargetRoll = 0;

                if (H2A.AutoPilot.TargetPitch <= 35)
                {
                    H2A.AutoPilot.TargetPitch = 35;
                    break;
                }
            }

            H2A.AutoPilot.TargetPitch = 40;
            Console.WriteLine("HII-A : Gravity Turn ended.");
            //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Gravity turn end");
        }

        public void BoosterSeparation()
        {
            //double TWR = H2A.Thrust / H2A.Mass;
            while (H2A.Parts.WithTag("Booster")[0].Engine.Thrust >= 300)
            {
                //TWR = H2A.Thrust / H2A.Mass;
            }

            Thread.Sleep(2000);
            H2A.Control.ToggleActionGroup(4);
            BoosterSep = true;
            Console.WriteLine("HII-A : Booster Separation.");
            //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Booster separation");
        }

        public void FairingSep()
        {
            while (H2A.Flight(H2A.SurfaceReferenceFrame).MeanAltitude <= 100000) { }
            H2A.Control.ToggleActionGroup(5);
            Console.WriteLine("HII-A : Fairing Separation.");
            //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Fairing Separation");
        }

        public void StageSep()
        {
            while (H2A.Thrust >= 150) { }
            H2A.Control.Throttle = 0;
            H2A.Parts.WithTag("Decoupler")[0].Decoupler.Decouple();
            
            Console.WriteLine("HII-A : First Stage Separation.");
            //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : First stage separation");
        }

        public void SecondEngineStartup()
        {
            H2A.Parts.WithTag("Second")[0].Engine.Active = true;
            H2A.Control.Throttle = 1;
            Console.WriteLine("HII-A : Second Engine Startup.");
            //sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Second engine startup");

            H2A.Control.ToggleActionGroup(6);
        }

        public void waitGoodPe()
        {
            while (H2A.Orbit.ApoapsisAltitude < Startup.GetInstance().GetFlightInfo().getPeriapsisTarget()) { H2A.AutoPilot.TargetPitchAndHeading(25, Startup.GetInstance().GetFlightInfo().getHead()); }

            H2A.Control.Throttle = 0;
            Console.WriteLine("HII-A : Second Engine Cutoff.");
        }

        public void waitGoodOrbit()
        {
            while (H2A.Orbit.TimeToApoapsis > 30 && H2A.Orbit.TimeToApoapsis < 50) { H2A.AutoPilot.TargetPitchAndHeading(0, Startup.GetInstance().GetFlightInfo().getHead()); H2A.Control.RCS = true; H2A.Control.Forward = 1; }

            while (H2A.Orbit.TimeToApoapsis > 30) { H2A.AutoPilot.TargetPitchAndHeading(0, Startup.GetInstance().GetFlightInfo().getHead()); }

            H2A.Control.Throttle = 1;
            Console.WriteLine("HII-A : Second Engine Startup 2.");

            H2A.Control.Forward = 0;

            while (H2A.Orbit.ApoapsisAltitude < Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
            {
                H2A.Control.Throttle = 1;
                H2A.AutoPilot.TargetPitchAndHeading(0, Startup.GetInstance().GetFlightInfo().getHead());
            }

            H2A.Control.Throttle = 0;
            Console.WriteLine("HII-A : Second Engine Cutoff 2.");
        }

        public void satelliteDeployment()
        {
            H2A.Parts.WithTag("SatSep")[0].Decoupler.Decouple();
            Console.WriteLine("HII-A : Satellite Deployed.");
        }

        public void CountDown()
        {
            var canvas = connection.UI().StockCanvas;
            var screenSize = canvas.RectTransform.Size;
            var panel = canvas.AddPanel();
            var rect = panel.RectTransform;
            rect.Size = Tuple.Create(300.0 * 1, 150.0 * 1);
            rect.Position = Tuple.Create(Convert.ToDouble(screenSize.Item1) / 2 - 200 * 2.3, Convert.ToDouble(screenSize.Item2) / 2 - 80 * 2.3);

            float remeningHour = Startup.GetInstance().GetFlightInfo().getRemeningHour();
            float remeningMinute = Startup.GetInstance().GetFlightInfo().getRemeningMinute();
            float remeningSecond = Startup.GetInstance().GetFlightInfo().getRemeningSecond();

            var time = panel.AddText($"T-{remeningHour}:{remeningMinute}:{remeningSecond}");
            time.RectTransform.Anchor = Tuple.Create(0.7, 0.23);
            time.RectTransform.Position = Tuple.Create(0.0, 0.0);
            time.RectTransform.Size = Tuple.Create(350.0 * 1, 200.0 * 1);
            time.Color = Tuple.Create(0.0, 0.0, 0.0);
            time.Size = 40;
            time.Visible = true;
            var alt = panel.AddText($"{Math.Round(H2A.Flight(H2A.SurfaceReferenceFrame).SurfaceAltitude - initSurfaceAlt)}m");
            alt.RectTransform.Anchor = Tuple.Create(0.7, -0.00);
            alt.RectTransform.Position = Tuple.Create(0.0, 0.0);
            alt.RectTransform.Size = Tuple.Create(350.0 * 1, 200.0 * 1);
            alt.Color = Tuple.Create(0.0, 0.0, 0.0);
            alt.Size = 40;
            alt.Visible = true;
            var speed = panel.AddText($"{Math.Round(H2A.Flight(H2A.SurfaceReferenceFrame).TrueAirSpeed)}m/s");
            speed.RectTransform.Anchor = Tuple.Create(0.7, -0.23);
            speed.RectTransform.Position = Tuple.Create(0.0, 0.0);
            speed.RectTransform.Size = Tuple.Create(350.0 * 1, 200.0 * 1);
            speed.Color = Tuple.Create(0.0, 0.0, 0.0);
            speed.Size = 40;
            speed.Visible = true;

            Console.WriteLine($"T-{remeningHour}:{remeningMinute}:{remeningSecond}");
            //Console.WriteLine(initSurfaceAlt);
            //Console.WriteLine(H2A.Flight(H2A.SurfaceReferenceFrame).SurfaceAltitude);

            while (T0 == false)
            {
                Thread.Sleep(1000);

                alt.Content = $"{Math.Round(H2A.Flight(H2A.SurfaceReferenceFrame).SurfaceAltitude - initSurfaceAlt)}m";
                speed.Content = $"{Math.Round(H2A.Flight(H2A.SurfaceReferenceFrame).TrueAirSpeed)}m/s";

                if (Hold == false)
                {
                    remeningSecond -= 1;

                    if (remeningSecond < 0)
                    {
                        remeningSecond = 59;
                        remeningMinute -= 1;

                        if (remeningMinute < 0)
                        {
                            remeningMinute = 59;
                            remeningHour -= 1;
                        }
                    }

                    if (remeningHour < 1)
                    {
                        if (remeningMinute < 10)
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T-0{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T-0{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T-0{remeningMinute}:{remeningSecond}");
                                time.Content = $"T-0{remeningMinute}:{remeningSecond}";
                            }
                        }
                        else
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T-{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T-{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T-{remeningMinute}:{remeningSecond}");
                                time.Content = $"T-{remeningMinute}:{remeningSecond}";
                            }
                        }
                    }
                    else
                    {
                        if (remeningMinute < 10)
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T-{remeningHour}:0{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T-{remeningHour}:0{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T-{remeningHour}:0{remeningMinute}:{remeningSecond}");
                                time.Content = $"T-{remeningHour}:0{remeningMinute}:{remeningSecond}";
                            }
                        }
                        else
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T-{remeningHour}:{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T-{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T-{remeningHour}:{remeningMinute}:{remeningSecond}");
                                time.Content = $"T-{remeningHour}:{remeningMinute}:{remeningSecond}";
                            }
                        }
                    }

                    if (remeningHour == 0 && remeningMinute == 0 && remeningSecond < 8)
                    {
                        T4 = true;
                    }

                    if (remeningHour < 0)
                    {
                        T0 = true;
                        break;
                    }
                }
            }

            float second = 0;
            float minute = 0;
            float hour = 0;
            remeningHour = hour;
            remeningMinute = minute;
            remeningSecond = second;

            Console.WriteLine($"T+0{remeningMinute}:0{remeningSecond}");
            time.Content = $"T+0{remeningMinute}:0{remeningSecond}";

            while (true)
            {
                Thread.Sleep(1000);

                alt.Content = $"{Math.Round(H2A.Flight(H2A.SurfaceReferenceFrame).SurfaceAltitude - initSurfaceAlt)}m";
                speed.Content = $"{Math.Round(H2A.Flight(H2A.SurfaceReferenceFrame).TrueAirSpeed)}m/s";

                if (Hold == false)
                {
                    remeningSecond += 1;

                    if (remeningSecond > 59)
                    {
                        remeningSecond = 0;
                        remeningMinute += 1;

                        if (remeningMinute > 59)
                        {
                            remeningMinute = 0;
                            remeningHour += 1;
                        }
                    }

                    if (remeningHour < 1)
                    {
                        if (remeningMinute < 10)
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T+0{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T+0{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T+0{remeningMinute}:{remeningSecond}");
                                time.Content = $"T+0{remeningMinute}:{remeningSecond}";
                            }
                        }
                        else
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T+{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T+{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T+{remeningMinute}:{remeningSecond}");
                                time.Content = $"T+{remeningMinute}:{remeningSecond}";
                            }
                        }
                    }
                    else
                    {
                        if (remeningMinute < 10)
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T+{remeningHour}:0{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T+{remeningHour}:0{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T+{remeningHour}:0{remeningMinute}:{remeningSecond}");
                                time.Content = $"T+{remeningHour}:0{remeningMinute}:{remeningSecond}";
                            }
                        }
                        else
                        {
                            if (remeningSecond < 10)
                            {
                                //Console.WriteLine($"T+{remeningHour}:{remeningMinute}:0{remeningSecond}");
                                time.Content = $"T+{remeningMinute}:0{remeningSecond}";
                            }
                            else
                            {
                                //Console.WriteLine($"T+{remeningHour}:{remeningMinute}:{remeningSecond}");
                                time.Content = $"T+{remeningHour}:{remeningMinute}:{remeningSecond}";
                            }
                        }
                    }
                }
            }
        }

        public void LogDoc()
        {
            //StreamWriter Log = new StreamWriter(@"C:\Users\Utilisateur\Documents\Log.txt", true);
            //Log.WriteLine("Ca marche ?");
            Thread Rec = new Thread(FlightRecord);
            Rec.Start();

            using (sw = new StreamWriter($@"C:\Users\Utilisateur\Documents\KSPRP\Flight\JAXA\H2A\H2Log_{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.txt", true))
            {
                sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : HII-2 startup.");
                while (T4 == false) { }
                while (true)
                {
                    Thread.Sleep(250);
                    sw.WriteLine($"[{Hour(connection.SpaceCenter().UT)}] : Speed = {H2A.Flight(H2A.SurfaceReferenceFrame).TrueAirSpeed}m/s | Altitude = {H2A.Flight(H2A.SurfaceReferenceFrame).MeanAltitude}m");
                }
            }
        }

        public void FlightRecord()
        {
            using (StreamWriter Record = new StreamWriter($@"C:\Users\Utilisateur\Documents\KSPRP\Flight\JAXA\H2A\H2Record_{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.csv", true))
            {
                while (T4 == false) { }
                Record.WriteLine("'Time', 'Speed (m/s)', 'Altitude (m)', 'TWR', 'Pitch'");
                
                while (true)
                {
                    Thread.Sleep(250);
                    Record.WriteLine($"'{Hour(connection.SpaceCenter().UT)}', '{H2A.Flight(H2A.SurfaceReferenceFrame).TrueAirSpeed}', '{H2A.Flight(H2A.SurfaceReferenceFrame).MeanAltitude}', '{H2A.Thrust / H2A.Mass}', '{H2A.AutoPilot.TargetPitch}'");
                }
            }
        }

        public static string Hour(double uT)
        {
            return $"{uT}";
        }
    }
}
