# Bảng đối chiếu bảo mật M05 với danh sách thực hành (nhóm B, C, D)

Trạng thái: **Có** = đã làm và có bằng chứng; **Một phần** = làm một phần, nêu rõ phần thiếu; **Không** = chưa làm; **N/A** = không có đối tượng tương ứng trong ứng dụng.

## B. Bảo mật lập trình ASP.NET

| # | Mục | Trạng thái | Ở đâu / ghi chú |
|---|---|---|---|
| 9 | Xác thực và phân quyền Identity | Có | `Program.cs` (Identity, khóa 5 lần / 5 phút), `[Authorize(Roles)]` trên controller. Kiểm tra bằng 403 khi User mở `/Student`. |
| 10 | Chống SQL Injection | Có | Chỉ dùng EF Core / LINQ, không có SQL nối chuỗi (không có `FromSql`, `ExecuteSql`). Chưa có bản demo nối chuỗi để so sánh. |
| 11 | Chống XSS | Có | Razor tự mã hóa đầu ra; không dùng `Html.Raw` cho dữ liệu người dùng. |
| 12 | Chống CSRF | Có | Bộ lọc `AutoValidateAntiforgeryTokenAttribute` toàn cục; thiếu hoặc sai token trả 400. Cookie HttpOnly, Secure, SameSite=Lax. |
| 13 | Connection string và secret | Một phần | Connection string LocalDB dùng Windows auth, không chứa mật khẩu. Mật khẩu tài khoản demo đọc từ cấu hình `Seed:DefaultPassword` (mặc định vẫn là giá trị demo). Chưa dùng User Secrets. |
| 14 | Tải tệp lên an toàn | Có | `DocumentController`: whitelist `.pdf/.docx`, 20 MB, kiểm tra magic bytes, tên GUID, lưu ngoài wwwroot, SHA-256. Có test. |
| 15 | Log, xử lý lỗi, giám sát | Có | `ILogger` ghi đăng nhập thành công/thất bại/khóa, truy cập trái phép (AccessDenied), đổi mật khẩu; AuditLog ghi thao tác nghiệp vụ. Trang `/Home/Error` không lộ stack trace ở môi trường không phải Development. |

## C. Thực hành nâng cao

| # | Mục | Trạng thái | Ở đâu / ghi chú |
|---|---|---|---|
| 16 | Đăng nhập nhiều lớp, quyền từ CSDL | Một phần | Vai trò lưu trong bảng Identity của CSDL, kiểm tra ở action; phiên hết hạn sau 30 phút không hoạt động. Chưa đọc quyền qua stored procedure. |
| 17 | Đổi mật khẩu, mật khẩu mạnh | Có | Tối thiểu 12 ký tự, đủ hoa, thường, số, ký tự đặc biệt; trang `/Account/ChangePassword` kiểm tra mật khẩu cũ. Có test chính sách. |
| 18 | Kiểm soát dữ liệu cá nhân | Có | `ThesisAccess.CanAccess`: học viên chỉ thấy đề tài của mình, giảng viên đề tài mình hướng dẫn, hội đồng đề tài được phân công. Áp dụng cho danh sách đề tài và mọi trang tài liệu. Có test. |
| 19 | Tìm kiếm và lọc an toàn | Có | Tìm theo mã, tên, lĩnh vực, học viên; lọc trạng thái, loại; tất cả là truy vấn tham số (LINQ), kết quả giới hạn theo quyền. |
| 20 | Nhật ký thao tác xuống CSDL | Có | Bảng `AuditLog` (người, hành động, đối tượng, thời điểm UTC), ghi cùng lần lưu với thay đổi. |
| 21 | Bảo vệ dữ liệu nhạy cảm trên báo cáo | Không | Ứng dụng chưa có báo cáo xuất dữ liệu. Danh sách học viên/chuyên gia chỉ dành cho Admin, Manager. |
| 22 | Bảo mật API | N/A | Ứng dụng là MVC, không có API JSON. Các action POST đã có CSRF và phân quyền. |
| 23 | Không lộ thông tin qua thông báo lỗi | Có | Trang lỗi chung, thông báo đăng nhập một câu cho cả sai tên và sai mật khẩu. |
| 24 | Sao lưu, khôi phục, sẵn sàng | Không | Chưa có quy trình sao lưu. Render free dùng SQLite tạm, dữ liệu mất khi khởi động lại (ghi rõ là bản demo). |
| 25 | Rà soát mã nguồn và cấu hình | Một phần | Đã rà: không có SQL nối chuỗi, không có `Html.Raw`, không có chuỗi kết nối chứa mật khẩu. Còn mật khẩu demo hiện trên trang đăng nhập (cần ẩn khi dùng thật). |

## D. Chống tấn công

| # | Mục | Trạng thái | Ở đâu / ghi chú |
|---|---|---|---|
| 26 | Giảm từ chối dịch vụ mức ứng dụng | Một phần | Giới hạn 10 lần đăng nhập mỗi phút mỗi IP (HTTP 429), giới hạn tệp 20 MB. Chưa giới hạn toàn ứng dụng. |
| 27 | Chống truy vấn làm nghẽn CSDL | Một phần | Kích thước trang tối đa 50, luôn phân trang và `Skip/Take`. Chưa đánh chỉ mục riêng cho tìm kiếm. |
| 28 | Kiểm tra thư viện không an toàn | Một phần | `dotnet list package --vulnerable --include-transitive`: còn một cảnh báo mức cao ở `SQLitePCLRaw.lib.e_sqlite3` (GHSA-2m69-gcr7-jv3q), chưa có bản vá trong NuGet. Chỉ dùng cho SQLite khi chạy container; bản chạy SQL Server không dùng. |
| 29 | Cấu hình an toàn khi tích hợp thư viện | Một phần | Thêm `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, HSTS ngoài Development. Chưa có Content-Security-Policy. |
| 30 | Kiểm thử bảo mật chức năng tải tệp | Có | `SecurityRequirementTests`: tệp văn bản đổi tên `.pdf` và tệp `.exe` đều bị từ chối. |

## Cách kiểm lại

```powershell
dotnet test
dotnet list PostgraduateManagement package --vulnerable --include-transitive
```
