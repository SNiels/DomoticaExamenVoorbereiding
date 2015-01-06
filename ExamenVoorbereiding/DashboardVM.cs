using GalaSoft.MvvmLight.Command;
using OPCLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ExamenVoorbereiding.VM
{
    public class DashboardVM:INotifyPropertyChanged
    {

        private const string BASE_PATH = "examenvoorbereding.PLC.";
        private OPCServerWrapper _server;
        private OPCNodeWrapper _automatischWrapperNode;
        private OPCNodeWrapper _vermogenWrapperNode;
        private OPCNodeWrapper _isVoldoendeNode;
        private OPCNodeWrapper _isWasMachineAanNode;
        private OPCNodeWrapper _drukknopNode;
        private DispatcherTimer _tmr;
        private double _vermogenDrempel;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Point> Points { get; set; }

        public bool Auto
        {
            get
            {
                return (bool)_automatischWrapperNode.Value;
            }
            set {
                _automatischWrapperNode.Value = value;
            }
        }

        public int GemetenVermogen
        {
            get
            {
                return (int)_vermogenWrapperNode.Value;
            }
            set
            {
                _vermogenWrapperNode.Value = value;
            }
        }

        public bool IsVoldoende
        {
            get
            {
                return (bool)_isVoldoendeNode.Value;
            }
            set
            {
                _isVoldoendeNode.Value = value;
            }
        }

        public double VermogenDrempel
        {
            get
            {
                return _vermogenDrempel;
            }
            set
            {
                _vermogenDrempel = value;
                OnPropertyChanged("VermogenDrempel");
                bool isVoldoende = IsVoldoende;
                IsVoldoende = GemetenVermogen > VermogenDrempel;
                if (isVoldoende != IsVoldoende)
                    OnPropertyChanged("IsVoldoende");
            }
        }

        public bool MachineDraait
        {
            get {
                return (bool)_isWasMachineAanNode.Value;
            }
            set {
                _isWasMachineAanNode.Value = value;
            }
        }

        public bool Drukknop
        {
            get
            {
                return (bool)_drukknopNode.Value;
            }
            set
            {
                _drukknopNode.Value = value;
            }
        }

        public DashboardVM()
        {
            Points = new ObservableCollection<Point>();
            Init();
        }

        private void Init()
        {
            GetWrappers();
            _tmr = new DispatcherTimer();
            _tmr.Interval = new TimeSpan(0,0,1);
            _tmr.Tick += _tmr_Tick;
            SubscribeToWrappers();
            _tmr.Start();
        }

        private void GetWrappers()
        {
            _server = OPCServerWrapper.GetOPCServerWrapper("Kepware.KEPServerEX.V5");
            OPCServerWrapper.SelectedServer = _server;
            var wrappers = _server.GetLeaves();
            _automatischWrapperNode = wrappers.Single(wrapper => wrapper.ItemId == BASE_PATH + "Automatisch");
            _vermogenWrapperNode = wrappers.Single(wrapper => wrapper.ItemId == BASE_PATH + "Vermogen");
            _isVoldoendeNode = wrappers.Single(wrapper => wrapper.ItemId == BASE_PATH + "IsVoldoendeVermogen");
            _isWasMachineAanNode = wrappers.Single(wrapper => wrapper.ItemId == BASE_PATH + "IsWasMachineAan");
            _drukknopNode = wrappers.Single(wrapper => wrapper.ItemId == BASE_PATH + "Drukknop");
        }

        private void SubscribeToWrappers()
        {
            _automatischWrapperNode.ValueChanged += _automatischWrapperNode_ValueChanged;
            _vermogenWrapperNode.ValueChanged += _vermogenWrapperNode_ValueChanged;
            _isWasMachineAanNode.ValueChanged += _isWasMachineAanNode_ValueChanged;
            _drukknopNode.ValueChanged += _drukknopNode_ValueChanged;

            _automatischWrapperNode.ListenToUpdates = true;
            _vermogenWrapperNode.ListenToUpdates = true;
            _isWasMachineAanNode.ListenToUpdates = true;
            _drukknopNode.ListenToUpdates = true;
        }

        void _automatischWrapperNode_ValueChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("Auto");
        }

        void _vermogenWrapperNode_ValueChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("GemetenVermogen");
            bool isVoldoende = IsVoldoende;
            IsVoldoende = GemetenVermogen > VermogenDrempel;
            if(isVoldoende!=IsVoldoende)
                OnPropertyChanged("IsVoldoende");
        }

        void _isWasMachineAanNode_ValueChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("MachineDraait");
        }

        void _drukknopNode_ValueChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("Drukknop");
        }

        public ICommand StartStopAction
        {
            get
            {
                return new RelayCommand(StartStop);
            }
        }

        private void StartStop()
        {
            Drukknop = true;
            Drukknop = false;
        }

        void _tmr_Tick(object sender, EventArgs e)
        {
            Points.Add(new Point(DateTime.Now.Ticks, GemetenVermogen));
            if (Points.Count > 20)
            {
                Points.RemoveAt(0);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
