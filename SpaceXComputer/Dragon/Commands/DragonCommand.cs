using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXComputer
{
    public class DragonCommand
    {
        [RegisterCommand("help")]
        public static bool Help(params string[] args)
        {
            if (args.Length == 0)
            {
                foreach (Command command in Command.Commands)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{command.Name} : {command.Description}");
                    Console.ResetColor();
                }
            }
            else
            {
                if (Command.Get(args[0]) != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(Command.Get(args[0]).Help);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Unknown command.");
                    Console.ResetColor();
                }
            }

            return true;
        }

        [RegisterCommand("maxspeed", "Set the max speed of the cargo for docking", "/maxspeed speed(in m/s)")]
        public static bool Maxspeed(params string[] args)
        {
            Dragon.MaxSpeed = Convert.ToSingle(args[0]);
            Console.WriteLine($"Max relative Speed set to {args[0]}m/s");

            //return Int32.TryParse(args[0], out int resultat);
            return true;
        }

        [RegisterCommand("killrotation", "Kill the difference relative rotation with the target", "use /killrotation")]
        public static bool KillRotation(params string[] args)
        {
            Dragon.KillRotationStart();
            return true;
        }

        [RegisterCommand("position", "Know the position of vessel for the target", "use /position")]
        public static bool Position(params string[] args)
        {
            if (Dragon.KnowPosition())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [RegisterCommand("rotation", "Know the rotation of vessel for the target", "use /rotation")]
        public static bool Rotation(params string[] args)
        {
            if (Dragon.KnowRotation())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [RegisterCommand("settarget", "Set the docking port target", "/settarget [vessel name] [docking port tag]")]
        public static bool Settarget(params string[] args)
        {
            Console.WriteLine("Settarget start");
            if (args[0] == null || args[1] == null)
            {
                return false;
            }
            else
            {
                return Dragon.ChangeTarget(args[0], args[1]);
            }
        }
    }
}
