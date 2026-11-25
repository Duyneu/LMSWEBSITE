/* ============================================================================
   Mục 3: Kiểm soát truy cập theo dòng (Row-Level Security) trên bảng Theses.
   Mỗi học viên chỉ thấy đề tài của mình; Manager và chủ database thấy tất cả.
   Chạy sau 01_least_privilege.sql.
   ========================================================================== */
USE [PostgraduateManagementDb];
GO
-- Gỡ chính sách cũ để chạy lại được
IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'ThesisPolicy')
    DROP SECURITY POLICY Security.ThesisPolicy;
GO
IF SCHEMA_ID('Security') IS NULL EXEC('CREATE SCHEMA Security');
GO
-- Bảng ánh xạ: người dùng database nào được xem học viên nào
IF OBJECT_ID('dbo.UserThesisScope') IS NULL
    CREATE TABLE dbo.UserThesisScope (
        DbUserName sysname NOT NULL,
        StudentId  int     NOT NULL,
        CONSTRAINT PK_UserThesisScope PRIMARY KEY (DbUserName, StudentId)
    );
GO
-- Người dùng học viên mẫu, không có login (chỉ để thử bằng EXECUTE AS)
IF USER_ID('lms_student_a') IS NULL CREATE USER lms_student_a WITHOUT LOGIN;
IF USER_ID('lms_student_b') IS NULL CREATE USER lms_student_b WITHOUT LOGIN;
GRANT SELECT ON dbo.Theses TO lms_student_a, lms_student_b;
-- Hai học viên đầu tiên trong bảng PostgraduateStudents
DELETE FROM dbo.UserThesisScope WHERE DbUserName IN ('lms_student_a', 'lms_student_b');
INSERT dbo.UserThesisScope (DbUserName, StudentId)
SELECT 'lms_student_a', MIN(Id) FROM dbo.PostgraduateStudents;
INSERT dbo.UserThesisScope (DbUserName, StudentId)
SELECT 'lms_student_b', MIN(Id) FROM dbo.PostgraduateStudents WHERE Id > (SELECT MIN(Id) FROM dbo.PostgraduateStudents);
GO
-- Hàm predicate: chủ database (ứng dụng) và App_Manager thấy tất cả, người khác theo bảng ánh xạ
CREATE OR ALTER FUNCTION Security.fn_ThesisFilter(@StudentId int)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
    SELECT 1 AS Allowed
    WHERE IS_MEMBER('db_owner') = 1
       OR IS_MEMBER('App_Manager') = 1
       OR EXISTS (SELECT 1 FROM dbo.UserThesisScope s
                  WHERE s.DbUserName = USER_NAME() AND s.StudentId = @StudentId);
GO
-- FILTER: lọc dòng khi đọc. BLOCK: chặn sửa dòng thành giá trị ngoài phạm vi.
CREATE SECURITY POLICY Security.ThesisPolicy
    ADD FILTER PREDICATE Security.fn_ThesisFilter(StudentId) ON dbo.Theses,
    ADD BLOCK  PREDICATE Security.fn_ThesisFilter(StudentId) ON dbo.Theses AFTER UPDATE
    WITH (STATE = ON);
GO

/* ============================ KIỂM TRA ===================================== */
DECLARE @n int;
SELECT @n = COUNT(*) FROM dbo.Theses;   -- chạy bằng chủ database
PRINT CONCAT(N'Chủ database thấy: ', @n, N' đề tài');

EXECUTE AS USER = 'lms_student_a';
    SELECT @n = COUNT(*) FROM dbo.Theses;
    PRINT CONCAT(N'lms_student_a thấy: ', @n, N' đề tài');
    -- Câu SELECT không có điều kiện WHERE: policy vẫn phải lọc dòng
    SELECT @n = COUNT(*) FROM dbo.Theses WHERE 1 = 1;
    PRINT CONCAT(N'  (SELECT với WHERE 1 = 1) vẫn chỉ: ', @n, N' đề tài');
REVERT;

EXECUTE AS USER = 'lms_student_b';
    SELECT @n = COUNT(*) FROM dbo.Theses;
    PRINT CONCAT(N'lms_student_b thấy: ', @n, N' đề tài');
REVERT;

EXECUTE AS USER = 'lms_manager_user';
    SELECT @n = COUNT(*) FROM dbo.Theses;
    PRINT CONCAT(N'lms_manager_user (App_Manager) thấy: ', @n, N' đề tài');
REVERT;

SELECT name, is_enabled FROM sys.security_policies;
GO
