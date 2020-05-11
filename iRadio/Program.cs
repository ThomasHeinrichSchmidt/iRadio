using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace iRadio
{
    // Done: parse Telnet.xml w/o <root>: done, use fragment, XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
    // ToDo: switch on messages while parsing


    // https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/linq/how-to-stream-xml-fragments-from-an-xmlreader
    class Program
    {
        static void Main(string[] args)
        {
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
            StreamReader TelnetFile = new StreamReader("Telnet.xml");
            IEnumerable<XElement> iRadioData =
                from el in StreamiRadioDoc(TelnetFile)
                select el;
            foreach (XElement el  in iRadioData)
            {
                switch (el.Name.ToString())
                {
                    case "update":
                        if (el.Attribute("id").Value == "play")
                        {
                            if (el.Element("value") != null && el.Element("value").Attribute("id").Value == "timep")
                            {
                                int s = int.Parse(el.Value.Trim('\r', '\n', ' '));
                                Console.WriteLine("Playing for {0}:{1:00}", s / 60, s % 60);
                            }
                            else if (el.Element("text") != null && el.Element("text").Attribute("id").Value == "track")
                            {
                                Console.WriteLine("Playing track '{0}'", el.Value.Trim('\r', '\n').Trim());
                            }

                        }
                        break;
                    case "view":
                        if (el.Attribute("id").Value == "status") Console.WriteLine("Status: value = {0}", el.Element("value").Value);  // TodO: find out how to enumerate child data   
                        ;
                        break;
                    default:
                        Console.WriteLine("{0}: {1}, {2} = {3}", el.NodeType, el.Name, el.Attribute("id"), el.Value.Trim());
                        break;
                }
                
            }

            // Console.ReadLine();
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
            foreach (XElement el in iRadioNetData)
            {
                if (el.NodeType != XmlNodeType.EndElement) Console.WriteLine("{0}: {1}, {2}", el.NodeType, el.Name, el.Value);
            }
            tcpClient.Close();
            netStream.Close();
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
                reader.MoveToContent();
                while (reader.Read())
                {
                    // Console.WriteLine(reader.Value); 
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            XElement el = XElement.ReadFrom(reader) as XElement;
                            if (el != null)
                                yield return el;
                            break;
                    }
                }
            }
        }
    }
}
