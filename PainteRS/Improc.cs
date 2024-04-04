using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace PainteRS
{
    internal class Improc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public List<Point> BP { get; } = new List<Point>(); //массив для чёрных пикселей
        public List<Point> clearPoints = new List<Point>();
        public double[][] clearImage;
        public Point centerMass;
        public Point centerMat;
        double MinMaxOtnoshenie = 0.0; // отношение минимальной длинны (от центра масс до фигуры) к максимальной
                                       // List <string> formuls = new List<string>();
        List<double> otnosheniya = new List<double>(); //первые 6 элементов для ф. Otnoshenie, остальные в FindRelativeCenterMassOrMat получают координаты центров
        public static List<List<String>> instructions = new List<List<String>>();
        private double[] functions;
        Dictionary<string, Dictionary<string, double>> functionsMap;
        //List <string> namesLibs = new List<string>();

        private string[] _LIST;
        public string[] LIST
        {
            get => _LIST;
            set
            {
                _LIST = value;
                OnPropertyChanged("LIST");
            }

        }
        public Improc(FrameworkElement elem) // получает канвас
        {

            // Get the size of canvas
            Size size = new Size(elem.Width, elem.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            elem.Measure(size);
            elem.Arrange(new Rect(size));


            RenderTargetBitmap bitmap = new RenderTargetBitmap
                ((int)size.Width, (int)size.Height, 96d, 96d,
      PixelFormats.Pbgra32); //создаём "катртинку" размерам с нашь canvas
                             //Обязательно использовать Pbrgba32 !!!!!
            elem.Measure(new Size((int)elem.Width, (int)elem.Height));
            elem.Arrange(new Rect(new Size((int)elem.Width, (int)elem.Height)));

            bitmap.Render(elem);//переносим туда наше полотно

            int widthPich = bitmap.PixelWidth;
            int heightPich = bitmap.PixelHeight;
            int[] array = new int[widthPich * 4 * heightPich]; // массив для пикселей

            bitmap.CopyPixels(array, widthPich * 4, 0); // копирование информации о пикселях в массив

            int x = 0;
            int y = 0;
            List<Color> colors = new List<Color>();
            for (int i = 0; i < array.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(array[i]);
                Color pixel = Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
                colors.Add(pixel);

                // Сверху информация о пикселах каждого раздельно
            }
            //  Снизу заносятся в массив чёрные пиксели

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].ToString() == "#FF000000")
                {
                    BP.Add(new Point(x, y));
                }
                if (i == widthPich * (y + 1) - 1)
                {
                    x = -1;
                    ++y;
                }
                x++;
            }
        }
        public (double cxl, double cxr, double cyt, double cyd) Counter()
        {
            double cxl = 0, cxr = 0, cyt = 0, cyd = 0;
            foreach (Point item in BP)
            {
                if (item.X < centerMat.X)
                {
                    cxl++;
                }
                else if (item.X > centerMat.X)
                {
                    cxr++;
                }
                if (item.Y < centerMat.Y)
                {
                    cyd++;
                }
                else if (item.Y > centerMat.Y)
                {
                    cyt++;
                }
            }
            return (cxl, cxr, cyt, cyd);
        }
        public int Calculator(List<string> TheMainFormula, object[] VariablesLol)
        {
            List<object> Probabilities = new List<object>();
            for (int i = 0; i < TheMainFormula.Count(); ++i)
            {
                string newString = string.Format(TheMainFormula[i], VariablesLol);
                newString = newString.Replace(',', '.');
                var result = new DataTable().Compute(newString, "");

                if (result.Equals(0) != true)
                {
                    Probabilities.Add(result);
                }
                else
                {
                    Probabilities.Add(0.00000000001);
                }
                //Probabilities.Add(result);

            }
            var themax = Probabilities.Max();
            int MaxIndex = Probabilities.IndexOf(themax);

            return MaxIndex;
        }
        public List<Point> FindRelativeCenterMassOrMat()
        {
            var centerMatMin = BP[0];
            Point centerMatMax = new Point(0, 0);

            foreach (var item in BP) // находит центр масс и математический центр
            {
                centerMass.X += item.X;
                centerMass.Y += item.Y;
                if (item.Y < centerMatMin.Y) centerMatMin.Y = item.Y;
                else if (item.Y > centerMatMax.Y) centerMatMax.Y = item.Y;
                if (item.X < centerMatMin.X) centerMatMin.X = item.X;
                else if (item.X > centerMatMax.X) centerMatMax.X = item.X;
            }

            centerMat = new Point((centerMatMax.X + centerMatMin.X) / 2,
                                  (centerMatMax.Y + centerMatMin.Y) / 2); // математический центр

            centerMass.X /= BP.Count;
            centerMass.Y /= BP.Count;

            /*otnosheniya[6] = centerMass.X;
            otnosheniya[7] = centerMass.Y;
            otnosheniya[8] = centerMat.X;
            otnosheniya[9] = centerMat.Y;*/

            Point LD = new Point(centerMatMin.X, centerMatMin.Y); // Left Down 
            Point RD = new Point(centerMatMax.X, centerMatMin.Y); // Right down
            Point LU = new Point(centerMatMin.X, centerMatMax.Y); // Left Up
            Point RU = new Point(centerMatMax.X, centerMatMax.Y); // Right Up                                                
            Point LM = new Point(centerMatMin.X, centerMat.Y); // Left Middle 
            Point MU = new Point(centerMat.X, centerMatMax.Y); // Middle up 
            List<Point> points = new List<Point> { centerMass, LD, LU, RD, RU, LM, MU, centerMat };
            return points;
        }
        public List<double> Koordinati(List<Point> cenrtesMass)
        {
            List<double> koordinati = new List<double>();
            foreach (Point item in cenrtesMass)
            {
                koordinati.Add(item.X);
                koordinati.Add(item.Y);
            }
            return koordinati;
        }

        public List<double> VsePeremennie((double, double, double, double) tuple, List<double> LIST1, List<double> LIST2) // создает список всех переменных для ацс
        {
            List<double> peremennie = new List<double>();
            peremennie.Add(tuple.Item1);
            peremennie.Add(tuple.Item2);
            peremennie.Add(tuple.Item3);
            peremennie.Add(tuple.Item4);
            peremennie.AddRange(LIST1);
            peremennie.AddRange(LIST2);
            return peremennie;


        }

        public List<double> Otnoshenie(List<Point> cenrtesMass)
        {

            double mindist = 1000.0;
            double maxdist = 0.0;
            double dist;
            int l = cenrtesMass.Count;
            for (int i = 0; i < cenrtesMass.Count; i++)
            {
                foreach (Point item in BP)
                {
                    //БРАКОВАНО
                    dist = Math.Sqrt(((item.X - cenrtesMass[i].X) * (item.X - cenrtesMass[i].X))
                                   + ((item.Y - cenrtesMass[i].Y) * (item.Y - cenrtesMass[i].Y)));
                    if (dist > maxdist) { maxdist = dist; }
                    if (dist < mindist && dist != 0) { mindist = dist; }
                }

                MinMaxOtnoshenie = mindist / maxdist;
                otnosheniya.Add(MinMaxOtnoshenie);
            }

            return otnosheniya;
        }

/*        private void incstruction0()
        {
            if (otnosheniya[0] < 0.2)
            {
                return "1";
            }
            else if (otnosheniya[0] > 0.35)
            {
                return "0";
            }
            else
            {
                return "NULL";
            }
        }*/

        public string WhatsLett() 
        {
            List<string> res = instructions[0];
            string result = "";
            for (int i = 0; i < res.Count; ++i)
            {
                result += " next " + res[i];
            }
            return result;
/*            if (otnosheniya[0] < 0.2)
            {
                return "1";
            }
            else if (otnosheniya[0] > 0.35)
            {
                return "0";
            }
            else
            {
                return "NULL";
            }*/
        }// Возвращает цифру которую мы выводим



        // Let's GO!!!
        private List<Point> getClearPoints()
        {
            double minX, minY;
            minX = BP[0].X;
            minY = BP[0].Y;
            for (int i = 0; i < BP.Count; i++)
            {
                if (BP[i].X < minX) { minX = BP[i].X; }
                if (BP[i].Y < minY) { minY = BP[i].Y; }
            }
            List<Point> clearPoints = new List<Point>();
            for (int i = 0; i < BP.Count; i++)
            {
                clearPoints.Add(new Point(BP[i].X - minX, BP[i].Y - minY));
            }
            return clearPoints;
        }

        private double[][] getClearImage()
        {
            clearPoints = getClearPoints();
            double maxX = clearPoints[0].X;
            double maxY = clearPoints[0].Y;

            for (int i = 0; i < clearPoints.Count; i++)
            {
                if (clearPoints[i].X > maxX) { maxX = clearPoints[i].X; }
                if (clearPoints[i].Y > maxY) { maxY = clearPoints[i].Y; }
            }
            int mX = (int)maxX;
            int mY = (int)maxY;

            double[][] clearImage = new double[mY + 1][];
            for (int i = 0; i < mY + 1; i++)
            {
                clearImage[i] = new double[mX + 1];
            }

            for (int i = 0; i < clearImage.Length; i++)
            {
                for (int j = 0; j < clearImage[0].Length; j++)
                {
                    clearImage[i][j] = 0;
                }
            }
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                clearImage[(int)clearPoints[i].Y][(int)clearPoints[i].X] = 1;
            }
            return clearImage;
        }

        // Служебная
        private Point getMax()
        {
            clearPoints = getClearPoints();
            double maxX = clearPoints[0].X;
            double maxY = clearPoints[0].Y;

            for (int i = 0; i < clearPoints.Count; i++)
            {
                if (clearPoints[i].X > maxX) { maxX = clearPoints[i].X; }
                if (clearPoints[i].Y > maxY) { maxY = clearPoints[i].Y; }
            }
            int mX = (int)maxX;
            int mY = (int)maxY;
            return new Point(maxX, maxY);
        }

        // Точки

        private Point getCenterMass()
        {
            double sumX = 0;
            double sumY = 0;
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                sumX += clearPoints[i].X;
                sumY += clearPoints[i].Y;
            }
            return new Point(sumX / clearPoints.Count, sumY / clearPoints.Count);
        }

        private Point getLeftUp()
        {
            return new Point(0, 0);
        }

        private Point getCenterUp()
        {
            return new Point(clearImage[0].Length / 2, 0);
        }

        private Point getRightUp()
        {
            return new Point(clearImage[0].Length - 1, 0);
        }

        private Point getLeftCenter()
        {
            return new Point(0, clearImage.Length / 2);
        }

        private Point getCenterCenter()
        {
            return new Point(clearImage[0].Length / 2, clearImage.Length / 2);
        }

        private Point getRightCenter()
        {
            return new Point(clearImage[0].Length - 1, clearImage.Length / 2);
        }

        private Point getLeftDown()
        {
            return new Point(0, clearImage.Length - 1);
        }

        private Point getCenterDown()
        {
            return new Point(clearImage[0].Length / 2, clearImage.Length - 1);
        }

        private Point getRightDown()
        {
            return new Point(clearImage[0].Length - 1, clearImage.Length - 1);
        }

        // L min max
        private double getLmin(Point p)
        {
            double minR = calculateR(p, clearPoints[0]);
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                double tmp = calculateR(p, clearPoints[i]);
                if (tmp < minR)
                {
                    minR = tmp;
                }
            }
            return minR;
        }

        private double getLmax(Point p)
        {
            double maxR = calculateR(p, clearPoints[0]);
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                double tmp = calculateR(p, clearPoints[i]);
                if (tmp > maxR)
                {
                    maxR = tmp;
                }
            }
            return maxR;
        }

        private double getLminToMax(Point p)
        {
            return getLmin(p) / getLmax(p);
        }

        private double getCircle(Point p)
        {
            double percent = 0.115;
            double r = clearImage.Length * percent;
            int count = 0;
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                if (calculateR(clearPoints[i], p) < r)
                {
                    ++count;
                }
            }
            return (double) count / clearPoints.Count;
        }

        private String myFormul()
        {
            double[] coeffs = { getLminToMax(getCenterMass()), getLminToMax(getCenterCenter()), getLminToMax(getLeftDown()), getLminToMax(getRightDown()),
            getLminToMax(getLeftUp()), getLminToMax(getRightUp()), getLminToMax(getLeftCenter()), getLminToMax(getCenterUp())};


            Dictionary<String, double> map = new Dictionary<String, double>();
            String[] letters = { "F", "L", "Y", "T", "H" };
            for (int i = 0; i < letters.Length; ++i)
            {
                double imp;
                if (letters[i] == "F")
                {
                    imp = (-4.21E1 + 2.16E2 * coeffs[0] + 3.2 * coeffs[1] + 2.41E2 * coeffs[3]) / 4.19E2;
                }
                else if (letters[i] == "L")
                {
                    imp = (-1.23E1 + 8.13E1 * coeffs[1] + 4.15E2 * coeffs[5] + 3.1 * coeffs[6]) / 4.87E2;
                }
                else if (letters[i] == "Y")
                {
                    imp = (-1.97 + 1.9E2 * coeffs[6] + 7.58E1 * coeffs[7]) / 2.64E2;
                }
                else if (letters[i] == "T")
                {
                    imp = (1.5E2 * coeffs[2] + 1.72 * coeffs[3] + 2.76E1 * coeffs[6]) / 1.8E2;
                }
                else if (letters[i] == "H")
                {
                    imp = (1.18 * coeffs[7]) / 1.18;
                }
                else
                {
                    imp = 0;
                }
                map.Add(letters[i], imp);
            }

            String maxLetter = letters[0];
            foreach (KeyValuePair<String, double> pair in map)
            {
                Console.WriteLine("Ключ: {0}, Значение: {1}", pair.Key, pair.Value);
                if (pair.Value > map[maxLetter])
                {
                    maxLetter = pair.Key;
                }
            }
            return maxLetter;
        }

        // Расстояние
        private double calculateR(Point p1, Point p2)
        {
            double rX = p1.X - p2.X;
            double rY = p1.Y - p2.Y;
            return Math.Sqrt(rX * rX + rY * rY);
        }

        private double sigmoid(double x, double max)
        {
            double slope = 10;
            double scaledValue = x / max;
            return 1 / (1 + Math.Exp(-slope * (scaledValue - 0.5)));
        }

        // Точка входа

        private string getAllResults()
        {
            // Для первого формата
            string mainString = "";
            for (int i = 0; i < instructions.Count; ++i)
            {
                string instructionFormat = instructions[i][0].Split('.')[1];
                if (instructionFormat == "txt")
                {
                    mainString += instructions[i][1] + "\n";
                    mainString += instructionsReader(i) + "\n";
                }
                else if (instructionFormat == "rsr")
                {
                    mainString += instructions[i][1] + "\n";
                    // новая интерпретация!!!
                    mainString += instructionsReader2(i);
                } else if (instructionFormat == "rsrm")
                {
                    mainString += instructions[i][1] + "\n";
                    // мини формат
                    mainString += instructionsReaderMini(i);
                }
            }
            return mainString;
        }

        private string instructionsReaderMini(int instructionNumber)
        {
            List<string> instruction = instructions[instructionNumber];
            Dictionary<string, double> results = new Dictionary<string, double>();
            Dictionary<string, double> percents = new Dictionary<string, double>();
            int stringNumber = 2;
            // блок 1 создание мапы результатов
            int instructionsCount = int.Parse(instruction[stringNumber]);
            ++stringNumber;
            for (int i = 0; i < instructionsCount; ++i)
            {
                string symbol = instruction[stringNumber][0].ToString();
                string mathExpression = instruction[stringNumber].Substring(2);
                double value = getCalculate(mathExpression);
                results.Add(symbol, value);
                ++stringNumber;
            }
            if (instruction[stringNumber].Trim() == "endfile")
            {
                foreach (var pair in results)
                {
                    percents.Add(pair.Key, pair.Value);
                }
            } else
            {
                // блок 2 (опционально)
                instructionsCount = int.Parse(instruction[stringNumber]);
                ++stringNumber;
                for (int i = 0; i < instructionsCount; ++i)
                {
                    string symbol = instruction[stringNumber][0].ToString();
                    string mathExpression = instruction[stringNumber].Substring(2);
                    double value = getCalculate(mathExpression);
                    percents.Add(symbol, value);
                    ++stringNumber;
                }
            }

            string symb = getMaxLetter(results);
            // проценты
            string result = "res: " + symb + "\n";
            foreach (var pair in percents)
            {
                result += pair.Key + ": " + Math.Round(pair.Value * 100, 2) + "%\n";
            }
            return result;
        }

        private string instructionsReader2(int instructionNumber)
        {
            List<string> instruction = instructions[instructionNumber];
            Dictionary<string, double> globalVariables = new Dictionary<string, double>();
            Dictionary<string, double> results = new Dictionary<string, double>();
            Dictionary<string, double> percents = new Dictionary<string, double>();
            int stringNumber = 2;
            // блок 1 создание мапы глобальных переменных
            int instructionsCount = int.Parse(instruction[stringNumber]);
            ++stringNumber;
            for (int i = 0; i < instructionsCount; ++i)
            {
                string[] pair = instruction[stringNumber].Split();
                string key = pair[0];
                int instCount = int.Parse(pair[1]);
                double value = getValue2(stringNumber, instruction, instCount, globalVariables);
                globalVariables.Add(key, value);
                stringNumber += instCount + 1;
            }
            if (instruction[stringNumber] == "endfile")
            {
                foreach (var pair in globalVariables)
                {
                    percents.Add(pair.Key, pair.Value);
                    results.Add(pair.Key, pair.Value);
                }
            }
            else
            {
                // блок 2 создание мапы процентов
                instructionsCount = int.Parse(instruction[stringNumber]);
                ++stringNumber;
                for (int i = 0; i < instructionsCount; ++i)
                {
                    string[] pair = instruction[stringNumber].Split();
                    string key = pair[0];
                    int instCount = int.Parse(pair[1]);
                    double value = getValue2(stringNumber, instruction, instCount, globalVariables);
                    percents.Add(key, value);
                    stringNumber += instCount + 1;
                }
                // блок 3 создание мапы результатов
                instructionsCount = int.Parse(instruction[stringNumber]);
                ++stringNumber;
                for (int i = 0; i < instructionsCount; ++i)
                {
                    string[] pair = instruction[stringNumber].Split();
                    string key = pair[0];
                    int instCount = int.Parse(pair[1]);
                    double value = getValue2(stringNumber, instruction, instCount, globalVariables);
                    results.Add(key, value);
                    stringNumber += instCount + 1;
                }
            }
            // Создание строки вывода
            // извлечение максимума
            string symb = getMaxLetter(results);
            // проценты
            string result = "res: " + symb + "\n";
            foreach (var pair in percents)
            {
                result += pair.Key + ": " + Math.Round(pair.Value * 100, 2) + "%\n";
            }
            return result;
        }

        private string instructionsReader(int instructionNumber)
        {
            // 0 название файла, 1 буквы, 2 заголовок инструкции (число переменных)
            List<string> instruction = instructions[instructionNumber];
            Dictionary<string, double> results = new Dictionary<string, double>();
            string[] letters = instruction[1].ToCharArray().Select(c => c.ToString()).ToArray();
            int stringNumber = 2;
            for (int i = 0; i < letters.Length; ++i)
            {
                results.Add(letters[i], getValue(stringNumber, instruction));
                stringNumber += int.Parse(instruction[stringNumber]) + 1;
            }

            return getMaxLetter(results);
        }

        private string getMaxLetter(Dictionary<string, double> letters)
        {
            string maxLetter = letters.FirstOrDefault(x => x.Value == letters.Values.Max()).Key;
            return letters[maxLetter] == 0 ? "Не распознано" : maxLetter;
        }

        private double getValue2(int stringNumber, List<string> instruction, int variablesCount, Dictionary<string, double> globalVariables)
        {
            double[] variables = new double[variablesCount];
            int start = stringNumber + 1;
            int end = start + variablesCount;
            for (int i = start; i < end; ++i)
            {
                string[] tmp = instruction[i].Split(new char[] { ' ' });
                int index = int.Parse(tmp[0]);
                double var1 = createVariable(tmp[2], tmp[3], variables, globalVariables);
                double var2 = createVariable(tmp[4], tmp[5], variables, globalVariables);
                variables[index] = getResultOperation(tmp[1], var1, var2);
            }
            return variables[variables.Length - 1];
        }

        private double getValue(int stringNumber, List<string> instruction)
        {
            double[] variables = new double[int.Parse(instruction[stringNumber])];
            int start = stringNumber + 1;
            int end = start + int.Parse(instruction[stringNumber]);
            for (int i = start; i < end; ++i)
            {
                string[] tmp = instruction[i].Split(new char[] { ' ' });
                int index = int.Parse(tmp[0]);
                double var1 = createVariable(tmp[2], tmp[3], variables);
                double var2 = createVariable(tmp[4], tmp[5], variables);
                variables[index] = getResultOperation(tmp[1], var1, var2);
            }
            return variables[variables.Length - 1];
        }

        private double getResultOperation(string type, double var1, double var2)
        {
            if (type == "+") { return var1 + var2; }
            if (type == "-") { return var1 - var2; }
            if (type == "*") { return var1 * var2; }
            if (type == "/") { return var1 / var2; }
            if (type == "abs") { return Math.Abs(var1); }
            if (type == "sqrt") { return Math.Sqrt(var1); }
            if (type == "=") { return var1; }
            if (type == "<") { return var1 < var2 ? 1 : 0; }
            if (type == ">") { return var1 > var2 ? 1 : 0; }
            if (type == "==") { return var1 == var2 ? 1 : 0; }
            if (type == "&") { return (!(var1 == 0)) && (!(var2 == 0)) ? 1 : 0; }
            if (type == "|") { return (!(var1 == 0)) || (!(var2 == 0)) ? 1 : 0; }
            if (type == "!") { return (!(var1 == 0)) ? 0 : 1; }
            if (type == "sig") { return sigmoid(var1, var2); }
            return 0;
        }

        private double createVariable(string type, string value, double[] variables)
        {
            if (type == "c") {
                double res;
                double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out res);
                return res;
            }
            if (type == "v") { return variables[int.Parse(value)]; }
            if (type == "f") { return getFunctionValue(value); }
            return 0;
        }

        private double createVariable(string type, string value, double[] variables, Dictionary<string, double> globalVariables)
        {
            if (type == "c")
            {
                double res;
                double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out res);
                return res;
            }
            if (type == "v") { return variables[int.Parse(value)]; }
            if (type == "gv") { return globalVariables[value]; }
            if (type == "f") { return getFunctionValue(value); }
            return 0;
        }

        private double functionReader(string name)
        {
            string[] nameArr = name.Split('(');
            if (nameArr[0] == "Lmin")
            {
                if (nameArr[1].Substring(0, nameArr[1].Length - 1) == "cm")
                {
                    return getLmin(getCenterMass());
                }
                string[] coordinates = nameArr[1].Substring(0, nameArr[1].Length - 1).Split(',');
                double coorX, coorY;

                double.TryParse(coordinates[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out coorX);
                double.TryParse(coordinates[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out coorY);
                return getLmin(new Point(coorX * clearImage[0].Length - 1, coorY * clearImage.Length - 1));
            }
            return 0;
        }

        private double getFunctionValue(string name)
        {
            double[] coeffs = this.functions;
            double res;
            // naming
            string[] nameArr = name.Split(new string[] { "::" }, StringSplitOptions.None);
            if (!int.TryParse(nameArr[0], out int result)) { 
                if (nameArr.Length == 1)
                {
                    return functionReader(nameArr[0]);
                }
                return this.functionsMap[nameArr[0]][nameArr[1]]; 
            }

            switch (name)
            {
                case "0":
                    res = coeffs[0]; break;
                case "1":
                    res = coeffs[1]; break;
                case "2":
                    res = coeffs[2]; break;
                case "3":
                    res = coeffs[3]; break;
                case "4":
                    res = coeffs[4]; break;
                case "5":
                    res = coeffs[5]; break;
                case "6":
                    res = coeffs[6]; break;
                case "7":
                    res = coeffs[7]; break;
                case "8":
                    res = coeffs[8]; break;
                case "9":
                    res = coeffs[9]; break;
                case "10":
                    res = coeffs[10]; break;
                case "11":
                    res = coeffs[11]; break;
                case "12":
                    res = coeffs[12]; break;
                case "13":
                    res = coeffs[13]; break;
                case "14":
                    res = coeffs[14]; break;
                case "15":
                    res = coeffs[15]; break;
                case "16":
                    res = coeffs[16]; break;
                case "17":
                    res = coeffs[17]; break;
                case "18":
                    res = coeffs[18]; break;
                case "19":
                    res = coeffs[19]; break;
                case "20":
                    res = coeffs[20]; break;
                case "21":
                    res = coeffs[21]; break;
                case "22":
                    res = coeffs[22]; break;
                case "23":
                    res = coeffs[23]; break;
                case "24":
                    res = coeffs[24]; break;
                case "25":
                    res = coeffs[25]; break;
                case "26":
                    res = coeffs[26]; break;
                case "27":
                    res = coeffs[27]; break;
                case "28":
                    res = coeffs[28]; break;
                case "29":
                    res = coeffs[29]; break;
                case "30":
                    res = coeffs[30]; break;
                case "31":
                    res = coeffs[31]; break;
                case "32":
                    res = coeffs[32]; break;
                case "33":
                    res = coeffs[33]; break;
                case "34":
                    res = coeffs[34]; break;
                case "35":
                    res = coeffs[35]; break;
                case "36":
                    res = coeffs[36]; break;
                default: res = 0; break;
            }
            return res;
        }

        private int getPixelsCount0()
        {
            int count = 0;
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                if (clearPoints[i].Y <  (double) clearImage.Length / 2)
                {
                    ++count;
                }
            }
            return count;
        }

        private int getPixelsCount1()
        {
            int count = 0;
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                if (clearPoints[i].Y > (double) clearImage.Length / 2)
                {
                    ++count;
                }
            }
            return count;
        }

        private int getPixelsCount2()
        {
            int count = 0;
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                if (clearPoints[i].X > (double) clearImage[0].Length / 2)
                {
                    ++count;
                }
            }
            return count;
        }

        private int getPixelsCount3()
        {
            int count = 0;
            for (int i = 0; i < clearPoints.Count; ++i)
            {
                if (clearPoints[i].X < (double) clearImage[0].Length / 2)
                {
                    ++count;
                }
            }
            return count;
        }

        private string getMainCalculate()
        {
            string exp = "f->sig(2.7 + (f->abs(4 - 64 / 4) + 2) * 3 / f->0, 5)";
            string exp2 = "2 + 2 * 5";
            string exp3 = "f->abs(4 - 64 / 4)";

            double value = getCalculate(exp);
            return value.ToString();
        }

        private double getCalculate(string exp)
        {
            List<string> fullSplit = getFullSplit(exp);
            if (fullSplit.Count == 1)
            {
                return getValueCalculate(fullSplit[0], "null");
            }

            string tmpExp = "";
            double value = getValueCalculate(fullSplit[0], fullSplit[1]);

            // Выполняем операции
            for (int i = 1; i < fullSplit.Count; ++i)
            {
                if (fullSplit[i] == "+" || fullSplit[i] == "-")
                {
                    string newStr = "";
                    if (fullSplit[i] == "-")
                    {
                        newStr += "-1 * " + fullSplit[i + 1] + " ";
                    }
                    else
                    {
                        newStr += fullSplit[i + 1] + " ";
                    }
                    if (i + 2 < fullSplit.Count)
                    {
                        for (int j = i + 2; j < fullSplit.Count; ++j)
                        {
                            newStr += fullSplit[j] + " ";
                        }
                    }
                    
                    return value + getCalculate(newStr);
                }

                if (fullSplit[i] == "*" || fullSplit[i] == "/")
                {
                    double arg2;
                    if (fullSplit[i + 1][0] == 'f' && i + 2 < fullSplit.Count && fullSplit[i + 2][0] == '(')
                    {
                        arg2 = getValueCalculate(fullSplit[i + 1], fullSplit[i + 2]);
                    }
                    else
                    {
                        arg2 = getValueCalculate(fullSplit[i + 1], "null");
                    }
                    if (fullSplit[i] == "*") { value *= arg2; }
                    if (fullSplit[i] == "/") { value /= arg2; }
                    ++i;
                }
            }
            return value;
        }

        // f->1; f->Lminmax::CenterLeft; f->abs(23 - 12 + 4); f->sig(23, 32);

        private double getValueCalculate(string value, string arg)
        {
            if (value[0] == '(')
            {
                return getCalculate(value.Substring(1, value.Length - 2));
            }
            else if (value[0] == 'f')
            {
                string func = value.Substring(3);
                if (func == "abs")
                {
                    return Math.Abs(getCalculate(arg));
                } else if (func == "sqrt")
                {
                    return Math.Sqrt(getCalculate(arg));
                } else if (func == "sig")
                {
                    string[] arguments = arg.Substring(1, arg.Length - 2).Split(',');
                    double arg1 = getCalculate(arguments[0]);
                    double arg2 = getCalculate(arguments[1]);
                    return sigmoid(arg1, arg2);
                }
                else
                {
                    return getFunctionValue(func);
                }
            }
            else
            {
                double res;
                double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out res);
                return res;
            }
        }

        private List<string> getFullSplit(string exp)
        {
            List<string> expSplit = getSplitString(exp);
            List<string> resultString = new List<string>();
            string tmp;
            string[] tmpArr;

            for (int i = 0; i < expSplit.Count; ++i)
            {
                if (expSplit[i].Contains("("))
                {
                    resultString.Add(expSplit[i]);
                }
                else
                {
                    tmp = expSplit[i].Trim();
                    tmpArr = tmp.Split(' ');
                    for (int j = 0; j < tmpArr.Length; ++j) { resultString.Add(tmpArr[j]); }
                }
            }

            return resultString;
        }

        private List<string> getSplitString(string exp)
        {
            List<string> mainArr = new List<string>();

            bool bracketFlag = true;
            int bracketsCount = 0;
            string tmpStr = "";
            for (int i = 0; i < exp.Length; ++i)
            {
                bracketFlag = true;
                if (exp[i] == '(')
                {
                    if (bracketsCount == 0)
                    {
                        if (!(tmpStr.Length == 0)) { mainArr.Add(tmpStr); tmpStr = ""; }
                    }
                    ++bracketsCount;
                }
                if (exp[i] == ')')
                {
                    --bracketsCount;
                    if (bracketsCount == 0)
                    {
                        tmpStr += exp[i];
                        bracketFlag = false;
                        if (!(tmpStr.Length == 0)) { mainArr.Add(tmpStr); tmpStr = ""; }
                    }
                }

                if (bracketFlag) { tmpStr += exp[i]; }
            }
            if (!(tmpStr.Length == 0)) { mainArr.Add(tmpStr); tmpStr = ""; }

            return mainArr;
        }

        public String getTestString()
        {
            init();
            string res = "Ядро: ";
            if (getLminToMax(getCenterMass()) < 0.2)
            {
                res += "1";
            }
            else if (getLminToMax(getCenterMass()) > 0.35)
            {
                res += "0";
            }
            else
            {
                res += "Не распознано";
            }
            res += "\n" + getAllResults();
/*            res = clearPoints.Count.ToString();*/
            return res;
        }

        private void init()
        {
            clearPoints = getClearPoints();
            clearImage = getClearImage();
            double[] functions = {
                getPixelsCount0(), getPixelsCount1(), getPixelsCount2(), getPixelsCount3(), getLminToMax(getCenterMass()),
                getLminToMax(getLeftDown()), getLminToMax(getRightDown()), getLminToMax(getLeftUp()), getLminToMax(getRightUp()),
                getLminToMax(getLeftCenter()), getLminToMax(getCenterUp()), getLminToMax(getCenterCenter()),
                // Координаты
                getCenterMass().X, getCenterMass().Y, getLeftDown().X, getLeftDown().Y, getRightDown().X, getRightDown().Y,
                getLeftUp().X, getLeftUp().Y, getRightUp().X, getRightUp().Y, getLeftCenter().X, getLeftCenter().Y, getCenterUp().X,
                getCenterUp().Y, getCenterCenter().X, getCenterCenter().Y,
                // Серклы 28                29                      30                          31                              32
                getCircle(getCenterUp()), getCircle(getRightUp()), getCircle(getLeftCenter()), getCircle(getCenterCenter()), getCircle(getCenterDown()),
                //  33                      34                      35                          36
                getCircle(getLeftDown()), getCircle(getLeftUp()), getCircle(getRightCenter()), getLminToMax(getCenterDown())};
            this.functions = functions;

            // functionsMap
            Dictionary<string, Dictionary<string, double>> fMap = new Dictionary<string, Dictionary<string, double>>();
            fMap.Add("pixelsCount", new Dictionary<string, double>());
            fMap.Add("Lmin/max", new Dictionary<string, double>());
            fMap.Add("CoordinateX", new Dictionary<string, double>());
            fMap.Add("CoordinateY", new Dictionary<string, double>());
            fMap.Add("Circle", new Dictionary<string, double>());

            // Добавление значений
            fMap["pixelsCount"].Add("Up", functions[0]);
            fMap["pixelsCount"].Add("Down", functions[1]);
            fMap["pixelsCount"].Add("Right", functions[2]);
            fMap["pixelsCount"].Add("Left", functions[3]);

            fMap["Lmin/max"].Add("CenterMass", functions[4]);
            fMap["Lmin/max"].Add("LeftDown", functions[5]);
            fMap["Lmin/max"].Add("RightDown", functions[6]);
            fMap["Lmin/max"].Add("LeftUp", functions[7]);
            fMap["Lmin/max"].Add("RightUp", functions[8]);
            fMap["Lmin/max"].Add("LeftCenter", functions[9]);
            fMap["Lmin/max"].Add("CenterUp", functions[10]);
            fMap["Lmin/max"].Add("CenterCenter", functions[11]);

            fMap["CoordinateX"].Add("CenterMass", functions[12]);
            fMap["CoordinateX"].Add("LeftDown", functions[14]);
            fMap["CoordinateX"].Add("RightDown", functions[16]);
            fMap["CoordinateX"].Add("LeftUp", functions[18]);
            fMap["CoordinateX"].Add("RightUp", functions[20]);
            fMap["CoordinateX"].Add("LeftCenter", functions[22]);
            fMap["CoordinateX"].Add("CenterUp", functions[24]);
            fMap["CoordinateX"].Add("CenterCenter", functions[26]);

            fMap["CoordinateY"].Add("CenterMass", functions[13]);
            fMap["CoordinateY"].Add("LeftDown", functions[15]);
            fMap["CoordinateY"].Add("RightDown", functions[17]);
            fMap["CoordinateY"].Add("LeftUp", functions[19]);
            fMap["CoordinateY"].Add("RightUp", functions[21]);
            fMap["CoordinateY"].Add("LeftCenter", functions[23]);
            fMap["CoordinateY"].Add("CenterUp", functions[25]);
            fMap["CoordinateY"].Add("CenterCenter", functions[27]);

            fMap["Circle"].Add("CenterUp", functions[28]);
            fMap["Circle"].Add("RightUp", functions[29]);
            fMap["Circle"].Add("LeftCenter", functions[30]);
            fMap["Circle"].Add("CenterCenter", functions[31]);
            fMap["Circle"].Add("CenterDown", functions[32]);
            fMap["Circle"].Add("LeftDown", functions[33]);
            fMap["Circle"].Add("LeftUp", functions[34]);
            fMap["Circle"].Add("RightCenter", functions[35]);

            fMap["Lmin/max"].Add("CenterDown", functions[36]);

            this.functionsMap = fMap;
        }

    }
}
