using Microsoft.EntityFrameworkCore;

using VKITActivityManager.Models;

namespace VKITActivityManager.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<HoatDong> DanhSachHoatDong { get; set; }
        public DbSet<HocPhi> DanhSachHocPhi { get; set; }
     
        public DbSet<ChuyenNganh> ChuyenNganhs { get; set; }
        public DbSet<HoatDongChuyenNganh> HoatDongChuyenNganhs { get; set; }
        public DbSet<LoaiHocBong> LoaiHocBongs { get; set; }
        public DbSet<SinhVienHocBong> SinhVienHocBongs { get; set; }
    }
}