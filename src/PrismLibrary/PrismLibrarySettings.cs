using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Data;
using PrismLibrary.Services;

namespace PrismLibrary
{
    public class PrismLibrarySettings : ObservableObject
    {
        private string executablePath = string.Empty;
        private string dataRoot = string.Empty;
        private string defaultProfile = string.Empty;
        private List<string> enabledInstanceIds = new List<string>();

        public string ExecutablePath { get => executablePath; set => SetValue(ref executablePath, value); }
        public string DataRoot { get => dataRoot; set => SetValue(ref dataRoot, value); }
        public string DefaultProfile { get => defaultProfile; set => SetValue(ref defaultProfile, value); }

        public List<string> EnabledInstanceIds
        {
            get => enabledInstanceIds;
            set => SetValue(ref enabledInstanceIds, value ?? new List<string>());
        }

        public bool IsInstanceEnabled(string instanceId)
        {
            return !string.IsNullOrWhiteSpace(instanceId) &&
                   EnabledInstanceIds != null &&
                   EnabledInstanceIds.Any(id => string.Equals(id, instanceId, StringComparison.OrdinalIgnoreCase));
        }

        public void SetInstanceEnabled(string instanceId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return;
            }

            if (EnabledInstanceIds == null)
            {
                EnabledInstanceIds = new List<string>();
            }

            var existing = EnabledInstanceIds.FirstOrDefault(id => string.Equals(id, instanceId, StringComparison.OrdinalIgnoreCase));
            if (enabled)
            {
                if (existing == null)
                {
                    EnabledInstanceIds.Add(instanceId);
                    OnPropertyChanged(nameof(EnabledInstanceIds));
                }
            }
            else if (existing != null)
            {
                EnabledInstanceIds.Remove(existing);
                OnPropertyChanged(nameof(EnabledInstanceIds));
            }
        }
    }

    public class InstanceEnableItem : ObservableObject
    {
        private bool isEnabled;

        public string Id { get; set; }
        public string Name { get; set; }

        public bool IsEnabled
        {
            get => isEnabled;
            set => SetValue(ref isEnabled, value);
        }
    }

    public class PrismLibrarySettingsViewModel : ObservableObject, ISettings
    {
        private readonly PrismLibraryPlugin plugin;
        private PrismLibrarySettings editingClone;

        private PrismLibrarySettings settings;
        public PrismLibrarySettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InstanceEnableItem> InstanceItems { get; } = new ObservableCollection<InstanceEnableItem>();

        public ICommand BrowseExecutableCommand { get; }
        public ICommand BrowseDataRootCommand { get; }
        public ICommand DetectDefaultsCommand { get; }
        public ICommand RefreshInstancesCommand { get; }

        public PrismLibrarySettingsViewModel(PrismLibraryPlugin plugin)
        {
            this.plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<PrismLibrarySettings>();
            Settings = savedSettings ?? new PrismLibrarySettings();

            BrowseExecutableCommand = new RelayCommand(() =>
            {
                var file = plugin.PlayniteApi.Dialogs.SelectFile("Prism Launcher|prismlauncher.exe");
                if (!string.IsNullOrEmpty(file))
                {
                    Settings.ExecutablePath = file;
                }
            });

            BrowseDataRootCommand = new RelayCommand(() =>
            {
                var folder = plugin.PlayniteApi.Dialogs.SelectFolder();
                if (!string.IsNullOrEmpty(folder))
                {
                    Settings.DataRoot = folder;
                    RefreshInstanceItems();
                }
            });

            DetectDefaultsCommand = new RelayCommand(() =>
            {
                var detected = PrismPaths.DetectDefaults();
                if (!string.IsNullOrWhiteSpace(detected.ExecutablePath))
                {
                    Settings.ExecutablePath = detected.ExecutablePath;
                }

                if (!string.IsNullOrWhiteSpace(detected.DataRoot))
                {
                    Settings.DataRoot = detected.DataRoot;
                }

                RefreshInstanceItems();
            });

            RefreshInstancesCommand = new RelayCommand(RefreshInstanceItems);
        }

        public void BeginEdit()
        {
            editingClone = Serialization.GetClone(Settings);
            RefreshInstanceItems();
        }

        public void CancelEdit()
        {
            Settings = editingClone;
            RefreshInstanceItems();
        }

        public void EndEdit()
        {
            ApplyInstanceItemsToSettings();
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            var paths = PrismPaths.Resolve(Settings.ExecutablePath, Settings.DataRoot);

            if (string.IsNullOrWhiteSpace(paths.ExecutablePath) || !File.Exists(paths.ExecutablePath))
            {
                errors.Add("Prism Launcher executable was not found. Browse to prismlauncher.exe.");
            }

            if (string.IsNullOrWhiteSpace(paths.DataRoot) || !Directory.Exists(paths.DataRoot))
            {
                errors.Add("Prism data root was not found. This is usually %AppData%\\PrismLauncher or your portable folder.");
            }
            else if (string.IsNullOrWhiteSpace(paths.InstancesDirectory) || !Directory.Exists(paths.InstancesDirectory))
            {
                errors.Add("Could not find an instances folder under the Prism data root.");
            }

            return errors.Count == 0;
        }

        public void RefreshInstanceItems()
        {
            ApplyInstanceItemsToSettings();
            InstanceItems.Clear();

            var paths = PrismPaths.Resolve(Settings.ExecutablePath, Settings.DataRoot);
            if (!paths.IsValid)
            {
                return;
            }

            foreach (var instance in InstanceScanner.Scan(paths))
            {
                InstanceItems.Add(new InstanceEnableItem
                {
                    Id = instance.Id,
                    Name = instance.Name,
                    IsEnabled = Settings.IsInstanceEnabled(instance.Id)
                });
            }
        }

        private void ApplyInstanceItemsToSettings()
        {
            if (InstanceItems.Count == 0)
            {
                return;
            }

            Settings.EnabledInstanceIds = InstanceItems
                .Where(item => item.IsEnabled)
                .Select(item => item.Id)
                .ToList();
        }
    }
}
