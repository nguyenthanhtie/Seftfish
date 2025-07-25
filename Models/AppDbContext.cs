using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Seftfish.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhSanPham> AnhSanPhams { get; set; }

    public virtual DbSet<BienTheSanPham> BienTheSanPhams { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DiaChi> DiaChis { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GiaTriThuocTinh> GiaTriThuocTinhs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThuocTinh> ThuocTinhs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // This method will be called only if no options are provided via DI
        // The connection string should be configured in Program.cs via DI
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhSanPham>(entity =>
        {
            entity.HasKey(e => e.IdAnh).HasName("PK__AnhSanPh__2A42605D335E175E");

            entity.ToTable("AnhSanPham");

            entity.Property(e => e.IdAnh).HasColumnName("ID_Anh");
            entity.Property(e => e.DuongDan).HasMaxLength(255);
            entity.Property(e => e.IdSanPham).HasColumnName("ID_SanPham");
            entity.Property(e => e.LoaiAnh).HasMaxLength(20);

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.AnhSanPhams)
                .HasForeignKey(d => d.IdSanPham)
                .HasConstraintName("FK__AnhSanPha__ID_Sa__49C3F6B7");
        });

        modelBuilder.Entity<BienTheSanPham>(entity =>
        {
            entity.HasKey(e => e.IdBienThe).HasName("PK__BienTheS__64C29E10302E03C3");

            entity.ToTable("BienTheSanPham");

            entity.HasIndex(e => e.Sku, "UQ__BienTheS__CA1ECF0D84A4016E").IsUnique();

            entity.Property(e => e.IdBienThe).HasColumnName("ID_BienThe");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IdSanPham).HasColumnName("ID_SanPham");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.SoLuongTonKho).HasDefaultValue(0);

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.BienTheSanPhams)
                .HasForeignKey(d => d.IdSanPham)
                .HasConstraintName("FK__BienTheSa__ID_Sa__534D60F1");

            entity.HasMany(d => d.IdGiaTris).WithMany(p => p.IdBienThes)
                .UsingEntity<Dictionary<string, object>>(
                    "ChiTietBienThe",
                    r => r.HasOne<GiaTriThuocTinh>().WithMany()
                        .HasForeignKey("IdGiaTri")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ChiTietBi__ID_Gi__5812160E"),
                    l => l.HasOne<BienTheSanPham>().WithMany()
                        .HasForeignKey("IdBienThe")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ChiTietBi__ID_Bi__571DF1D5"),
                    j =>
                    {
                        j.HasKey("IdBienThe", "IdGiaTri").HasName("PK__ChiTietB__DB9B181E6B3A9491");
                        j.ToTable("ChiTietBienThe");
                        j.IndexerProperty<int>("IdBienThe").HasColumnName("ID_BienThe");
                        j.IndexerProperty<int>("IdGiaTri").HasColumnName("ID_GiaTri");
                    });
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.IdChiTiet).HasName("PK__ChiTietD__1EF2F70590B908AA");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.IdChiTiet).HasColumnName("ID_ChiTiet");
            entity.Property(e => e.GiaLucDat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IdBienThe).HasColumnName("ID_BienThe");
            entity.Property(e => e.IdDonHang).HasColumnName("ID_DonHang");

            entity.HasOne(d => d.IdBienTheNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.IdBienThe)
                .HasConstraintName("FK__ChiTietDo__ID_Bi__693CA210");

            entity.HasOne(d => d.IdDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.IdDonHang)
                .HasConstraintName("FK__ChiTietDo__ID_Do__68487DD7");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.IdChiTiet).HasName("PK__ChiTietG__1EF2F7055ADC6BD9");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.IdChiTiet).HasColumnName("ID_ChiTiet");
            entity.Property(e => e.IdBienThe).HasColumnName("ID_BienThe");
            entity.Property(e => e.IdGioHang).HasColumnName("ID_GioHang");
            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.IdBienTheNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.IdBienThe)
                .HasConstraintName("FK__ChiTietGi__ID_Bi__5FB337D6");

            entity.HasOne(d => d.IdGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.IdGioHang)
                .HasConstraintName("FK__ChiTietGi__ID_Gi__5EBF139D");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.IdDanhGia).HasName("PK__DanhGia__6C898AE1613503E2");

            entity.Property(e => e.IdDanhGia).HasColumnName("ID_DanhGia");
            entity.Property(e => e.AnhDanhGia).HasMaxLength(255);
            entity.Property(e => e.BinhLuan).HasMaxLength(1000);
            entity.Property(e => e.IdSanPham).HasColumnName("ID_SanPham");
            entity.Property(e => e.IdTaiKhoan).HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdSanPhamNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.IdSanPham)
                .HasConstraintName("FK__DanhGia__ID_SanP__6D0D32F4");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK__DanhGia__ID_TaiK__6C190EBB");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.IdDanhMuc).HasName("PK__DanhMuc__662ACB014327F35E");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.IdDanhMuc).HasColumnName("ID_DanhMuc");
            entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
            entity.Property(e => e.DuongDanSeo)
                .HasMaxLength(255)
                .HasColumnName("DuongDanSEO");
            entity.Property(e => e.IdDanhMucCha).HasColumnName("ID_DanhMucCha");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);

            entity.HasOne(d => d.IdDanhMucChaNavigation).WithMany(p => p.InverseIdDanhMucChaNavigation)
                .HasForeignKey(d => d.IdDanhMucCha)
                .HasConstraintName("FK__DanhMuc__ID_Danh__412EB0B6");
        });

        modelBuilder.Entity<DiaChi>(entity =>
        {
            entity.HasKey(e => e.IdDiaChi).HasName("PK__DiaChi__C793F25250032D36");

            entity.ToTable("DiaChi");

            entity.Property(e => e.IdDiaChi).HasColumnName("ID_DiaChi");
            entity.Property(e => e.DiaChiChiTiet).HasMaxLength(255);
            entity.Property(e => e.HoTenNguoiNhan).HasMaxLength(100);
            entity.Property(e => e.IdTaiKhoan).HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.MacDinh).HasDefaultValue(false);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.DiaChis)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK__DiaChi__ID_TaiKh__3D5E1FD2");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.IdDonHang).HasName("PK__DonHang__99B72639FBBCB63B");

            entity.ToTable("DonHang");

            entity.Property(e => e.IdDonHang).HasColumnName("ID_DonHang");
            entity.Property(e => e.IdDiaChi).HasColumnName("ID_DiaChi");
            entity.Property(e => e.IdTaiKhoan).HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.IdDiaChiNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdDiaChi)
                .HasConstraintName("FK__DonHang__ID_DiaC__656C112C");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK__DonHang__ID_TaiK__6383C8BA");
        });

        modelBuilder.Entity<GiaTriThuocTinh>(entity =>
        {
            entity.HasKey(e => e.IdGiaTri).HasName("PK__GiaTriTh__F59860EA68760E35");

            entity.ToTable("GiaTriThuocTinh");

            entity.Property(e => e.IdGiaTri).HasColumnName("ID_GiaTri");
            entity.Property(e => e.GiaTri).HasMaxLength(100);
            entity.Property(e => e.IdThuocTinh).HasColumnName("ID_ThuocTinh");

            entity.HasOne(d => d.IdThuocTinhNavigation).WithMany(p => p.GiaTriThuocTinhs)
                .HasForeignKey(d => d.IdThuocTinh)
                .HasConstraintName("FK__GiaTriThu__ID_Th__4F7CD00D");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.IdGioHang).HasName("PK__GioHang__C033AA177DF2F20D");

            entity.ToTable("GioHang");

            entity.Property(e => e.IdGioHang).HasColumnName("ID_GioHang");
            entity.Property(e => e.IdTaiKhoan).HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.IdTaiKhoan)
                .HasConstraintName("FK__GioHang__ID_TaiK__5AEE82B9");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.IdSanPham).HasName("PK__SanPham__617EA3927B33296E");

            entity.ToTable("SanPham");

            entity.Property(e => e.IdSanPham).HasColumnName("ID_SanPham");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IdDanhMuc).HasColumnName("ID_DanhMuc");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenSanPham).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.IdDanhMucNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.IdDanhMuc)
                .HasConstraintName("FK__SanPham__ID_Danh__45F365D3");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.IdTaiKhoan).HasName("PK__TaiKhoan__0E3EC2107542413B");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D10534DC07DC52").IsUnique();

            entity.Property(e => e.IdTaiKhoan).HasColumnName("ID_TaiKhoan");
            entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        modelBuilder.Entity<ThuocTinh>(entity =>
        {
            entity.HasKey(e => e.IdThuocTinh).HasName("PK__ThuocTin__C49F0885BDDBEB68");

            entity.ToTable("ThuocTinh");

            entity.Property(e => e.IdThuocTinh).HasColumnName("ID_ThuocTinh");
            entity.Property(e => e.TenThuocTinh).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
