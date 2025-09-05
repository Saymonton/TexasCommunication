using Microsoft.UI.Xaml.Data;

namespace TemplateStudioTest.Helpers;
public class BoolInverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is bool boolValue && !boolValue;
    
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
