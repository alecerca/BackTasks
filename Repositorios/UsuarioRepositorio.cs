using BackTasks.Models;
using BackTasks.Repositorios.IRepositorio;

namespace BackTasks.Repositorios
{
    public class UsuarioRepositorio : Repositorio<Usuario>, IUsuario
    {
        private readonly ApplicationDbContext _db;
        
        public UsuarioRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Usuario> Actualizar(Usuario entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Usuarios.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
