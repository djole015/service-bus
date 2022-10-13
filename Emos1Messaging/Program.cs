using Emos1Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) => 
        services.AddHostedService<Worker>()
    )
    .Build()
    .Run();

