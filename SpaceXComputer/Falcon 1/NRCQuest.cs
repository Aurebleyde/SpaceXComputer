using System;
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
    public class NRCQuest
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel nrcQuest;

        public NRCQuest(Vessel vessel, RocketBody rocketBody)
        {
            nrcQuest = vessel;
            this.rocketBody = rocketBody;
        }

        public Tuple<Double, Double> positionNRCQuest()
        {
            var refFrame = nrcQuest.SurfaceReferenceFrame;
            double Longitude = nrcQuest.Flight(refFrame).Longitude;
            double Latitude = nrcQuest.Flight(refFrame).Latitude;

            Tuple<Double, Double> positionNRCQuest = Tuple.Create<Double, Double>(Latitude, Longitude);
            return positionNRCQuest;
        }
    }
}
