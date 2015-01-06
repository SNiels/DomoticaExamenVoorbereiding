using Newtonsoft.Json;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string BASE_URI = @"http://localhost:9000/";
        private const string BASE_PATH = "examenvoorbereding.PLC.";
        private IEnumerable<WCFNode> _nodes;
        private WCFNode _isAanNode;
        private WCFNode _vermogenNode;
        private DispatcherTimer _timer;
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _nodes = await GetWCFNodes(new Uri(BASE_URI+"GetWCFNodes"));
            _vermogenNode = _nodes.SingleOrDefault(node => node.ItemId == BASE_PATH + "Vermogen");
            _isAanNode = _nodes.SingleOrDefault(node => node.ItemId == BASE_PATH + "IsWasMachineAan");
            await GetData();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick+= (ticksender,eventParams)=>{
                GetData();
            };
            _timer.Start();
        }

        private async Task GetData()
        {
            txtVermogen.Text = "Het voltage is " + await GetWCFValue(_vermogenNode.ItemId) + "V.";
            bool isAan = bool.Parse((await GetWCFValue(_isAanNode.ItemId)));
            txtIsAan.Text = "De machine is " + (isAan ? "aan" : "uit") + ".";
        }


        public static async Task<IEnumerable<WCFNode>> GetWCFNodes(Uri uri)
        {
            HttpClient client = new HttpClient();
            String response = await client.GetStringAsync(uri);
            return ParseJSONToNodes(response);
        }

        public static IEnumerable<WCFNode> ParseJSONToNodes(String response)
        {
            IEnumerable<WCFNode> nodes;
            nodes = JsonConvert.DeserializeObject<IEnumerable<WCFNode>>(response);
            return nodes;
        }

        public static async Task<string> GetWCFValue(string itemId)
        {
            HttpClient client = new HttpClient();
            int unixTimestamp = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return await client.GetStringAsync(new Uri(BASE_URI + @"GetWCFNodeValue/" + itemId+"?time="+unixTimestamp));
        }
    }
}
