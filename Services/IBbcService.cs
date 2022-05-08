using WhatTimeIsEastEndersOnTonight.Models;

namespace WhatTimeIsEastEndersOnTonight.Services;
public interface IBbcService
{
    Task<EpisodeInfo?> GetEastEndersEpisodeInfoAsync();
}
