BEGIN TRANSACTION;
GO

CREATE TABLE [ItemsTable2] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    CONSTRAINT [PK_ItemsTable2] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220329144921_22', N'5.0.0');
GO

COMMIT;
GO

