using iRadio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRadio
{
    public class Macro
    {
        /// <summary>
        /// (re) play remote control key commands 
        /// </summary>
        private readonly string name = "";
        private readonly string[] command;
        private int step = 0;
        private int steps = 0;
        private static Macro runningInstance = null;

        public Macro(string name, string[] command)   // command = Noxon.Commands.Key  -OR-  "@input-string"   
        {                                             // e.g. new iRadioConsole.Macro("F1", new string [] { "N", "R", "R", "@hr3", "R", "R"});  
            steps = command.Length;
            if (steps > 0)
            {
                this.name = name;
                this.command = new string[command.Length];
                for (int i = 0; i < command.Length; i++)
                {
                    if (command[i].Length == 1)
                    {
                        char key = command[i][0];
                        if (Noxon.Commands.ContainsKey(key)) this.command[i] = command[i];
                        else
                        {
                            steps = 0;
                            System.Diagnostics.Debug.WriteLine("Macro constructor failed, given char was not a Noxon command key");
                            break;
                        }
                    }
                    else if (command[i].Length > 1)
                    {
                        if (command[i][0] != '@')
                        {
                            steps = 0;
                            System.Diagnostics.Debug.WriteLine("Macro constructor failed, given string needs @ as first char");
                            break;
                        }
                        else
                        {
                            this.command[i] = command[i];
                        }
                    }
                    else
                    {
                        steps = 0;
                        System.Diagnostics.Debug.WriteLine("Macro constructor failed, zero length string not sensible");
                        break;
                    }
                }
            }
        }

        public bool Step() 
        {
            if (step < steps && (this == runningInstance || runningInstance == null))
            {
                runningInstance = this;
                System.Diagnostics.Debug.WriteLine("[{0}] Processing macro {1}, step {2}, busy = {3})", Show.currentTitle, name, step, Noxon.Busy);
                if (!Noxon.Busy)
                {
                    if (command[step].Length == 1)
                    {
                        Noxon.netStream.Command(command[step][0]);
                        System.Diagnostics.Debug.WriteLine("Processing macro {0}, step {1}: key '{2}'", name, step, command[step][0]);
                    }
                    else if (command[step].Length > 1)
                    {
                        Noxon.netStream.String(command[step].Substring(1));
                        System.Diagnostics.Debug.WriteLine("Processing macro {0}, step {1}: string '{2}'", name, step, command[step]);
                    }
                    Thread.Sleep(1000);  // busy does not work as expected
                    step++;
                    if (step == steps) runningInstance = null;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }
        
        public bool Abort() 
        {
            step = 0;
            steps = 0;
            if (this == runningInstance) runningInstance = null;
            return true;
        }
    }
}
