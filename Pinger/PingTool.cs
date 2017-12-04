using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Pinger
{
    public class PingTool
    {
        private Ping[] _pingPool;
        private int _poolSize;

        public PingTool() : this(5)
        {
        }

        public PingTool(int poolSize)
        {
            _pingPool = new Ping[poolSize];
            for (int index = 0; index < poolSize; index++)
                _pingPool[index] = new Ping();
            _poolSize = poolSize;
        }

        public long Ping(string uri)
        {
            Ping ping = new Ping();
            try
            {
                return ping.Send(uri).RoundtripTime;
            }
            catch
            {
                return 999;
            }
        }

        public Dictionary<int, long> PingAsync(IEnumerable<string> urls)
        {
            int total = urls.Count();
            IEnumerator<string> urlEnumerator = urls.GetEnumerator();
            Dictionary<int, long> pings = new Dictionary<int, long>();

            //urlEnumerator.Reset();
            for (int index = 0; index < total; index += _poolSize)
            {
                int pingCount = _poolSize;
                if (index + _poolSize > total)
                {
                    /* Final array */
                    pingCount = total - index - 1;
                }

                Task<PingReply>[] pingTasks = new Task<PingReply>[pingCount];
                string[] currentBatchUrls = new string[pingCount];

                for (int pingIndex = 0; pingIndex < pingCount; pingIndex++)
                {
                    urlEnumerator.MoveNext();
                    pingTasks[pingIndex] = _pingPool[pingIndex].SendPingAsync(urlEnumerator.Current);
                    currentBatchUrls[pingIndex] = urlEnumerator.Current;
                }

                /* Wait for all pings to complete */
                for (int pingIndex = 0; pingIndex < pingCount; pingIndex++)
                {
                    long pingTime;
                    try
                    {
                        pingTasks[pingIndex].Wait();
                        pingTime = pingTasks[pingIndex].Result.RoundtripTime;
                    }
                    catch
                    {
                        /* Cannot get world, assume max ping */
                        pingTime = 999;
                    }
                    pings.Add(index + pingIndex + 1, pingTime);
                }
            }

            return pings;
        }
    }
}