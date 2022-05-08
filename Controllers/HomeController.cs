using Microsoft.AspNetCore.Mvc;
using WhatTimeIsEastEndersOnTonight.Services;

namespace WhatTimeIsEastEndersOnTonight.Controllers;
public class HomeController : Controller
{
    private readonly IBbcService _bbcService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IBbcService bbcService, ILogger<HomeController> logger)
    {
        _bbcService = bbcService ?? throw new ArgumentNullException(nameof(bbcService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Request received at Home Page.");

        var episodeInfo = await _bbcService.GetEastEndersEpisodeInfoAsync();
        return View(episodeInfo);
    }
}
