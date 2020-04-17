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
    class DragonEvent : Event
    {
        protected Connection connection;

        protected Dragon Dragon;

        public DragonEvent(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;

            Thread Command = new Thread(CommandControl);
            

            Dragon = new Dragon(vessel, RocketBody.DRAGON);
            Dragon.Startup(connection);

            Command.Start();
        }

        public static void CommandControl()
        {
            while (true)
            {
                string command = Console.ReadLine();
                if (command[0] == '/')
                {
                    string[] items = command.Split('/', ' ');
                    // 
                    // maxspeed
                    // argument 1
                    // argument 2
                    string[] args = new string[items.Length - 2];

                    for (int i = 2; i < items.Length; i++)
                    {
                        args[i - 2] = items[i];
                    }


                    if(!Command.Execute(items[1], args))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Syntax error in command : " + Command.Get(items[1]).Help);
                        Console.ResetColor();
                    }
                }

                command = null;
            }
        }
    }
}
