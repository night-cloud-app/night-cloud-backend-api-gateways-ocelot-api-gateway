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

        builder.BuildOcelotJson();
        
        builder.Configuration.AddJsonFile(
            builder.Environment.IsDevelopment() ? "ocelot.development.json" : "ocelot.json", optional: false,
            reloadOnChange: true);

        builder.Services.AddOcelot(builder.Configuration);
        builder.Services.AddRoutePatternHelper();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }

    private static void BuildOcelotJson(this WebApplicationBuilder builder)
    {
        var ocelot = File.ReadAllText("ocelot.json");
        var result = ocelot.Replace("{HOST}", builder.Configuration["Ocelot:Host"]);
        File.WriteAllText("ocelot.json", result);
    }
}