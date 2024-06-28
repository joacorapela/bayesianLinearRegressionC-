
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BayesianLinearRegressionDemo1;

public partial class SecondWindow : Window
{
    public SecondWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}
