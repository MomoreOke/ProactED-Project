# Simple email test
param([string]$TestType = "basic")

Write-Host "Testing ProactED Email Service" -ForegroundColor Cyan

$BaseUrl = "http://localhost:5261"
$endpoint = "/EmailTest/TestBasicEmail"

try {
    Write-Host "Sending test email..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "$BaseUrl$endpoint" -Method POST -UseBasicParsing
    
    if ($response.StatusCode -eq 200) {
        Write-Host "SUCCESS: Email test completed (Status 200)" -ForegroundColor Green
        Write-Host "Check noahjamal303@gmail.com for the test email" -ForegroundColor White
    } else {
        Write-Host "FAILED: Status $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Email Configuration:" -ForegroundColor Cyan
Write-Host "- Sender: jamalnabila3709@gmail.com"
Write-Host "- Recipient: noahjamal303@gmail.com" 
Write-Host "- Test Mode: Enabled"
