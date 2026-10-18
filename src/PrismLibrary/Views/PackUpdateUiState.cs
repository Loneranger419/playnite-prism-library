namespace PrismLibrary.Views
{
    public sealed class PackUpdateUiState
    {
        public bool IsVisible { get; set; }
        public bool CanCheck { get; set; }
        public bool CanUpdate { get; set; }
        public string UpdateButtonLabel { get; set; } = "Update in Prism";
        public string StatusText { get; set; }
    }
}
