using System;

namespace PrismLibrary.Services.Metadata
{
    public sealed class PackUpdateStatus
    {
        public string InstanceId { get; set; }
        public string InstanceName { get; set; }
        public string PackId { get; set; }
        public string Platform { get; set; }
        public string CurrentVersionId { get; set; }
        public string CurrentVersionName { get; set; }
        public string LatestVersionId { get; set; }
        public string LatestVersionName { get; set; }
        public bool UpdateAvailable { get; set; }
        public string Error { get; set; }
        public DateTime CheckedAtUtc { get; set; }

        public string Summary
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    return Error;
                }

                if (!UpdateAvailable)
                {
                    return "Up to date (" + (CurrentVersionName ?? CurrentVersionId ?? "?") + ")";
                }

                var from = CurrentVersionName ?? CurrentVersionId ?? "?";
                var to = LatestVersionName ?? LatestVersionId ?? "?";
                return from + " → " + to;
            }
        }
    }
}
