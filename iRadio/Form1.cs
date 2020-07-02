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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iRadio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static async Task<string> SendRequest(string server, int port, string method, string data)
        {
            try
            {
                // set up IP address of server
                IPAddress ipAddress = null;
                IPHostEntry ipHostInfo = Dns.GetHostEntry(server);
                for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                {
                    if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ipHostInfo.AddressList[i];
                        break;
                    }
                }
                if (ipAddress == null)
                    throw new Exception("Unable to find an IPv4 address for server");

                TcpClient client = new TcpClient();
                await client.ConnectAsync(ipAddress, port); // connect to the server

                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                StreamReader reader = new StreamReader(networkStream);

                writer.AutoFlush = true;
                string requestData = "method=" + method + "&" + "data=" + data + "&eor"; // 'end-of-requet'
                await writer.WriteLineAsync(requestData);
                string response = await reader.ReadLineAsync();

                client.Close();

                return response;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        } // SendRequest

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
            await isOPen;
            button1.Enabled = isOPen.Result;
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
    }
}
