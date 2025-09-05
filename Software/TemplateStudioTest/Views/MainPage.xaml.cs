using Microsoft.UI.Xaml.Controls;

using TemplateStudioTest.ViewModels;

namespace TemplateStudioTest.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void ComboBoxPortNames_DropDownOpened(object sender, object e)
    {
        ViewModel.UpdatePortNamesCommand.Execute(DataContext);
    }
}
