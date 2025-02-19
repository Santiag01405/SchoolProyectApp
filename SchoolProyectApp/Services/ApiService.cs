using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://SchoolProject123.somee.com/") // Cambia por tu URL
            };
        }

        public async Task<AuthResponse?> LoginAsync(User user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponse>(responseJson);
        }
    }
}

