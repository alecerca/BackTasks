using BackTasks.Controllers;
using BackTasks.Repositorios.IRepositorio;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackTasks.Models
{
    public class Jwt
    {
        private readonly IUsuario _userRepo;
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Audience { get; set; }

        public async Task<ActionResult<dynamic>> ValidarToken(ClaimsIdentity identity)
        {
            try
            {
                if(identity.Claims.Count() == 0)
                {
                    return new
                    {
                        success = false,
                        message = "Verificar si estas enviando el token valido",
                        result = ""
                    };
                }

                var id = identity.Claims.FirstOrDefault(v => v.Type == "id").Value;

                var user = await _userRepo.Obtener(v => v.Id.ToString() == id);

                return new
                {
                    success = true,
                    message = "exito",
                    result = user
                };

            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    result = ""
                };
            }
        }
    }
}
