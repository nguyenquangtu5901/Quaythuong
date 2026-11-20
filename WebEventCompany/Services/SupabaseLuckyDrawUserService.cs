using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebEventCompany.Models;
using WebEventCompany.Options;

namespace WebEventCompany.Services;

public class SupabaseLuckyDrawUserService : ILuckyDrawUserService
{
    private const int PageSize = 1000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<SupabaseOptions> _options;
    private readonly IConfiguration _configuration;

    public SupabaseLuckyDrawUserService(
        HttpClient httpClient,
        IOptionsMonitor<SupabaseOptions> options,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _options = options;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<LuckyDrawParticipant>> GetParticipantsAsync(CancellationToken cancellationToken = default)
    {
        var url = GetUrl();
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình Supabase. Hãy điền Url và ApiKey trong appsettings.json rồi Stop/Run lại app.");
        }

        var participants = new List<LuckyDrawParticipant>();
        var offset = 0;

        while (true)
        {
            var page = await FetchPageAsync(url, apiKey, offset, cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            foreach (var row in page)
            {
                var code = (row.Code ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                participants.Add(new LuckyDrawParticipant
                {
                    Code = code.ToUpperInvariant(),
                    Name = string.IsNullOrWhiteSpace(row.FullName) ? (row.ChineseName ?? "Khách mời") : row.FullName.Trim(),
                    Dept = string.IsNullOrWhiteSpace(row.Position) ? "Chưa có bộ phận" : row.Position.Trim()
                });
            }

            if (page.Count < PageSize)
            {
                break;
            }

            offset += PageSize;
        }

        return participants
            .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    private string GetUrl()
    {
        return FirstNonEmpty(_options.CurrentValue.Url, _configuration["Supabase:Url"]);
    }

    private string GetApiKey()
    {
        return FirstNonEmpty(_options.CurrentValue.ApiKey, _configuration["Supabase:ApiKey"]);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;
    }

    private async Task<List<SupabaseUserRow>> FetchPageAsync(string url, string apiKey, int offset, CancellationToken cancellationToken)
    {
        var table = Uri.EscapeDataString(_options.CurrentValue.UserTable);
        var requestUri = $"{url.TrimEnd('/')}/rest/v1/{table}?select=id,Code,FULL_NAME,POSITION,%E5%A7%93%E5%90%8D&order=id.asc";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("apikey", apiKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.TryAddWithoutValidation("Range-Unit", "items");
        request.Headers.TryAddWithoutValidation("Range", $"{offset}-{offset + PageSize - 1}");
        request.Headers.TryAddWithoutValidation("Prefer", "count=exact");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Không đọc được bảng {_options.CurrentValue.UserTable} từ Supabase ({(int)response.StatusCode}): {body}");
        }

        return JsonSerializer.Deserialize<List<SupabaseUserRow>>(body, JsonOptions) ?? [];
    }
}
