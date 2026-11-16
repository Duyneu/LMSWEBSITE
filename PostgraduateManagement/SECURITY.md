# Tài liệu Bảo mật Hệ thống Quản lý Sau đại học (Module M05)

Tài liệu này mô tả kiến trúc an toàn thông tin, các biện pháp kỹ thuật và cơ chế kiểm soát truy cập được triển khai trực tiếp trong mã nguồn của phân hệ Quản lý Sau đại học (PostgraduateManagement).

---

## 1. Xác thực và Quản lý Danh tính (Authentication)

### 1.1. Cơ chế băm mật khẩu (Password Hashing)
Hệ thống sử dụng cơ chế xác thực danh tính tích hợp của **ASP.NET Core 8 Identity**.
- **Thuật toán băm**: PBKDF2 với HMAC-SHA256.
- **Số vòng lặp (Iterations)**: 10,000 vòng lặp (cấu hình mặc định của Identity V3 Password Hasher trong .NET 8).
- **Muối (Salt)**: Chuỗi ngẫu nhiên 128-bit được sinh tự động theo chuẩn mã hóa cho từng tài khoản, lưu kèm chuỗi băm để ngăn chặn tấn công từ điển và bảng băm cầu vồng (Rainbow Table).

### 1.2. Chính sách khóa tài khoản (Account Lockout)
- Tiến trình đăng nhập tại `AccountController.Login` kích hoạt cờ `lockoutOnFailure: true`.
- Khi người dùng nhập sai mật khẩu vượt quá ngưỡng quy định, tài khoản sẽ chuyển sang trạng thái tạm khóa (`result.IsLockedOut`), giảm thiểu nguy cơ tấn công dò quét mật khẩu tự động (Brute Force).

### 1.3. Định hướng tích hợp đăng nhập một lần (Single Sign-On - SSO)
- Kiến trúc hệ thống thiết kế theo chuẩn mở, hỗ trợ OIDC với hệ thống định danh của trường.
- Cho phép liên kết tài khoản định danh cán bộ và học viên thông qua mã cán bộ (`SharedPersonCode`) và mã học viên (`SharedStudentCode`).

---

## 2. Kiểm soát Phân quyền (Authorization & Access Control)

Hệ thống áp dụng mô hình phân quyền đa tầng kết hợp giữa Phân quyền theo vai trò (Role-based Access Control - RBAC) và Kiểm soát theo phạm vi dữ liệu (Resource-based Ownership / Scope Enforcement).

### 2.1. Phân quyền theo vai trò (RBAC)
Hệ thống phân định 5 nhóm vai trò người dùng chính:
- **Admin**: Quản trị viên hệ thống, quản lý tài khoản, danh mục và cấu hình hạ tầng.
- **Manager**: Chuyên viên phòng Đào tạo Sau đại học, có quyền phê duyệt đề cương, phân công cán bộ hướng dẫn/phản biện, thiết lập lịch bảo vệ và xác nhận tài liệu số hóa lưu chiểu.
- **Faculty**: Cán bộ, giảng viên, nhà khoa học tham gia hướng dẫn khoa học hoặc phản biện đề tài.
- **Committee**: Thành viên hội đồng đánh giá luận văn/đề án, thực hiện ghi nhận điểm số và biên bản đánh giá trong các phiên bảo vệ được phân công.
- **Student**: Học viên sau đại học, nộp hồ sơ đề tài, tải lên tài liệu số hóa và theo dõi tiến độ đào tạo của bản thân.

### 2.2. Kiểm soát theo quyền sở hữu tài nguyên (Resource-based Ownership / Scope Checks)
Bên cạnh vai trò, hệ thống kiểm tra phạm vi truy cập trên từng bản ghi thực thể:
- **Học viên (Student)**: Chỉ có quyền xem thông tin chi tiết và tải lên tài liệu cho các đề tài thuộc sở hữu của chính mình (`thesis.StudentId` trùng khớp với định danh học viên của tài khoản đăng nhập). Yêu cầu truy cập đề tài của người khác bị từ chối bằng phản hồi `Forbid()` hoặc `NotFound()`.
- **Cán bộ hướng dẫn (Faculty)**: Chỉ có quyền xem chi tiết và theo dõi tiến độ của những đề tài mà cán bộ được phân công vai trò hướng dẫn (`Supervisor`) trong danh sách phân công (`ThesisAssignment`).
- **Thành viên hội đồng (Committee)**: Chỉ được phép truy cập chi tiết phiên bảo vệ và ghi nhận điểm đánh giá đối với đề tài mà cán bộ được chỉ định tham gia hội đồng (`CouncilMember`).
- **Chuyên viên quản lý (Manager) / Admin**: Có quyền thực thi các nghiệp vụ phê duyệt đề cương (`ApproveThesis`), phân công cán bộ (`AssignPerson`), và lập lịch bảo vệ (`ScheduleDefense`). Học viên và giảng viên không được phép thực thi các thao tác này.

---

## 3. Hệ thống Nhật ký Kiểm toán (Audit Logging)

Để đáp ứng yêu cầu thanh tra, đối soát và phục vụ công tác hậu kiểm, hệ thống triển khai thực thể `AuditLog` và ghi vết tự động vào cơ sở dữ liệu cho các thao tác trọng yếu.

### 3.1. Cấu trúc bản ghi nhật ký (`AuditLog`)
Mỗi sự kiện kiểm toán ghi nhận các trường thông tin:
- `Id`: Khóa chính tự tăng của bản ghi nhật ký.
- `UserId`: Mã định danh người dùng thực hiện thao tác (từ `ClaimsPrincipal`).
- `UserName`: Tên đăng nhập người dùng thực hiện.
- `Action`: Tên hành vi thao tác (ví dụ: `ApproveThesis`, `AssignPerson`, `RecordDefenseResult`, `UploadDocument`, `ConfirmDocument`).
- `EntityName`: Tên đối tượng dữ liệu bị tác động (ví dụ: `Thesis`, `ThesisAssignment`, `DefenseResult`, `SubmittedDocument`).
- `EntityId`: Mã khóa chính hoặc mã nghiệp vụ của thực thể.
- `Details`: Mô tả chi tiết nội dung thay đổi hoặc tham số nghiệp vụ.
- `Timestamp`: Thời gian ghi nhận sự kiện theo chuẩn UTC.

### 3.2. Các sự kiện bắt buộc ghi nhật ký kiểm toán
1. **Phê duyệt đề tài**: Ghi vết khi chuyên viên phòng Sau đại học phê duyệt đề cương hoặc chuyển trạng thái vòng đời đề tài (`ThesisService.ApproveThesisAsync`, `TransitionStatusAsync`).
2. **Phân công cán bộ**: Ghi vết khi chỉ định cán bộ hướng dẫn hoặc phân công phản biện/hội đồng (`EligibilityAndAssignmentService.AssignPersonAsync`).
3. **Ghi nhận điểm bảo vệ**: Ghi vết khi hội đồng hoàn tất chấm điểm và lưu kết quả bảo vệ (`DefenseAndDocumentService.RecordDefenseResultAsync`).
4. **Tải lên tài liệu số hóa**: Ghi vết tệp tin nộp lưu chiểu kèm thông tin phiên bản và mã băm SHA-256 (`DefenseAndDocumentService.UploadDocumentAsync`).
5. **Xác nhận tài liệu**: Ghi vết khi cán bộ kiểm tra và xác nhận hồ sơ số hóa hợp lệ (`DefenseAndDocumentService.ConfirmDocumentAsync`).

---

## 4. Quản lý Chuyên gia ngoài trường (External Experts)

Quy chế đào tạo sau đại học quy định hội đồng bảo vệ và phản biện độc lập cần có sự tham gia của chuyên gia ngoài trường nhằm đảm bảo tính khách quan.

### 4.1. Cơ chế dữ liệu thực thể `AcademicPerson`
- **Đánh dấu nguồn gốc cán bộ**: Thuộc tính `IsExternal` (kiểu `bool`):
  - `false`: Cán bộ cơ hữu thuộc trường.
  - `true`: Chuyên gia ngoài trường, nhà khoa học thuộc viện nghiên cứu hoặc doanh nghiệp liên kết.
- **Mã định danh cán bộ nullable**: Cột `SharedPersonCode` được thiết kế chấp nhận giá trị `null` (`public string? SharedPersonCode { get; set; }`), do chuyên gia ngoài trường không có mã số cán bộ nội bộ.
- **Chỉ mục duy nhất có điều kiện (Filtered Unique Index)**: Trong `AppDbContext.OnModelCreating`, cấu hình chỉ mục duy nhất có bộ lọc:
  ```csharp
  entity.HasIndex(e => e.SharedPersonCode)
        .IsUnique()
        .HasFilter("[SharedPersonCode] IS NOT NULL");
  ```
  Cấu hình này đảm bảo tính duy nhất của mã cán bộ cơ hữu, đồng thời cho phép nhiều chuyên gia ngoài trường cùng có giá trị `SharedPersonCode` là `NULL` mà không gây xung đột khóa.

---

## 5. Bảo mật Tải lên và Lưu trữ Tài liệu (File Storage & Upload Security)

Hệ thống số hóa hồ sơ nộp lưu chiểu (Thuyết minh, Toàn văn, Biên bản sửa đổi) được bảo vệ bằng các lớp kiểm soát kỹ thuật nghiêm ngặt:

### 5.1. Vị trí lưu trữ ngoài thư mục công khai (Outside-wwwroot Storage)
- Tệp tin tải lên không được lưu trong thư mục `wwwroot` của ứng dụng web nhằm loại bỏ nguy cơ thực thi mã độc từ xa qua URL trực tiếp.
- Đường dẫn lưu trữ được cấu hình qua tham số `Storage:UploadPath` trong `appsettings.json` (thư mục cách ly `PostgraduateStorage/uploads`).

### 5.2. Đổi tên tệp bằng GUID ngẫu nhiên phía máy chủ
- Khi ghi tệp lên đĩa, tên tệp vật lý được tạo ngẫu nhiên bằng chuỗi GUID:
  ```csharp
  var physicalFileName = $"{Guid.NewGuid()}{extension}";
  ```
- Tên tệp gốc được làm sạch thông qua `Path.GetFileName` để loại bỏ các ký tự điều hướng thư mục (Path Traversal), sau đó lưu vào trường `FileName` trong cơ sở dữ liệu phục vụ hiển thị.

### 5.3. Danh sách định dạng cho phép và Giới hạn kích thước
- **Whitelist định dạng**: Chỉ chấp nhận hai phần mở rộng tệp: `.pdf` và `.docx`. Mọi định dạng khác (bao gồm `.exe`, `.dll`, `.bat`, `.sh`, `.php`, `.asp`) đều bị từ chối.
- **Giới hạn kích thước**: Dung lượng tệp tối đa cho phép là 20 MB (20 * 1024 * 1024 bytes). Các tệp vượt quá giới hạn bị từ chối ngay tại tầng kiểm tra.

### 5.4. Xác thực chữ ký nhị phân (Magic Bytes Verification)
Phần mở rộng tệp có thể bị giả mạo. Do đó, hệ thống đọc các byte đầu tiên của luồng tệp tin (File Stream) để xác thực định dạng nhị phân thực tế:
- **Tệp PDF**: Kiểm tra 4 byte đầu tiên phải là `0x25, 0x50, 0x44, 0x46` (tương ứng ký tự `%PDF`).
- **Tệp DOCX (OpenXML ZIP)**: Kiểm tra 4 byte đầu tiên phải là `0x50, 0x4B, 0x03, 0x04` (tương ứng ký tự `PK\x03\x04`).

### 5.5. Mã băm toàn vẹn SHA-256 (Integrity Hash)
- Khi tệp được tải lên, hệ thống tính toán mã băm SHA-256 (chuỗi hex 64 ký tự) dựa trên toàn bộ nội dung byte của tệp và lưu vào thuộc tính `Sha256Hash` của thực thể `SubmittedDocument`.
- Chuỗi băm này đóng vai trò xác thực tính toàn vẹn của tài liệu, phát hiện mọi sự can thiệp hoặc thay đổi trái phép trên hệ thống tệp vật lý.

### 5.6. Tải tệp có kiểm soát quyền truy cập (Secure Download Endpoint)
- Tệp tin được cung cấp qua action `DocumentController.Download(int id)` có gắn cờ `[Authorize]`.
- Action kiểm tra quyền sở hữu và phạm vi người dùng trước khi truyền phát tệp (FileStreamResult):
  - Người dùng vai trò Student chỉ tải được tài liệu của đề tài mình thực hiện.
  - Người dùng vai trò Faculty chỉ tải được tài liệu của đề tài mình hướng dẫn.
  - Người dùng vai trò Committee chỉ tải được tài liệu của đề tài trong hội đồng mình tham gia.
  - Quản lý và Quản trị viên được quyền tải để kiểm tra hồ sơ.

---

## 6. Bảo mật Tầng ứng dụng Web (Web Application Security)

### 6.1. Phòng chống tấn công CSRF (Cross-Site Request Forgery)
- Hệ thống áp dụng bộ lọc toàn cục `AutoValidateAntiforgeryTokenAttribute` trong cấu hình `AddControllersWithViews`:
  ```csharp
  builder.Services.AddControllersWithViews(options =>
  {
      options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
  });
  ```
- Tất cả các yêu cầu thay đổi trạng thái (POST, PUT, DELETE) bắt buộc phải mang Antiforgery Token hợp lệ trong form dữ liệu hoặc header.
- Thao tác đăng xuất (`AccountController.Logout`) bắt buộc sử dụng phương thức `[HttpPost]` đi kèm `[ValidateAntiForgeryToken]`, ngăn chặn hành vi ép người dùng đăng xuất trái ý muốn qua phương thức GET.

### 6.2. Cấu hình Cookie an toàn
Cookie phiên làm việc của ASP.NET Core Identity được thiết lập với các thuộc tính bảo vệ tiêu chuẩn:
- `HttpOnly = true`: Ngăn chặn mã kịch bản phía máy khách (JavaScript) truy cập vào cookie, hạn chế rủi ro đánh cắp phiên khi xảy ra lỗ hổng XSS.
- `SameSite = SameSiteMode.Lax`: Hạn chế gửi kèm cookie trong các yêu cầu liên trang (cross-site requests), giảm thiểu nguy cơ CSRF trong các liên kết điều hướng.
- `SecurePolicy = CookieSecurePolicy.Always`: Chỉ cho phép truyền tải cookie thông qua các kết nối được mã hóa HTTPS.

### 6.3. Phòng chống tấn công Open Redirect
Tại phương thức `AccountController.Login`:
- Tham số `ReturnUrl` được kiểm tra qua `Url.IsLocalUrl(model.ReturnUrl)`.
- Chỉ các đường dẫn nội bộ trong cùng ứng dụng mới được chấp thuận chuyển hướng; các URL bên ngoài hoặc không hợp lệ sẽ tự động điều hướng về trang mặc định của hệ thống.

### 6.4. Phòng chống SQL Injection và XSS
- **SQL Injection**: Toàn bộ thao tác truy xuất dữ liệu trong các tầng Repository và Service sử dụng Entity Framework Core với cú pháp LINQ và truy vấn có tham số hóa (Parameterized Queries). Hệ thống không ghép nối chuỗi thô trong `FromSqlRaw` hoặc `ExecuteSqlRaw`.
- **Cross-Site Scripting (XSS)**: Công cụ dựng giao diện Razor Engine tự động mã hóa ngữ cảnh HTML đối với mọi biến hiển thị. Hệ thống không sử dụng `Html.Raw` trên các trường dữ liệu do người dùng nhập vào.

### 6.5. Bảo vệ kênh truyền dữ liệu (HTTPS & HSTS)
- Pipeline ứng dụng kích hoạt `app.UseHttpsRedirection()` nhằm tự động chuyển đổi các yêu cầu HTTP sang HTTPS.
- Trong môi trường sản xuất, hệ thống áp dụng `app.UseHsts()` (HTTP Strict Transport Security) để yêu cầu trình duyệt luôn sử dụng giao thức HTTPS khi gửi dữ liệu.

---

## 7. Rào chắn Nghiệp vụ cho Đề tài Nghiên cứu Khoa học (IsGraduationCapstone Guard)

Phân hệ M05 phân tách rõ ràng giữa hai loại hình nghiên cứu sau đại học:
1. **Đề tài tốt nghiệp (Graduation Capstone)**: `IsGraduationCapstone = true` (Đề án thạc sĩ, Luận văn thạc sĩ, Luận án tiến sĩ). Thuộc đối tượng được bảo vệ tốt nghiệp trước hội đồng.
2. **Đề tài nghiên cứu khoa học (Scientific Research)**: `IsGraduationCapstone = false`. Chỉ phục vụ nghiệm thu đề tài NCKH, không được phép đưa vào quy trình bảo vệ tốt nghiệp của Module M05.

### Cơ chế kiểm soát kỹ thuật:
- **Thẩm định điều kiện bảo vệ**: Phương thức `ThesisService.ValidateForDefenseAsync(int id)` kiểm tra giá trị `thesis.IsGraduationCapstone`. Nếu là `false`, phương thức ghi nhật ký cảnh báo và ném ngoại lệ `InvalidOperationException` từ chối xử lý:
  ```csharp
  if (!thesis.IsGraduationCapstone)
  {
      _logger.LogWarning("Đề tài {Code} là đề tài NCKH (IsGraduationCapstone = false), bị từ chối vào luồng bảo vệ M05.", thesis.Code);
      throw new InvalidOperationException("Đề tài nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp theo quy định của Module M05.");
  }
  ```
- **Lập lịch bảo vệ**: Phương thức `DefenseAndDocumentService.ScheduleDefenseAsync` bắt buộc gọi `ValidateForDefenseAsync` trước khi khởi tạo phiên bảo vệ.
- **Chuyển đổi trạng thái**: Phương thức `ThesisService.TransitionStatusAsync` kiểm tra và ngăn chặn việc chuyển trạng thái đề tài NCKH sang các giai đoạn bảo vệ (`SubmittedForReview`, `Defended`).
- **Kiểm thử tự động**: Cơ chế kiểm soát này được bảo vệ bởi bộ kiểm thử đơn vị trong `PostgraduateManagement.Tests/ThesisServiceTests.cs`.
