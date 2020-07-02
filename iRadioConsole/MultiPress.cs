using System;
using System.Linq;

namespace iRadio
{
    public class MultiPress
    {
        public static MultiPressCommand[] CreateMultiPressCommands(string s)
        {
            // 1 - 1.,?!-&@*#_~             max. 10 chars allowed in result
            // 2 - abcä2 
            // 3 - def3
            // 4 - ghi4
            // 5 - jkl5
            // 6 - mnoö6 
            // 7 - pqrs7
            // 8 - tuvü8 
            // 9 - wxyz9 
            // 0 - "0 " (0 space)
            string[] MultiPressChars = new string[] { "0 ", "1.,?!-&@*#_~", "abcä2", "def3", "ghi4", "jkl5", "mnoö6", "pqrs7", "tuvü8", "wxyz9" };
            MultiPressCommand[] mpc = new MultiPressCommand[10];
            int n = 0;
            // truncate string to max 10 
            // for each char in string
            //      find index in MultiPressChars with MultiPressChars[index].Contains(char)
            //      MultiPressCommand[i].Digit = index
            //      MultiPressCommand[i++].Times = Position of char in MultiPressChars[index]
            // return MultiPressCommand[]

            // use result: 
            // foreach (mpc in MultiPressCommand[]) 
            //      for (i=0; i<mpc.Times; i++) { 
            //          netStream.Write(intToByteArray(Noxon.NoxonCommands[mpc.Digit].Key), 0, sizeof(int)); 
            //          Thread.Sleep(same); 
            //      }
            // Thread.Sleep(next); 

            if (s.Length > 0) s = s.Substring(0, Math.Min(s.Length, 10));
            s = s.ToLower();
            foreach (char c in s)
            {
                int i = Array.FindIndex(MultiPressChars, m => m.Contains(c));
                if (i < 0) continue;
                mpc[n] = new MultiPressCommand { Digit = i, Times = MultiPressChars[i].IndexOf(c) + 1 };
                n++;
                if (n >= 10) break;
            }
            Array.Resize<MultiPressCommand>(ref mpc, n);
            return mpc;
        }
    }


    public class MultiPressCommand
    {
        public int Digit { get; set; }
        public int Times { get; set; }

        public override string ToString()
        {
            return String.Format("[{0}] x {1} ", Digit, Times);
        }
    }


}
