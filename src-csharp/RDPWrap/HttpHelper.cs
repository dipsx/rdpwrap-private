// Copyright 2026 sjackson0109 — Apache License 2.0

namespace RDPWrap.Common;

/// <summary>
/// HTTP download helpers that replace the WinInet-based GitINIFile /
/// DownloadFileToDisk procedures from RDPWInst.dpr.
/// Uses <see cref="HttpClient"/> with a shared static instance.
/// </summary>
public static class HttpHelper
{
    private const int MaxDownloadBytes = 64 * 1024 * 1024;

    // Single shared instance — HttpClient is designed to be reused.
    private static readonly HttpClient _client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 5,
    })
    {
        Timeout = TimeSpan.FromSeconds(60),
        DefaultRequestHeaders = { { "User-Agent", "RDP-Wrapper-Updater/1.0" } }
    };

    /// <summary>
    /// Downloads the text content at <paramref name="url"/> and returns it as
    /// a string. Returns <c>null</c> on any failure.
    /// Mirrors the Delphi GitINIFile function.
    /// </summary>
    public static async Task<string?> DownloadStringAsync(string url)
    {
        try
        {
            using var response = await GetAsync(url).ConfigureAwait(false);
            if (response.Content.Headers.ContentLength is > MaxDownloadBytes)
                throw new InvalidDataException("HTTP response exceeds the maximum allowed size.");

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            if (bytes.Length > MaxDownloadBytes)
                throw new InvalidDataException("HTTP response exceeds the maximum allowed size.");

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[-] HTTP download failed ({url}): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Downloads the binary content at <paramref name="url"/> and saves it to
    /// <paramref name="destPath"/>. Returns <c>true</c> when the file exists
    /// and is non-empty after download.
    /// Mirrors the Delphi DownloadFileToDisk function.
    /// </summary>
    public static async Task<bool> DownloadFileAsync(string url, string destPath)
    {
        string tempPath = destPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using var response = await GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            if (response.Content.Headers.ContentLength is > MaxDownloadBytes)
                throw new InvalidDataException("HTTP response exceeds the maximum allowed size.");

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            if (bytes.Length == 0 || bytes.Length > MaxDownloadBytes)
                throw new InvalidDataException("HTTP response has an invalid size.");

            await File.WriteAllBytesAsync(tempPath, bytes).ConfigureAwait(false);
            File.Move(tempPath, destPath, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[-] HTTP file download failed ({url}): {ex.Message}");
            return false;
        }
        finally
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
        }
    }

    private static async Task<HttpResponseMessage> GetAsync(
        string url,
        HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Only HTTPS download URLs are allowed.");

        var response = await _client.GetAsync(url, completion).ConfigureAwait(false);
        var finalUri = response.RequestMessage?.RequestUri;
        if (finalUri is null || finalUri.Scheme != Uri.UriSchemeHttps)
        {
            response.Dispose();
            throw new InvalidOperationException("HTTPS download redirected to a non-HTTPS URL.");
        }

        response.EnsureSuccessStatusCode();
        return response;
    }

    /// <summary>
    /// Synchronous wrapper for <see cref="DownloadStringAsync"/> — suitable
    /// for the installer's purely-sequential flow.
    /// </summary>
    public static string? DownloadString(string url)
        => DownloadStringAsync(url).GetAwaiter().GetResult();

    /// <summary>
    /// Synchronous wrapper for <see cref="DownloadFileAsync"/>.
    /// </summary>
    public static bool DownloadFile(string url, string destPath)
        => DownloadFileAsync(url, destPath).GetAwaiter().GetResult();
}
