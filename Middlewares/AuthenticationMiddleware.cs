using System.Net;
using System.Net.Http.Headers;
using OcelotApiGateway.DataTransferObjects;
using Routes.Helpers;

namespace OcelotApiGateway.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(
        RequestDelegate next, 
        IConfiguration configuration,
        ILogger<AuthenticationMiddleware> logger)
    {
        _logger = logger;
        _next = next;
        _httpClient = new HttpClient();
        _configuration = configuration;
        _logger.LogInformation("Authentication middleware was invoked");
    }
    public async Task InvokeAsync(HttpContext context, IRoutePatternHelper routePatternHelper)
    {
        var protectedRoutes = GetProtectedRoutes();
        var currentPath = context.Request.Path;

        OcelotPath? route = null;
        foreach (var protectedRoute in protectedRoutes)
        {
            var values = routePatternHelper.GetRouteValues(protectedRoute.UpstreamPathTemplate, currentPath);
            var path = routePatternHelper.SetRouteValuesIntoPattern(protectedRoute.UpstreamPathTemplate, values);
            if (path == currentPath)
            {
                route = protectedRoute;
            }
        }
        
        if (route is null || route.Protected == false)
        {
            _logger.LogInformation($"{currentPath} is not protected");
            await _next(context);
            return;
        }
        _logger.LogInformation($"{currentPath} is protected");
        
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authorizationHeader is not null)
        {
            if (AuthenticationHeaderValue.TryParse(authorizationHeader, out var headerValue))
            {
                var accessToken = headerValue.Parameter;
                if (accessToken is not null)
                {
                    var path = _configuration["AuthenticationMicroservice:ValidateTokenPath"];
                    using var request = new HttpRequestMessage(HttpMethod.Post, path);
                    request.Content = JsonContent.Create(accessToken);;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await _httpClient.SendAsync(request);
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var user = await response.Content.ReadFromJsonAsync<ValidatedUser>();
                        if (user is not null && user.UserId != Guid.Empty)
                        {
                            _logger.LogInformation($"Access token is valid, user id: {user.UserId.ToString()}");
                            await _next(context);
                            return;
                        }
                    }
                    _logger.LogInformation($"Access token is not valid");
                }
            }
        }
        _logger.LogError("Authentication header does not exist");
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.CompleteAsync();
    }

    private IEnumerable<OcelotPath> GetProtectedRoutes()
    {
        var routeSection = _configuration.GetSection("Routes").Get<OcelotPath[]>();
        if (routeSection == null)
        {
            throw new InvalidOperationException("Routes section does not exist in IConfiguration");
        }
        return routeSection.Where(x => x.Protected is true);
    }
}