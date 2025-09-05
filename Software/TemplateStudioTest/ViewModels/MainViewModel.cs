using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TemplateStudioTest.Core.Contracts.Services;
using TemplateStudioTest.Core.Models;
using TemplateStudioTest.Helpers;

namespace TemplateStudioTest.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ISerialService _serial;

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

    [ObservableProperty] private string btnConnectText = "Connect.Text".GetLocalized();
    public MainViewModel(ISerialService serial)
    {
        Serial = serial;
        UpdatePortNames();
        LoadComboboxes();
    }
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

        SelectedPortName    = PortNames.FirstOrDefault()!;
        SelectedDataBits    = BaudRates?[3] ?? BaudRates!.LastOrDefault()!;
        SelectedBaudRate    = BaudRates?[4] ?? BaudRates!.LastOrDefault()!;
        SelectedParity      = Parity.None;
        SelectedStopBits    = System.IO.Ports.StopBits.One;
    }
    [RelayCommand]
    private void ConnectCOM()
    {
        if (string.IsNullOrWhiteSpace(SelectedPortName))
            return;

        var serialM = new SerialModel()
        {
            PortName = SelectedPortName,
            BaudRates = SelectedBaudRate,
            Parities = SelectedParity,
            StopBits = SelectedStopBits
        };
        if (Serial.TryConnect(serialM))
        {
            BtnConnectText = "Disconnect.Text".GetLocalized();
        }
    }
}
