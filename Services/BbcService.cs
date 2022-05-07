using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using WhatTimeIsEastEndersOnTonight.Models;
using WhatTimeIsEastEndersOnTonight.Services;

namespace WhatTimeIsEastEndersOn.Services
{
    public class BbcService : IBbcService
    {
        private readonly string _url = "https://www.bbc.co.uk/iplayer/guide";
        private readonly HttpClient _httpClient = new(); // Todo: HttpClientFactory

        public BbcService()
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        public async Task<EpisodeInfo?> GetEastEndersEpisodeInfo()
        {
            var html = await GetScheduleHtmlString();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var scheduleItems = document.DocumentNode.QuerySelectorAll(".schedule-item");

            if (scheduleItems == null || !scheduleItems.Any())
                return null;

            var episodeInfo = new EpisodeInfo();

            foreach (var item in scheduleItems)
            {
                var anchor = item?.QuerySelector("div.gel-layout > div > a");
                var ariaLabel = anchor?.GetAttributes()?.FirstOrDefault(x => x?.Name == "aria-label");

                if (ariaLabel == null || !ariaLabel.Value.ToUpper().Contains("EASTENDERS"))
                    continue;

                var startTime = item.QuerySelector(".schedule-item__start-time");

                if (startTime != null)
                    episodeInfo.StartTime = startTime.InnerText;

                var synopsis = item.QuerySelector("p.list-content-item__synopsis");

                if (synopsis != null)
                    episodeInfo.Synopsis = synopsis.InnerText;
            }
            return episodeInfo;
        }

        private async Task<string> GetScheduleHtmlString()
        {
            var response = await _httpClient.GetAsync(_url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return string.Empty;
        }
    }
}
