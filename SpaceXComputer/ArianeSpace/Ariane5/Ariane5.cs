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
//using UnityEngine;
//using UnityEngineInternal;
//using Trajectories;
using systemAlias = global::System;
using Google.Protobuf;
using KRPC.Client.Services.Trajectories;

namespace SpaceXComputer
{
    public class Ariane5
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel ariane5;

        public Ariane5(Vessel vessel, RocketBody rocketBody)
        {
            ariane5 = vessel;
            this.rocketBody = rocketBody;
        }

        public void Ariane5Startup(Connection connectionLink)
        {
            connection = connectionLink;

            ariane5.AutoPilot.Engage();
            ariane5.AutoPilot.TargetPitchAndHeading(90, Startup.GetInstance().GetFlightInfo().getHead());

            ariane5.Parts.WithTag("Vulcain2")[0].Engine.Active = true;

            ariane5.Control.Throttle = 1;
            Console.WriteLine("ARIANE V : Allumage Vulcain.");

            Thread.Sleep(7000);

            var thrust = ariane5.Thrust;
            if (thrust < 2000)
            {
                Console.WriteLine("ARIANE V : Engine Abort.");
                Console.WriteLine("Trust : " + thrust);
                Console.WriteLine("Available Thrust : 2100");
                ariane5.Control.Throttle = 0;
                ariane5.Parts.WithTag("Vulcain2")[0].Engine.Active = false;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    ariane5.Parts.WithTag("EAP")[i].Engine.Active = true;
                }

                foreach (LaunchClamp clamp in ariane5.Parts.LaunchClamps)
                {
                    clamp.Release();
                }
                Console.WriteLine("ARIANE V : Décollage.");
            }

            Thread.Sleep(2000);
            ariane5.AutoPilot.TargetRoll = 270;

            Thread.Sleep(5000);
            ariane5.AutoPilot.TargetPitch = 85;

        }

        public void GravityTurn()
        {
            ariane5.AutoPilot.TargetRoll = 270;
            var Ft = ariane5.Thrust;
            var Fw = ariane5.Mass * ariane5.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var TWRstart = TWR;
            var pit = 85f;

            while (pit > 15)
            {
                Ft = ariane5.Thrust;
                Fw = ariane5.Mass * ariane5.Orbit.Body.SurfaceGravity;
                TWR = Ft / Fw;

                var difSup = ((90 * TWR) / TWRstart);
                double dif = (difSup - 90) / TWRstart;
                float dif2 = Convert.ToSingle(dif);
                pit = 90 - dif2 - 5;
                ariane5.AutoPilot.TargetPitch = pit;
                ariane5.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHead();
                ariane5.AutoPilot.TargetRoll = 270;

                if (ariane5.Orbit.ApoapsisAltitude >= Startup.GetInstance().GetFlightInfo().getPeriapsisTarget())
                {
                    ariane5.AutoPilot.TargetPitch = 0;
                    break;
                }

                var vesselFuel = (connection.AddStream(() => ariane5.Resources.Amount("SolidFuel")));
                if (vesselFuel.Get() < 40)
                {
                    Thread.Sleep(5000);
                    ariane5.AutoPilot.TargetPitch = 15;
                    break;
                }
            }

            ariane5.AutoPilot.TargetPitch = 10;
        }

        public void GravityTurn2()
        {
            ariane5.AutoPilot.TargetRoll = 270;
            var Ft = ariane5.Thrust;
            var Fw = ariane5.Mass * ariane5.Orbit.Body.SurfaceGravity;
            var TWR = Ft / Fw;
            var TWRstart = TWR;
            var pit = 40f;

            while (pit > 15)
            {
                Ft = ariane5.Thrust;
                Fw = ariane5.Mass * ariane5.Orbit.Body.SurfaceGravity;
                TWR = Ft / Fw;

                var difSup = ((35 * TWR) / TWRstart);
                double dif = (difSup - 35) / TWRstart;
                float dif2 = Convert.ToSingle(dif);
                pit = 90 - dif2;
                ariane5.AutoPilot.TargetPitch = pit;
                ariane5.AutoPilot.TargetHeading = Startup.GetInstance().GetFlightInfo().getHead();
                ariane5.AutoPilot.TargetRoll = 270;

                if (ariane5.Orbit.ApoapsisAltitude >= Startup.GetInstance().GetFlightInfo().getPeriapsisTarget())
                {
                    ariane5.AutoPilot.TargetPitch = 0;
                    break;
                }
            }

            ariane5.AutoPilot.TargetPitch = 10;
        }

        public void EAPSep()
        {
            while (true)
            {
                var vesselFuel = (connection.AddStream(() => ariane5.Resources.Amount("SolidFuel")));
                if (vesselFuel.Get() < 40)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        ariane5.Parts.WithTag("EAPsep")[i].Engine.Active = true;
                    }

                    ariane5.Parts.WithTag("EAPsepA")[0].Decoupler.Decouple();
                    ariane5.Parts.WithTag("EAPsepB")[0].Decoupler.Decouple();
                    Console.Write("ARIANE V : Séparation des EAP.");
                    break;
                }
            }
        }

        public void EPCSep()
        {
            while (true)
            {
                var vesselFuel = (connection.AddStream(() => ariane5.Resources.Amount("LqdHydrogen")));
                if (vesselFuel.Get() - 40270 < 1)
                {
                    Thread.Sleep(7000);
                    ariane5.Parts.WithTag("EPCsep")[0].Decoupler.Decouple();
                    Console.Write("ARIANE V : Séparation de l'EPC.");
                    break;
                }
            }
        }

        public void CoiffeSep()
        {
            while (ariane5.Flight(ariane5.SurfaceReferenceFrame).SurfaceAltitude < 65000) { Thread.Sleep(1000); }

            ariane5.Control.ToggleActionGroup(5);
            Console.WriteLine("ARIANE V : Larguage de la coiffe.");
        }

        public void ESCstartup()
        {
            ariane5.AutoPilot.TargetPitch = 0;
            Thread.Sleep(7000);
            ariane5.Parts.WithTag("ESC")[0].Engine.Active = true;
            Console.WriteLine("ARIANE V : Allumage ESC.");
        }

        public Vessel GetVessel()
        {
            return ariane5;
        }

        public void SetVessel(Vessel vessel)
        {
            ariane5 = vessel;
        }
    }
}
