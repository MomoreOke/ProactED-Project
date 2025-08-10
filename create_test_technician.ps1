# PowerShell script to create a test technician user
Write-Host "Creating test technician user..." -ForegroundColor Green

# SQL to insert a test technician user
$connectionString = "Server=localhost;Database=FeenaloFinaleMaster;Trusted_Connection=true;TrustServerCertificate=true;"

$sql = @"
-- First check if user already exists
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'noahjamal303@gmail.com')
BEGIN
    -- Insert the test technician user
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
        AccessFailedCount, FirstName, LastName, IsEmailVerified
    ) VALUES (
        NEWID(),
        'TestTechnician',
        'TESTTECHNICIAN',
        'noahjamal303@gmail.com',
        'NOAHJAMAL303@GMAIL.COM',
        1,
        'AQAAAAIAAYagAAAAEGFfQvqhAAF3n0n6hBf8N+QhMfcpQ8QhMf8==', -- Dummy password hash
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        'Noah',
        'Jamal',
        1
    );
    
    -- Get the user ID
    DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'noahjamal303@gmail.com');
    
    -- Add to Technician role (assuming the role exists)
    IF EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Technician')
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        SELECT @UserId, Id FROM AspNetRoles WHERE Name = 'Technician';
    END
    ELSE
    BEGIN
        -- If Technician role doesn't exist, create it first
        DECLARE @RoleId NVARCHAR(450) = NEWID();
        INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (@RoleId, 'Technician', 'TECHNICIAN');
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
    END
    
    PRINT 'Test technician user created successfully with email: noahjamal303@gmail.com';
END
ELSE
BEGIN
    PRINT 'User with email noahjamal303@gmail.com already exists';
END
"@

try {
    Write-Host "Connecting to database..." -ForegroundColor Yellow
    
    # Execute the SQL
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $sql -QueryTimeout 30
    
    Write-Host "✅ Test technician user setup completed!" -ForegroundColor Green
    Write-Host "Email: noahjamal303@gmail.com" -ForegroundColor Cyan
    Write-Host "Role: Technician" -ForegroundColor Cyan
}
catch {
    Write-Host "❌ Error creating test technician: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "You may need to manually create a user with email 'noahjamal303@gmail.com' and assign Technician role" -ForegroundColor Yellow
}

Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
