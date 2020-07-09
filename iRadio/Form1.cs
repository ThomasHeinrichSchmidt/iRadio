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
                toolTip1.Show(Noxon.currentArtist, button1);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Task<bool> isOPen = Task.Run(() => Noxon.OpenAsync());    // https://stackoverflow.com/questions/14962969/how-can-i-use-async-to-increase-winforms-performance
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
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);

            await Task.Run(() =>
            {
                IEnumerable<XElement> iRadioNetData =
                from el in NoxonAsync.StreamiRadioNet(Noxon.netStream)
                select el;

                Noxon.Parse(iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut, Program.FormShow);
            });

        }
        private async void Form_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("KeyDown: {0}", e.KeyCode);
            if (Noxon.netStream == null || Noxon.textEntry)  
            {   // if nothing is received or text entry instead of local hotkeys 
                return;
            }
            char command = ' ';

            if (e.Control && e.KeyCode == Keys.S)       // Ctrl-S Save
            {
                // Do what you want here
            }
            if (e.KeyCode == Keys.D1) command = '1';
            if (e.KeyCode == Keys.D2) command = '2';
            if (e.KeyCode == Keys.D3) command = '3';
            if (e.KeyCode == Keys.D4) command = '4';
            if (e.KeyCode == Keys.D5) command = '5';  // h --> KeyDown: H  // H -->   KeyDown: ShiftKey, KeyDown: H
            if (e.KeyCode == Keys.Left) command = 'L';
            if (e.KeyCode == Keys.Right) command = 'R';
            if (e.KeyCode == Keys.Up) command = 'U';
            if (e.KeyCode == Keys.Down) command = 'D';
            if (e.KeyCode == Keys.VolumeUp) command = '+';
            if (e.KeyCode == Keys.VolumeDown) command = '-';
            if (e.KeyCode == Keys.BrowserFavorites) command = 'F';
            if (e.KeyCode == Keys.Home) command = 'H';
            if (e.KeyCode == Keys.F1) Favorites.Get();
            if (command == ' ')
            {
                KeysConverter kc = new KeysConverter();
                char c = kc.ConvertToString(e.KeyCode)[0];  // first char in keyboard string, e.g. (F)avorites, (H)ome, (M)enu, ... 
                if (Noxon.Commands.ContainsKey(c)) command = c;
            }
            if (command != ' ')
            {
                Task<int> ret = Noxon.netStream.GetNetworkStream().CommandAsync(command);
                await ret;
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
            }
        }

        private void StatusStrip1_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("Status", "iRadio messages");
        }

        private void ListBoxDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDisplay.SelectedIndex != FormShow.selectedIndex)
            {
                listBoxDisplay.SelectedIndexChanged -= new EventHandler(ListBoxDisplay_SelectedIndexChanged);
                listBoxDisplay.SelectedIndex = FormShow.selectedIndex;
                listBoxDisplay.SelectedIndexChanged += new EventHandler(ListBoxDisplay_SelectedIndexChanged);
            }
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
        public async static Task<int> StringAsync(this NetworkStream netStream, string str)
        {
            MultiPressCommand[] mpc = MultiPress.CreateMultiPressCommands(str);
            foreach (MultiPressCommand m in mpc)
                for (int i = 0; i < m.Times; i++)
                {
                    await netStream.CommandAsync(Convert.ToChar(48 + m.Digit));
                    Thread.Sleep(Noxon.MultiPressDelayForSameKey);
                }
            Thread.Sleep(Noxon.MultiPressDelayForNextKey);
            return 0;
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
                            TaskStatus tstat = TaskStatus.Created;
                            AggregateException tex = null;

                            try
                            {
                                if (!FormShow.browsing) timeoutTimer.Start();
                                //  https://docs.microsoft.com/de-de/dotnet/core/porting/
                                Task<XNode> t = XNode.ReadFromAsync(reader, cancellation.Token); 
                                el = t.Result as XElement;  // ToDo: if iRadio = "Nicht verfügbar" or "NOXON" ==> ReadFromAsync() is canceled (OK!) but does not resume normal reading
                                tstat = t.Status;           // also: no more data received if <browse> menu
                                tex = t.Exception;
                                if (!FormShow.browsing) timeoutTimer.Stop();
                            }
                            catch (Exception ex)
                            {
                                el = new XElement("FormStreamiRadioExceptionXElementAfterReadFromFails", ex.Message + "=" + tex?.Message); 
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
        public static bool browsing = false;
        public static int selectedIndex = -1;
        public void Browse(XElement e, Lines line0)
        {
            browsing = true;
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "title").FirstOrDefault()) != null)
            {
                Show.lastBrowsedTitle = Tools.Normalize(elem);
                Program.form.progressWifi.Invoke((MethodInvoker)delegate {
                    Program.form.labelTitle.Text = Tools.Normalize(elem);
                });
            }

            bool clearnotusedlines = false;
            if ((e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "scrid").FirstOrDefault()) != null)
            {
                // <view id="browse">
                //  < view id = "browse" >
                //  < text id = "scrid" > 102.2 </ text >
                //  < text id = "cbid" > 3 </ text >
                // ...
                clearnotusedlines = true;   // only clear not used lines if this is a complete "screen", not a later "update" to it
            }
            bool[] printline = new bool[Noxon.ListLines];
            for (int i = 0; i < Noxon.ListLines; i++)
            {
                if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "line" + i).FirstOrDefault()) != null)
                {
                    printline[i] = true;
                    // Console.CursorTop = (int)line0 + i;
                    Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                        Program.form.listBoxDisplay.Items[i] = "";  // ClearLine()
                    });
                    // flags:
                    //  d -  folder 📁
                    //  ds - folder 📁
                    //  p -  song   ♪
                    //  ps - song   ♪ ♪
                    if (elem.Attribute("flag") != null && elem.Attribute("flag").Value == "ds")   //  <text id="line0" flag="ds">History</text>
                    {
                        selectedIndex = i;
                        Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                            Program.form.listBoxDisplay.SelectedIndex = i;
                        });
                    }
                    if (elem.Attribute("flag") != null && elem.Attribute("flag").Value == "ps")   //    <text id="line0" flag="ps">Radio Efimera</text>
                    {
                        selectedIndex = i;
                        Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                            Program.form.listBoxDisplay.SelectedIndex = i;
                        });
                    }
                    Show.lastBrowsedLines[i] = Tools.Normalize(elem);
                    Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                        Program.form.listBoxDisplay.Items[i] = Tools.Normalize(elem);
                    });
                }
            }
            if (clearnotusedlines)
            {
                for (int i = 0; i < Noxon.ListLines; i++)
                {
                    if (!printline[i])
                    {
                        Show.lastBrowsedLines[i] = "";
                        Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                            Program.form.listBoxDisplay.Items[i] = "";  // ClearLine()
                        });
                    }
                }
            }
        }
        public void Header()
        {
        }
        public void Line(string caption, Lines line, XElement e)
        {
            browsing = false;
            FormShow.selectedIndex = -1;
            Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                Program.form.listBoxDisplay.SelectedIndex = -1;
            });
            switch (line)
            {
                case Lines.Title:
                    Program.form.progressWifi.Invoke((MethodInvoker)delegate {
                        Program.form.labelTitle.Text = Tools.Normalize(e);
                    });
                    break;
                case Lines.WiFi:
                    Program.form.progressWifi.Invoke((MethodInvoker)delegate {
                        Program.form.progressWifi.Value = int.TryParse(Tools.Normalize(e), out int result) ? result : 0;
                    });
                    break;
                case Lines.Buffer:
                    Program.form.progressWifi.Invoke((MethodInvoker)delegate {
                        Program.form.progressBuffer.Value = int.TryParse(Tools.Normalize(e), out int result) ? result : 0;
                    });
                    break;
                case Lines.Icon:
                    if (caption == "Icon-Play")
                    {
                        Program.form.listBox1.Invoke((MethodInvoker)delegate {
                            Program.form.toolStripStatusLabel1.Image = iRadio.Properties.Resources.play;
                        });
                    }
                    break;
                case Lines.Artist:
                    Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                        Program.form.listBoxDisplay.Items[0] = Tools.Normalize(e);
                        Program.form.listBoxDisplay.Items[3] = "";  // last line not used 
                    });
                    break;
                case Lines.Album:
                    Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                        Program.form.listBoxDisplay.Items[1] = Tools.Normalize(e);
                        Program.form.listBoxDisplay.Items[3] = "";  // last line not used 
                    });
                    break;
                case Lines.Track:
                    Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                        Program.form.listBoxDisplay.Items[2] = Tools.Normalize(e);
                        Program.form.listBoxDisplay.Items[3] = "";  // last line not used 
                    });
                    break;

                default:
                    //                    Show.Line("Welcome", Lines.Icon, el);   //   <icon id="welcome" text="wlan@ths / wlan@t-h-schmidt.de">welcome</icon>
                    //                    Show.Line("Title", Lines.Title, e);        // 1
                    //                    Show.Line("Artist", Lines.Artist, el);     // 2 = line0
                    //                    Show.Line("Album", Lines.Album, el);       // 3
                    //                    Show.Line("Track", Lines.Track, el);       // 4
                    //                    Show.Line("Date", Lines.Status, el);
                    //                    Show.Line(caption, Lines.Status, el);
                    //                    Show.Line("Icon-Shuffle", Lines.Icon, el);
                    //                    Show.Line("Icon-Repeat", Lines.Icon, el);
                    //                    Show.Line("Busy=", Lines.Busy, el);
                    break;
            }
            
        }
        public void Msg(XElement e, Lines line0)
        {
        }
        public void PlayingTime(XElement el, Lines line)
        {
            browsing = false;
            if (line == Lines.PlayingTime)
            {
                int s = int.TryParse(Tools.Normalize(el), out int result) ? result : 0;
                int h = s / (60 * 60);
                int m = s / 60 - h * 60;
                string hms = s < 60 * 60 ? String.Format("{0:00}:{1:00}", s / 60, s % 60) : String.Format("{0:00}:{1:00}:{2:00}", h, m, s % 60);
                Program.form.labelPlaying.Invoke((MethodInvoker)delegate {
                    Program.form.labelPlaying.Text = hms;
                });
            }
        }
        public void Status(XElement e, Lines line)
        {
            Image statusImage = iRadio.Properties.Resources.hourglass;
            if (e.Name == "icon" && e.Attribute("id").Value == "play")
            {
                if (e.Value == "play") statusImage = iRadio.Properties.Resources.play;
                if (e.Value == "empty") statusImage = iRadio.Properties.Resources.iRadio;
            }
            if ((e.Name == "view" || e.Name == "update") && e.Attribute("id").Value == "welcome")
            {
                statusImage = iRadio.Properties.Resources.hand;
            }
            Program.form.listBox1.Invoke((MethodInvoker)delegate {
                Program.form.toolStripStatusLabel1.Text = Tools.Normalize(e);
                Program.form.toolStripStatusLabel1.Image = statusImage;
            });
        }
        public void Log(System.IO.StreamWriter parsedElementsWriter, System.IO.TextWriter stdOut, XElement el)
        {
            Program.form.listBox1.Invoke((MethodInvoker)delegate {
                if (Program.form.listBox1.Items.Count < 10)  // Running on the UI thread
                {
                    Program.form.listBox1.Items.Add(el.ToString());
                }
                else
                {
                    for (int i = 0; i < Program.form.listBox1.Items.Count - 1; i++) Program.form.listBox1.Items[i] = Program.form.listBox1.Items[i + 1];
                    Program.form.listBox1.Items[^1] = el.ToString();
                }
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