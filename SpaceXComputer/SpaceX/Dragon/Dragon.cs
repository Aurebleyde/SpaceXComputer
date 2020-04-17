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
    public class Dragon
    {
        public static Connection connection;
        public static Vessel dragon;

        public Dragon(Vessel vessel, RocketBody rocketBody)
        {
            dragon = vessel;
            rocketBody = RocketBody.DRAGON;
        }

        public void Startup(Connection connectionLink)
        {
            connection = connectionLink;
        }

        public static float MaxSpeed;

        public static void KillRotationStart()
        {
            Thread killRot = new Thread(KillRotation);
            killRot.Start();
        }
        public static void KillRotation()
        {
            while (true)
            {
                Tuple<double, double, double, double> Rotation = dragon.Rotation(connection.SpaceCenter().TargetDockingPort.ReferenceFrame);

                dragon.Control.Pitch = Convert.ToSingle(Rotation.Item1) / 4;
                dragon.Control.Roll = Convert.ToSingle(Rotation.Item2) / 4;
                dragon.Control.Yaw = Convert.ToSingle(Rotation.Item3) / 4;
            }
        }

        public static bool ChangeTarget(string name, string tag)
        {
            DockingPort target = null;

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Equals(name) && vesselTarget.Type.Equals(VesselType.Station))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Vessel found...");
                    Console.ResetColor();

                    try
                    {
                        target = vesselTarget.Parts.WithTag(tag)[0].DockingPort;

                        connection.SpaceCenter().TargetDockingPort = target;
                        break;
                    }
                    catch
                    {
                        
                    }
                }
            }

            if (target == null)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Target not found");
                Console.ResetColor();
                return false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Target Found and Set !");
                Console.ResetColor();
                return true;
            }
        }

        public static bool KnowPosition()
        {
            /*if (connection.SpaceCenter().TargetDockingPort != null)
            {*/
                Console.WriteLine($"{dragon.Position(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item1} | {dragon.Position(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item2} | {dragon.Position(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item3}");
                return true;
            /*}
            else
            {
                return false;
            }*/
        }
        public static bool KnowRotation()
        {
            /*if (connection.SpaceCenter().TargetDockingPort != null)
            {*/
            Console.WriteLine($"{dragon.Rotation(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item1} | {dragon.Rotation(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item2} | {dragon.Rotation(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item3} | {dragon.Rotation(connection.SpaceCenter().TargetDockingPort.ReferenceFrame).Item4}");
            return true;
            /*}
            else
            {
                return false;
            }*/
        }
    }
}
