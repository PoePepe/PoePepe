﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.LiveSearch.Api.League;
using Serilog;
using Skreet2k.Common.Models;

namespace PoePepe.UI.Services;

public class LeagueService
{
    public bool IsLoaded { get; private set; }
    public EventHandler LeagueNamesLoaded;
    public EventHandler LeagueNamesLoadFailed;
    public string[] ActualLeagueNames { get; private set; } = { "Standard", "Hardcore", "Ruthless", "Hardcore Ruthless" };

    public async Task LoadActualLeagueNamesAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetLeaguesAsync(cancellationToken);
        if (!response.IsSuccess)
        {
            Log.Error("Failed to load actual league names. {ErrorMessage}", response.ErrorMessage);

            LeagueNamesLoadFailed?.Invoke(this, EventArgs.Empty);

            return;
        }

        ActualLeagueNames = response.Content.ToArray();

        LeagueNamesLoaded?.Invoke(this, EventArgs.Empty);

        IsLoaded = true;
    }

    /// <summary>
    /// Получить список актуальных лиг.
    /// </summary>
    private async Task<ResultList<string>> GetLeaguesAsync(CancellationToken token = default)
    {
        var defaultLeagues = new[] {"Standard", "Hardcore", "Solo Self-Found", "Hardcore SSF", "Ruthless", "Hardcore Ruthless", "SSF Ruthless", "Hardcore SSF Ruthless"};
        
        var client = App.Current.Services.GetRequiredService<PoeLeagueApiService>();
        
        var rawLeagueResult = await client.GetLeaguesAsync(token);

        if (!rawLeagueResult.IsSuccess)
        {
            return new ResultList<string>
            {
                ErrorMessage = rawLeagueResult.ErrorMessage,
                ReturnCode = rawLeagueResult.ReturnCode
            };
        }
        var result = new List<string>(10);
        foreach (var league in rawLeagueResult.Content.Leagues)
        {
            if (defaultLeagues.Contains(league.Name))
            {
                continue;
            }
            
            if (Regex.IsMatch(league.Name, "SSF"))
            {
                continue;
            }
            
            result.Add(league.Name);
        }
        
        result.Add("Standard");
        result.Add("Hardcore");
        result.Add("Ruthless");
        result.Add("Hardcore Ruthless");

        return new ResultList<string>(result);
    }
}