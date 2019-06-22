using System;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer2
{
    public class Startup
    {
        Event TuMeSaoul;

        public Launcher Launcher;
        private Starship starship;

        public Connection connection;

        static void Main()
        {
            new Startup();
        }

        public Startup()
        {
            connection = new Connection(address: IPAddress.Parse("127.0.0.1"), rpcPort: 50000, streamPort: 50001);
            Console.WriteLine("Program is in startup");

            var krpc = connection.KRPC();
            var spaceCenter = connection.SpaceCenter();
            starship = new Starship(spaceCenter.ActiveVessel);
            starship.Launch(connection, spaceCenter.ActiveVessel);
            //if (Launcher.getRocket() == "Starship") { Starship.Launch(connection, spaceCenter.ActiveVessel); }
        }
    }
}
