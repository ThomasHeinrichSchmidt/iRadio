using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace iRadio
{

    public class Show : IShow
    {

        public static string currentTitle = "";
        public static string currentLine0 = "";
        public static int columnBrowse = 0;
        public static int columnHeader = 10;

        public static string lastBrowsedTitle = "";
        public static string[] lastBrowsedLines = new string[Noxon.ListLines];
        public void Header()
        {
            Console.Clear();
            Console.Title = "NOXON iRadio";
            Console.CursorVisible = false;
            Console.CursorTop = 0;
            Console.CursorLeft = Show.columnHeader;
            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("NOXON iRadio");
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }
        public void Line(string caption, Lines line, XElement e, bool continueBrowsing = false)
        {
            Console.CursorTop = (int)line;
            Console.CursorLeft = (int)Lines.columnShow;
            ClearLine((int)Lines.columnShow, (int)line);

            ConsoleColor bg = Console.BackgroundColor;
            ConsoleColor fg = Console.ForegroundColor;
            if (caption == "Title")
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                currentTitle = Tools.Normalize(e);
            }
            if ((int)line == (int)Lines.line0) currentLine0 = Tools.Normalize(e);

            Console.WriteLine("{0} '{1}'", caption, Tools.Normalize(e));
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;

            Console.CursorTop = (int)Lines.Separator;
            Console.CursorLeft = (int)Lines.columnShow;
            Console.WriteLine("{0}", new String('-', Console.WindowWidth - Console.CursorLeft - 1));
        }
        public void PlayingTime(XElement el, Lines line)
        {
            Console.CursorTop = (int)line;
            Console.CursorLeft = (int)Lines.columnShow;
            int s = int.Parse(el.Value.Trim('\r', '\n', ' '));
            ClearLine((int)Lines.columnShow, (int)line);
            int h = s / (60 * 60);
            int m = s / 60 - h * 60;
            string hms = s < 60 * 60 ? String.Format("{0:00}:{1:00}", s / 60, s % 60) : String.Format("{0:00}:{1:00}:{2:00}", h, m, s % 60);
            Console.WriteLine("                     Playing for {0}", hms);
        }
        public void Status(XElement e, Lines line)
        {
            Console.CursorTop = (int)line;
            Console.CursorLeft = (int)Lines.columnShow;
            Console.WriteLine("Status Icon '{0}'", Tools.Normalize(e));
            if (e.Value.Contains("empty"))
            {
                for (int i = 1; i < (int)line; i++)
                {
                    // ClearLine(columnShow, i);   //   <icon id="play">empty</icon>    < icon id = "shuffle" > empty </ icon >    < icon id = "repeat" > empty </ icon >
                }
            }
        }
        public void Msg(XElement e, Lines line0)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            for (int i = 0; i < Noxon.ListLines; i++)
            {
                if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "line" + i).FirstOrDefault()) != null)
                {
                    Console.CursorTop = (int) line0 + i;
                    Console.CursorLeft = Show.columnBrowse;
                    if (elem.Value == "")
                    {
                        ClearLine(Show.columnBrowse, (int)line0 + i);
                    }
                    else
                    {
                        Console.WriteLine(Tools.Normalize(elem));
                    }
                }
            }
        }
        public void Browse(XElement e, Lines line0, bool searchingPossible)
        {
            XElement elem;   // loop <text id="line0"> ...  <text id="line3">
            if ((elem = e.DescendantsAndSelf("text").Where(r => r.Attribute("id").Value == "title").FirstOrDefault()) != null)
            {
                Line("Title", Lines.Title, elem);
                lastBrowsedTitle = Tools.Normalize(elem);
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
                    Console.CursorTop = (int)line0 + i;
                    Console.CursorLeft = Show.columnBrowse;
                    ConsoleColor bg = Console.BackgroundColor;
                    ConsoleColor fg = Console.ForegroundColor;
                    ClearLine(Show.columnBrowse, (int) line0 + i);           // if (elem.Value == "")

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
                    Console.WriteLine(Tools.Normalize(elem));  // else
                    Console.BackgroundColor = bg;
                    Console.ForegroundColor = fg;
                    lastBrowsedLines[i] = Tools.Normalize(elem);
                }
            }
            if (clearnotusedlines)
            {
                for (int i = 0; i < Noxon.ListLines; i++)
                {
                    if (!printline[i])
                    {
                        ClearLine(Show.columnBrowse, (int)line0 + i);
                        lastBrowsedLines[i] = "";
                    }
                }
            }
        }
        public void Log(System.IO.StreamWriter parsedElementsWriter, System.IO.TextWriter stdOut, XElement el)
        {
            if (parsedElementsWriter != null && stdOut != null && el != null)
            {
                Console.SetOut(parsedElementsWriter); // re-direct
                Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("hh: mm:ss.fff"), el.ToString());
                Console.SetOut(stdOut); // stop re-direct
                parsedElementsWriter.Flush();
            }
        }

        private void ClearLine(int column, int line)
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
