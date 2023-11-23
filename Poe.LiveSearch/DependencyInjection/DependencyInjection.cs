using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Api.League;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Persistence;
using Poe.LiveSearch.Services;
using Poe.LiveSearch.Services.RateLimiter;
using Refit;

namespace Poe.LiveSearch.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddApiServices(configuration);
        services.AddWorkers(configuration);

        return services;
    }

    private static IServiceCollection AddWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FoundChannelWorker>();
        services.AddSingleton<LiveSearchChannelWorker>();
        services.AddSingleton<WhisperChannelWorker>();
        services.AddSingleton<Service>();
        services.AddSingleton<ServiceState>();
        
        return services;
    }

    private static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LiteDbOptions>(configuration.GetSection(LiteDbOptions.DefaultSection));
        
        var poeApiOptionsSection =
            configuration.GetSection(PoeApiOptions.DefaultSection);

        var poeApiOptions = poeApiOptionsSection.Get<PoeApiOptions>();

        services.AddOptions<PoeApiOptions>().Configure(opt => poeApiOptionsSection.Bind(opt));
        
        services.AddSingleton<RateLimiterState>();
        services.AddTransient<RateLimiterHttpMessageHandler>();
        services.AddTransient<LoggingHttpMessageHandler>();
        services.AddTransient<SessionHeaderHttpMessageHandler>();
        services.AddScoped<PoeTradeApiService>();
        services.AddScoped<PoeLeagueApiService>();

        services.AddRefitClients<IRefitApi>(client => client.BaseAddress = new Uri(poeApiOptions.BaseInternalApiAddress));

        services.AddRefitClient<IPoeLeagueApi>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(poeApiOptions.BaseExternalApiAddress))
            .AddHttpMessageHandler<LoggingHttpMessageHandler>()
            .AddHttpMessageHandler<SessionHeaderHttpMessageHandler>();

        return services;
    }
    
    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LiteDbOptions>(configuration.GetSection(LiteDbOptions.DefaultSection));
        services.AddSingleton<ILiteDbContext, LiteDbContext>();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<IUserCredentialsRepository, UserCredentialsRepository>();

        return services;
    }
    
    private static IServiceCollection AddRefitClients<T>(this IServiceCollection services,
        Action<HttpClient> configureClient, Assembly assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        var types = assembly.DefinedTypes
            .Where(x => x.IsInterface && x.ImplementedInterfaces.Contains(typeof(T)));

        var refitSetting = new RefitSettings(new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        }));

        foreach (var typeInfo in types)
        {
            services.AddRefitClient(typeInfo, refitSetting)
                .ConfigureHttpClient(configureClient)
                .AddHttpMessageHandler<RateLimiterHttpMessageHandler>()
                .AddHttpMessageHandler<LoggingHttpMessageHandler>()
                .AddHttpMessageHandler<SessionHeaderHttpMessageHandler>();
        }

        return services;
    }
}