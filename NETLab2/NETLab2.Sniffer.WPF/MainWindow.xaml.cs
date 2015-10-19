using Microsoft.Win32;
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
        bool SnifferIsRunning = false;
        Shared.ViewModels.Sniffer sniffer;
        public MainWindow()
        {
            InitializeComponent();
            sniffer = new Shared.ViewModels.Sniffer();
            DataContext = sniffer;
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit += Current_Exit;
            if (SnifferIsRunning)
            {
                sniffer.Stop();
                StartStopButton.Content = "Начать";
            }
            else
            {
                sniffer.Start();
                StartStopButton.Content = "Остановить";
            }
            SnifferIsRunning = !SnifferIsRunning;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            sniffer.Stop();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            sniffer.Clear();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "CSV";
            dialog.AddExtension = true;
            dialog.ShowDialog();
            sniffer.Export(dialog.FileName);
        }

        private void AdapterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdapterComboBox.SelectedIndex != -1)
                sniffer.CurrentInterface = sniffer.Interfaces[AdapterComboBox.SelectedIndex];
        }

        //private void ProtocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ProtocolCombobox.SelectedIndex != -1 && sniffer != null)
        //        sniffer.FilterChanged(ProtocolCombobox.SelectedIndex);
        //}

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PacketsDataGrid.SelectedIndex != -1)
                sniffer.SelectPacket(PacketsDataGrid.SelectedIndex);
        }
    }
}
