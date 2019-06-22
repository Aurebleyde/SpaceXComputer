using System;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer2
{
    public class Starship
    {
        public Launcher launcher;

        private Connection connection;
        public Vessel starship;
        public Starship(Vessel vessel)
        {
            starship = vessel;
        }

        private StarshipHopper starshipHopper;

        public void Launch(Connection connection, Vessel vessel)
        {
            Console.WriteLine("Starship is in startup.");

            this.connection = connection;

            //if (launcher.getVersion() == "Hopper")
            //{
                starshipHopper = new StarshipHopper(vessel);
                starshipHopper.starshipHopper = connection.SpaceCenter().ActiveVessel;
                starshipHopper.Hopp();
            //}
        }
    }
}
