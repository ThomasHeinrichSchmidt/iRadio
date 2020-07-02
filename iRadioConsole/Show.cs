using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace iRadio
{
    public class Show
    {
        public const int lineTitle = 1;
        public const int lineArtist = 2;
        public const int line0 = 2;
        public const int lineAlbum = 3;
        public const int lineTrack = 4;
        public const int linePlayingTime = 5;
        public const int lineSeparator = 6;
        public const int lineIcon = 7;
        public const int lineWiFi = 8;
        public const int lineBuffer = 9;
        public const int lineStatus = 10;
        public const int lineBusy = 11;
        public const int lineWaiting = 12;
        public static int columnBrowse = 0;
        public static int columnHeader = 10;
        public const int columnShow = 0;

        public static string currentTitle = "";
        public static string currentLine0 = "";

        public static string lastBrowsedTitle = "";
        public static string[] lastBrowsedLines = new string[Noxon.ListLines];
        public static void Header()
        {
            Console.Clear();
            Console.Title = "NOXON iRadio";
            Console.CursorVisible = false;
            Console.CursorTop = 0;
            Console.CursorLeft = columnHeader;
            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("NOXON iRadio");
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }


        public static void Line(string caption, int line, XElement e)
        {
            Console.CursorTop = line;
            Console.CursorLeft = columnShow;
            ClearLine(columnShow, line);

            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            if (caption == "Title")
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                currentTitle = Normalize(e);
            }
            if (line == line0) currentLine0 = Normalize(e);

            Console.WriteLine("{0} '{1}'", caption, Normalize(e));
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;

            Console.CursorTop = lineSeparator;
            Console.CursorLeft = columnShow;
            Console.WriteLine("{0}", new String('-', Console.WindowWidth - Console.CursorLeft - 1));
        }

        private static string Normalize(XElement e)
        {
            // string original = e.Value.Trim('\r', '\n').Trim();
            string original = e.Value.Replace('\r', ' ').Replace('\n', ' ').Trim();
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

        public static void PlayingTime(XElement el, int line)
        {
            Console.CursorTop = line;
            Console.CursorLeft = columnShow;
            int s = int.Parse(el.Value.Trim('\r', '\n', ' '));
            ClearLine(columnShow, line);
            Console.WriteLine("                     Playing for {0:00}:{1:00}", s / 60, s % 60);
        }

        public static void Status(XElement e, int line)
        {
            Console.CursorTop = line;
            Console.CursorLeft = columnShow;
            Console.WriteLine("Status Icon '{0}'", Normalize(e));
            if (e.Value.Contains("empty"))
            {
                for (int i = 1; i < line; i++)
                {
                    // ClearLine(columnShow, i);   //   <icon id="play">empty</icon>    < icon id = "shuffle" > empty </ icon >    < icon id = "repeat" > empty </ icon >
                }
            }
        }

        public static void Msg(XElement e, int line0)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            for (int i = 0; i < Noxon.ListLines; i++)
            {
                if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "line" + i).FirstOrDefault()) != null)
                {
                    Console.CursorTop = line0 + i;
                    Console.CursorLeft = columnBrowse;
                    if (elem.Value == "")
                    {
                        ClearLine(columnBrowse, line0 + i);
                    }
                    else
                    {
                        Console.WriteLine(Normalize(elem));
                    }
                }
            }
        }

        public static void Browse(XElement e, int line0)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "title").FirstOrDefault()) != null)
            {
                Line("Title", lineTitle, elem);
                lastBrowsedTitle = Normalize(elem);
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
                    Console.CursorTop = line0 + i;
                    Console.CursorLeft = columnBrowse;
                    ConsoleColor bg = Console.BackgroundColor;
                    ConsoleColor fg = Console.ForegroundColor;
                    ClearLine(columnBrowse, line0 + i);           // if (elem.Value == "")

                    // flags:
                    //  d -  folder 📁
                    //  ds - folder 📁
                    //  p -  song   ♪
                    //  ps - song   ♪ ♪
                    if (elem.Attribute("flag") != null && elem.Attribute("flag").Value == "ds")   //  <text id="line0" flag="ds">History</text>
                    {
                        Console.BackgroundColor = fg;
                        Console.ForegroundColor = bg;
                    }
                    if (elem.Attribute("flag") != null && elem.Attribute("flag").Value == "ps")   //    <text id="line0" flag="ps">Radio Efimera</text>
                    {
                        Console.BackgroundColor = fg;
                        Console.ForegroundColor = bg;
                    }
                    Console.WriteLine(Normalize(elem));  // else
                    Console.BackgroundColor = bg;
                    Console.ForegroundColor = fg;
                    lastBrowsedLines[i] = Normalize(elem);
                }
            }
            if (clearnotusedlines)
            {
                for (int i = 0; i < Noxon.ListLines; i++)
                {
                    if (!printline[i])
                    {
                        ClearLine(columnBrowse, line0 + i);
                        lastBrowsedLines[i] = "";
                    }
                }
            }
        }

        private static void ClearLine(int column, int line)
        {
            int top = Console.CursorTop;
            int left = Console.CursorLeft;
            Console.CursorTop = line;
            Console.CursorLeft = column;
            Console.WriteLine(new String(' ', Console.WindowWidth - Console.CursorLeft));
            Console.CursorTop = top;
            Console.CursorLeft = left;
        }

    }
}
