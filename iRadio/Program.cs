using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace iRadio
{
    // ToDo: display source messages if not detected/parsed


    // Done: use LINQ for spotting data in XMLs
    // Done: switch on messages while parsing
    // Done: parse Telnet.xml w/o <root>: done, use fragment, XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };


    // https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/linq/how-to-stream-xml-fragments-from-an-xmlreader
    class Program
    {
        static void Main(string[] args)
        {
            FileStream ostrm;  // pepare to re-direct Console.WriteLine
            StreamWriter writer;
            TextWriter stdOut = Console.Out;
            try
            {
                ostrm = new FileStream("./iRadio.txt", FileMode.Create, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open iRadio.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }

            Console.SetOut(writer); // re-direct to file 
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
            Console.Clear();
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.WriteLine("NOXON iRadio");

            StreamReader TelnetFile = new StreamReader("Telnet.xml");
            IEnumerable<XElement> iRadioData =
                from el in StreamiRadioDoc(TelnetFile)
                select el;
            Parse(iRadioData, writer, stdOut);

            writer.Close();
            ostrm.Close();
            Environment.Exit(1);


            Console.WriteLine("iRadio Telnet port 10100:");
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connect?view=netcore-3.1
            // Uses a remote endpoint to establish a socket connection.
            TcpClient tcpClient = new TcpClient();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.36"), 10100);
            tcpClient.Connect(ipEndPoint);
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.getstream?view=netcore-3.1
            // Uses the GetStream public method to return the NetworkStream.
            NetworkStream netStream = tcpClient.GetStream();

            IEnumerable<XElement> iRadioNetData =
                from el in StreamiRadioNet(netStream)
                select el;
            Parse(iRadioNetData, writer, stdOut);
            tcpClient.Close();
            netStream.Close();
        }


        private static void Parse(IEnumerable<XElement> iRadioData, StreamWriter writer, TextWriter stdOut)
        {
            foreach (XElement el in iRadioData)
            {
                // int timep;  // using LINQ is not really more readable ...
                // XElement elem = el.DescendantsAndSelf("update").Where(r => r.Attribute("id").Value == "play").FirstOrDefault();  // == null || <update id="play"> < value id = "timep" min = "0" max = "65535" > 1698 </ value >
                // if ((elem = el.DescendantsAndSelf("update").Where(r => r.Attribute("id").Value == "play" && r.Element("value").Attribute("id").Value == "timep").FirstOrDefault()) != null) timep = int.Parse(elem.Value.Trim('\r', '\n', ' ')); 

                Thread.Sleep(100); // 100ms

                switch (el.Name.ToString())
                {
                    case "update":
                        if (el.Attribute("id").Value == "play")
                        {
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "timep")
                            {
                                ShowPlayingTime(el);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "track")
                            {
                                ShowTrack(el);
                            }
                        }
                        break;
                    case "view":
                        if (el.Attribute("id").Value == "play")
                        {
                            foreach (XElement e in el.Elements())
                            {
                                if (e.Name == "text" && e.Attribute("id").Value == "title")
                                {
                                    ShowTitle(e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "artist")
                                {
                                    ShowArtist(e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "album")
                                {
                                    ShowAlbum(e);
                                }
                                else if (e.Name == "text" && e.Attribute("id").Value == "track")
                                {
                                    // Console.WriteLine("Track '{0}'", e.Value.Trim('\r', '\n').Trim());
                                    ShowTrack(e);
                                }
                                else if (e.Name == "value" && e.Attribute("id").Value == "timep")
                                {
                                    // int s = int.Parse(e.Value.Trim('\r', '\n', ' '));
                                    // Console.WriteLine("Playing @ {0:00}:{1:00}", s / 60, s % 60);
                                    ShowPlayingTime(e);
                                }
                            }
                        }
                        if (el.Attribute("id").Value == "status")
                        {
                            Console.WriteLine("Status, value = {0}", el.Element("value").Value);
                            foreach (XElement e in el.Elements())
                            {
                                if (e.Name == "icon" && e.Attribute("id").Value == "play")
                                {
                                    ShowStatus(e);
                                }
                            }

                        }
                        if (el.Attribute("id").Value == "msg")
                        {
                            if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "scrid")
                            {
                                // Console.WriteLine(".");
                                ShowStatus(el);
                            }
                        }
                        break;
                    default:
                        Console.SetOut(writer); // re-direct
                        Console.WriteLine("{0}", el.ToString());
                        Console.SetOut(stdOut); // stop re-direct
                        break;
                }
            }
        }

        private static void ShowAlbum(XElement e)
        {
            Console.CursorTop = 1;
            Console.CursorLeft = 0;
            Console.WriteLine(new String(' ', 50));
            Console.CursorTop = 1;
            Console.CursorLeft = 0;
            Console.WriteLine("Album '{0}'", e.Value.Trim('\r', '\n').Trim());
        }

        private static void ShowArtist(XElement e)
        {
            Console.CursorTop = 2;
            Console.CursorLeft = 0;
            Console.WriteLine(new String(' ', 50));
            Console.CursorTop = 2;
            Console.CursorLeft = 0;
            Console.WriteLine("Artist '{0}'", e.Value.Trim('\r', '\n').Trim());
        }

        private static void ShowTitle(XElement e)
        {
            Console.CursorTop = 3;
            Console.CursorLeft = 0;
            Console.WriteLine(new String(' ', 50));
            Console.CursorTop = 3;
            Console.CursorLeft = 0;
            Console.WriteLine("Title '{0}'", e.Value.Trim('\r', '\n').Trim());
        }

        private static void ShowTrack(XElement el)
        {
            Console.CursorTop = 4;
            Console.CursorLeft = 0;
            Console.WriteLine(new String(' ', 50));
            Console.CursorTop = 4;
            Console.CursorLeft = 0;
            Console.WriteLine("Playing track '{0}'", el.Value.Trim('\r', '\n').Trim());
        }

        private static void ShowPlayingTime(XElement el)
        {
            Console.CursorTop = 5;
            Console.CursorLeft = 0;
            int s = int.Parse(el.Value.Trim('\r', '\n', ' '));
            Console.WriteLine("Playing for {0:00}:{1:00}", s / 60, s % 60);
        }

        private static void ShowStatus(XElement e)
        {
            Console.CursorTop = 10;
            Console.CursorLeft = 0;
            Console.WriteLine("Status Icon '{0}'", e.Value.Trim('\r', '\n').Trim());
        }


        static IEnumerable<XElement> StreamiRadioDoc(TextReader stringReader)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
            using (XmlReader reader = XmlReader.Create(stringReader, settings))
            {
                // reader.MoveToContent();
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element) { 
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
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
            using (XmlReader reader = XmlReader.Create(netStream, settings))
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
    }
}
