using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using TemplateStudioTest.Contracts.Services;
using TemplateStudioTest.Core.Contracts.Services;
using TemplateStudioTest.Core.Models;
using TemplateStudioTest.Helpers;
using TemplateStudioTest.Views;

namespace TemplateStudioTest.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    #region Serial Props
    [ObservableProperty] private ISerialService _serial;

    [ObservableProperty] private string[] portNames;
    [ObservableProperty] private int[] baudRates;
    [ObservableProperty] private Parity[] parities;
    [ObservableProperty] private StopBits[] stopBits;
    [ObservableProperty] private int[] dataBits;

    [ObservableProperty] private string selectedPortName;
    [ObservableProperty] private int selectedBaudRate;
    [ObservableProperty] private Parity selectedParity;
    [ObservableProperty] private StopBits selectedStopBits;
    [ObservableProperty] private int selectedDataBits;
    public string BtnConnectText => IsSerialConnected ? "Disconnect".GetLocalized() : "Connect".GetLocalized();

    public bool IsSerialConnected => Serial.IsConnected();
    #endregion

    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, ISerialService serialService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        Serial = serialService;
        UpdatePortNames();
        LoadComboboxes();
    }

    #region Serial Methods
    [RelayCommand]
    private void UpdatePortNames()
    {
        PortNames = (string[])Serial.GetPortNames();
    }
    private void LoadComboboxes()
    {
        BaudRates = [9600, 19200, 38400, 57600, 115200];

        Parities = (Parity[])Enum.GetValues(typeof(Parity));
        StopBits = (StopBits[])Enum.GetValues(typeof(StopBits));
        DataBits = [5, 6, 7, 8];

        SelectedPortName = PortNames.LastOrDefault()!;
        SelectedDataBits = DataBits?[3] ?? DataBits!.LastOrDefault()!;
        SelectedBaudRate = BaudRates?[4] ?? BaudRates!.LastOrDefault()!;
        SelectedParity = Parity.None;
        SelectedStopBits = System.IO.Ports.StopBits.One;
    }
    [RelayCommand]
    private void ConnectCOM()
    {
        try
        {
            if (!IsSerialConnected)
            {
                if (string.IsNullOrWhiteSpace(SelectedPortName))
                    return;

                var serialM = new SerialModel()
                {
                    PortName = SelectedPortName,
                    BaudRates = SelectedBaudRate,
                    DataBits = SelectedDataBits,
                    Parities = SelectedParity,
                    StopBits = SelectedStopBits
                };
                if (Serial.TryConnect(serialM))
                {
                    OnPropertyChanged(nameof(IsSerialConnected));
                    OnPropertyChanged(nameof(BtnConnectText));
                }
            }
            else
            {
                if (Serial.TryDisconnect())
                {
                    OnPropertyChanged(nameof(IsSerialConnected));
                    OnPropertyChanged(nameof(BtnConnectText));
                }
            }
        }
        catch (Exception ex)
        {
            App.MainWindow.ShowMessageDialogAsync("SerialError".GetLocalized() + "\n" + ex.Message, "Error");
        }
    }
    #endregion

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
