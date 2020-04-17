using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    public class Startup
    {
        Event spacexEvent;
        public FlightInfo flightInfo;

        public Connection connection;
        public Connection connectionFirstStage;
        protected static Startup instance;

        public static void Main()
        {
            new Startup();
        }

        public Startup()
        {
            CommandFire.FireRegistery();

            instance = this;
            flightInfo = new FlightInfo();
            //connection = new Connection(address: IPAddress.Parse("127.0.0.1"), rpcPort: 50000, streamPort: 50001); // Tester ::1
            Console.WriteLine("Falcon is in startup");
            connection = new Connection(address: IPAddress.Parse("192.168.1.88"), rpcPort: 60000, streamPort: 60001);

            //connectionFirstStage = new Connection(address: IPAddress.Parse("192.168.1.88"), rpcPort: 50000, streamPort: 50001);

            connectionFirstStage = connection;

            Console.WriteLine("Program startup in 5 seconds...");
            //Thread.Sleep(5000);

            if (Startup.GetInstance().GetFlightInfo().getMultiPhaseDragon() == true)
            {
                using (connection)
                {
                    var krpc = connection.KRPC();
                    var spaceCenter = connection.SpaceCenter();
                    spacexEvent = new DragonEvent(spaceCenter.ActiveVessel, connection);
                }
            }
            else if (Startup.GetInstance().GetFlightInfo().getJustLandingBurn())
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new LandingBurnEvent(spaceCenter.ActiveVessel, connection);
            }
            else if (Startup.GetInstance().GetFlightInfo().getDragonV2() == true)
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new DragonV2Event(spaceCenter.ActiveVessel, connection);
            }
            else if (GetFlightInfo().getHyperloop() == true)
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new Hyperloop(spaceCenter.ActiveVessel, connection);
            }
            else if (Startup.GetInstance().GetFlightInfo().getRocket() == "FH")
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new FalconheavyEvent(spaceCenter.ActiveVessel, connection);
            }
            else if (Startup.GetInstance().GetFlightInfo().getRocket() == "A5")
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new Ariane5Event(spaceCenter.ActiveVessel, connection);
            }
            else if (Startup.GetInstance().GetFlightInfo().getRocket() == "F9RDev")
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new F9RDevEvent(spaceCenter.ActiveVessel, connection);
            }
            else if (Startup.GetInstance().GetFlightInfo().getRocket() == "H2A")
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new H2AEvent(spaceCenter.ActiveVessel, connection);
            }
            else
            {
                var krpc = connection.KRPC();
                var spaceCenter = connection.SpaceCenter();
                spacexEvent = new Falcon9Event(spaceCenter.ActiveVessel, connection, connectionFirstStage);
            }
        }

        /*public long timeRemain(String targetTime)
        {
            String dateStop = targetTime;

            // Custom date format
            TimeZone format = new TimeSpan("dd/MM/yy HH:mm:ss");

            Date d1 = new Date();
            Date d2 = null;

            try
            {
                d2 = format.parse(dateStop);
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
            // Get msec from each, and subtract.



            long diff = d2.getTime() - d1.getTime();

            if (Rachel.getInstance().getFlightInfo().isAborted())
            {
                return time;
            }
            time = diff / 1000;

            return time;

        }*/

        public static Startup GetInstance()
        {
            return instance;
        }

        public FlightInfo GetFlightInfo()
        {
            return flightInfo;
        }
    }
}
