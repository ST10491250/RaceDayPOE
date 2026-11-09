-- Run only against your existing RaceDay database if these Event fields are missing.
-- This adds nullable/defaulted columns and does not delete user data.
IF OBJECT_ID(N'dbo.Events', N'U') IS NOT NULL
BEGIN
 IF COL_LENGTH('dbo.Events','Location') IS NULL ALTER TABLE dbo.Events ADD Location nvarchar(max) NOT NULL CONSTRAINT DF_Events_Location DEFAULT('');
 IF COL_LENGTH('dbo.Events','Distance') IS NULL ALTER TABLE dbo.Events ADD Distance nvarchar(max) NOT NULL CONSTRAINT DF_Events_Distance DEFAULT('');
 IF COL_LENGTH('dbo.Events','EventType') IS NULL ALTER TABLE dbo.Events ADD EventType nvarchar(max) NOT NULL CONSTRAINT DF_Events_EventType DEFAULT('');
 IF COL_LENGTH('dbo.Events','BannerImage') IS NULL ALTER TABLE dbo.Events ADD BannerImage nvarchar(max) NULL;
END;
