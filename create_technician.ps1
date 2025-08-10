# Create a technician user with specific email for testing
Write-Host "=== Creating Technician User ===" -ForegroundColor Green

# Use Entity Framework migrations or direct SQL to add a technician user
$userId = [System.Guid]::NewGuid().ToString()
$email = "noahjamal303@gmail.com"
$userName = "noah.technician"
$firstName = "Noah"
$lastName = "Jamal"
$userNameUpper = $userName.ToUpper()
$emailUpper = $email.ToUpper()

Write-Host "Creating user: $email" -ForegroundColor Yellow

# SQL to insert user directly into database
$sql = @"
INSERT INTO [AspNetUsers] (
    [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], 
    [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
    [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled],
    [AccessFailedCount], [FirstName], [LastName], [IsEmailVerified]
) VALUES (
    '$userId', '$userName', '$userNameUpper', '$email', '$emailUpper', 
    1, 'AQAAAAEAACcQAAAAEJ1234567890abcdefghijk', NEWID(), NEWID(),
    NULL, 0, 0, 1, 0, '$firstName', '$lastName', 1
);

-- Add to Technician role if role exists
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT '$userId', [Id] FROM [AspNetRoles] WHERE [Name] = 'Technician' AND NOT EXISTS (
    SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = '$userId' AND [RoleId] = [AspNetRoles].[Id]
);
"@

Write-Host "SQL Query:" -ForegroundColor Cyan
Write-Host $sql

# Save SQL to file for manual execution
$sql | Out-File -FilePath "create_technician.sql" -Encoding UTF8
Write-Host "`nSQL saved to create_technician.sql" -ForegroundColor Green
Write-Host "You can execute this in SQL Server Management Studio or via sqlcmd" -ForegroundColor Yellow

Write-Host "`nAlternatively, we can use the web interface to register this user..." -ForegroundColor Magenta
