using AutoMapper;
using BackTasks.Models;
using BackTasks.Models.Dto;
using BackTasks.Repositorios.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace BackTasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareaController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITarea _tareaRepo;
        private readonly IProyecto _proyeRepo;
        private readonly IUsuario _userRepo;
        protected APIResponse _response;

        public TareaController(IMapper mapper, ITarea tareaRepo, IProyecto proyeRepo, IUsuario userRepo)
        {
            _mapper = mapper;
            _tareaRepo = tareaRepo;
            _proyeRepo = proyeRepo;
            _response = new();
            _userRepo = userRepo;
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetTareas(int id)
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
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.isExitoso = false;
                    return Unauthorized(_response);
                }

                IEnumerable<Tarea> tareaList = await _tareaRepo.ObtenerTodos(v => v.IdP == id);
                if(tareaList == null)
                {
                    _response.isExitoso = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<IEnumerable<Tarea>>(tareaList);
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

        [HttpPost("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> CrearTarea([FromBody] TareaDtoCreate createDto, int id)
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

                var proye = await _proyeRepo.Obtener(v => v.Id == id && v.Creador == user.Id);

                if (proye == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isExitoso = false;
                    return BadRequest(_response);
                }

                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.isExitoso = false;
                    return Unauthorized(_response);
                }

                if (!ModelState.IsValid)
                {
                    _response.isExitoso = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if(createDto == null)
                {
                    return BadRequest(createDto);
                }

                Tarea modelo = _mapper.Map<Tarea>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                modelo.Estado = false;
                modelo.IdP = id;
                
                if(await _tareaRepo.Obtener(v => v.Nombre == createDto.Nombre && id == v.IdP) != null)
                {
                    ModelState.AddModelError("NombreExiste", "La tarea con ese nombre ya existe");
                    return BadRequest(ModelState);
                }

                await _tareaRepo.Crear(modelo);
                _response.Resultado = modelo;
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

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarTarea(int id)
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

                if (id <= 0)
                {
                    _response.isExitoso = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.isExitoso = false;
                    return Unauthorized(_response);
                }

                var tarea = await _tareaRepo.Obtener(v => v.Id == id);
                if(tarea == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.isExitoso = false;
                    return NotFound(_response);
                }

                await _tareaRepo.Remover(tarea);
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

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> ActualizarTarea(int id, [FromBody] TareaDtoUpdate updateDto)
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

            var proye = await _proyeRepo.Obtener(v => v.Creador == user.Id);

            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.isExitoso = false;
                return BadRequest(_response);
            }

            if (updateDto == null || id <= 0)
            {
                _response.isExitoso = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            updateDto.Id = id;

            if(await _proyeRepo.Obtener(v => v.Id == updateDto.IdP) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Proyecto creador no existe");
                return BadRequest(ModelState);
            }
            Tarea modelo = _mapper.Map<Tarea>(updateDto);

            await _tareaRepo.Actualizar(modelo);
            _response.Resultado = modelo;
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);

        }

    }
}
