using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
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
                BaseAddress = new Uri("https://SchoolProject123.somee.com/")
            };
        }

        //  Login de usuario
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
                var url = $"api/users/{userId}";
                Console.WriteLine($"🌍 Llamando a la API: {_httpClient.BaseAddress}{url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error en la API: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // ✅ IMPRIMIR JSON COMPLETO ANTES DE DESERIALIZAR
                Console.WriteLine($"📜 JSON recibido de la API:\n{json}");

                // 🔹 Deserializar el JSON en un objeto User
                var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // ✅ IMPRIMIR CADA PROPIEDAD DEL USER PARA VERIFICAR QUE SE LEYÓ BIEN
                if (user != null)
                {
                    Console.WriteLine($"✔ Usuario cargado correctamente:");
                    Console.WriteLine($"   🔹 UserID: {user.UserID}");
                    Console.WriteLine($"   🔹 Nombre: {user.UserName}");
                    Console.WriteLine($"   🔹 Email: {user.Email}");
                    Console.WriteLine($"   🔹 RoleID: {user.RoleID}");
                }
                else
                {
                    Console.WriteLine("⚠ Error: El JSON recibido no pudo deserializarse correctamente.");
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en GetUserDetailsAsync: {ex.Message}");
                return null;
            }
        }


        //  Obtener lista de usuarios
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

        // Obtener notificaciones
        public async Task<List<Notification>?> GetNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/notifications");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Error al obtener las notificaciones.");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var notifications = JsonSerializer.Deserialize<List<Notification>>(responseJson);

                Console.WriteLine($"✅ {notifications?.Count} notificaciones encontradas.");

                return notifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetNotificationsAsync: {ex.Message}");
                return null;
            }
        }



        // Obtener cursos
       /* public async Task<List<Course>?> GetCoursesAsync()
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
        }*/

        // Obtener calificaciones (grades)
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

        //Incripciones
        public async Task<List<Enrollment>?> GetEnrollmentsAsync(int userId)
        {
            try
            {
                Console.WriteLine($"📡 Buscando inscripciones para userID: {userId}");

                // 🔹 Obtener la lista de todos los estudiantes
                var studentsResponse = await _httpClient.GetAsync("api/students");

                if (!studentsResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Error al obtener la lista de estudiantes.");
                    return null;
                }

                var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
                var students = JsonSerializer.Deserialize<List<Student>>(studentsJson);

                if (students == null || !students.Any())
                {
                    Console.WriteLine("⚠ No se encontraron estudiantes.");
                    return null;
                }

                // 🔹 Buscar el StudentID que pertenece al userID del usuario autenticado
              var student = students.FirstOrDefault(s => s.UserID == userId);

                if (student == null)
                {
                    Console.WriteLine("❌ No se encontró un StudentID asociado a este usuario.");
                    return null;
                }

                Console.WriteLine($"✅ StudentID encontrado: {student.UserID}");

                // 🔹 Obtener todas las inscripciones
                var enrollmentsResponse = await _httpClient.GetAsync("api/enrollments");

                if (!enrollmentsResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Error al obtener inscripciones.");
                    return null;
                }

                var enrollmentsJson = await enrollmentsResponse.Content.ReadAsStringAsync();
                var allEnrollments = JsonSerializer.Deserialize<List<Enrollment>>(enrollmentsJson);

                if (allEnrollments == null || !allEnrollments.Any())
                {
                    Console.WriteLine("⚠ No se encontraron inscripciones.");
                    return null;
                }



                // 🔹 Filtrar inscripciones del usuario autenticado
                var userEnrollments = allEnrollments.Where(e => e.UserID == student.UserID).ToList();

                Console.WriteLine($"✅ {userEnrollments.Count} inscripciones encontradas para el usuario {userId} con StudentID {student.UserID}");

                return userEnrollments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetEnrollmentsAsync: {ex.Message}");
                return null;
            }
        }

        // Obtener un curso por su ID
        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/courses/{courseId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Error al obtener el curso.");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Course>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetCourseByIdAsync: {ex.Message}");
                return null;
            }
        }


        //Actualizar cursos
        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                var json = JsonSerializer.Serialize(new
                {
                    course.CourseID,
                    course.Name,
                    course.Description,
                    course.UserID
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤 Enviando actualización: {json}");

                var response = await _httpClient.PutAsync($"api/courses/{course.CourseID}", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Curso actualizado con éxito.");
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
                Console.WriteLine($"❌ Excepción en UpdateCourseAsync: {ex.Message}");
                return false;
            }
        }


        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };


        //Horario
        public async Task<List<Course>> GetUserSchedule(int userId)
        {
            try
            {
                var url = $"https://SchoolProject123.somee.com/api/schedule/{userId}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Course>>(json, _jsonOptions);
                }
                else
                {
                    Console.WriteLine($"❌ Error al obtener horario: {response.StatusCode}");
                    return new List<Course>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción al obtener horario: {ex.Message}");
                return new List<Course>();
            }
        }
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                // 🔹 Construimos la URL completa combinando BaseAddress con el endpoint
                var fullUrl = new Uri(_httpClient.BaseAddress, endpoint);
                Console.WriteLine($"🌍 Haciendo GET a: {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error en GET {fullUrl}: {response.StatusCode}");
                    return default;
                }

                // 🔹 Leer y deserializar la respuesta JSON
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📜 Respuesta JSON: {json}");

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en GetAsync<{typeof(T).Name}>: {ex.Message}");
                return default;
            }
        }

        public async Task<List<Enrollment>> GetUserWeeklySchedule(int userId)
        {
            try
            {
                var response = await GetAsync<List<Enrollment>>($"api/enrollments/user/{userId}/schedule");

                if (response == null || response.Count == 0)
                {
                    Debug.WriteLine("API no devolvió datos.");
                }
                else
                {
                    Debug.WriteLine($"Datos recibidos: {response.Count} registros.");
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetUserWeeklySchedule: {ex.Message}");
                return null;
            }
        }


        /*public async Task<List<Enrollment>> GetUserWeeklySchedule(int userId)
        {
            return await GetAsync<List<Enrollment>>($"api/enrollments/user/{userId}/schedule");
        }*/

        public async Task<List<Notification>> GetUserNotifications(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<Notification>>($"api/notifications?userID={userId}");
        }

        public async Task<List<AttendanceNotification>> GetAttendanceNotifications(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<AttendanceNotification>>($"api/attendance/parent/{userId}");
        }

        //Busqueda de usuario

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("⚠ No se puede buscar un usuario sin nombre.");
                return new List<User>();
            }

            try
            {
                string encodedQuery = Uri.EscapeDataString(query); // 🔹 Escapa caracteres especiales
                string url = $"api/users/search?query={encodedQuery}";

                Console.WriteLine($"🌍 GET: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error en la API: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"🔍 Detalle del error: {errorContent}");
                    return new List<User>();
                }

                return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error HTTP: {ex.Message}");
                return new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error desconocido: {ex.Message}");
                return new List<User>();
            }
        }

        /*public async Task<List<User>> SearchUsersAsync(string query)
        {
            return await _httpClient.GetFromJsonAsync<List<User>>($"api/users/search?name={query}");
        }*/


        //Envio de notificacion a usuario
        public async Task<bool> SendNotificationAsync(Notification notification)
        {
            var response = await _httpClient.PostAsJsonAsync("api/notifications", notification);
            return response.IsSuccessStatusCode;
        }


        //Metodos para las evaluaciones
        public async Task<List<Evaluation>> GetEvaluationsAsync(int userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Evaluation>>($"api/evaluations?userID={userId}") ?? new List<Evaluation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener evaluaciones: {ex.Message}");
                return new List<Evaluation>();
            }
        }

        public async Task<bool> CreateEvaluationAsync(Evaluation evaluation)
        {
            try
            {
                string url = "api/evaluations";

                var response = await _httpClient.PostAsJsonAsync(url, evaluation);

                string responseContent = await response.Content.ReadAsStringAsync(); // 🔹 Lee el mensaje de error

                Console.WriteLine($"🌍 POST {url} - Código: {response.StatusCode}");
                Console.WriteLine($"📜 Respuesta API: {responseContent}"); // 🔍 Muestra la respuesta completa

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CreateEvaluationAsync: {ex.Message}");
                return false;
            }
        }


        /*public async Task<bool> CreateEvaluationAsync(Evaluation evaluation)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/evaluations", evaluation);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear evaluación: {ex.Message}");
                return false;
            }
        }*/

        public async Task<bool> UpdateEvaluationAsync(Evaluation evaluation)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/evaluations/{evaluation.EvaluationID}", evaluation);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar evaluación: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEvaluationAsync(int evaluationId, int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/evaluations/{evaluationId}?userID={userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al eliminar evaluación: {ex.Message}");
                return false;
            }
        }

        //Cursos
        public async Task<List<Course>> GetCoursesAsync()
        {
            try
            {
                string url = $"api/courses";
                Console.WriteLine($"🌍 GET: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error en la API: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"🔍 Detalle del error: {errorContent}");
                    return new List<Course>();
                }

                return await response.Content.ReadFromJsonAsync<List<Course>>() ?? new List<Course>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error HTTP: {ex.Message}");
                return new List<Course>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error desconocido: {ex.Message}");
                return new List<Course>();
            }
        }

        //Asistencia

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Enviando a {endpoint}: {json}");

                var response = await _httpClient.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"POST {endpoint}: {response.StatusCode} - {error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en PostAsync<{typeof(T).Name}>: {ex.Message}");
                return false;
            }
        }

        /* public async Task<bool> PostAsync<T>(string endpoint, T data)
         {
             try
             {
                 var json = JsonSerializer.Serialize(data);
                 var content = new StringContent(json, Encoding.UTF8, "application/json");

                 var response = await _httpClient.PostAsync(endpoint, content);
                 if (!response.IsSuccessStatusCode)
                 {
                     var error = await response.Content.ReadAsStringAsync();
                     Console.WriteLine($"❌ Error POST {endpoint}: {response.StatusCode} - {error}");
                     return false;
                 }

                 return true;
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"❌ Excepción en PostAsync<{typeof(T).Name}>: {ex.Message}");
                 return false;
             }
         }*/

        // Marcar notificación como leída
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/notifications/{notificationId}/read", null);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Notificación {notificationId} marcada como leída.");
                    return true;
                }

                Console.WriteLine($"❌ Error al marcar como leída: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en MarkNotificationAsReadAsync: {ex.Message}");
                return false;
            }
        }

        // Eliminar notificación
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/notifications/{notificationId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"🗑️ Notificación {notificationId} eliminada correctamente.");
                    return true;
                }

                Console.WriteLine($"❌ Error al eliminar: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en DeleteNotificationAsync: {ex.Message}");
                return false;
            }
        }

        //Eliminar asistencia
        public async Task<bool> DeleteAttendanceAsync(int attendanceId)
        {
            var response = await _httpClient.DeleteAsync($"https://SchoolProject123.somee.com/api/attendance/{attendanceId}");
            return response.IsSuccessStatusCode;
        }


    }
}
          
        




