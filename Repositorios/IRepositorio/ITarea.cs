using BackTasks.Models;

namespace BackTasks.Repositorios.IRepositorio
{
    public interface ITarea : IRepositorio<Tarea>
    {
        Task<Tarea> Actualizar(Tarea tareas);
    }
}
