using Pinger;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace OsrsPinger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PingTool pinger;
        private long lowestPing = 999;
        private List<int> lowestPingWorldList = new List<int>();
        private Stopwatch time;
        private Thread thread;
        private ObservableCollection<RSWorld> _worlds;
        private Thread contThread;
        private bool contPingWorlds;
        public MainWindow()
        {
            InitializeComponent();

            pinger = new PingTool();
            PingGrid.AllowDrop = false;
            _worlds = new ObservableCollection<RSWorld>();
            PingGrid.ItemsSource = _worlds;
            time = new Stopwatch();
            contPingWorlds = true;


            Run();
        }

        private void Run()
        {
            Reset();
            thread.Start();
        }

        private void Reset()
        {
            contPingWorlds = false;
            if (contThread != null) while (contThread.IsAlive) { }
            if (thread != null && thread.IsAlive) thread.Abort();

            thread = new Thread(PingWorlds);
            contThread = new Thread(ContPingWorlds);

            _worlds.Clear();
            contPingWorlds = true;
            PingGrid.Items.Refresh();
        }

        private void PingWorlds()
        {
            time.Reset();
            time.Start();

            string[] urls = new string[94];
            for (int i = 0; i < 94; i++)
                urls[i] = $"oldschool{i + 1}.runescape.com";

            var pingResults = pinger.PingAsync(urls);
            foreach (KeyValuePair<int, long> pingResult in pingResults)
            {
                long ping = pingResult.Value;
                if (ping != 999 && ping != 0)
                {
                    Dispatcher.Invoke(() => AddRow(pingResult.Key, ping));
                    HandlePingChanges(pingResult.Key, pingResult.Value);
                }

            }
            time.Stop();
            contThread.Start();
            var elapsed = time.Elapsed.Milliseconds.ToString();
            Dispatcher.Invoke(() => StopwatchTb.Text = $"Operation took: {elapsed}ms" );
        }

        private void ContPingWorlds()
        {
            int i = 0;

            while (contPingWorlds)
            {
                Dispatcher.Invoke(SortWorlds);

                switch (i)
                {
                    case (0):
                        for (int j = 0; j < lowestPingWorldList.Count; j++)
                        {
                            var currentWorld = _worlds.First(w => w.World == lowestPingWorldList[i]);
                            currentWorld.Ping = pinger.Ping($"oldschool{lowestPingWorldList[i]}.runescape.com");
                            var beforeCount = lowestPingWorldList.Count;
                            HandlePingChanges(currentWorld.World, currentWorld.Ping);
                            if (lowestPingWorldList.Count < beforeCount) j = 0;
                        }
                        i++;
                        break;

                    case (1):

                        for (int j = 0; j < _worlds.Count; j++)
                        {
                            var ping = _worlds[j].Ping;
                            _worlds[j].Ping = pinger.Ping($"oldschool{_worlds[j].World}.runescape.com");

                            HandlePingChanges(_worlds[j].World, _worlds[j].Ping);
                        }
                        i = 0;
                        break;
                }
            }
        }


        private void HandlePingChanges(int world, long ping)
        {
            if (ping > lowestPing)
            {
                lowestPingWorldList.Remove(world);
                return;
            }

            if (ping == lowestPing)
            {
                if (!lowestPingWorldList.Exists(w => w == world))
                    lowestPingWorldList.Add(world);
                return;
            }

            lowestPing = ping;
            lowestPingWorldList.Clear();
            lowestPingWorldList.Add(world);
        }


        private void SortWorlds()
        {
            var column = PingGrid.Columns[1];

            PingGrid.Items.SortDescriptions.Clear();
            PingGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Ascending));

            foreach (var pingGridColumn in PingGrid.Columns)
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