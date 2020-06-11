using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;

namespace iRadio
{
    // TODO: add unit tests, e.g. CreateMultiPressCommands
    // TODO: improve Browse (avoid blank lines)
    //       correct browse display: media@... / Musik / Ordner (sind nur 2, es wird aber der Rest von vorher angezeigt)
    //         <update id="browse">
    //            <text id="line2" flag="ds">Interpreten</text>
    //            <text id="line3" flag="d">Alben</text>
    //         </update>
    //         <update id="browse">
    //            <text id="line1" flag="ds">Wiedergabelisten</text>
    //            <text id="line2" flag="d">Interpreten</text>
    //         </update>
    // ToDo: avoid to freeze on XElement.ReadFrom(reader) if iRadio does not transmit any more 
    //       correct Turn on NOXON (cold boot), "5" (Preset 5), (L)eft ==> Crash, iRadioConsole freezes: does not longer detect KEYs and netstream, must close/re-open socket.
    // TODO: F1 - F3 Favoriten #1 - #3 
    // TODO: add searching for keyword by using remote control digits for letters  (1x 2 = a, 2x 2 = b, 3x 2 = c, etc.) - how long to wait for enter next char = 1100ms (same = 100ms)
    //       (check NOXON feedback and/or busy to keep in sync)
    //       
    // TODO: ConsoleKey.F1: run macro to choose Favourite #1
    // TODO: retrieve list of favorites: "KEY_FAVORITES" "KEY_DOWN" with  <value id="listpos" min="1" max="26">1</value>    UNTIL  max  -- show in separate list
    // TODO: enable scripting: record, play sequence of remote control keys (check NOXON feedback and/or busy to keep in sync) - e.g. for quick selection of some playlist 

    // ToDo: search for NOXON (Noxon-iRadio?), not IP // tracert  192.168.178.36  -->  001B9E22FBB7.fritz.box [192.168.178.36]  // MAC Address: 00:1B:9E:22:FB:B7   // Nmap 7.70 scan  Host: 192.168.178.36 (001B9E22FBB7.fritz.box)	Status: Up
    //       would need to scan local (?) IP addresses to find host like MAC address and then probe port 10100.

    // ========================
    // DONE: check 6 missing keys from remote control = ON/OFF (??), "KEY_PRESET"(0x2B), "KEY_DELFAV"(0x2E), "KEY_ADDFAV"(0x2D), "KEY_MUTE"(??), "KEY_INTERNETRADIO"(0xAA)
    // DONE: mark currently selected line - separate "windows" for Browse and Play 
    // DONE: check Console.KeyAvailable continously (without Parse() of incoming XML messages), in separate timer 
    // Done: process commands 0 ... 5 + more keys on front panel of radio? (stop, rev, play/stop, fw, < ^ > v   w a s d   // WRC service @ WaaRemoteCtrl.cpp, see // https://github.com/clementleger/noxonremote
    // Done: 2-iRadio-non-parsed-elements.txt 
    // Done: network stream CanWrite() --> 
    // Done: exception handling and/or enforce XML reading with wrong char set - No, reading not UTF-8, but ISO-8859-1
    // Done: do not use "ISO-8859-9" encoding, ignore or replace character instead (record testing data using Telnet.ps1 on 'WDR 3'), not possible, XML must be well formed always
    // Done: write xml.log from port ('Artist' does not change when preset changes)
    // Done: display source messages if not detected/parsed
    // Done: use LINQ for spotting data in XMLs
    // Done: switch on messages while parsing
    // Done: parse Telnet.xml w/o <root>: done, use fragment, XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };


    // https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/linq/how-to-stream-xml-fragments-from-an-xmlreader
    public class Program
    {
        static bool testmode = false;

        public const int lineTitle = 1;
        public const int lineArtist = 2;
        public const int line0      = 2;
        public const int lineAlbum = 3;
        public const int lineTrack = 4;
        public const int linePlayingTime = 5;
        public const int lineSeparator = 6;
        public const int lineIcon = 7;
        public const int lineWiFi = 8;
        public const int lineBuffer = 9;
        public const int lineStatus = 10;
        public const int lineBusy = 11;
        public const int lineWaiting = 12;
        public static int columnBrowse = 0;
        public static int columnHeader = 10;
        public const int columnShow = 0;

        public static char keypressed = ' ';
        public static System.Timers.Timer unShowKeyPressedTimer;
        public static System.Timers.Timer keyPressedTimer;
        public static NetworkStream netStream = null;


        static void Main(string[] args)
        {
            FileStream ostrm1, ostrm2;  // pepare to re-direct Console.WriteLine
            StreamWriter nonParsedElementsWriter, parsedElementsWriter;
            TextWriter stdOut = Console.Out;
            columnBrowse = Console.BufferWidth / 2 + 2;
            columnHeader = Console.BufferWidth / 2 - 5;

            unShowKeyPressedTimer = new System.Timers.Timer(2000);  // reset key display after a second or two
            unShowKeyPressedTimer.Elapsed += ResetShowKeyPressed;
            keyPressedTimer = new System.Timers.Timer(100);        // loop console for key press
            keyPressedTimer.Elapsed += ProcessKeyPressed;
            keyPressedTimer.Start();

            try
            {
                ostrm1 = new FileStream("./iRadio-non-parsed-elements.txt", FileMode.Create, FileAccess.Write);
                nonParsedElementsWriter = new StreamWriter(ostrm1);
                ostrm2 = new FileStream("./iRadio-logging.txt", FileMode.Create, FileAccess.Write);
                parsedElementsWriter = new StreamWriter(ostrm2);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open iRadio .txt files for writing");
                Console.WriteLine(e.Message);
                return;
            }

            Console.SetOut(nonParsedElementsWriter); // re-direct to file 
            Console.WriteLine("iRadio play data:");
            string markup = @"
                                <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1698 </value > </update >  
                                <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1699 </value > </update >  
                             ";   //  if missing: unexpected end of file. Elements not closed: Root.
            IEnumerable<string> playData =
                from el in StreamiRadioDoc(new StringReader(markup))
                where (string)el.Attribute("id") == "play"
                select (string)el.Element("value");
            foreach (string str in playData)
            {
                Console.WriteLine(str);
            }

            Console.WriteLine("iRadio Telnet.xml:");
            Console.SetOut(stdOut); // stop re-direct
                                    // use Console cursor control from now on 
            ShowHeader();

            StreamReader TelnetFile = new StreamReader("Telnet.xml");
            IEnumerable<XElement> iRadioData =
                from el in StreamiRadioDoc(TelnetFile)
                select el;
            Parse(null, iRadioData, null, nonParsedElementsWriter, stdOut);  // don't log parsed elements

            if (testmode)
            {
                CloseStreams(ostrm1, ostrm2, nonParsedElementsWriter, parsedElementsWriter);
                Environment.Exit(1);
            }

            ShowHeader();

            while (true)
            {
                // Console.WriteLine("iRadio Telnet port 10100:");
                // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connect?view=netcore-3.1
                // Uses a remote endpoint to establish a socket connection.
                TcpClient tcpClient = new TcpClient();
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

                IEnumerable<XElement> iRadioNetData =
                    from el in StreamiRadioNet(netStream)
                    select el;
                Parse(netStream, iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut);
                netStream.Close();
                tcpClient.Close();
            }
            CloseStreams(ostrm1, ostrm2, nonParsedElementsWriter, parsedElementsWriter);
        }

        private static void ResetShowKeyPressed(object sender, ElapsedEventArgs e)
        {
            // reset key display 
            if (keypressed != ' ' ) ShowLine("Key=", lineStatus + 1, new XElement("value", "  "));
        }

        private static void ProcessKeyPressed(object sender, ElapsedEventArgs e)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo c = Console.ReadKey(true);
                char ch;
                switch (c.Key) {
                    case ConsoleKey.LeftArrow:
                        ch = 'L';
                        break;
                    case ConsoleKey.RightArrow:
                        ch = 'R';
                        break;
                    case ConsoleKey.UpArrow:
                        ch = 'U';
                        break;
                    case ConsoleKey.DownArrow:
                        ch = 'D';
                        break;
                    case ConsoleKey.VolumeUp:
                        ch = '+';
                        break;
                    case ConsoleKey.VolumeDown:
                        ch = '-';
                        break;
                    case ConsoleKey.BrowserFavorites:
                        ch = 'F';
                        break;
                    case ConsoleKey.Home:
                        ch = 'H';
                        break;
                    case ConsoleKey.F1:
                        // run macro to choose Favourite #1 - could show list of favorites: "KEY_FAVORITES" with  <value id="listpos" min="1" max="26">1</value> until max
                        // 
                        // ProbingSendLetters();

                        Console.WriteLine("F1 was pressed, starting ReadKey() loop");
                        while (true)
                        {
                            int sel = 5;
                            if (sel == 1)   // 7 Bit ASCII only 
                            {
                                Console.WriteLine("enter character using numpad to sent to NOXON (e.g. Alt+123, but only works for chars < 128)");
                                ConsoleKeyInfo cp = Console.ReadKey(true);    // enter character to sent to NOXON (e.g. Alt+123, but only works for chars < 128)
                                int i = Encoding.GetEncoding("ISO-8859-1").GetBytes(new char[] { cp.KeyChar })[0];
                                ch = cp.KeyChar;
                                System.Diagnostics.Debug.WriteLine("Transmit: ASC({0}={1}) --> 0x{2}", i, ch, BitConverter.ToString(Noxon.intToByteArray(i)));
                                netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
                            }
                            else if (sel == 2)  // two hex digits 
                            {
                                Console.WriteLine("enter character using two hex digits (00..FF) to sent to NOXON");
                                string line = Console.ReadLine();
                                byte[] bytes = { 0, 0, 0, System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Parse(line).Value[0]};
                                System.Diagnostics.Debug.WriteLine("Transmit: Hex = {0} --> 0x{1}", line, BitConverter.ToString(bytes));
                                netStream.Write(bytes, 0, bytes.Length);
                            }
                            else if (sel == 3)  // ascii value 0..255 
                            {
                                Console.WriteLine("enter character using ascii code 0..255 to sent to NOXON");
                                string line = Console.ReadLine();
                                int i = Int32.Parse(line);
                                System.Diagnostics.Debug.WriteLine("Transmit: ASC({0} --> 0x{1})", line, BitConverter.ToString(Noxon.intToByteArray(i)));
                                netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
                            }
                            else if (sel == 4)  // loop all ascii values 
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
                            else if (sel == 5)  // macro: Internetradio ... hr3 
                            {
                                Console.WriteLine("run macro to choose hr3, hold on ...");
                                int i = 170; // KEY_INTERNETRADIO
                                Transmit(i);
                                i = 39; // KEY_RIGHT  --> Alle Sender
                                Transmit(i);
                                i = 39; // KEY_RIGHT  --> Senderliste 
                                Transmit(i);

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
                                Transmit(i);

                                Console.ReadKey();  // wait for find ro complete, 
                                                    //<update id="browse">
                                                    //  <text id="line0" flag="ps">hr3</text>
                                                    //  <text id="line1" flag="d">hr4</text>
                                                    //  <text id="line2" flag="p">Hrvatski Radio Frankfurt</text>
                                                    //  <text id="line3" flag="p">Hrw laut.fm</text>
                                                    //  <icon id="hchyDn">empty</icon>
                                                    //</update>

                                i = 39; // KEY_RIGHT  --> play 
                                Transmit(i);

                                Console.ReadKey();
                            }
                        }
                        break;
                    default:
                        ch = c.KeyChar;
                        break;
                }
                // if (ch == 'q') break;
                if (Noxon.Commands.ContainsKey(ch))
                {
                    if (netStream != null)
                    {
                        if (netStream.CanWrite)
                        {
                            netStream.Write(Noxon.intToByteArray(Noxon.Commands[ch].Key), 0, sizeof(int));
                        }
                        keypressed = ch;
                        ShowLine("Key=", lineStatus + 1, new XElement("value", keypressed + " > " + Noxon.Commands[ch].Desc));
                        unShowKeyPressedTimer.Start();
                    }
                }
            }
        }

        private static void Transmit(int i)
        {
            System.Diagnostics.Debug.WriteLine("Transmit: ASC({0} --> 0x{1})", i, BitConverter.ToString(Noxon.intToByteArray(i)));
            netStream.Write(Noxon.intToByteArray(i), 0, sizeof(int));
            Thread.Sleep(500);
        }

        private static void ProbingSendLetters()
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

        private static void ShowHeader()
        {
            Console.Clear();
            Console.Title = "NOXON iRadio";
            Console.CursorVisible = false;
            Console.CursorTop = 0;
            Console.CursorLeft = columnHeader;
            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("NOXON iRadio");
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        private static void Parse(NetworkStream netStream, IEnumerable<XElement> iRadioData, StreamWriter parsedElementsWriter, StreamWriter nonParsedElementsWriter, TextWriter stdOut)
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
                    Console.WriteLine("{0}", el.ToString());
                    Console.SetOut(stdOut); // stop re-direct
                    parsedElementsWriter.Flush();
                }

                switch (el.Name.ToString())
                {
                    case "update":
                        if (el.Attribute("id").Value == "play")   // <update id="play">
                        {
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "timep")
                            {
                                ShowPlayingTime(el, linePlayingTime);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "buflvl")
                            {
                                ShowLine("Buffer[%]", lineBuffer, el);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "wilvl")
                            {
                                ShowLine("WiFi[%]", lineWiFi, el);
                            }
                            else if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "date")   // <value id="date" 
                            {
                                int i;
                                if (int.TryParse(el.Value, out i) && i > 0) ShowLine("Date", lineStatus, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "track")
                            {
                                ShowLine("Track", lineTrack, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "artist")
                            {
                                ShowLine("Artist", lineArtist, el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "album")
                            {
                                ShowLine("Album", lineAlbum, el);
                            }
                            else
                            {
                                LogElement(nonParsedElementsWriter, stdOut, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "status")  // <update id="status">
                        {
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "play")
                            {
                                ShowLine("Icon-Play", lineIcon, el);
                            }
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "shuffle")
                            {
                                ShowLine("Icon-Shuffle", lineIcon, el);
                            }
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "repeat")
                            {
                                ShowLine("Icon-Repeat", lineIcon, el);
                            }
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "busy")    // <value id="busy" 
                            {
                                ShowLine("Busy=", lineBusy, el);
                            }
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "listpos")    // <value id="listpos" 
                            {
                                string min = el.Element("value").Attribute("min").Value;  // <value id="listpos" min="1" max="26">23</value> 
                                string max = el.Element("value").Attribute("max").Value;
                                string caption = "From (" + min + ".." + max + ") @ ";
                                ShowLine(caption, lineStatus, el);
                            }
                        }
                        else if (el.Attribute("id").Value == "browse")
                        {
                            ShowBrowse(el, line0);
                        }
                        else
                        {
                            LogElement(nonParsedElementsWriter, stdOut, el);
                        }
                        break;
                    case "view":
                        if (el.Attribute("id").Value == "play")
                        {
                            foreach (XElement e in el.Elements())
                            {
                                if (e.Name == "text" && e.Attribute("id").Value == "title")
                                {
                                    ShowLine("Title", lineTitle, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "artist")
                                {
                                    ShowLine("Artist", lineArtist, e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "album")
                                {
                                    ShowLine("Album", lineAlbum,e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "track")
                                {
                                    ShowLine("Track", lineTrack, e);
                                }
                                else if (e.Name == "value" && e.Attribute("id").Value == "timep")
                                {
                                    ShowPlayingTime(e, linePlayingTime);
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
                                    ShowStatus(e, lineStatus, line0);
                                }
                            }

                        }
                        else if (el.Attribute("id").Value == "msg")
                        {
                            if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "scrid")
                            {
                                ShowMsg(el, lineStatus, line0);
                            }
                        }
                        else if (el.Attribute("id").Value == "browse")
                        {
                            if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "scrid")
                            {
                                ShowBrowse(el, line0);
                            }
                        }
                        else
                        {
                            LogElement(nonParsedElementsWriter, stdOut, el);
                        }
                        break;
                    case "CloseStream":
                        return;
                    default:
                        LogElement(nonParsedElementsWriter, stdOut, el);
                        break;
                }
            }
        }

        private static void ShowLine(string caption, int line, XElement e)
        {
            Console.CursorTop = line;
            Console.CursorLeft = columnShow;
            ClearLine(columnShow, line);

            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            if (caption == "Title")
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("{0} '{1}'", caption, Normalize(e));
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;

            Console.CursorTop = lineSeparator;
            Console.CursorLeft = columnShow;
            Console.WriteLine("{0}", new String('-', Console.WindowWidth - Console.CursorLeft -1));
        }

        private static string Normalize(XElement e)
        {
            // string original = e.Value.Trim('\r', '\n').Trim();
            string original = e.Value.Replace('\r', ' ').Replace('\n', ' ').Trim();
            byte[] encoded = Encoding.GetEncoding(1252).GetBytes(original);
            string corrected = Encoding.UTF8.GetString(encoded);
            string normalized;
            char badc = '\xfffd';
            if (corrected.Contains(badc))
            {
                normalized = original;
            }
            else
            {
                normalized = corrected;
            }
            return normalized;
        }

        private static void ShowPlayingTime(XElement el, int line)
        {
            Console.CursorTop = line;
            Console.CursorLeft = columnShow;
            int s = int.Parse(el.Value.Trim('\r', '\n', ' '));
            ClearLine(columnShow, line);
            Console.WriteLine("                     Playing for {0:00}:{1:00}", s / 60, s % 60);
        }

        private static void ShowStatus(XElement e, int line, int line0)
        {
            Console.CursorTop = line;
            Console.CursorLeft = columnShow;
            Console.WriteLine("Status Icon '{0}'", Normalize(e));
            if (e.Value.Contains("empty"))
            {
                for (int i = 1; i < line; i++)
                {
                    // ClearLine(columnShow, i);   //   <icon id="play">empty</icon>    < icon id = "shuffle" > empty </ icon >    < icon id = "repeat" > empty </ icon >
                }
            }
        }

        private static void ShowMsg(XElement e, int line, int line0)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            for (int i = 0; i < 4; i++)
            {
                if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "line" + i).FirstOrDefault()) != null)
                {
                    Console.CursorTop = line0 + i;                     
                    Console.CursorLeft = columnBrowse;
                    if (elem.Value == "")
                    {
                        ClearLine(columnBrowse, line0 + i);
                    }
                    else
                    {
                        Console.WriteLine(Normalize(elem));
                    }

                }
            }
        }

        private static void ShowBrowse(XElement e, int line0)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "title").FirstOrDefault()) != null)
            {
                ShowLine("Title", lineTitle, elem);
            }

            for (int i = 0; i < 4; i++)
            {
                if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "line" + i).FirstOrDefault()) != null)
                {
                    Console.CursorTop = line0 + i;
                    Console.CursorLeft = columnBrowse;
                    ConsoleColor bg = Console.BackgroundColor;
                    ConsoleColor fg = Console.ForegroundColor;
                    ClearLine(columnBrowse, line0 + i);           // if (elem.Value == "")

                    if (elem.Attribute("flag") != null && elem.Attribute("flag").Value == "ds")   //  <text id="line0" flag="ds">History</text>
                    {
                        Console.BackgroundColor = fg;
                        Console.ForegroundColor = bg;
                    }
                    if (elem.Attribute("flag") != null && elem.Attribute("flag").Value == "ps")   //    <text id="line0" flag="ps">Radio Efimera</text>
                    {
                        Console.BackgroundColor = fg;
                        Console.ForegroundColor = bg;
                    }
                    Console.WriteLine(Normalize(elem));  // else
                    Console.BackgroundColor = bg;
                    Console.ForegroundColor = fg;
                }
            }
        }

        private static void ClearLine(int column, int line)
        {
            int top = Console.CursorTop;
            int left = Console.CursorLeft;
            Console.CursorTop = line;
            Console.CursorLeft = column;
            Console.WriteLine(new String(' ', Console.WindowWidth - Console.CursorLeft));
            Console.CursorTop = top;
            Console.CursorLeft = left;
        }

        private static void LogElement(StreamWriter nonParsedElementsWriter, TextWriter stdOut, XElement el)
        {
            Console.SetOut(nonParsedElementsWriter); // re-direct
            Console.WriteLine("{0}", el.ToString());
            Console.SetOut(stdOut); // stop re-direct
            nonParsedElementsWriter.Flush();
        }

        private static void CloseStreams(FileStream ostrm1, FileStream ostrm2, StreamWriter nonParsedElementsWriter, StreamWriter parsedElementsWriter)
        {
            parsedElementsWriter.Close();
            nonParsedElementsWriter.Close();
            ostrm1.Close();
            ostrm2.Close();
        }

        static IEnumerable<XElement> StreamiRadioDoc(TextReader stringReader)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false };
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None, Encoding.GetEncoding("ISO-8859-1"));  // needed to avoid exception "WDR 3 zum Nachhören"
            using (XmlReader reader = XmlReader.Create(stringReader, settings, context))
            {
                // reader.MoveToContent();
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    { 
                        XElement el = XElement.ReadFrom(reader) as XElement;
                        if (el != null)
                            yield return el;
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        static IEnumerable<XElement> StreamiRadioNet(NetworkStream netStream)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false  };
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None, Encoding.GetEncoding("ISO-8859-1"));  // needed to avoid exception "WDR 3 zum Nachhören"
            string[] waiting = new string[] { @" \ ", " | ", " / ", " - "};
            int waited = 0;
            using (XmlReader reader = XmlReader.Create(netStream, settings, context))                                             //                                           ^---
            {
                while (true)
                {
                    if (reader.EOF)
                    {
                        Thread.Sleep(200);  // need to re-open netstream, but how?
                        string waitingForSignal = "     waiting for signal  " + waiting[waited++ % 4] + "                "; // + "connected=" + netStream.Socket.connected;
                        ShowStatus(new XElement("value", waitingForSignal), lineWaiting, line0);
                        if (waited > 5 * 1000 / 200)  // 60s
                        {
                            XElement el = new XElement("CloseStream");
                            yield return el;
                        }
                    }
                    // reader.MoveToContent();
                    while (!reader.EOF)
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            XElement el;
                            try
                            {
                                el = XElement.ReadFrom(reader) as XElement;  // ToDo: can ReadFrom() forever, if iRadio = "Nicht verfügbar" or "NOXON"
                            }
                            catch
                            {
                                el = new XElement("Dummy");
                            }
                            if (el != null)
                                yield return el;
                        }
                        else
                        {
                            try
                            {
                                reader.Read();
                            }
                            catch
                            {
                                // continue
                            }
                        }
                    }
                }
            }
        }
    }
}
