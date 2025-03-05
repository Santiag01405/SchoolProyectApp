using System;
using System.Buffers.Text;
using System.Collections.Generic;
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
                BaseAddress = new Uri("https://SchoolProject123.somee.com/") // Cambia por tu URL en producción
            };
        }

        // 🔹 Login de usuario
        public async Task<AuthResponse?> LoginAsync(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);

                // 🔹 Mostrar en consola el código de estado HTTP
                Console.WriteLine($"🔹 Código de respuesta: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Error en la solicitud de login.");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                // 🔹 Mostrar en consola la respuesta JSON
                Console.WriteLine($"🔹 Respuesta del backend: {responseJson}");

                // 🔹 Intentar deserializar la respuesta
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseJson);

                if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
                {
                    Console.WriteLine("❌ El JSON no contiene un token válido.");
                    return null;
                }

                return authResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en LoginAsync: {ex.Message}");
                return null;
            }
        }

        // 🔹 Registro de usuario

        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                var userData = new
                {
                    userName = user.UserName,
                    email = user.Email,
                    passwordHash = user.Password,
                    roleID = user.RoleID
                };

                var json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/register", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔹 Código de respuesta: {response.StatusCode}");
                Console.WriteLine($"🔹 Respuesta del backend: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Registro exitoso.");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Error en la solicitud de registro.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en RegisterAsync: {ex.Message}");
                return false;
            }
        }


        //Actualizacion de usuario
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(new
                {
                    user.UserID,
                    user.UserName,
                    user.Email,
                    PasswordHash = user.Password, // Convertimos Password a PasswordHash
                    user.RoleID // 🔹 Se incluye RoleID en la solicitud
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤 JSON Enviado: {json}");

                var response = await _httpClient.PutAsync($"api/users/{user.UserID}", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Usuario actualizado con éxito.");
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error en la actualización: {response.StatusCode} - {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en UpdateUserAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(responseJson);
        }

        public async Task<User> GetUserDetailsAsync(int userId)
        {
            try
            {
                var url = $"api/users/{userId}"; // 
                Console.WriteLine($"Llamando a la API: {_httpClient.BaseAddress}{url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Respuesta de la API: {json}");

                    return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    Console.WriteLine($"❌ Error en la API: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en GetUserDetailsAsync: {ex.Message}");
                return null;
            }
        }


        //Details

        /*public async Task<User?> GetUserDetailsAsync(string userId)
        {
            Console.WriteLine($"📡 Solicitando datos del usuario con ID: {userId}");

            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✔ Respuesta de la API: {json}");
                return JsonSerializer.Deserialize<User>(json);
            }
            else
            {
                Console.WriteLine($"❌ Error en la API: {response.StatusCode}");
                return null;
            }
        }*/



        /*public async Task<AuthResponse?> RegisterAsync(User user)
        {
            try
            {
                var userData = new
                {
                    userName = user.UserName,
                    email = user.Email,
                    passwordHash = user.Password,
                    roleID = user.RoleID
                };

                var json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/register", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔹 Código de respuesta: {response.StatusCode}");
                Console.WriteLine($"🔹 Respuesta del backend: {responseJson}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Error en la solicitud de registro.");
                    return null;
                }

                // 🔹 Si la respuesta solo contiene un mensaje, lo manejamos aquí
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseJson);
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.Message))
                {
                    Console.WriteLine("✅ Registro exitoso.");
                    return authResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en RegisterAsync: {ex.Message}");
                return null;
            }
        }*/


        // 🔹 Obtener lista de usuarios
        public async Task<List<User>?> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                if (!response.IsSuccessStatusCode) return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetUsersAsync: {ex.Message}");
                return null;
            }
        }

        // 🔹 Obtener notificaciones
        public async Task<List<Notification>?> GetNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/notifications");
                if (!response.IsSuccessStatusCode) return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Notification>>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetNotificationsAsync: {ex.Message}");
                return null;
            }
        }

        // 🔹 Obtener cursos
        public async Task<List<Course>?> GetCoursesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/courses");
                if (!response.IsSuccessStatusCode) return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Course>>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetCoursesAsync: {ex.Message}");
                return null;
            }
        }

        // 🔹 Obtener calificaciones (grades)
        public async Task<List<Grade>?> GetGradesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/grades");
                if (!response.IsSuccessStatusCode) return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Grade>>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetGradesAsync: {ex.Message}");
                return null;
            }
        }
    }
}



/*using System;
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

        public async Task<AuthResponse?> RegisterAsync(User user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/register", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponse>(responseJson);
        }
    }
}*/

