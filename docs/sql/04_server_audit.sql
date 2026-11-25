/* ============================================================================
   Mục 5 và 8: SQL Server Audit ghi ra file (đăng nhập thất bại, đọc/sửa/xóa
   đề tài, thay đổi cấu trúc bảng).
   Trước khi chạy: tạo thư mục C:\SqlAudit và chỉ cho tài khoản dịch vụ SQL Server
   cùng quản trị viên quyền ghi/đọc, để người dùng thường không xóa được log.
   ========================================================================== */
USE [master];
GO
-- Gỡ cấu hình cũ để chạy lại được
IF EXISTS (SELECT 1 FROM sys.server_audit_specifications WHERE name = 'LmsServerSpec')
BEGIN
    ALTER SERVER AUDIT SPECIFICATION LmsServerSpec WITH (STATE = OFF);
    DROP SERVER AUDIT SPECIFICATION LmsServerSpec;
END
GO
USE [PostgraduateManagementDb];
GO
IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = 'LmsDbSpec')
BEGIN
    ALTER DATABASE AUDIT SPECIFICATION LmsDbSpec WITH (STATE = OFF);
    DROP DATABASE AUDIT SPECIFICATION LmsDbSpec;
END
GO
USE [master];
GO
IF EXISTS (SELECT 1 FROM sys.server_audits WHERE name = 'LmsAudit')
BEGIN
    ALTER SERVER AUDIT LmsAudit WITH (STATE = OFF);
    DROP SERVER AUDIT LmsAudit;
END
GO
CREATE SERVER AUDIT LmsAudit
TO FILE (FILEPATH = 'C:\SqlAudit\', MAXSIZE = 50 MB, MAX_ROLLOVER_FILES = 5)
WITH (ON_FAILURE = CONTINUE);
GO
ALTER SERVER AUDIT LmsAudit WITH (STATE = ON);
GO
-- Cấp máy chủ: đăng nhập thất bại, đổi mật khẩu login
CREATE SERVER AUDIT SPECIFICATION LmsServerSpec
FOR SERVER AUDIT LmsAudit
    ADD (FAILED_LOGIN_GROUP),
    ADD (LOGIN_CHANGE_PASSWORD_GROUP)
WITH (STATE = ON);
GO
USE [PostgraduateManagementDb];
GO
-- Cấp database: đọc/sửa/xóa bảng Theses, đổi cấu trúc đối tượng
CREATE DATABASE AUDIT SPECIFICATION LmsDbSpec
FOR SERVER AUDIT LmsAudit
    ADD (SELECT, UPDATE, DELETE ON OBJECT::dbo.Theses BY public),
    ADD (SCHEMA_OBJECT_CHANGE_GROUP)
WITH (STATE = ON);
GO

/* ============================ KIỂM TRA ===================================== */
-- Mô phỏng: đọc bảng nhạy cảm, một lệnh DDL, rồi đọc log
SELECT COUNT(*) AS SoDeTai FROM dbo.Theses;
IF OBJECT_ID('dbo.AuditDemo') IS NOT NULL DROP TABLE dbo.AuditDemo;
CREATE TABLE dbo.AuditDemo (Id int);
DROP TABLE dbo.AuditDemo;
GO
-- Log phải có người thực hiện, thời điểm và đối tượng bị tác động
WAITFOR DELAY '00:00:03';   -- chờ hàng đợi audit ghi xuống file
SELECT TOP 10 event_time AS ThoiDiem, action_id AS HanhDong, server_principal_name AS NguoiThucHien,
       database_name AS Database_, object_name AS DoiTuong, LEFT(statement, 60) AS Lenh
FROM sys.fn_get_audit_file('C:\SqlAudit\*.sqlaudit', DEFAULT, DEFAULT)
ORDER BY event_time DESC;
GO
