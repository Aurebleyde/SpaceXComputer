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
    public class OCISLY
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel droneShip;

        public OCISLY(Vessel vessel, RocketBody rocketBody)
        {
            droneShip = vessel;
            this.rocketBody = rocketBody;
        }

        public Tuple<Double, Double> positionOCISLY()
        {
            var refFrame = droneShip.SurfaceReferenceFrame;
            double Longitude = droneShip.Flight(refFrame).Longitude;
            double Latitude = droneShip.Flight(refFrame).Latitude;

            return Tuple.Create<Double, Double>(Latitude, Longitude);
        }

        /*public Vessel GetVessel()
        {
            return droneShip;
        }

        public void SetVessel(Vessel vessel)
        {
            droneShip = vessel;
        }*/
    }
}
