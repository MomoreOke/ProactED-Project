INSERT INTO [AspNetUsers] (
    [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], 
    [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
    [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled],
    [AccessFailedCount], [FirstName], [LastName], [IsEmailVerified]
) VALUES (
    '6d5cfb5c-f54d-4933-b725-6dc9f6e3995b', 'noah.technician', 'NOAH.TECHNICIAN', 'noahjamal303@gmail.com', 'NOAHJAMAL303@GMAIL.COM', 
    1, 'AQAAAAEAACcQAAAAEJ1234567890abcdefghijk', NEWID(), NEWID(),
    NULL, 0, 0, 1, 0, 'Noah', 'Jamal', 1
);

-- Add to Technician role if role exists
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT '6d5cfb5c-f54d-4933-b725-6dc9f6e3995b', [Id] FROM [AspNetRoles] WHERE [Name] = 'Technician' AND NOT EXISTS (
    SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = '6d5cfb5c-f54d-4933-b725-6dc9f6e3995b' AND [RoleId] = [AspNetRoles].[Id]
);
