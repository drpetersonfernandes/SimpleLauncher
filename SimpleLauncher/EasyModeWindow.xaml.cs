using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SimpleLauncher.Services.CreateFolders;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.DownloadService;
using SimpleLauncher.Services.EasyMode;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.SharedModels;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher;

internal partial class EasyModeWindow : IDisposable, INotifyPropertyChanged, ILoadingState
{
    private readonly PlaySoundEffects _playSoundEffects;
    private EasyModeManager _manager;
    private readonly IConfiguration _configuration;

    // Track download states for all components
    private readonly Dictionary<string, DownloadButtonState> _downloadStates = new();

    public bool IsEmulatorDownloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            UpdateAddSystemButtonState();
        }
    } = true;

    public bool IsCoreDownloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            UpdateAddSystemButtonState();
        }
    } = true;

    public bool IsImagePack1Downloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack2Downloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack3Downloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack4Downloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack5Downloaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack1Available
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack2Available
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack3Available
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack4Available
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack5Available
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsOperationInProgress
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            UpdateAddSystemButtonState();
        }
    }

    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    // Helper to get/set download state with notification
    private DownloadButtonState GetDownloadState(string type)
    {
        return _downloadStates.GetValueOrDefault(type, DownloadButtonState.Idle);
    }

    private void SetDownloadState(string type, DownloadButtonState state)
    {
        _downloadStates[type] = state;
        // Update legacy boolean properties for XAML binding
        switch (type)
        {
            case EasyModeManager.DownloadType.Emulator:
                IsEmulatorDownloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.Core:
                IsCoreDownloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack1:
                IsImagePack1Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack2:
                IsImagePack2Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack3:
                IsImagePack3Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack4:
                IsImagePack4Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack5:
                IsImagePack5Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
        }
    }

    private string _currentDownloadType;

    // Backing field for DownloadStatus property
    private string _downloadStatus = string.Empty;

    private string DownloadStatus
    {
        // ReSharper disable once UnusedMember.Local
        get => _downloadStatus;
        set
        {
            _downloadStatus = value;
            DownloadStatusTextBlock.Text = value;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged; // INotifyPropertyChanged implementation

    // Thread-safe operation tracking
    private int _operationInProgressFlag;

    private bool TryStartOperation()
    {
        // Returns true if we successfully started (was 0, now 1)
        if (Interlocked.CompareExchange(ref _operationInProgressFlag, 1, 0) != 0)
            return false;

        IsOperationInProgress = true;
        return true;
    }

    private void EndOperation()
    {
        IsOperationInProgress = false;
        Interlocked.Exchange(ref _operationInProgressFlag, 0);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            // Ensure the main content area is disabled to prevent Tab-key navigation
            MainContentGrid?.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    public EasyModeWindow(PlaySoundEffects playSoundEffects, IConfiguration configuration)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _configuration = configuration;
        _playSoundEffects = playSoundEffects;

        // Set DataContext for XAML bindings to work
        DataContext = this;

        // Get the DownloadManager from the service provider
        _downloadManager = App.ServiceProvider.GetRequiredService<DownloadManager>();

        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        Closed += CloseWindowRoutineAsync;
        Loaded += EasyModeWindowLoadedAsync;
    }

    private async void EasyModeWindowLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await InitializeManagerAsync();
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[EasyModeWindowLoadedAsync] Error initializing EasyModeManager.");
        }
    }

    private async Task InitializeManagerAsync()
    {
        SetLoadingState(true, (string)Application.Current.TryFindResource("Loadingconfiguration") ?? "Loading configuration...");
        await Task.Yield(); // Allow UI to render the loading overlay

        _manager = await EasyModeManager.LoadAsync();

        SetLoadingState(false);

        if (_manager is not { Systems.Count: > 0 })
        {
            MessageBoxLibrary.EasyModeUnavailableMessageBox();
            SystemNameDropdown.IsEnabled = false;
            SystemFolderTextBox.IsEnabled = false;
            DownloadEmulatorButton.IsEnabled = false;
            DownloadCoreButton.IsEnabled = false;
            DownloadImagePackButton1.IsEnabled = false;
            DownloadImagePackButton2.IsEnabled = false;
            DownloadImagePackButton3.IsEnabled = false;
            DownloadImagePackButton4.IsEnabled = false;
            DownloadImagePackButton5.IsEnabled = false;
            AddSystemButton.IsEnabled = false;
            return;
        }

        PopulateSystemDropdown();
    }

    /// Populates the system dropdown with a sorted list of system names based on the configuration data.
    /// The method retrieves the list of system configurations from the EasyModeManager. It filters systems
    /// that have a non-empty and valid `EmulatorDownloadLink` in their corresponding emulator configuration.
    /// System names are then sorted alphabetically before being assigned to the `ItemsSource` property of the
    /// dropdown UI element.
    /// Preconditions:
    /// - `_manager` should be initialized and its `Systems` property should not be null.
    /// - `SystemNameDropdown` should refer to a valid ComboBox.
    /// Postconditions:
    /// - The dropdown is populated with a sorted list of valid system names. If no valid systems are found,
    /// the dropdown is left empty.
    /// Applies to:
    /// - The method is specifically designed for use in the `EasyModeWindow` class and interacts
    /// with its UI components
    private void PopulateSystemDropdown()
    {
        try
        {
            if (_manager?.Systems == null)
            {
                SystemNameDropdown.ItemsSource = new List<string>(); // return an empty list
                return;
            }

            var sortedSystemNames = _manager.Systems
                .Where(static system => !string.IsNullOrEmpty(system.Emulators?.Emulator?.EmulatorDownloadLink))
                .Select(static system => system.SystemName)
                .OrderBy(static name => name)
                .ToList();

            SystemNameDropdown.ItemsSource = sortedSystemNames;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error populating system dropdown.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            SystemNameDropdown.ItemsSource = new List<string>(); // Assign an empty list if there's any error
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null)
        {
            // Reset all states if no system is selected
            DownloadEmulatorButton.IsEnabled = false;
            DownloadCoreButton.IsEnabled = false;

            IsImagePack1Available = false;
            IsImagePack2Available = false;
            IsImagePack3Available = false;
            IsImagePack4Available = false;
            IsImagePack5Available = false;

            SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Downloaded);
            SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Downloaded);
            SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Downloaded);
            SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Downloaded);
            SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Downloaded);
            SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Downloaded);
            SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Downloaded);

            UpdateAddSystemButtonState();
            SystemFolderTextBox.Text = string.Empty;
            return;
        }

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem == null)
        {
            // This should ideally not happen if PopulateSystemDropdown is correct, but handle defensively
            return;
        }

        var emulator = selectedSystem.Emulators?.Emulator;
        // Determine if download links exist for image packs (for visibility)
        IsImagePack1Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack2Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink2) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack3Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink3) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack4Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink4) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack5Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink5) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);

        // Check if Emulator file already exists on disk. If so, mark it as "downloaded".
        var emulatorLocation = selectedSystem.Emulators?.Emulator?.EmulatorLocation;
        if (!string.IsNullOrEmpty(emulatorLocation))
        {
            var resolvedEmulatorPath = PathHelper.ResolveRelativeToAppDirectory(emulatorLocation);
            SetDownloadState(EasyModeManager.DownloadType.Emulator, File.Exists(resolvedEmulatorPath) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        }
        else
        {
            // If no location is defined, it can't exist, so it needs to be downloaded.
            SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Idle);
        }

        // Check if Core file already exists or is not needed.
        var coreLocation = selectedSystem.Emulators?.Emulator?.CoreLocation;
        var coreDownloadLink = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
        if (!string.IsNullOrEmpty(coreLocation))
        {
            var resolvedCorePath = PathHelper.ResolveRelativeToAppDirectory(coreLocation);
            SetDownloadState(EasyModeManager.DownloadType.Core, File.Exists(resolvedCorePath) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        }
        else
        {
            // If no location is defined, it's considered "ready" only if no download is offered.
            SetDownloadState(EasyModeManager.DownloadType.Core, string.IsNullOrEmpty(coreDownloadLink) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        }

        // Reset download status for image packs.
        SetDownloadState(EasyModeManager.DownloadType.ImagePack1, string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack2, string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack3, string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack4, string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack5, string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);

        // Resolve path for display in the textbox
        SystemFolderTextBox.Text = PathHelper.ResolveRelativeToAppDirectory(selectedSystem.SystemFolder);

        UpdateAddSystemButtonState();
    }

    private async void DownloadEmulatorButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsEmulatorDownloaded)); // Force UI update for IsEnabled binding
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.Emulator);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsEmulatorDownloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadEmulatorButtonClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    // Only reset to Failed if still in Downloading state (not if successfully Downloaded)
                    if (GetDownloadState(EasyModeManager.DownloadType.Emulator) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsEmulatorDownloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadEmulatorButtonClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadEmulatorButtonClickAsync.");
        }
    }

    private async void DownloadCoreButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsCoreDownloaded));
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.Core);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsCoreDownloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadCoreButtonClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    if (GetDownloadState(EasyModeManager.DownloadType.Core) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsCoreDownloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadCoreButtonClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadCoreButtonClickAsync.");
        }
    }

    private async void DownloadImagePackButton1ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsImagePack1Downloaded));
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack1);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsImagePack1Downloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton1ClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    if (GetDownloadState(EasyModeManager.DownloadType.ImagePack1) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsImagePack1Downloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadImagePackButton1ClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton1ClickAsync.");
        }
    }

    private async void DownloadImagePackButton2ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsImagePack2Downloaded));
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack2);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsImagePack2Downloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton2ClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    if (GetDownloadState(EasyModeManager.DownloadType.ImagePack2) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsImagePack2Downloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadImagePackButton2ClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton2ClickAsync.");
        }
    }

    private async void DownloadImagePackButton3ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsImagePack3Downloaded));
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack3);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsImagePack3Downloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton3ClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    if (GetDownloadState(EasyModeManager.DownloadType.ImagePack3) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsImagePack3Downloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadImagePackButton3ClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton3ClickAsync.");
        }
    }

    private async void DownloadImagePackButton4ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsImagePack4Downloaded));
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack4);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsImagePack4Downloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton4ClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    if (GetDownloadState(EasyModeManager.DownloadType.ImagePack4) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsImagePack4Downloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadImagePackButton4ClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton4ClickAsync.");
        }
    }

    private async void DownloadImagePackButton5ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Downloading);
                OnPropertyChanged(nameof(IsImagePack5Downloaded));
                await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack5);
            }
            catch (Exception ex)
            {
                SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Failed);
                OnPropertyChanged(nameof(IsImagePack5Downloaded));
                if (!_disposed)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton5ClickAsync.");
                }
            }
            finally
            {
                if (!_disposed)
                {
                    // EndOperation is called by HandleDownloadAndExtractComponentAsync
                    if (GetDownloadState(EasyModeManager.DownloadType.ImagePack5) == DownloadButtonState.Downloading)
                    {
                        SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Failed);
                        OnPropertyChanged(nameof(IsImagePack5Downloaded));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DownloadImagePackButton5ClickAsync: {ex}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton5ClickAsync.");
        }
    }

    // Helper method to reduce code duplication for downloads and extractions
    private async Task HandleDownloadAndExtractComponentAsync(string type)
    {
        if (_disposed) return;

        _currentDownloadType = type;

        // State already set by caller to Downloading

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return;

        string downloadUrl;
        string componentName;
        string easyModeExtractPath;

        switch (type)
        {
            case EasyModeManager.DownloadType.Emulator:
                downloadUrl = selectedSystem.Emulators?.Emulator?.EmulatorDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.EmulatorDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
                break;
            case EasyModeManager.DownloadType.Core:
                downloadUrl = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.CoreDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("Core") ?? "Core";
                break;
            case EasyModeManager.DownloadType.ImagePack1:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack1") ?? "Image Pack 1";
                break;
            case EasyModeManager.DownloadType.ImagePack2:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack2") ?? "Image Pack 2";
                break;
            case EasyModeManager.DownloadType.ImagePack3:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack3") ?? "Image Pack 3";
                break;
            case EasyModeManager.DownloadType.ImagePack4:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack4") ?? "Image Pack 4";
                break;
            case EasyModeManager.DownloadType.ImagePack5:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack5") ?? "Image Pack 5";
                break;
            default:
                return;
        }

        var destinationPath = PathHelper.ResolveRelativeToAppDirectory(easyModeExtractPath);

        // Ensure valid URL and destination path
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            EndOperation();
            DownloadStatus = $"{errorNodownloadUrLfor} {componentName}";
            SetDownloadState(type, DownloadButtonState.Idle); // Reset state on error
            return;
        }

        if (string.IsNullOrEmpty(destinationPath))
        {
            var errorInvalidDestinationPath = (string)Application.Current.TryFindResource("ErrorInvalidDestinationPath") ?? "Error: Invalid destination path for";
            DownloadStatus = $"{errorInvalidDestinationPath} {componentName}";

            EndOperation();
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[HandleDownloadAndExtractComponentAsync] Invalid destination path for {componentName}: {easyModeExtractPath}");
            SetDownloadState(type, DownloadButtonState.Idle); // Reset state on error
            return;
        }

        try
        {
            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            DownloadStatus = $"{preparingtodownload} {componentName}...";

            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            var success = false;

            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading} {componentName}...";

            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (_disposed) return;

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                DownloadStatus = $"{extracting} {componentName}...";
                LoadingOverlay.Content = $"{extracting} {componentName}...";
                LoadingOverlay.Visibility = Visibility.Visible;
                await Task.Yield(); // Allow UI to render the loading overlay
                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

            if (success)
            {
                EndOperation();
                var hasbeensuccessfullydownloadedandinstalled = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                DownloadStatus = $"{componentName} {hasbeensuccessfullydownloadedandinstalled}";

                // Notify user
                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                StopDownloadButton.IsEnabled = false;
                // Mark as successfully downloaded
                SetDownloadState(type, DownloadButtonState.Downloaded);
            }
            else // Download was not completed successfully (either cancelled, locked, or other failure)
            {
                if (_disposed) return;

                if (_downloadManager.IsUserCancellation) // User cancelled the download
                {
                    var downloadof = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    DownloadStatus = $"{downloadof} {componentName} {wascanceled}";
                    StopDownloadButton.IsEnabled = false;
                    EndOperation();
                    SetDownloadState(type, DownloadButtonState.Failed); // Re-enable on cancel
                }
                else if (_downloadManager.IsFileLockedDuringDownload) // Specific check for file lock during download
                {
                    await MessageBoxLibrary.ShowDownloadFileLockedMessageBoxAsync(_downloadManager.TempFolder);
                    EndOperation();
                }
                else if (_downloadManager.IsDownloadCompleted) // This means download was completed, but something went wrong *after* (e.g., during cleanup or a very late error)
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    DownloadStatus = $"{errorFailedtoextract} {componentName}.";
                    EndOperation();
                    SetDownloadState(type, DownloadButtonState.Failed); // Re-enable on extraction failure
                    await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
                }
                else // Generic download failure (not user cancelled, not file locked, not extraction failure)
                {
                    var errorDuringDownload = (string)Application.Current.TryFindResource("Errorduringdownload") ?? "Error during download";
                    DownloadStatus = $"{errorDuringDownload}: {componentName}.";

                    EndOperation();
                    // Fallback to original behavior for download failures
                    await ShowDownloadErrorDialogAsync(type, selectedSystem);
                    SetDownloadState(type, DownloadButtonState.Failed); // Re-enable on download failure
                }

                StopDownloadButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            if (_disposed) return;

            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            DownloadStatus = $"{errorduring2} {componentName} {downloadprocess2}";

            // Notify developer only if it's not a disk space error
            // Disk space errors are user-environment issues, not code issues
            if (!(ex is IOException ioEx && (ioEx.Message.Contains("Insufficient disk space") || ioEx.Message.Contains("Cannot check disk space"))))
            {
                var contextMessage = $"Error downloading {componentName}.\n" +
                                     $"URL: {downloadUrl}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }

            // Check if the download failed due to a file lock
            if (_downloadManager.IsFileLockedDuringDownload)
            {
                EndOperation();
                await MessageBoxLibrary.ShowDownloadFileLockedMessageBoxAsync(_downloadManager.TempFolder);
            }

            // If download was completed, the exception was likely during extraction.
            else if (_downloadManager.IsDownloadCompleted)
            {
                EndOperation();
                await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
            }
            else // Exception was during download
            {
                EndOperation();
                await ShowDownloadErrorDialogAsync(type, selectedSystem);
            }

            if (_disposed) return;

            StopDownloadButton.IsEnabled = false;
            SetDownloadState(type, DownloadButtonState.Failed); // Re-enable on exception
            EndOperation();
        }
        finally
        {
            _currentDownloadType = null;
        }
    }

    private static Task ShowDownloadErrorDialogAsync(string type, EasyModeSystemConfig selectedSystem)
    {
        switch (type)
        {
            case EasyModeManager.DownloadType.Emulator:
                return MessageBoxLibrary.ShowEmulatorDownloadErrorMessageBoxAsync(selectedSystem);
            case EasyModeManager.DownloadType.Core:
                return MessageBoxLibrary.ShowCoreDownloadErrorMessageBoxAsync(selectedSystem);
            case EasyModeManager.DownloadType.ImagePack1:
            case EasyModeManager.DownloadType.ImagePack2:
            case EasyModeManager.DownloadType.ImagePack3:
            case EasyModeManager.DownloadType.ImagePack4:
            case EasyModeManager.DownloadType.ImagePack5:
                return MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
            default:
                MessageBoxLibrary.DownloadExtractionFailedMessageBox();
                break;
        }

        return Task.CompletedTask;
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadProgressEventArgs e)
    {
        if (_disposed) return;

        Dispatcher.InvokeAsync(() =>
        {
            if (_disposed) return;

            DownloadProgressBar.Value = e.ProgressPercentage;
            DownloadStatus = e.StatusMessage;
        });
    }

    private EasyModeSystemConfig GetSelectedSystem()
    {
        if (_disposed || _manager == null) return null;

        return SystemNameDropdown.SelectedItem != null
            ? _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString())
            : null;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed) return; // Early exit if window is already disposed

        _downloadManager.CancelDownload();
        StopDownloadButton.IsEnabled = false;
        DownloadProgressBar.Value = 0;

        var cancelingdownload2 = (string)Application.Current.TryFindResource("Cancelingdownload") ?? "Canceling download...";
        DownloadStatus = cancelingdownload2;

        if (_currentDownloadType != null)
        {
            // Only re-enable if it was actually downloading
            var currentState = GetDownloadState(_currentDownloadType);
            if (currentState == DownloadButtonState.Downloading)
            {
                SetDownloadState(_currentDownloadType, DownloadButtonState.Failed);
                // Force UI update for the specific button
                var propertyName = GetBooleanPropertyNameForType(_currentDownloadType);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    OnPropertyChanged(propertyName);
                }
            }

            _currentDownloadType = null;
        }

        // Always reset operation flag to re-enable UI
        IsOperationInProgress = false;
    }

    private async void AddSystemButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (IsOperationInProgress) return;

            IsOperationInProgress = true;

            try // Top-level catch for async Task method
            {
                var selectedSystem = GetSelectedSystem();
                if (selectedSystem == null) return;

                string systemFolderRaw;
                if (!string.IsNullOrEmpty(SystemFolderTextBox.Text) && !string.IsNullOrWhiteSpace(SystemFolderTextBox.Text))
                {
                    systemFolderRaw = SystemFolderTextBox.Text;
                }
                else
                {
                    systemFolderRaw = Path.Combine("%BASEFOLDER%", "roms", selectedSystem.SystemName);
                    // No need to update SystemFolderTextBox.Text here, it's already updated in SelectionChanged or will be updated by the user
                }

                var systemImageFolderRaw = selectedSystem.SystemImageFolder;

                var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

                // --- Start Async Operation ---
                try
                {
                    // Disable button during operation
                    AddSystemButton.IsEnabled = false;

                    // Show overlay
                    LoadingOverlay.Content = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
                    LoadingOverlay.Visibility = Visibility.Visible;
                    await Task.Yield(); // Allow UI to render the loading overlay

                    // Update System.xml with the *unresolved* paths, as system.xml expects them.
                    await UpdateSystemXmlAsync(systemXmlPath, selectedSystem, systemFolderRaw, systemImageFolderRaw);

                    // --- If XML update succeeds, continue with folder creation and UI updates ---
                    LoadingOverlay.Content = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";

                    // Resolve paths before passing to folder creation
                    var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(systemFolderRaw);
                    var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolderRaw);

                    // Create System Folders using *resolved* paths
                    CreateDefaultSystemFolders.CreateFolders(selectedSystem.SystemName, resolvedSystemFolder, resolvedSystemImageFolder, _configuration);

                    var systemhasbeensuccessfullyadded = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
                    DownloadStatus = systemhasbeensuccessfullyadded;

                    // Notify user
                    MessageBoxLibrary.SystemAddedMessageBox(selectedSystem.SystemName, resolvedSystemFolder, resolvedSystemImageFolder);

                    // Close the window after successful addition
                    Close();
                }
                catch (InvalidOperationException ex) // Catch specific exceptions from the helper
                {
                    var errorFailedtoaddsystem = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
                    DownloadStatus = $"{errorFailedtoaddsystem} {ex.Message}";

                    // Error is already logged by the helper method.
                    // Notify user
                    MessageBoxLibrary.AddSystemFailedMessageBox(ex.Message);
                }
                catch (Exception ex) // Catch any other unexpected errors
                {
                    var errorFailedtoaddsystem = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
                    DownloadStatus = errorFailedtoaddsystem;

                    // Notify developer
                    const string contextMessage = "Unexpected error adding system.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.AddSystemFailedMessageBox();
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed; // Hide overlay
                    if (IsLoaded) // Check if the window is still loaded
                    {
                        AddSystemButton.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in AddSystemButtonClickAsync.");
            }
            finally
            {
                IsOperationInProgress = false;
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in AddSystemButtonClickAsync.");
        }
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private static async Task UpdateSystemXmlAsync(
        string xmlPath,
        EasyModeSystemConfig selectedSystem,
        string systemFolder, // This should be the raw path with %BASEFOLDER%
        string systemImageFolder) // This should be the raw path with %BASEFOLDER%
    {
        XDocument xmlDoc = null; // Initialize to null
        try
        {
            // Attempt to load existing XML content asynchronously
            if (File.Exists(xmlPath))
            {
                try
                {
                    var xmlContent = await File.ReadAllTextAsync(xmlPath);
                    // Only parse if content is not empty to avoid errors with empty files
                    if (!string.IsNullOrWhiteSpace(xmlContent))
                    {
                        xmlDoc = XDocument.Parse(xmlContent);
                        // Check if the root element is valid
                        if (xmlDoc.Root == null || xmlDoc.Root.Name != "SystemConfigs")
                        {
                            // If root is null or incorrect, treat as invalid and create new
                            xmlDoc = null; // Reset xmlDoc to trigger creation below

                            // Notify developer
                            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new XmlException("Loaded system.xml has missing or invalid root element."), "Invalid root in system.xml, creating new.");
                        }
                    }
                }
                catch (XmlException ex) // Catch specific XML parsing errors
                {
                    // Notify developer
                    // Log the parsing error but proceed to create a new document
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error parsing existing system.xml, creating new.");

                    xmlDoc = null; // Ensure we create a new one
                }
                catch (Exception ex) // Catch other file reading errors
                {
                    // Notify developer
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error reading existing system.xml.");

                    throw new IOException("Could not read the existing system configuration file.", ex); // Rethrow as IO
                }
            }

            // If xmlDoc is still null (file didn't exist, was empty, or had invalid root), create a new one
            xmlDoc ??= new XDocument(new XElement("SystemConfigs"));

            // --- Proceed with modification logic ---
            if (xmlDoc.Root != null)
            {
                var systemManagers = xmlDoc.Root.Descendants("SystemConfig").ToList(); // Safe now because Root is guaranteed
                var existingSystem = systemManagers.FirstOrDefault(config => config.Element("SystemName")?.Value == selectedSystem.SystemName);
                if (existingSystem != null)
                {
                    // Overwrite existing system (in memory)
                    OverwriteExistingSystem(existingSystem, selectedSystem, systemFolder, systemImageFolder);
                }
                else
                {
                    // Create new system element (in memory)
                    var newSystemElement = SaveNewSystem(selectedSystem, systemFolder, systemImageFolder);
                    xmlDoc.Root.Add(newSystemElement);
                }
            }

            // Sort the elements (in memory)
            if (xmlDoc.Root != null)
            {
                var sortedElements = xmlDoc.Root.Elements("SystemConfig")
                    .OrderBy(static systemElement => systemElement.Element("SystemName")?.Value)
                    .ToList(); // Create a list of sorted elements
                // Replace the nodes in the original document's root
                xmlDoc.Root.ReplaceNodes(sortedElements);
            }

            // Save the updated and sorted XML document asynchronously with proper formatting
            // Use SaveOptions.None for default indentation
            await Task.Run(() =>
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ", // Use 2 spaces for indentation
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = System.Text.Encoding.UTF8
                };

                using var writer = XmlWriter.Create(xmlPath, settings);
                xmlDoc.Declaration ??= new XDeclaration("1.0", "utf-8", null);
                xmlDoc.Save(writer);
            });
        }
        catch (IOException ex) // Handle file saving errors (permissions, disk full, etc.)
        {
            // Notify developer
            const string contextMessage = "Error saving system.xml.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            throw new InvalidOperationException("Could not save system configuration.", ex);
        }
        catch (Exception ex) // Catch other potential errors
        {
            // Notify developer
            const string contextMessage = "Unexpected error updating system.xml.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            throw new InvalidOperationException("An unexpected error occurred while updating system configuration.", ex);
        }
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private static XElement SaveNewSystem(EasyModeSystemConfig selectedSystem, string systemFolder, string systemImageFolder)
    {
        var newSystemElement = new XElement("SystemConfig",
            new XElement("SystemName", selectedSystem.SystemName),
            new XElement("SystemFolders", new XElement("SystemFolder", systemFolder)), // Only one folder from EasyMode
            new XElement("SystemImageFolder", systemImageFolder),
            new XElement("SystemIsMAME", selectedSystem.SystemIsMame.ToString()),
            new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format))),
            new XElement("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString()),
            new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))),
            new XElement("Emulators",
                new XElement("Emulator",
                    new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                    new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                    new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                    , new XElement("ImagePackDownloadLink", selectedSystem.Emulators.Emulator.ImagePackDownloadLink)
                    , new XElement("ImagePackDownloadLink2", selectedSystem.Emulators.Emulator.ImagePackDownloadLink2)
                    , new XElement("ImagePackDownloadLink3", selectedSystem.Emulators.Emulator.ImagePackDownloadLink3)
                    , new XElement("ImagePackDownloadLink4", selectedSystem.Emulators.Emulator.ImagePackDownloadLink4)
                    , new XElement("ImagePackDownloadLink5", selectedSystem.Emulators.Emulator.ImagePackDownloadLink5)
                    , new XElement("ImagePackDownloadExtractPath", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath)
                )
            )
        );
        return newSystemElement;
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private static void OverwriteExistingSystem(XElement existingSystem, EasyModeSystemConfig selectedSystem, string systemFolder, string systemImageFolder)
    {
        existingSystem.SetElementValue("SystemName", selectedSystem.SystemName);
        // Update SystemFolders
        var foldersElement = existingSystem.Element("SystemFolders");
        if (foldersElement == null)
        {
            foldersElement = new XElement("SystemFolders");
            // Add it after SystemName to maintain order
            existingSystem.Element("SystemName")?.AddAfterSelf(foldersElement);
        }

        foldersElement.ReplaceNodes(new XElement("SystemFolder", systemFolder)); // Only one folder from EasyMode

        existingSystem.SetElementValue("SystemImageFolder", systemImageFolder);
        existingSystem.SetElementValue("SystemIsMAME", selectedSystem.SystemIsMame.ToString());
        existingSystem.Element("FileFormatsToSearch")?.ReplaceNodes(selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format)));
        existingSystem.SetElementValue("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString());
        existingSystem.Element("FileFormatsToLaunch")?.ReplaceNodes(selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format)));
        existingSystem.Element("Emulators")?.Remove();
        existingSystem.Add(new XElement("Emulators",
            new XElement("Emulator",
                new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                , new XElement("ImagePackDownloadLink", selectedSystem.Emulators.Emulator.ImagePackDownloadLink)
                , new XElement("ImagePackDownloadLink2", selectedSystem.Emulators.Emulator.ImagePackDownloadLink2)
                , new XElement("ImagePackDownloadLink3", selectedSystem.Emulators.Emulator.ImagePackDownloadLink3)
                , new XElement("ImagePackDownloadLink4", selectedSystem.Emulators.Emulator.ImagePackDownloadLink4)
                , new XElement("ImagePackDownloadLink5", selectedSystem.Emulators.Emulator.ImagePackDownloadLink5)
                , new XElement("ImagePackDownloadExtractPath", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath)
            )
        ));
    }

    private void UpdateAddSystemButtonState()
    {
        if (_disposed) return;

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem?.Emulators?.Emulator == null)
        {
            AddSystemButton.IsEnabled = false;
            return;
        }

        var emulatorConfig = selectedSystem.Emulators.Emulator;

        // The emulator is always required if a download link exists.
        var isEmulatorDownloadRequired = !string.IsNullOrEmpty(emulatorConfig.EmulatorDownloadLink);
        var isEmulatorReady = !isEmulatorDownloadRequired || IsEmulatorDownloaded;

        // The core is only required if a download link for it exists.
        var isCoreDownloadRequired = !string.IsNullOrEmpty(emulatorConfig.CoreDownloadLink);
        var isCoreReady = !isCoreDownloadRequired || IsCoreDownloaded;

        // The "Add System" button is enabled if all *required* components (emulator and core) are ready.
        // Image packs are optional and do not affect this logic.
        AddSystemButton.IsEnabled = isEmulatorReady && isCoreReady && !IsOperationInProgress;
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private string GetBooleanPropertyNameForType(string type)
    {
        return type switch
        {
            EasyModeManager.DownloadType.Emulator => nameof(IsEmulatorDownloaded),
            EasyModeManager.DownloadType.Core => nameof(IsCoreDownloaded),
            EasyModeManager.DownloadType.ImagePack1 => nameof(IsImagePack1Downloaded),
            EasyModeManager.DownloadType.ImagePack2 => nameof(IsImagePack2Downloaded),
            EasyModeManager.DownloadType.ImagePack3 => nameof(IsImagePack3Downloaded),
            EasyModeManager.DownloadType.ImagePack4 => nameof(IsImagePack4Downloaded),
            EasyModeManager.DownloadType.ImagePack5 => nameof(IsImagePack5Downloaded),
            _ => null
        };
    }

    private async void CloseWindowRoutineAsync(object sender, EventArgs e)
    {
        try // Top-level catch for async Task method
        {
            if (StopDownloadButton.IsEnabled)
            {
                StopDownloadButton_Click(null, null);
                await Task.Delay(200);
            }

            _manager = null;
            Dispose();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error closing the Add System window.");
        }
    }

    private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var chooseaFolderwithRoMsorIsOs2 = (string)Application.Current.TryFindResource("ChooseafolderwithROMsorISOsforthissystem") ?? "Choose a folder with 'ROMs' or 'ISOs' for this system";

        // Create a new OpenFolderDialog
        var openFolderDialog = new OpenFolderDialog
        {
            Title = chooseaFolderwithRoMsorIsOs2
        };

        // Show the dialog and handle the result
        if (openFolderDialog.ShowDialog() == true)
        {
            SystemFolderTextBox.Text = openFolderDialog.FolderName;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error opening the download link.");

            // Notify user
            MessageBoxLibrary.CouldNotOpenTheDownloadLink();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _downloadManager?.Dispose();
        _manager?.Dispose();

        _disposed = true;
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        _downloadManager?.CancelDownload();
        IsOperationInProgress = false;

        LoadingOverlay.Visibility = Visibility.Collapsed;
        MainContentGrid?.IsEnabled = true;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in EasyModeWindow.");
        UpdateStatusBar.UpdateContent("Emergency reset performed.", Application.Current.MainWindow as MainWindow);
    }
}