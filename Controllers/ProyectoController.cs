using AutoMapper;
using BackTasks.Repositorios.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using BackTasks;
using System.Security.Claims;
using BackTasks.Models;
using System.Net;
using BackTasks.Models.Dto;

namespace BackTasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProyectoController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IProyecto _proyeRepo;
        private readonly IUsuario _userRepo;

        public ProyectoController(IMapper mapper, IProyecto proyeRepo, IUsuario userRepo)
        {
            _response = new();
            _mapper = mapper;
            _proyeRepo = proyeRepo;
            _userRepo = userRepo;
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetProyecto(int id)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if(identity.Claims.Count() == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso = false;
                    return BadRequest(_response);
                }

                var num = identity.Claims.FirstOrDefault(v => v.Type == "id").Value;

                var user = await _userRepo.Obtener(v => v.Id.ToString() == num);

                if(user == null)
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.isExitoso = false;
                    return Unauthorized(_response);
                }

                IEnumerable<Proyecto> proyeList = await _proyeRepo.ObtenerTodos(v => v.Creador == id && user.Id == id);

                if(proyeList == null)
                {
                    _response.isExitoso =false;
                    _response.StatusCode=HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Resultado = _mapper.Map<IEnumerable<ProyectoDto>>(proyeList); 

                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.isExitoso = false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CrearProyecto([FromBody] ProyectoDtoCreate createDto)
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

                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                 
                if(createDto == null)
                {
                    return BadRequest(createDto);
                }

                Proyecto modelo = _mapper.Map<Proyecto>(createDto);

                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                modelo.Creador = user.Id;
               
                if (await _proyeRepo.Obtener(v => v.Nombre == createDto.Nombre && v.Creador == modelo.Creador) != null) 
                {
                    ModelState.AddModelError("NombreExiste", "El Proyecto con ese nombre ya existe");
                    return BadRequest(ModelState);
                }
                
                await _proyeRepo.Crear(modelo);

                _response.Resultado = modelo;
                _response.StatusCode = HttpStatusCode.Created;

                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.isExitoso =false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> EliminarProyecto(int id)
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
                    _response.isExitoso =false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if(await _proyeRepo.Obtener(v => v.Creador == user.Id) != null )
                {
                    var proyecto = await _proyeRepo.Obtener(v => v.Id == id && v.Creador == user.Id);
                    if(proyecto == null)
                    {
                        _response.isExitoso = false;
                        _response.StatusCode = HttpStatusCode.NotFound;
                        return NotFound(_response);
                    }

                    await _proyeRepo.Remover(proyecto);

                    _response.StatusCode =HttpStatusCode.NoContent;
                    return Ok(_response);
                }

            }
            catch (Exception ex)
            {
                _response.isExitoso = false;
                _response.Errors = new List<string> { ex.ToString() };
            }
            return BadRequest(_response);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActualizarProyecto(int id, [FromBody] ProyectoDtoUpdate updateDto)
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

            if(updateDto == null || id <= 0)
            {
                _response.isExitoso = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            updateDto.Creador = user.Id;
            updateDto.Id = id;

            if(await _userRepo.Obtener(v => v.Id == updateDto.Creador) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Creador no Existe");
                return BadRequest(ModelState);
            }

            Proyecto modelo = _mapper.Map<Proyecto>(updateDto);
             
            await _proyeRepo.Actualizar(modelo);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }


    }
}
