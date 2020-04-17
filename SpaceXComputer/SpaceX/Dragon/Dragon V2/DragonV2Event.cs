using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class DragonV2Event : Event
    {
        protected Connection connection;

        protected DragonV2 dragonV2;

        public DragonV2Event(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            dragonV2 = new DragonV2(vessel, RocketBody.DRAGONV2);
            var Dragon = dragonV2;

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Equals("[SpaceX] DragonV2") && vesselTarget.Type.Equals(VesselType.Ship))
                {
                    dragonV2.dragonV2 = vesselTarget;
                    dragonV2.dragonV2.Name = "[SpaceX] DragonV2";
                    Console.WriteLine("DRAGON V2 : DragonV2 accisition signal.");
                    break;
                }
            }

            dragonV2.abortEngage();
        }
    }
}
