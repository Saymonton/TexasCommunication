using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TemplateStudioTest.Core.Contracts.Services;

namespace TemplateStudioTest.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ISerialService _serial;

    [ObservableProperty] private string[] portNames;
    [ObservableProperty] private string[] baudRates;
    [ObservableProperty] private Parity[] parities;
    [ObservableProperty] private StopBits[] stopBits;

    [ObservableProperty] private string selectedPortName;
    [ObservableProperty] private string selectedBaudRate;
    [ObservableProperty] private Parity selectedParity;
    [ObservableProperty] private StopBits selectedStopBits;
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
        BaudRates = ["9600", "19200", "38400", "57600", "115200"];
        Parities = (Parity[])Enum.GetValues(typeof(Parity));
        StopBits = (StopBits[])Enum.GetValues(typeof(StopBits));

        SelectedPortName    = PortNames.FirstOrDefault()!;
        SelectedBaudRate    = BaudRates?[4] ?? BaudRates!.LastOrDefault()!;
        SelectedParity      = Parity.None;
        SelectedStopBits    = System.IO.Ports.StopBits.One;
    }
}
