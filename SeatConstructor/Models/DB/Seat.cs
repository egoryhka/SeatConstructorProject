using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SeatConstructor.Models.DB
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public string Postfix { get; set; }

        [Required]
        public string PathTo3DModel { get; set; }

        [Required]
        public string PathToImage { get; set; }

    }
}
