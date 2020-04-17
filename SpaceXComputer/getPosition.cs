using System;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class getPosition : Event
    {

        protected Connection connection;
        protected Vessel vessel;

        public getPosition(Vessel vessel, Connection connection)
        {
            vessel = connection.SpaceCenter().ActiveVessel;

            Console.WriteLine("Lat : " + vessel.Flight(vessel.SurfaceReferenceFrame).Latitude);
            Console.WriteLine("Long : " + vessel.Flight(vessel.SurfaceReferenceFrame).Longitude);

            Console.ReadKey();
        }
    }
}
