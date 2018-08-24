﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinShell.UIManagement;

namespace WinShell
{
    /// <summary>
    /// Class for parsing and processing commands.
    /// </summary>
    public class CommandProcessor
    {
        /// <summary>
        /// Gets the UI Manager instance associated with the command processor.
        /// </summary>
        private UIManager UIManager { get; set; }

        /// <summary>
        /// Gets the output window associated with this command.
        /// </summary>
        public ConsoleWindow Window { get; private set; }

        /// <summary>
        /// Gets the executor associated with this command processor instance.
        /// </summary>
        public CommandExecutor Executor { get; private set; }

        /// <summary>
        /// Gets the parser associated with this command processor instance.
        /// </summary>
        public CommandParser Parser { get; private set; }

        /// <summary>
        /// Gets the built-in commands associated with this command processor instance.
        /// </summary>
        public BuiltinLibrary Builtins { get; private set; }

        public CommandProcessor(UIManager uiManager)
        {
            UIManager = uiManager;
            Executor = new CommandExecutor(this);
            Builtins = new BuiltinLibrary(this);
            Parser = new CommandParser(this);
        }

        /// <summary>
        /// Parses and processes the specified command string.
        /// </summary>
        /// <param name="command">Command string to process.</param>
        /// <param name="shellSession">Shell session to use with this command.</param>
        /// <returns>A value indicating whether we were able to successfully process the command.</returns>
        public bool ProcessCommand(string command, ShellSession shellSession)
        {
            Window = shellSession.Window;

            var chdirCommand = $"cd \"{shellSession.CurrentDirectory}\"";
            Window.WriteCommandLink(shellSession.CurrentDirectory, Window.RunShellRequestCommand, chdirCommand);
            Window.WriteInfoText($" ==> {command}\n");

            // For command execution, switch to the session's current directory.
            Directory.SetCurrentDirectory(shellSession.CurrentDirectory);

            try
            {
                ProcessorCommand pCommand = Parser.Parse(command);
                if (pCommand is SingleProcessCommand)
                {
                    pCommand.ConsoleWindow = Window;
                    Executor.ExecuteSingleProcessCommand(pCommand);
                }
                else if (pCommand is MultiProcessCommand)
                {
                    pCommand.ConsoleWindow = Window;
                    Executor.ExecuteMultipleProcessCommand(pCommand);
                }
            }
            catch(KeyNotFoundException e)
            {
                Executor.WriteOutputText("Command not recognized.");
                return false; //return value may be used later, but unlikely
            }

            // Update the session's current directory based on the result of the command.
            shellSession.CurrentDirectory = Directory.GetCurrentDirectory();

            return true;
        }
    }
}
