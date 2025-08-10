#!/usr/bin/env pwsh

Write-Host "=== EMAIL TESTING MONITOR ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "🌐 Email Test Page: http://localhost:5261/EmailTest" -ForegroundColor Green
Write-Host "📧 Test Recipient: noahjamal303@gmail.com" -ForegroundColor Green
Write-Host "📋 Configuration: Test Mode Enabled" -ForegroundColor Yellow
Write-Host ""

Write-Host "📝 Instructions:" -ForegroundColor Yellow
Write-Host "1. The application is running on http://localhost:5261" -ForegroundColor White
Write-Host "2. Visit the email test page: http://localhost:5261/EmailTest" -ForegroundColor White
Write-Host "3. Click 'Send Basic Test Email' to test email functionality" -ForegroundColor White
Write-Host "4. Check this terminal for email logs" -ForegroundColor White
Write-Host "5. Check your email: noahjamal303@gmail.com" -ForegroundColor White
Write-Host ""

Write-Host "🔍 Common Email Issues to Check:" -ForegroundColor Yellow
Write-Host "• Email may be in spam/junk folder" -ForegroundColor White
Write-Host "• Gmail may take 1-5 minutes to deliver" -ForegroundColor White
Write-Host "• Check application logs below for errors" -ForegroundColor White
Write-Host "• Verify Gmail app password is correct" -ForegroundColor White
Write-Host ""

Write-Host "📧 Email Configuration Status:" -ForegroundColor Yellow
$appsettings = Get-Content "appsettings.json" | ConvertFrom-Json
$emailSettings = $appsettings.EmailSettings

Write-Host "  Sender: $($emailSettings.SenderEmail)" -ForegroundColor Green
Write-Host "  SMTP: $($emailSettings.SmtpHost):$($emailSettings.SmtpPort)" -ForegroundColor Green
Write-Host "  SSL: $($emailSettings.EnableSsl)" -ForegroundColor Green
Write-Host "  Enabled: $($emailSettings.EnableEmails)" -ForegroundColor Green
Write-Host "  Test Mode: $($emailSettings.TestMode)" -ForegroundColor Green
Write-Host "  Test Recipient: $($emailSettings.TestEmailRecipient)" -ForegroundColor Green
Write-Host ""

Write-Host "🚀 Ready to test! Use the web interface or watch for email logs below." -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
