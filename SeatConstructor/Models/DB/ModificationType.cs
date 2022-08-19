using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SeatConstructor.Models.DB
{
    public class ModificationType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PositionInSummaryCode { get; set; }

        [Required]
        public string Name { get; set; }

    }
}
