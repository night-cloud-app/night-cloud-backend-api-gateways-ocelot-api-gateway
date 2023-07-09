using Ocelot.Middleware;
using OcelotApiGateway.Middlewares;
using Serilog;

namespace OcelotApiGateway.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseMiddleware<AuthenticationMiddleware>();

        app.UseOcelot().Wait();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();

        return app;
    }
}