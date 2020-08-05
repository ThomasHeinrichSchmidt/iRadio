using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using System.Xml.Linq;


namespace iRadio
{
    // TODO: add radio hardware keys (stop, play, ...)
    // TODO: Favorites dialog (show, double click to play), class Favorites remembers 'flags' p, ps, d, ds -- show in separate console list
    // TODO: F1 - F10 Favoriten #1 - #10
    // TODO: show seperate window with remote control and clickable keys
    // TODO: iRadio: add tool tips with id="artist" to buttons [1], [2],...
    // TODO: iRadio: add tool tips to listBoxDisplay (only if text longer than box?)
    // TODO: add more tests, using moq
    // TODO: add commands (<key> <pause> <comment>) to Telnet.xml for 'interactive' testing - but what do you want to test then?
    // TODO: blank/invalidate listBoxDisplay on <update id="config">  <text id = "title" > Netzwerkassistent </ text > </ update >
    // TODO: README.md: add NoxonRemoteLetters.jpg (updated with current keys)
    //                  ![NOXON Remote Keymap](https://github.com/ThomasHeinrichSchmidt/iRadio/blob/master/iRadioConsole/Properties/NoxonRemoteLetters.jpg?raw=true "NOXON Remote Keymap")
    // TODO: enable scripting/macros: record, play sequence of remote control keys (check NOXON feedback and/or busy to keep in sync) - e.g. for quick selection of some playlist 
    // TODO: localize NOXON resource strings https://stackoverflow.com/questions/1142802/how-to-use-localization-in-c-sharp
    // TODO: add settings to app.config (timeout for timeoutTimer, last IP address used)
    // TODO: ILRepack/ILMerge iRadio.exe

    // ========================
    // DONE: avoid to freeze on XElement.ReadFrom(reader) if iRadio does not transmit any more - timeoutTimer.Start() only if "Nicht verfÃ¼gbar", ... ("browse" OK!)
    //       correct: Turn on NOXON (cold boot), "5" (Preset 5), (L)eft ==> Crash, iRadioConsole freezes: does not longer detect KEYs and netstream, must close/re-open socket.
    //       correct: freeze "NOXON"
    //       corrected: close stream if "Nicht verfÃ¼gbar"
    // DONE: handle "Connect to NOXON iRadio failed (TimedOut, Ein Verbindungsversuch ist fehlgeschlagen, da die Gegenstelle nach einer bestimmten Zeitspanne nicht richtig reagiert hat, oder die hergestellte Verbindung war fehlerhaft, da der verbundene Host nicht reagiert hat 192.168.2.99:10100)
    // DONE: nothing transmitted any more after  <text id="line0">Nicht verfÃ¼gbar</text>
    // DONE: show/enable search text entry if possible with radio 
    // DONE: double click on status bar opens Telnet message listbox
    // DONE: Settings: remember last IP, toggle timestamps in .logs, 
    // DONE: ProcessKeyPressed(): add more keys, update NoxonRemoteLetters.jpg
    // DONE: iRadio: correct PingHosts() for Noxon.OpenAsync()
    // DONE: now works only if Noxon.Parse() is called, seems not to wait on 'busy' 
    //       how to change sequence of Favoriten? NOT POSSIBLE. Need to have 1-3 at top of list, otherwise F1-F3 does not really make sense
    // DONE: search for NOXON (Noxon-iRadio?), not IP // tracert  192.168.178.36  -->  001B9E22FBB7.fritz.box [192.168.178.36]  // MAC Address: 00:1B:9E:22:FB:B7   // Nmap 7.70 scan  Host: 192.168.178.36 (001B9E22FBB7.fritz.box)	Status: Up
    //       would need to scan local (?) IP addresses to find host like MAC address and then probe port 10100, see Ping.cs 
    //          >cscs.exe Ping.cs
    //              192.168.178.1 is up: (0 ms)
    //              192.168.178.49 is up: (0 ms)
    //              192.168.178.40 is up: (1 ms)
    //              192.168.178.36 is up: (51 ms)
    //              192.168.178.44 is up: (105 ms)
    //              Took 274 milliseconds. 5 hosts active.
    // DONE: retrieve list of favorites: "KEY_FAVORITES" "KEY_DOWN" with  <value id="listpos" min="1" max="26">1</value>    UNTIL  max  
    // DONE: ConsoleKey.F1: run macro to choose Favourite #1 - provide class Macro storing desired commands and execution state, ignore keyboard commands during execution
    // DONE: add searching for keyword by using remote control digits for letters  (1x 2 = a, 2x 2 = b, 3x 2 = c, etc.) - how long to wait for enter next char = 1100ms (same = 100ms)
    //       (check NOXON feedback and/or busy to keep in sync)
    // DONE: improve Browse (avoid blank lines)
    //       correct browse display: media@... / Musik / Ordner (sind nur 2, es wird aber der Rest von vorher angezeigt)
    //         <update id="browse">
    //            <text id="line2" flag="ds">Interpreten</text>
    //            <text id="line3" flag="d">Alben</text>
    //         </update>
    //         <update id="browse">
    //            <text id="line1" flag="ds">Wiedergabelisten</text>
    //            <text id="line2" flag="d">Interpreten</text>
    //         </update>
    //       Meine Stationen / Heavy Metal / (3)
    // DONE: add iRadio-Welcome.txt 
    //      <update id = "welcome" >
    //        < icon id="welcome" text="wlan@ths / wlan@t-h-schmidt.de">welcome</icon>
    //      </update>
    // DONE: handle <Dummy /> - radio started to transmit this starting today 17.6.2020 (hä?) - own mistake, removed.
    // DONE: add unit tests, e.g. CreateMultiPressCommands
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


    public class ConsoleProgram
    {

        public static char keypressed = ' ';
        public static System.Timers.Timer unShowKeyPressedTimer;
        public static System.Timers.Timer keyPressedTimer;
        public static Show ConsoleShow = new Show();

        static void Main(string[] args)
        {

            Noxon.Testmode = false;
            if (args.Length > 0) Console.WriteLine("args not yet implemented.");

            FileStream ostrm1, ostrm2;  // pepare to re-direct Console.WriteLine
            StreamWriter nonParsedElementsWriter, parsedElementsWriter;
            TextWriter stdOut = Console.Out;
            Show.columnBrowse = Console.BufferWidth / 2 + 2;
            Show.columnHeader = Console.BufferWidth / 2 - 5;

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

            if (Noxon.Testmode)
            {
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
            }

            if (Noxon.Testmode)
            {
                Console.WriteLine("iRadio Telnet.xml:");
                Console.SetOut(stdOut); // stop re-direct
                                        // use Console cursor control from now on 
                ConsoleShow.Header();
                StreamReader TelnetFile = new StreamReader("Telnet.xml");
                IEnumerable<XElement> iRadioData =
                    from el in StreamiRadioDoc(TelnetFile)
                    select el;
                Noxon.Parse(iRadioData, null, nonParsedElementsWriter, stdOut, ConsoleShow);  // don't log parsed elements
            }

            if (Noxon.Testmode)
            {
                CloseStreams(ostrm1, ostrm2, nonParsedElementsWriter, parsedElementsWriter);
                Environment.Exit(1);
            }

            // https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/linq/how-to-stream-xml-fragments-from-an-xmlreader
            ConsoleShow.Header();
            while (true)
            {
                Noxon.Open();
                IEnumerable<XElement> iRadioNetData =
                    from el in StreamiRadioNet(Noxon.netStream)
                    select el;

                Noxon.Parse(iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut, ConsoleShow);
                // new Thread(delegate () {Noxon.Parse(Noxon.netStream, iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut); }).Start();

                Noxon.Close();
            }
            // CloseStreams(ostrm1, ostrm2, nonParsedElementsWriter, parsedElementsWriter);
        }

        private static void ResetShowKeyPressed(object sender, ElapsedEventArgs e)
        {
            // reset key display 
            if (keypressed != ' ' ) ConsoleShow.Line("Key=", Lines.Status + 1, new XElement("value", "  "));
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
                        // ProbingSendLetters();
                        Console.WriteLine("F1 pressed");
                        ch = ' ';
                        int sel = 7;
                        switch (sel) {
                            case 6: Noxon.Macro = new iRadio.Macro("F1", new string[] { "N", "R", "R", "@hr3", "R", "R" }); break;  // macro executed in Noxon.Parse(), i.e. Internetradio ... hr3  
                            case 7: Favorites.Get(); break;
                            default: break;
                        }
                        break;
                    default:
                        ch = c.KeyChar;
                        break;
                }
                // if (ch == 'q') break;
                if (Noxon.Commands.ContainsKey(ch))
                {
                    if (Noxon.netStream != null)
                    {
                        Noxon.netStream.Command(ch);
                        keypressed = ch;
                        ConsoleShow.Line("Key=", Lines.Status + 1, new XElement("value", keypressed + " > " + Noxon.Commands[ch].Desc));
                        unShowKeyPressedTimer.Start();
                    }
                }
            }
        }




        public static void LogElement(StreamWriter nonParsedElementsWriter, TextWriter stdOut, XElement el)
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
                        if (XElement.ReadFrom(reader) is XElement el)
                            yield return el;
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        private static XmlReader reader;

        static IEnumerable<XElement> StreamiRadioNet(ITestableNetworkStream netStream)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false  };
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None, Encoding.GetEncoding("ISO-8859-1"));  // needed to avoid exception "WDR 3 zum Nachhören"
            string[] waiting = new string[] { @" \ ", " | ", " / ", " - "};
            int waited = 0;

            using (reader = XmlReader.Create(netStream.GetStream(), settings, context))                                             //                                           ^---
            {
                while (true)
                {
                    if (reader.EOF)
                    {
                        Thread.Sleep(200);  // need to re-open netstream, but how?
                        string waitingForSignal = "     waiting for signal  " + waiting[waited++ % 4] + "                "; // + "connected=" + netStream.Socket.connected;
                        ConsoleShow.Status(new XElement("value", waitingForSignal), Lines.Waiting);
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
                                el = XElement.ReadFrom(reader) as XElement;  // can ReadFrom() forever, if iRadio = "Nicht verfügbar" or "NOXON"
                            }
                            catch
                            {
                                el = new XElement("ConsoleStreamiRadioExceptionXElementAfterReadFromFails");
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

        private static void ProcessKeyParseTimeout(object sender, ElapsedEventArgs e)
        {
            // System.Timers.Timer timeoutTimer;
            // timeoutTimer = new System.Timers.Timer(100);        // check if ReadFrom(reader) times out
            // timeoutTimer.Elapsed += ProcessKeyParseTimeout;

            // timeoutTimer.Start();
            // ...
            // timeoutTimer.Stop();

            reader.Close();  // does not make the blocking ReadFrom() call to return
        }


    }
}
