﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SpaceXComputer
{
    public sealed class Command
    {
        internal delegate bool ExecutableCommand(string[] args);

        #region Attributes
        
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
        /// All the loaded commands (Read-only)
        /// </summary>
        public static ReadOnlyCollection<Command> Commands
        {
            get { return _commands.AsReadOnly(); }
        }
        
        
        
        /// <summary>
        /// (Private) The method executed when calling the method
        /// </summary>
        private ExecutableCommand _method { get; }
        
        /// <summary>
        /// (Private) The loaded command list
        /// </summary>
        private static List<Command> _commands { get; set; } = new List<Command>(8);
        #endregion
        
        
        #region Methods

        /// <summary>
        /// Execute a name-based command
        /// </summary>
        /// <param name="name">The command name</param>
        /// <param name="sender">The command sender guid of the player / server</param>
        /// <param name="args">The arguments of the command</param>
        /// <returns>Returns true if execute, false if not existing or syntax error</returns>
        public static bool Execute(string name, params string[] args)
        {
            Command command = Command.Get(name);

            return command != null && command.Execute(args);
        }
        
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="sender">The command sender guid of the player / server</param>
        /// <param name="args">The arguments of the command</param>
        /// <returns>Returns if the command executed well</returns>
        public bool Execute(params string[] args)
        {
            return _method.Invoke(args);
        }
        
        /// <summary>
        /// Creates a command from its properties
        /// </summary>
        /// <param name="name">The name of the executed command, such as 'plugins' for '/plugins'</param>
        /// <param name="description">The description of the command given by /help</param>
        /// <param name="help">The help given by doing /help [command] or when a command fails</param>
        /// <param name="method">The method executed when calling the method</param>
        internal Command(string name, string description, string help, ExecutableCommand method)
        {
            this.Name = name;
            this.Description = description;
            this.Help = help;
            this._method = method;
            
            _commands.Add(this);
        }

        /// <summary>
        /// Get a command by its name
        /// </summary>
        /// <param name="name">The name of the command</param>
        /// <returns>The command object</returns>
        public static Command Get(string name)
        {
            foreach (Command cmd in _commands)
            {
                if (cmd.Name == name)
                {
                    return cmd;
                }
            }

            return null;
        }
        #endregion
        
    }
}

