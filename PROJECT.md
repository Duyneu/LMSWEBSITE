# Project: Module M05 – Postgraduate Management (Quản lý Đào tạo Sau đại học)

## Architecture
Module M05 là ứng dụng ASP.NET Core MVC (.NET 8) dạng Capability Showcase hoàn chỉnh, tuân thủ Clean Architecture phân lớp và Repository Pattern.
- **Runtime & Framework**: .NET 8 (`net8.0`), ASP.NET Core MVC, Entity Framework Core 8.0.11, ASP.NET Core Identity.
- **Database**: SQL Server / LocalDB (`(localdb)\mssqllocaldb`), quản lý tập trung trong `AppDbContext : IdentityDbContext<IdentityUser>`.
- **Layers**:
  1. `Models/Entities`: Thực thể Domain nghiệp vụ (`PostgraduateStudent`, `AcademicPerson`, `Thesis`, `ThesisAssignment`, `EligibilityCheck`, `DefenseSession`, `DefenseResult`, `SubmittedDocument`) và các Enums.
  2. `Data`: `AppDbContext` kế thừa `IdentityDbContext`, cấu hình quan hệ, khóa ngoại, indexes và `DbInitializer` nạp dữ liệu mẫu phong phú.
  3. `Repositories`: Generic repository (`IRepository<T>`, `Repository<T>`), Unit of Work (`IUnitOfWork`, `UnitOfWork`), và Specific Repositories (`IThesisRepository`, `IAcademicPersonRepository`, `IDefenseSessionRepository`, `ISubmittedDocumentRepository`).
  4. `Services`: Business services (`IThesisService`, `IEligibilityAndAssignmentService`, `IDefenseAndDocumentService`, `IAiReviewerSuggestionService`).
  5. `ViewModels`: ViewModels tách biệt hoàn toàn khỏi Domain Models cho từng màn hình (`ThesisViewModels`, `AssignmentViewModels`, `DefenseViewModels`, `DocumentViewModels`, `AiSuggestionViewModels`).
  6. `Controllers`: `ThesisController`, `SupervisorAssignmentController` (explicit path `~/Views/Assignment/Index.cshtml`), `DefenseSessionController` (explicit path `~/Views/Defense/Index.cshtml`), `DocumentController`, `AccountController`.
  7. `Views`: Bootstrap 5 UI, tổ chức chính xác vào 4 thư mục `Views/Thesis/`, `Views/Assignment/`, `Views/Defense/`, `Views/Document/` kèm `Views/Shared/`.

## Code Layout
```
PostgraduateManagement/
├── Controllers/
│   ├── ThesisController.cs
│   ├── SupervisorAssignmentController.cs
│   ├── DefenseSessionController.cs
│   ├── DocumentController.cs
│   └── AccountController.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbInitializer.cs
├── Models/
│   ├── Enums/
│   │   ├── AcademicDegree.cs
│   │   ├── AssignmentRole.cs
│   │   ├── CheckResult.cs
│   │   ├── DefenseStatus.cs
│   │   ├── DocumentType.cs
│   │   ├── ThesisStatus.cs
│   │   └── ThesisType.cs
│   └── Entities/
│       ├── AcademicPerson.cs
│       ├── DefenseResult.cs
│       ├── DefenseSession.cs
│       ├── EligibilityCheck.cs
│       ├── PostgraduateStudent.cs
│       ├── SubmittedDocument.cs
│       ├── Thesis.cs
│       └── ThesisAssignment.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IAcademicPersonRepository.cs
│   │   ├── IDefenseSessionRepository.cs
│   │   ├── IRepository.cs
│   │   ├── ISubmittedDocumentRepository.cs
│   │   ├── IThesisRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Implementations/
│       ├── AcademicPersonRepository.cs
│       ├── DefenseSessionRepository.cs
│       ├── Repository.cs
│       ├── SubmittedDocumentRepository.cs
│       ├── ThesisRepository.cs
│       └── UnitOfWork.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAiReviewerSuggestionService.cs
│   │   ├── IDefenseAndDocumentService.cs
│   │   ├── IEligibilityAndAssignmentService.cs
│   │   └── IThesisService.cs
│   └── Implementations/
│       ├── AiReviewerSuggestionService.cs
│       ├── DefenseAndDocumentService.cs
│       ├── EligibilityAndAssignmentService.cs
│       └── ThesisService.cs
├── ViewModels/
│   ├── AiSuggestionViewModels.cs
│   ├── AssignmentViewModels.cs
│   ├── DefenseViewModels.cs
│   ├── DocumentViewModels.cs
│   └── ThesisViewModels.cs
├── Views/
│   ├── Assignment/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   ├── Defense/
│   │   ├── Index.cshtml
│   │   ├── Schedule.cshtml
│   │   └── RecordResult.cshtml
│   ├── Document/
│   │   ├── Index.cshtml
│   │   ├── Upload.cshtml
│   │   └── DigitizationReport.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _StatusBadge.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   └── Thesis/
│       ├── Create.cshtml
│       ├── Details.cshtml
│       ├── Edit.cshtml
│       └── Index.cshtml
├── appsettings.json
├── Program.cs
└── PostgraduateManagement.csproj
```

## Feature Inventory
Mọi tính năng được khảo sát và khai phá từ `ORIGINAL_REQUEST.md` được ánh xạ đầy đủ:
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | .NET 8 Project Initialization | Khởi tạo dự án ASP.NET Core MVC .NET 8, cấu hình `app.UseStaticFiles()` và package EF Core 8.0.11 | M1 | survey |
| 2 | PostgraduateStudent Model | Quản lý học viên sau đại học liên kết `SharedStudentCode` | M1 | survey |
| 3 | AcademicPerson Model | Quản lý chuyên gia/giảng viên với `SharedPersonCode`, `FullName`, `AcademicDegree`, `ResearchField`, `IsExternal` | M1 | survey |
| 4 | Thesis Model & Capstone Flag | Quản lý đề tài với `ThesisType`, `ThesisStatus`, `StudentId` và cờ `IsGraduationCapstone = true` | M1 | survey |
| 5 | ThesisAssignment Model | Phân công vai trò `supervisor`, `reviewer`, `committee` và cờ `IsExternal` | M1 | survey |
| 6 | EligibilityCheck Model | Kiểm tra điều kiện với `Result`, `Basis`, `RegulationVersion`, `CheckedAt` | M1 | survey |
| 7 | DefenseSession & DefenseResult Models | Lập lịch bảo vệ (`ScheduledAt`, `Location`, `Status`) và ghi nhận kết quả (`Comments`, `Score`, `Pass`) | M1 | survey |
| 8 | SubmittedDocument Model | Quản lý nộp số hóa 3 loại văn bản bắt buộc (`ThuyetMinh`, `ToanVan`, `BienBanSuaDoi`), version, upload time | M1 | survey |
| 9 | AppDbContext Configuration | Cấu hình EF Core Fluent API, quan hệ, khóa ngoại, indexes hiệu năng và tích hợp IdentityDbContext | M1 | survey |
| 10 | Seed Data Generation | Nạp dữ liệu mẫu phong phú: học viên, chuyên gia nội bộ/ngoài trường, đề tài tốt nghiệp & NCKH, phân công, phiên bảo vệ, tài liệu | M1 | survey |
| 11 | Generic Repository & UnitOfWork | Mẫu thiết kế Generic Repository `IRepository<T>`, `Repository<T>` và `IUnitOfWork` | M2 | survey |
| 12 | Specific Repositories | Triển khai các repository chuyên biệt: `ThesisRepository`, `AcademicPersonRepository`, `DefenseSessionRepository`, `SubmittedDocumentRepository` | M2 | survey |
| 13 | ThesisService Lifecycle | Quản lý toàn bộ vòng đời đề tài (Draft → Registered → Approved → InProgress → SubmittedForReview → Defended → Completed) | M3 | survey |
| 14 | IsGraduationCapstone Gate | Kiểm tra nghiêm ngặt cờ `IsGraduationCapstone`, chặn đề tài NCKH tham gia quy trình bảo vệ M05 | M3 | survey |
| 15 | Eligibility Validation | Kiểm tra điều kiện người hướng dẫn, phản biện, hội đồng dựa trên học vị, lĩnh vực, phiên bản quy chế | M3 | survey |
| 16 | Assignment Management | Phân công và quản lý vai trò chuyên gia nội bộ và chuyên gia ngoài trường (`IsExternal`) | M3 | survey |
| 17 | Defense Scheduling & Scoring | Lập lịch phiên bảo vệ, cập nhật trạng thái và ghi nhận điểm số, nhận xét, kết quả Đạt/Không đạt | M3 | survey |
| 18 | Digital Document Management | Quản lý nộp tài liệu số hóa, quản lý phiên bản và xác nhận tài liệu | M3 | survey |
| 19 | Strictly Scoped Digitization Metric | Tính toán tỷ lệ số hóa chính xác 100% trong phạm vi danh mục tài liệu M05, không mở rộng toàn quy trình | M3 | survey |
| 20 | AI Reviewer Suggestion Heuristic | Heuristic gợi ý phản biện độc lập dựa trên độ tương đồng lĩnh vực nghiên cứu và học hàm/học vị (GS > PGS > TS) | M3 | survey |
| 21 | Advisory-Only AI Guard | Đảm bảo kết quả gợi ý AI chỉ mang tính tham khảo (stub/advisory), không tự động phân công | M3 | survey |
| 22 | Thesis ViewModels & Controller | Xây dựng CRUD, phân trang, lọc và điều khiển vòng đời đề tài sau đại học | M4 | survey |
| 23 | Assignment ViewModels & Controller | Xây dựng màn hình phân công với chỉ định view tường minh: `~/Views/Assignment/Index.cshtml` | M4 | survey |
| 24 | Defense ViewModels & Controller | Xây dựng màn hình tổ chức bảo vệ với chỉ định view tường minh: `~/Views/Defense/Index.cshtml` | M4 | survey |
| 25 | Document ViewModels & Controller | Quản lý tải lên, duyệt tài liệu và báo cáo tiến độ số hóa | M4 | survey |
| 26 | Identity & Authorization | Phân quyền vai trò người dùng (Học viên, Giảng viên, Hội đồng, Quản trị) | M4 | survey |
| 27 | Views/Thesis (Bootstrap 5) | Giao diện danh sách, chi tiết, tạo mới, chỉnh sửa đề tài kèm status badge | M5 | survey |
| 28 | Views/Assignment (Bootstrap 5) | Giao diện phân công, hiển thị danh sách gợi ý AI kèm lý do/điểm số và nút phân công thủ công | M5 | survey |
| 29 | Views/Defense (Bootstrap 5) | Giao diện lập lịch bảo vệ, danh sách hội đồng, form nhập điểm và kết quả | M5 | survey |
| 30 | Views/Document (Bootstrap 5) | Giao diện nộp tài liệu, danh sách phiên bản và dashboard tiến độ số hóa 3 loại văn bản | M5 | survey |
| 31 | Full Capability Showcase Verification | Kiểm tra biên dịch 0 lỗi, quy chuẩn comment dòng đầu `// File: `, 100% acceptance criteria | M6 | survey |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | DbContext, Entities & Data Seeding | Khởi tạo project .NET 8, cấu hình Entity Framework Core, Domain Entities, Enums, AppDbContext & DbInitializer | none | DONE |
| M2 | Repository Layer | Generic Repository, Unit of Work, và các Specific Repositories | M1 | DONE |
| M3 | Business Service Layer & AI Suggestion Stub | ThesisService (với cờ chặn NCKH), EligibilityAndAssignmentService, DefenseAndDocumentService (tỷ lệ số hóa M05), AiReviewerSuggestionService (heuristic độc lập) | M2 | DONE |
| M4 | ViewModels & Controllers | ThesisController, SupervisorAssignmentController (explicit view path), DefenseSessionController (explicit view path), DocumentController, AccountController, ViewModels tách biệt | M3 | DONE |
| M5 | Views (.cshtml) with Bootstrap 5 | 4 thư mục views chính (`Views/Thesis/`, `Views/Assignment/`, `Views/Defense/`, `Views/Document/`), layout, status badges, validation, AI suggestions UI, digitization dashboard | M4 | DONE |
| M6 | Capability Showcase & Acceptance Verification | Kiểm tra toàn diện 0 lỗi build, xác thực toàn bộ nghiệp vụ (NCKH block, digitization ratio, explicit routing, identity, code style) và audit | M5 | DONE |

## Interface Contracts

### 1. IRepository<T> & IUnitOfWork
```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}

public interface IUnitOfWork : IDisposable
{
    IThesisRepository Theses { get; }
    IAcademicPersonRepository AcademicPersons { get; }
    IDefenseSessionRepository DefenseSessions { get; }
    ISubmittedDocumentRepository SubmittedDocuments { get; }
    IRepository<PostgraduateStudent> PostgraduateStudents { get; }
    IRepository<ThesisAssignment> ThesisAssignments { get; }
    IRepository<EligibilityCheck> EligibilityChecks { get; }
    IRepository<DefenseResult> DefenseResults { get; }
    Task<int> CompleteAsync();
}
```

### 2. Specific Repositories
```csharp
public interface IThesisRepository : IRepository<Thesis>
{
    Task<IEnumerable<Thesis>> GetThesesWithDetailsAsync(string? search, ThesisStatus? status, ThesisType? type, int page, int pageSize);
    Task<int> GetThesesCountAsync(string? search, ThesisStatus? status, ThesisType? type);
    Task<Thesis?> GetThesisWithFullDetailsAsync(int id);
}

public interface IAcademicPersonRepository : IRepository<AcademicPerson>
{
    Task<IEnumerable<AcademicPerson>> GetEligibleReviewersAsync(string researchField);
}

public interface IDefenseSessionRepository : IRepository<DefenseSession>
{
    Task<DefenseSession?> GetSessionWithDetailsAsync(int id);
    Task<IEnumerable<DefenseSession>> GetUpcomingSessionsAsync();
}

public interface ISubmittedDocumentRepository : IRepository<SubmittedDocument>
{
    Task<IEnumerable<SubmittedDocument>> GetByThesisIdAsync(int thesisId);
}
```

### 3. Business Service Layer
```csharp
public interface IThesisService
{
    Task<PagedResult<ThesisDto>> GetThesesAsync(ThesisFilterDto filter);
    Task<ThesisDetailDto?> GetThesisByIdAsync(int id);
    Task<ThesisDto> CreateThesisAsync(CreateThesisDto dto);
    Task<bool> ApproveThesisAsync(int id);
    Task<bool> TransitionStatusAsync(int id, ThesisStatus newStatus);
    Task<bool> ValidateForDefenseAsync(int id); // Rejects NCKH (IsGraduationCapstone == false)
}

public interface IEligibilityAndAssignmentService
{
    Task<EligibilityCheckResultDto> CheckEligibilityAsync(int thesisId, int personId, AssignmentRole role);
    Task<bool> AssignPersonAsync(int thesisId, int personId, AssignmentRole role, bool isExternal);
    Task<IEnumerable<ThesisAssignmentDto>> GetAssignmentsByThesisAsync(int thesisId);
}

public interface IDefenseAndDocumentService
{
    Task<DefenseSessionDto> ScheduleDefenseAsync(ScheduleDefenseDto dto);
    Task<DefenseResultDto> RecordDefenseResultAsync(RecordDefenseResultDto dto, string recordedBy);
    Task<SubmittedDocumentDto> UploadDocumentAsync(UploadDocumentDto dto);
    Task<bool> ConfirmDocumentAsync(int documentId, string confirmedBy);
    Task<DigitizationProgressDto> GetDigitizationProgressAsync(int thesisId); // Strictly 3 M05 mandatory document types
}

public interface IAiReviewerSuggestionService
{
    Task<IEnumerable<ReviewerSuggestionDto>> SuggestReviewersAsync(int thesisId);
}
```

## Mandatory Coding Standard
1. Dòng đầu tiên của MỌI tệp mã nguồn (.cs, .cshtml, v.v.): `// File: <đường_dẫn_tương_đối>` (ví dụ `// File: Controllers/ThesisController.cs`).
2. Mã nguồn (classes, properties, methods, variables) viết bằng tiếng Anh chuẩn.
3. Chú thích nghiệp vụ (comments) viết bằng tiếng Việt rõ ràng, giải thích ngữ cảnh nghiệp vụ.
4. Không tạo thêm project test ngoài lề.
