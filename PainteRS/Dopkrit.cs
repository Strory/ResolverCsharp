using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PainteRS // режим для разработчиков
{
    internal class Dopkrit
    {
        List<(string lett, double min, double max, double proc)> simb = 
            new List<(string lett, double min, double max, double proc)>(); // Здесь хранятся: минимальное (расстояие от центра масс до фигуры), максимальное расстояние, и среднее арифметическое между всеми расстояниями
        public void SeeFiles()
        {
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("D:\\source\\repos\\PainteRS (1)\\PainteRS (1)\\PainteRS (1)\\PainteRS\\PainteRS\\test.txt"); 
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                switch (line)
                {
                    case "form_1":
                        break;
                    default: throw new Exception();
                }
                line = sr.ReadLine();
                while (line != null)
                {
                    if (line != "")
                    {
                        var tmp = line.Split(' ');
                        simb.Add((
                            (tmp[0])[0].ToString(),
                            Convert.ToDouble(tmp[1]),
                            Convert.ToDouble(tmp[2]),
                            Convert.ToDouble(tmp[3]) 
                            ) );
                    }
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

        }

        public string Whats_Letter(double proch)
        {
            if (simb.Count == 0)
                return "err";
            string LettOut = "";
            double deltaMin = 1;
            for (int i = 0; i < simb.Count; i++)
            {
                if (proch > simb[i].min && proch < simb[i].max)
                {
                    if (Math.Abs(simb[i].proc - proch) < deltaMin)
                    {
                        LettOut = simb[i].lett;
                        deltaMin = Math.Abs(simb[i].proc - proch);
                    }
                }
            }
            return LettOut;
        }
    }
}
