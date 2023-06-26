using BackTasks.Models;

namespace BackTasks.Repositorios.IRepositorio
{
    public interface IProyecto : IRepositorio<Proyecto>
    {
        Task<Proyecto> Actualizar(Proyecto entidad);
    }
}
