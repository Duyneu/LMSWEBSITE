/* ============================================================================
   Mục 8: Giám sát tài khoản người dùng và tài khoản hệ thống.
   Các truy vấn chỉ đọc, chạy bằng tài khoản quản trị.
   ========================================================================== */
USE [master];
GO
-- 1. Chính sách mật khẩu, hạn dùng, trạng thái disable của mọi SQL login (kể cả sa)
SELECT name, is_disabled, is_policy_checked, is_expiration_checked, create_date, modify_date,
       LOGINPROPERTY(name, 'BadPasswordCount') AS SoLanSaiMatKhau,
       LOGINPROPERTY(name, 'IsLocked')         AS DangBiKhoa,
       LOGINPROPERTY(name, 'LockoutTime')      AS ThoiDiemKhoa
FROM sys.sql_logins
ORDER BY is_disabled, name;
GO
-- 2. Cảnh báo: sa còn bật, login không kiểm tra chính sách mật khẩu
SELECT name, N'sa đang bật: nên disable nếu không cần' AS CanhBao FROM sys.sql_logins WHERE name = 'sa' AND is_disabled = 0
UNION ALL
SELECT name, N'không áp dụng chính sách mật khẩu' FROM sys.sql_logins WHERE is_policy_checked = 0 AND name NOT LIKE '##%';
GO
-- 3. Thành viên các vai trò quyền cao (sysadmin, securityadmin)
SELECT r.name AS VaiTro, m.name AS ThanhVien, m.type_desc, m.is_disabled
FROM sys.server_role_members rm
JOIN sys.server_principals r ON r.principal_id = rm.role_principal_id
JOIN sys.server_principals m ON m.principal_id = rm.member_principal_id
WHERE r.name IN ('sysadmin', 'securityadmin')
ORDER BY r.name, m.name;
GO
-- 4. Đăng nhập sai trong SQL Server error log (instance đầy đủ như SQLEXPRESS; LocalDB không có error log nên lệnh này báo lỗi)
EXEC sys.xp_readerrorlog 0, 1, N'Login failed';
GO
-- 5. Đăng nhập sai và đăng nhập ngoài giờ qua Server Audit (cần 04_server_audit.sql)
SELECT event_time, server_principal_name, client_ip, succeeded, LEFT(statement, 80) AS ChiTiet
FROM sys.fn_get_audit_file('C:\SqlAudit\*.sqlaudit', DEFAULT, DEFAULT)
WHERE action_id = 'LGIF'                                   -- LOGIN FAILED
   OR DATEPART(HOUR, event_time) NOT BETWEEN 7 AND 19      -- ngoài khung giờ làm việc (theo giờ UTC của log)
ORDER BY event_time DESC;
GO
-- 6. Tài khoản không còn dùng thì disable, không xóa vội để còn truy vết
-- ALTER LOGIN lms_support_login DISABLE;
GO
