using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
using System.Text.Json;

namespace Raggle.Core.Extensions;

public static class IServiceCollectionExtensions
{
    // Only One Singleton
    public static IServiceCollection SetJsonDocumentManager(
        this IServiceCollection services,
        JsonSerializerOptions? jsonOptions = null)
    {
        services.RemoveAll<IDocumentManager>();
        services.AddSingleton<IDocumentManager>(sp =>
        {
            var documentStorage = sp.GetRequiredService<IDocumentStorage>();
            return new JsonDocumentManager(documentStorage, jsonOptions);
        });
        return services;
    }

    // Only One Singleton

}
