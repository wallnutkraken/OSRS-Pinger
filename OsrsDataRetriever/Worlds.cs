using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace OsrsDataRetriever
{
    public static class Worlds
    {
        private const string worldListUrl = "http://oldschool.runescape.com/slu";

        public static IEnumerable<World> GetWorlds()
        {
            string html;
            using (WebClient client = new WebClient())
            {
                html = client.DownloadString(worldListUrl);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            IList<HtmlNode> worldLinks = doc.QuerySelectorAll(".server-list__world-link");

            return worldLinks.Select(link =>
            {
                int worldNumber = int.Parse(link.Id.Replace("slu-world-", "")) - 300;
                return new World($"oldschool{worldNumber}.runescape.com", worldNumber);
            });
        }
    }
}
