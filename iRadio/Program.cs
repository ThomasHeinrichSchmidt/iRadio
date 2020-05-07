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

            string markup = @"<Root> 
                                <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1698 </value > </update >  
                                <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1699 </value > </update >  
                             </Root>";   //  if missing: unexpected end of file. Elements not closed: Root.

            IEnumerable <string> grandChildData =
                from el in StreamRootChildDoc(new StringReader(markup_child))
                where (int)el.Attribute("Key") > 1
                select (string)el.Element("GrandChild");

            IEnumerable<string> playData =
                from el in StreamiRadioDoc(new StringReader(markup))
                where (string)el.Attribute("id") == "play"
                select (string)el.Element("value");

            RootStreamReader TelnetFile = new RootStreamReader("Telnet.xml");
            IEnumerable<string> iRadioData =
                from el in StreamiRadioDoc(TelnetFile)
                where (string)el.Attribute("id") == "play"
                select (string)el.Element("value");

            Console.WriteLine("Child data:");
            foreach (string str in grandChildData)
            {
                Console.WriteLine(str);
            }
            Console.WriteLine("iRadio test data:");
            foreach (string str in playData)
            {
                Console.WriteLine(str);
            }
            Console.WriteLine("Telnet.xml:");
            foreach (string str in iRadioData)
            {
                Console.WriteLine(str);
            }
        }

        static IEnumerable<XElement> StreamiRadioDoc(TextReader stringReader)
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


    // https://stackoverflow.com/questions/56260094/is-it-possible-to-alter-the-node-value-with-xmlreader
    class RootedXmlReader : XmlReader
    {
        private XmlReader _reader;
        private XmlReader _root;
        private bool start = false;
        private string rootnode = "<Root>";

        private RootedXmlReader(TextReader reader)
        {
            _reader = XmlReader.Create(reader);
            _root = XmlReader.Create(rootnode);
        }
        public override bool Read()
        {
            if (start)
            {
                start = false;
                return _root.Read();
            }
            else
            {
                return _reader.Read();
            }
        }
        #region Wrapper Boilerplate
        public override XmlNodeType NodeType => _reader.NodeType;
        public override string LocalName => _reader.LocalName;
        public override string NamespaceURI => _reader.NamespaceURI;
        public override string Prefix => _reader.Prefix;
        public override string Value => _reader.Value;
        public override int Depth => _reader.Depth;
        public override string BaseURI => _reader.BaseURI;
        public override bool IsEmptyElement => _reader.IsEmptyElement;
        public override int AttributeCount => _reader.AttributeCount;
        public override bool EOF => _reader.EOF;
        public override ReadState ReadState => _reader.ReadState;
        public override XmlNameTable NameTable => _reader.NameTable;
        public override string GetAttribute(string name) => _reader.GetAttribute(name);
        public override string GetAttribute(string name, string namespaceURI) => _reader.GetAttribute(name, namespaceURI);
        public override string GetAttribute(int i) => _reader.GetAttribute(i);
        public override string LookupNamespace(string prefix) => _reader.LookupNamespace(prefix);
        public override bool MoveToAttribute(string name) => _reader.MoveToAttribute(name);
        public override bool MoveToAttribute(string name, string ns) => _reader.MoveToAttribute(name, ns);
        public override bool MoveToElement() => _reader.MoveToElement();
        public override bool MoveToFirstAttribute() => _reader.MoveToFirstAttribute();
        public override bool MoveToNextAttribute() => _reader.MoveToNextAttribute();
        public override bool ReadAttributeValue() => _reader.ReadAttributeValue();
        public override void ResolveEntity() => _reader.ResolveEntity();
        #endregion Wrapper Boilerplate
    }

    class RootStreamReader : StreamReader
    {
        public RootStreamReader(string path)
          : base(path)
        {
        }

        private bool start = false;
        private string rootnode = "<Root>";
        private int rootnodecount = 0;

        public override string ReadLine()
        {
            if (start)
            {
                start = false;
                return rootnode + base.ReadLine();
            }
            else {
                return base.ReadLine();
            }
        }

        public override int Read()
        {
            if (start)
            {
                if (rootnodecount >= rootnode.Length)
                {
                    start = false;
                }
                return (int) rootnode[rootnodecount++];

            }
            else
            {
                return base.Read();
            }
        }

    }
}
