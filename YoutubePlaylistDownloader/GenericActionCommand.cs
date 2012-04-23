using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;

namespace YoutubePlaylistDownloader
{
    public class GenericActionCommand : ICommand, INotifyPropertyChanged
    {
        private Action<object> eventAction;

        public Action<object> EventAction
        {
            get { return eventAction; }
            set
            {
                eventAction = value;
                RaisePropertyChanged("EventAction");
            }
        }

        public GenericActionCommand() : this(null)
        {

        }

        public GenericActionCommand(Action<object> eventAction)
        {
            this.eventAction = eventAction;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return eventAction != null;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (eventAction != null)
                eventAction(parameter);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
