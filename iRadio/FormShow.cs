using iRadio.Properties;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;


namespace iRadio
{
    public class FormShow : IShow
    {
        private static bool browsing = false;
        private static bool searchingPossible = false;
        public static int selectedIndex = -1;

        public static bool Browsing { 
            get => browsing;
            set
            {
                browsing = value;
                Program.form.Invoke((MethodInvoker)delegate
                {
                    Noxon.textEntry = browsing && FormShow.SearchingPossible;
                    Program.form.textBoxSearch.Enabled = Noxon.textEntry;
                    Program.form.textBoxSearch.Visible = Noxon.textEntry;
                    Program.form.pictureBoxFind.Enabled = Noxon.textEntry;
                    Program.form.pictureBoxFind.Visible = Noxon.textEntry;
                    // for (int i = 1; i <= 9; i++) ((Button)Program.form.Controls["button" + i.ToString()]).Enabled = !Noxon.textEntry;  // dis- or enable buttons1..9
                });
            }
        }

        public static bool SearchingPossible { 
            get => searchingPossible; 
            set
            { 
                searchingPossible = value;
                if (Program.form.labelTitle.Text == "NOXON") searchingPossible = false;
                if (Program.form.labelTitle.Text == iRadioConsole.Properties.Resources.NoxonTitleFavorites) searchingPossible = value;  // search active also in Favorites
            }
        }

        public void Browse(XElement e, Lines line0, bool searchingPossible)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "title").FirstOrDefault()) != null)
            {
                Show.lastBrowsedTitle = Tools.Normalize(elem);
                Program.form.progressWifi.Invoke((MethodInvoker)delegate {
                    Program.form.labelTitle.Text = Tools.Normalize(elem);
                });
            }
            FormShow.SearchingPossible = searchingPossible;
            Browsing = true;

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
                    Fav.flag lineflag = Fav.flag.Nothing;
                    if (elem.Attribute("flag") != null)
                    {
                        switch (elem.Attribute("flag").Value)
                        {
                            case "d":     //  d -  folder 📁
                                lineflag = Fav.flag.Folder;
                                break;
                            case "ds":    //                      <text id="line0" flag="ds">History</text>)
                                lineflag = Fav.flag.Folder;
                                selectedIndex = i;
                                Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                                    Program.form.listBoxDisplay.SelectedIndex = i;
                                });
                                break;
                            case "p":     //  p -  song   ♪
                                lineflag = Fav.flag.Song;
                                break;
                            case "ps":    //  ps - song   ♪ ♪     < text id = "line0" flag = "ps" > Radio Efimera </ text >
                                lineflag = Fav.flag.Songs;
                                selectedIndex = i;
                                Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                                    Program.form.listBoxDisplay.SelectedIndex = i;
                                });
                                break;
                        }
                    }
                    Show.lastBrowsedLines[i] = Tools.Normalize(elem);
                    Show.lastBrowsedFlags[i] = lineflag;
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

        public async void Line(string caption, Lines line, XElement e, bool continueBrowsing = false)
        {
            if (!continueBrowsing)
            {
                Browsing = false;
                FormShow.selectedIndex = -1;
            }
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
                    if (caption == "Welcome")
                    {
                        Program.form.Invoke((MethodInvoker)delegate {
                            Program.form.toolStripStatusLabel1.Image = iRadio.Properties.Resources.hand;
                        });
                    }
                    if (caption == "Icon-Play")
                    {
                        Program.form.Invoke((MethodInvoker)delegate {
                            Program.form.toolStripStatusLabel1.Image = iRadio.Properties.Resources.play;
                        });
                    }
                    if (caption == "Icon-Shuffle")
                    {
                        // <update id="status">  < icon id = "shuffle" >  empty  |  shuffle  </ icon >  </ update >
                        bool shuffle = Tools.Normalize(e) == "shuffle";
                        Program.form.Invoke((MethodInvoker)delegate {
                            Program.form.pictureBoxShuffle.Visible = shuffle;
                        });
                    }
                    if (caption == "Icon-Repeat")
                    {
                        // <update id="status"> < icon id = "repeat" > empty  |  repeat </ icon >  </ update >
                        // <update id="status"> < icon id = "repeat" text = "all" > repeat </ icon >  </ update >
                        bool repeat = Tools.Normalize(e) == "repeat";
                        bool all = false;                                   
                        if (e.Element("icon") != null && e.Element("icon").Attribute("text") != null && e.Element("icon").Attribute("text").Value == "all") all = true;
                        if (e.Attribute("text") != null && e.Attribute("text").Value == "all") all = true;  //   <icon id="repeat" text="all">repeat</icon> 
                        Program.form.Invoke((MethodInvoker)delegate {
                            if (all) Program.form.pictureBoxRepeat.Image = iRadio.Properties.Resources.RepeatAll;
                            else Program.form.pictureBoxRepeat.Image = iRadio.Properties.Resources.Repeat;
                            Program.form.pictureBoxRepeat.Visible = repeat;
                        });
                    }
                    else if (caption == "CloseStreamAndReturn")  // stream closed due to "Nicht verfÃ¼gbar"
                    {
                        await Noxon.netStream?.GetNetworkStream().CommandAsync('L');
                    }
                    else if (caption == "CloseStream")  
                    {
                        Program.form.Invoke((MethodInvoker)delegate {
                            Program.form.toolStripStatusLabel1.Image = iRadio.Properties.Resources.hand;
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
                case Lines.Status:
                    if (caption == "Date")
                    {
                        // date is always 0
                    }
                    else if (caption == "Volume")
                    {
                        Program.form.Invoke((MethodInvoker)delegate {
                            Settings.Default.Volume = int.TryParse(Tools.Normalize(e), out int result) ? result : 0;
                            Program.form.trackBarVolume.Enabled = true;
                            Program.form.trackBarVolume.Value = Settings.Default.Volume;
                        });

                    }
                    else
                    {
                        Program.form.Invoke((MethodInvoker)delegate {
                            Program.form.toolStripStatusLabel1.Image = iRadio.Properties.Resources.ListImg;
                            Program.form.toolStripStatusLabel1.Text = caption;
                        });
                    }
                    break;
                case Lines.Busy:
                    Cursor c = Cursors.Default;
                    int busy = int.TryParse(Tools.Normalize(e), out int result) ? result : 0;
                    if (busy == 1) c = Cursors.WaitCursor;
                    Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate {
                        Cursor.Current = c;
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
            Browsing = false;
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
                if (e.Value == "empty") statusImage = iRadio.Properties.Resources.Antenna;
            }
            if ((e.Name == "view" || e.Name == "update") && e.Attribute("id").Value == "welcome")
            {
                statusImage = iRadio.Properties.Resources.hand;
            }
            Program.form.Invoke((MethodInvoker)delegate {
                Program.form.toolStripStatusLabel1.Text = "\t" + Tools.Normalize(e);
                Program.form.toolStripStatusLabel1.Image = statusImage;
            });
        }
        public void Log(System.IO.StreamWriter parsedElementsWriter, System.IO.TextWriter stdOut, XElement el)
        {
            try
            {
                if (Program.formLogging != null && !Program.form.Disposing)
                {
                    Program.form.Invoke((MethodInvoker)delegate
                    {
                        if (Program.formLogging.listBox1.Items.Count < 100)  // Running on the UI thread
                    {
                            Program.formLogging.listBox1.Items.Add(el.ToString());
                        }
                        else
                        {
                            for (int i = 0; i < Program.formLogging.listBox1.Items.Count - 1; i++) Program.formLogging.listBox1.Items[i] = Program.formLogging.listBox1.Items[i + 1];
                            Program.formLogging.listBox1.Items[^1] = el.ToString();
                        }
                        Program.formLogging.listBox1.SelectedIndex = Program.formLogging.listBox1.Items.Count - 1;
                    });
                }
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("Program.form.Invoke failed ({0})", e.Message);
            }

            if (parsedElementsWriter != null && stdOut != null && el != null)
            {
                Console.SetOut(parsedElementsWriter); // re-direct
                if (Properties.Settings.Default.LogTimestamps) Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("hh: mm:ss.fff"), el.ToString());
                else Console.WriteLine("{0}", el.ToString());
                Console.SetOut(stdOut); // stop re-direct
                parsedElementsWriter.Flush();
            }
        }
    }
}