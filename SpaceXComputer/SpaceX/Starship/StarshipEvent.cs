using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.Trajectories;

namespace SpaceXComputer
{
    class StarshipEvent : Event
    {
        protected Connection connection;

        protected Starship Starship;

        public StarshipEvent(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;

            Starship = new Starship(vessel, RocketBody.STARSHIP);
            Starship.Startup(connection);
        }
    }
}
