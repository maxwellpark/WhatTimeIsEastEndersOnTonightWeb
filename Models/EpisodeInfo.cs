namespace WhatTimeIsEastEndersOnTonight.Models
{
    public class EpisodeInfo
    {
        public string? StartTime { get; set; }
        public string? Synopsis { get; set; }

        public EpisodeInfo()
        {

        }

        public EpisodeInfo(string startTime, string synopsis)
        {
            StartTime = startTime;
            Synopsis = synopsis;
        }
    }
}
