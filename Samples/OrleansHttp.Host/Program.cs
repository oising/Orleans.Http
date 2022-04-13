using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Http;
using Orleans.Http.ApiExplorer;
using OrleansHttp.Grains;
using OrleansHttp.Host;

var builder = WebApplication.CreateBuilder();
builder.Logging.SetMinimumLevel(LogLevel.Information).AddConsole();
builder.Host
    .UseConsoleLifetime()
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseLocalhostClustering()
            // .Configure<ClusterOptions>(options =>
            // {
            //     options.ClusterId = "dev";
            //     options.ServiceId = "HelloWorldApp";
            // })
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences());
    });

builder.Services
    .AddSingleton<IApiDescriptionGroupCollectionProvider, GrainRouterApi>()
    .TryAddEnumerable(
        ServiceDescriptor.Transient<IApiDescriptionProvider, ArseApiDescriptionProvider>());

builder.Services
    .AddHttpContextAccessor()
    .AddGrainRouter()
    .AddJsonMediaType()
    .AddProtobufMediaType()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo()
        {
            Version = "v1",
            Title = "Orleans.Http Sample API",
            Description = "An automatically generated OpenAPI mapping to grains' APIs"
        });
    });

builder.WebHost
    .UseUrls("http://*:9090");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app
    .UseRouting()
    .UseEndpoints(
        endpoints =>
        {
            endpoints.MapGrains("grains");
            endpoints.MapSwagger();

        })
    .UseRouteGrainProviders(providers =>
    {
        providers.RegisterRouteGrainProvider<RandomGuidRouteGrainProvider>(nameof(RandomGuidRouteGrainProvider));
    });

await app.RunAsync();

// var hostBuilder = new HostBuilder();
//             hostBuilder.UseConsoleLifetime();
//             hostBuilder.ConfigureLogging(logging => logging.AddConsole());
//             hostBuilder.ConfigureWebHostDefaults(webBuilder =>
//             {
//                 webBuilder.UseUrls("http://*:9090");
//                 webBuilder.UseStartup<Startup>();
//                 webBuilder.ConfigureServices(services =>
//                 {
//                     services.AddSwaggerGen();
//                 });
//             });
//
//             hostBuilder.UseOrleans(b =>
//             {
//                 b.UseLocalhostClustering();
//
//                 b.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences());
//             });
//
//             var host = hostBuilder.Build();
//             await host.StartAsync();
//
Console.WriteLine("Press <enter> to exit.");
Console.ReadLine();