using System.Drawing;
using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TemplateStudioTest.Contracts.Services;
using TemplateStudioTest.Core.Contracts.Services;
using TemplateStudioTest.Core.Models;
using TemplateStudioTest.Helpers;

namespace TemplateStudioTest.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    #region Serial Props
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

    public bool IsSerialConnected => Serial.IsConnected();

    public string BtnConnectText => IsSerialConnected ? "Disconnect".GetLocalized() : "Connect".GetLocalized();
    #endregion

    [ObservableProperty] private SolidColorBrush led_0_Foreground = new (Colors.White);
    [ObservableProperty] private SolidColorBrush led_1_Foreground = new (Colors.White);
    [ObservableProperty] private SolidColorBrush led_2_Foreground = new (Colors.White);
    [ObservableProperty] private SolidColorBrush led_3_Foreground = new (Colors.White);
    private readonly CancellationTokenSource cancellationTokenSource;
    public MainViewModel(ISerialService serial)
    {
        cancellationTokenSource = new();
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

        SelectedPortName    = PortNames.LastOrDefault()!;
        SelectedDataBits    = DataBits?[3] ?? DataBits!.LastOrDefault()!;
        SelectedBaudRate    = BaudRates?[4] ?? BaudRates!.LastOrDefault()!;
        SelectedParity      = Parity.None;
        SelectedStopBits    = System.IO.Ports.StopBits.One;
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

                    UpdateLedStatus();
                }
            }
            else
            {
                if (Serial.TryDisconnect())
                {
                    OnPropertyChanged(nameof(IsSerialConnected));
                    OnPropertyChanged(nameof(BtnConnectText));

                    UpdateLedsUI([0, 0, 0, 0]);
                }
            }
        }
        catch (Exception ex)
        {
            App.MainWindow.ShowMessageDialogAsync("SerialError".GetLocalized() + "\n" + ex.Message, "Error");
        }
    }

    private void UpdateLedsUI(byte[] ledsStatus)
    {
        Led_0_Foreground = ledsStatus[0] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
        Led_1_Foreground = ledsStatus[1] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
        Led_2_Foreground = ledsStatus[2] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
        Led_3_Foreground = ledsStatus[3] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
    }
    private void UpdateLedsUI(byte ledOnIndex)
    {
        Led_0_Foreground = new SolidColorBrush(Colors.White);
        Led_1_Foreground = new SolidColorBrush(Colors.White);
        Led_2_Foreground = new SolidColorBrush(Colors.White);
        Led_3_Foreground = new SolidColorBrush(Colors.White);
        switch (ledOnIndex)
        {
            case 0: Led_0_Foreground = new SolidColorBrush(Colors.Red); break;
            case 1: Led_1_Foreground = new SolidColorBrush(Colors.Red); break;
            case 2: Led_2_Foreground = new SolidColorBrush(Colors.Red); break;
            case 3: Led_3_Foreground = new SolidColorBrush(Colors.Red); break;
        }
    }
    private async void UpdateLedStatus()
    {
        (bool sucess, byte byteResponse) = await Serial.TryGetLedsStatus(cancellationTokenSource.Token);
        if (sucess)
        {
            UpdateLedsUI(byteResponse);
        }
        else
        {
            // Mostrar que não foi possível obter o status

        }
    }
    private async Task SetLedStatus(byte ledIndex)
    {
        (bool sucess, byte byteResponse) = await Serial.TrySetLedsStatus(cancellationTokenSource.Token, ledIndex);
        if (sucess)
        {
            UpdateLedsUI(byteResponse);
        }
        else
        {

        }
    }

    [RelayCommand]
    private void LedClick(object ledIndex)
    {
        if (!IsSerialConnected) return;
        if(int.TryParse(ledIndex?.ToString(), out int index) && index >= 0 && index <= 3)
        {
            SetLedStatus((byte)index);
            //UpdateLedStatus();
        }
    }

}
