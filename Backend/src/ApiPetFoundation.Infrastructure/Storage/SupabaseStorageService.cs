using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ApiPetFoundation.Infrastructure.Storage
{
    public class SupabaseStorageService : IStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseSettings _settings;

        public SupabaseStorageService(HttpClient httpClient, IOptions<SupabaseSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> UploadPetImageAsync(int petId, string fileName, Stream content, string contentType)
        {
            if (string.IsNullOrWhiteSpace(_settings.Url))
                throw new InvalidOperationException("Supabase Url is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.ServiceRoleKey))
                throw new InvalidOperationException("Supabase ServiceRoleKey is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.Bucket))
                throw new InvalidOperationException("Supabase Bucket is not configured.");

            var safeFileName = Path.GetFileName(fileName);
            var path = $"pets/{petId}/{Guid.NewGuid():N}_{safeFileName}";
            var requestUri = $"{_settings.Url}/storage/v1/object/{_settings.Bucket}/{path}?upsert=true";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ServiceRoleKey);
            request.Headers.Add("apikey", _settings.ServiceRoleKey);
            request.Content = new StreamContent(content);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Supabase upload failed: {response.StatusCode} {body}");
            }

            return $"{_settings.Url}/storage/v1/object/public/{_settings.Bucket}/{path}";
        }
    }
}
