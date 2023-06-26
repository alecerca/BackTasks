using BackTasks.Models;

namespace BackTasks.Repositorios.IRepositorio
{
    public interface IUsuario : IRepositorio<Usuario>
    {
        Task<Usuario> Actualizar(Usuario entidad);
    }
}
