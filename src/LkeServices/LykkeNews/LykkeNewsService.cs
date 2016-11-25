using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.LykkeNews;
using Core.Settings;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Common;
using Common.Cache;
using Common.Log;

namespace LkeServices.LykkeNews
{
    public class LykkeNewsService : ILykkeNewsService
    {
        private readonly BaseSettings _baseSettings;
        private readonly ILog _log;
        private readonly ICacheManager _cacheManager;
        private const string NewsCacheKey = "lykke_news_cache";

        public LykkeNewsService(BaseSettings baseSettings, ILog log, ICacheManager cacheManager)
        {
            _baseSettings = baseSettings;
            _log = log;
            _cacheManager = cacheManager;
        }

        public async Task<IEnumerable<ILykkeNewsRecord>> GetRecordsCached(int? skip = null, int? take = null)
        {
            var allRecords = await _cacheManager.Get(NewsCacheKey, async () => await GetRecords());

            if (skip != null)
                allRecords = allRecords.Skip(skip.Value);

            if (take != null)
                allRecords = allRecords.Take(take.Value);

            //Text is needed only for first record
            bool firstRecord = true;
            foreach (var record in allRecords)
            {
                record.Text = firstRecord ? RemoveTitleFromText(record.Text, record.Title) : null;
                firstRecord = false;
            }

            return allRecords;
        }

        private async Task<IQueryable<ILykkeNewsRecord>> GetRecords()
        {
            var result = new List<ILykkeNewsRecord>();
            try
            {
                var feedAbstract = await ParseRss(_baseSettings.LykkeNews.RssUrl);
                var feedHtml = await ParseRss(_baseSettings.LykkeNews.RssUrlWithMarkup);

                var itemsHtmlDict = feedHtml.ToDictionary(GetItemUniqueKey);

                foreach (var item in feedAbstract)
                {
                    string imgUrl = null;
                    if (itemsHtmlDict.ContainsKey(GetItemUniqueKey(item)))
                    {
                        imgUrl = Regex.Match(itemsHtmlDict[GetItemUniqueKey(item)].Content,
                            _baseSettings.LykkeNews.RegexToGrabFirstImg).Groups[1].Value;
                        imgUrl = CorrectImgUrl(imgUrl);
                    }

                    result.Add(new LykkeNewsRecordDto
                    {
                        Author = item.Author,
                        DateTime = item.PublishDate,
                        Text = item.Content,
                        Title = item.Title,
                        Url = CorrectUrl(item.Link),
                        ImgUrl = imgUrl
                    });
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("LykkeNewsService", "GetRecordsCached", "", ex);
            }

            return result.AsQueryable();
        }

        private async Task<IList<SyndicationItem>> ParseRss(string url)
        {
            try
            {
                var client = new HttpClient();
                using (var stream = await client.GetStreamAsync(url))
                {
                    var doc = XDocument.Load(stream);
                    // RSS/Channel/item
                    var entries =
                        from item in
                        doc.Root.Descendants()
                            .First(i => i.Name.LocalName == "channel")
                            .Elements()
                            .Where(i => i.Name.LocalName == "item")
                        select new SyndicationItem
                        {
                            Content = item.Elements().First(i => i.Name.LocalName == "description").Value,
                            Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                            PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                            Title = item.Elements().First(i => i.Name.LocalName == "title").Value,
                            Author = item.Elements().First(i => i.Name.LocalName == "author").Value
                        };
                    return entries.ToList();
                }
            }
            catch
            {
                return new List<SyndicationItem>();
            }
        }

        private DateTime ParseDate(string date)
        {
            DateTime result;
            if (DateTime.TryParse(date, out result))
                return result.ToUniversalTime();
            else
                return DateTime.MinValue;
        }

        private string CorrectImgUrl(string imgUrl)
        {
            return imgUrl.Replace("&amp;", "&");
        }

        private string GetItemUniqueKey(SyndicationItem item)
        {
            return $"{item.Title}_{item.PublishDate}";
        }

        private string RemoveTitleFromText(string text, string title)
        {
            var afterTitleNewLines = "\n\n\n\n";
            var afterTitleNewLinesIndex = text.IndexOf(afterTitleNewLines, StringComparison.Ordinal);
            string titleInText = string.Empty;
            if (afterTitleNewLinesIndex > 0)
            {
                titleInText = text.Substring(0, afterTitleNewLinesIndex);
            }
            return afterTitleNewLinesIndex > 0 && titleInText == title ? text.Substring(afterTitleNewLinesIndex + afterTitleNewLines.Length) : text;
        }

        private string CorrectUrl(string url)
        {
            return $"{url}?do=export_xhtml";
        }
        //public Task<IEnumerable<ILykkeNewsRecord>> GetRecordsCached(int? skip = null, int? take = null)
        //      {
        //          throw new NotImplementedException();
        //      }


        private class SyndicationItem
        {
            public string Link { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime PublishDate { get; set; }
            public string Author { get; set; }
        }
    }
}
