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
    public class C2SecondStage
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel secondStage;

        public C2SecondStage(Vessel vessel)
        {
            secondStage = vessel;
        }

        public void SecondStageStartup()
        {
            secondStage.AutoPilot.Engage();
            secondStage.AutoPilot.TargetPitchAndHeading(10, Startup.GetInstance().GetFlightInfo().getHead());
            Thread.Sleep(1800);
            secondStage.Control.Throttle = 1;
            secondStage.Parts.Engines[1].Active = true;
            Console.WriteLine("STAGE 2 : Second engine startup.");
        }

        public void fairingSep()
        {
            while (true)
            {
                if (secondStage.Flight(null).MeanAltitude > 60000)
                {
                    secondStage.Control.ToggleActionGroup(5);
                    Console.WriteLine("STAGE 2 : Fairing separation.");
                    break;
                }
            }
        }

        public void SECO()
        {
            while (secondStage.Orbit.ApoapsisAltitude < Startup.GetInstance().GetFlightInfo().getPeriapsisTarget())
            {

            }
            secondStage.Control.Throttle = 0;
            Console.WriteLine("STAGE 2 : Second engine cutoff.");
            SESSECO2();
        }

        public void SESSECO2()
        {
            while (secondStage.Orbit.TimeToApoapsis > 10) { }
            secondStage.Control.Throttle = 1;
            Console.WriteLine("STAGE 2 : Second Engine Startup.");
            var pit = 0;

            while (true)
            {
                if (secondStage.Orbit.ApoapsisAltitude >= Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
                {
                    if (secondStage.Orbit.TimeToApoapsis > 200)
                    {
                        pit = 10;
                    }
                    else
                    {
                        pit = -10;
                    }
                }
                else if (secondStage.Orbit.ApoapsisAltitude <= Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
                {
                    if (secondStage.Orbit.TimeToApoapsis > 20)
                    {
                        pit = -10;
                    }
                    else
                    {
                        pit = 10;
                    }
                }

                if (Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() == Startup.GetInstance().GetFlightInfo().getApoapsisTarget())
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
                    if (secondStage.Orbit.ApoapsisAltitude >= (Startup.GetInstance().GetFlightInfo().getApoapsisTarget() - 2000))
                    {
                        secondStage.Control.Throttle = 0;
                        Console.WriteLine("STAGE 2 : Second Engine CutOff");
                        break;
                    }
                }

                secondStage.AutoPilot.TargetPitchAndHeading(pit, Startup.GetInstance().GetFlightInfo().getHead());
            }


        }

        public void satSep()
        {
            secondStage.AutoPilot.TargetPitch = 90;
            Thread.Sleep(120000);
            secondStage.Parts.Decouplers[1].Decouple();
            Console.WriteLine("STAGE 2 : Satellite deployed.");
            Thread.Sleep(100000000);
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
