# Script T-SQL nhóm A (SQL Server)

Chạy trên database `PostgraduateManagementDb`, theo thứ tự. Với LocalDB:

```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -E -C -f 65001 -v LabPassword="<mật khẩu lab>" -i 01_least_privilege.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -C -f 65001 -i 02_row_level_security.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -C -f 65001 -i 03_dynamic_data_masking.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -C -f 65001 -i 04_server_audit.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -C -f 65001 -i 06_backup_restore.sql
sqlcmd -S "(localdb)\mssqllocaldb" -E -C -f 65001 -i 07_login_monitoring.sql
```

`LabPassword` phải thỏa chính sách mật khẩu Windows (dài, đủ hoa, thường, số, ký tự đặc biệt). Không ghi mật khẩu vào file.
Tạo sẵn thư mục `C:\SqlAudit` và `C:\SqlBackup` trước khi chạy 04 và 06.

| File | Mục | Chạy được trên LocalDB/Express |
|---|---|---|
| `01_least_privilege.sql` | 1 phân quyền tối thiểu, 2 quyền trên bảng, view, procedure | Có, đã chạy và kiểm tra |
| `02_row_level_security.sql` | 3 Row-Level Security | Có, đã chạy: học viên thấy 1/5 đề tài, Manager thấy 5/5 |
| `03_dynamic_data_masking.sql` | 4 Dynamic Data Masking | Có, đã chạy: Support thấy `aXXX@XXXX.com`, Manager thấy bản rõ |
| `04_server_audit.sql` | 5 SQL Server Audit | Có, đã chạy: log có người, thời điểm, đối tượng |
| `05_tde.sql` | 6 Transparent Data Encryption | **Không**: Express/LocalDB báo lỗi 33117. Cần Developer, Standard hoặc Enterprise |
| `06_backup_restore.sql` | 7 sao lưu và khôi phục | Một phần: full, differential, CHECKSUM, VERIFYONLY, restore thử đã chạy; bản mã hóa và log backup để dạng mẫu |
| `07_login_monitoring.sql` | 8 giám sát tài khoản | Có, trừ `xp_readerrorlog` (LocalDB không có error log) |

Ghi chú khi dùng chung với ứng dụng: ứng dụng đăng nhập bằng Windows và là chủ database, nên Row-Level Security cho phép xem tất cả
và Dynamic Data Masking không che dữ liệu với ứng dụng.
