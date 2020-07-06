using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace iRadio
{
    public enum Lines
    {
        lineTitle = 1,
        lineArtist = 2,
        line0 = 2,
        lineAlbum = 3,
        lineTrack = 4,
        linePlayingTime = 5,
        lineSeparator = 6,
        lineIcon = 7,
        lineWiFi = 8,
        lineBuffer = 9,
        lineStatus = 10,
        lineBusy = 11,
        lineWaiting = 12,
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
            string original = e.Value.Replace('\r', ' ').Replace('\n', ' ').Trim();
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