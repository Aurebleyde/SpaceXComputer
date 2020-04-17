using System;
using System.Threading;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class StarshipHopperEvent : Event
    {
        protected Connection connection;
        protected StarshipHopper starship;

        public StarshipHopperEvent(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            starship = new StarshipHopper(vessel, RocketBody.F9_FIRST_STAGE);

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (Startup.GetInstance().GetFlightInfo().getDragon() == false)
                {
                    if (vesselTarget.Name.Equals("Starship Hopper") && vesselTarget.Type.Equals(VesselType.Probe))
                    {
                        starship.starship = vesselTarget;
                        starship.starship.Name = "Starship Hopper";
                        Console.WriteLine("STARSHIP : Starship Hopper accisition signal.");
                        break;
                    }
                }
            }

            starship.StarshipStartup(connection);

            Console.ReadKey();
        }
    }
}
