using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VKITActivityManager.Models;

namespace VKITActivityManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.DanhSachHoatDong.OrderByDescending(a => a.NgayTao).ToListAsync();

            // Lấy danh sách học phí và SẮP XẾP THEO NGÀNH để đảm bảo các dòng cùng ngành nằm cạnh nhau
            ViewBag.DanhSachHocPhi = await _context.DanhSachHocPhi
                                                   .OrderBy(x => x.NganhDaoTao)
                                                   .ToListAsync();

            return View(data);
        }
        // ==========================================
        // TÍNH NĂNG XEM CHI TIẾT BÀI VIẾT / TIN TỨC
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hoatDong = await _context.DanhSachHoatDong.FirstOrDefaultAsync(m => m.Id == id);

            if (hoatDong == null) return NotFound();

            return View(hoatDong);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        /* public IActionResult ChiTietNganh(int id)
         {
             // Lấy thông tin ngành từ bảng NganhDaoTaos
             var nganh = _context.NganhDaoTaos.Find(id);
             if (nganh == null) return NotFound();

             // SỬ DỤNG ĐÚNG TÊN BẢNG LÀ DanhSachHoatDong (giống dòng 37 của bạn)
             var hoatDongs = _context.DanhSachHoatDong
                 .Where(h => h.NganhId == id)
                 .OrderByDescending(h => h.NgayTao)
                 .ToList();

             ViewBag.TenNganh = nganh.TenNganh;
             ViewBag.MoTa = nganh.MoTa;

             return View(hoatDongs);
         }*/
        // 1. KHI NGƯỜI DÙNG CLICK VÀO NGÀNH (Hiển thị Ảnh 1)
        public IActionResult DanhSachHoatDongNganh(int id)
        {
            var nganh = _context.ChuyenNganhs.Find(id);
            if (nganh == null) return RedirectToAction("Index");

            // Chỉ lấy bài viết thuộc về đúng ID ngành được click
            var danhSach = _context.HoatDongChuyenNganhs
                .Where(x => x.ChuyenNganhId == id)
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            ViewBag.TenNganh = nganh.TenNganh;
            return View(danhSach);
        }

        // 2. KHI NGƯỜI DÙNG CLICK VÀO 1 THẺ BÀI VIẾT (Hiển thị Ảnh 2)
        public IActionResult ChiTietHoatDongNganh(int id)
        {
            // Lấy bài viết dựa vào ID của chính bài viết đó
            var baiViet = _context.HoatDongChuyenNganhs.Find(id);
            if (baiViet == null) return RedirectToAction("Index");

            return View(baiViet);
        }
    }
}