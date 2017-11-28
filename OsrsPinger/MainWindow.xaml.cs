using Pinger;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        private Thread thread;
        private ObservableCollection<RSWorld> _worlds;

        public MainWindow()
        {
            InitializeComponent();

            PingGrid.AllowDrop = false;

            pinger = new PingTool();
            _worlds = new ObservableCollection<RSWorld>();

            Run();
        }

        private void Run()
        {
            Reset();

            thread = new Thread(PingWorlds);
            thread.Start();

            Thread checkThread = new Thread(EnableRefreshButton);
            checkThread.Start();
        }

        private void Reset()
        {
            if (thread != null && thread.IsAlive) thread?.Abort();

            RefreshBtn.IsEnabled = false;
            LowestTblk.Text = string.Empty;
            _worlds.Clear();
            PingGrid.Items.Refresh();
        }

        private void EnableRefreshButton()
        {
            while (thread.IsAlive) { Thread.Sleep(100); }
            Dispatcher.Invoke(() => RefreshBtn.IsEnabled = true);
        }

        private void PingWorlds()
        {
            for (int i = 1; i <= 94; i++)
            {
                string host = $"oldschool{i}.runescape.com";
                long ping = pinger.Ping(host);

                if (ping != 999)
                {
                    if (lowestPing == ping)
                    {
                        lowestPingWorldList.Add(i);
                    }

                    if (lowestPing > ping)
                    {
                        lowestPingWorldList.Clear();

                        lowestPing = ping;
                        lowestPingWorldList.Add(i);
                    }

                    Dispatcher.Invoke(() => AddRow(i, ping));
                    Dispatcher.Invoke(() =>
                    {
                        Decorator dec = (Decorator)VisualTreeHelper.GetChild(PingGrid, 0);
                        if (dec != null)
                        {
                            ScrollViewer scrollViewer = (ScrollViewer)dec.Child;
                            if (scrollViewer != null)
                            {
                                scrollViewer.ScrollToEnd();
                            }
                        }
                    });
                }
            }
            SetLowestPingWorlds();
        }

        private void SetLowestPingWorlds()
        {
            bool first = false;
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