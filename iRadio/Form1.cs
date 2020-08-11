using iRadio.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace iRadio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
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
            Noxon.IP = IPAddress.Parse(Settings.Default.NoxonIP);  // use previous IP first
            Task<bool> isOpen = Task.Run(() => NoxonAsync.OpenAsync()); // https://stackoverflow.com/questions/14962969/how-can-i-use-async-to-increase-winforms-performance
            try
            {
                await isOpen;
                button1.Enabled = isOpen.Result;
                Program.formLogging.Text = "NOXON iRadio - " + Noxon.IP.ToString() + ":10100";
            }
            catch (SocketException exs)
            {
                MessageBox.Show(exs.Message, "iRadio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Noxon.netStream = null;
            }

            Program.form.trackBarVolume.Value = Settings.Default.Volume;
            foreach (string entry in Properties.Settings.Default.FavoritesList)
            {
                Program.form.listBoxFavs.Items.Add(entry);
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

            int round = 0;
            do
            {
                await Task.Run(() =>
                {
                    IEnumerable<XElement> iRadioNetData =
                    from el in NoxonAsync.StreamiRadioNet(Noxon.netStream)
                    select el;

                    Noxon.Parse(iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut, Program.FormShow);
                });
                System.Diagnostics.Debug.WriteLine("Parse canceled, due to 'Nicht verfÃ¼gbar' - restarting Parse() now, round {0}", round++);

                // Noxon.Close();
                // Noxon.Open();
                // await NoxonAsync.OpenAsync();
                await Task.Run(() => NoxonAsync.OpenAsync());
            } while (true);

        }
        private async void Form_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("KeyDown: {0}", e.KeyCode);

            bool isLetterOrDigit = char.IsLetterOrDigit((char)e.KeyCode);
            if (Noxon.netStream != null && isLetterOrDigit && Noxon.textEntry && textBox1.Visible)
            {
                KeysConverter kc = new KeysConverter();
                textBox1.Text += kc.ConvertToString(e.KeyCode)[0];
                return;
            }
            if (Noxon.netStream == null 
                || (Noxon.textEntry && textBox1.Focused && (isLetterOrDigit || e.KeyCode == Keys.Enter))
                || listBoxFavs.Focused)  
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
            if (e.KeyCode == Keys.Enter) command = 'R';
            if (e.KeyCode == Keys.Up) command = 'U';
            if (e.KeyCode == Keys.Down) command = 'D';
            if (e.KeyCode == Keys.VolumeUp) command = '+';
            if (e.KeyCode == Keys.VolumeDown) command = '-';
            if (e.KeyCode == Keys.BrowserFavorites) command = 'F';
            if (e.KeyCode == Keys.Home) command = 'H';
            if (e.KeyCode == Keys.F1) {
                command = ' ';
                //Task<bool> get = Task.Run(() => Favorites.Get()); 
                //await get;
                //Properties.Settings.Default.FavoritesList = Favorites.List();
                //return;
            }
            if (command == ' ' && isLetterOrDigit)
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
            if (Program.formLogging == null) Program.formLogging = new FormLogging();
            Program.formLogging.Show();
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.NoxonIP = Noxon.IP.ToString();
            Properties.Settings.Default.Volume = Program.form.trackBarVolume.Value;
            Properties.Settings.Default.Save();
        }

        private async void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // await Task.Run(() => Noxon.netStream.GetNetworkStream().StringAsync(textBox1.Text));
                await Noxon.netStream.GetNetworkStream().StringAsync(textBox1.Text);
                await Noxon.netStream.GetNetworkStream().CommandAsync('R');
                while (Noxon.Busy) Thread.Sleep(100);
                await Noxon.netStream.GetNetworkStream().CommandAsync('D');  // refresh display
                await Noxon.netStream.GetNetworkStream().CommandAsync('U');
                textBox1.Text = null;
                listBoxDisplay.Focus();
                Noxon.textEntry = false;
            }
        }

        private async void PictureBoxRefresh_Click(object sender, EventArgs e)
        {
            Task<bool> get = Task.Run(() => Favorites.Get());
            await get;
            if (Favorites.List() != null)
            {
                Properties.Settings.Default.FavoritesList = Favorites.List();
                Program.form.listBoxFavs.Items.Clear();
                foreach (string entry in Properties.Settings.Default.FavoritesList)
                {
                    Program.form.listBoxFavs.Items.Add(entry);
                }
            }
            return;
        }

        private void ListBoxDisplay_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ListBoxDisplay_DoubleClick: request cancellation.Cancel()");
            // await Task.Run(() => NoxonAsync.cancellation.Cancel());
        }

        private async void ListBoxFavs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBoxFavs.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                await SelectFavorite(listBoxFavs.SelectedIndex);
                return;
            }
        }

        private async void ListBoxFavs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await SelectFavorite(listBoxFavs.SelectedIndex);
                return;
            }
        }

        private async Task SelectFavorite(int selectedIndex)
        {
            List<string> down = new List<string> { "F" };   // goto favorites
            for (int i = 0; i < selectedIndex; i++) down.Add("D");  // go down as often as necessary
            down.Add("R");  // choose station
            Macro m = new iRadio.Macro("listBoxFavs_KeyDown/MouseDoubleClick", down.ToArray());
            await Task.Run(() => m.Execute());
        }
    }
}