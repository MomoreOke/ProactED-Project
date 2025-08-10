# ProactED PPT Generation Context

Use this document as the complete prompt for generating a 5–10 minute defense presentation deck for a final-year project. The audience includes supervisors and examiners (mixed technical/non‑technical). The team is 3 presenters. The goal is to clearly explain the problem, solution, architecture, workflow, outcomes, and SDG impact, then segue into a live demo.

## Presentation Constraints
- Duration: 5–10 minutes total
- Team: 3 speakers
- Style: clean, minimal, modern; accessible to non‑technical viewers
- Tone: confident, concise, impact-focused
- Visuals: use icons, simple diagrams, and clean charts; avoid dense code on slides

## Desired Deck Structure (Slides and Target Content)
1) Title & Team
- Project: ProactED – Predictive Asset Management & Maintenance Platform
- Team: Nabila, [Member 2], [Member 3]
- One‑line value: Predictive analytics and timetable‑aware maintenance for educational equipment

2) Problem Statement
- Institutions face equipment downtime, manual scheduling, and poor utilization insight
- Timetable data is trapped in PDFs; manual extraction is slow and error‑prone
- Inventory tracking is fragmented; alerts come too late for labs/classes

3) Objectives
- Reduce unplanned downtime through predictive maintenance
- Automate timetable‑based equipment usage tracking
- Optimize stock awareness with low‑stock and reorder alerts
- Provide intuitive dashboards and notifications for stakeholders

4) Proposed Solution (High‑Level)
- ASP.NET Core MVC web app with Entity Framework Core (SQL)
- Python ML microservice for failure prediction (HTTP API)
- Azure Form Recognizer for timetable PDF parsing (tables + text)
- Real‑time UI updates with SignalR; Identity for auth; email notifications

5) Architecture Overview (Diagram)
- Blocks: Web App (ASP.NET MVC), Database (SQL/EF Core), ML API (Python), Azure Form Recognizer, Email/SMTP, SignalR
- Data flow: User → Web UI → services → DB/ML/Form Recognizer → back to dashboards/alerts

6) Key Features
- Predictive Maintenance: on‑demand and batch predictions, risk levels, confidence scores
- Inventory Management: stock levels, min stock thresholds, reorder prompts
- Timetable Integration: upload PDFs; extract rooms/time ranges; map usage to equipment
- Notifications: in‑app and email alerts
- Dashboards & Reports: KPIs, charts, historical prediction tracking
- Model Interpretability: explainability modal from equipment list (brain icon)

7) Methodology (Quick)
- Requirements → MVC/service design → iterative sprints & TDD → ML & Azure integration → testing → deployment
- Challenges handled: ML service reliability (retry/backoff), PDF structure variability (text + table fallback)

8) Workflow: “How It Works”
- Upload timetable PDF → Azure Form Recognizer extracts tables; fallback to text parsing
- Controller parses room codes (e.g., PB001, LAB203) and time ranges (e.g., 09:00–11:00)
- Map room‑level weekly hours to equipment in those rooms → compute per‑semester usage
- Trigger/enable ML failure predictions via Python API; store results; update dashboards
- Alerts fire for high risk and low stock; users act via UI

9) Tools & Technologies
- Backend: ASP.NET Core MVC, C#, Entity Framework Core, SignalR, ASP.NET Identity
- Frontend: Razor + Bootstrap 5, Chart.js, jQuery
- ML: Python (scikit‑learn/FastAPI), prediction API on localhost:5000
- AI Services: Azure Form Recognizer (DocumentAnalysisClient, prebuilt‑document)
- Dev/Test: Postman, xUnit, GitHub Actions; PDF text extraction via iTextSharp

10) Expected Outcomes / Results (targets for defense)
- Downtime reduction via early risk detection (e.g., 20–30% target)
- 90% automation of timetable usage extraction for planning
- Faster data processing (minutes → seconds) vs manual parsing
- Improved lab scheduling and equipment utilization visibility

11) SDG Alignment
- SDG 4 (Quality Education): better lab/classroom scheduling insights; reliable equipment availability; data‑driven resource planning
- SDG 12 (Responsible Consumption & Production): extend equipment lifespan; reduce waste via proactive maintenance and smarter stock control

12) Demo Plan (Preview)
- Open app → Upload timetable PDF → show extracted usage in semester details/progress
- Run a predictive maintenance action from Equipment → display risk/confidence
- Show low‑stock indicator and alert flow
- Brief view of dashboard charts and notification examples

13) Risks, Limitations & Future Work
- Risks: ML service availability; PDF variability; data quality
- Future: containerized deployment; robust CI/CD; additional sensors/IoT; richer analytics; mobile app

14) Conclusion
- Restate value: predictive, timetable‑aware, and actionable maintenance platform for education
- Call to action: proceed to live demo

## Content Hints Pulled From Codebase & Docs
- Timetable PDF processing uses both iText text extraction and Azure Form Recognizer tables; room pattern e.g., /\b[A-Z]{1,3}\d{3,4}\b/; time ranges e.g., 09:00–11:00
- Mapping from room weekly hours → equipment in those rooms; persists to SemesterEquipmentUsage; statistics computed (total hours, weekly averages)
- ML integration: .NET app (HttpClient) → Python API on http://localhost:5000 (single & batch); health check endpoints; predictions stored
- Services registered in Program.cs: EquipmentPredictionService (HttpClient), PredictionMetricsService, ModelInterpretabilityService, EquipmentAIInsightService, EnhancedEquipmentTrackingService; SignalR configured with JSON settings
- Model interpretability accessible via brain icon in Equipment list; backend endpoint returns explanation (e.g., SHAP‑style values)

## Slide Copy Suggestions (ready-to-use)
- Problem: “Manual timetable processing and reactive maintenance cause downtime and poor lab planning.”
- Objective: “Predict failures early, automate usage analytics, and enable proactive decisions.”
- Solution: “ASP.NET MVC + Python ML + Azure Form Recognizer, unified by clean dashboards and alerts.”
- Feature bullets: short, benefit‑first; add icons: brain (AI), bell (alerts), table/pdf (Form Recognizer), chart (dashboard)
- Outcome bullets: emphasize saved time, increased reliability, actionable insights
- SDG bullets: SDG 4 → improved educational operations; SDG 12 → resource efficiency and waste reduction

## Diagram Directions
- One system diagram: blocks for Web App, DB, Python ML API, Azure Form Recognizer, Email/SignalR
- One workflow diagram: Upload PDF → Extract (tables/text) → Parse (rooms/times) → Map to equipment → Store usage → Predict → Alert → Dashboard

## Visual & Branding Guidelines
- Theme: clean, light background, accent colors (primary blue, secondary teal/green)
- Fonts: large headings, minimal text; 3–5 bullets per slide max
- Charts: simple bar/line; avoid clutter; percentage labels for clarity
- Icons: Bootstrap/Fluent icons for consistency (brain, bell, calendar, chart, database)

## Speaker Plan (3 presenters, 5–10 min)
- Speaker 1 (2–3 min): Problem, Objectives, Solution overview
- Speaker 2 (2–3 min): Architecture, Workflow, Key features
- Speaker 3 (2–3 min): Outcomes, SDG alignment, Demo preview
- Reserve 1–2 min buffer for transitions or brief Q&A pre‑demo

## Assets to Include (if the PPT generator supports images/placeholders)
- Screenshot placeholders: Equipment list with brain icon; Timetable upload page; Dashboard charts; Low‑stock badge
- Code snippet placeholders: small excerpts from controller/service interfaces (avoid dense code)

## Optional Appendix Content
- Glossary: EF Core, SignalR, SHAP, Form Recognizer
- Testing checklist: ML API health, PDF extraction success, prediction storage, alerts
- Deployment note: Azure App Service (Free tier feasible for demos), configure app settings and connection strings

## Final Slide: Demo Next
- Title: “Live Demo”
- Bullets: Upload timetable → View usage and predictions → Show alert → Dashboard metrics

