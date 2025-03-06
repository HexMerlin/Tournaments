# SetupSolutionConfigurations.ps1
# This script provides instructions for setting up solution configurations in Visual Studio 2022

Write-Host "Tournament Management Application - Solution Configuration Setup" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Green
Write-Host ""
Write-Host "This script provides instructions for setting up solution configurations in Visual Studio 2022." -ForegroundColor Yellow
Write-Host ""
Write-Host "Please follow these steps manually in Visual Studio:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Right-click on the solution in Solution Explorer" -ForegroundColor White
Write-Host "2. Select 'Configuration Manager...'" -ForegroundColor White
Write-Host "3. In the 'Active solution configuration' dropdown, click 'New...'" -ForegroundColor White
Write-Host "4. Enter 'Local' as the name" -ForegroundColor White
Write-Host "5. Copy settings from 'Debug'" -ForegroundColor White
Write-Host "6. Click OK" -ForegroundColor White
Write-Host ""
Write-Host "7. Repeat steps 3-6 to create an 'Azure' configuration, copying from 'Release'" -ForegroundColor White
Write-Host ""
Write-Host "After setting up the configurations, you can switch between them using:" -ForegroundColor Cyan
Write-Host "- The Solution Configuration dropdown in the Visual Studio toolbar" -ForegroundColor White
Write-Host "- The Launch Profile dropdown in the Visual Studio toolbar" -ForegroundColor White
Write-Host ""
Write-Host "Local Configuration:" -ForegroundColor Green
Write-Host "- Uses Development environment" -ForegroundColor White
Write-Host "- Connects to local SQL Server" -ForegroundColor White
Write-Host "- Web app connects to local API" -ForegroundColor White
Write-Host ""
Write-Host "Azure Configuration:" -ForegroundColor Green
Write-Host "- Uses Production environment" -ForegroundColor White
Write-Host "- Connects to Azure SQL Database" -ForegroundColor White
Write-Host "- Web app connects to Azure-hosted API" -ForegroundColor White
Write-Host ""
Write-Host "Note: The IsDevelopment() method in the API will return:" -ForegroundColor Yellow
Write-Host "- true when using the Local configuration" -ForegroundColor White
Write-Host "- false when using the Azure configuration" -ForegroundColor White
Write-Host ""
Write-Host "Configuration complete! You can now switch between Local and Azure environments." -ForegroundColor Green 