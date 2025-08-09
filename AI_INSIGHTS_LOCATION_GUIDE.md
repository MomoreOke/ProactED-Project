# AI Insights Location Guide

## Where to Find AI Insights in Your ML Dashboards

### 1. Original ML Dashboard (`/MLDashboard`)

**Location:** Scroll down to the bottom of the dashboard after the high-risk equipment table.

**AI Insights Section:**
- **Card Title:** "AI Insights & Recommendations" (with lightbulb icon)
- **Left Column:** "Quick Insights" - Shows predictive trends from the AI system
- **Right Column:** "Smart Recommendations" - Shows actionable recommendations from the AI system
- **Enhanced Dashboard Button:** Click "View Enhanced Dashboard" to access the full AI features

### 2. Enhanced ML Dashboard (`/MLDashboard/Enhanced`)

**Multiple AI Insight Locations:**

#### A. Main AI Insights Panel (Bottom of Page)
- **Card Title:** "AI-Generated Insights & Recommendations"
- **Left Column:** "Predictive Trends" - Comprehensive trend analysis
- **Right Column:** "Smart Recommendations" - Detailed recommendations with cost estimates

#### B. High-Risk Equipment Table (Middle of Page)
- **AI Insights Column:** Each equipment row shows AI-generated insights
- **"AI Analysis" Button:** Click to open detailed AI analysis modal for specific equipment

#### C. Equipment Details Modal
- **Trigger:** Click "AI Analysis" button on any equipment
- **Content:** 
  - Equipment information
  - Risk analysis
  - Primary risk factors
  - Recommended actions
  - Cost analysis

### 3. API Endpoints (For Developers)

The AI insights are powered by these API endpoints:

- **Smart Recommendations:** `GET /MLDashboard/GetSmartRecommendations`
- **Predictive Trends:** `GET /MLDashboard/GetPredictiveTrends`  
- **Equipment AI Insight:** `GET /MLDashboard/GetEquipmentAIInsight/{equipmentId}`

### 4. AI Insight Features

#### Smart Recommendations Include:
- Priority level (IMMEDIATE, HIGH, MEDIUM, LOW)
- Time frame for action
- Description of recommended action
- Estimated cost savings

#### Predictive Trends Include:
- Trend type (POSITIVE, NEGATIVE, WARNING, CAUTION)
- Impact level (HIGH, MODERATE, LOW)
- Trend description with percentages
- Relevant icons and color coding

#### Equipment AI Insights Include:
- Risk factors analysis
- Maintenance recommendations
- Cost estimations
- Equipment age and condition analysis
- Performance metrics

### 5. Troubleshooting

If AI insights are not showing:

1. **Check Console:** Open browser developer tools (F12) and check for JavaScript errors
2. **Verify API:** The insights load from API endpoints - ensure they're responding
3. **Refresh Dashboard:** Try refreshing the page to reload insights
4. **Check Network:** Ensure internet connectivity for API calls

### 6. Key Benefits

- **Real-time Analysis:** AI insights update with each dashboard refresh
- **Cost Estimates:** Financial impact analysis for maintenance decisions
- **Priority-based:** Recommendations ranked by urgency and impact
- **Equipment-specific:** Individual AI analysis for each piece of equipment
- **Professional UI:** Clean, modern interface with intuitive icons and colors

---

**Note:** The AI insights system uses a sophisticated service that analyzes equipment data, maintenance history, and predictive models to provide actionable intelligence for proactive maintenance decisions.
