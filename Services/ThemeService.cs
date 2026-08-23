using Azure;
using Azure.Data.Tables;
using MetroStart.Entities;
using MetroStart.Themes.Responses;

namespace MetroStart.Services;

public sealed record ThemePage(
    IReadOnlyList<ThemeEntity> Items,
    string? ContinuationToken);

public sealed class ThemeService(
    TableServiceClient tableService,
    ILogger<ThemeService> logger)
{
    private readonly TableClient themes = tableService.GetTableClient("themes");
    private readonly TableClient titles = tableService.GetTableClient("themeTitles");

    public async Task<ThemePage> GetPageAsync(
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        await themes.CreateIfNotExistsAsync(cancellationToken);
        await foreach (var page in themes
            .QueryAsync<ThemeEntity>(cancellationToken: cancellationToken)
            .AsPages(continuationToken, pageSize))
        {
            return new(page.Values, page.ContinuationToken);
        }
        return new([], null);
    }

    public async Task<bool> TryAddAsync(
        SharedTheme theme,
        CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            themes.CreateIfNotExistsAsync(cancellationToken),
            titles.CreateIfNotExistsAsync(cancellationToken));

        if (await ExistingThemeHasTitleAsync(theme.Title, cancellationToken))
        {
            return false;
        }

        var normalizedTitle = theme.Title.Trim().ToUpperInvariant();
        var reservation = new TableEntity("title", normalizedTitle)
        {
            ["Title"] = theme.Title,
        };
        try
        {
            await titles.AddEntityAsync(reservation, cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            return false;
        }

        try
        {
            await themes.AddEntityAsync(
                new ThemeEntity(
                    theme.Author.Trim(),
                    theme.Title.Trim(),
                    true,
                    theme.ThemeContent),
                cancellationToken);
            return true;
        }
        catch
        {
            try
            {
                await titles.DeleteEntityAsync(
                    reservation.PartitionKey,
                    reservation.RowKey,
                    cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Could not roll back title reservation for {Title}",
                    theme.Title);
            }
            throw;
        }
    }

    private async Task<bool> ExistingThemeHasTitleAsync(
        string title,
        CancellationToken cancellationToken)
    {
        await foreach (var _ in themes.QueryAsync<ThemeEntity>(
            entity => entity.RowKey == title,
            maxPerPage: 1,
            cancellationToken: cancellationToken))
        {
            return true;
        }
        return false;
    }
}
