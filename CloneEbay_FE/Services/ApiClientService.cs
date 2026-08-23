using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CloneEbay_FE.Models;

namespace CloneEbay_FE.Services
{
    public interface IApiClientService
    {
        Task<ApiResponseModel<T>?> GetAsync<T>(string endpoint, string? bearerToken = null);
        Task<ApiResponseModel<T>?> PostAsync<T>(string endpoint, object? data, string? bearerToken = null);
        Task<ApiResponseModel<T>?> PutAsync<T>(string endpoint, object? data, string? bearerToken = null);
        Task<ApiResponseModel<T>?> DeleteAsync<T>(string endpoint, string? bearerToken = null);
    }

    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClientService(HttpClient httpClient, IConfiguration configuration)
        {
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5180/api";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient = httpClient;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponseModel<T>?> GetAsync<T>(string endpoint, string? bearerToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(content, _jsonOptions);
            }
            catch
            {
                return new ApiResponseModel<T>
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Message = content
                };
            }
        }

        public async Task<ApiResponseModel<T>?> PostAsync<T>(string endpoint, object? data, string? bearerToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(content, _jsonOptions);
            }
            catch
            {
                return new ApiResponseModel<T>
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Message = content
                };
            }
        }

        public async Task<ApiResponseModel<T>?> PutAsync<T>(string endpoint, object? data, string? bearerToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(content, _jsonOptions);
            }
            catch
            {
                return new ApiResponseModel<T>
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Message = content
                };
            }
        }

        public async Task<ApiResponseModel<T>?> DeleteAsync<T>(string endpoint, string? bearerToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            if (!string.IsNullOrEmpty(bearerToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            try { return JsonSerializer.Deserialize<ApiResponseModel<T>>(content, _jsonOptions); }
            catch { return new ApiResponseModel<T> { Success = response.IsSuccessStatusCode, StatusCode = (int)response.StatusCode, Message = content }; }
        }
    }
}
