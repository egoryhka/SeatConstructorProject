using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeatConstructor.Models.DB
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public int? RoleId { get; set; }
        public virtual RoleModel Role { get; set; }

    }
}
