// File: Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Models.Entities;

namespace PostgraduateManagement.Data
{
    /// <summary>
    /// Context truy cập cơ sở dữ liệu cho Module M05 – Quản lý Sau đại học.
    /// Kế thừa IdentityDbContext để quản lý tập trung xác thực và phân quyền người dùng.
    /// </summary>
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Bảng dữ liệu người học sau đại học
        /// </summary>
        public DbSet<PostgraduateStudent> PostgraduateStudents { get; set; } = null!;

        /// <summary>
        /// Bảng dữ liệu cán bộ, giảng viên, chuyên gia khoa học
        /// </summary>
        public DbSet<AcademicPerson> AcademicPersons { get; set; } = null!;

        /// <summary>
        /// Bảng dữ liệu đề tài sau đại học (Luận văn, Luận án, Đề án)
        /// </summary>
        public DbSet<Thesis> Theses { get; set; } = null!;

        /// <summary>
        /// Bảng phân công chuyên môn (Người hướng dẫn, Phản biện, Hội đồng)
        /// </summary>
        public DbSet<ThesisAssignment> ThesisAssignments { get; set; } = null!;

        /// <summary>
        /// Bảng thẩm định điều kiện quy chuẩn đào tạo
        /// </summary>
        public DbSet<EligibilityCheck> EligibilityChecks { get; set; } = null!;

        /// <summary>
        /// Bảng dữ liệu phiên bảo vệ đề tài
        /// </summary>
        public DbSet<DefenseSession> DefenseSessions { get; set; } = null!;

        /// <summary>
        /// Bảng kết quả và biên bản đánh giá phiên bảo vệ
        /// </summary>
        public DbSet<DefenseResult> DefenseResults { get; set; } = null!;

        /// <summary>
        /// Bảng lưu trữ và quản lý tài liệu số hóa nộp lưu chiểu M05
        /// </summary>
        public DbSet<SubmittedDocument> SubmittedDocuments { get; set; } = null!;

        /// <summary>
        /// Bảng lưu trữ vết kiểm toán bảo mật và thao tác hệ thống
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bắt buộc gọi phương thức cấu hình gốc cho Identity
            base.OnModelCreating(builder);

            // 1. Cấu hình bảng PostgraduateStudent
            builder.Entity<PostgraduateStudent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SharedStudentCode).IsUnique();
                entity.Property(e => e.SharedStudentCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            });

            // 2. Cấu hình bảng AcademicPerson
            builder.Entity<AcademicPerson>(entity =>
            {
                entity.HasKey(e => e.Id);
                var filter = Database.IsSqlite() ? "SharedPersonCode IS NOT NULL" : "[SharedPersonCode] IS NOT NULL";
                entity.HasIndex(e => e.SharedPersonCode).IsUnique().HasFilter(filter);
                entity.HasIndex(e => e.ResearchField);
                entity.HasIndex(e => e.IsExternal);
                entity.Property(e => e.SharedPersonCode).HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ResearchField).IsRequired().HasMaxLength(300);
            });

            // 3. Cấu hình bảng Thesis
            builder.Entity<Thesis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.ThesisStatus);
                entity.HasIndex(e => e.IsGraduationCapstone);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Field).IsRequired().HasMaxLength(250);

                // Quan hệ với người học: Không xóa đề tài khi xóa người học
                entity.HasOne(t => t.Student)
                      .WithMany(s => s.Theses)
                      .HasForeignKey(t => t.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 4. Cấu hình bảng ThesisAssignment
            builder.Entity<ThesisAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ThesisId, e.PersonId, e.Role });

                // Xóa đề tài thì xóa phân công liên quan
                entity.HasOne(a => a.Thesis)
                      .WithMany(t => t.Assignments)
                      .HasForeignKey(a => a.ThesisId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Không tự động xóa cán bộ khi xóa phân công (tránh multiple cascade paths trong SQL Server)
                entity.HasOne(a => a.Person)
                      .WithMany(p => p.Assignments)
                      .HasForeignKey(a => a.PersonId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. Cấu hình bảng EligibilityCheck
            builder.Entity<EligibilityCheck>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Basis).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RegulationVersion).IsRequired().HasMaxLength(100);

                // Xóa đề tài thì xóa lịch sử kiểm tra điều kiện của đề tài
                entity.HasOne(ec => ec.Thesis)
                      .WithMany(t => t.EligibilityChecks)
                      .HasForeignKey(ec => ec.ThesisId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Không xóa cán bộ khi xóa bản ghi thẩm định (tránh multiple cascade paths)
                entity.HasOne(ec => ec.Person)
                      .WithMany(p => p.EligibilityChecks)
                      .HasForeignKey(ec => ec.PersonId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 6. Cấu hình bảng DefenseSession
            builder.Entity<DefenseSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);

                // Quan hệ 1-Nhiều giữa Thesis và DefenseSession
                entity.HasOne(ds => ds.Thesis)
                      .WithMany(t => t.DefenseSessions)
                      .HasForeignKey(ds => ds.ThesisId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 7. Cấu hình bảng DefenseResult
            builder.Entity<DefenseResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.Property(e => e.Comments).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.RecordedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Score).HasPrecision(4, 2);

                // Quan hệ 1-1 giữa DefenseSession và DefenseResult
                entity.HasOne(dr => dr.Session)
                      .WithOne(ds => ds.DefenseResult)
                      .HasForeignKey<DefenseResult>(dr => dr.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. Cấu hình bảng SubmittedDocument
            builder.Entity<SubmittedDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ThesisId, e.DocumentType });
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Sha256Hash).HasMaxLength(64);

                // Xóa đề tài thì xóa toàn bộ tài liệu nộp liên quan
                entity.HasOne(sd => sd.Thesis)
                      .WithMany(t => t.SubmittedDocuments)
                      .HasForeignKey(sd => sd.ThesisId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 9. Cấu hình bảng AuditLog
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Timestamp);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(4000);
            });
        }
    }
}
