﻿using AutoMapper;
using BackTasks.Models;
using BackTasks.Models.Dto;
using BackTasks.Repositorios.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BackTasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _userRepo;
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public UsuarioController(IUsuario userRepo, IMapper mapper, IConfiguration configuration)
        {
            _configuration = configuration;
            _mapper = mapper;
            _userRepo = userRepo;
            _response = new();
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetUsuarios()
        {
            try
            {
                IEnumerable<Usuario> userList = await _userRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<UsuarioDto>>(userList);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isExitoso = false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetUser(int id)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (identity.Claims.Count() == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso = false;
                    return BadRequest(_response);
                }

                var num = identity.Claims.FirstOrDefault(v => v.Type == "id").Value;

                var usuario = await _userRepo.Obtener(v => v.Id.ToString() == num);

                if (usuario == null)
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.isExitoso = false;
                    return Unauthorized(_response);
                }
                
                if (id <= 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso =false;
                    return BadRequest(_response);
                }

                var user = await _userRepo.Obtener(v => v.Id == id);

                if(user == null)
                {
                    _response.isExitoso = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<UsuarioDto>(user);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.isExitoso = false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> SignUp([FromBody] UsuarioDtoCreate createUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if(await _userRepo.Obtener(v => v.Nombre.ToLower() == createUser.Nombre.ToLower() || v.Email.ToLower() == createUser.Email.ToLower()) != null)
                {
                    ModelState.AddModelError("NombreExiste", "El usuario ya existe");
                    _response.isExitoso = false;
                    _response.StatusCode=HttpStatusCode.InternalServerError;
                    return BadRequest(ModelState);
                }

                if(createUser == null)
                {
                    return BadRequest(createUser);
                }

                Usuario modelo = _mapper.Map<Usuario>(createUser);


                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;

                await _userRepo.Crear(modelo);

                var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("id", modelo.Id.ToString()),
                new Claim("nombre", modelo.Nombre)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
                var signin = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    jwt.Issuer,
                    jwt.Audience,
                    claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: signin
                );

                _response.Resultado = modelo;
                _response.Token = new JwtSecurityTokenHandler().WriteToken(token);
                _response.StatusCode = HttpStatusCode.Created;

                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.isExitoso = false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<APIResponse>> Login([FromBody] Object optData)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(optData.ToString());

            string user = data.email.ToString();
            string pass = data.password.ToString();

            var usuario = await _userRepo.Obtener(v => v.Email.ToLower() == user.ToLower());
            var password = await _userRepo.Obtener(v => v.Password.ToLower() == pass.ToLower());

            if (usuario == null && password == null)
            {
                _response.isExitoso = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                ModelState.AddModelError("NoExiste", "Credenciales Incorrectas");
                return BadRequest(ModelState);
            }

            if (password == null)
            {
                _response.isExitoso = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                ModelState.AddModelError("NoExiste", "Password Incorrecto");
                return BadRequest(ModelState);
            }

            if (usuario == null)
            {
                _response.isExitoso = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                ModelState.AddModelError("NoExiste", "El Usuario no Existe");
                return BadRequest(ModelState);
            }
            
            var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("id", usuario.Id.ToString()),
                new Claim("nombre", usuario.Nombre)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var signin = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signin
            );

            _response.Resultado = usuario.Id;
            _response.Token = new JwtSecurityTokenHandler().WriteToken(token);
            _response.isExitoso = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (identity.Claims.Count() == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso = false;
                    return BadRequest(_response);
                }

                var num = identity.Claims.FirstOrDefault(v => v.Type == "id").Value;

                var user = await _userRepo.Obtener(v => v.Id.ToString() == num);

                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso = false;
                    return BadRequest(_response);
                }

                if(id <= 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso = false;
                    return BadRequest(_response);
                }
                var usuario = await _userRepo.Obtener(v => v.Id == id);


                await _userRepo.Remover(usuario);

                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.isExitoso = false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return BadRequest(_response);
        }
    }
}
