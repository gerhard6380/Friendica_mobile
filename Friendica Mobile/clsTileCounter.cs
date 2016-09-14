using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile
{
    public class clsTileCounter : BindableClass
    {
        private int _counterUnseenHome;
        public int CounterUnseenHome
        {
            get { return _counterUnseenHome;  }
            set
            {
                if (value < 100 && value >= 0)
                    _counterUnseenHome = value;
                else if (value >= 100)
                    _counterUnseenHome = 99;
                OnPropertyChanged("CounterUnseenHome");
            }
        }

        private int _counterUnseenNetwork;
        public int CounterUnseenNetwork
        {
            get { return _counterUnseenNetwork; }
            set
            {
                if (value < 100 && value >= 0)
                    _counterUnseenNetwork = value;
                else if (value >= 100)
                    _counterUnseenNetwork = 99;
                OnPropertyChanged("CounterUnseenNetwork");
            }
        }

        private int _counterUnseenMessages;
        public int CounterUnseenMessages
        {
            get { return _counterUnseenMessages; }
            set
            {
                if (value < 100 && value >= 0)
                    _counterUnseenMessages = value;
                else if (value >= 100)
                    _counterUnseenMessages = 99;
                OnPropertyChanged("CounterUnseenMessages");
            }
        }

        public clsTileCounter()
        {
            // retrieve values on loading clsTileCounter in App.xaml.cs
            //CounterUnseenHome = 5;
            //CounterUnseenNetwork = 100;
            CounterUnseenMessages = 0;
        }

        public void DeleteCounterOnLiveTile()
        {
            // Aktivität zum Rücksetzen des counters auf dem Live Tile
            throw new NotImplementedException();
        }

        public void ResetCounter(object counter)
        {
            // Aktivität zum Rücksetzen eines Counters in der App
            throw new NotImplementedException();
        }
    }

}
