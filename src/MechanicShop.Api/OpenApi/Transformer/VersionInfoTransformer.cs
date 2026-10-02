using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace mechanicShop.Api.OpenApi.Transformer;

internal sealed class VersionInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var version = context.DocumentName;

        document.Info.Version = version;
        document.Info.Title = $"MechanicShop API {version}";

        return Task.CompletedTask;
    }
}