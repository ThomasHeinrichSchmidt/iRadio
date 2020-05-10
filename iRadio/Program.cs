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
    // ToDo: parse Telnet.xml w/o <root>: done, use fragment, XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
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
            IEnumerable<string> iRadioData =
                from el in StreamiRadioDoc(TelnetFile)
                where (string)el.Attribute("id") == "play"
                select (string)el.Element("value");
            foreach (string str in iRadioData)
            {
                Console.WriteLine(str);
            }

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
                Console.WriteLine("{0}: {1}, {2}", el.NodeType, el.Name, el.Value);
            }
            tcpClient.Close();
            netStream.Close();
        }

        static IEnumerable<XElement> StreamiRadioDoc(TextReader stringReader)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
            using (XmlReader reader = XmlReader.Create(stringReader, settings))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "update")
                            {
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                if (el != null)
                                    yield return el;
                            }
                            break;
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
