using Pinger;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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

        public MainWindow()
        {
            InitializeComponent();

            PingGrid.AllowDrop = false;

            pinger = new PingTool();
            _worlds = new ObservableCollection<RSWorld>();
            time = new Stopwatch();

            Run();
        }

        private void Run()
        {
            Reset();

            thread = new Thread(PingWorlds);
            thread.Start();
        }

        private void Reset()
        {
            if (thread != null && thread.IsAlive) thread?.Abort();

            RefreshBtn.IsEnabled = false;
            LowestTblk.Text = string.Empty;
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
                if (ping != 999)
                {
                    if (lowestPing == ping)
                        lowestPingWorldList.Add(pingResult.Key);
                    if (lowestPing > ping)
                    {
                        lowestPingWorldList.Clear();

                        lowestPing = ping;
                        lowestPingWorldList.Add(pingResult.Key);
                    }
                }

                Dispatcher.Invoke(() => AddRow(pingResult.Key, ping));
            }
            time.Stop();
            SetLowestPingWorlds();
            Dispatcher.Invoke(() => RefreshBtn.IsEnabled = true);
        }

        private void SetLowestPingWorlds()
        {
            bool first = false;
            Dispatcher.Invoke(() => LowestTblk.Text += $"Operation took {time.Elapsed}\n");
            foreach (var world in lowestPingWorldList)
            {
                if (!first)
                {
                    Dispatcher.Invoke(() => LowestTblk.Text += $"Lowest Ping:\nWorld {world}, ");
                    first = true;
                    continue;
                }

                Dispatcher.Invoke(() => LowestTblk.Text += $"{world}, ");
            }
            Dispatcher.Invoke(() => LowestTblk.Text = LowestTblk.Text.Remove(LowestTblk.Text.Length - 2));
            Dispatcher.Invoke(() => LowestTblk.Text += $": {lowestPing}ms");
            Dispatcher.Invoke(SortWorlds);
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
            PingGrid.ItemsSource = _worlds;
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