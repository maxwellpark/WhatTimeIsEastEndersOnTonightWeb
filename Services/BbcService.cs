using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using WhatTimeIsEastEndersOnTonight.Models;

namespace WhatTimeIsEastEndersOnTonight.Services
{
    public class BbcService : IBbcService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BbcService> _logger;
        private static readonly string _scheduleItemSelector = ".schedule-item";

        public BbcService(IHttpClientFactory httpClientFactory, ILogger<BbcService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        public async Task<EpisodeInfo?> GetEastEndersEpisodeInfoAsync()
        {
            var episodeInfo = await GetEastEndersEpisodeInfoByChannelAsync("bbcone");

            if (episodeInfo?.StartTime == null)
                return episodeInfo;

            // Check BBC 2 in case programme has moved from BBC 1
            episodeInfo = await GetEastEndersEpisodeInfoByChannelAsync("bbctwo");
            return episodeInfo;
        }

        private async Task<EpisodeInfo?> GetEastEndersEpisodeInfoByChannelAsync(string channel)
        {
            var html = await GetScheduleHtmlStringAsync(slug: channel);

            if (string.IsNullOrWhiteSpace(html))
            {
                _logger.LogError("Missing HTML string. Unable to parse.");
                return null;
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var scheduleItems = document.DocumentNode.QuerySelectorAll(_scheduleItemSelector);

            if (scheduleItems == null || !scheduleItems.Any())
            {
                _logger.LogInformation("No Schedule Items found in iplayer HTML. Query selector: " + _scheduleItemSelector);
                return null;
            }

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

            if (episodeInfo.StartTime == null)
                _logger.LogWarning("Could not find EastEnders start time. Either it's not on or the parsing is incorrect.");

            return episodeInfo;
        }

        private async Task<string> GetScheduleHtmlStringAsync(string slug = "")
        {
            var url = GetScheduleUrl(slug);
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }

            _logger.LogError("Unable to get iplayer schedule. Request failed with status code: " + response.StatusCode);
            return string.Empty;
        }

        private static string GetScheduleUrl(string slug = "")
        {
            return "https://www.bbc.co.uk/iplayer/guide/" + slug;
        }
    }
}
