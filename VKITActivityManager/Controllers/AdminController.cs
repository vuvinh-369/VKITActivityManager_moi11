using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VKITActivityManager.Models;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;
namespace VKITActivityManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            // ĐÃ SỬA: Loại trừ PhanLoai == 1 (Tin tức) và PhanLoai == 2 (Video)
            var data = await _context.DanhSachHoatDong
                .Where(x => x.PhanLoai != 1 && x.PhanLoai != 2)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.DanhSachHocPhi = await _context.DanhSachHocPhi.ToListAsync();

            // HOẠT ĐỘNG CHUYÊN NGÀNH
            ViewBag.DanhSachHDCN = await _context.HoatDongChuyenNganhs
                .Include(x => x.ChuyenNganh)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.DanhSachSVHB = await _context.SinhVienHocBongs
                .Include(s => s.LoaiHocBong)
                .OrderByDescending(s => s.NgayNhan)
                .ToListAsync();

            ViewBag.DanhSachLoaiHB = await _context.LoaiHocBongs.OrderByDescending(x => x.Id).ToListAsync();

            return View(data);
        }

        [HttpGet]
        public IActionResult Create(int? type)
        {
            var model = new HoatDong { PhanLoai = type ?? 3 }; // Sửa mặc định thành 3 (Banner) thay vì 1 (Tin Tức)
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> Create(HoatDong hoatDong, IFormFile? fileAnh)
        {
            // Bỏ qua kiểm tra fileAnh trong ModelState vì ta xử lý thủ công
            ModelState.Remove("fileAnh");

            // THÊM ĐOẠN NÀY ĐỂ TRỊ BỆNH CHO BANNER
            if (hoatDong.PhanLoai == 3)
            {
                ModelState.Remove("TieuDe");
                ModelState.Remove("NoiDung");

                // Tự động gán dữ liệu rác vào để thỏa mãn yêu cầu của Database (nếu DB yêu cầu NOT NULL)
                hoatDong.TieuDe = "Banner Slider " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                hoatDong.NoiDung = "Nội dung hình ảnh Banner";
            }

            if (ModelState.IsValid)
            {
                // Xử lý ảnh... (Giữ nguyên code bên trong của bạn)
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "hoatdong");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(fileStream); }
                    hoatDong.DuongDanAnh = "/images/hoatdong/" + uniqueFileName;
                }

                hoatDong.NgayTao = DateTime.Now;
                _context.DanhSachHoatDong.Add(hoatDong);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Thêm mới nội dung thành công!";
                TempData["ActiveTab"] = hoatDong.PhanLoai.ToString();
                return RedirectToAction(nameof(Index));
            }
            return View(hoatDong);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var hoatDong = await _context.DanhSachHoatDong.FindAsync(id);
            return hoatDong == null ? NotFound() : View(hoatDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> Edit(int id, HoatDong hoatDong, IFormFile? fileAnh)
        {
            if (id != hoatDong.Id) return NotFound();
            ModelState.Remove("fileAnh");

            // THÊM ĐOẠN NÀY ĐỂ TRỊ BỆNH CHO BANNER
            if (hoatDong.PhanLoai == 3)
            {
                ModelState.Remove("TieuDe");
                ModelState.Remove("NoiDung");
                hoatDong.TieuDe = "Banner Slider " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                hoatDong.NoiDung = "Nội dung hình ảnh Banner";
            }

            if (ModelState.IsValid)
            {
                // Giữ nguyên đoạn code cập nhật bên trong của bạn...
                var hoatDongDb = await _context.DanhSachHoatDong.FindAsync(id);
                if (hoatDongDb == null) return NotFound();

                // CẬP NHẬT ĐẦY ĐỦ CÁC TRƯỜNG DỮ LIỆU
                hoatDongDb.TieuDe = hoatDong.TieuDe;
                // ... Các dòng code dưới giữ nguyên ...

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật dữ liệu hoàn tất!";
                TempData["ActiveTab"] = hoatDongDb.PhanLoai.ToString();
                return RedirectToAction(nameof(Index));
            }
            return View(hoatDong);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var hoatDong = await _context.DanhSachHoatDong.FindAsync(id);
            if (hoatDong != null)
            {
                int phanLoaiTam = hoatDong.PhanLoai;
                _context.DanhSachHoatDong.Remove(hoatDong);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Đã xóa nội dung thành công!";
                TempData["ActiveTab"] = phanLoaiTam.ToString();
            }
            return RedirectToAction(nameof(Index));
        }

// =========================================================================
// QUẢN LÝ HỌC PHÍ (ĐÃ TÍCH HỢP CỘT HỆ ĐÀO TẠO)
// =========================================================================

// GET: Admin/CreateHocPhi
[HttpGet]
public IActionResult CreateHocPhi()
        {
            return View();
        }

        // POST: Admin/CreateHocPhi
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo (CSRF)
        public async Task<IActionResult> CreateHocPhi([Bind("Id,NganhDaoTao,HeDaoTao,DonViApDung,MucHocPhi,HocPhiGiam25,HocPhiGiam50,ThoiGian,LaDongPhu")] HocPhi hocPhi)
        {
            if (ModelState.IsValid)
            {
                _context.DanhSachHocPhi.Add(hocPhi);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Thêm mức học phí mới thành công!";
                TempData["ActiveTab"] = "hocphi"; // Trả về đúng tab "Học phí"
                return RedirectToAction(nameof(Index));
            }
            return View(hocPhi);
        }

        // GET: Admin/EditHocPhi/5
        [HttpGet]
        public async Task<IActionResult> EditHocPhi(int? id)
        {
            // Bổ sung kiểm tra ID để tránh lỗi màn hình trắng nếu người dùng gõ sai URL
            if (id == null) return NotFound();

            var hocPhi = await _context.DanhSachHocPhi.FindAsync(id);

            if (hocPhi == null) return NotFound();

            return View(hocPhi);
        }

        // POST: Admin/EditHocPhi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHocPhi(int id, [Bind("Id,NganhDaoTao,HeDaoTao,DonViApDung,MucHocPhi,HocPhiGiam25,HocPhiGiam50,ThoiGian,LaDongPhu")] HocPhi hocPhi)
        {
            // Tránh lỗi bảo mật khi ai đó cố tình đổi ID ngầm
            if (id != hocPhi.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hocPhi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý trường hợp có 2 admin cùng sửa 1 lúc hoặc dữ liệu không tồn tại
                    if (!_context.DanhSachHocPhi.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }

                TempData["SuccessMsg"] = "Cập nhật thông tin học phí thành công!";
                TempData["ActiveTab"] = "hocphi";
                return RedirectToAction(nameof(Index));
            }
            return View(hocPhi);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteHocPhi(int id)
        {
            _context.DanhSachHocPhi.Remove(await _context.DanhSachHocPhi.FindAsync(id)); await _context.SaveChangesAsync();
            TempData["ActiveTab"] = "5"; return RedirectToAction(nameof(Index));
        }
        // =========================================================================
        // QUẢN LÝ HOẠT ĐỘNG CHUYÊN NGÀNH (TÍCH HỢP VÀO ADMIN)
        // =========================================================================

        // GET: Admin/IndexHDCN
        public async Task<IActionResult> IndexHDCN(int? nganhId)
        {
            var danhSachNganh = await _context.ChuyenNganhs.ToListAsync();
            ViewBag.NganhList = new SelectList(danhSachNganh, "Id", "TenNganh", nganhId);
            ViewBag.SelectedNganhId = nganhId;

            var query = _context.HoatDongChuyenNganhs.Include(h => h.ChuyenNganh).AsQueryable();

            if (nganhId.HasValue)
            {
                query = query.Where(h => h.ChuyenNganhId == nganhId.Value);
            }

            var danhSachBaiViet = await query.OrderByDescending(h => h.NgayTao).ToListAsync();
            return View(danhSachBaiViet);
        }

        // GET: Admin/CreateHDCN
        public IActionResult CreateHDCN(int? nganhId)
        {
            ViewBag.ChuyenNganhId = new SelectList(_context.ChuyenNganhs, "Id", "TenNganh", nganhId);
            return View();
        }

        // POST: Admin/CreateHDCN
        // POST: Admin/CreateHDCN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> CreateHDCN(
    HoatDongChuyenNganh hoatDong,
    IFormFile? hinhAnh)
        {
            ModelState.Remove("ChuyenNganh");

            if (ModelState.IsValid)
            {
                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "hoatdong"
                    );

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string fileName = Guid.NewGuid().ToString()
                                    + Path.GetExtension(hinhAnh.FileName);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(stream);
                    }

                    hoatDong.DuongDanAnh =
                        "/images/hoatdong/" + fileName;
                }

                hoatDong.NgayTao = DateTime.Now;

                _context.HoatDongChuyenNganhs.Add(hoatDong);

                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Thêm thành công";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.ChuyenNganhId = new SelectList(
                _context.ChuyenNganhs,
                "Id",
                "TenNganh",
                hoatDong.ChuyenNganhId
            );

            return View(hoatDong);
        }
        // GET: Admin/EditHDCN/5
        public async Task<IActionResult> EditHDCN(int? id)
        {
            if (id == null) return NotFound();

            var hoatDong = await _context.HoatDongChuyenNganhs.FindAsync(id);
            if (hoatDong == null) return NotFound();

            ViewBag.ChuyenNganhId = new SelectList(_context.ChuyenNganhs, "Id", "TenNganh", hoatDong.ChuyenNganhId);
            return View(hoatDong);
        }

        // POST: Admin/EditHDCN/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> EditHDCN(
     int id,
     HoatDongChuyenNganh hoatDong,
     IFormFile? hinhAnh)
        {
            if (id != hoatDong.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var data = await _context.HoatDongChuyenNganhs.FindAsync(id);

                    if (data == null)
                    {
                        return NotFound();
                    }

                    data.TieuDe = hoatDong.TieuDe;
                    data.TieuDePhu = hoatDong.TieuDePhu;
                    data.NoiDung = hoatDong.NoiDung;
                    data.ChuyenNganhId = hoatDong.ChuyenNganhId;

                    // Upload ảnh mới
                    if (hinhAnh != null && hinhAnh.Length > 0)
                    {
                        string uploadFolder = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            "images",
                            "hoatdong"
                        );

                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string fileName =
                            Guid.NewGuid().ToString() +
                            Path.GetExtension(hinhAnh.FileName);

                        string filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await hinhAnh.CopyToAsync(stream);
                        }

                        data.DuongDanAnh = "/images/hoatdong/" + fileName;
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMsg"] = "Cập nhật thành công!";

                    return RedirectToAction(nameof(IndexHDCN));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            ViewBag.ChuyenNganhId = new SelectList(
                _context.ChuyenNganhs,
                "Id",
                "TenNganh",
                hoatDong.ChuyenNganhId
            );

            return View(hoatDong);
        }

        // POST: Admin/DeleteHDCN/5 (Xóa trực tiếp không cần view confirm)
        [HttpPost]
        public async Task<IActionResult> DeleteHDCN(int id)
        {
            var hoatDong = await _context.HoatDongChuyenNganhs.FindAsync(id);
            int? rNganhId = hoatDong?.ChuyenNganhId;
            if (hoatDong != null)
            {
                _context.HoatDongChuyenNganhs.Remove(hoatDong);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(IndexHDCN), new { nganhId = rNganhId });
        }
        // GET: Admin/CreateSVHB
        public IActionResult CreateSVHB()
        {
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong");
            return View();
        }

     
[HttpPost]
[ValidateAntiForgeryToken]
[RequestSizeLimit(1073741824)]
public async Task<IActionResult> CreateSVHB(
    SinhVienHocBong sv,
    IFormFile? fileAnh)
        {
            ModelState.Remove("LoaiHocBong");

            if (ModelState.IsValid)
            {
                // UPLOAD ẢNH
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "sinhvienhocbong"
                    );

                    // Tạo folder nếu chưa có
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Tạo tên ảnh mới
                    string fileName =
                        Guid.NewGuid().ToString()
                        + Path.GetExtension(fileAnh.FileName);

                    string filePath =
                        Path.Combine(uploadFolder, fileName);

                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileAnh.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn vào DB
                    sv.HinhAnh =
                        "/images/sinhvienhocbong/" + fileName;
                }

                sv.NgayNhan = DateTime.Now;

                _context.SinhVienHocBongs.Add(sv);

                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] =
                    "Thêm sinh viên thành công!";

                TempData["ActiveTab"] = "svhocbong";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiHocBongId =
                new SelectList(
                    _context.LoaiHocBongs,
                    "Id",
                    "TenHocBong",
                    sv.LoaiHocBongId
                );

            return View(sv);
        }


        // GET: Admin/EditSVHB/5
        [HttpGet]
        public async Task<IActionResult> EditSVHB(int? id)
        {
            if (id == null) return NotFound();

            var sv = await _context.SinhVienHocBongs.FindAsync(id);
            if (sv == null) return NotFound();

            // Truyền danh sách Loại Học Bổng ra Dropdown để Admin có thể đổi loại học bổng nếu chọn nhầm
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong", sv.LoaiHocBongId);
            return View(sv);
        }

        // POST: Admin/EditSVHB/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> EditSVHB(int id, SinhVienHocBong sv, IFormFile? fileAnh)
        {
            if (id != sv.Id) return NotFound();

            ModelState.Remove("LoaiHocBong");
            ModelState.Remove("HinhAnh");

            if (ModelState.IsValid)
            {
                var data = await _context.SinhVienHocBongs.FindAsync(id);

                if (data == null) return NotFound();

                data.MaSV = sv.MaSV;
                data.TenSinhVien = sv.TenSinhVien;
                data.Lop = sv.Lop;
                data.LoaiHocBongId = sv.LoaiHocBongId;

                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "sinhvienhocbong"
                    );

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName =
                        Guid.NewGuid().ToString() +
                        Path.GetExtension(fileAnh.FileName);

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileAnh.CopyToAsync(fileStream);
                    }

                    data.HinhAnh = "/images/sinhvienhocbong/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Cập nhật sinh viên thành công!";
                TempData["ActiveTab"] = "svhocbong";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiHocBongId = new SelectList(
                _context.LoaiHocBongs,
                "Id",
                "TenHocBong",
                sv.LoaiHocBongId
            );

            return View(sv);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSVHB(int id)
        {
            var sv = await _context.SinhVienHocBongs.FindAsync(id);
            if (sv != null) { _context.SinhVienHocBongs.Remove(sv); await _context.SaveChangesAsync(); }
            TempData["ActiveTab"] = "svhocbong";
            return RedirectToAction(nameof(Index));
        }
        // =========================================================================
        // QUẢN LÝ DANH MỤC "LOẠI HỌC BỔNG" (CÁC THANH MÀU SẮC)
        // =========================================================================

        // GET: Admin/CreateLoaiHB
        [HttpGet]
        public IActionResult CreateLoaiHB()
        {
            return View();
        }

        // POST: Admin/CreateLoaiHB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLoaiHB([Bind("Id,TenHocBong,MoTa,SoSuat,MauNen")] LoaiHocBong loaiHB)
        {

            if (ModelState.IsValid)
            {
                _context.Add(loaiHB);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm Loại Học Bổng mới thành công!";
                TempData["ActiveTab"] = "loaihocbong";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiHB);
        }

        // GET: Admin/EditLoaiHB/5
        [HttpGet]
        public async Task<IActionResult> EditLoaiHB(int? id)
        {
            if (id == null) return NotFound();
            var loaiHB = await _context.LoaiHocBongs.FindAsync(id);
            if (loaiHB == null) return NotFound();
            return View(loaiHB);
        }

        // POST: Admin/EditLoaiHB/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoaiHB(int id, [Bind("Id,TenHocBong,MoTa,SoSuat,MauNen")] LoaiHocBong loaiHB)
        {
            if (id != loaiHB.Id) return NotFound();

            // Thêm dòng này để ép hệ thống bỏ qua kiểm tra danh sách sinh viên
            ModelState.Remove("SinhVienHocBongs");
            if (id != loaiHB.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(loaiHB);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật Loại Học Bổng thành công!";
                TempData["ActiveTab"] = "loaihocbong";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiHB);
        }

        // POST: Admin/DeleteLoaiHB/5
        [HttpPost]
        public async Task<IActionResult> DeleteLoaiHB(int id)
        {
            var loaiHB = await _context.LoaiHocBongs.FindAsync(id);
            if (loaiHB != null)
            {
                _context.LoaiHocBongs.Remove(loaiHB);
                await _context.SaveChangesAsync();
            }
            TempData["ActiveTab"] = "loaihocbong";
            return RedirectToAction(nameof(Index));
        }
    }
}