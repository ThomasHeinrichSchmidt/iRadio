﻿using System;
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

    // ToDo: process commands 0 ... 5 + more keys on front panel of radio? (stop, rev, play/stop, fw, < ^ > v   w a s d   // WRC service @ WaaRemoteCtrl.cpp
    // ToDo: avoid to freeze on XElement.ReadFrom(reader) if iRadio does not transmit any more 
    // ToDo: search for NOXON (Noxon-iRadio?), not IP // tracert  192.168.178.36  -->  001B9E22FBB7.fritz.box [192.168.178.36]  // MAC Address: 00:1B:9E:22:FB:B7   // Nmap 7.70 scan  Host: 192.168.178.36 (001B9E22FBB7.fritz.box)	Status: Up
    //       would need to scan local (?) IP addresses to find host like MAC address and then probe port 10100.

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
    class Program
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
        public const int lineWaiting = 11;

        public static char keypressed = ' ';

        struct NoxonCommand
        {
            public int Key;
            public string Desc;
            public NoxonCommand (int key, string desc)
            {
                Key = key;
                Desc = desc;
            }
        }

        Dictionary<char, NoxonCommand> NoxonCommands = new Dictionary<char, NoxonCommand>()
        {
            { 'L', new NoxonCommand(0x25, "KEY_LEFT") },
            { 'U', new NoxonCommand(0x26, "KEY_UP") },
            { 'R', new NoxonCommand(0x27, "KEY_RIGHT") },
            { 'D', new NoxonCommand(0x28, "KEY_DOWN") },
            { 'F', new NoxonCommand(0xAB, "KEY_FAVORITES") },
            { 'H', new NoxonCommand(0xAC, "KEY_HOME") },
            { '-', new NoxonCommand(0xAE, "KEY_VOL_DOWN") },
            { '+', new NoxonCommand(0xAF, "KEY_VOL_UP") },
            { '>', new NoxonCommand(0xB0, "KEY_NEXT") },
            { '<', new NoxonCommand(0xB1, "KEY_PREVIOUS") },
            { 'S', new NoxonCommand(0xB2, "KEY_STOP") },
            { 'P', new NoxonCommand(0xB3, "KEY_PLAY") },
            { 'I', new NoxonCommand(0xBA, "KEY_INFO") },
            { 'R', new NoxonCommand(0xC0, "KEY_REPEAT") },
            { '*', new NoxonCommand(0xDB, "KEY_SETTINGS") },
            { 'X', new NoxonCommand(0xDC, "KEY_SHUFFLE") },
            { '0', new NoxonCommand(0x30, "KEY_0") },
            { '1', new NoxonCommand(0x31, "KEY_1") },
            { '2', new NoxonCommand(0x32, "KEY_2") },
            { '3', new NoxonCommand(0x33, "KEY_3") },
            { '4', new NoxonCommand(0x34, "KEY_4") },
            { '5', new NoxonCommand(0x35, "KEY_5") },
            { '6', new NoxonCommand(0x36, "KEY_6") },
            { '7', new NoxonCommand(0x37, "KEY_7") },
            { '8', new NoxonCommand(0x38, "KEY_8") },
            { '9', new NoxonCommand(0x39, "KEY_9") }
        };


        static void Main(string[] args)
        {
            FileStream ostrm1, ostrm2;  // pepare to re-direct Console.WriteLine
            StreamWriter nonParsedElementsWriter, parsedElementsWriter;
            TextWriter stdOut = Console.Out;

            System.Timers.Timer timer = new System.Timers.Timer(1000);  // reset key display after a second
            timer.Elapsed += ResetShowKeyPressed;
            timer.Start();

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
                NetworkStream netStream = tcpClient.GetStream();

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

    private static void ShowHeader()
        {
            Console.Clear();
            Console.Title = "NOXON iRadio";
            Console.CursorVisible = false;
            Console.CursorTop = 0;
            Console.CursorLeft = 10;
            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
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

                if (testmode) Thread.Sleep(1000); // 50ms  used to delay parsing of Telnet.xml, otherwise it's over very quickly
                if (parsedElementsWriter != null)
                {
                    Console.SetOut(parsedElementsWriter); // re-direct
                    Console.WriteLine("{0}", el.ToString());
                    Console.SetOut(stdOut); // stop re-direct
                    parsedElementsWriter.Flush();
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo c = Console.ReadKey(true);
                    char ch = c.KeyChar;
                    if (ch == 'q') break;
                    byte result = 32;  // space
                    try
                    {
                        result = Convert.ToByte(ch);
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine("Unable to convert u+{0} to a byte.", Convert.ToInt16(ch).ToString("X4"));
                    }
                    byte[] buffer = new byte[2];
                    buffer[0] = result;
                    if (netStream != null)
                    {
                        if (netStream.CanWrite) {
                            netStream.Write(buffer, 0, 1);
                        }
                        keypressed = ch;
                        ShowLine("Key=", lineStatus + 1, new XElement("value", keypressed));
                    }
                }

                switch (el.Name.ToString())
                {
                    case "update":
                        if (el.Attribute("id").Value == "play")
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
                        else if (el.Attribute("id").Value == "status")
                        {
                            if (el.Element("icon") != null && el.Element("icon").Attribute("id").Value == "play")
                            {
                                ShowLine("Icon", lineIcon, el);
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
            ClearLine(line);
            Console.CursorTop = line;
            Console.CursorLeft = 0;
            string original = e.Value.Trim('\r', '\n').Trim();
            byte[] encoded = Encoding.GetEncoding(1252).GetBytes(original);  
            string corrected = Encoding.UTF8.GetString(encoded);
            char badc = '\xfffd';
            if (corrected.Contains(badc))
            {
                Console.WriteLine("{0} '{1}'", caption, original);
            }
            else
            {
                Console.WriteLine("{0} '{1}'", caption, corrected);
            }
            Console.CursorTop = lineSeparator;
            Console.CursorLeft = 0;
            Console.WriteLine("{0}", new String('-', Console.WindowWidth));
        }

        private static void ShowPlayingTime(XElement el, int line)
        {
            Console.CursorTop = line;
            Console.CursorLeft = 0;
            int s = int.Parse(el.Value.Trim('\r', '\n', ' '));
            ClearLine(line);
            Console.WriteLine("                     Playing for {0:00}:{1:00}", s / 60, s % 60);
        }

        private static void ShowStatus(XElement e, int line, int line0)
        {
            Console.CursorTop = line;
            Console.CursorLeft = 0;
            Console.WriteLine("Status Icon '{0}'", e.Value.Trim('\r', '\n').Trim());
            if (e.Value.Contains("empty"))
            {
                for (int i = 1; i < line; i++)
                {
                    ClearLine(i);
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
                    Console.CursorLeft = 0;
                    if (elem.Value == "") ClearLine(line0 + i);
                    else Console.WriteLine(elem.Value);
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
                    Console.CursorLeft = 0;
                    if (elem.Value == "") ClearLine(line0 + i);
                    else Console.WriteLine(elem.Value);
                }
            }
        }

        private static void ClearLine(int line)
        {
            int top = Console.CursorTop;
            Console.CursorTop = line;
            Console.CursorLeft = 0;
            Console.WriteLine(new String(' ', Console.WindowWidth));
            Console.CursorTop = top;
            Console.CursorLeft = 0;
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
