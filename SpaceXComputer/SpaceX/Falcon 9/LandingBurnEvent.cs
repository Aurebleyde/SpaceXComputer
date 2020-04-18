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
    class LandingBurnEvent : Event
    {
        protected Connection connection;
        protected F9FirstStage firstStage;

        public LandingBurnEvent(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            firstStage = new F9FirstStage(vessel, RocketBody.F9_FIRST_STAGE);

            firstStage.firstStage = connection.SpaceCenter().ActiveVessel;

            //firstStage.landingBurn2(connection);
        }
    }
}
