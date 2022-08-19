using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SeatConstructor.Models.DB
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SummaryCode { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactTel { get; set; }

    }
}
