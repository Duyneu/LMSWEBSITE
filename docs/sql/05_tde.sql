/* ============================================================================
   Mục 6: Mã hóa database (Transparent Data Encryption).
   YÊU CẦU: SQL Server Developer, Standard hoặc Enterprise. Express và LocalDB
   KHÔNG hỗ trợ TDE, lệnh ENCRYPTION ON sẽ báo lỗi. Script này dùng database
   thử TdeDemo, không đụng vào PostgraduateManagementDb.
   ========================================================================== */
USE [master];
GO
IF DB_ID('TdeDemo') IS NULL CREATE DATABASE TdeDemo;
GO
-- 1. Master key và certificate nằm trong master
IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = '$(MasterKeyPassword)';
IF NOT EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'TdeCert')
    CREATE CERTIFICATE TdeCert WITH SUBJECT = 'TDE certificate cho TdeDemo';
GO
-- 2. Sao lưu certificate cùng khóa riêng, lưu tách khỏi file dữ liệu và file backup
-- BACKUP CERTIFICATE TdeCert TO FILE = 'C:\SqlBackup\TdeCert.cer'
--     WITH PRIVATE KEY (FILE = 'C:\SqlBackup\TdeCert.pvk', ENCRYPTION BY PASSWORD = '$(CertBackupPassword)');
GO
USE [TdeDemo];
GO
-- 3. Database encryption key và bật mã hóa
IF NOT EXISTS (SELECT 1 FROM sys.dm_database_encryption_keys WHERE database_id = DB_ID())
    CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256 ENCRYPTION BY SERVER CERTIFICATE TdeCert;
GO
ALTER DATABASE TdeDemo SET ENCRYPTION ON;
GO
-- 4. Kiểm tra: encryption_state = 3 nghĩa là đã mã hóa
SELECT DB_NAME(database_id) AS DatabaseName, encryption_state, percent_complete, key_algorithm, key_length
FROM sys.dm_database_encryption_keys;
GO
-- 5. Khôi phục sang instance khác khi thiếu certificate phải thất bại; có đủ certificate thì thành công.
