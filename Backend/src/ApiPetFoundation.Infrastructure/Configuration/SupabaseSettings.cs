namespace ApiPetFoundation.Infrastructure.Configuration
{
    public class SupabaseSettings
    {
        public string Url { get; set; } = string.Empty;
        public string ServiceRoleKey { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
    }
}
