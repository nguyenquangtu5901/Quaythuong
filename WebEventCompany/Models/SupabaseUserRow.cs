using System.Text.Json.Serialization;

namespace WebEventCompany.Models;

public class SupabaseUserRow
{
    public long Id { get; set; }

    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("FULL_NAME")]
    public string? FullName { get; set; }

    [JsonPropertyName("POSITION")]
    public string? Position { get; set; }

    [JsonPropertyName("姓名")]
    public string? ChineseName { get; set; }
}
