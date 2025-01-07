using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DESTAP.Models
{
    public class DB_Context : DbContext
    {
        public DB_Context(DbContextOptions<DB_Context> options)
            : base(options)
        {
        }

        public DbSet<UserModel> TB_Users { get; set; }
        public DbSet<RoleModel> TB_Roles { get; set; }
        public DbSet<ChangeTrackModel> TB_ChangeTrack { get; set; }
        public DbSet<CPATrackModel> TB_CPATrack { get; set; }
        public DbSet<DVTrackModel> TB_DVTrack { get; set; }

        public DbSet<ChangeTrackModel> TB_ChangeTrack_backup { get; set; }
        public DbSet<CPATrackModel> TB_CPATrack_backup { get; set; }
        public DbSet<DVTrackModel> TB_DVTrack_backup { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DepartmentModel için keyless yapı tanımlanıyor
            modelBuilder.Entity<DepartmentModel>().HasNoKey();
            modelBuilder.Entity<UserByDepartmentModel>().HasNoKey();
           

            // UserID ile User arasında bire çok ilişki
            modelBuilder.Entity<ChangeTrackModel>()
                .HasOne(dv => dv.User)
                .WithMany(u => u.CHTracks)
                .HasForeignKey(dv => dv.UserID);


            // RSP_User ile User arasında bire çok ilişki
            modelBuilder.Entity<ChangeTrackModel>()
                .HasOne(dv => dv.ResponsibleUser)
                .WithMany(u => u.ResponsibleCHTracks)
                .HasForeignKey(dv => dv.RSP_User);

            modelBuilder.Entity<DVTrackModel>()
                .HasOne(dv => dv.User)
                .WithMany(u => u.DVTracks)
                .HasForeignKey(dv => dv.UserID);


            // RSP_User ile User arasında bire çok ilişki
            modelBuilder.Entity<DVTrackModel>()
                .HasOne(dv => dv.ResponsibleUser)
                .WithMany(u => u.ResponsibleDVTracks)
                .HasForeignKey(dv => dv.RSP_User);

            modelBuilder.Entity<CPATrackModel>()
                .HasOne(dv => dv.User)
                .WithMany(u => u.CPATracks)
                .HasForeignKey(dv => dv.UserID);


            // RSP_User ile User arasında bire çok ilişki
            modelBuilder.Entity<CPATrackModel>()
                .HasOne(dv => dv.ResponsibleUser)
                .WithMany(u => u.ResponsibleCPATracks)
                .HasForeignKey(dv => dv.RSP_User);

         
        }

        // Custom method to call stored procedure and fetch departments
        public async Task<List<DepartmentModel>> GetDepartmentsFromProcedureAsync()
        {
            // SQL sorgusunu çalıştırıyoruz ve Department modeline uygun şekilde veriyi alıyoruz
            var departments = await this.Set<DepartmentModel>()
                .FromSqlRaw("EXEC dbo.SP_DepartmentsWithGroup")
                .ToListAsync();

            return departments;
        }
        public async Task<List<UserByDepartmentModel>> GetUsersByDepartmentAsync(string department)
        {
            var users = await this.Set<UserByDepartmentModel>()
                .FromSqlRaw("EXEC dbo.SP_GetUsersByDepartment @depart", new SqlParameter("@depart", department))
                .ToListAsync();

            return users;
        }
        
    }
}