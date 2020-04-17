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
    public class Starship
    {
        public Connection connection;
        public Vessel starship;

        public Starship(Vessel vessel, RocketBody rocketBody)
        {
            starship = vessel;
            rocketBody = RocketBody.STARSHIP;
        }

        public void Startup(Connection connection)
        {
            Console.WriteLine("Starship : Starship is in startup.");

            Thread.Sleep(5000);

            starship.Parts.WithTag("BigLeft")[0].ControlSurface.Deployed = true;
            starship.Parts.WithTag("BigLeft")[0].ControlSurface.Inverted = true;
            var LargeLeftWing = starship.Parts.WithTag("BigLeft")[0].Modules.Single(s => s.Name.Equals("ModuleControlSurface"));
            //Console.WriteLine(LargeLeftWing.GetField("DeployModulePos"));
            LargeLeftWing.SetFieldFloat("DeployPosition", 0.5f);
            
        }
    }
}
