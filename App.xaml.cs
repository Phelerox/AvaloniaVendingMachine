using Avalonia;
using Avalonia.Markup.Xaml;

namespace VendingMachine {
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
