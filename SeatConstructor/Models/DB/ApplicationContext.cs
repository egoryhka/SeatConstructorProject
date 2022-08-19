using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SeatConstructor.Models.DB
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }


        public DbSet<Seat> Seats { get; set; }
        //public DbSet<Modification> Modifications { get; set; }
        public DbSet<ModificationType> ModificationTypes { get; set; }
        public DbSet<ModForSeat> ModsForSeats { get; set; }


        //------------
        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        //------------


        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string adminRoleName = "admin";
            string userRoleName = "user";

            string adminLogin = "egoryhka";
            string adminPassword = "158394";

            //стандартные роли
            RoleModel adminRole = new RoleModel { Id = 1, Name = adminRoleName };
            RoleModel userRole = new RoleModel { Id = 2, Name = userRoleName };

            //админ
            UserModel adminUser = new UserModel
            {
                Id = 1,
                Login = adminLogin,
                Password = adminPassword,
                RoleId = adminRole.Id
            };


            modelBuilder.Entity<RoleModel>().HasData(new RoleModel[] { adminRole, userRole });
            modelBuilder.Entity<UserModel>().HasData(new UserModel[] { adminUser, });

        }



    }
}
