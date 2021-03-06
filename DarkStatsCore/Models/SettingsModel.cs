using System;
using System.ComponentModel.DataAnnotations;

public class SettingsModel
{
    [Required]
    [Url]
    [ValidDarkStatsUrl]
    public string Url { get; set; }
    [Required]
    [Range(1, 300)]
    public int SaveTime { get; set; }
    [Required]
    [Range(100, 5000)]
    public double DashboardRefreshTime { get; set; }
}