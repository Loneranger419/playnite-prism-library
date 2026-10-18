using System;
using System.Collections.Generic;

namespace PrismLibrary.Models
{
    public sealed class PrismInstance
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string InstanceDirectory { get; set; }
        public string MinecraftDirectory { get; set; }
        public string IconKey { get; set; }
        public string IconPath { get; set; }
        public string MinecraftVersion { get; set; }
        public List<string> Loaders { get; set; } = new List<string>();
        public string Group { get; set; }
        public ulong TotalTimePlayedSeconds { get; set; }
        public DateTime? LastLaunch { get; set; }
    }
}
