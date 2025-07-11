using System.ComponentModel.DataAnnotations;
using FEENALOoFINALE.Models.ViewModels;

namespace FEENALOoFINALE.Models.ViewModels
{
    /// <summary>
    /// Enhanced user management view model
    /// </summary>
    public class UserManagementViewModel : PaginatedViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new();
        public UserFilterOptions FilterOptions { get; set; } = new();
        public UserStatistics Statistics { get; set; } = new();
        public bool CanCreateUsers { get; set; } = true;
        public bool CanManageRoles { get; set; } = true;
        public bool CanExport { get; set; } = true;
    }

    public class UserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string RolesDisplay => string.Join(", ", Roles);
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public bool CanResetPassword { get; set; } = true;
    }

    public class UserFilterOptions
    {
        public List<RoleOption> Roles { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<UserStatusOption> Statuses { get; set; } = new();
    }

    public class RoleOption
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class DepartmentOption
    {
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class UserStatusOption
    {
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int UnconfirmedUsers { get; set; }
        public int UsersLoggedInToday { get; set; }
        public int NewUsersThisMonth { get; set; }
    }

    /// <summary>
    /// Enhanced user creation/editing view model
    /// </summary>
    public class UserFormViewModel : BaseViewModel
    {
        new public string? UserId { get; set; }
        public bool IsEdit => !string.IsNullOrEmpty(UserId);

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Display(Name = "User Roles")]
        public List<string> SelectedRoles { get; set; } = new();

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Password fields (only for new users or password reset)
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        public bool SendWelcomeEmail { get; set; } = true;
        public bool RequirePasswordChange { get; set; } = true;

        // Form options
        public List<RoleSelectionOption> AvailableRoles { get; set; } = new();
        public List<string> AvailableDepartments { get; set; } = new();

        // Validation
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
        public bool HasValidationErrors => ValidationErrors.Any();
    }

    public class RoleSelectionOption
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
    }

    /// <summary>
    /// Enhanced login view model
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Username or email is required")]
        [Display(Name = "Username or Email")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }

        // Additional features
        public bool ShowCaptcha { get; set; } = false;
        public string? CaptchaResponse { get; set; }
        public bool AllowPasswordReset { get; set; } = true;
        public bool AllowRegistration { get; set; } = true;
        public bool ShowSocialLogins { get; set; } = false;
        public int FailedAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockoutEnd { get; set; }

        // Validation
        public string? ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }

    /// <summary>
    /// User profile view model
    /// </summary>
    public class UserProfileViewModel : BaseViewModel
    {
        new public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool EmailConfirmed { get; set; }

        // User preferences
        public UserPreferences Preferences { get; set; } = new();
        
        // Activity summary
        public UserActivitySummary ActivitySummary { get; set; } = new();
        
        // Permissions
        public bool CanEditProfile { get; set; } = true;
        public bool CanChangePassword { get; set; } = true;
        public bool CanManagePreferences { get; set; } = true;
    }

    public class UserPreferences
    {
        public string Theme { get; set; } = "light";
        public string TimeZone { get; set; } = "UTC";
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        public string TimeFormat { get; set; } = "12h";
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = false;
        public bool SmsNotifications { get; set; } = false;
        public int SessionTimeout { get; set; } = 30; // minutes
        public string DefaultDashboard { get; set; } = "overview";
        public int PageSize { get; set; } = 20;
    }

    public class UserActivitySummary
    {
        public int TotalLogins { get; set; }
        public int LoginsThisMonth { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksCompletedThisMonth { get; set; }
        public int AlertsCreated { get; set; }
        public int ReportsGenerated { get; set; }
        public DateTime? LastActivity { get; set; }
        public List<string> RecentActivities { get; set; } = new();
    }

    /// <summary>
    /// Password change view model
    /// </summary>
    public class ChangePasswordViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Password strength indicators
        public bool ShowPasswordStrength { get; set; } = true;
        public List<PasswordRequirement> PasswordRequirements { get; set; } = new();
    }

    public class PasswordRequirement
    {
        public string Text { get; set; } = string.Empty;
        public bool IsMet { get; set; } = false;
        public bool IsRequired { get; set; } = true;
    }
}
