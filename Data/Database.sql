-- =============================================
--  CSDL QUAN LY CoffeeTea
--  He QTCSDL: SQL Server
-- =============================================


IF DB_ID(N'QL_CoffeeTea') IS NOT NULL
BEGIN
    DROP DATABASE QL_CoffeeTea;
END
GO

CREATE DATABASE QL_CoffeeTea;
GO

USE QL_CoffeeTea;
GO

-- =============================================
-- 1. BANG VAI TRO
-- =============================================
CREATE TABLE VaiTro
(
    MaVaiTro      VARCHAR(10)    NOT NULL PRIMARY KEY,
    TenVaiTro     NVARCHAR(50)   NOT NULL,
    MoTa          NVARCHAR(255)  NULL
);
GO



-- =============================================
-- 2. BANG NHAN VIEN
-- =============================================
CREATE TABLE NhanVien
(
    MaNhanVien      VARCHAR(10)     NOT NULL PRIMARY KEY,
    HoTen           NVARCHAR(100)   NOT NULL,
    GioiTinh        NVARCHAR(10)    NULL,
    NgaySinh        DATE            NULL,
    SoDienThoai     VARCHAR(15)     NULL,
    Email           VARCHAR(100)    NULL,
    DiaChi          NVARCHAR(200)   NULL,
    AnhDaiDien      NVARCHAR(255)   NULL,
    TenDangNhap     VARCHAR(50)     NOT NULL UNIQUE,
    MatKhau         VARCHAR(100)    NOT NULL,
    MaVaiTro        VARCHAR(10)     NOT NULL,
    NgayVaoLam      DATE            NULL,
    LuongCoBan      DECIMAL(18,2)   NOT NULL DEFAULT 0,
    TrangThai       NVARCHAR(30)    NOT NULL DEFAULT N'Đang làm',
    CONSTRAINT FK_NhanVien_VaiTro
        FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- =============================================
-- 3. BANG DANH MUC MON
-- =============================================
CREATE TABLE DanhMucMon
(
    MaDanhMuc       VARCHAR(10)     NOT NULL PRIMARY KEY,
    TenDanhMuc      NVARCHAR(100)   NOT NULL,
    MoTa            NVARCHAR(255)   NULL
);
GO

-- =============================================
-- 4. BANG MON
-- =============================================
CREATE TABLE Mon
(
    MaMon           VARCHAR(10)     NOT NULL PRIMARY KEY,
    TenMon          NVARCHAR(120)   NOT NULL,
    MaDanhMuc       VARCHAR(10)     NOT NULL,
    DonGia          DECIMAL(18,2)   NOT NULL CHECK (DonGia >= 0),
    DonViTinh       NVARCHAR(30)    NOT NULL DEFAULT N'Ly',
    TrangThai       NVARCHAR(30)    NOT NULL DEFAULT N'Đang bán',
    MoTa            NVARCHAR(255)   NULL,
    CONSTRAINT FK_Mon_DanhMucMon
        FOREIGN KEY (MaDanhMuc) REFERENCES DanhMucMon(MaDanhMuc)
);
GO

-- =============================================
-- 5. BANG BAN
-- =============================================
CREATE TABLE Ban
(
    MaBan           VARCHAR(10)     NOT NULL PRIMARY KEY,
    TenBan          NVARCHAR(50)    NOT NULL,
    KhuVuc          NVARCHAR(50)    NULL,
    SoChoNgoi       INT             NOT NULL CHECK (SoChoNgoi > 0),
    TrangThai       NVARCHAR(30)    NOT NULL DEFAULT N'Trống'
);
GO

-- =============================================
-- 6. BANG NHA CUNG CAP
-- =============================================
CREATE TABLE NhaCungCap
(
    MaNCC           VARCHAR(10)     NOT NULL PRIMARY KEY,
    TenNCC          NVARCHAR(150)   NOT NULL,
    SoDienThoai     VARCHAR(15)     NULL,
    Email           VARCHAR(100)    NULL,
    DiaChi          NVARCHAR(200)   NULL,
    GhiChu          NVARCHAR(255)   NULL
);
GO

-- =============================================
-- 7. BANG NGUYEN LIEU
-- =============================================
CREATE TABLE NguyenLieu
(
    MaNguyenLieu    VARCHAR(10)     NOT NULL PRIMARY KEY,
    TenNguyenLieu   NVARCHAR(120)   NOT NULL,
    DonViTinh       NVARCHAR(30)    NOT NULL,
    SoLuongTon      DECIMAL(18,2)   NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    MucCanhBao      DECIMAL(18,2)   NOT NULL DEFAULT 0 CHECK (MucCanhBao >= 0),
    DonGiaNhapGanNhat DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (DonGiaNhapGanNhat >= 0),
    GhiChu          NVARCHAR(255)   NULL
);
GO

-- =============================================
-- 8. BANG CONG THUC MON
-- =============================================
CREATE TABLE CongThucMon
(
    MaCongThuc      VARCHAR(10)     NOT NULL PRIMARY KEY,
    MaMon           VARCHAR(10)     NOT NULL,
    MaNguyenLieu    VARCHAR(10)     NOT NULL,
    SoLuongSuDung   DECIMAL(18,2)   NOT NULL CHECK (SoLuongSuDung > 0),
    GhiChu          NVARCHAR(255)   NULL,
    CONSTRAINT FK_CongThucMon_Mon
        FOREIGN KEY (MaMon) REFERENCES Mon(MaMon),
    CONSTRAINT FK_CongThucMon_NguyenLieu
        FOREIGN KEY (MaNguyenLieu) REFERENCES NguyenLieu(MaNguyenLieu)
);
GO

-- =============================================
-- 9. BANG PHIEU NHAP
-- =============================================
CREATE TABLE PhieuNhap
(
    MaPhieuNhap     VARCHAR(10)     NOT NULL PRIMARY KEY,
    NgayNhap        DATETIME        NOT NULL DEFAULT GETDATE(),
    MaNCC           VARCHAR(10)     NOT NULL,
    MaNhanVien      VARCHAR(10)     NOT NULL,
    TongTien        DECIMAL(18,2)   NOT NULL DEFAULT 0 CHECK (TongTien >= 0),
    GhiChu          NVARCHAR(255)   NULL,
    CONSTRAINT FK_PhieuNhap_NhaCungCap
        FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC),
    CONSTRAINT FK_PhieuNhap_NhanVien
        FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);
GO

-- =============================================
-- 10. BANG CHI TIET PHIEU NHAP
-- =============================================
CREATE TABLE ChiTietPhieuNhap
(
    MaCTPN          VARCHAR(10)     NOT NULL PRIMARY KEY,
    MaPhieuNhap     VARCHAR(10)     NOT NULL,
    MaNguyenLieu    VARCHAR(10)     NOT NULL,
    SoLuong         DECIMAL(18,2)   NOT NULL CHECK (SoLuong > 0),
    DonGiaNhap      DECIMAL(18,2)   NOT NULL CHECK (DonGiaNhap >= 0),
    ThanhTien       AS (SoLuong * DonGiaNhap) PERSISTED,
    CONSTRAINT FK_CTPN_PhieuNhap
        FOREIGN KEY (MaPhieuNhap) REFERENCES PhieuNhap(MaPhieuNhap),
    CONSTRAINT FK_CTPN_NguyenLieu
        FOREIGN KEY (MaNguyenLieu) REFERENCES NguyenLieu(MaNguyenLieu)
);
GO

-- =============================================
-- 11. BANG HOA DON
-- =============================================
CREATE TABLE HoaDon
(
    MaHoaDon        VARCHAR(10)     NOT NULL PRIMARY KEY,
    NgayLap         DATETIME        NOT NULL DEFAULT GETDATE(),
    MaNhanVien      VARCHAR(10)     NOT NULL,
    MaBan           VARCHAR(10)     NOT NULL,
    TongTien        DECIMAL(18,2)   NOT NULL DEFAULT 0 CHECK (TongTien >= 0),
    PhuongThucTT    NVARCHAR(50)    NULL,
    TrangThai       NVARCHAR(30)    NOT NULL DEFAULT N'Chưa thanh toán',
    GhiChu          NVARCHAR(255)   NULL,
    CONSTRAINT FK_HoaDon_NhanVien
        FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),
    CONSTRAINT FK_HoaDon_Ban
        FOREIGN KEY (MaBan) REFERENCES Ban(MaBan)
);
GO

-- =============================================
-- 12. BANG CHI TIET HOA DON
-- =============================================
CREATE TABLE ChiTietHoaDon
(
    MaCTHD          VARCHAR(10)     NOT NULL PRIMARY KEY,
    MaHoaDon        VARCHAR(10)     NOT NULL,
    MaMon           VARCHAR(10)     NOT NULL,
    SoLuong         INT             NOT NULL CHECK (SoLuong > 0),
    DonGia          DECIMAL(18,2)   NOT NULL CHECK (DonGia >= 0),
    ThanhTien       AS (SoLuong * DonGia) PERSISTED,
    GhiChu          NVARCHAR(255)   NULL,
    CONSTRAINT FK_CTHD_HoaDon
        FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    CONSTRAINT FK_CTHD_Mon
        FOREIGN KEY (MaMon) REFERENCES Mon(MaMon)
);
GO

-- =============================================
-- DU LIEU MAU
-- =============================================

-- 1. VAI TRO
INSERT INTO VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
('VT01', N'Admin',     N'Toàn quyền hệ thống'),
('VT02', N'Quản lý',   N'Quản lý nghiệp vụ và tài khoản nhân viên'),
('VT03', N'Nhân viên', N'Order bán hàng và nhập hàng');
GO

-- 2. NHAN VIEN
INSERT INTO NhanVien
(MaNhanVien, HoTen, GioiTinh, NgaySinh, SoDienThoai, Email, DiaChi, AnhDaiDien, TenDangNhap, MatKhau, MaVaiTro, NgayVaoLam, LuongCoBan, TrangThai)
VALUES
('NV01', N'Nguyễn Hoàng Anh', N'Nam', '1999-03-15', '0901000001', 'admin123',  N'Quận 1', N'Images/Employees/default-avatar.png', 'admin',   '123456', 'VT01', '2023-01-10', 15000000, N'Đang làm'),
('NV02', N'Trần Mỹ Linh',     N'Nữ',  '2000-07-20', '0901000002', 'ql123',    N'Quận 3', N'Images/Employees/default-avatar.png', 'quanly1', '123456', 'VT02', '2023-03-12', 12000000, N'Đang làm'),
('NV03', N'Lê Quốc Bảo',      N'Nam', '1998-11-05', '0901000003', 'ql2@coffee.vn',    N'Quận 5', N'Images/Employees/default-avatar.png', 'quanly2', '123456', 'VT02', '2022-12-01', 12000000, N'Đang làm'),
('NV04', N'Phạm Thu Hà',      N'Nữ',  '2001-01-18', '0901000004', 'nv123',    N'Quận 10', N'Images/Employees/default-avatar.png', 'nhanvien1','123456', 'VT03', '2023-05-01',  7000000, N'Đang làm'),
('NV05', N'Võ Minh Tâm',      N'Nam', '1997-09-09', '0901000005', 'nv2@coffee.vn',    N'Tân Bình', N'Images/Employees/default-avatar.png', 'nhanvien2','123456', 'VT03', '2022-10-15',  7000000, N'Đang làm'),
('NV06', N'Đặng Ngọc Mai',    N'Nữ',  '1996-12-25', '0901000006', 'nv3@coffee.vn',    N'Phú Nhuận', N'Images/Employees/default-avatar.png', 'nhanvien3','123456', 'VT03', '2022-11-20',  7000000, N'Đang làm'),
('NV07', N'Bùi Thanh Nam',    N'Nam', '1995-04-30', '0901000007', 'nv4@coffee.vn',    N'Bình Thạnh', N'Images/Employees/default-avatar.png', 'nhanvien4','123456', 'VT03', '2022-09-10', 7000000, N'Đang làm'),
('NV08', N'Ngô Gia Hân',      N'Nữ',  '2002-08-14', '0901000008', 'nv5@coffee.vn',    N'Gò Vấp', N'Images/Employees/default-avatar.png', 'nhanvien5','123456', 'VT03', '2024-01-05',  6500000, N'Đang làm'),
('NV09', N'Phan Quốc Khánh',  N'Nam', '1994-06-06', '0901000009', 'nv6@coffee.vn',    N'Thủ Đức', N'Images/Employees/default-avatar.png', 'nhanvien6','123456', 'VT03', '2021-08-08',  7500000, N'Đang làm'),
('NV10', N'Tạ Thảo Nhi',      N'Nữ',  '2003-10-11', '0901000010', 'nv7@coffee.vn',    N'Quận 7', N'Images/Employees/default-avatar.png', 'nhanvien7','123456', 'VT03', '2024-06-01',  6000000, N'Đang làm');
GO
-- 3. DANH MUC MON
INSERT INTO DanhMucMon (MaDanhMuc, TenDanhMuc, MoTa) VALUES
('DM01', N'Cà phê',         N'Nhóm cà phê truyền thống và máy'),
('DM02', N'Trà sữa',        N'Nhóm trà sữa'),
('DM03', N'Trà trái cây',   N'Nhóm trà hoa quả'),
('DM04', N'Đá xay',         N'Nhóm blended'),
('DM05', N'Sinh tố',        N'Nhóm sinh tố trái cây'),
('DM06', N'Nước ép',        N'Nhóm nước ép'),
('DM07', N'Bánh ngọt',      N'Nhóm bánh ăn kèm'),
('DM08', N'Topping',        N'Nhóm topping thêm'),
('DM09', N'Trà nóng',       N'Nhóm trà nóng'),
('DM10', N'Món đặc biệt',   N'Món theo mùa hoặc signature');

-- 4. MON
INSERT INTO Mon (MaMon, TenMon, MaDanhMuc, DonGia, DonViTinh, TrangThai, MoTa) VALUES
('M01', N'Cà phê đen',           'DM01', 25000, N'Ly', N'Đang bán', N'Cà phê đen đá'),
('M02', N'Cà phê sữa',           'DM01', 30000, N'Ly', N'Đang bán', N'Cà phê sữa đá'),
('M03', N'Bạc xỉu',              'DM01', 32000, N'Ly', N'Đang bán', N'Bạc xỉu ngọt dịu'),
('M04', N'Trà sữa trân châu',    'DM02', 40000, N'Ly', N'Đang bán', N'Trà sữa truyền thống'),
('M05', N'Trà đào cam sả',       'DM03', 45000, N'Ly', N'Đang bán', N'Trà đào thơm mát'),
('M06', N'Matcha đá xay',        'DM04', 50000, N'Ly', N'Đang bán', N'Đá xay matcha'),
('M07', N'Sinh tố bơ',           'DM05', 48000, N'Ly', N'Đang bán', N'Sinh tố bơ'),
('M08', N'Nước ép cam',          'DM06', 35000, N'Ly', N'Đang bán', N'Nước ép cam tươi'),
('M09', N'Bánh tiramisu',        'DM07', 38000, N'Phần', N'Đang bán', N'Bánh ngọt ăn kèm'),
('M10', N'Trà vải hoa hồng',     'DM10', 47000, N'Ly', N'Đang bán', N'Món signature'),
('M11', N'Americano',             'DM01', 35000, N'Ly',   N'Đang bán', N'Cà phê máy pha loãng'),
('M12', N'Cappuccino',            'DM01', 45000, N'Ly',   N'Đang bán', N'Cà phê sữa bọt'),
('M13', N'Latte',                 'DM01', 45000, N'Ly',   N'Đang bán', N'Cà phê sữa tươi'),
('M14', N'Espresso',              'DM01', 30000, N'Ly',   N'Đang bán', N'Cà phê đậm vị'),
('M15', N'Trà sữa matcha',        'DM02', 45000, N'Ly',   N'Đang bán', N'Trà sữa vị matcha'),
('M16', N'Trà sữa khoai môn',     'DM02', 43000, N'Ly',   N'Đang bán', N'Trà sữa vị khoai môn'),
('M17', N'Trà sữa socola',        'DM02', 44000, N'Ly',   N'Đang bán', N'Trà sữa vị socola'),
('M18', N'Trà chanh',             'DM03', 25000, N'Ly',   N'Đang bán', N'Trà chanh giải khát'),
('M19', N'Trà tắc',               'DM03', 25000, N'Ly',   N'Đang bán', N'Trà tắc mát lạnh'),
('M20', N'Trà dâu',               'DM03', 39000, N'Ly',   N'Đang bán', N'Trà dâu trái cây'),
('M21', N'Chocolate đá xay',      'DM04', 52000, N'Ly',   N'Đang bán', N'Đá xay vị socola'),
('M22', N'Cookie đá xay',         'DM04', 55000, N'Ly',   N'Đang bán', N'Đá xay bánh quy'),
('M23', N'Sinh tố xoài',          'DM05', 45000, N'Ly',   N'Đang bán', N'Sinh tố xoài chín'),
('M24', N'Sinh tố dâu',           'DM05', 46000, N'Ly',   N'Đang bán', N'Sinh tố dâu tươi'),
('M25', N'Nước ép dưa hấu',       'DM06', 35000, N'Ly',   N'Đang bán', N'Nước ép dưa hấu'),
('M26', N'Nước ép ổi',            'DM06', 35000, N'Ly',   N'Đang bán', N'Nước ép ổi tươi'),
('M27', N'Bánh flan',             'DM07', 20000, N'Phần', N'Đang bán', N'Bánh flan mềm mịn'),
('M28', N'Bánh mousse chanh dây', 'DM07', 42000, N'Phần', N'Đang bán', N'Bánh mousse vị chanh dây'),
('M29', N'Trân châu thêm',        'DM08', 10000, N'Phần', N'Đang bán', N'Topping thêm'),
('M30', N'Kem cheese thêm',       'DM08', 15000, N'Phần', N'Đang bán', N'Topping kem cheese');
-- 5. BAN
INSERT INTO Ban (MaBan, TenBan, KhuVuc, SoChoNgoi, TrangThai) VALUES
('B01', N'Bàn 01', N'Tầng trệt', 2, N'Trống'),
('B02', N'Bàn 02', N'Tầng trệt', 2, N'Trống'),
('B03', N'Bàn 03', N'Tầng trệt', 4, N'Trống'),
('B04', N'Bàn 04', N'Tầng trệt', 4, N'Trống'),
('B05', N'Bàn 05', N'Tầng trệt', 6, N'Đã đặt'),
('B06', N'Bàn 06', N'Lầu 1',     2, N'Trống'),
('B07', N'Bàn 07', N'Lầu 1',     2, N'Trống'),
('B08', N'Bàn 08', N'Lầu 1',     4, N'Trống'),
('B09', N'Bàn 09', N'Sân vườn',  4, N'Trống'),
('B10', N'Bàn 10', N'Sân vườn',  6, N'Bảo trì');

-- 6. NHA CUNG CAP
INSERT INTO NhaCungCap (MaNCC, TenNCC, SoDienThoai, Email, DiaChi, GhiChu) VALUES
('NCC01', N'Công ty Cà phê Việt',      '0281111111', 'ncc1@mail.com', N'Quận 12, TP.HCM', N'Chuyên cà phê hạt'),
('NCC02', N'Trà Ngon Sài Gòn',         '0281111112', 'ncc2@mail.com', N'Bình Tân, TP.HCM', N'Chuyên trà'),
('NCC03', N'Sữa Tươi Thành Công',      '0281111113', 'ncc3@mail.com', N'Hóc Môn, TP.HCM', N'Cung cấp sữa'),
('NCC04', N'Đường Mía Việt',           '0281111114', 'ncc4@mail.com', N'Long An', N'Đường và syrup'),
('NCC05', N'Đá Sạch Minh Phát',        '0281111115', 'ncc5@mail.com', N'Thủ Đức, TP.HCM', N'Đá viên'),
('NCC06', N'Trái Cây Miền Nhiệt Đới',  '0281111116', 'ncc6@mail.com', N'Tiền Giang', N'Trái cây tươi'),
('NCC07', N'Bao Bì Ly Nhựa Xanh',      '0281111117', 'ncc7@mail.com', N'Bình Dương', N'Ly, ống hút'),
('NCC08', N'Topping House',            '0281111118', 'ncc8@mail.com', N'Quận 8, TP.HCM', N'Trân châu, thạch'),
('NCC09', N'Bánh Ngọt An Nhiên',       '0281111119', 'ncc9@mail.com', N'Quận 3, TP.HCM', N'Bánh ngọt'),
('NCC10', N'Matcha Nhật Việt',         '0281111120', 'ncc10@mail.com', N'Quận 7, TP.HCM', N'Bột matcha');

-- 7. NGUYEN LIEU
INSERT INTO NguyenLieu (MaNguyenLieu, TenNguyenLieu, DonViTinh, SoLuongTon, MucCanhBao, DonGiaNhapGanNhat, GhiChu) VALUES
('NL01', N'Cà phê hạt',       N'Kg', 25,   5, 180000, N'Dùng cho cà phê'),
('NL02', N'Sữa đặc',          N'Hộp', 40,  10, 28000, N'Pha cà phê sữa'),
('NL03', N'Sữa tươi',         N'Lít', 35,  8, 32000, N'Dùng cho bạc xỉu'),
('NL04', N'Trà đen',          N'Kg', 18,   4, 150000, N'Pha trà sữa'),
('NL05', N'Trân châu đen',    N'Kg', 20,   5, 90000, N'Topping'),
('NL06', N'Đào ngâm',         N'Hộp', 22,  6, 75000, N'Trà đào'),
('NL07', N'Cam tươi',         N'Kg', 30,   8, 45000, N'Nước ép cam'),
('NL08', N'Bột matcha',       N'Kg', 10,   2, 550000, N'Đá xay matcha'),
('NL09', N'Bơ sáp',           N'Kg', 16,   4, 70000, N'Sinh tố bơ'),
('NL10', N'Syrup hoa hồng',   N'Chai', 12, 3, 95000, N'Trà vải hoa hồng');

-- 8. CONG THUC MON
INSERT INTO CongThucMon (MaCongThuc, MaMon, MaNguyenLieu, SoLuongSuDung, GhiChu) VALUES
('CT01', 'M01', 'NL01', 0.02, N'20g cà phê hạt cho 1 ly'),
('CT02', 'M02', 'NL01', 0.02, N'20g cà phê hạt'),
('CT03', 'M02', 'NL02', 0.05, N'50g sữa đặc'),
('CT04', 'M03', 'NL01', 0.01, N'10g cà phê'),
('CT05', 'M03', 'NL03', 0.15, N'150ml sữa tươi'),
('CT06', 'M04', 'NL04', 0.01, N'10g trà đen'),
('CT07', 'M04', 'NL05', 0.03, N'30g trân châu'),
('CT08', 'M05', 'NL06', 0.10, N'1 phần đào ngâm'),
('CT09', 'M06', 'NL08', 0.02, N'20g bột matcha'),
('CT10', 'M10', 'NL10', 0.03, N'30ml syrup hoa hồng');

-- 9. PHIEU NHAP
INSERT INTO PhieuNhap (MaPhieuNhap, NgayNhap, MaNCC, MaNhanVien, TongTien, GhiChu) VALUES
('PN01', '2026-04-01 08:00:00', 'NCC01', 'NV05', 1800000, N'Nhập cà phê đầu tháng'),
('PN02', '2026-04-01 09:00:00', 'NCC03', 'NV05', 1120000, N'Nhập sữa'),
('PN03', '2026-04-02 08:30:00', 'NCC02', 'NV05', 1500000, N'Nhập trà đen'),
('PN04', '2026-04-02 10:00:00', 'NCC08', 'NV05', 900000,  N'Nhập topping'),
('PN05', '2026-04-03 07:45:00', 'NCC06', 'NV05', 1350000, N'Nhập trái cây'),
('PN06', '2026-04-04 08:20:00', 'NCC10', 'NV05', 1100000, N'Nhập matcha'),
('PN07', '2026-04-05 09:10:00', 'NCC04', 'NV05', 950000,  N'Nhập syrup'),
('PN08', '2026-04-06 08:40:00', 'NCC09', 'NV05', 760000,  N'Nhập bánh'),
('PN09', '2026-04-07 08:15:00', 'NCC05', 'NV05', 500000,  N'Nhập đá sạch'),
('PN10', '2026-04-08 11:00:00', 'NCC07', 'NV05', 680000,  N'Nhập ly nhựa');

-- 10. CHI TIET PHIEU NHAP
INSERT INTO ChiTietPhieuNhap (MaCTPN, MaPhieuNhap, MaNguyenLieu, SoLuong, DonGiaNhap) VALUES
('CTPN01', 'PN01', 'NL01', 10, 180000),
('CTPN02', 'PN02', 'NL03', 35, 32000),
('CTPN03', 'PN03', 'NL04', 10, 150000),
('CTPN04', 'PN04', 'NL05', 10, 90000),
('CTPN05', 'PN05', 'NL07', 30, 45000),
('CTPN06', 'PN06', 'NL08', 2,  550000),
('CTPN07', 'PN07', 'NL10', 10, 95000),
('CTPN08', 'PN08', 'NL06', 10, 76000),
('CTPN09', 'PN09', 'NL02', 10, 28000),
('CTPN10', 'PN10', 'NL09', 8,  85000);

-- 11. HOA DON
INSERT INTO HoaDon (MaHoaDon, NgayLap, MaNhanVien, MaBan, TongTien, PhuongThucTT, TrangThai, GhiChu) VALUES
('HD01', '2026-04-08 07:30:00', 'NV02', 'B01', 55000,  N'Tiền mặt', N'Đã thanh toán', N'Khách lẻ'),
('HD02', '2026-04-08 08:00:00', 'NV02', 'B02', 80000,  N'Chuyển khoản', N'Đã thanh toán', N'Khách 2 người'),
('HD03', '2026-04-08 08:30:00', 'NV02', 'B03', 125000, N'Tiền mặt', N'Đã thanh toán', N'Khách nhóm nhỏ'),
('HD04', '2026-04-08 09:00:00', 'NV02', 'B04', 45000,  N'Tiền mặt', N'Đã thanh toán', N'1 món'),
('HD05', '2026-04-08 09:30:00', 'NV02', 'B05', 97000,  N'Chuyển khoản', N'Đã thanh toán', N'Bàn đặt trước'),
('HD06', '2026-04-08 10:00:00', 'NV02', 'B06', 35000,  N'Tiền mặt', N'Đã thanh toán', N'Nước ép'),
('HD07', '2026-04-08 10:30:00', 'NV02', 'B07', 100000, N'Chuyển khoản', N'Đã thanh toán', N'2 món matcha'),
('HD08', '2026-04-08 11:00:00', 'NV02', 'B08', 76000,  N'Tiền mặt', N'Đã thanh toán', N'Bánh và nước'),
('HD09', '2026-04-08 11:30:00', 'NV02', 'B09', 47000,  N'Tiền mặt', N'Chưa thanh toán', N'Chờ khách trả tiền'),
('HD10', '2026-04-08 12:00:00', 'NV02', 'B03', 70000,  N'Chuyển khoản', N'Đã thanh toán', N'Khách quay lại');

-- 12. CHI TIET HOA DON
INSERT INTO ChiTietHoaDon (MaCTHD, MaHoaDon, MaMon, SoLuong, DonGia, GhiChu) VALUES
('CTHD01', 'HD01', 'M01', 1, 25000, N'Ít đá'),
('CTHD02', 'HD01', 'M02', 1, 30000, N'Bình thường'),
('CTHD03', 'HD02', 'M04', 2, 40000, N'50% đường'),
('CTHD04', 'HD03', 'M05', 1, 45000, N''),
('CTHD05', 'HD03', 'M09', 2, 38000, N''),
('CTHD06', 'HD04', 'M05', 1, 45000, N'Ít ngọt'),
('CTHD07', 'HD05', 'M10', 1, 47000, N''),
('CTHD08', 'HD05', 'M02', 1, 30000, N''),
('CTHD09', 'HD05', 'M01', 1, 25000, N''),
('CTHD10', 'HD06', 'M08', 1, 35000, N'Không đá'),
('CTHD11', 'HD07', 'M06', 2, 50000, N''),
('CTHD12', 'HD08', 'M09', 1, 38000, N''),
('CTHD13', 'HD08', 'M01', 1, 25000, N''),
('CTHD14', 'HD08', 'M02', 1, 30000, N''),
('CTHD15', 'HD09', 'M10', 1, 47000, N''),
('CTHD16', 'HD10', 'M03', 1, 32000, N''),
('CTHD17', 'HD10', 'M08', 1, 35000, N''),
('CTHD18', 'HD10', 'M01', 1, 25000, N''),
('CTHD19', 'HD02', 'M01', 1, 25000, N''),
('CTHD20', 'HD04', 'M09', 1, 38000, N'');
GO
