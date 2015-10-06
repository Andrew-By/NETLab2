using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NETLab2.Sniffer.Shared.ViewModels
{
    public class Packet : INotifyPropertyChanged
    {


        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
