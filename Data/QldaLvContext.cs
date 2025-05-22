using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_UML.Data;

public partial class QldaLvContext : DbContext
{
    public QldaLvContext()
    {
    }

    public QldaLvContext(DbContextOptions<QldaLvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaoCaoTienDo> BaoCaoTienDos { get; set; }

    public virtual DbSet<DeCuongKeHoach> DeCuongKeHoaches { get; set; }

    public virtual DbSet<DeTai> DeTais { get; set; }

    public virtual DbSet<GiangVien> GiangViens { get; set; }

    public virtual DbSet<HoiDongBaoVe> HoiDongBaoVes { get; set; }

    public virtual DbSet<KetQua> KetQuas { get; set; }

    public virtual DbSet<KiBaoVeDoAn> KiBaoVeDoAns { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<LuanVan> LuanVans { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<PhanCongGiangVien> PhanCongGiangViens { get; set; }

    public virtual DbSet<QuanTriVien> QuanTriViens { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<ThanhVienHoiDong> ThanhVienHoiDongs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=THUAN-PHAM\\SQL2022;Initial Catalog=QLDA_LV;Persist Security Info=True;User ID=sa;Password=thuan23082004;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaoCaoTienDo>(entity =>
        {
            entity.HasKey(e => e.MaBctd).HasName("PK__BaoCaoTi__88536869412C6EB8");

            entity.ToTable("BaoCaoTienDo");

            entity.Property(e => e.MaBctd)
                .HasMaxLength(20)
                .HasColumnName("MaBCTD");
            entity.Property(e => e.MaDeTai).HasMaxLength(20);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NhanXet).HasMaxLength(255);
            entity.Property(e => e.NoiDungBaoCao).HasMaxLength(255);
            entity.Property(e => e.TepTinNop).HasMaxLength(255);

            entity.HasOne(d => d.MaDeTaiNavigation).WithMany(p => p.BaoCaoTienDos)
                .HasForeignKey(d => d.MaDeTai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BaoCaoTie__MaDeT__50E5F592");
        });

        modelBuilder.Entity<DeCuongKeHoach>(entity =>
        {
            entity.HasKey(e => e.MaDeCuong).HasName("PK__DeCuongK__A38A0F3280767FB1");

            entity.ToTable("DeCuongKeHoach");

            entity.Property(e => e.MaDeCuong).HasMaxLength(20);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.KeHoachThucHien).HasMaxLength(255);
            entity.Property(e => e.MaDeTai).HasMaxLength(20);
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDungDeCuong).HasMaxLength(255);

            entity.HasOne(d => d.MaDeTaiNavigation).WithMany(p => p.DeCuongKeHoaches)
                .HasForeignKey(d => d.MaDeTai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeCuongKe__MaDeT__4D1564AE");
        });

        modelBuilder.Entity<DeTai>(entity =>
        {
            entity.HasKey(e => e.MaDeTai).HasName("PK__DeTai__9F967D5B66E467DE");

            entity.ToTable("DeTai");

            entity.Property(e => e.MaDeTai).HasMaxLength(20);
            entity.Property(e => e.MaKi).HasMaxLength(20);
            entity.Property(e => e.MaSinhVien).HasMaxLength(20);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenDeTai).HasMaxLength(200);
            entity.Property(e => e.TrangThaiDuyet)
                .HasMaxLength(20)
                .HasDefaultValue("Ch? duy?t");

            entity.HasOne(d => d.MaKiNavigation).WithMany(p => p.DeTais)
                .HasForeignKey(d => d.MaKi)
                .HasConstraintName("FK__DeTai__MaKi__44801EAD");

            entity.HasOne(d => d.MaSinhVienNavigation).WithMany(p => p.DeTais)
                .HasForeignKey(d => d.MaSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeTai__MaSinhVie__438BFA74");
        });

        modelBuilder.Entity<GiangVien>(entity =>
        {
            entity.HasKey(e => e.MaGiangVien).HasName("PK__GiangVie__C03BEEBA7A1299FD");

            entity.ToTable("GiangVien");

            entity.HasIndex(e => e.MaNguoiDung, "UQ__GiangVie__C539D763E94613FE").IsUnique();

            entity.Property(e => e.MaGiangVien).HasMaxLength(20);
            entity.Property(e => e.ChucVu).HasMaxLength(30);
            entity.Property(e => e.ChuyenMon).HasMaxLength(100);
            entity.Property(e => e.HocVi).HasMaxLength(100);
            entity.Property(e => e.MaNguoiDung).HasMaxLength(20);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.GiangVien)
                .HasForeignKey<GiangVien>(d => d.MaNguoiDung)
                .HasConstraintName("FK__GiangVien__MaNgu__32616E72");
        });

        modelBuilder.Entity<HoiDongBaoVe>(entity =>
        {
            entity.HasKey(e => e.MaHoiDong).HasName("PK__HoiDongB__998808B32F781F6C");

            entity.ToTable("HoiDongBaoVe");

            entity.Property(e => e.MaHoiDong).HasMaxLength(20);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayThanhLap).HasColumnType("datetime");
            entity.Property(e => e.TenHoiDong).HasMaxLength(100);
        });

        modelBuilder.Entity<KetQua>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQua__D5B3102AA8F8BEF5");

            entity.ToTable("KetQua", tb => tb.HasTrigger("trg_CapNhatTrangThaiLuanVan"));

            entity.Property(e => e.MaKetQua).HasMaxLength(20);
            entity.Property(e => e.DiemBaoVe).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.DiemQuaTrinh).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.MaDeTai).HasMaxLength(20);
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NhanXet).HasMaxLength(255);

            entity.HasOne(d => d.MaDeTaiNavigation).WithMany(p => p.KetQuas)
                .HasForeignKey(d => d.MaDeTai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KetQua__MaDeTai__5D4BCC77");
        });

        modelBuilder.Entity<KiBaoVeDoAn>(entity =>
        {
            entity.HasKey(e => e.MaKi).HasName("PK__KiBaoVeD__2725CF795317C7CB");

            entity.ToTable("KiBaoVeDoAn");

            entity.Property(e => e.MaKi).HasMaxLength(20);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.HocKy).HasMaxLength(100);
            entity.Property(e => e.NamHoc).HasMaxLength(100);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TenKi).HasMaxLength(100);
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.MaLop).HasName("PK__LopHoc__3B98D27303D785CB");

            entity.ToTable("LopHoc");

            entity.Property(e => e.MaLop).HasMaxLength(20);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.Khoa).HasMaxLength(100);
            entity.Property(e => e.MaGiangVien).HasMaxLength(20);
            entity.Property(e => e.NienKhoa).HasMaxLength(20);
            entity.Property(e => e.TenLop).HasMaxLength(100);

            entity.HasOne(d => d.MaGiangVienNavigation).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.MaGiangVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc__MaGiangV__353DDB1D");
        });

        modelBuilder.Entity<LuanVan>(entity =>
        {
            entity.HasKey(e => e.MaLuanVan).HasName("PK__LuanVan__E68DF2C0BBE79B58");

            entity.ToTable("LuanVan");

            entity.Property(e => e.MaLuanVan).HasMaxLength(20);
            entity.Property(e => e.MaDeTai).HasMaxLength(20);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.NgayDuyet).HasColumnType("datetime");
            entity.Property(e => e.NgayNop).HasColumnType("datetime");
            entity.Property(e => e.TepTinNop).HasMaxLength(255);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đang bảo vệ");
            entity.Property(e => e.TrangThaiDuyet)
                .HasMaxLength(20)
                .HasDefaultValue("Chờ duyệt");
            entity.Property(e => e.XacNhanBaoVe)
                .HasMaxLength(20)
                .HasDefaultValue("Chờ duyệt");

            entity.HasOne(d => d.MaDeTaiNavigation).WithMany(p => p.LuanVans)
                .HasForeignKey(d => d.MaDeTai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LuanVan__MaDeTai__5792F321");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D7623DDA3C8A");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.SoDienThoai, "UQ__NguoiDun__0389B7BD527B67F8").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "UQ__NguoiDun__55F68FC0DF1FCB3E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534898BB5C3").IsUnique();

            entity.HasIndex(e => e.SoCmnd, "UQ__NguoiDun__F5EEA1C6E20A39E1").IsUnique();

            entity.Property(e => e.MaNguoiDung).HasMaxLength(20);
            entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.QueQuan).HasMaxLength(255);
            entity.Property(e => e.SoCmnd)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("SoCMND");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        modelBuilder.Entity<PhanCongGiangVien>(entity =>
        {
            entity.HasKey(e => new { e.MaDeTai, e.MaGiangVien, e.VaiTro }).HasName("PK__PhanCong__07DFDE28555E9CE1");

            entity.ToTable("PhanCongGiangVien");

            entity.Property(e => e.MaDeTai).HasMaxLength(20);
            entity.Property(e => e.MaGiangVien).HasMaxLength(20);
            entity.Property(e => e.VaiTro).HasMaxLength(20);

            entity.HasOne(d => d.MaDeTaiNavigation).WithMany(p => p.PhanCongGiangViens)
                .HasForeignKey(d => d.MaDeTai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanCongG__MaDeT__4850AF91");

            entity.HasOne(d => d.MaGiangVienNavigation).WithMany(p => p.PhanCongGiangViens)
                .HasForeignKey(d => d.MaGiangVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanCongG__MaGia__4944D3CA");
        });

        modelBuilder.Entity<QuanTriVien>(entity =>
        {
            entity.HasKey(e => e.MaQuanTri).HasName("PK__QuanTriV__05FA934860067E9D");

            entity.ToTable("QuanTriVien");

            entity.HasIndex(e => e.MaNguoiDung, "UQ__QuanTriV__C539D7630E4A5B95").IsUnique();

            entity.Property(e => e.MaQuanTri).HasMaxLength(20);
            entity.Property(e => e.MaNguoiDung).HasMaxLength(20);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.QuanTriVien)
                .HasForeignKey<QuanTriVien>(d => d.MaNguoiDung)
                .HasConstraintName("FK__QuanTriVi__MaNgu__2E90DD8E");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.MaSinhVien).HasName("PK__SinhVien__939AE775461B8EAA");

            entity.ToTable("SinhVien");

            entity.HasIndex(e => e.MaNguoiDung, "UQ__SinhVien__C539D76389C57656").IsUnique();

            entity.Property(e => e.MaSinhVien).HasMaxLength(20);
            entity.Property(e => e.MaLop).HasMaxLength(20);
            entity.Property(e => e.MaNguoiDung).HasMaxLength(20);

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SinhVien__MaLop__3AF6B473");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.SinhVien)
                .HasForeignKey<SinhVien>(d => d.MaNguoiDung)
                .HasConstraintName("FK__SinhVien__MaNguo__3A02903A");
        });

        modelBuilder.Entity<ThanhVienHoiDong>(entity =>
        {
            entity.HasKey(e => new { e.MaHoiDong, e.MaGiangVien }).HasName("PK__ThanhVie__258BB658C7C1C2CC");

            entity.ToTable("ThanhVienHoiDong");

            entity.Property(e => e.MaHoiDong).HasMaxLength(20);
            entity.Property(e => e.MaGiangVien).HasMaxLength(20);
            entity.Property(e => e.VaiTro).HasMaxLength(20);

            entity.HasOne(d => d.MaGiangVienNavigation).WithMany(p => p.ThanhVienHoiDongs)
                .HasForeignKey(d => d.MaGiangVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhVien__MaGia__63F8CA06");

            entity.HasOne(d => d.MaHoiDongNavigation).WithMany(p => p.ThanhVienHoiDongs)
                .HasForeignKey(d => d.MaHoiDong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhVien__MaHoi__6304A5CD");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
