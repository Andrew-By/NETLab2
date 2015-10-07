using NETLab2.Sniffer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NETLab2.Sniffer.WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Shared.ViewModels.Sniffer sniffer;
        public MainWindow()
        {
            InitializeComponent();
            sniffer = new Shared.ViewModels.Sniffer();
            this.DataContext = sniffer;
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            sniffer.Start();
        }

        private void AdapterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdapterComboBox.SelectedIndex != -1)
                sniffer.CurrentInterface = sniffer.Interfaces[AdapterComboBox.SelectedIndex];
        }
    }
}
