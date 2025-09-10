using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using TemplateStudioTest.Core.Contracts.Services;
using TemplateStudioTest.Helpers;

namespace TemplateStudioTest.ViewModels;

public partial class LedControlViewModel : ObservableRecipient, IDisposable
{
    #region EasterEgg
    private void EasterEgg()
    {
        ; ; ; ; ;  
        ; ; 
        ; ; 
        ; ; ; ; ;
              ; ;
        ; ; ; ; ;
    }
    #endregion
    #region Serial Props
    [ObservableProperty] private ISerialService _serial;
    private readonly DispatcherQueue _uiQueue;
    public bool IsButtonsEnabled => IsSerialConnected && IsToggleButtonOn;
    public bool IsSerialConnected => Serial.IsConnected();
    
    [ObservableProperty] private bool isToggleButtonOn = false;
    partial void OnIsToggleButtonOnChanged(bool value)
    {
        if (value)
        {
            OnStart();
        }
        else
        {
            OnStop();
        }
        OnPropertyChanged(nameof(IsButtonsEnabled));
    }

    #endregion
    public string ToggleBtnOnContent => "On".GetLocalized();
    public string ToggleBtnOffContent => "Off".GetLocalized();
    public string ToggleBtnOnSingleLedContent => "MultiLed".GetLocalized();
    public string ToggleBtnOffSingleLedContent => "SingleLed".GetLocalized();
    [ObservableProperty] private bool isToggleBtnLedModeOn = false;
    partial void OnIsToggleBtnLedModeOnChanged(bool value)
    {
        if (!value)
        {

        }
    }
    
    [ObservableProperty] private SolidColorBrush led_0_Foreground = new(Colors.White);
    [ObservableProperty] private SolidColorBrush led_1_Foreground = new(Colors.White);
    [ObservableProperty] private SolidColorBrush led_2_Foreground = new(Colors.White);
    [ObservableProperty] private SolidColorBrush led_3_Foreground = new(Colors.White);
    private CancellationTokenSource cancellationTokenSource;
    public LedControlViewModel(ISerialService serial)
    {
        _uiQueue = DispatcherQueue.GetForCurrentThread();
        Serial = serial;
        InitializeProps();
        Serial.OnSerialConnected += Serial_OnSerialConnected;
        Serial.OnSerialDisconnected += Serial_OnSerialDisconnected;
    }

    private void InitializeProps()
    {
        OnPropertyChanged(nameof(IsSerialConnected));
        if (IsSerialConnected)
            OnStart();
    }
    private void Serial_OnSerialDisconnected(object? sender, EventArgs e) => OnStop();
    private void Serial_OnSerialConnected(object? sender, EventArgs e) => OnStart();

    private void OnStart()
    {
        OnPropertyChanged(nameof(IsButtonsEnabled));
        OnPropertyChanged(nameof(IsSerialConnected));
        if (IsToggleButtonOn)
        {            
            StartMonitoringRoutine();
            GetLedStatus();
        }

    }
    private void OnStop()
    {
        OnPropertyChanged(nameof(IsButtonsEnabled));
        OnPropertyChanged(nameof(IsSerialConnected));
        IsToggleButtonOn = false;
        StopMonitoringRoutine();        
        UpdateLedsUI([0, 0, 0, 0]);
    }

    private void StartMonitoringRoutine()
    {
        Serial.OnLedsStatusReceived += Serial_OnLedsStatusReceived;
        cancellationTokenSource = new();
        _ = Task.Run(() => Serial.StartMonitoringRoutine(cancellationTokenSource.Token));
    }


    private void StopMonitoringRoutine()
    {
        Serial.OnLedsStatusReceived -= Serial_OnLedsStatusReceived;
        cancellationTokenSource?.Cancel();
    }

    private void Serial_OnLedsStatusReceived(object? sender, byte data) =>
        _uiQueue.TryEnqueue(() => UpdateLedsUI(data));


    private void UpdateLedsUI(byte[] ledsStatus)
    {
        Led_0_Foreground = ledsStatus[0] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
        Led_1_Foreground = ledsStatus[1] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
        Led_2_Foreground = ledsStatus[2] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
        Led_3_Foreground = ledsStatus[3] == 1 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
    }
    private void UpdateLedsUI(byte ledOnIndex)
    {
        var leds = new byte[4];
        leds[ledOnIndex] = 1;
        UpdateLedsUI(leds);
    }
    private void GetLedStatus()
    {
        if (Serial.TryGetLedsStatus())
        {
            //UpdateLedsUI(byteResponse);
        }
        else
        {
            // Mostrar que não foi possível obter o status

        }
    }
    private void SetLedStatus(byte ledIndex)
    {
        if (Serial.TrySetLedsStatus(ledIndex))
        {

        }
        else
        {
            // Mostrar que não foi possível setar o led
        }
    }

    [RelayCommand]
    private void LedClick(object ledIndex)
    {
        if (!IsButtonsEnabled || !IsSerialConnected) return;
        if (int.TryParse(ledIndex?.ToString(), out int index) && index >= 0 && index <= 3)
        {
            SetLedStatus((byte)index);
            //UpdateLedStatus();
        }
    }

    public void Dispose() => StopMonitoringRoutine();
}
