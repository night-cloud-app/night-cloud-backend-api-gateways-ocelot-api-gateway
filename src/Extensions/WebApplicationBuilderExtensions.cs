using Ocelot.DependencyInjection;
using NightCloud.Common.Routes.Helpers;
using Serilog;

namespace OcelotApiGateway.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration)
            => configuration.ReadFrom.Configuration(context.Configuration));

        builder.Configuration.AddJsonFile(
            builder.Environment.IsDevelopment() ? "ocelot.development.json" : "ocelot.json", optional: false,
            reloadOnChange: true);

        builder.Services.AddOcelot(builder.Configuration);
        builder.Services.AddRoutePatternHelper();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }
}