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

            string markup = @"  <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1698 </value > </update >  
                                <update id=""play"" > <value id =""timep"" min=""0"" max=""65535"" > 1699 </value > </update >  
                             ";   //  if missing: unexpected end of file. Elements not closed: Root.

            // string rootnode = "<Root>  </Root>";
            XmlReader _root = RootedXmlReader.Create(new StringReader(markup));
            _root.Read();
            _root.Read();
            _root.Read();
            _root.Read();


            IEnumerable<string> grandChildData =
                from el in StreamRootChildDoc(new StringReader(markup_child))
                where (int)el.Attribute("Key") > 1
                select (string)el.Element("GrandChild");

            IEnumerable<string> playData =
                from el in StreamiRadioDoc(new StringReader(markup))
                where (string)el.Attribute("id") == "play"
                select (string)el.Element("value");

            StreamReader TelnetFile = new StreamReader("Telnet.xml");
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
            using (XmlReader reader = RootedXmlReader.Create(stringReader))
            {
                // reader.MoveToContent();
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
            using (XmlReader reader = XmlReader.Create(stringReader))
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
        private static bool start = true;
        private static bool root = false;
        private string rootnode = "<Root>  </Root>";

        private RootedXmlReader(TextReader reader)
        {
            _reader = XmlReader.Create(reader);
            _root = XmlReader.Create(new StringReader(rootnode));
        }
        public static new XmlReader Create(TextReader input)
        {
            start = true;
            root = false;
            return new RootedXmlReader(input);
        }

        public override bool Read()
        {
            if (root) start = false;
            if (start)
            {
                root = true;
                return _root.Read();
            }
            else
            {
                return _reader.Read();
            }
        }
        #region Wrapper Boilerplate
        public override XmlNodeType NodeType => (start) ? _root.NodeType : _reader.NodeType;
        public override string LocalName => (start) ? _root.LocalName : _reader.LocalName;
        public override string NamespaceURI => (start) ? _root.NamespaceURI : _reader.NamespaceURI;
        public override string Prefix => (start) ? _root.Prefix : _reader.Prefix;
        public override string Value => (start) ? _root.Value : _reader.Value;
        public override int Depth => (start) ? _root.Depth : _reader.Depth;
        public override string BaseURI => (start) ? _root.BaseURI : _reader.BaseURI;
        public override bool IsEmptyElement => (start) ? _root.IsEmptyElement : _reader.IsEmptyElement;
        public override int AttributeCount => (start) ? _root.AttributeCount : _reader.AttributeCount;
        public override bool EOF => (start) ? _root.EOF : _reader.EOF;
        public override ReadState ReadState => (start) ? _root.ReadState : _reader.ReadState;
        public override XmlNameTable NameTable => (start) ? _root.NameTable : _reader.NameTable;
        public override string GetAttribute(string name) => (start) ? _root.GetAttribute(name) : _reader.GetAttribute(name);
        public override string GetAttribute(string name, string namespaceURI) => (start) ? _root.GetAttribute(name, namespaceURI) : _reader.GetAttribute(name, namespaceURI);
        public override string GetAttribute(int i) => (start) ? _root.GetAttribute(i) : _reader.GetAttribute(i);
        public override string LookupNamespace(string prefix) => (start) ? _root.LookupNamespace(prefix) : _reader.LookupNamespace(prefix);
        public override bool MoveToAttribute(string name) => (start) ? _root.MoveToAttribute(name) : _reader.MoveToAttribute(name);
        public override bool MoveToAttribute(string name, string ns) => (start) ? _root.MoveToAttribute(name, ns) : _reader.MoveToAttribute(name, ns);
        public override bool MoveToElement() => (start) ? _root.MoveToElement() : _reader.MoveToElement();
        public override bool MoveToFirstAttribute() => (start) ? _root.MoveToFirstAttribute() : _reader.MoveToFirstAttribute();
        public override bool MoveToNextAttribute() => (start) ? _root.MoveToNextAttribute() : _reader.MoveToNextAttribute();
        public override bool ReadAttributeValue() => (start) ? _root.ReadAttributeValue() : _reader.ReadAttributeValue();
        public override void ResolveEntity() => _reader.ResolveEntity();  // (start) ? _root.ResolveEntity() : _reader.ResolveEntity();
        #endregion Wrapper Boilerplate
    }
}
