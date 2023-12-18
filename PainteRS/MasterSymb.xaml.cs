using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PainteRS
{
    /// <summary>
    /// Окно режима для разработчиков
    /// </summary>
    public partial class MasterSymb : Window
    {
        readonly MainWindow MainWindow;
        public string letter;
        public MasterSymb(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e) 
        {
            MainWindow.Start_Proc(LetterText.Text);
            MainWindow.IsProc = true;
            MainWindow.IsHitTestVisible = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow.Proc_Console();
            MainWindow.IsProc = false;
        }
    }
}
