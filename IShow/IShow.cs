using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace iRadio
{
    public enum Lines
    {
        Title = 1,
        Artist = 2,
        line0 = 2,
        Album = 3,
        Track = 4,
        PlayingTime = 5,
        Separator = 6,
        Icon = 7,
        WiFi = 8,
        Buffer = 9,
        Status = 10,
        Busy = 11,
        Waiting = 12,
        columnShow = 0
}
public interface IShow
    {
        void Browse(XElement e, Lines line0);
        void Header();
        void Line(string caption, Lines line, XElement e);
        void Msg(XElement e, Lines line0);
        void PlayingTime(XElement el, Lines line);
        void Status(XElement e, Lines line);
        void Log(System.IO.StreamWriter parsedElementsWriter = null, System.IO.TextWriter stdOut = null, XElement el = null);
    }

    public class Tools
    {
        public static string Normalize(XElement e)
        {
            // string original = e.Value.Trim('\r', '\n').Trim();
            string original = e.Value.Replace('\r', ' ').Replace('\n', ' ').Trim();  // does only leave values, no xml tags
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            byte[] encoded = Encoding.GetEncoding(1252).GetBytes(original);
            string corrected = Encoding.UTF8.GetString(encoded);
            string normalized;
            char badc = '\xfffd';
            if (corrected.Contains(badc))
            {
                normalized = original;
            }
            else
            {
                normalized = corrected;
            }
            return normalized;
        }
    }
}