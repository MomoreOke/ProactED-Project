# 📅 Enhanced Timetable Management System

## Overview

The Enhanced Timetable Management System is a comprehensive module designed to manage academic semesters and track equipment usage patterns for predictive maintenance. This system provides full CRUD operations, automatic semester completion detection, progress tracking, and seamless integration with the predictive maintenance functionality.

## 🚀 Key Features

### 1. **Comprehensive Semester Management**
- **Full CRUD Operations**: Create, Read, Update, and Delete semester records
- **Automatic Completion Detection**: System automatically detects when semesters end and updates their status
- **Progress Tracking**: Real-time progress monitoring through academic periods
- **File Management**: PDF timetable upload with automatic processing and equipment usage extraction

### 2. **Enhanced User Experience**
- **Real-time Validation**: Form validation with immediate feedback
- **Progress Indicators**: Visual progress bars and timeline displays
- **Confirmation Dialogs**: Safe deletion and editing with confirmation prompts
- **Auto-dismissing Alerts**: User-friendly notification system

### 3. **Advanced Analytics**
- **Equipment Usage Tracking**: Automatic extraction of equipment usage from timetable PDFs
- **Progress Dashboards**: Comprehensive semester progress visualization
- **Statistics Overview**: Detailed statistics and metrics
- **Maintenance Recommendations**: AI-powered maintenance suggestions based on usage patterns

### 4. **Seamless Integration**
- **Predictive Maintenance**: Enhanced accuracy through semester-based usage data
- **Equipment Lifecycle**: Integration with equipment management system
- **User Management**: Full integration with user authentication and authorization

## 📁 System Architecture

### Controllers
- **`TimetableController`**: Main controller handling all semester operations
- **`PredictiveMaintenanceController`**: Updated with redirects to new timetable system

### Models
- **`Semester`**: Core semester entity with computed properties
- **`SemesterEquipmentUsage`**: Equipment usage tracking per semester
- **View Models**: Comprehensive view models for different operations

### Views
- **`Index.cshtml`**: Main dashboard with current semester overview
- **`Upload.cshtml`**: Enhanced upload form with validation
- **`Progress.cshtml`**: Comprehensive progress dashboard
- **`List.cshtml`**: Semester listing with filtering and sorting
- **`Details.cshtml`**: Detailed semester information
- **`Edit.cshtml`**: Semester editing with file replacement

## 🔧 Core Functionality

### 1. Semester Upload Process

```csharp
// Enhanced upload with validation
[HttpPost]
public async Task<IActionResult> Upload(SemesterUploadViewModel model)
{
    // Validation checks
    // Overlap detection
    // File processing
    // Background processing
}
```

**Features:**
- **Date Validation**: Prevents past dates and overlapping semesters
- **File Validation**: PDF format and size validation
- **Background Processing**: Non-blocking timetable analysis
- **Progress Tracking**: Real-time processing status updates

### 2. Automatic Semester Completion

```csharp
private async Task CheckAndUpdateSemesterCompletion(Semester semester)
{
    // Check if semester has ended
    // Update status automatically
    // Log completion
}
```

**Features:**
- **Automatic Detection**: System checks semester end dates
- **Status Updates**: Automatic status changes
- **Audit Trail**: Complete history of changes

### 3. Progress Tracking

```csharp
public double ProgressPercentage
{
    get
    {
        // Calculate progress based on current date
        // Return percentage with precision
    }
}
```

**Features:**
- **Real-time Calculation**: Dynamic progress updates
- **Visual Indicators**: Progress bars and timelines
- **Week Tracking**: Current week and remaining weeks

### 4. Equipment Usage Analytics

```csharp
private Dictionary<string, double> ParseTimetableText(string text, int semesterWeeks)
{
    // Extract room schedules
    // Calculate usage hours
    // Map to equipment
}
```

**Features:**
- **PDF Text Extraction**: Automatic timetable parsing
- **Usage Calculation**: Weekly and total usage hours
- **Equipment Mapping**: Automatic equipment identification

## 📊 Dashboard Features

### 1. Main Dashboard (`Index.cshtml`)
- **Current Semester Overview**: Active semester status and progress
- **Quick Actions**: Direct access to common operations
- **Statistics Cards**: Key metrics at a glance
- **Notifications**: System alerts and warnings
- **Recent Semesters**: Quick access to recent records

### 2. Progress Dashboard (`Progress.cshtml`)
- **Progress Visualization**: Timeline and progress bars
- **Weekly Breakdown**: Week-by-week progress tracking
- **Equipment Usage Summary**: Detailed usage statistics
- **Maintenance Recommendations**: AI-powered suggestions
- **Statistics Overview**: Comprehensive metrics

### 3. Semester List (`List.cshtml`)
- **Advanced Filtering**: Status, processing, and date filters
- **Sorting Options**: Multiple sort criteria
- **View Modes**: Card and table views
- **Bulk Operations**: Mass actions for multiple semesters
- **Search Functionality**: Text-based search

## 🔐 Security & Validation

### 1. Input Validation
- **Model Validation**: Server-side validation with error messages
- **Client-side Validation**: Real-time form validation
- **File Validation**: PDF format and size restrictions
- **Date Validation**: Logical date range validation

### 2. Authorization
- **User Authentication**: Login required for all operations
- **Role-based Access**: Different permissions for different user types
- **Audit Trail**: Complete history of changes and operations

### 3. Data Integrity
- **Foreign Key Constraints**: Proper database relationships
- **Cascade Operations**: Automatic cleanup of related data
- **Transaction Management**: Atomic operations for data consistency

## 📈 Analytics & Reporting

### 1. Semester Statistics
- **Total Semesters**: Complete semester count
- **Active Semesters**: Currently active semesters
- **Completion Rates**: Success and completion metrics
- **Processing Status**: File processing statistics

### 2. Equipment Analytics
- **Usage Patterns**: Weekly and total usage analysis
- **High Usage Detection**: Identification of heavily used equipment
- **Maintenance Correlation**: Usage-based maintenance predictions
- **Cost Analysis**: Estimated maintenance costs

### 3. Progress Metrics
- **Completion Percentage**: Real-time progress tracking
- **Time Remaining**: Days and weeks until completion
- **Milestone Tracking**: Key academic period milestones
- **Trend Analysis**: Historical progress patterns

## 🔄 Integration Points

### 1. Predictive Maintenance
- **Enhanced Accuracy**: Semester-based usage data improves predictions
- **Equipment Lifecycle**: Integration with equipment management
- **Maintenance Scheduling**: Semester-aware maintenance planning
- **Risk Assessment**: Usage-based risk calculations

### 2. User Management
- **Authentication**: Full integration with ASP.NET Core Identity
- **User Tracking**: Upload and modification tracking
- **Permission Management**: Role-based access control
- **Audit Logging**: Complete user action history

### 3. File Management
- **PDF Processing**: Automatic timetable text extraction
- **File Storage**: Secure file storage with unique naming
- **File Validation**: Format and size validation
- **Cleanup Operations**: Automatic file cleanup

## 🎯 User Workflows

### 1. Semester Creation Workflow
1. **Navigate** to Timetable Management
2. **Click** "Upload Semester Timetable"
3. **Fill** semester information (name, dates, weeks)
4. **Upload** PDF timetable file
5. **Review** validation and preview
6. **Submit** for processing
7. **Monitor** processing status
8. **View** results and analytics

### 2. Semester Management Workflow
1. **View** current semester status
2. **Monitor** progress and analytics
3. **Edit** semester details if needed
4. **Replace** timetable file if required
5. **Track** equipment usage patterns
6. **Review** maintenance recommendations
7. **Plan** next semester upload

### 3. Progress Monitoring Workflow
1. **Access** progress dashboard
2. **Review** current week and progress
3. **Analyze** equipment usage statistics
4. **Check** maintenance recommendations
5. **Monitor** completion timeline
6. **Prepare** for semester transition

## 🛠️ Technical Implementation

### 1. Database Schema
```sql
-- Semester table with computed properties
CREATE TABLE Semesters (
    SemesterId INT PRIMARY KEY IDENTITY(1,1),
    SemesterName NVARCHAR(100) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    NumberOfWeeks INT NOT NULL,
    IsActive BIT DEFAULT 1,
    ProcessingStatus INT DEFAULT 0,
    -- Additional fields...
);

-- Equipment usage tracking
CREATE TABLE SemesterEquipmentUsages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SemesterId INT FOREIGN KEY REFERENCES Semesters(SemesterId),
    EquipmentId INT FOREIGN KEY REFERENCES Equipment(EquipmentId),
    WeeklyUsageHours DECIMAL(8,2),
    -- Additional fields...
);
```

### 2. Background Processing
```csharp
// Background file processing
_ = Task.Run(async () => await ProcessTimetableFile(semester.SemesterId));
```

### 3. Real-time Updates
```javascript
// Auto-refresh progress data
setInterval(function() {
    location.reload();
}, 300000); // 5 minutes
```

## 📋 Configuration

### 1. File Upload Settings
- **Maximum File Size**: 10MB
- **Allowed Formats**: PDF only
- **Storage Location**: `wwwroot/uploads/timetables/`
- **File Naming**: Unique GUID-based naming

### 2. Processing Settings
- **Background Processing**: Asynchronous file analysis
- **Progress Updates**: Real-time status updates
- **Error Handling**: Comprehensive error logging
- **Retry Logic**: Automatic retry on failures

### 3. UI Settings
- **Auto-dismiss Alerts**: 8-10 second timeout
- **Progress Refresh**: 5-minute intervals
- **Validation Feedback**: Immediate form validation
- **Confirmation Dialogs**: Safe operation confirmations

## 🚀 Future Enhancements

### 1. Planned Features
- **Bulk Upload**: Multiple semester upload capability
- **Advanced Analytics**: Machine learning-based predictions
- **Mobile Support**: Responsive mobile interface
- **API Integration**: RESTful API for external systems

### 2. Performance Optimizations
- **Caching**: Redis-based caching for frequently accessed data
- **Database Optimization**: Indexing and query optimization
- **File Compression**: Automatic PDF compression
- **CDN Integration**: Content delivery network for files

### 3. Advanced Analytics
- **Predictive Modeling**: Advanced ML-based predictions
- **Trend Analysis**: Historical pattern analysis
- **Custom Reports**: User-defined report generation
- **Data Export**: Multiple format export capabilities

## 📞 Support & Maintenance

### 1. Monitoring
- **Error Logging**: Comprehensive error tracking
- **Performance Monitoring**: Response time tracking
- **Usage Analytics**: User behavior analysis
- **System Health**: Overall system status monitoring

### 2. Maintenance
- **Regular Backups**: Automated database backups
- **File Cleanup**: Automatic old file removal
- **Database Maintenance**: Regular optimization
- **Security Updates**: Regular security patches

### 3. Documentation
- **API Documentation**: Complete API reference
- **User Guides**: Step-by-step user instructions
- **Developer Guides**: Technical implementation details
- **Troubleshooting**: Common issues and solutions

---

*This enhanced timetable management system provides a robust, user-friendly, and feature-rich solution for academic semester management and equipment usage tracking, seamlessly integrated with the predictive maintenance system.* 