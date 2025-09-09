using Microsoft.UI.Xaml.Controls;
using TemplateStudioTest.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TemplateStudioTest.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class HexCommunicationPage : Page
{
    public HexCommunicationViewModel ViewModel
    {
        get;
    }
    public HexCommunicationPage()
    {
        ViewModel = App.GetService<HexCommunicationViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
