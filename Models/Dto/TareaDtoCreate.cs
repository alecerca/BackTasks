using System.ComponentModel.DataAnnotations;

namespace BackTasks.Models.Dto
{
    public class TareaDtoCreate
    {
        [Required]
        public string Nombre { get; set; }
    }
}
