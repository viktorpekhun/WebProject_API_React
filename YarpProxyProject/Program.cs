using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Додаємо підтримку логування
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Вивід логів у консоль

// Налаштування проксі
builder.Services.AddReverseProxy()
    .LoadFromMemory(new[]
    {
        new RouteConfig
        {
            RouteId = "defaultRoute",
            ClusterId = "backendCluster",
            Match = new RouteMatch { Path = "{**catch-all}" }
        }
    },
    new[]
    {
        new ClusterConfig
        {
            ClusterId = "backendCluster",
            LoadBalancingPolicy = "RoundRobin",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "instance1", new DestinationConfig { Address = "https://localhost:5000" } },
                { "instance2", new DestinationConfig { Address = "https://localhost:5001" } },
                { "instance3", new DestinationConfig { Address = "https://localhost:5002" } }
            }
        }
    });

var app = builder.Build();

app.UseRouting();
app.MapReverseProxy();

app.Run();
