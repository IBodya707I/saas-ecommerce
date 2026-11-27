using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.EventBus.Abstractions;
using Platform.EventBus.Publisher;

namespace Platform.EventBus.Extensions;

public static class EventBusExtensions
{
    public static IHostApplicationBuilder AddEventPublisher(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        return builder;
    }
}
