Ascendly AI
Ascendly AI is a career intelligence platform that helps candidates understand how well their resume fits a role, improve their application, and practice for the interview that comes next.
The project started with a simple idea: instead of treating resume screening and interview preparation as two separate problems, connect them into one practical workflow.
Live: https://ascendlyai.in
API: https://api.ascendlyai.in
What Ascendly AI Does
Ascendly currently focuses on two core experiences:
Resume Analyzer
Upload a resume and a target job description to get a structured analysis covering:
ATS score
Resume match
Formatting score
Keyword match
Matched and missing keywords
Direct and transferable matches
Genuine skill gaps
Humanization suggestions
ATS tailoring guidance
Application recommendation
Final application readiness
The system separates deterministic work, such as PDF validation and text extraction, from AI-based contextual reasoning.
Role-Agnostic Mock Interview
The mock interview is designed to work beyond software interviews.
A candidate can choose:
Target role
Resume (optional)
Job description (optional)
Custom topics (optional)
Interview type
Difficulty
Duration
Interview style
Interviewer role
Interviewer context
Ascendly generates a complete interview blueprint once and stores the session, topics, and questions in PostgreSQL.
The interview then runs question by question:
Interview Setup
      ↓
AI-generated Interview Blueprint
      ↓
Topic
      ↓
Question
      ↓
Candidate Answer
      ↓
Question-level Evaluation
      ↓
Next Topic
      ↓
Final Interview Evaluation
Each evaluated question can contain:
Score
Strengths
Mistakes
Improvements
Better Phrase
The final interview report summarizes:
Overall score
Communication
Technical / domain performance
Problem solving
Behavioral performance
Role alignment
Strongest areas
Areas to improve
Repeated mistakes
Preparation plan
Interview readiness
Why I Built It
A lot of career tools stop at a score.
I wanted Ascendly to answer the more useful questions:
What am I missing for this role?
What should I change in my resume?
How would I perform if the interview started today?
What should I practice next?
That is the direction behind the project.
Core Product Flow
                    ASCENDLY AI

             ┌─────────────────────┐
             │   Resume + Job JD   │
             └──────────┬──────────┘
                        ↓
              ┌───────────────────┐
              │  Resume Analyzer  │
              └─────────┬─────────┘
                        ↓
        ┌─────────────────────────────────┐
        │ ATS + Match + Gaps + Guidance  │
        └────────────────┬────────────────┘
                         ↓
              ┌───────────────────┐
              │  Mock Interview   │
              └─────────┬─────────┘
                        ↓
             Questions + Answers
                        ↓
             Question-level Feedback
                        ↓
             Final Interview Report
The idea is to turn analysis into action rather than stopping at a number.
Tech Stack
Frontend
Angular
TypeScript
Standalone Components
Angular Signals
Reactive Forms
Angular Router
HttpClient
HTTP Interceptors
Route Guards
Bootstrap
Markdown support
Prism syntax highlighting
Backend
ASP.NET Core Web API
.NET 8
C#
Dependency Injection
Middleware
REST APIs
JWT authentication
Refresh-token rotation
Role-based authorization
Repository Pattern where used
Data
PostgreSQL
SQL Server
Azure SQL Database
Entity Framework Core
Dapper
LINQ
Stored Procedures
Views
Joins
Indexes
AI / Document Processing
Gemini API
Structured JSON AI responses
PdfPig PDF text extraction
Cloud / Tools
Azure
Render
Vercel
Docker
Cloudinary
Git
GitHub
Swagger / OpenAPI
Postman
Visual Studio
VS Code
Architecture
The backend is split into separate responsibilities instead of keeping everything inside controllers.
Frontend (Angular)
        │
        ▼
ASP.NET Core API
        │
        ├── Controllers
        │
        ├── Application
        │      ├── DTOs
        │      └── Interfaces
        │
        ├── Domain
        │      └── Entities
        │
        └── Infrastructure
               ├── Services
               ├── Persistence
               └── EF Core Migrations
AI boundary
Gemini is kept behind application-level abstractions.
That means the rest of the system does not need to know the details of the Gemini HTTP API.
The general approach is:
Deterministic backend work
- validation
- PDF parsing
- persistence
- objective signals
- API orchestration

            +

AI reasoning
- semantic role matching
- contextual gap analysis
- interview generation
- answer evaluation
- preparation guidance
This keeps model-dependent reasoning isolated from the core application flow.
Authentication & Security
Ascendly includes a complete authentication flow built around short-lived access tokens and refresh tokens.
The authentication system includes:
Email verification
BCrypt password hashing
Short-lived JWT access tokens
Refresh-token rotation
HttpOnly / Secure cookies
Angular route guards
JWT HTTP interceptor
Automatic token refresh
Protection against concurrent refresh requests
Server-side logout revocation
Role-based authorization
The goal was to make authentication secure without making the application difficult to use.
Mock Interview Data Model
Mock Interview state is persisted in PostgreSQL rather than kept only in application memory.
User
 │
 └── InterviewSession
       │
       └── InterviewTopic
              │
              └── InterviewQuestion
                    ├── InterviewAnswer
                    └── QuestionEvaluation
       │
       └── FinalInterviewEvaluation
This allows the interview to retain its:
configuration
generated questions
candidate answers
question evaluations
final evaluation
completion state
The database schema is managed with Entity Framework Core migrations.
API Endpoints
Resume Analyzer
POST /api/Resume/analyze
Accepts the resume PDF and job description and returns the structured analysis.
Mock Interview
POST /api/Interview/start
POST /api/Interview/{sessionId}/answer
POST /api/Interview/{sessionId}/evaluate-topic/{topicIndex}
POST /api/Interview/{sessionId}/complete
The interview is intentionally designed to avoid one AI request per question.
The main flow is:
1 generation call
+
1 evaluation call per completed topic
+
1 final evaluation call
This keeps the workflow more practical for API usage and avoids unnecessary model calls.
Database Migrations
The project uses EF Core migrations.
From the backend directory:
dotnet ef migrations add <MigrationName> `
  --project Ascendly.Infrastructure/Ascendly.Infrastructure.csproj `
  --startup-project Ascendly.API/Ascendly.API.csproj
Apply migrations:
dotnet ef database update `
  --project Ascendly.Infrastructure/Ascendly.Infrastructure.csproj `
  --startup-project Ascendly.API/Ascendly.API.csproj
Never commit API keys or other production secrets.
Development secrets are kept outside the repository.
Running Locally
Backend
From:
backend/
Build:
dotnet build
Run the API:
dotnet run --project Ascendly.API --launch-profile http
Swagger is available from the running API.
Frontend
From:
frontend/
Install dependencies:
npm install
Run locally:
ng serve
The Angular application then runs on the local development server.
Configuration
The application uses environment-based configuration for values such as:
Database connection strings
JWT settings
Email configuration
Gemini API key / model configuration
Cloud service configuration
Frontend API URL
Do not place production secrets directly in source-controlled files.
Production
Ascendly has a deployed frontend and backend environment.
Frontend
https://ascendlyai.in

Backend API
https://api.ascendlyai.in
The application uses environment-based production configuration so the same codebase can run against the appropriate local or production services.
Engineering Decisions
A few decisions in this project were intentional:
Generate the interview blueprint once
Questions are generated once and stored.
The application does not ask the AI model to invent the next question every time the candidate clicks Next.
Evaluate a topic's questions together
When a topic is complete, its questions and answers are evaluated together in one AI call.
The stored result is still question-level feedback.
Keep topic grouping separate from scoring
Topics organize the interview experience.
The product does not maintain a separate topic-score database entity.
Keep the final report separate
The final report is an aggregate assessment.
It does not repeat every question and answer already shown on the topic evaluation screens.
Current Project Structure
AscendlyAI/
│
├── backend/
│   ├── Ascendly.API/
│   ├── Ascendly.Application/
│   ├── Ascendly.Domain/
│   └── Ascendly.Infrastructure/
│
├── frontend/
│   └── src/app/
│
└── AscendlyAI.sln
What I Learned Building It
This project ended up being much more than wiring an LLM to a web application.
Some of the biggest lessons were:
Designing application boundaries before adding features
Building secure authentication instead of treating JWT as a single feature
Working with EF Core migrations and PostgreSQL
Combining EF Core and Dapper for different data-access needs
Keeping deterministic processing separate from LLM reasoning
Handling structured AI responses instead of relying on free-form text
Designing Angular state around asynchronous API workflows
Persisting interview state so the application does not depend on in-memory state
Thinking about failure states, token refresh, duplicate requests, and user experience
Deploying and troubleshooting the application in a real cloud environment
Project Status
Ascendly AI is a working, deployed project.
The core product currently includes:
Resume Analyzer
Role-agnostic Mock Interview
Question-level interview feedback
Final interview readiness report
Secure authentication
PostgreSQL persistence
Gemini AI integration
Angular frontend
ASP.NET Core backend
Cloud deployment
Author
Gaurav Vennamwar
Full Stack .NET Developer
Portfolio: https://gaurav-portfolio-woad.vercel.app/
GitHub: https://github.com/Gaurav-Vennamwar
LinkedIn: https://linkedin.com/in/gaurav-vennamwar-0b79b0212
Final Note
Ascendly AI is a project I built to solve a problem I was actively facing myself:
understanding where I stand for a role, improving my application, and becoming better prepared for the interview.
It is still an evolving engineering project, but the core product is live and usable.
