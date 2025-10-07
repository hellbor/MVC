IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251007131852_mssql.local_migration_852'
)
BEGIN
    CREATE TABLE [Courses] (
        [CourseID] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Credits] int NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251007131852_mssql.local_migration_852'
)
BEGIN
    CREATE TABLE [Students] (
        [ID] int NOT NULL IDENTITY,
        [LastName] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [EnrollmentDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251007131852_mssql.local_migration_852'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [EnrollmentID] int NOT NULL IDENTITY,
        [CourseID] int NOT NULL,
        [StudentID] int NOT NULL,
        [Grade] int NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([EnrollmentID]),
        CONSTRAINT [FK_Enrollments_Courses_CourseID] FOREIGN KEY ([CourseID]) REFERENCES [Courses] ([CourseID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Enrollments_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [Students] ([ID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251007131852_mssql.local_migration_852'
)
BEGIN
    CREATE INDEX [IX_Enrollments_CourseID] ON [Enrollments] ([CourseID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251007131852_mssql.local_migration_852'
)
BEGIN
    CREATE INDEX [IX_Enrollments_StudentID] ON [Enrollments] ([StudentID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251007131852_mssql.local_migration_852'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251007131852_mssql.local_migration_852', N'9.0.9');
END;

COMMIT;
GO

