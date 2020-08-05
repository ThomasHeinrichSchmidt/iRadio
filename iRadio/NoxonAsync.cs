using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;


namespace iRadio
{
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
            {
                for (int i = 0; i < m.Times; i++)
                {
                    await netStream.CommandAsync(Convert.ToChar(48 + m.Digit));
                    Thread.Sleep(Noxon.MultiPressDelayForSameKey);
                }
                Thread.Sleep(Noxon.MultiPressDelayForNextKey);
            }
            return 0;
        }


        private static XmlReader reader;

        public static IEnumerable<XElement> StreamiRadioNet(ITestableNetworkStream netStream)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false, Async = true };
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None, Encoding.GetEncoding("ISO-8859-1"));  // needed to avoid exception "WDR 3 zum Nachhören"
            CancellationTokenSource cancellation = new CancellationTokenSource();
            System.Timers.Timer timeoutTimer = new System.Timers.Timer(5000);        // check if ReadFrom(reader) times out
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
                                if (!FormShow.Browsing) timeoutTimer.Start();
                                //  https://docs.microsoft.com/de-de/dotnet/core/porting/
                                Task<XNode> t = XNode.ReadFromAsync(reader, cancellation.Token); 
                                el = t.Result as XElement;  // ToDo: if iRadio = "Nicht verfügbar" or "NOXON" ==> ReadFromAsync() is canceled (OK!) but does not resume normal reading
                                tstat = t.Status;           // also: no more data received if <browse> menu
                                tex = t.Exception;
                                if (!FormShow.Browsing) timeoutTimer.Stop();
                            }
                            catch (Exception ex)
                            {
                                el = new XElement("CloseStream", "FormStreamiRadioExceptionXElementAfterReadFromFails"+ ex.Message + "=" + tex?.Message);
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
}