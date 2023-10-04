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

        builder.ConfigureOcelot();
        
        builder.Services.AddOcelot(builder.Configuration);
        builder.Services.AddRoutePatternHelper();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }

    private static void ConfigureOcelot(this WebApplicationBuilder builder)
    {
        string ocelotPath;
        
        if (builder.Environment.IsProduction())
        {
            ocelotPath = "ocelot.json";
            builder.BuildOcelotJson(ocelotPath);
        }
        else
        {
            ocelotPath = "ocelot.development.json";
        }
        
        builder.Configuration.AddJsonFile(
            ocelotPath, 
            optional: false,
            reloadOnChange: true);
    }

    private static void BuildOcelotJson(this WebApplicationBuilder builder, string ocelotPath)
    {
        var ocelot = File.ReadAllText(ocelotPath);
        var result = ocelot.Replace("{HOST}", builder.Configuration["Ocelot:Host"]);
        File.WriteAllText(ocelotPath, result);
    }
}