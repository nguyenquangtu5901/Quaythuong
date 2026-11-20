namespace WebEventCompany.Options;

public class SupabaseOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Anon public key hoặc service_role key (chỉ dùng ở server).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    public string UserTable { get; set; } = "user";
}
