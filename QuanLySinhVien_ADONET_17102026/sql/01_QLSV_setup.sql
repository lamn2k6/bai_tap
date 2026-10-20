-- Script minh họa: CHỈ tạo bảng nếu chưa tồn tại.
-- Nếu database của bạn đã có LopHoc/SinhVien, hãy kiểm tra tên và kiểu cột
-- trước khi sử dụng code. Script này KHÔNG xóa dữ liệu cũ.
IF DB_ID(N'QLSV') IS NULL
    CREATE DATABASE QLSV;
GO
USE QLSV;
GO
IF OBJECT_ID(N'dbo.LopHoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LopHoc
    (
        MaLop VARCHAR(20) NOT NULL PRIMARY KEY,
        TenLop NVARCHAR(100) NOT NULL
    );
END;
GO
IF OBJECT_ID(N'dbo.SinhVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SinhVien
    (
        MaSV VARCHAR(20) NOT NULL PRIMARY KEY,
        MaLop VARCHAR(20) NOT NULL,
        HoTen NVARCHAR(100) NOT NULL,
        GioiTinh NVARCHAR(10) NOT NULL,
        NgaySinh DATE NOT NULL,
        Diem FLOAT NOT NULL,
        SoDienThoai VARCHAR(20) NOT NULL,
        Email VARCHAR(150) NOT NULL,
        CONSTRAINT FK_SinhVien_LopHoc FOREIGN KEY (MaLop)
            REFERENCES dbo.LopHoc(MaLop),
        CONSTRAINT CK_SinhVien_Diem CHECK (Diem BETWEEN 0 AND 10)
    );
END;
GO
-- Dữ liệu mẫu, không trùng khóa nếu chạy lại
IF NOT EXISTS (SELECT 1 FROM dbo.LopHoc WHERE MaLop = 'L01')
    INSERT INTO dbo.LopHoc (MaLop, TenLop) VALUES ('L01', N'Công nghệ thông tin 1');
IF NOT EXISTS (SELECT 1 FROM dbo.LopHoc WHERE MaLop = 'L02')
    INSERT INTO dbo.LopHoc (MaLop, TenLop) VALUES ('L02', N'Công nghệ thông tin 2');
GO
IF NOT EXISTS (SELECT 1 FROM dbo.SinhVien WHERE MaSV = 'SV001')
    INSERT INTO dbo.SinhVien
        (MaSV, MaLop, HoTen, GioiTinh, NgaySinh, Diem, SoDienThoai, Email)
    VALUES
        ('SV001', 'L01', N'Nguyễn Văn An', N'Nam', '2005-06-15', 8.5, '0912345678', 'an@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.SinhVien WHERE MaSV = 'SV002')
    INSERT INTO dbo.SinhVien
        (MaSV, MaLop, HoTen, GioiTinh, NgaySinh, Diem, SoDienThoai, Email)
    VALUES
        ('SV002', 'L02', N'Trần Thị Bình', N'Nữ', '2005-10-20', 9.0, '0987654321', 'binh@example.com');
GO
SELECT * FROM dbo.LopHoc;
SELECT * FROM dbo.SinhVien;
