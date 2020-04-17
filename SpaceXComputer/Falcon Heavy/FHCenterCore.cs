using System;
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

namespace SpaceXComputer
{
    public class FHCenterCore
    {
        public Connection connection;
        public Connection connectionFirstStage;
        public RocketBody rocketBody;
        public Vessel centerCore;
        public Vessel droneShip;

        public Boolean BoostbackBurn = false;


        public FHCenterCore(Vessel vessel, RocketBody rocketBody)
        {
            centerCore = vessel;
            this.rocketBody = rocketBody;
        }

        public void FHStartup(Connection connectionLink)
        {
            connection = connectionLink;
            centerCore.Control.ToggleActionGroup(7);
            centerCore.AutoPilot.Engage();

            centerCore.Control.Throttle = 1;
            centerCore.AutoPilot.TargetPitchAndHeading(90, Startup.GetInstance().GetFlightInfo().getHead());

            //Boosters ignition
            centerCore.Parts.WithTag("MainCentralSB")[0].Engine.Active = true;
            centerCore.Parts.WithTag("MainSecondSB")[0].Engine.Active = true;
            centerCore.Parts.WithTag("MainSecondSB")[1].Engine.Active = true;
            centerCore.Parts.WithTag("MainCentralSB")[1].Engine.Active = true;
            centerCore.Parts.WithTag("MainSecondSB")[2].Engine.Active = true;
            centerCore.Parts.WithTag("MainSecondSB")[3].Engine.Active = true;
            for (int i = 0; i < 12; i++)
            {
                centerCore.Parts.WithTag("MainSB")[i].Engine.Active = true;
            }

            Thread.Sleep(500);

            //Centercore ignition
            centerCore.Parts.WithTag("MainCentral")[0].Engine.ThrustLimit = 0.80f;
            centerCore.Parts.WithTag("MainSecond")[0].Engine.ThrustLimit = 0.8f;
            centerCore.Parts.WithTag("MainSecond")[1].Engine.ThrustLimit = 0.8f;
            centerCore.Parts.WithTag("MainCentral")[0].Engine.Active = true;
            centerCore.Parts.WithTag("MainSecond")[0].Engine.Active = true;
            centerCore.Parts.WithTag("MainSecond")[1].Engine.Active = true;
            for (int i = 0; i < 6; i++)
            {
                centerCore.Parts.WithTag("Main")[i].Engine.ThrustLimit = 0.8f;
                centerCore.Parts.WithTag("Main")[i].Engine.Active = true;
            }


            Console.WriteLine("FIRST STAGE : Main engine startup.");
            float thrust;
            thrust = centerCore.Parts.Engines[1].Thrust;
            Thread.Sleep(2000);

            thrust = centerCore.Thrust;
            if (thrust < 19400)
            {
                Console.WriteLine("FIRST STAGE : Engine Abort.");
                Console.WriteLine("Trust : " + thrust);
                Console.WriteLine("Available Thrust : " + centerCore.AvailableThrust);
                centerCore.Control.Throttle = 0;
                centerCore.Parts.Engines[2].Active = false;
                centerCore.Parts.WithTag("MainCentral")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecond")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecond")[1].Engine.Active = false;
                centerCore.Parts.WithTag("MainCentralSB")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[1].Engine.Active = false;
                centerCore.Parts.WithTag("MainCentralSB")[1].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[2].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[3].Engine.Active = false;
                for (int i = 0; i < 6; i++)
                {
                    centerCore.Parts.WithTag("Main")[i].Engine.Active = false;
                }
                for (int i = 0; i < 12; i++)
                {
                    centerCore.Parts.WithTag("MainSB")[i].Engine.Active = false;
                }
            }
            else if (Startup.GetInstance().GetFlightInfo().getStaticFire() == true)
            {
                Console.WriteLine("Static Fire...");
                Thread.Sleep(7000);
                Console.WriteLine("FIRST STAGE : Static Fire ended.");
                Console.WriteLine("Trust : " + thrust);
                Console.WriteLine("Available Thrust : " + centerCore.AvailableThrust);
                centerCore.Control.Throttle = 0;
                centerCore.Parts.Engines[2].Active = false;
                centerCore.Parts.WithTag("MainCentral")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecond")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecond")[1].Engine.Active = false;
                centerCore.Parts.WithTag("MainCentralSB")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[0].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[1].Engine.Active = false;
                centerCore.Parts.WithTag("MainCentralSB")[1].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[2].Engine.Active = false;
                centerCore.Parts.WithTag("MainSecondSB")[3].Engine.Active = false;
                for (int i = 0; i < 6; i++)
                {
                    centerCore.Parts.WithTag("Main")[i].Engine.Active = false;
                }
                for (int i = 0; i < 12; i++)
                {
                    centerCore.Parts.WithTag("MainSB")[i].Engine.Active = false;
                }
            }
            else
            {
                foreach (LaunchClamp clamp in centerCore.Parts.LaunchClamps)
                {
                    //clamp.Release();
                    centerCore.Control.ToggleActionGroup(8);
                }
                Console.WriteLine("CENTER CORE : Liftoff.");
            }
        }

        public Vessel GetVessel()
        {
            return centerCore;
        }

        public void SetVessel(Vessel vessel)
        {
            centerCore = vessel;
        }
    }
}