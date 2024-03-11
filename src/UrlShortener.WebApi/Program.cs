using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UrlShortener.Core.Domain;
using UrlShortener.Core.Messages;
using UrlShortener.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(x => x.AddConsole());
builder.Services.AddOptions<ShortenerOptions>()
.Configure<IConfiguration>((o, c) => {
    c.Bind(nameof(ShortenerOptions), o);
});

// add code that will only allow local and domain specific.
var  allowAllOrigins = "_allowAllOrigins";

// add cors configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowAllOrigins,
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// register the helper
builder.Services.AddTransient(sp =>{
    var options = sp.GetRequiredService<IOptions<ShortenerOptions>>().Value;

    return new StorageTableHelper(options.DataStorage!);
});

var app = builder.Build();
app.UseCors(allowAllOrigins);

app.MapGet("/", () => "Minimal Api");

app.MapPost("/api/UrlArchive", async (HttpRequest req) =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("UrlArchive");
    var options = app.Services.GetRequiredService<IOptions<ShortenerOptions>>().Value;
    var storage = app.Services.GetRequiredService<StorageTableHelper>();

    logger.LogInformation("HTTP trigger - UrlArchive");

    ShortUrlEntity input;
    ShortUrlEntity result;
    try
    {
        // Validation of the inputs
        if (req == null)
        {
            return Results.NotFound();
        }

        using (var reader = new StreamReader(req.Body))
        {
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            input = JsonSerializer.Deserialize<ShortUrlEntity>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (input == null)
            {
                return Results.NotFound();
            }
        }

        result = await storage.ArchiveShortUrlEntity(input).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error was encountered.");
        return Results.BadRequest(new { ex.Message });
    }

    return Results.Ok(result);
});

app.MapPost("/api/UrlClickStatsByDay", async (HttpRequest req) =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("UrlClickStatsByDay");
    var options = app.Services.GetRequiredService<IOptions<ShortenerOptions>>().Value;
    var storage = app.Services.GetRequiredService<StorageTableHelper>();

    logger.LogInformation("HTTP trigger: UrlClickStatsByDay");

    UrlClickStatsRequest input;
    var result = new ClickDateList();

    // Validation of the inputs
    if (req == null)
    {
        return Results.NotFound();
    }

    try
    {
        using (var reader = new StreamReader(req.Body))
        {
            var strBody = await reader.ReadToEndAsync().ConfigureAwait(false);
            input = JsonSerializer.Deserialize<UrlClickStatsRequest>(strBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (input == null)
            {
                return Results.NotFound();
            }
        }

        var rawStats = await storage.GetAllStatsByVanity(input.Vanity).ConfigureAwait(false);

        result.Items = rawStats.GroupBy(s => DateTime.Parse(s.Datetime).Date)
                                .Select(stat => new ClickDate
                                {
                                    DateClicked = stat.Key.ToString("yyyy-MM-dd"),
                                    Count = stat.Count()
                                }).OrderBy(s => DateTime.Parse(s.DateClicked).Date).ToList<ClickDate>();

        var host = string.IsNullOrEmpty(options.CustomDomain) ? req.Host.ToString() : options.CustomDomain.ToString();
        result.Url = Utility.GetShortUrl(host, input.Vanity);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error was encountered.");
        return Results.BadRequest(new { Message = $"{ex.Message}" });
    }

    return Results.Ok(result);
});

app.MapPost("/api/UrlCreate", async (HttpRequest req) =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("UrlCreate");
    var options = app.Services.GetRequiredService<IOptions<ShortenerOptions>>().Value;
    var storage = app.Services.GetRequiredService<StorageTableHelper>();

    logger.LogInformation($"Creating shortURL: {req}");
    string userId = string.Empty;
    ShortRequest input;
    var result = new ShortResponse();

    try
    {
        // Validation of the inputs
        if (req == null)
        {
            return Results.NotFound();
        }

        using (var reader = new StreamReader(req.Body))
        {
            var strBody = await reader.ReadToEndAsync().ConfigureAwait(false);
            input = JsonSerializer.Deserialize<ShortRequest>(strBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (input == null)
            {
                return Results.NotFound();
            }
        }

        // If the Url parameter only contains whitespaces or is empty return with BadRequest.
        if (string.IsNullOrWhiteSpace(input.Url))
        {
            return Results.BadRequest(new { Message = "The url parameter can not be empty." });
        }

        // Validates if input.url is a valid aboslute url, aka is a complete refrence to the resource, ex: http(s)://google.com
        if (!Uri.IsWellFormedUriString(input.Url, UriKind.Absolute))
        {
            return Results.BadRequest(new { Message = $"{input.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'." });
        }

        string longUrl = input.Url.Trim();
        string vanity = string.IsNullOrWhiteSpace(input.Vanity) ? "" : input.Vanity.Trim();
        string title = string.IsNullOrWhiteSpace(input.Title) ? "" : input.Title.Trim();

        ShortUrlEntity newRow;

        if (!string.IsNullOrEmpty(vanity))
        {
            newRow = new ShortUrlEntity(longUrl, vanity, title, input.Schedules);
            if (await storage.IfShortUrlEntityExist(newRow).ConfigureAwait(false))
            {
                return Results.Conflict(new { Message = "This Short URL already exist." });
            }
        }
        else
        {
            newRow = new ShortUrlEntity(longUrl, await Utility.GetValidEndUrl(vanity, storage).ConfigureAwait(false), title, input.Schedules);
        }

        await storage.SaveShortUrlEntity(newRow).ConfigureAwait(false);

        var host = string.IsNullOrEmpty(options.CustomDomain) ? req.Host.ToString() : options.CustomDomain.ToString();
        result = new ShortResponse(host, newRow.Url, newRow.RowKey, newRow.Title);

        logger.LogInformation("Short Url created.");

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error was encountered.");
        return Results.BadRequest(new { ex.Message });
    }
});

app.MapGet("/api/UrlList", async () =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("UrlList");
    var options = app.Services.GetRequiredService<IOptions<ShortenerOptions>>().Value;
    var storage = app.Services.GetRequiredService<StorageTableHelper>();

    logger.LogInformation($"Starting UrlList...");

    var result = new ListResponse();
    string userId = string.Empty;

    try
    {
        result.UrlList = await storage.GetAllShortUrlEntities().ConfigureAwait(false);
        result.UrlList = result.UrlList.Where(p => !(p.IsArchived ?? false)).ToList();

        var host = string.IsNullOrEmpty(options.CustomDomain) ? app.Environment.ApplicationName : options.CustomDomain;
        foreach (ShortUrlEntity url in result.UrlList)
        {
            url.ShortUrl = Utility.GetShortUrl(host, url.RowKey);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error was encountered.");
        return Results.BadRequest(new { ex.Message });
    }

    return Results.Ok(result);
});

app.MapGet("/{shortUrl}", async (string shortUrl, HttpContext context) =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("redirect");
    var options = app.Services.GetRequiredService<IOptions<ShortenerOptions>>().Value;
    var storage = app.Services.GetRequiredService<StorageTableHelper>();

    string redirectUrl = "https://azure.com";

    if (!string.IsNullOrWhiteSpace(shortUrl))
    {
        redirectUrl = options.DefaultRedirectUrl ?? redirectUrl;

        var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);
        var newUrl = await storage.GetShortUrlEntity(tempUrl).ConfigureAwait(false);

        if (newUrl != null)
        {
            logger.LogInformation($"Found it: {newUrl.Url}");
            newUrl.Clicks++;
            await storage.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey)).ConfigureAwait(false);
            await storage.SaveShortUrlEntity(newUrl).ConfigureAwait(false);
            redirectUrl = WebUtility.UrlDecode(newUrl.ActiveUrl);
        }
    }
    else
    {
        logger.LogInformation("Bad Link, resorting to fallback.");
    }

    context.Response.Redirect(redirectUrl);
});

app.MapPost("/api/UrlUpdate", async (HttpRequest req) =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("UrlUpdate");
    var options = app.Services.GetRequiredService<IOptions<ShortenerOptions>>().Value;
    var storage = app.Services.GetRequiredService<StorageTableHelper>();

    logger.LogInformation($"HTTP trigger - UrlUpdate");

    string userId = string.Empty;
    ShortUrlEntity input;
    ShortUrlEntity result;

    try
    {
        // Validation of the inputs
        if (req == null)
        {
            return Results.NotFound();
        }

        using (var reader = new StreamReader(req.Body))
        {
            var strBody = await reader.ReadToEndAsync().ConfigureAwait(false);
            input = JsonSerializer.Deserialize<ShortUrlEntity>(strBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (input == null)
            {
                return Results.NotFound();
            }
        }

        // If the Url parameter only contains whitespaces or is empty return with BadRequest.
        if (string.IsNullOrWhiteSpace(input.Url))
        {
            return Results.BadRequest(new { Message = "The url parameter can not be empty." });
        }

        // Validates if input.url is a valid absolute url, aka is a complete reference to the resource, ex: http(s)://google.com
        if (!Uri.IsWellFormedUriString(input.Url, UriKind.Absolute))
        {
            return Results.BadRequest(new { Message = $"{input.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'." });
        }

        result = await storage.UpdateShortUrlEntity(input).ConfigureAwait(false);
        var host = string.IsNullOrEmpty(options.CustomDomain) ? req.Host.ToString() : options.CustomDomain.ToString();
        result.ShortUrl = Utility.GetShortUrl(host, result.RowKey);

    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error was encountered.");
        return Results.BadRequest(new { ex.Message });
    }

    return Results.Ok(result);
});

app.Run();
