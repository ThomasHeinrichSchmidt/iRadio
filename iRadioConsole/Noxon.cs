using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iRadio
{
    public interface ITestableNetworkStream
    {
        Stream GetStream();
        NetworkStream GetNetworkStream();
        int Read([In, Out] byte[] buffer, int offset, int size);
        void Write([In, Out] byte[] buffer, int offset, int size);
        bool CanWrite { get;  }
        void Close();
    }
    public class TestableNetworkStream : ITestableNetworkStream
    {
        private readonly NetworkStream stream;

        public TestableNetworkStream(NetworkStream ns)
        {
            this.stream = ns ?? throw new ArgumentNullException("TestableNetworkStream was null");
        }

        public Stream GetStream()
        {
            return stream;
        }
        public NetworkStream GetNetworkStream()
        {
            return stream;
        }
        public bool CanWrite
        {
            get
            {
                return this.stream.CanWrite;
            }
        }
        public int Read([In, Out] byte[] buffer, int offset, int size)
        {
            return this.stream.Read(buffer, offset, size);
        }
        public void Write([In, Out] byte[] buffer, int offset, int size)
        {
            this.stream.Write(buffer, offset, size);
        }
        public void Close()
        {
            this.stream.Close();
        }
    }
    public static class Noxon
    {
        public static bool Testmode { get; set; }
        public static bool Busy { get; set; }
        public static ITestableNetworkStream netStream = null;
        public static TcpClient tcpClient = null;
        public static bool textEntry = false;    // TODO: if focus on keyboard entry in contrast to local hotkeys 
        public const int ListLines = 4;
        public static string currentArtist = "";

        public static Dictionary<char, Command> Commands = new Dictionary<char, Command>()
        {                                                                      // .NET runtime exception on startup if duplicate Dictionary Key value, e.g. 'S'
            { 'L', new Command { Key = 0x25, Desc = "KEY_LEFT" } },            
            { 'U', new Command { Key = 0x26, Desc = "KEY_UP" } },
            { 'R', new Command { Key = 0x27, Desc = "KEY_RIGHT" } },
            { 'D', new Command { Key = 0x28, Desc = "KEY_DOWN" } },
            { 'C', new Command { Key = 0x2B, Desc = "KEY_PRESET" } },          // (C)hannnel  + key 0..9 to store new preset       0x2D="KEY_DELFAV"  
            { 'A', new Command { Key = 0x2D, Desc = "KEY_ADDFAV" } },          // (A)dd favourite if channel/station playing 
            { 'E', new Command { Key = 0x2E, Desc = "KEY_DELFAV" } },          // (E)rase favourite if entry in favourites list selected 
            { 'N', new Command { Key = 0xAA, Desc = "KEY_INTERNETRADIO" } },   // I(N)ternetradio
            { 'F', new Command { Key = 0xAB, Desc = "KEY_FAVORITES" } },       // (F)avorites
            { 'H', new Command { Key = 0xAC, Desc = "KEY_HOME" } },            // (H)ome
            { '-', new Command { Key = 0xAE, Desc = "KEY_VOL_DOWN" } },
            { '+', new Command { Key = 0xAF, Desc = "KEY_VOL_UP" } },
            { '>', new Command { Key = 0xB0, Desc = "KEY_NEXT" } },
            { '<', new Command { Key = 0xB1, Desc = "KEY_PREVIOUS" } },
            { 'S', new Command { Key = 0xB2, Desc = "KEY_STOP" } },
            { 'P', new Command { Key = 0xB3, Desc = "KEY_PLAY" } },
            { 'I', new Command { Key = 0xBA, Desc = "KEY_INFO" } },
            { '*', new Command { Key = 0xC0, Desc = "KEY_REPEAT" } },
            { 'M', new Command { Key = 0xDB, Desc = "KEY_SETTINGS" } },        // (M)enu
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

        public static byte[] IntToByteArray(int value)
        {
            return new byte[] {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value};
        }

        public static int MultiPressDelayForSameKey = 100;
        public static int MultiPressDelayForNextKey = 1100;

        public static int Command(this ITestableNetworkStream netStream, char commandkey)
        {
            try
            {
                if (netStream.CanWrite && Commands.ContainsKey(commandkey))
                {
                    System.Diagnostics.Debug.WriteLine("\t\tTransmit Command('{0}'): ASC({1} --> 0x{2})", commandkey, Commands[commandkey].Key, BitConverter.ToString(Noxon.IntToByteArray(Commands[commandkey].Key)));
                    netStream.Write(Noxon.IntToByteArray(Commands[commandkey].Key), 0, sizeof(int));
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
                if (netStream != null && netStream.CanWrite) netStream.Write(Noxon.IntToByteArray(Commands[commandkey].Key), 0, sizeof(int));
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public static int String(this ITestableNetworkStream netStream, string str)
        {
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands(str);
            foreach (MultiPressCommand m in mpc)
                for (int i = 0; i < m.Times; i++)
                {
                    netStream.Command(Convert.ToChar(48 + m.Digit));
                    Thread.Sleep(MultiPressDelayForSameKey);
                }
            Thread.Sleep(MultiPressDelayForNextKey);
            return 0;
        }

        // https://stackoverflow.com/questions/13634868/get-the-default-gateway
        private static System.Net.IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                // .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .FirstOrDefault();
        }

        // https://stackoverflow.com/questions/4042789/how-to-get-ip-of-all-hosts-in-lan
        static CountdownEvent countdown;
        static int upCount = 0;
        static readonly object lockObj = new object();
        private static readonly List<string> IPsFound = new List<string>();
        static IPAddress IP = IPAddress.Parse(iRadioConsole.Properties.Resources.NoxonIP);

        private static bool PingHosts()
        {
            string gateway = GetDefaultGateway().ToString();
            if (gateway != null) {
                System.Diagnostics.Debug.WriteLine("DefaultGateway = " + gateway);
                string ipBase = Regex.Replace(gateway, @"\.[0-9]+$", "") + ".";
                System.Diagnostics.Debug.WriteLine("yields IP base " + gateway);

                countdown = new CountdownEvent(1);
                Stopwatch sw = new Stopwatch();
                upCount = 0;
                sw.Start();
                for (int i = 1; i < 255; i++)
                {
                    string ip = ipBase + i.ToString();
                    try
                    {
                        Ping p = new Ping();
                        p.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        p.SendAsync(ip, 100, ip);
                        countdown.AddCount();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Ping.SendAsync  failed on host {0} = {1}", ip, ex.Message);
                    }
                }
                countdown.Signal();
                countdown.Wait();
                sw.Stop();
                TimeSpan span = new TimeSpan(sw.ElapsedTicks);
                System.Diagnostics.Debug.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount);
                if (IP != null) return true;
                else return false;
            }
            else
            {
                return false;
            }
        }
        static void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                IPsFound.Add(ip);
                // IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                // string name = hostEntry.HostName;
                // System.Diagnostics.Debug.WriteLine("{0} ({1}) is up: ({2} ms)", ip, name, e.Reply.RoundtripTime);
                System.Diagnostics.Debug.WriteLine("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime);
                using (TcpClient tcpClient = new TcpClient())
                {
                    try
                    {
                        tcpClient.Connect(ip, 10100);
                        System.Diagnostics.Debug.WriteLine("Port 10100 on {0} is open", ip);
                        IP = IPAddress.Parse(ip);
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine("Port 10100 on {0} is closed", ip);
                    }
                }
                lock (lockObj)
                {
                    upCount++;
                }
            }
            else if (e.Reply == null)
            {
                System.Diagnostics.Debug.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
            countdown.Signal();
        }

        public static bool Open()
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connect?view=netcore-3.1
            // Uses a remote endpoint to establish a socket connection.
            tcpClient = new TcpClient();
            IPAddress ip = IPAddress.Parse(iRadioConsole.Properties.Resources.NoxonIP);
            if (PingHosts()) ip = IP;
            IPEndPoint ipEndPoint = new IPEndPoint(ip, 10100);  // using iRadio Telnet port 10100
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
            netStream = new TestableNetworkStream(tcpClient.GetStream());
            return true;
        }

        public static async Task<bool> OpenAsync()
        {
            tcpClient = new TcpClient();
            IPAddress ip = Noxon.IP;   
            if (Noxon.PingHosts()) ip = Noxon.IP;
            await tcpClient.ConnectAsync(ip, 10100); // connect to iRadio server port
            netStream = new TestableNetworkStream(tcpClient.GetStream());
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

        private static iRadio.Macro macro;
        public static iRadio.Macro Macro {
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

        public static bool GetListMinMax(out int min, out int max)
        {
            min = listposmin;
            max = listposmax;
            if (min > 0 && max > 0) return true;
            return false;
        }
        public static void ResetListMinMax()
        {
            listposmin = 0;
            listposmax = 0;
        }

        public static void Parse(IEnumerable<XElement> iRadioData, StreamWriter parsedElementsWriter, StreamWriter nonParsedElementsWriter, TextWriter stdOut, IShow Show)  // System.Windows.Forms.Form form
        {
            foreach (XElement el in iRadioData)
            {
                // int timep;  // using LINQ is not really more readable ...
                // XElement elem = el.DescendantsAndSelf("update").Where(r => r.Attribute("id").Value == "play").FirstOrDefault();  // == null || <update id="play"> < value id = "timep" min = "0" max = "65535" > 1698 </ value >
                // if ((elem = el.DescendantsAndSelf("update").Where(r => r.Attribute("id").Value == "play" && r.Element("value").Attribute("id").Value == "timep").FirstOrDefault()) != null) timep = int.Parse(elem.Value.Trim('\r', '\n', ' ')); 

                if (Testmode) Thread.Sleep(200); // 50ms  used to delay parsing of Telnet.xml, otherwise it's over very quickly
                Show.Log(parsedElementsWriter, stdOut, el);

                if (Macro != null) Macro.Step(); // process macro, if any

                switch (el.Name.ToString())
                {
                    case "update":
                        if (el.Attribute("id").Value == "play")   // <update id="play">
                        {
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "timep")
                            {
                                Show.PlayingTime(el, Lines.PlayingTime);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "buflvl")
                            {
                                Show.Line("Buffer[%]", Lines.Buffer, el);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "wilvl")
                            {
                                Show.Line("WiFi[%]", Lines.WiFi, el);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "date")   // <value id="date" 
                            {
                                if (int.TryParse(el.Value, out int i) && i > 0) Show.Line("Date", Lines.Status, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "track")
                            {
                                Show.Line("Track", Lines.Track, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "artist")
                            {
                                Show.Line("Artist", Lines.Artist, el);
                                currentArtist = Tools.Normalize(el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "album")
                            {
                                Show.Line("Album", Lines.Album, el);
                            }
                            else
                            {
                                ConsoleProgram.LogElement(nonParsedElementsWriter, stdOut, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "status")  // <update id="status">
                        {
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "play")
                            {
                                Show.Line("Icon-Play", Lines.Icon, el);
                            }
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "shuffle")
                            {
                                Show.Line("Icon-Shuffle", Lines.Icon, el);
                            }
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "repeat")
                            {
                                Show.Line("Icon-Repeat", Lines.Icon, el);
                            }
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "busy")    // <value id="busy" 
                            {
                                Busy = false;
                                if (int.TryParse(el.Value, out int busyval)) Busy = busyval == 1;  // el.Value = "\n  1\n"
                                Show.Line("Busy=", Lines.Busy, el);
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
                                Show.Line(caption, Lines.Status, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "welcome")  // <update id="welcome">
                        {
                            //   <icon id="welcome" text="wlan@ths / wlan@t-h-schmidt.de">welcome</icon>
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "welcome")
                            {
                                Show.Line("Welcome", Lines.Icon, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "browse")
                        {
                            Show.Browse(el, Lines.line0);
                        }
                        else
                        {
                            ConsoleProgram.LogElement(nonParsedElementsWriter, stdOut, el);
                        }
                        break;
                    case "view":
                        if (el.Attribute("id").Value == "play")
                        {
                            foreach (XElement e in el.Elements())
                            {
                                if (e.Name == "text" && e.Attribute("id").Value == "title")
                                {
                                    Show.Line("Title", Lines.Title, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "artist")
                                {
                                    Show.Line("Artist", Lines.Artist, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "album")
                                {
                                    Show.Line("Album", Lines.Album, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "track")
                                {
                                    Show.Line("Track", Lines.Track, e);
                                }
                                else if (e.Name == "value" && e.Attribute("id").Value == "timep")
                                {
                                    Show.PlayingTime(e, Lines.PlayingTime);
                                }
                                else if (e.Name == "value" && e.Attribute("id").Value == "buflvl")
                                {
                                    Show.Line("Buffer[%]", Lines.Buffer, e);
                                }
                                else if (e.Name == "value" && e.Attribute("id").Value == "wilvl")
                                {
                                    Show.Line("WiFi[%]", Lines.WiFi, e);
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
                                    Show.Status(e, Lines.Status);
                                }
                            }

                        }
                        else if (el.Attribute("id").Value == "msg")
                        {
                            if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "scrid")
                            {
                                Show.Msg(el, Lines.line0);
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
                                Show.Browse(el, Lines.line0);
                            }
                        }
                        else if (el.Attribute("id").Value == "welcome")  // <view id="welcome">
                        {
                            // <view id="welcome">  < icon id = "welcome" text = "wlan@ths / wlan@t-h-schmidt.de" > welcome </ icon >    </ view >
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "welcome")
                            {
                                Show.Status(el, Lines.Status);
                            }
                        }
                        else
                        {
                            ConsoleProgram.LogElement(nonParsedElementsWriter, stdOut, el);
                        }
                        break;
                    case "CloseStream":
                        return;
                    default:
                        ConsoleProgram.LogElement(nonParsedElementsWriter, stdOut, el);
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

    public static class Favorites
    {
        private static readonly List<string> list = new List<string>();
        public static bool Get()
        {
            Noxon.ResetListMinMax();
            Macro mf = new iRadio.Macro("Favorites.Get.F", new string[] {"F", "F"});  // select (F)avorites, 2 times to update Show.lastBrowsedLines
            while (mf.Step()) ;
            if (Noxon.GetListMinMax(out int min, out int max) && Show.lastBrowsedTitle == iRadioConsole.Properties.Resources.NoxonTitleFavorites)
            {
                list.Clear();
                int entries = max - min + 1;
                for (int i = 0; i < Math.Min(Noxon.ListLines, entries); i++)    // first 4 (or less) 
                {
                    if (Show.lastBrowsedLines[i] != "")
                    {
                        list.Add(Show.lastBrowsedLines[i]);
                        System.Diagnostics.Debug.WriteLine("Favorites.Get(): list[{0}] = {1}", list.Count, list.Last());
                    }
                }
                if (entries > 4)
                {
                    Macro md3 = new iRadio.Macro("Favorites.Get.D", new string[] { "D", "D", "D" }); // scroll 3 entries down to 4th 
                    while (md3.Step()) ;
                    for (int i = Noxon.ListLines; i < entries; i++)
                    {
                        Macro md = new iRadio.Macro("Favorites.Get.D", new string[] { "D" });
                        while (md.Step()) ;
                        list.Add(Show.lastBrowsedLines[3]);
                        System.Diagnostics.Debug.WriteLine("Favorites.Get(): list[{0}] = {1}", list.Count, list.Last());
                    }
                }
                Macro mh = new iRadio.Macro("Favorites.Get.D", new string[] { "H"}); // home again
                while (mh.Step()) ;
            }
            return true;
        }
    }
}
