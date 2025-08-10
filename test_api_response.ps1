# Test API Response for Days to Failure Feature
# This script verifies that the ML Dashboard API includes the new failure timeline data

Write-Host "🧪 Testing Days to Failure API Response" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

try {
    # Wait a moment for the application to fully start
    Start-Sleep -Seconds 3
    
    Write-Host "📡 Testing API endpoint: https://localhost:5001/MLDashboard/GetDashboardData" -ForegroundColor Yellow
    
    # Test the API endpoint
    $response = Invoke-RestMethod -Uri "https://localhost:5001/MLDashboard/GetDashboardData" -Method GET -SkipCertificateCheck
    
    if ($response) {
        Write-Host "✅ API endpoint responding successfully" -ForegroundColor Green
        
        # Check if we have high risk equipment data
        if ($response.highRiskEquipment -and $response.highRiskEquipment.Count -gt 0) {
            Write-Host "✅ High-risk equipment data found: $($response.highRiskEquipment.Count) items" -ForegroundColor Green
            
            # Check the first item for Days to Failure properties
            $firstItem = $response.highRiskEquipment[0]
            
            $properties = @('daysToFailure', 'daysToFailureDisplay', 'maintenanceUrgency')
            $allPropertiesFound = $true
            
            foreach ($prop in $properties) {
                if ($firstItem.PSObject.Properties.Name -contains $prop) {
                    Write-Host "✅ Property '$prop' found: $($firstItem.$prop)" -ForegroundColor Green
                } else {
                    Write-Host "❌ Property '$prop' missing" -ForegroundColor Red
                    $allPropertiesFound = $false
                }
            }
            
            if ($allPropertiesFound) {
                Write-Host ""
                Write-Host "🎯 FEATURE VERIFICATION SUCCESSFUL!" -ForegroundColor Green
                Write-Host "Days to Failure feature is working correctly" -ForegroundColor Green
                
                # Show sample data
                Write-Host ""
                Write-Host "📊 Sample Equipment Data:" -ForegroundColor Cyan
                Write-Host "Equipment ID: $($firstItem.equipmentId)" -ForegroundColor White
                Write-Host "Risk Level: $($firstItem.riskLevel)" -ForegroundColor White
                Write-Host "Failure Probability: $($firstItem.failureProbability)" -ForegroundColor White
                Write-Host "Days to Failure: $($firstItem.daysToFailure)" -ForegroundColor Yellow
                Write-Host "Display Text: $($firstItem.daysToFailureDisplay)" -ForegroundColor Yellow
                Write-Host "Maintenance Urgency: $($firstItem.maintenanceUrgency)" -ForegroundColor Yellow
            }
        } else {
            Write-Host "⚠️ No high-risk equipment data found" -ForegroundColor Yellow
        }
        
        # Check recent predictions too
        if ($response.recentPredictions -and $response.recentPredictions.Count -gt 0) {
            Write-Host ""
            Write-Host "✅ Recent predictions data found: $($response.recentPredictions.Count) items" -ForegroundColor Green
            
            $recentItem = $response.recentPredictions[0]
            if ($recentItem.PSObject.Properties.Name -contains 'daysToFailure') {
                Write-Host "✅ Recent predictions also include Days to Failure data" -ForegroundColor Green
            }
        }
        
        Write-Host ""
        Write-Host "📈 Data Source: $($response.dataSource)" -ForegroundColor Cyan
        Write-Host "Total Equipment: $($response.totalEquipment)" -ForegroundColor Cyan
        Write-Host "High Risk Count: $($response.highRisk)" -ForegroundColor Cyan
        
    } else {
        Write-Host "❌ No response from API" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Error testing API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "This is normal if the application is still starting up." -ForegroundColor Gray
}

Write-Host ""
Write-Host "🌐 Dashboard URL: https://localhost:5001/MLDashboard" -ForegroundColor Green
Write-Host "Look for the '📅 Days to Failure' column in both tables!" -ForegroundColor Green
