using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Pinger;

namespace OsrsPinger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly PingTool _pinger;
        private long _lowestPing = 999;
        private readonly List<int> _lowestPingWorldList = new List<int>();
        private readonly Stopwatch _time;
        private Thread _thread;
        private readonly ObservableCollection<RSWorld> _worlds;
        private bool _contPingWorlds;

        public MainWindow()
        {
            InitializeComponent();

            _pinger = new PingTool();
            PingGrid.AllowDrop = false;
            _worlds = new ObservableCollection<RSWorld>();
            PingGrid.ItemsSource = _worlds;
            _time = new Stopwatch();
            _contPingWorlds = true;

            Run();
        }

        private void Run()
        {
            Reset();
            _thread.Start();
        }

        private void Reset()
        {
            _contPingWorlds = false;
            if (_thread != null) while (_thread.IsAlive) { }
            _thread = new Thread(PingWorlds);

            _worlds.Clear();
            _contPingWorlds = true;
            PingGrid.Items.Refresh();
        }

        private void PingWorlds()
        {
            _time.Reset();
            _time.Start();

            string[] urls = new string[94];
            for (int i = 0; i < 94; i++)
                urls[i] = $"oldschool{i + 1}.runescape.com";

            Dictionary<int, long> pingResults = _pinger.PingAsync(urls);
            foreach (KeyValuePair<int, long> pingResult in pingResults)
            {
                long ping = pingResult.Value;
                if (ping != 999 && ping != 0)
                {
                    Dispatcher.Invoke(() => AddRow(pingResult.Key, ping));
                    HandlePingChanges(pingResult.Key, pingResult.Value);
                }
            }
            _time.Stop();

            string elapsed = _time.Elapsed.Milliseconds.ToString();
            Dispatcher.Invoke(() => StopwatchTb.Text = $"Operation took: {elapsed}ms");
            ContPingWorlds();
        }

        private void ContPingWorlds()
        {
            int i = 0;

            while (_contPingWorlds)
            {
                Dispatcher.Invoke(SortWorlds);

                switch (i)
                {
                    case (0):
                        for (int j = 0; j < _lowestPingWorldList.Count; j++)
                        {
                            RSWorld currentWorld = _worlds.First(w => w.World == _lowestPingWorldList[i]);
                            currentWorld.Ping = _pinger.Ping($"oldschool{_lowestPingWorldList[i]}.runescape.com");
                            int beforeCount = _lowestPingWorldList.Count;
                            HandlePingChanges(currentWorld.World, currentWorld.Ping);
                            if (_lowestPingWorldList.Count < beforeCount) j = 0;
                        }
                        i++;
                        break;

                    case (1):

                        for (int j = 0; j < _worlds.Count; j++)
                        {
                            _worlds[j].Ping = _pinger.Ping($"oldschool{_worlds[j].World}.runescape.com");
                            HandlePingChanges(_worlds[j].World, _worlds[j].Ping);
                        }
                        i = 0;
                        break;
                }
            }
        }

        private void HandlePingChanges(int world, long ping)
        {
            if (ping > _lowestPing)
            {
                _lowestPingWorldList.Remove(world);
                return;
            }

            if (ping == _lowestPing)
            {
                if (!_lowestPingWorldList.Exists(w => w == world))
                    _lowestPingWorldList.Add(world);
                return;
            }

            _lowestPing = ping;
            _lowestPingWorldList.Clear();
            _lowestPingWorldList.Add(world);
        }

        private void SortWorlds()
        {
            DataGridColumn column = PingGrid.Columns[1];

            PingGrid.Items.SortDescriptions.Clear();
            PingGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Ascending));

            foreach (DataGridColumn pingGridColumn in PingGrid.Columns)
            {
                pingGridColumn.SortDirection = null;
            }
            column.SortDirection = ListSortDirection.Ascending;

            PingGrid.Items.Refresh();
        }

        private void AddRow(int world, long ping)
        {
            _worlds.Add(new RSWorld(world, ping));
            PingGrid.Items.Refresh();
        }
    }

    internal class RSWorld
    {
        public int World { get; set; }
        public long Ping { get; set; }

        public RSWorld(int world, long ping)
        {
            World = world;
            Ping = ping;
        }
    }
}