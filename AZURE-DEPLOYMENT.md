# Azure Deployment Guide for Tournament Management Application

This guide provides instructions for deploying the Tournament Management Application to Azure.

## Prerequisites

- Azure subscription
- Azure CLI installed (optional, for command-line deployment)
- Visual Studio 2022 Professional with Azure development workload installed

## Azure Resources Required

1. **Azure App Service** for hosting the API
2. **Azure SQL Database** for data storage
3. **Azure Static Web Apps** for hosting the Blazor WebAssembly client

## Deployment Steps

### 1. Deploy the Database

1. In the Azure Portal, create a new Azure SQL Database:
   - Server name: `tournaments-sql-server`
   - Database name: `TournamentsDb`
   - Create a new admin user and password
   - Configure performance tier as needed

2. Configure firewall rules to allow access from:
   - Your development machine
   - Azure services

3. Get the connection string from the Azure Portal

### 2. Update API Configuration

1. Update the connection string in `Tournaments.Api/appsettings.Production.json`:
   ```json
   "ConnectionStrings": {
     "TournamentsApiContext": "Server=tournaments-sql-server.database.windows.net;Database=TournamentsDb;User Id=your_username;Password=your_password;TrustServerCertificate=True;"
   }
   ```

### 3. Deploy the API to Azure App Service

#### Using Visual Studio:

1. Right-click on the `Tournaments.Api` project in Solution Explorer
2. Select "Publish..."
3. Choose "Azure" as the target
4. Select "Azure App Service (Windows)" as the specific target
5. Choose "Create new" or select an existing App Service
6. Configure the App Service:
   - Name: `tournaments-api`
   - Subscription: Your Azure subscription
   - Resource Group: Create new or use existing
   - Hosting Plan: Create new or use existing
7. Click "Create" and wait for the App Service to be created
8. Click "Publish" to deploy the API

### 4. Deploy the Web App to Azure Static Web Apps

#### Using Visual Studio:

1. Right-click on the `Tournaments.Web` project in Solution Explorer
2. Select "Publish..."
3. Choose "Azure" as the target
4. Select "Azure Static Web App" as the specific target
5. Configure the Static Web App:
   - Name: `tournaments-web-app`
   - Subscription: Your Azure subscription
   - Resource Group: Same as API
   - Plan type: Free
6. Click "Create" and wait for the Static Web App to be created
7. Click "Publish" to deploy the Web App

### 5. Configure CORS in the API

1. In the Azure Portal, navigate to your API App Service
2. Go to "CORS" in the left menu
3. Add the URL of your Static Web App (e.g., `https://tournaments-web-app.azurestaticapps.net`)
4. Save the changes

## Switching Between Local and Azure Environments

### Using Visual Studio:

1. Use the Solution Configuration dropdown in the toolbar to switch between "Local" and "Azure"
2. Use the Launch Profile dropdown to select the appropriate profile for each project

### Configuration Details:

- **Local**: Uses Development environment, connects to local SQL Server
- **Azure**: Uses Production environment, connects to Azure SQL Database

## Troubleshooting

### Common Issues:

1. **Database Connection Errors**:
   - Check the connection string in `appsettings.Production.json`
   - Verify firewall rules in Azure SQL Database

2. **CORS Errors**:
   - Ensure the Static Web App URL is added to the CORS configuration in the API App Service
   - Check for any typos in the URL

3. **Deployment Failures**:
   - Check the deployment logs in Visual Studio
   - Check the logs in the Azure Portal for the specific resource

## Maintenance

### Updating the Deployment:

1. Make changes to the codebase
2. Switch to the "Azure" configuration in Visual Studio
3. Test locally to ensure everything works
4. Publish the updated projects to Azure

### Monitoring:

1. Use Azure Application Insights for monitoring (optional)
2. Check the logs in the Azure Portal for each resource 