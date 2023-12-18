using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace PainteRS
{
    public partial class MainWindow : Window
    {
        bool IsLeftButtonDawn = false; // зажата ли кнопка на холсте
        bool flagKistOrStir = true; // кисть=true или ластик=false
        Point MouseCoordinates; // координата мыши
        Improc salo;
        List<double> proc;
        public bool IsProc = false;
        string WhatStr;
        int flag = 0;
        bool clear = false;
        bool flagerase=false;
        List<string> formuls = new List<string>();
        List<string> namesLibs = new List<string>();
        int LibraryCount = 0;
        List<string> ListOfACSLetters = new List<string>(8);
        bool ACSLibraryON = false;

        double CMassX = 0;  //центры
        double CMassY = 0;
        double CMatX = 0;
        double CMatY = 0;


        public MainWindow()
        {
            // Dopkrit.SeeFiles();
            InitializeComponent();
            Improc ob = new Improc(Canv);
            DataContext = ob;
            Canv.Cursor = new Cursor(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Curs\\CurP2.cur");
        }

        private void Canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsLeftButtonDawn = true;
            MouseCoordinates = e.GetPosition(Canv);
            clear = false;
            flagerase = false;
        }
        private void Canv_MouseLeave(object sender, MouseEventArgs e)
        {
            IsLeftButtonDawn = false;
        }
        private void Canv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsLeftButtonDawn = false;
        }
        private void Canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsLeftButtonDawn && flagKistOrStir)
            {
                Line line = new Line()
                {
                    X1 = MouseCoordinates.X,
                    Y1 = MouseCoordinates.Y,
                    X2 = e.GetPosition(Canv).X,
                    Y2 = e.GetPosition(Canv).Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                MouseCoordinates = e.GetPosition(Canv);
                Canv.Children.Add(line);
            }
            else if (IsLeftButtonDawn && !flagKistOrStir)
            {
                Point MPos = e.GetPosition(Canv);
                Ellipse ellipse = new Ellipse()
                {
                    Fill = Brushes.White,
                    Stroke = Brushes.White,
                    Width = 15,
                    Height = 15,

                };
                Canvas.SetLeft(ellipse, MPos.X - 5);
                Canvas.SetTop(ellipse, MPos.Y - 5);
                Canv.Children.Add(ellipse);

            }
        }

        private void CreateI_Click(object sender, RoutedEventArgs e) // Очистка холста
        {
            Canv.Children.Clear();
            Canv.Cursor = new Cursor(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Curs\\CurP2.cur");
            flagKistOrStir = true;
            Result.Text = "Вывод";
            clear = false;
            flagerase = false;
        }
        private void BrushI_Click(object sender, RoutedEventArgs e)// кисть
        {
            flagKistOrStir = true;
            Canv.Cursor = new Cursor(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Curs\\CurP2.cur");
            clear = false;
            flagerase = false;
        }
        private async void EraseI_Click(object sender, RoutedEventArgs e) // ластик
        {
            if (flagerase == true)
            {
                Canv.Children.Clear();
                Canv.Cursor = new Cursor(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Curs\\CurP2.cur");
                flagKistOrStir = true;
                flagerase = false;
                flagKistOrStir = true;
                clear = true;
            }
            else if( clear == true)
            {
                await Task.Delay(2000);
                key.Visibility = Visibility.Visible;
                key.Width = 785;
                key.Height = 450;
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\clear.jpg"));
                DOWN.Height = 0;
                UP.Height = 450;
                clear=false;
            }
            else
            {
                Canv.Cursor = new Cursor(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Curs\\CurE2.cur");
                flagKistOrStir = false;
                flagerase = true;
            }
        }
        private void FaqI_Click(object sender, MouseButtonEventArgs e)  // Инструкция
        {
            key.Visibility = Visibility.Visible;
            key.Width = 785;
            key.Height = 440;
            key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide1.png"));
            DOWN.Height = 0;
            UP.Height = 440;
            flag = 1;
            clear = false;
            flagerase = false;
        }
        private void CreatorsI_Click(object sender, RoutedEventArgs e) // Создатели
        {
            key.Visibility = Visibility.Visible;
            key.Width = 785;
            key.Height = 450;
            key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\creats.jpg"));
            DOWN.Height = 0;
            UP.Height = 450;
            clear = false;
            flagerase = false;
        }
        private void keyturnoff(object sender, MouseButtonEventArgs e)
        {
            if (flag == 0) {
                key.Visibility = Visibility.Hidden;
                key.Width = 1;
                key.Height = 1;
                UP.Height = 54;
                DOWN.Height = 407.04;
            }
            else if (flag == 1)
            {
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide2.png"));
                flag = 2;
            }
            else if (flag == 2)
            {
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide3.png"));
                flag = 3;
            }
            else if (flag == 3)
            {
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide4.png"));
                flag = 4;
            }
            else if (flag == 4)
            {
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide5.png"));
                flag = 5;
            }
            else if (flag == 5)
            {
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide6.png"));
                flag = 6;
            }
            else if (flag == 6)
            {
                key.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Guide7.png"));
                flag = 0;
            }
            clear = false;
            flagerase = false;
        } // Вывод на весь экран
        private void RezButton_Click(object sender, RoutedEventArgs e)
        {
            clear = false;
            flagerase = false;
            //Console.Clear();
            salo = new Improc(Canv);
            if (salo.BP.Count != 0)
            {
/*                Result.Text = "";*/
                var prob = salo.Otnoshenie(salo.FindRelativeCenterMassOrMat());

                CMassX = salo.centerMass.X; // обновление центров
                CMassY = salo.centerMass.Y;
                CMatX = salo.centerMat.X;
                CMatY = salo.centerMat.Y;

/*                List<string> ListOfResults = new List<string>(1 + LibraryCount);*/
                /*                var lett = salo.WhatsLett();*/

                var lett = salo.getTestString();
                string superBigString = lett.ToString() + "\n";
                Result.Text = superBigString;

/*                if (ACSLibraryON)
                {
                    object[] ObjectListOfAllMinMaxes = prob.Cast<object>().ToArray();
                    for (int i = 0; i < LibraryCount; ++i)
                    {
                        int FoundMaxIndex = salo.Calculator(formuls, ObjectListOfAllMinMaxes);
                        superBigString += string.Format("Library{0}: {1}", i, ListOfACSLetters[i][FoundMaxIndex]) + "\n";

                    }

                }
                Result.AppendText(superBigString);
                if (IsProc)
                {
                    proc.Add(prob[0]);
                }*/

                Ellipse ellipse = new Ellipse()
                {
                    Fill = Brushes.Red,
                    Stroke = Brushes.White,
                    Width = 5,
                    Height = 5,

                };
                Canvas.SetLeft(ellipse, salo.centerMass.X);
                Canvas.SetTop(ellipse, salo.centerMass.Y);
                Canv.Children.Add(ellipse);
                Ellipse ellipseMat = new Ellipse()
                {
                    Fill = Brushes.Green,
                    Stroke = Brushes.White,
                    Width = 5,
                    Height = 5,

                };
                Canvas.SetLeft(ellipseMat, salo.centerMat.X);
                Canvas.SetTop(ellipseMat, salo.centerMat.Y);
                Canv.Children.Add(ellipseMat);
            }
            else
            {
                Result.Text = "Вы не ввели символ!";
            }
        }// Кнопка результата

        public void Proc_Console()
        {
            if (proc.Count != 0)
            {
                Console.WriteLine(WhatStr + "= " + proc.Min().ToString() + " " + proc.Max().ToString() + " " + proc.Sum() / proc.Count);
            }
            else
            {
                Console.WriteLine("0");
            }
        }
        public void Start_Proc(string lett)
        {
            proc = new List<double>();
            WhatStr = lett;
        }

        private void PackageI_Click(object sender, RoutedEventArgs e)
        {
            ++LibraryCount;
            ACSLibraryON = true;
            string Line;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            namesLibs.Add(ofd.SafeFileName);

            List<String> tmp = new List<string>();
            tmp.Add(ofd.SafeFileName);
/*            Improc.instructions.Add(tmp);*/

            (DataContext as Improc).LIST = namesLibs.ToArray();
            foreach (string name in (DataContext as Improc).LIST)
            {
                Console.WriteLine(name);
            }
            StreamReader sr = new StreamReader(ofd.FileName);
/*            Line = sr.ReadLine();

            tmp.Add(Line);*/
            /*           Improc.instructions.Add(tmp);*/
            while ((Line = sr.ReadLine()) != null)
            {
                tmp.Add(Line);
            }
            sr.Close();
            Improc.instructions.Add(tmp);

            /*
                        ListOfACSLetters.Add(Line);
                        Line = sr.ReadLine();
                        while (Line != null)
                        {
                            formuls.Add(Line);
                            Line = sr.ReadLine();
                        }
                        sr.Close();*/
        }// Подключение пакетов
    }
}
