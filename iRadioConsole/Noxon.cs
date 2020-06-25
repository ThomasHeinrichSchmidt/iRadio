using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;


namespace iRadio
{

    public static class Noxon
    {
        public static bool testmode { get; set; }
        public static bool busy { get; set; }
        public static NetworkStream netStream = null;
        public static TcpClient tcpClient = null;

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

        public static int MultiPressDelayForSameKey = 100;
        public static int MultiPressDelayForNextKey = 1100;

        public static int Command(this NetworkStream netStream, char commandkey)
        {
            try
            {
                if (netStream.CanWrite && Commands.ContainsKey(commandkey))
                {
                    System.Diagnostics.Debug.WriteLine("\t\tTransmit Command('{0}'): ASC({1} --> 0x{2})", commandkey, Commands[commandkey].Key, BitConverter.ToString(Noxon.intToByteArray(Commands[commandkey].Key)));
                    netStream.Write(Noxon.intToByteArray(Commands[commandkey].Key), 0, sizeof(int));
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (System.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine("\t\tTransmit Command() failed ({0})", e.Message);
                Close();
                Open();
                if (netStream != null && netStream.CanWrite) netStream.Write(Noxon.intToByteArray(Commands[commandkey].Key), 0, sizeof(int));
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public static int String(this NetworkStream netStream, string str)
        {
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands(str);
            foreach (MultiPressCommand m in mpc)
                for (int i = 0; i < m.Times; i++)
                {
                    netStream.Command(Convert.ToChar(48 + m.Digit ));
                    Thread.Sleep(MultiPressDelayForSameKey);
                }
            Thread.Sleep(MultiPressDelayForNextKey);
            return 0;
        }

        public static bool Open()
        {
            // Console.WriteLine("iRadio Telnet port 10100:");
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connect?view=netcore-3.1
            // Uses a remote endpoint to establish a socket connection.
            tcpClient = new TcpClient();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.36"), 10100);
            while (!tcpClient.Connected)
            {
                try
                {
                    tcpClient.Connect(ipEndPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Connect to NOXON iRadio failed ({0}, {1})", ex.SocketErrorCode, ex.Message);
                }
            }

            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.getstream?view=netcore-3.1
            // Uses the GetStream public method to return the NetworkStream.
            netStream = tcpClient.GetStream();
            return true;
        }

        public static bool Close()
        {
            netStream.Close();
            tcpClient.Close();
            netStream = null;
            tcpClient = null;
            return true;
        }

        private static iRadioConsole.Macro macro;
        public static iRadioConsole.Macro Macro {
            get
            {
                return macro;
            }
            set
            {
                if (value != macro)
                {
                    macro = value;
                }
            }
        }


        private static int listposmin = 0;
        private static int listposmax = 0;
        public static void Parse(NetworkStream netStream, IEnumerable<XElement> iRadioData, StreamWriter parsedElementsWriter, StreamWriter nonParsedElementsWriter, TextWriter stdOut)
        {
            foreach (XElement el in iRadioData)
            {
                // int timep;  // using LINQ is not really more readable ...
                // XElement elem = el.DescendantsAndSelf("update").Where(r => r.Attribute("id").Value == "play").FirstOrDefault();  // == null || <update id="play"> < value id = "timep" min = "0" max = "65535" > 1698 </ value >
                // if ((elem = el.DescendantsAndSelf("update").Where(r => r.Attribute("id").Value == "play" && r.Element("value").Attribute("id").Value == "timep").FirstOrDefault()) != null) timep = int.Parse(elem.Value.Trim('\r', '\n', ' ')); 

                if (testmode) Thread.Sleep(200); // 50ms  used to delay parsing of Telnet.xml, otherwise it's over very quickly
                if (parsedElementsWriter != null)
                {
                    Console.SetOut(parsedElementsWriter); // re-direct
                    Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("hh: mm:ss.fff"), el.ToString());
                    Console.SetOut(stdOut); // stop re-direct
                    parsedElementsWriter.Flush();
                }

                if (Macro != null) Macro.Step(); // process macro, if any

                switch (el.Name.ToString())
                {
                    case "update":
                        if (el.Attribute("id").Value == "play")   // <update id="play">
                        {
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "timep")
                            {
                                Show.PlayingTime(el, Show.linePlayingTime);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "buflvl")
                            {
                                Show.Line("Buffer[%]", Show.lineBuffer, el);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "wilvl")
                            {
                                Show.Line("WiFi[%]", Show.lineWiFi, el);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "date")   // <value id="date" 
                            {
                                int i;
                                if (int.TryParse(el.Value, out i) && i > 0) Show.Line("Date", Show.lineStatus, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "track")
                            {
                                Show.Line("Track", Show.lineTrack, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "artist")
                            {
                                Show.Line("Artist", Show.lineArtist, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "album")
                            {
                                Show.Line("Album", Show.lineAlbum, el);
                            }
                            else
                            {
                                Program.LogElement(nonParsedElementsWriter, stdOut, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "status")  // <update id="status">
                        {
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "play")
                            {
                                Show.Line("Icon-Play", Show.lineIcon, el);
                            }
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "shuffle")
                            {
                                Show.Line("Icon-Shuffle", Show.lineIcon, el);
                            }
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "repeat")
                            {
                                Show.Line("Icon-Repeat", Show.lineIcon, el);
                            }
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "busy")    // <value id="busy" 
                            {
                                busy = false;
                                if (int.TryParse(el.Value, out int busyval)) busy = busyval == 1 ? true: false;  // el.Value = "\n  1\n"
                                Show.Line("Busy=", Show.lineBusy, el);
                                // System.Diagnostics.Debug.WriteLine("Status, busy = {0})", busy);
                            }
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "listpos")    // <value id="listpos" 
                            {
                                string min = el.Element("value").Attribute("min").Value;  // <value id="listpos" min="1" max="26">23</value> 
                                string max = el.Element("value").Attribute("max").Value;
                                string caption = "From (" + min + ".." + max + ") @ ";
                                int.TryParse(min, out listposmin);  
                                int.TryParse(max, out listposmax);
                                // int value = 0;
                                // if (int.TryParse(el.Value, out value)) el.Value = (value+1).ToString();  // NOXON list index is one too low
                                Show.Line(caption, Show.lineStatus, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "welcome")  // <update id="welcome">
                        {
                            //   <icon id="welcome" text="wlan@ths / wlan@t-h-schmidt.de">welcome</icon>
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "welcome")
                            {
                                Show.Line("Welcome", Show.lineIcon, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "browse")
                        {
                            Show.Browse(el, Show.line0);
                        }
                        else
                        {
                            Program.LogElement(nonParsedElementsWriter, stdOut, el);
                        }
                        break;
                    case "view":
                        if (el.Attribute("id").Value == "play")
                        {
                            foreach (XElement e in el.Elements())
                            {
                                if (e.Name == "text" && e.Attribute("id").Value == "title")
                                {
                                    Show.Line("Title", Show.lineTitle, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "artist")
                                {
                                    Show.Line("Artist", Show.lineArtist, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "album")
                                {
                                    Show.Line("Album", Show.lineAlbum, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "track")
                                {
                                    Show.Line("Track", Show.lineTrack, e);
                                }
                                else if (e.Name == "value" && e.Attribute("id").Value == "timep")
                                {
                                    Show.PlayingTime(e, Show.linePlayingTime);
                                }
                            }
                        }
                        else if (el.Attribute("id").Value == "status")
                        {
                            // Console.WriteLine("Status, value = {0}", el.Element("value").Value);
                            foreach (XElement e in el.Elements())
                            {
                                if (e.Name == "icon" && e.Attribute("id").Value == "play")
                                {
                                    Show.Status(e, Show.lineStatus, Show.line0);
                                }
                            }

                        }
                        else if (el.Attribute("id").Value == "msg")
                        {
                            if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "scrid")
                            {
                                Show.Msg(el, Show.lineStatus, Show.line0);
                                XElement elem = el.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "line0").FirstOrDefault();  // == null || <view id="msg">  < text id = "scrid" > 82 </ text >    < text id = "line0" > Nicht verfÃ¼gbar </ text >
                                if (elem != null)
                                {
                                    if (elem.Value == iRadioConsole.Properties.Resources.NoxonMessageToCloseStream) return;  // close stream if "Nicht verfÃ¼gbar"
                                }
                            }
                        }
                        else if (el.Attribute("id").Value == "browse")
                        {
                            if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "scrid")
                            {
                                Show.Browse(el, Show.line0);
                            }
                        }
                        else if (el.Attribute("id").Value == "welcome")  // <view id="welcome">
                        {
                            // <view id="welcome">  < icon id = "welcome" text = "wlan@ths / wlan@t-h-schmidt.de" > welcome </ icon >    </ view >
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "welcome")
                            {
                                Show.Status(el, Show.lineStatus, Show.line0);
                            }
                        }
                        else
                        {
                            Program.LogElement(nonParsedElementsWriter, stdOut, el);
                        }
                        break;
                    case "CloseStream":
                        return;
                    default:
                        Program.LogElement(nonParsedElementsWriter, stdOut, el);
                        break;
                }
            }
        }
    }

    public class Command
    {
        public int Key { get; set; }
        public string Desc { get; set; }
    }

    public class Send
    {
        public static void TransmitMacroHR3(NetworkStream netStream)
        {
            Console.WriteLine("run macro to choose hr3, hold on ...");
            int i = 170; // KEY_INTERNETRADIO
            Transmit(i, netStream);
            i = 39; // KEY_RIGHT  --> Alle Sender
            Transmit(i, netStream);
            i = 39; // KEY_RIGHT  --> Senderliste 
            Transmit(i, netStream);

            int tnext = 1100;
            int tsame = 100;

            Thread.Sleep(tnext);  // wait for list to load 

            netStream.Write(Noxon.intToByteArray(Noxon.Commands['4'].Key), 0, sizeof(int));  // g
            Thread.Sleep(tsame);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['4'].Key), 0, sizeof(int));  // h     h

            Thread.Sleep(tnext);

            netStream.Write(Noxon.intToByteArray(Noxon.Commands['7'].Key), 0, sizeof(int));  // p
            Thread.Sleep(tsame);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['7'].Key), 0, sizeof(int));  // q
            Thread.Sleep(tsame);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['7'].Key), 0, sizeof(int));  // r     r

            Thread.Sleep(tnext);

            netStream.Write(Noxon.intToByteArray(Noxon.Commands['3'].Key), 0, sizeof(int));  // d
            Thread.Sleep(tsame);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['3'].Key), 0, sizeof(int));  // e
            Thread.Sleep(tsame);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['3'].Key), 0, sizeof(int));  // f
            Thread.Sleep(tsame);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['3'].Key), 0, sizeof(int));  // 3     3

            Thread.Sleep(tnext);

            i = 39; // KEY_RIGHT  --> Search 
            Transmit(i, netStream);

            Console.ReadKey();  // wait for find ro complete, 
                                //<update id="browse">
                                //  <text id="line0" flag="ps">hr3</text>
                                //  <text id="line1" flag="d">hr4</text>
                                //  <text id="line2" flag="p">Hrvatski Radio Frankfurt</text>
                                //  <text id="line3" flag="p">Hrw laut.fm</text>
                                //  <icon id="hchyDn">empty</icon>
                                //</update>

            i = 39; // KEY_RIGHT  --> play 
            Transmit(i, netStream);

            Console.ReadKey();
        }

        private static void Transmit(int i, NetworkStream netStream)
        {
            System.Diagnostics.Debug.WriteLine("Transmit: ASC({0} --> 0x{1})", i, BitConverter.ToString(Noxon.intToByteArray(i)));
            netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
            Thread.Sleep(500);
        }

        public static void TransmitAllASCIIvvaluesStepByStep(NetworkStream netStream)
        {
            Console.WriteLine("probing all characters 0..255 to sent to NOXON");
            for (int i = 0; i < 256; i++)
            {   /*
                                    if (37 <= i && i <= 57) continue;
                                    if (64 <= i && i <= 90) continue;
                                    if (97 <= i && i <= 122) continue;
                                    if (171 <= i && i <= 179) continue;
                                    */
                System.Diagnostics.Debug.WriteLine("Transmit: ASC({0} --> 0x{1})", i, BitConverter.ToString(Noxon.intToByteArray(i)));
                netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
                // Thread.Sleep(500);
                Console.ReadKey(true);
            }
        }

        public static void TransmitCharacterFromASCIIvalue(NetworkStream netStream)
        {
            Console.WriteLine("enter character using ascii code 0..255 to sent to NOXON");
            string line = Console.ReadLine();
            int i = Int32.Parse(line);
            System.Diagnostics.Debug.WriteLine("Transmit: ASC({0} --> 0x{1})", line, BitConverter.ToString(Noxon.intToByteArray(i)));
            netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
        }

        public static void TransmitCharacterFrom2HexDigits(NetworkStream netStream)
        {
            Console.WriteLine("enter character using two hex digits (00..FF) to sent to NOXON");
            string line = Console.ReadLine();
            byte[] bytes = { 0, 0, 0, System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Parse(line).Value[0] };
            System.Diagnostics.Debug.WriteLine("Transmit: Hex = {0} --> 0x{1}", line, BitConverter.ToString(bytes));
            netStream.Write(bytes, 0, bytes.Length);
        }

        public static char Transmit7BitASCIIcharacterEnteredFromNumpad(NetworkStream netStream)
        {
            char ch;
            Console.WriteLine("enter character using numpad to sent to NOXON (e.g. Alt+123, but only works for chars < 128)");
            ConsoleKeyInfo cp = Console.ReadKey(true);    // enter character to sent to NOXON (e.g. Alt+123, but only works for chars < 128)
            int i = Encoding.GetEncoding("ISO-8859-1").GetBytes(new char[] { cp.KeyChar })[0];
            ch = cp.KeyChar;
            System.Diagnostics.Debug.WriteLine("Transmit: ASC({0}={1}) --> 0x{2}", i, ch, BitConverter.ToString(Noxon.intToByteArray(i)));
            netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
            return ch;
        }

        public static void ProbingSendLetters(NetworkStream netStream)
        {
            int next = 1100;  // 100 = same key   <-[1000..1050]->   1100 = next letter
            int same = 100;
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['2'].Key), 0, sizeof(int));  // a
            Thread.Sleep(same);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['2'].Key), 0, sizeof(int));  // b
            Thread.Sleep(same);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['2'].Key), 0, sizeof(int));  // c

            Thread.Sleep(next);

            netStream.Write(Noxon.intToByteArray(Noxon.Commands['9'].Key), 0, sizeof(int));  // w
            Thread.Sleep(same);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['9'].Key), 0, sizeof(int));  // x
            Thread.Sleep(same);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['9'].Key), 0, sizeof(int));  // y
            Thread.Sleep(same);

            Thread.Sleep(next);

            netStream.Write(Noxon.intToByteArray(Noxon.Commands['9'].Key), 0, sizeof(int));  // w
            Thread.Sleep(same);
            netStream.Write(Noxon.intToByteArray(Noxon.Commands['9'].Key), 0, sizeof(int));  // x
            Thread.Sleep(same);

            Thread.Sleep(3000);  // show result
        }
    }
}
