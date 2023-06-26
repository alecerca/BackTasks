using BackTasks.Models;
using BackTasks.Repositorios.IRepositorio;

namespace BackTasks.Repositorios
{
    public class ProyectoRepositorio : Repositorio<Proyecto>, IProyecto
    {
        private readonly ApplicationDbContext _db;
        public ProyectoRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Proyecto> Actualizar(Proyecto entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Proyectos.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
