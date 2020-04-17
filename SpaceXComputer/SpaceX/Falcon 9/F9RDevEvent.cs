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
    class F9RDevEvent : Event
    {
        protected Connection connection;

        protected F9RDev f9RDev;
        protected GrassHopper grassHopper;

        public F9RDevEvent(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;

            f9RDev = new F9RDev(vessel, RocketBody.F9RDev);
            f9RDev.startup(connection);
        }

        public void Guidage()
        {
            //f9RDev.Guidance(connection);
        }
    }
}
