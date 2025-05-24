using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Models;

namespace QuanLyKhuDanCu.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets
        public DbSet<HoKhau> HoKhaus { get; set; }
        public DbSet<NhanKhau> NhanKhaus { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<YeuCauDichVu> YeuCauDichVus { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<PhanAnh> PhanAnhs { get; set; }
        public DbSet<LoaiPhi> LoaiPhis { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<TamTruTamVang> TamTruTamVangs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure PhanAnh entity to have nullable PhanHoi
            builder.Entity<PhanAnh>()
                .Property(p => p.PhanHoi)
                .IsRequired(false);
                
            // Configure TamTruTamVang entity to have nullable NguoiDuyetId
            builder.Entity<TamTruTamVang>()
                .Property(t => t.NguoiDuyetId)
                .IsRequired(false);

            // Configure YeuCauDichVu entity to have nullable NguoiXuLyId
            builder.Entity<YeuCauDichVu>(entity => 
            {
                entity.Property(y => y.NguoiXuLyId)
                    .IsRequired(false);
                    
                entity.HasOne(y => y.NguoiXuLy)
                    .WithMany()
                    .HasForeignKey(y => y.NguoiXuLyId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.YeuCauDichVus) 
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.DichVu)
                    .WithMany(d => d.YeuCauDichVus) 
                    .HasForeignKey(e => e.DichVuId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PhanAnh entity relationships
            builder.Entity<PhanAnh>(entity =>
            {
                entity.Property(p => p.NguoiXuLyId)
                    .IsRequired(false); 

                entity.Property(p => p.PhanHoi)
                    .IsRequired(false); 

                entity.HasOne(p => p.NguoiXuLy)
                    .WithMany() 
                    .HasForeignKey(p => p.NguoiXuLyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                    .WithMany(u => u.PhanAnhs) 
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); 
            });

            // Configure ThongBao entity
            builder.Entity<ThongBao>(entity =>
            {
                entity.Property(t => t.FileDinhKem)
                    .IsRequired(false); // Make FileDinhKem optional

                entity.HasOne(t => t.NguoiTao)
                    .WithMany(u => u.ThongBaos) // Assuming ApplicationUser has a ThongBaos collection
                    .HasForeignKey(t => t.NguoiTaoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
            
            // Configure relationships
            // HoKhau - ChuHo (ApplicationUser)
            builder.Entity<HoKhau>()
                .HasOne(h => h.ChuHo)
                .WithMany()
                .HasForeignKey(h => h.ChuHoId)
                .OnDelete(DeleteBehavior.Restrict);

            // NhanKhau - HoKhau
            builder.Entity<NhanKhau>()
                .HasOne(n => n.HoKhau)
                .WithMany(h => h.NhanKhaus)
                .HasForeignKey(n => n.HoKhauId)
                .OnDelete(DeleteBehavior.Cascade);

            // PhanAnh - User (ApplicationUser)
            builder.Entity<PhanAnh>()
                .HasOne(p => p.User)
                .WithMany(u => u.PhanAnhs)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PhanAnh - NguoiXuLy (ApplicationUser)
            builder.Entity<PhanAnh>()
                .HasOne(p => p.NguoiXuLy)
                .WithMany()
                .HasForeignKey(p => p.NguoiXuLyId)
                .OnDelete(DeleteBehavior.Restrict);

            // YeuCauDichVu - User (ApplicationUser)
            builder.Entity<YeuCauDichVu>()
                .HasOne(y => y.User)
                .WithMany(u => u.YeuCauDichVus)
                .HasForeignKey(y => y.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // YeuCauDichVu - NguoiXuLy (ApplicationUser)
            builder.Entity<YeuCauDichVu>()
                .HasOne(y => y.NguoiXuLy)
                .WithMany()
                .HasForeignKey(y => y.NguoiXuLyId)
                .OnDelete(DeleteBehavior.Restrict);

            // YeuCauDichVu - DichVu
            builder.Entity<YeuCauDichVu>()
                .HasOne(y => y.DichVu)
                .WithMany(d => d.YeuCauDichVus)
                .HasForeignKey(y => y.DichVuId)
                .OnDelete(DeleteBehavior.Restrict);

            // ThongBao - NguoiTao (ApplicationUser)
            builder.Entity<ThongBao>()
                .HasOne(t => t.NguoiTao)
                .WithMany(u => u.ThongBaos)
                .HasForeignKey(t => t.NguoiTaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // HoaDon - HoKhau
            builder.Entity<HoaDon>()
                .HasOne(h => h.HoKhau)
                .WithMany(hk => hk.HoaDons)
                .HasForeignKey(h => h.HoKhauId)
                .OnDelete(DeleteBehavior.Cascade);

            // HoaDon - LoaiPhi
            builder.Entity<HoaDon>()
                .HasOne(h => h.LoaiPhi)
                .WithMany(l => l.HoaDons)
                .HasForeignKey(h => h.LoaiPhiId)
                .OnDelete(DeleteBehavior.Restrict);

            // HoaDon - NguoiThu (ApplicationUser)
            builder.Entity<HoaDon>()
                .HasOne(h => h.NguoiThu)
                .WithMany()
                .HasForeignKey(h => h.NguoiThuId)
                .OnDelete(DeleteBehavior.Restrict);

            // TamTruTamVang - NhanKhau
            builder.Entity<TamTruTamVang>()
                .HasOne(t => t.NhanKhau)
                .WithMany(n => n.TamTruTamVangs)
                .HasForeignKey(t => t.NhanKhauId)
                .OnDelete(DeleteBehavior.Cascade);

            // TamTruTamVang - NguoiDuyet (ApplicationUser)
            builder.Entity<TamTruTamVang>()
                .HasOne(t => t.NguoiDuyet)
                .WithMany()
                .HasForeignKey(t => t.NguoiDuyetId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
