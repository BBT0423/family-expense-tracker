# InnoFinite CLI - Next Steps After Project Creation

After creating a new project using the InnoFinite CLI template, follow these steps to set up and start development:

## 1. Environment Setup

### Prerequisites
- **.NET SDK 10.0** or higher
- **Node.js 20** or higher
- **npm** or **yarn** package manager
- **Git** for version control

### Install Dependencies
```bash
# Restore .NET packages
dotnet restore

# Install Node.js dependencies
cd SummaryYearSalary.Web
npm ci
cd ..
```

## 2. API Development

### Run the API
```bash
dotnet run --project SummaryYearSalary.Api
```

The API will be available at `https://localhost:5001` (or configured port)

## 3. Web Frontend Development

### Run Development Server
```bash
cd SummaryYearSalary.Web
npm run dev
```

The application will be available at `http://localhost:5173` (Vite default)

## 4. CI/CD Pipeline Setup

### Required Environment Variables (for Bitbucket)
```
AZURE_PUBLISH_PROFILE                    # Azure App Service deployment profile for API
AZURE_STATIC_WEB_APPS_DEPLOYMENT_TOKEN   # Azure Static Web Apps deployment token for frontend
SONAR_TOKEN                              # SonarCloud authentication token
SONAR_ORGANIZATION                       # SonarCloud organization key
SONAR_PROJECT_KEY                        # SonarCloud project key
```

### Setup Instructions

1. **SonarCloud Setup:**
   - Create an organization and project at [SonarCloud](https://sonarcloud.io)
   - Generate an authentication token in SonarCloud (Settings → Security → Tokens)
   - Note your organization key and project key

2. **Bitbucket Pipeline Variables:**
   - Go to Repository Settings → Pipelines → Environment variables
   - Add all required environment variables listed above

### Pipeline Features
- **SonarCloud Integration** - Automated code quality scanning and quality gate checks
- **Branch Validation** - Enforces naming convention: `feature/KEY-###-description`, `bugfix/KEY-###-description`, `hotfix/KEY-###-description`
- **Pull Request Validation** - Requires PR titles to include Jira key format: `[KEY-###] Description`
- **Parallel Builds** - API and Web builds run in parallel for faster pipeline execution
- **Automated Deployments** - Auto-deployment to Azure on `main` branch
"# family-expense-tracker" 
