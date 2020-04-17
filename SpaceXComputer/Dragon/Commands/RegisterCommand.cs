﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SpaceXComputer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterCommand : Attribute
    {
        /// <summary>
        /// The name of the executed command, such as 'plugins' for '/plugins'
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The description of the command given by /help
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// The help given by doing /help [command] or when a command fails
        /// </summary>
        public string Help { get; }

        /// <summary>
        /// Registers a command
        /// </summary>
        /// <param name="name">The name of the executed command, such as 'plugins' for '/plugins'</param>
        /// <param name="description">The description of the command given by /help</param>
        /// <param name="help">The help given by doing /help [command] or when a command fails</param>
        public RegisterCommand(string name, string description = "No description available.", string help = "No help available.")
        {
            this.Name = name;
            this.Description = description;
            this.Help = help;
        }
    }
}