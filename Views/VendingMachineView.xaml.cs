using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VendingMachine.Views
{
    public class VendingMachineView : UserControl
    {
        public VendingMachineView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}