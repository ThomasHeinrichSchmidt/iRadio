using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;


namespace iRadio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            if (Noxon.netStream != null)
            {
                Task<int> ret = Noxon.netStream.GetNetworkStream().CommandAsync('1');
                await ret;
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Task<bool> isOPen = Noxon.OpenAsync();
            try
            {
                await isOPen;
                button1.Enabled = isOPen.Result;
            }
            catch (SocketException exs)
            {
                MessageBox.Show(exs.Message, "iRadio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Noxon.netStream = null;
            }

            StreamWriter nonParsedElementsWriter, parsedElementsWriter;
            TextWriter stdOut = Console.Out;
            Console.WriteLine("Console.WriteLine()");

            try
            {
                nonParsedElementsWriter = new StreamWriter(new FileStream("./iRadio-non-parsed-elements.txt", FileMode.Create, FileAccess.Write));
                parsedElementsWriter = new StreamWriter(new FileStream("./iRadio-logging.txt", FileMode.Create, FileAccess.Write));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open iRadio .txt files for writing");
                Console.WriteLine(ex.Message);
                return;
            }

            if (Noxon.netStream == null) return;
            await Task.Run(() =>
            {
                IEnumerable<XElement> iRadioNetData =
                from el in NoxonAsync.StreamiRadioNet(Noxon.netStream)
                select el;

                Noxon.Parse(iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut, Program.FormShow);
            });

        }
    }
    public static class NoxonAsync
    {
        public async static Task<int> CommandAsync(this NetworkStream netStream, char commandkey)
        {
            try
            {
                if (netStream.CanWrite && Noxon.Commands.ContainsKey(commandkey))
                {
                    System.Diagnostics.Debug.WriteLine("\t\tTransmit CommandAsync('{0}'): ASC({1} --> 0x{2})", commandkey, Noxon.Commands[commandkey].Key, BitConverter.ToString(Noxon.IntToByteArray(Noxon.Commands[commandkey].Key)));
                    await netStream.WriteAsync(Noxon.IntToByteArray(Noxon.Commands[commandkey].Key), 0, sizeof(int));
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (System.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine("\t\tTransmit CommandAsync() failed ({0})", e.Message);
                Noxon.Close();
                Task<bool> isOPen = Noxon.OpenAsync();
                await isOPen;
                if (netStream != null && netStream.CanWrite) await netStream.WriteAsync(Noxon.IntToByteArray(Noxon.Commands[commandkey].Key), 0, sizeof(int));
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private static XmlReader reader;

        public static IEnumerable<XElement> StreamiRadioNet(ITestableNetworkStream netStream)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false, Async = true };
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None, Encoding.GetEncoding("ISO-8859-1"));  // needed to avoid exception "WDR 3 zum Nachhören"
            CancellationTokenSource cancellation = new CancellationTokenSource();
            System.Timers.Timer timeoutTimer = new System.Timers.Timer(10000);        // check if ReadFrom(reader) times out
            timeoutTimer.Elapsed += (sender, e) => ParseTimeout(sender, e, cancellation);

            using (reader = XmlReader.Create(netStream.GetStream(), settings, context))                                             //                                           ^---
            {
                while (true)
                {
                    while (!reader.EOF)
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            XElement el;
                            try
                            {
                                timeoutTimer.Start();
                                //  https://docs.microsoft.com/de-de/dotnet/core/porting/
                                Task<XNode> t = XNode.ReadFromAsync(reader, cancellation.Token); 
                                el = t.Result as XElement;  // ToDo: if iRadio = "Nicht verfügbar" or "NOXON" ==> ReadFromAsync() is canceled (OK!) but does not resume normal reading
                                timeoutTimer.Stop();
                            }
                            catch
                            {
                                el = new XElement("FormStreamiRadioExceptionXElementAfterReadFromFails");
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
            // cancellation.Dispose();
        }
        private static void ParseTimeout(object sender, ElapsedEventArgs e, CancellationTokenSource cancellation)
        {
            sender.ToString();
            e.ToString();
            cancellation.Cancel();
        }
    }

    public class FormShow : IShow
    {
        public void Browse(XElement e, Lines line0)
        {
        }
        public void Header()
        {
        }
        public void Line(string caption, Lines line, XElement e)
        {
            switch (line)
            {
                case Lines.lineWiFi:
                    Program.form.progressWifi.Invoke((MethodInvoker)delegate {
                        Program.form.progressWifi.Value = int.TryParse(Tools.Normalize(e), out int result) ? result : 0;
                    });
                    break;
                default:
                    break;
            }
            
        }
        public void Msg(XElement e, Lines line0)
        {
        }
        public void PlayingTime(XElement el, Lines line)
        {
        }
        public void Status(XElement e, Lines line)
        {
        }
        public void Log(System.IO.StreamWriter parsedElementsWriter, System.IO.TextWriter stdOut, XElement el)
        {
            Program.form.listBox1.Invoke((MethodInvoker) delegate {
                Program.form.listBox1.Items.Add(el.ToString());   // Running on the UI thread
                Program.form.listBox1.SelectedIndex = Program.form.listBox1.Items.Count - 1;
            });

            if (parsedElementsWriter != null && stdOut != null && el != null)
            {
                Console.SetOut(parsedElementsWriter); // re-direct
                Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("hh: mm:ss.fff"), el.ToString());
                Console.SetOut(stdOut); // stop re-direct
                parsedElementsWriter.Flush();
            }
        }

    }
}