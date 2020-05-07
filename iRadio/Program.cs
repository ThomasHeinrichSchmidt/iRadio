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

    // https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/linq/how-to-stream-xml-fragments-from-an-xmlreader
    class Program
    {
        static void Main(string[] args)
        {
            string markup = @"<Root>  
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

            IEnumerable<string> grandChildData =
                from el in StreamRootChildDoc(new StringReader(markup))
                where (int)el.Attribute("Key") > 1
                select (string)el.Element("GrandChild");

            foreach (string str in grandChildData)
            {
                Console.WriteLine(str);
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
