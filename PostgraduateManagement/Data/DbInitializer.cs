// File: Data/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Data
{
    /// <summary>
    /// Khởi tạo và nạp dữ liệu mẫu phong phú phục vụ showcase Module M05 – Quản lý Sau đại học.
    /// Bao gồm phân quyền Identity, người học, cán bộ/chuyên gia nội bộ & ngoài trường,
    /// đề tài tốt nghiệp, đề tài NCKH, phân công, thẩm định điều kiện, bảo vệ và tài liệu số hóa.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Khởi tạo cơ sở dữ liệu và nạp dữ liệu ban đầu
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Tạo và cập nhật cơ sở dữ liệu nếu chưa tồn tại
            await context.Database.EnsureCreatedAsync();

            // 2. Khởi tạo các vai trò Identity (Roles)
            string[] roles = { "Admin", "Manager", "Faculty", "Student", "Committee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Khởi tạo tài khoản mẫu (Identity Users)
            await SeedUsersAsync(userManager, scope.ServiceProvider.GetRequiredService<IConfiguration>()["Seed:DefaultPassword"] ?? "Password123!");

            // 4. Nếu đã có dữ liệu học viên thì không nạp lại
            if (await context.PostgraduateStudents.AnyAsync())
            {
                return;
            }

            // 5. Nạp danh sách người học sau đại học (5 Postgraduate Students)
            var students = new List<PostgraduateStudent>
            {
                new PostgraduateStudent
                {
                    SharedStudentCode = "HV2024-001",
                    FullName = "Nguyễn Văn An",
                    Email = "an.nv@student.lms.edu.vn",
                    PhoneNumber = "0901234001",
                    Major = "Khoa học máy tính",
                    EnrollmentYear = 2024
                },
                new PostgraduateStudent
                {
                    SharedStudentCode = "HV2024-002",
                    FullName = "Trần Thị Bích",
                    Email = "bich.tt@student.lms.edu.vn",
                    PhoneNumber = "0901234002",
                    Major = "Hệ thống thông tin",
                    EnrollmentYear = 2024
                },
                new PostgraduateStudent
                {
                    SharedStudentCode = "HV2023-003",
                    FullName = "Lê Hoàng Nam",
                    Email = "nam.lh@student.lms.edu.vn",
                    PhoneNumber = "0901234003",
                    Major = "Khoa học máy tính",
                    EnrollmentYear = 2023
                },
                new PostgraduateStudent
                {
                    SharedStudentCode = "HV2024-004",
                    FullName = "Phạm Minh Đức",
                    Email = "duc.pm@student.lms.edu.vn",
                    PhoneNumber = "0901234004",
                    Major = "Kỹ thuật phần mềm",
                    EnrollmentYear = 2024
                },
                new PostgraduateStudent
                {
                    SharedStudentCode = "HV2025-005",
                    FullName = "Đỗ Thu Hà",
                    Email = "ha.dt@student.lms.edu.vn",
                    PhoneNumber = "0901234005",
                    Major = "An toàn thông tin",
                    EnrollmentYear = 2025
                }
            };
            await context.PostgraduateStudents.AddRangeAsync(students);
            await context.SaveChangesAsync();

            // 6. Nạp danh sách cán bộ / chuyên gia (6 Academic Persons: GS, PGS, TS; Nội bộ & Ngoài trường)
            var persons = new List<AcademicPerson>
            {
                new AcademicPerson
                {
                    SharedPersonCode = "CB-001",
                    FullName = "GS.TS. Nguyễn Thanh Tùng",
                    AcademicDegree = AcademicDegree.GS,
                    ResearchField = "Khoa học máy tính & Trí tuệ nhân tạo",
                    IsExternal = false,
                    Email = "tung.nt@lms.edu.vn",
                    PhoneNumber = "0912345001",
                    Workplace = "Khoa Sau đại học & CNTT"
                },
                new AcademicPerson
                {
                    SharedPersonCode = "CB-002",
                    FullName = "PGS.TS. Trần Đình Quế",
                    AcademicDegree = AcademicDegree.PGS,
                    ResearchField = "Hệ thống thông tin & Cơ sở dữ liệu",
                    IsExternal = false,
                    Email = "que.td@lms.edu.vn",
                    PhoneNumber = "0912345002",
                    Workplace = "Khoa Hệ thống thông tin"
                },
                new AcademicPerson
                {
                    SharedPersonCode = "CB-003",
                    FullName = "TS. Lê Quang Dũng",
                    AcademicDegree = AcademicDegree.TS,
                    ResearchField = "Kỹ thuật phần mềm & Kiến trúc hệ thống",
                    IsExternal = false,
                    Email = "dung.lq@lms.edu.vn",
                    PhoneNumber = "0912345003",
                    Workplace = "Bộ môn Công nghệ phần mềm"
                },
                new AcademicPerson
                {
                    SharedPersonCode = "CB-004",
                    FullName = "GS.TS. Hoàng Minh Sơn",
                    AcademicDegree = AcademicDegree.GS,
                    ResearchField = "Khoa học máy tính & Tối ưu hóa",
                    IsExternal = true,
                    Email = "son.hm@hust.edu.vn",
                    PhoneNumber = "0912345004",
                    Workplace = "Đại học Bách Khoa Hà Nội"
                },
                new AcademicPerson
                {
                    SharedPersonCode = "CB-005",
                    FullName = "PGS.TS. Đỗ Bá Long",
                    AcademicDegree = AcademicDegree.PGS,
                    ResearchField = "Trí tuệ nhân tạo & Xử lý ngôn ngữ tự nhiên",
                    IsExternal = true,
                    Email = "long.db@vast.ac.vn",
                    PhoneNumber = "0912345005",
                    Workplace = "Viện Hàn lâm Khoa học và Công nghệ Việt Nam"
                },
                new AcademicPerson
                {
                    SharedPersonCode = "CB-006",
                    FullName = "TS. Ngô Quốc Hưng",
                    AcademicDegree = AcademicDegree.TS,
                    ResearchField = "An toàn thông tin & Mạng máy tính",
                    IsExternal = true,
                    Email = "hung.nq@techcorp.vn",
                    PhoneNumber = "0912345006",
                    Workplace = "Tập đoàn Công nghệ AI & An ninh mạng"
                }
            };
            await context.AcademicPersons.AddRangeAsync(persons);
            await context.SaveChangesAsync();

            // 7. Nạp danh sách đề tài (5 Theses: 4 Capstone [IsGraduationCapstone = true], 1 NCKH [IsGraduationCapstone = false])
            var theses = new List<Thesis>
            {
                new Thesis
                {
                    Code = "LA-2023-001",
                    Title = "Nghiên cứu mô hình học sâu tối ưu trong nhận dạng tiếng nói tiếng Việt",
                    ThesisType = ThesisType.LuanAn,
                    Field = "Khoa học máy tính & Trí tuệ nhân tạo",
                    StartDate = new DateTime(2023, 10, 1),
                    EndDate = new DateTime(2026, 10, 1),
                    ThesisStatus = ThesisStatus.SubmittedForReview,
                    StudentId = students[2].Id, // Lê Hoàng Nam
                    IsGraduationCapstone = true,
                    Abstract = "Nghiên cứu các kiến trúc mạng Transformer và mô hình khuếch tán nhằm cải thiện độ chính xác nhận dạng tiếng nói tiếng Việt trong môi trường nhiều nhiễu."
                },
                new Thesis
                {
                    Code = "LV-2024-002",
                    Title = "Xây dựng hệ thống phát hiện gian lận trong giao dịch tài chính thời gian thực",
                    ThesisType = ThesisType.LuanVan,
                    Field = "Hệ thống thông tin & Cơ sở dữ liệu",
                    StartDate = new DateTime(2024, 2, 1),
                    EndDate = new DateTime(2025, 2, 1),
                    ThesisStatus = ThesisStatus.Completed,
                    StudentId = students[1].Id, // Trần Thị Bích
                    IsGraduationCapstone = true,
                    Abstract = "Ứng dụng kiến trúc xử lý dòng sự kiện kết hợp mô hình Graph Neural Network để phát hiện các giao dịch đáng ngờ trong ngân hàng số."
                },
                new Thesis
                {
                    Code = "DA-2024-003",
                    Title = "Phát triển nền tảng học tập trực tuyến thông minh Microservices",
                    ThesisType = ThesisType.DeAn,
                    Field = "Kỹ thuật phần mềm & Kiến trúc hệ thống",
                    StartDate = new DateTime(2024, 3, 1),
                    EndDate = new DateTime(2025, 3, 1),
                    ThesisStatus = ThesisStatus.InProgress,
                    StudentId = students[3].Id, // Phạm Minh Đức
                    IsGraduationCapstone = true,
                    Abstract = "Thiết kế và triển khai giải pháp LMS phân tán chịu tải cao, tích hợp công cụ phân tích hành vi học tập và gợi ý bài giảng cá nhân hóa."
                },
                new Thesis
                {
                    Code = "LV-2024-004",
                    Title = "Ứng dụng thị giác máy tính và học sâu trong chẩn đoán ảnh y tế",
                    ThesisType = ThesisType.LuanVan,
                    Field = "Khoa học máy tính & Trí tuệ nhân tạo",
                    StartDate = new DateTime(2024, 4, 1),
                    EndDate = new DateTime(2025, 4, 1),
                    ThesisStatus = ThesisStatus.Approved,
                    StudentId = students[0].Id, // Nguyễn Văn An
                    IsGraduationCapstone = true,
                    Abstract = "Phân đoạn tự động tổn thương phổi trên ảnh chụp cắt lớp vi tính bằng mô hình U-Net đa thang đo kết hợp cơ chế chú ý."
                },
                new Thesis
                {
                    Code = "NCKH-2026-01",
                    Title = "Nghiên cứu giải pháp bảo mật dữ liệu phân tán trên mạng Blockchain",
                    ThesisType = ThesisType.DeAn,
                    Field = "An toàn thông tin & Mạng máy tính",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2026, 1, 1),
                    ThesisStatus = ThesisStatus.Registered,
                    StudentId = students[4].Id, // Đỗ Thu Hà
                    IsGraduationCapstone = false, // Đề tài NCKH, chặn bảo vệ M05
                    Abstract = "Khảo sát và đánh giá hiệu năng các thuật toán đồng thuận bảo mật quyền riêng tư trên sổ cái phân tán."
                }
            };
            await context.Theses.AddRangeAsync(theses);
            await context.SaveChangesAsync();

            // 8. Nạp danh sách phân công chuyên gia (Thesis Assignments)
            var assignments = new List<ThesisAssignment>
            {
                // Đề tài 1: LA-2023-001 (Tiến sĩ)
                new ThesisAssignment
                {
                    ThesisId = theses[0].Id,
                    PersonId = persons[0].Id, // GS.TS. Nguyễn Thanh Tùng
                    Role = AssignmentRole.Supervisor,
                    IsExternal = false,
                    AssignedAt = new DateTime(2023, 10, 15),
                    Notes = "Quyết định giao đề tài và cử người hướng dẫn chính số 128/QĐ-SĐH"
                },
                new ThesisAssignment
                {
                    ThesisId = theses[0].Id,
                    PersonId = persons[4].Id, // PGS.TS. Đỗ Bá Long (Ngoài trường)
                    Role = AssignmentRole.Reviewer,
                    IsExternal = true,
                    AssignedAt = new DateTime(2026, 9, 1),
                    Notes = "Phân công phản biện độc lập ngoài cơ sở đào tạo"
                },
                new ThesisAssignment
                {
                    ThesisId = theses[0].Id,
                    PersonId = persons[3].Id, // GS.TS. Hoàng Minh Sơn (Ngoài trường)
                    Role = AssignmentRole.Committee,
                    CommitteePosition = CommitteePosition.Chair,
                    IsExternal = true,
                    AssignedAt = new DateTime(2026, 10, 1),
                    Notes = "Chủ tịch Hội đồng đánh giá Luận án Tiến sĩ"
                },

                // Đề tài 2: LV-2024-002 (Luận văn thạc sĩ đã bảo vệ)
                new ThesisAssignment
                {
                    ThesisId = theses[1].Id,
                    PersonId = persons[1].Id, // PGS.TS. Trần Đình Quế
                    Role = AssignmentRole.Supervisor,
                    IsExternal = false,
                    AssignedAt = new DateTime(2024, 2, 10),
                    Notes = "Quyết định giao đề tài số 45/QĐ-SĐH"
                },
                new ThesisAssignment
                {
                    ThesisId = theses[1].Id,
                    PersonId = persons[2].Id, // TS. Lê Quang Dũng
                    Role = AssignmentRole.Reviewer,
                    IsExternal = false,
                    AssignedAt = new DateTime(2025, 1, 5),
                    Notes = "Cán bộ phản biện luận văn"
                },
                new ThesisAssignment
                {
                    ThesisId = theses[1].Id,
                    PersonId = persons[5].Id, // TS. Ngô Quốc Hưng (Ngoài trường)
                    Role = AssignmentRole.Committee,
                    CommitteePosition = CommitteePosition.Chair,
                    IsExternal = true,
                    AssignedAt = new DateTime(2025, 1, 15),
                    Notes = "Ủy viên Hội đồng ngoài cơ sở đào tạo"
                },

                // Đề tài 3: DA-2024-003
                new ThesisAssignment
                {
                    ThesisId = theses[2].Id,
                    PersonId = persons[2].Id, // TS. Lê Quang Dũng
                    Role = AssignmentRole.Supervisor,
                    IsExternal = false,
                    AssignedAt = new DateTime(2024, 3, 15),
                    Notes = "Quyết định phân công hướng dẫn đề án ứng dụng số 89/QĐ-SĐH"
                },

                // Đề tài 4: LV-2024-004
                new ThesisAssignment
                {
                    ThesisId = theses[3].Id,
                    PersonId = persons[0].Id, // GS.TS. Nguyễn Thanh Tùng
                    Role = AssignmentRole.Supervisor,
                    IsExternal = false,
                    AssignedAt = new DateTime(2024, 4, 10),
                    Notes = "Quyết định phân công hướng dẫn số 102/QĐ-SĐH"
                },

                // Đề tài 5: NCKH-2026-01 (Đề tài NCKH)
                new ThesisAssignment
                {
                    ThesisId = theses[4].Id,
                    PersonId = persons[5].Id, // TS. Ngô Quốc Hưng (Ngoài trường)
                    Role = AssignmentRole.Supervisor,
                    IsExternal = true,
                    AssignedAt = new DateTime(2025, 1, 15),
                    Notes = "Cố vấn khoa học đề tài NCKH sinh viên/học viên"
                }
            };
            await context.ThesisAssignments.AddRangeAsync(assignments);
            await context.SaveChangesAsync();

            // 9. Nạp danh sách thẩm định điều kiện (Eligibility Checks)
            var eligibilityChecks = new List<EligibilityCheck>
            {
                new EligibilityCheck
                {
                    ThesisId = theses[0].Id,
                    PersonId = persons[0].Id,
                    Role = AssignmentRole.Supervisor,
                    Result = CheckResult.Eligible,
                    Basis = "Đạt chuẩn Giáo sư ngành Khoa học máy tính, đã hướng dẫn thành công 3 NCS theo Thông tư 18/2021/TT-BGDĐT",
                    RegulationVersion = "Thông tư 18/2021/TT-BGDĐT",
                    CheckedAt = new DateTime(2023, 10, 10)
                },
                new EligibilityCheck
                {
                    ThesisId = theses[0].Id,
                    PersonId = persons[4].Id,
                    Role = AssignmentRole.Reviewer,
                    Result = CheckResult.Eligible,
                    Basis = "Đạt chuẩn PGS đúng chuyên ngành Trí tuệ nhân tạo, là chuyên gia ngoài trường đáp ứng cơ cấu hội đồng phản biện độc lập",
                    RegulationVersion = "Thông tư 18/2021/TT-BGDĐT",
                    CheckedAt = new DateTime(2026, 8, 25)
                },
                new EligibilityCheck
                {
                    ThesisId = theses[1].Id,
                    PersonId = persons[1].Id,
                    Role = AssignmentRole.Supervisor,
                    Result = CheckResult.Eligible,
                    Basis = "Đạt chuẩn PGS ngành Hệ thống thông tin theo Thông tư 23/2021/TT-BGDĐT",
                    RegulationVersion = "Thông tư 23/2021/TT-BGDĐT",
                    CheckedAt = new DateTime(2024, 2, 5)
                },
                new EligibilityCheck
                {
                    ThesisId = theses[4].Id, // NCKH
                    PersonId = persons[2].Id,
                    Role = AssignmentRole.Committee,
                    Result = CheckResult.Ineligible,
                    Basis = "Đề tài Nghiên cứu khoa học (IsGraduationCapstone = false) không thuộc luồng bảo vệ tốt nghiệp sau đại học của Module M05",
                    RegulationVersion = "Quy định Đào tạo SĐH 2024",
                    CheckedAt = new DateTime(2025, 1, 20)
                }
            };
            await context.EligibilityChecks.AddRangeAsync(eligibilityChecks);
            await context.SaveChangesAsync();

            // 10. Nạp phiên bảo vệ và kết quả bảo vệ (Defense Sessions & Defense Results)
            var defenseSession1 = new DefenseSession
            {
                ThesisId = theses[1].Id, // LV-2024-002
                ScheduledAt = DateTime.UtcNow.AddDays(-14),
                Location = "Phòng Bảo vệ Luận văn Sau đại học A2-302",
                Status = DefenseStatus.Defended,
                CommitteeNotes = "Hội đồng gồm 5 thành viên (có 2 thành viên ngoài trường). Phiên bảo vệ diễn ra nghiêm túc, đúng quy chế."
            };
            await context.DefenseSessions.AddAsync(defenseSession1);
            await context.SaveChangesAsync();

            var defenseResult1 = new DefenseResult
            {
                SessionId = defenseSession1.Id,
                Comments = "Luận văn đạt chất lượng xuất sắc, có tính mới và khả năng ứng dụng thực tế cao. Học viên nắm chắc kiến thức và trả lời thuyết phục các câu hỏi chất vấn của hội đồng.",
                Score = 9.20m,
                Pass = true,
                RecordedBy = "TS. Lê Quang Dũng (Thư ký Hội đồng)",
                RecordedAt = DateTime.UtcNow.AddDays(-14)
            };
            await context.DefenseResults.AddAsync(defenseResult1);

            var defenseSession2 = new DefenseSession
            {
                ThesisId = theses[0].Id, // LA-2023-001
                ScheduledAt = DateTime.UtcNow.AddDays(7),
                Location = "Hội trường A1 - Tòa nhà Điều hành Đào tạo",
                Status = DefenseStatus.Scheduled,
                CommitteeNotes = "Đã thông qua hồ sơ phản biện độc lập. Chuẩn bị tài liệu bảo vệ chính thức."
            };
            await context.DefenseSessions.AddAsync(defenseSession2);
            await context.SaveChangesAsync();

            // 11. Nạp danh mục tài liệu số hóa nộp lưu chiểu (Submitted Documents)
            // Giới hạn nghiêm ngặt trong 3 loại tài liệu bắt buộc của M05: ThuyetMinh, ToanVan, BienBanSuaDoi
            var documents = new List<SubmittedDocument>
            {
                // Đề tài 2 (LV-2024-002) nộp đủ cả 3 loại bắt buộc -> Tỷ lệ số hóa 100%
                new SubmittedDocument
                {
                    ThesisId = theses[1].Id,
                    DocumentType = DocumentType.ThuyetMinh,
                    FileName = "ThuyetMinh_DeTai_LV2024002.pdf",
                    FilePath = "/uploads/documents/ThuyetMinh_DeTai_LV2024002.pdf",
                    Version = 1,
                    UploadedAt = new DateTime(2024, 2, 1),
                    ConfirmedBy = "Phòng Đào tạo Sau đại học",
                    ConfirmedAt = DateTime.UtcNow.AddDays(-3)
                },
                new SubmittedDocument
                {
                    ThesisId = theses[1].Id,
                    DocumentType = DocumentType.ToanVan,
                    FileName = "ToanVan_LuanVan_LV2024002_v2.pdf",
                    FilePath = "/uploads/documents/ToanVan_LuanVan_LV2024002_v2.pdf",
                    Version = 2,
                    UploadedAt = new DateTime(2025, 1, 10),
                    ConfirmedBy = "Phòng Đào tạo Sau đại học",
                    ConfirmedAt = DateTime.UtcNow.AddDays(-3)
                },
                new SubmittedDocument
                {
                    ThesisId = theses[1].Id,
                    DocumentType = DocumentType.BienBanSuaDoi,
                    FileName = "BienBanGiaiTrinhSuaDoi_LV2024002.pdf",
                    FilePath = "/uploads/documents/BienBanGiaiTrinhSuaDoi_LV2024002.pdf",
                    Version = 1,
                    UploadedAt = new DateTime(2025, 2, 20),
                    ConfirmedBy = "Chủ tịch Hội đồng bảo vệ",
                    ConfirmedAt = DateTime.UtcNow.AddDays(-3)
                },

                // Đề tài 1 (LA-2023-001) nộp 2 loại bắt buộc -> Tỷ lệ số hóa 66.7%
                new SubmittedDocument
                {
                    ThesisId = theses[0].Id,
                    DocumentType = DocumentType.ThuyetMinh,
                    FileName = "ThuyetMinh_LuanAn_LA2023001.pdf",
                    FilePath = "/uploads/documents/ThuyetMinh_LuanAn_LA2023001.pdf",
                    Version = 1,
                    UploadedAt = new DateTime(2023, 10, 5),
                    ConfirmedBy = "Phòng Đào tạo Sau đại học",
                    ConfirmedAt = DateTime.UtcNow.AddDays(-3)
                },
                new SubmittedDocument
                {
                    ThesisId = theses[0].Id,
                    DocumentType = DocumentType.ToanVan,
                    FileName = "ToanVan_LuanAn_TienSi_LA2023001.pdf",
                    FilePath = "/uploads/documents/ToanVan_LuanAn_TienSi_LA2023001.pdf",
                    Version = 1,
                    UploadedAt = new DateTime(2026, 8, 20),
                    ConfirmedBy = "Phòng Đào tạo Sau đại học",
                    ConfirmedAt = DateTime.UtcNow.AddDays(-3)
                },

                // Đề tài 3 (DA-2024-003) nộp 1 loại bắt buộc -> Tỷ lệ số hóa 33.3%
                new SubmittedDocument
                {
                    ThesisId = theses[2].Id,
                    DocumentType = DocumentType.ThuyetMinh,
                    FileName = "ThuyetMinh_DeAn_DA2024003.pdf",
                    FilePath = "/uploads/documents/ThuyetMinh_DeAn_DA2024003.pdf",
                    Version = 1,
                    UploadedAt = new DateTime(2024, 3, 5),
                    ConfirmedBy = "Phòng Đào tạo Sau đại học",
                    ConfirmedAt = DateTime.UtcNow.AddDays(-3)
                }
            };
            await context.SubmittedDocuments.AddRangeAsync(documents);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Khởi tạo các tài khoản người dùng mẫu cho từng vai trò
        /// </summary>
        private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager, string defaultPassword)
        {
            var demoUsers = new[]
            {
                new { Email = "admin@lms.edu.vn", UserName = "admin@lms.edu.vn", Role = "Admin" },
                new { Email = "manager@lms.edu.vn", UserName = "manager@lms.edu.vn", Role = "Manager" },
                new { Email = "tung.nt@lms.edu.vn", UserName = "tung.nt@lms.edu.vn", Role = "Faculty" },
                new { Email = "an.nv@student.lms.edu.vn", UserName = "an.nv@student.lms.edu.vn", Role = "Student" },
                new { Email = "que.td@lms.edu.vn", UserName = "que.td@lms.edu.vn", Role = "Committee" }
            };

            foreach (var item in demoUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(item.Email);
                if (existingUser == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = item.UserName,
                        Email = item.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, defaultPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, item.Role);
                    }
                }
            }
        }
    }
}
