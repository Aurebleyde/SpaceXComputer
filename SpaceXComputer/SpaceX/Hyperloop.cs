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
using KRPC.Client.Services.UI;
//using UnityEngine;
//using UnityEngineInternal;
//using Trajectories;
using systemAlias = global::System;
using Google.Protobuf;
using KRPC.Client.Services.Trajectories;

namespace SpaceXComputer
{
    class Hyperloop : Event
    {
        public Connection connection;
        public Vessel hyperLoop;

        public Hyperloop(Vessel vessel, Connection connection)
        {
            hyperLoop = vessel;
            Console.WriteLine("Hyperloop startup.");
            hyperLoop.Control.Brakes = true;
            HUD(connection);
        }

        public void HUD(Connection connection)
        {
            var conn = new Connection("HUD");
            var canvas = conn.UI().StockCanvas;

            var screenSize = canvas.RectTransform.Size;

            var panel = canvas.AddPanel();

            var rect = panel.RectTransform;
            rect.Size = Tuple.Create(600.0, 250.0);
            rect.Position = Tuple.Create(Convert.ToDouble(screenSize.Item1)/2 - 350, Convert.ToDouble(screenSize.Item2)/2-175);

            var speed = panel.AddText("Speed : 0 km/h");
            speed.RectTransform.Position = Tuple.Create(-200.0, 62.5);
            speed.Color = Tuple.Create(1.0, 1.0, 1.0);
            speed.Size = 25;

            var mach = panel.AddText("Mach : 0");
            mach.RectTransform.Position = Tuple.Create(-200.0, -62.5);
            mach.Color = Tuple.Create(1.0, 1.0, 1.0);
            mach.Size = 10;

            while (true)
            {
                speed.Content = "Speed : " + (Math.Round(hyperLoop.Flight(hyperLoop.SurfaceReferenceFrame).TrueAirSpeed) * 3.6) + " km/h";
                mach.Content = "Mach : " + (Math.Round(hyperLoop.Flight(hyperLoop.SurfaceReferenceFrame).TrueAirSpeed) / hyperLoop.Flight(hyperLoop.SurfaceReferenceFrame).SpeedOfSound);

                Thread.Sleep(100);
            }
        }

        public Vessel GetVessel()
        {
            return hyperLoop;
        }

        public void SetVessel(Vessel vessel)
        {
            hyperLoop = vessel;
        }
    }
}
