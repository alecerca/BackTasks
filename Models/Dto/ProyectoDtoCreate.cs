﻿using System.ComponentModel.DataAnnotations;

namespace BackTasks.Models.Dto
{
    public class ProyectoDtoCreate
    {
        [Required]
        public string Nombre { get; set; }

    }
}
