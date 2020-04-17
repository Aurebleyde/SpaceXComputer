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
    public class C2ThirdStage
    {
        public Connection connection;
        public RocketBody rocketBody;
        public Vessel thirdStage;

        public C2ThirdStage(Vessel vessel)
        {
            thirdStage = vessel;
        }



        public Vessel GetVessel()
        {
            return thirdStage;
        }

        public void SetVessel(Vessel vessel)
        {
            thirdStage = vessel;
        }
    }
}
