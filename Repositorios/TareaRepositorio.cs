using BackTasks.Models;
using BackTasks.Repositorios.IRepositorio;

namespace BackTasks.Repositorios
{
    public class TareaRepositorio : Repositorio<Tarea>, ITarea
    {
        private readonly ApplicationDbContext _db;

        public TareaRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Tarea> Actualizar(Tarea entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Tareas.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
