using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRadio
{
    class Noxon
    {
        public static Dictionary<char, Command> Commands = new Dictionary<char, Command>()
        {
            { 'L', new Command { Key = 0x25, Desc = "KEY_LEFT" } },      // .NET runtime exception on startup if duplicate Dictionary Key value, e.g. 'S'
            { 'U', new Command { Key = 0x26, Desc = "KEY_UP" } },
            { 'R', new Command { Key = 0x27, Desc = "KEY_RIGHT" } },
            { 'D', new Command { Key = 0x28, Desc = "KEY_DOWN" } },
            { 'C', new Command { Key = 0x2B, Desc = "KEY_PRESET" } },          // (C)hannnel  + key 0..9 to store new preset       0x2D="KEY_DELFAV"  
            { 'A', new Command { Key = 0x2D, Desc = "KEY_ADDFAV" } },          // (A)dd favourite if channel/station playing 
            { 'E', new Command { Key = 0x2E, Desc = "KEY_DELFAV" } },          // (E)rase favourite if entry in favourites list selected 
            { 'N', new Command { Key = 0xAA, Desc = "KEY_INTERNETRADIO" } },   // I(N)ternetradio
            { 'F', new Command { Key = 0xAB, Desc = "KEY_FAVORITES" } },
            { 'H', new Command { Key = 0xAC, Desc = "KEY_HOME" } },
            { '-', new Command { Key = 0xAE, Desc = "KEY_VOL_DOWN" } },
            { '+', new Command { Key = 0xAF, Desc = "KEY_VOL_UP" } },
            { '>', new Command { Key = 0xB0, Desc = "KEY_NEXT" } },
            { '<', new Command { Key = 0xB1, Desc = "KEY_PREVIOUS" } },
            { 'S', new Command { Key = 0xB2, Desc = "KEY_STOP" } },
            { 'P', new Command { Key = 0xB3, Desc = "KEY_PLAY" } },
            { 'I', new Command { Key = 0xBA, Desc = "KEY_INFO" } },
            { '*', new Command { Key = 0xC0, Desc = "KEY_REPEAT" } },
            { 'M', new Command { Key = 0xDB, Desc = "KEY_SETTINGS" } },
            { 'X', new Command { Key = 0xDC, Desc = "KEY_SHUFFLE" } },
            { '0', new Command { Key = 0x30, Desc = "KEY_0" } },
            { '1', new Command { Key = 0x31, Desc = "KEY_1" } },
            { '2', new Command { Key = 0x32, Desc = "KEY_2" } },
            { '3', new Command { Key = 0x33, Desc = "KEY_3" } },
            { '4', new Command { Key = 0x34, Desc = "KEY_4" } },
            { '5', new Command { Key = 0x35, Desc = "KEY_5" } },
            { '6', new Command { Key = 0x36, Desc = "KEY_6" } },
            { '7', new Command { Key = 0x37, Desc = "KEY_7" } },
            { '8', new Command { Key = 0x38, Desc = "KEY_8" } },
            { '9', new Command { Key = 0x39, Desc = "KEY_9" } }                 // only 30 commands, remote has 32 (incl. On/Off and Mute, missing here)
        };

        public static byte[] intToByteArray(int value)
        {
            return new byte[] {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value};
        }
    }

    public class Command
    {
        public int Key { get; set; }
        public string Desc { get; set; }
    }


}
