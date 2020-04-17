﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpaceXComputer
{
    internal static class CommandFire
    {
        internal static void FireRegistery()
        {
            
            List<MethodInfo> internalMethods = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(RegisterCommand), false).Length > 0)
                .ToList();

            MethodInfo[] methods = internalMethods.ToArray();

            List<RegisterCommand> regestry = new List<RegisterCommand>();

            for (int i = 0; i < methods.Length; i++)
            {
                RegisterCommand rc = (RegisterCommand) methods[i].GetCustomAttribute(typeof(RegisterCommand), false);
                
                ParameterInfo[] commandParameters = methods[i].GetParameters();
                
                // if good method format
                if (methods[i].IsPublic && methods[i].ReturnType == typeof(bool) && commandParameters.Length == 1 &&
                    commandParameters[0].ParameterType == typeof(string[]))
                {
                    try
                    {
                        Command.ExecutableCommand commandMethod =
                            (Command.ExecutableCommand) Delegate.CreateDelegate(typeof(Command.ExecutableCommand),
                                methods[i]);

                        new Command(rc.Name, rc.Description, rc.Help, commandMethod);
                        //Console.WriteLine("Successfully registered command /" + rc.Name);
                    }
                    catch (Exception e)
                    {
                        string protection = "private";
                        if (methods[i].IsPublic) protection = "public";

                        string statism = "";
                        if (methods[i].IsStatic) statism = "static";

                        string parameters = "";
                        for (int j = 0; j < commandParameters.Length; j++)
                        {
                            
                            parameters += commandParameters[i].ParameterType + " " + commandParameters[i].Name;

                            if (j < parameters.Length - 1) parameters += " ";
                        }
                        
                        
                        Console.WriteLine("Error while creating delegate from " + protection + " " + statism + " " + 
                                          methods[i].ReturnType + " " + methods[i].Name + " (" + parameters + "): \n" + e);
                    }
                }

                else
                {
                    Console.WriteLine("Unable to register command " + rc.Name + ": must be like \"public static bool Function_Name(params string[] args)\"");
                }
            }
        }
    }
}