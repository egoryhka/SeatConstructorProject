using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SeatConstructor.Models.DB
{
    public class ModForSeat
    {
        [Key]
        public int Id { get; set; }
        public int SeatId { get; set; }
        public int ModificationId { get; set; }
        public int TypeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }
        public string Description { get; set; }

        [Required]
        public string PathTo3DModel { get; set; }

        [Required]
        public virtual ModificationType Type { get; set; }

        [Required]
        public virtual Seat Seat { get; set; }

        //[Required]
        //public virtual Modification Modification { get; set; }

        public bool Mirror { get; set; }
        public string OffsetX { get; set; }
        public string OffsetY { get; set; }
        public string OffsetZ { get; set; }

    }
}
