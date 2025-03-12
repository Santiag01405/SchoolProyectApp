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

                Console.WriteLine($"✅ StudentID encontrado: {student.StudentID}");

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
                var userEnrollments = allEnrollments.Where(e => e.StudentID == student.StudentID).ToList();

                Console.WriteLine($"✅ {userEnrollments.Count} inscripciones encontradas para el usuario {userId} con StudentID {student.StudentID}");

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
                    course.TeacherID
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



    }
}
          
        




