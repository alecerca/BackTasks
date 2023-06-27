using AutoMapper;
using BackTasks.Models;
using BackTasks.Models.Dto;

namespace BackTasks
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Proyecto, ProyectoDto>().ReverseMap();
            CreateMap<Tarea, TareaDto>().ReverseMap();

            CreateMap<Usuario, UsuarioDtoCreate>().ReverseMap();
            CreateMap<Proyecto, ProyectoDtoCreate>().ReverseMap();
            CreateMap<Tarea, TareaDtoCreate>().ReverseMap();

            CreateMap<Proyecto, ProyectoDtoUpdate>().ReverseMap();
            CreateMap<Tarea, TareaDtoUpdate>().ReverseMap();
        }
    }
}
