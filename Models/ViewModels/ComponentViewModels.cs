namespace FEENALOoFINALE.Models.ViewModels
{
    /// <summary>
    /// Configuration for data table columns
    /// </summary>
    public class DataTableColumn
    {
        public string PropertyName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool Sortable { get; set; } = true;
        public bool Visible { get; set; } = true;
        public int? Width { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public DataTableColumnType Type { get; set; } = DataTableColumnType.Text;
        public string Format { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = false;
        public string? LinkController { get; set; }
        public string? LinkAction { get; set; }
        public string? LinkRouteProperty { get; set; }
    }

    /// <summary>
    /// Configuration for data table actions
    /// </summary>
    public class DataTableAction
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string CssClass { get; set; } = "btn-sm";
        public string Color { get; set; } = "primary";
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? RouteProperty { get; set; }
        public bool RequiresConfirmation { get; set; } = false;
        public string ConfirmationMessage { get; set; } = "Are you sure?";
        public string[] RequiredRoles { get; set; } = Array.Empty<string>();
        public string? OnClick { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public enum DataTableColumnType
    {
        Text,
        Number,
        Date,
        DateTime,
        Boolean,
        Currency,
        Percentage,
        Badge,
        Link,
        Html,
        Image
    }

    /// <summary>
    /// Enhanced notification system
    /// </summary>
    public class NotificationViewModel
    {
        public List<NotificationMessage> Messages { get; set; } = new();
        public NotificationSettings Settings { get; set; } = new();
    }

    public class NotificationSettings
    {
        public bool EnableAutoClose { get; set; } = true;
        public int AutoCloseDelay { get; set; } = 5000; // milliseconds
        public bool EnableSounds { get; set; } = false;
        public string Position { get; set; } = "top-right";
        public int MaxVisible { get; set; } = 5;
        public bool EnableStacking { get; set; } = true;
    }

    /// <summary>
    /// Form validation helper
    /// </summary>
    public class FormValidationHelper
    {
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public Dictionary<string, string> SuccessMessages { get; set; } = new();
        public Dictionary<string, string> WarningMessages { get; set; } = new();
        public bool IsValid => !Errors.Any();
        
        public void AddError(string field, string message)
        {
            if (!Errors.ContainsKey(field))
                Errors[field] = new List<string>();
            
            Errors[field].Add(message);
        }
        
        public void AddSuccess(string field, string message)
        {
            SuccessMessages[field] = message;
        }
        
        public void AddWarning(string field, string message)
        {
            WarningMessages[field] = message;
        }
        
        public List<string> GetErrors(string field)
        {
            return Errors.ContainsKey(field) ? Errors[field] : new List<string>();
        }
        
        public string? GetSuccess(string field)
        {
            return SuccessMessages.ContainsKey(field) ? SuccessMessages[field] : null;
        }
        
        public string? GetWarning(string field)
        {
            return WarningMessages.ContainsKey(field) ? WarningMessages[field] : null;
        }
    }

    /// <summary>
    /// Modal dialog configuration
    /// </summary>
    public class ModalDialogViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Size { get; set; } = "modal-lg"; // modal-sm, modal-lg, modal-xl
        public bool Centered { get; set; } = true;
        public bool Scrollable { get; set; } = false;
        public bool CloseOnBackdrop { get; set; } = true;
        public bool CloseOnEscape { get; set; } = true;
        public List<ModalButton> Buttons { get; set; } = new();
        public string? OnShow { get; set; }
        public string? OnHide { get; set; }
    }

    public class ModalButton
    {
        public string Text { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public string? OnClick { get; set; }
        public bool DismissModal { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public string? Icon { get; set; }
    }

    /// <summary>
    /// Chart configuration
    /// </summary>
    public class ChartViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ChartType Type { get; set; } = ChartType.Line;
        public int Height { get; set; } = 300;
        public bool Responsive { get; set; } = true;
        public bool ShowLegend { get; set; } = true;
        public bool ShowTooltip { get; set; } = true;
        public List<ChartDataset> Datasets { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public ChartOptions Options { get; set; } = new();
    }

    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
        public int BorderWidth { get; set; } = 2;
        public bool Fill { get; set; } = false;
    }

    public class ChartOptions
    {
        public bool Responsive { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = false;
        public ChartLegend Legend { get; set; } = new();
        public ChartTooltip Tooltip { get; set; } = new();
        public Dictionary<string, object> Scales { get; set; } = new();
    }

    public class ChartLegend
    {
        public bool Display { get; set; } = true;
        public string Position { get; set; } = "top";
    }

    public class ChartTooltip
    {
        public bool Enabled { get; set; } = true;
        public string Mode { get; set; } = "index";
        public bool Intersect { get; set; } = false;
    }

    public enum ChartType
    {
        Line,
        Bar,
        Pie,
        Doughnut,
        Radar,
        PolarArea,
        Scatter
    }
}
