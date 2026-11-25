/* ============================================================================
   Mục 4: Che dữ liệu nhạy cảm (Dynamic Data Masking).
   Masking chỉ che khi hiển thị; nó không thay cho mã hóa hay phân quyền.
   Chạy sau 01_least_privilege.sql.
   ========================================================================== */
USE [PostgraduateManagementDb];
GO
ALTER TABLE dbo.PostgraduateStudents ALTER COLUMN Email       ADD MASKED WITH (FUNCTION = 'email()');
ALTER TABLE dbo.PostgraduateStudents ALTER COLUMN PhoneNumber ADD MASKED WITH (FUNCTION = 'partial(2, "XXXXXX", 2)');
ALTER TABLE dbo.AcademicPersons      ALTER COLUMN Email       ADD MASKED WITH (FUNCTION = 'email()');
ALTER TABLE dbo.AcademicPersons      ALTER COLUMN PhoneNumber ADD MASKED WITH (FUNCTION = 'partial(2, "XXXXXX", 2)');
GO
-- Chỉ vai trò được xem bản rõ mới có UNMASK
GRANT UNMASK TO App_Manager;
GRANT SELECT ON dbo.AcademicPersons TO App_Support;
GO

/* ============================ KIỂM TRA ===================================== */
PRINT N'--- App_Support (không có UNMASK): dữ liệu đã che ---';
EXECUTE AS USER = 'lms_support_user';
    SELECT TOP 3 FullName, Email, PhoneNumber FROM dbo.PostgraduateStudents;
REVERT;

PRINT N'--- App_Manager (có UNMASK): bản rõ ---';
EXECUTE AS USER = 'lms_manager_user';
    SELECT TOP 3 FullName, Email, PhoneNumber FROM dbo.PostgraduateStudents;
REVERT;

-- Giới hạn: người có quyền SELECT vẫn có thể suy luận giá trị qua điều kiện lọc
PRINT N'--- Suy luận qua WHERE (giới hạn của masking) ---';
EXECUTE AS USER = 'lms_support_user';
    SELECT COUNT(*) AS SoDongKhopEmailDoan FROM dbo.PostgraduateStudents WHERE Email LIKE 'a%';
REVERT;

SELECT OBJECT_NAME(object_id) AS Bang, name AS Cot, masking_function FROM sys.masked_columns;
GO
