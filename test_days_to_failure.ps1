# Test Days to Failure Feature
# This script tests the ML Dashboard with the new "Days to Failure" enhancement

Write-Host "🧪 Testing Days to Failure Feature" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Test the ML Dashboard API endpoint
$dashboardUrl = "https://localhost:7094/MLDashboard/GetDashboardData"

try {
    Write-Host "📊 Testing ML Dashboard endpoint..." -ForegroundColor Yellow
    
    # Test with curl to avoid SSL certificate issues
    $response = curl -k -s $dashboardUrl
    
    if ($response) {
        Write-Host "✅ Dashboard endpoint responding" -ForegroundColor Green
        
        # Check if response contains days to failure data
        if ($response -match "daysToFailure" -and $response -match "daysToFailureDisplay" -and $response -match "maintenanceUrgency") {
            Write-Host "✅ Days to Failure data present in response" -ForegroundColor Green
            Write-Host "🎯 Feature implementation successful!" -ForegroundColor Green
        } else {
            Write-Host "❌ Days to Failure data missing from response" -ForegroundColor Red
            Write-Host "Response preview: $($response.Substring(0, [Math]::Min(200, $response.Length)))" -ForegroundColor Gray
        }
    } else {
        Write-Host "❌ No response from dashboard endpoint" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error testing dashboard: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 Expected Features:" -ForegroundColor Cyan
Write-Host "• Days to Failure calculation based on failure probability" -ForegroundColor White
Write-Host "• Maintenance urgency classification" -ForegroundColor White
Write-Host "• Enhanced dashboard tables with failure timeline" -ForegroundColor White
Write-Host "• Real-time predictions with scheduling guidance" -ForegroundColor White

Write-Host ""
Write-Host "🔧 Feature Mapping:" -ForegroundColor Cyan
Write-Host "• Critical (90%+): 7 days to failure" -ForegroundColor Red
Write-Host "• High (70-89%): 30 days to failure" -ForegroundColor DarkYellow
Write-Host "• Medium (40-69%): 90 days to failure" -ForegroundColor Yellow
Write-Host "• Low (<40%): 365 days to failure" -ForegroundColor Green

Write-Host ""
Write-Host "Test completed! Check the dashboard at https://localhost:7094/MLDashboard for visual confirmation." -ForegroundColor Magenta
