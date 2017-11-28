using System.Net.NetworkInformation;

namespace Pinger
{
    public class PingTool
    {
        public long Ping(string uri)
        {
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send(uri);
                return reply.RoundtripTime;
            }
            catch
            {
                return 999;
            }
        }
    }
}