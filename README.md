# Predictive Maintenance Management System

A comprehensive, enterprise-grade predictive maintenance management system built with ASP.NET Core, Entity Framework, and modern web technologies. This system provides advanced analytics, AI-powered failure prediction, and real-time monitoring capabilities for industrial equipment maintenance.

## 🚀 Features

### Advanced Analytics & Dashboard
- **Real-time KPI monitoring** with dynamic charts and metrics
- **Predictive insights** using machine learning algorithms
- **Interactive dashboards** with customizable views
- **Trend analysis** and historical data visualization
- **Performance metrics** tracking and reporting

### AI-Powered Predictions
- **Failure prediction engine** with confidence scoring
- **Risk assessment** algorithms for equipment health
- **Anomaly detection** for proactive maintenance
- **Predictive maintenance scheduling** optimization
- **Real-time monitoring** with alert systems

### Equipment Management
- **Comprehensive equipment tracking** with detailed specifications
- **Maintenance history** and lifecycle management
- **Inventory integration** with automatic reordering
- **Bulk operations** for efficient data management
- **Advanced filtering** and search capabilities

### User Management & Security
- **Role-based access control** with multiple user types
- **Email verification** system for secure registration
- **Password strength validation** and security policies
- **User activity tracking** and audit trails
- **Admin dashboard** for user management

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0, Entity Framework Core
- **Frontend**: Razor Pages, Bootstrap 5, Chart.js, jQuery
- **Database**: SQL Server with Entity Framework migrations
- **Authentication**: ASP.NET Core Identity
- **Real-time**: SignalR for live updates
- **API**: RESTful APIs with comprehensive endpoints
- **Analytics**: Custom ML algorithms for predictive analytics

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Git

## 🔧 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/predictive-maintenance-system.git
   cd predictive-maintenance-system
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   - Open `appsettings.json`
   - Update the `DefaultConnection` string to match your SQL Server instance

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

The application will be available at `https://localhost:5001`

## 📊 System Architecture

### Models
- **Equipment**: Core equipment entities with specifications and status
- **FailurePrediction**: AI-powered prediction models with confidence metrics
- **MaintenanceLog**: Historical maintenance records and scheduling
- **User**: Role-based user management with authentication
- **Alert**: Real-time notification system
- **Inventory**: Parts and supplies management

### Controllers
- **DashboardController**: Advanced analytics and KPI management
- **EquipmentController**: Equipment CRUD operations with bulk actions
- **FailurePredictionController**: AI prediction endpoints and monitoring
- **UserController**: User management and authentication
- **API Controllers**: RESTful endpoints for external integrations

### Services
- **AdvancedAnalyticsService**: Machine learning and statistical analysis
- **EmailService**: Notification and verification system
- **ReportService**: Data export and reporting capabilities

## 🔍 Key Features in Detail

### Predictive Analytics
- **Health Score Calculation**: Advanced algorithms assess equipment condition
- **Failure Risk Assessment**: ML-powered risk analysis with confidence intervals
- **Maintenance Optimization**: Intelligent scheduling based on usage patterns
- **Cost Analysis**: ROI tracking for maintenance activities

### Real-time Monitoring
- **Live Dashboard Updates**: Real-time KPI refreshing
- **Alert Systems**: Instant notifications for critical events
- **Performance Tracking**: Continuous monitoring of equipment metrics
- **Anomaly Detection**: Automated identification of unusual patterns

### Advanced UI/UX
- **Modern, Responsive Design**: Bootstrap 5 with custom styling
- **Interactive Charts**: Dynamic data visualization with Chart.js
- **Dual View Modes**: Table and card layouts for different use cases
- **Quick Actions**: Streamlined workflows for common tasks
- **Progressive Enhancement**: Graceful degradation for all browsers

## 🚦 API Endpoints

### Equipment Management
- `GET /api/equipment` - List all equipment with filtering
- `POST /api/equipment` - Create new equipment
- `PUT /api/equipment/{id}` - Update equipment details
- `DELETE /api/equipment/{id}` - Remove equipment

### Predictions
- `GET /api/predictions` - Get failure predictions
- `POST /api/predictions/generate` - Generate new predictions
- `GET /api/predictions/{id}/details` - Get prediction details

### Analytics
- `GET /api/analytics/kpis` - Get current KPIs
- `GET /api/analytics/trends` - Get trend data
- `GET /api/analytics/health-scores` - Get equipment health scores

## 🔐 Security Features

- **Input Validation**: Comprehensive server-side validation
- **XSS Protection**: Built-in cross-site scripting prevention
- **CSRF Protection**: Anti-forgery token implementation
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **Password Security**: Bcrypt hashing with salt
- **Role-based Authorization**: Granular permission system

## 📈 Performance Optimization

- **Lazy Loading**: Efficient data loading strategies
- **Caching**: Redis-ready caching implementation
- **Database Optimization**: Indexed queries and efficient joins
- **Async Operations**: Non-blocking I/O throughout the application
- **Compression**: Response compression for faster loading

## 🧪 Testing

Run the test suite:
```bash
dotnet test
```

## 📦 Deployment

### Production Setup
1. **Environment Configuration**
   - Set `ASPNETCORE_ENVIRONMENT=Production`
   - Configure production database connection
   - Set up SSL certificates

2. **Database Migration**
   ```bash
   dotnet ef database update --environment Production
   ```

3. **Publish Application**
   ```bash
   dotnet publish -c Release
   ```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Chart.js for beautiful data visualization
- Bootstrap for responsive design framework
- Entity Framework for robust ORM capabilities
- ASP.NET Core team for the excellent framework

## 📧 Contact

For questions or support, please contact the development team or create an issue in the repository.

---

**Built with ❤️ for modern industrial maintenance management**
