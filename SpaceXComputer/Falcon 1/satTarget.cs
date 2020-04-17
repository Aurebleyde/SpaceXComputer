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
    public class SatTarget
    {
        public Connection connection;
        public Vessel satTarget;
        public RocketBody rocketBody;

        public SatTarget(Vessel vessel, RocketBody rocketBody)
        {
            satTarget = vessel;
            this.rocketBody = rocketBody;
        }

        public Tuple<Double, Double> positionSatTarget()
        {
            var refFrame = satTarget.SurfaceReferenceFrame;
            double Longitude = satTarget.Flight(refFrame).Longitude;
            double Latitude = satTarget.Flight(refFrame).Latitude;

            Tuple<Double, Double> positionSatTarget = Tuple.Create<Double, Double>(Latitude, Longitude);
            return positionSatTarget;
        }
    }
}