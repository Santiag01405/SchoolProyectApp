using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProyectApp.Models
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string Message { get; set; }
        public required int RoleID { get; set; }  // Agrega esta propiedad para la autentificacion de roles
                                                        //Preguntar a chargpt cuando funcione
    }
}
