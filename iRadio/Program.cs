using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace iRadio
{
    // ToDo: parse Telnet.xml w/o <root>
    // ToDo: switch on messages while parsing


    // https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/linq/how-to-stream-xml-fragments-from-an-xmlreader
    class Program
    {
        static void Main(string[] args)
        {
            string markup_child = @"<Root>  
                  <Child Key=""01"">  
                    <GrandChild>aaa</GrandChild>  
                  </Child>  
                  <Child Key=""02"">  
                    <GrandChild>bbb</GrandChild>  
                  </Child>  
                  <Child Key=""03"">  
                    <GrandChild>ccc</GrandChild>  
                  </Child>  
                </Root>";

            string markup = @"<iRadio> 
                                <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1698 </value > </update >  
                             </iRadio>";

            IEnumerable <string> grandChildData =
                from el in StreamRootChildDoc(new StringReader(markup_child))
                where (int)el.Attribute("Key") > 1
                select (string)el.Element("GrandChild");

            IEnumerable<string> playData =
                from el in StreamiRadioDoc(new StreamReader("Telnet.xml"))
                where (string)el.Attribute("id") == "play"
                select (string)el.Element("value");

            foreach (string str in grandChildData)
            {
                Console.WriteLine(str);
            }
            foreach (string str in playData)
            {
                Console.WriteLine(str);
            }
        }

        static IEnumerable<XElement> StreamiRadioDoc(StreamReader stringReader)
        {
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stringReader))
            {
                reader.MoveToContent();
                // Parse the file and display each of the nodes.  
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
        static IEnumerable<XElement> StreamRootChildDoc(StringReader stringReader)
        {
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stringReader))
            {
                reader.MoveToContent();
                // Parse the file and display each of the nodes.  
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "Child")
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
    }
}
