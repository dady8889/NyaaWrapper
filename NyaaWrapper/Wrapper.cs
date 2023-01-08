using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using NyaaWrapper.Extensions;
using NyaaWrapper.Structures;
using NyaaWrapper.Utilities;

namespace NyaaWrapper
{
    public class Wrapper
    {
        private readonly IConfiguration config;

        public Wrapper(IConfiguration conf = null)
        {
            config = conf ?? Configuration.Default.WithDefaultLoader();
        }

        public async Task<List<NyaaTorrentStruct>> GetNyaaEntries(QueryOptions options)
        {
            List<NyaaTorrentStruct> torrents = await GetTorrents("https://nyaa.si", options);
            return torrents;
        }

        public async Task<List<NyaaTorrentStruct>> GetSukebeiEntries(QueryOptions options)
        {
            List<NyaaTorrentStruct> torrents = await GetTorrents("https://sukebei.nyaa.si", options);
            return torrents;
        }

        private async Task<List<NyaaTorrentStruct>> GetTorrents(string url, QueryOptions options)
        {
            options.Search = options.Search.Replace(" ", "+");
            IDocument document = await BrowsingContext.New(config)
                .OpenAsync($"{url}/?f={options.Filter.GetUri()}&c={options.Category.GetUri()}&q={options.Search}");
            List<NyaaTorrentStruct> torrents = new List<NyaaTorrentStruct>();
            IEnumerable<IElement> rows = document.QuerySelectorAll("tbody tr");

            if (options.Amount != 0)
                rows = rows.Take(options.Amount);

            foreach (IElement row in rows)
            {
                int dataCellCounter = 0;
                var block = new List<string>();

                foreach (IElement td in row.Children.Where(m => m.LocalName == "td"))
                {
                    // the cell with name can also contain comments.
                    if (dataCellCounter == 1)
                    {
                        // select only title without comments.
                        var titleLink = td.Children.Where(m => m.LocalName == "a" && !m.GetAttribute("href").Contains("comments")).FirstOrDefault();
                        if (titleLink != null)
                        {
                            block.Add(titleLink.GetAttribute("href"));
                            block.Add(titleLink.TextContent.Replace("\t", "").Replace("\n", ""));
                        }
                    }
                    else // other data cells
                    {
                        // can contain download/torrent links
                        if (td.Children.Count(m => m.LocalName == "a") != 0)
                        {
                            foreach (IElement link in td.Children.Where(m => m.LocalName == "a"))
                            {
                                block.Add(link.GetAttribute("href"));
                            }
                        }

                        // or plain text
                        string temp = td.TextContent.Replace("\t", "").Replace("\n", "");
                        if (temp != "")
                        {
                            block.Add(temp);
                        }
                    }

                    dataCellCounter++;
                }

                torrents.Add(new NyaaTorrentStruct
                {
                    Category = StringUtilities.GetCategory(block[0]),
                    Id = int.Parse(block[1].Substring(6)),
                    Url = url + block[1],
                    Name = block[2],
                    DownloadUrl = url + block[3],
                    Magnet = block[4],
                    Size = block[5],
                    Date = block[6],
                    Seeders = int.Parse(block[7]),
                    Leechers = int.Parse(block[8]),
                    CompletedDownloads = int.Parse(block[9]),
                });
            }

            return torrents;
        }
    }
}