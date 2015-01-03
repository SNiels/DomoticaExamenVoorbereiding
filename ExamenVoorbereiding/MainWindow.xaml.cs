using OPCLib.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace ExamenVoorbereiding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string BASE_PATH = "PLC.";
        private OPCServerWrapper _server;
        private OPCNodeWrapper _automatischWrapperNode;
        private OPCNodeWrapper _vermogenWrapperNode;
        private OPCNodeWrapper _isVoldoendeNode;
        private OPCNodeWrapper _isWasMachineAanNode;
        private OPCNodeWrapper _drukknopNode;
        private DispatcherTimer _tmr;
        public MainWindow()
        {
            InitializeComponent();
            Init();
            this.Loaded += MainWindow_Loaded;
        }

        private void Init()
        {
            GetWrappers();
            btnStartStop.MouseUp += btnStartStop_MouseUp;
            _tmr = new DispatcherTimer();
            _tmr.Interval = new TimeSpan(500);
            _tmr.Tick += _tmr_Tick;
        }

        private void GetWrappers()
        {
            _server = OPCServerWrapper.GetOPCServerWrapper("Kepware.KEPServerEX.V5");
            OPCServerWrapper.SelectedServer = _server;
            var wrappers = _server.GetOPCNodeWrappers();
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
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeToWrappers();
            _tmr.Start();
        }

        void _automatischWrapperNode_ValueChanged(object sender, EventArgs e)
        {
            chkAuto.IsOn = (bool) _automatischWrapperNode.Value;
        }

        void _vermogenWrapperNode_ValueChanged(object sender, EventArgs e)
        {
            double vermogen = (double) _vermogenWrapperNode.Value;
            txtGemetenVermogen.Text = vermogen.ToString();
            _isVoldoendeNode.Value = vermogen > txtVermogenDrempel.Value;
        }

        void _isWasMachineAanNode_ValueChanged(object sender, EventArgs e)
        {
            chkMachineDraait.IsOn = (bool) _isWasMachineAanNode.Value;
        }

        void btnStartStop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
            _drukknopNode.Value = true;
        }

        void _tmr_Tick(object sender, EventArgs e)
        {
            line.Points.Add(new Point(DateTime.Now.Second,(double)_vermogenWrapperNode.Value));
            if (line.Points.Count > 20)
            {
                line.Points.RemoveAt(0);
            }
        }
    }
}
