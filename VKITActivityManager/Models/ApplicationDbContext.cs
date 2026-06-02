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
        // THÊM 2 BẢNG MỚI NÀY VÀO DƯỚI CÙNG
        public DbSet<PhanLoaiVideo> PhanLoaiVideos { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<ChuyenNganh> ChuyenNganhs { get; set; }
        public DbSet<HoatDongChuyenNganh> HoatDongChuyenNganhs { get; set; }
        public DbSet<LoaiHocBong> LoaiHocBongs { get; set; }
        public DbSet<SinhVienHocBong> SinhVienHocBongs { get; set; }
        // BẢNG QUẢN LÝ ƯU ĐIỂM CHƯƠNG TRÌNH
        public DbSet<UuDiem> UuDiems { get; set; }
        public DbSet<AnhUuDiem> AnhUuDiems { get; set; }
        // BẢNG QUẢN LÝ CHATBOX FAQ
        public DbSet<CauHoiThuongGap> CauHoiThuongGaps { get; set; }
        // Ghi đè hàm này để tự động tạo 2 loại Video mặc định cho Admin chọn
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sinh dữ liệu mẫu (Seed Data)
            modelBuilder.Entity<PhanLoaiVideo>().HasData(
                new PhanLoaiVideo { Id = 1, TenPhanLoai = "Video Giới thiệu (Toàn màn hình)" },
                new PhanLoaiVideo { Id = 2, TenPhanLoai = "Video Danh sách (Ngang ở dưới)" }
            );
        }
    }
}