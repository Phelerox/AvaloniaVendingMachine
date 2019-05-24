using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;

using DynamicData;

using Microsoft.CSharp.RuntimeBinder;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace VendingMachine.ViewModels {
    public class VendingMachineViewModel : ViewModelBase, ISupportsActivation {
        private VendingMachine VendingMachine { get; set; }
        private ObservableAsPropertyHelper<IReadOnlyCollection<StockedProduct>> ? stockedProducts;
        public IReadOnlyCollection<StockedProduct> ? StockedProducts => stockedProducts?.Value;
        public Dictionary<IProduct, int> ShoppingCart = new Dictionary<IProduct, int>();
        private IObserver < IReadOnlyCollection < (IProduct product, uint quantity) >> ? orderReceiver;
        [Reactive] public int ProductsInCart { get; private set; }

        public string VendingMachineGreeting => "Welcome to the Vending Machine!";

        public ViewModelActivator Activator { get; }

        public VendingMachineViewModel() {
            IVendingMachineManager manager = new CocaColaVendingMachineManager();
            //TODO: Have a customer associated with each ViewModel, so that I could have two VendingMachineViews in MainWindow that each have different backing ViewModels and Customers.
            //TODO: Would be so cool to then showcase the power of the reactive paradigm by allowing instant reservations (time-limited?) of the stock in a user's cart, preventing other users from buying it.
            //TODO: For bonus points, it wouldn't be very hard to use different threads.
            VendingMachine = new VendingMachine(manager, Currency.SEK, 25);
            VendingMachine.NewIncomingStreamOfOrders(Observable.Create < IReadOnlyCollection < (IProduct product, uint quantity) >>(
                (IObserver < IReadOnlyCollection < (IProduct product, uint quantity) >> observer) => {
                    if (orderReceiver == null) {
                        orderReceiver = observer;
                    } else {
                        throw new Exception();
                    }
                    return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                }));

            var ObservingStockDisposable = VendingMachine.StockStatus.ToCollection().ToProperty(source: this, property: p => p.StockedProducts, out stockedProducts!);

            Activator = new ViewModelActivator();
            DoInsertCoin = ReactiveCommand.Create<int>(RunInsertCoin);
            var canPlaceOrder = this.WhenAnyValue(
                x => x.ProductsInCart,
                (productsInCart) =>
                productsInCart > 0);
            DoPlaceOrder = ReactiveCommand.Create(RunPlaceOrder, canPlaceOrder);
            this.WhenActivated(disposables => {
                // Handle ViewModel activation and deactivation.
                Disposable.Create(() => this.HandleDeactivation()).DisposeWith(disposables);
                ObservingStockDisposable.DisposeWith(disposables);
            });
        }

        public ReactiveCommand<int, Unit> DoInsertCoin { get; }
        public ReactiveCommand<Unit, Unit> DoPlaceOrder { get; }

        void RunInsertCoin(int denomination) {
            throw new NotImplementedException();
        }

        public void CartItemChanged(AvaloniaPropertyChangedEventArgs e) {
            NumericUpDown spinner = (NumericUpDown)e.Sender;
            IProduct product = (IProduct)spinner.Tag;
            int quantity = (int)spinner.Value;
            int oldQuantity;
            bool alreadyInCart = ShoppingCart.TryGetValue(product, out oldQuantity);
            if (quantity <= 0) {
                ProductsInCart -= oldQuantity;
                ShoppingCart.Remove(product);
            } else {
                ProductsInCart += (quantity - oldQuantity);
                ShoppingCart[product] = quantity;
            }
            //Debugging section:
            if (ShoppingCart.Keys.Count == 0 && ProductsInCart != 0) {
                throw new Exception("Critical invariant does not hold!");
            }
            Console.WriteLine($"Updated item in Shopping Cart: {product} {quantity}");
            foreach (var cartItem in ShoppingCart) {
                if (cartItem.Key != product)
                    Console.WriteLine($"Rest of Shopping Cart: {cartItem.Key} {cartItem.Value}");
            }
        }

        void RunPlaceOrder() {
            var order = new List < (IProduct product, uint quantity) > (ShoppingCart.Keys.Count);
            foreach (var item in ShoppingCart) {
                order.Add((item.Key, (uint)item.Value));
            }

            orderReceiver?.OnNext(order.AsReadOnly());
            //TODO: Reset all NumericUpDown values to 0. Might need to do a proper databinding for the value to a collection Property in the ViewModel. https://stackoverflow.com/questions/15380466/xaml-binding-a-collection-within-a-datatemplate
        }

        private void HandleDeactivation() {
            return;
        }

    }
}
