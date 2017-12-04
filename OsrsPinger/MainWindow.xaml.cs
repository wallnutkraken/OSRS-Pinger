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
        public MainWindow()
        {
            InitializeComponent();

            pinger = new PingTool();
            PingGrid.AllowDrop = false;
            _worlds = new ObservableCollection<RSWorld>();
            PingGrid.ItemsSource = _worlds;
            time = new Stopwatch();

            thread = new Thread(PingWorlds);
            contThread = new Thread(ContPingWorlds);

            Run();
        }

        private void Run()
        {
            Reset();
            thread.Start();
        }

        private void Reset()
        {
            if (thread != null && thread.IsAlive) thread.Abort();
            if (contThread != null && contThread.IsAlive) contThread.Abort();

            RefreshBtn.IsEnabled = false;
         
            _worlds.Clear();
            PingGrid.Items.Refresh();
        }

        private void PingWorlds()
        {
            Dispatcher.Invoke(() => RefreshBtn.IsEnabled = false);

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
                }

            }
            time.Stop();
            contThread.Start();
            Dispatcher.Invoke(() => RefreshBtn.IsEnabled = true);
        }

        private void ContPingWorlds()
        {
            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dispatcher.Invoke(SortWorlds);
                    switch (i)
                    {
                        case (0):
                            foreach (var lowPingWorld in lowestPingWorldList)
                            {
                                var currentWorld = _worlds.First(w => w.World == lowPingWorld);
                                currentWorld.Ping = pinger.Ping($"oldschool{lowPingWorld}.runescape.com");

                                HandlePingChanges(currentWorld.World, currentWorld.Ping);
                            }
                            break;

                        case (1):

                            foreach (var world in _worlds)
                            {
                                var ping = world.Ping;
                                world.Ping = pinger.Ping($"oldschool{world.World}.runescape.com");

                                HandlePingChanges(world.World, world.Ping);
                            }
                            i = 0;
                            break;
                    }
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
                if(!lowestPingWorldList.Exists(w => w == world))
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

        private void RefreshBtn_OnClick_(object sender, RoutedEventArgs e)
        {
            Run();
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