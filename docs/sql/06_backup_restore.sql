/* ============================================================================
   Mục 7: Sao lưu và khôi phục an toàn.
   Trước khi chạy: tạo C:\SqlBackup, chỉ cấp quyền NTFS cho tài khoản dịch vụ SQL Server
   và quản trị viên; tài khoản ứng dụng không được đọc thư mục này.
   Sao lưu mã hóa (BACKUP ... WITH ENCRYPTION) cần Standard/Enterprise/Developer và
   một certificate; bản Express/LocalDB chỉ làm được phần CHECKSUM bên dưới.
   ========================================================================== */
USE [master];
GO
-- Full: CHECKSUM phát hiện trang bị hỏng ngay lúc sao lưu
BACKUP DATABASE PostgraduateManagementDb
TO DISK = 'C:\SqlBackup\Lms_full.bak'
WITH INIT, CHECKSUM, STATS = 25;
GO
-- Differential: chỉ phần thay đổi từ lần full gần nhất
BACKUP DATABASE PostgraduateManagementDb
TO DISK = 'C:\SqlBackup\Lms_diff.bak'
WITH DIFFERENTIAL, INIT, CHECKSUM;
GO
-- Transaction log (cần recovery model FULL; mặc định ở model có thể là SIMPLE nên bước này có thể báo lỗi):
-- ALTER DATABASE PostgraduateManagementDb SET RECOVERY FULL;
-- BACKUP LOG PostgraduateManagementDb TO DISK = 'C:\SqlBackup\Lms_log.trn' WITH INIT, CHECKSUM;

-- Mẫu bản mã hóa (Standard trở lên):
-- BACKUP DATABASE PostgraduateManagementDb TO DISK = 'C:\SqlBackup\Lms_full_enc.bak'
--     WITH INIT, CHECKSUM, ENCRYPTION (ALGORITHM = AES_256, SERVER CERTIFICATE = BackupCert);
GO
-- Kiểm tra file sao lưu đọc được và không hỏng, chưa khôi phục
RESTORE VERIFYONLY FROM DISK = 'C:\SqlBackup\Lms_full.bak' WITH CHECKSUM;
GO
-- Khôi phục thử sang tên khác để không đè database đang dùng
DECLARE @data nvarchar(260) = CONVERT(nvarchar(260), SERVERPROPERTY('InstanceDefaultDataPath')) + 'LmsRestoreTest.mdf';
DECLARE @log  nvarchar(260) = CONVERT(nvarchar(260), SERVERPROPERTY('InstanceDefaultLogPath'))  + 'LmsRestoreTest_log.ldf';
DECLARE @sql nvarchar(max);
SELECT @sql = N'RESTORE DATABASE LmsRestoreTest FROM DISK = ''C:\SqlBackup\Lms_full.bak'' WITH REPLACE, CHECKSUM, '
            + (SELECT STRING_AGG(CONCAT(N'MOVE ''', name, N''' TO ''',
                     CASE type_desc WHEN 'ROWS' THEN @data ELSE @log END, N''''), N', ')
               FROM sys.master_files WHERE database_id = DB_ID('PostgraduateManagementDb'));
EXEC (@sql);
GO
SELECT COUNT(*) AS SoDeTaiSauKhoiPhuc FROM LmsRestoreTest.dbo.Theses;
GO
DROP DATABASE LmsRestoreTest;
GO
