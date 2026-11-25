/* ============================================================================
   Mục 1 và 2: Phân quyền đặc quyền tối thiểu; quyền trên bảng, view, procedure.
   Chạy:  sqlcmd -S "(localdb)\mssqllocaldb" -E -C -v LabPassword="<mật khẩu lab>" -i 01_least_privilege.sql
   LabPassword chỉ dùng cho môi trường thực hành, không ghi mật khẩu vào file.
   Chạy lại được nhiều lần (idempotent).
   ========================================================================== */
USE [master];
GO
IF SUSER_ID('lms_reader_login')  IS NULL CREATE LOGIN lms_reader_login  WITH PASSWORD = '$(LabPassword)', CHECK_POLICY = ON;
IF SUSER_ID('lms_manager_login') IS NULL CREATE LOGIN lms_manager_login WITH PASSWORD = '$(LabPassword)', CHECK_POLICY = ON;
IF SUSER_ID('lms_auditor_login') IS NULL CREATE LOGIN lms_auditor_login WITH PASSWORD = '$(LabPassword)', CHECK_POLICY = ON;
IF SUSER_ID('lms_support_login') IS NULL CREATE LOGIN lms_support_login WITH PASSWORD = '$(LabPassword)', CHECK_POLICY = ON;
GO
USE [PostgraduateManagementDb];
GO
-- Người dùng trong database, gắn với login
IF USER_ID('lms_reader_user')  IS NULL CREATE USER lms_reader_user  FOR LOGIN lms_reader_login;
IF USER_ID('lms_manager_user') IS NULL CREATE USER lms_manager_user FOR LOGIN lms_manager_login;
IF USER_ID('lms_auditor_user') IS NULL CREATE USER lms_auditor_user FOR LOGIN lms_auditor_login;
IF USER_ID('lms_support_user') IS NULL CREATE USER lms_support_user FOR LOGIN lms_support_login;
GO
-- Vai trò theo nhóm nghiệp vụ; gán user vào role, không cấp quyền trực tiếp cho user
IF DATABASE_PRINCIPAL_ID('App_Reader')  IS NULL CREATE ROLE App_Reader;   -- chỉ xem báo cáo qua view
IF DATABASE_PRINCIPAL_ID('App_Manager') IS NULL CREATE ROLE App_Manager;  -- nhập liệu, không được xóa
IF DATABASE_PRINCIPAL_ID('App_Auditor') IS NULL CREATE ROLE App_Auditor;  -- đọc nhật ký kiểm toán
IF DATABASE_PRINCIPAL_ID('App_Support') IS NULL CREATE ROLE App_Support;  -- hỗ trợ, thấy dữ liệu đã che
GO
IF IS_ROLEMEMBER('App_Reader',  'lms_reader_user')  = 0 ALTER ROLE App_Reader  ADD MEMBER lms_reader_user;
IF IS_ROLEMEMBER('App_Manager', 'lms_manager_user') = 0 ALTER ROLE App_Manager ADD MEMBER lms_manager_user;
IF IS_ROLEMEMBER('App_Auditor', 'lms_auditor_user') = 0 ALTER ROLE App_Auditor ADD MEMBER lms_auditor_user;
IF IS_ROLEMEMBER('App_Support', 'lms_support_user') = 0 ALTER ROLE App_Support ADD MEMBER lms_support_user;
GO

/* ---- Mục 2: view chỉ hiện các cột được phép xem (không có email, số điện thoại, tóm tắt) ---- */
CREATE OR ALTER VIEW dbo.vw_ThesisSummary AS
    SELECT t.Id, t.Code, t.Title, t.Field, t.ThesisType, t.ThesisStatus, t.StartDate, t.EndDate,
           s.FullName AS StudentName
    FROM dbo.Theses t
    LEFT JOIN dbo.PostgraduateStudents s ON s.Id = t.StudentId;
GO
CREATE OR ALTER VIEW dbo.vw_StudentDirectory AS
    SELECT Id, SharedStudentCode, FullName, Major, EnrollmentYear
    FROM dbo.PostgraduateStudents;
GO

/* ---- Mục 2: procedure có kiểm soát; logic kiểm tra đặt trước khi ghi ---- */
CREATE OR ALTER PROCEDURE dbo.usp_UpdateThesisStatus
    @ThesisId  int,
    @NewStatus int
AS
BEGIN
    SET NOCOUNT ON;
    -- 0..7 là các giá trị hợp lệ của ThesisStatus (Draft ... Archived)
    IF @NewStatus NOT BETWEEN 0 AND 7
        THROW 50001, 'Trạng thái đề tài không hợp lệ.', 1;

    DECLARE @Old int = (SELECT ThesisStatus FROM dbo.Theses WHERE Id = @ThesisId);
    IF @Old IS NULL
        THROW 50002, 'Không tìm thấy đề tài.', 1;
    IF @Old = 7
        THROW 50003, 'Đề tài đã lưu trữ, không được đổi trạng thái.', 1;

    BEGIN TRAN;
        UPDATE dbo.Theses SET ThesisStatus = @NewStatus WHERE Id = @ThesisId;
        -- Ghi nhật ký trong cùng giao dịch với thay đổi
        INSERT dbo.AuditLogs (UserId, UserName, Action, EntityName, EntityId, Details, Timestamp)
        VALUES (CAST(USER_ID() AS nvarchar(450)), SUSER_SNAME(), N'UpdateThesisStatus', N'Thesis', CAST(@ThesisId AS nvarchar(450)),
                CONCAT(N'Trạng thái ', @Old, N' -> ', @NewStatus), SYSUTCDATETIME());
    COMMIT;
END;
GO

/* ---- Cấp quyền: chỉ cấp phần cần dùng, DENY các thao tác vượt quyền ---- */
-- Reader: không có quyền trên bảng; chỉ SELECT view (chuỗi sở hữu dbo cho phép view đọc bảng gốc)
GRANT SELECT ON dbo.vw_ThesisSummary    TO App_Reader;
GRANT SELECT ON dbo.vw_StudentDirectory TO App_Reader;

-- Manager: đọc, thêm, sửa; không xóa; ghi trạng thái đề tài qua procedure
GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo TO App_Manager;
DENY  DELETE                 ON SCHEMA::dbo TO App_Manager;
GRANT EXECUTE ON dbo.usp_UpdateThesisStatus TO App_Manager;

-- Auditor: đọc nhật ký và view; không sửa nhật ký
GRANT SELECT ON dbo.AuditLogs           TO App_Auditor;
GRANT SELECT ON dbo.vw_ThesisSummary    TO App_Auditor;
GRANT SELECT ON dbo.vw_StudentDirectory TO App_Auditor;

-- Support: chỉ đọc bảng học viên (dữ liệu nhạy cảm sẽ bị che ở 04_data_masking.sql)
GRANT SELECT ON dbo.PostgraduateStudents TO App_Support;

-- Bảng tài khoản chứa băm mật khẩu: không vai trò nghiệp vụ nào được đọc
DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUsers TO App_Reader, App_Manager, App_Auditor, App_Support;
-- Nhật ký chỉ được thêm bởi procedure, không ai sửa hoặc xóa trực tiếp
DENY UPDATE, DELETE ON dbo.AuditLogs TO App_Reader, App_Manager, App_Auditor, App_Support;
GO

/* ============================ KIỂM TRA ===================================== */
-- Mỗi bước thao tác vượt quyền PHẢI báo lỗi; thao tác hợp lệ PHẢI chạy được.
DECLARE @t TABLE (Buoc nvarchar(80), KetQua nvarchar(20));
DECLARE @x int;

EXECUTE AS USER = 'lms_reader_user';
    BEGIN TRY SELECT TOP 1 @x = Id FROM dbo.Theses;                  INSERT @t VALUES (N'Reader SELECT bảng Theses', N'SAI: không bị chặn'); END TRY
    BEGIN CATCH                                                INSERT @t VALUES (N'Reader SELECT bảng Theses', N'Bị từ chối (đúng)'); END CATCH
    BEGIN TRY SELECT TOP 1 @x = Id FROM dbo.vw_ThesisSummary;        INSERT @t VALUES (N'Reader SELECT view',        N'Cho phép (đúng)'); END TRY
    BEGIN CATCH                                                INSERT @t VALUES (N'Reader SELECT view',        N'SAI: bị chặn'); END CATCH
    BEGIN TRY DELETE FROM dbo.Theses;                          INSERT @t VALUES (N'Reader DELETE Theses',      N'SAI: không bị chặn'); END TRY
    BEGIN CATCH                                                INSERT @t VALUES (N'Reader DELETE Theses',      N'Bị từ chối (đúng)'); END CATCH
REVERT;

EXECUTE AS USER = 'lms_manager_user';
    BEGIN TRY DELETE FROM dbo.Theses WHERE 1 = 0;              INSERT @t VALUES (N'Manager DELETE Theses',     N'SAI: không bị chặn'); END TRY
    BEGIN CATCH                                                INSERT @t VALUES (N'Manager DELETE Theses',     N'Bị từ chối (đúng)'); END CATCH
    BEGIN TRY SELECT TOP 1 @x = LEN(PasswordHash) FROM dbo.AspNetUsers;  INSERT @t VALUES (N'Manager đọc băm mật khẩu',  N'SAI: không bị chặn'); END TRY
    BEGIN CATCH                                                INSERT @t VALUES (N'Manager đọc băm mật khẩu',  N'Bị từ chối (đúng)'); END CATCH
    BEGIN TRY EXEC dbo.usp_UpdateThesisStatus 1, 99;           INSERT @t VALUES (N'Procedure trạng thái sai',  N'SAI: không bị chặn'); END TRY
    BEGIN CATCH                                                INSERT @t VALUES (N'Procedure trạng thái sai',  N'Bị từ chối (đúng)'); END CATCH
REVERT;

SELECT Buoc AS [Bước kiểm tra], KetQua AS [Kết quả] FROM @t;

-- Đối chiếu quyền thực tế
SELECT USER_NAME(m.member_principal_id) AS NguoiDung, USER_NAME(m.role_principal_id) AS VaiTro
FROM sys.database_role_members m
WHERE USER_NAME(m.role_principal_id) LIKE 'App[_]%'
ORDER BY VaiTro;
SELECT USER_NAME(grantee_principal_id) AS Role, state_desc, permission_name, OBJECT_NAME(major_id) AS DoiTuong
FROM sys.database_permissions
WHERE USER_NAME(grantee_principal_id) LIKE 'App[_]%'
ORDER BY Role, state_desc, permission_name;
GO
