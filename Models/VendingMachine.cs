using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;

using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static DynamicData.Kernel.OptionExtensions;

namespace VendingMachine {

    public enum Currency {
        Euros,
        Dollars,
        SEK
    }

    public enum CashOfDenomination {
        Enkrona = 1,
        Femkrona = 5,
        Tiokrona = 10,
        Tjugolapp = 20,
        Femtiolapp = 50,
        Hundralapp = 100,
        Femhundralapp = 500,
        Tusenlapp = 1000,
    }

    public class VendingMachine : ReactiveObject, ILegalEntity {
        public static readonly CashOfDenomination[] MoneyDenominations = new CashOfDenomination[] {
            CashOfDenomination.Enkrona, CashOfDenomination.Femkrona, CashOfDenomination.Tiokrona, CashOfDenomination.Tjugolapp,
            CashOfDenomination.Femtiolapp, CashOfDenomination.Hundralapp, CashOfDenomination.Femhundralapp, CashOfDenomination.Tusenlapp
        };
        private readonly IVendingMachineManager manager;
        private readonly SourceCache<StockedProduct, int> _stock = new SourceCache<StockedProduct, int>((p) => p.Product.GetHashCode());
        public IObservable<IChangeSet<StockedProduct, int>> StockStatus => _stock.AsObservableCache().Connect();
        public static readonly decimal VAT = 0.25M;
        [Reactive] public decimal Income { get; private set; }

        [Reactive] public decimal Expenses { get; private set; }
        public decimal Profit => ProfitHelper.Value;
        private readonly ObservableAsPropertyHelper<decimal> ProfitHelper;
        [Reactive] public decimal ProfitExcludingCurrentStockOutlay { get; private set; }

        private readonly Dictionary<Customer, List<Order>> orders = new Dictionary<Customer, List<Order>>();
        public IReadOnlyCollection<Order> OrdersBy(Customer customer) {
            return orders[customer].AsReadOnly();
        }
        public readonly Currency Currency;
        private readonly uint MaxCapacityPerProduct;

        public IDisposable UpdateProfitExcludingCurrentStockOutlaySubscriptionFactory() {
            UpdateProfitExcludingCurrentStockOutlay();
            return Disposable.Empty;
        }

        public decimal UpdateProfitExcludingCurrentStockOutlay() {
            ProfitExcludingCurrentStockOutlay = Profit + _stock.Items.Select(sp => sp.Product.WholesalePrice * sp.Quantity).Aggregate<decimal>((acc, nextValue) => acc + nextValue);
            return ProfitExcludingCurrentStockOutlay;
        }

        public VendingMachine(IVendingMachineManager manager, Currency currency, uint maxCapacityPerProduct) {
            ProfitHelper = this.WhenAnyValue(x => x.Income, x => x.Expenses, (income, expenses) => income - expenses).ToProperty(source: this, property: x => x.Profit);
            StockStatus.SubscribeMany((_) => UpdateProfitExcludingCurrentStockOutlaySubscriptionFactory());
            this.ObservableForProperty(x => x.Profit).Subscribe(x => UpdateProfitExcludingCurrentStockOutlay());
            Currency = currency;
            MaxCapacityPerProduct = maxCapacityPerProduct;
            this.manager = manager;
            //No need to worry about null because of C# 8.0.
            //   (It would be caught at compile-time)
            //To have IVendingMachineOperator be nullable,
            //I'd have to declare it as IVendingMachineOperator?
            this.manager.SetupVendingMachine(_stock.AsObservableCache()).Subscribe(StockProduct);
        }

        private void StockProduct((IProduct product, uint quantity)p) {
            if (p.quantity <= 0) {
                throw new ArgumentOutOfRangeException();
            }
            Expenses += p.product.WholesalePrice * p.quantity;
            Console.WriteLine($"Stocking {p.quantity} of {p.product}.");
            if (_stock.Lookup(p.product.GetHashCode()).HasValue) {
                _stock.Lookup(p.product.GetHashCode()).Value.Restock(p.quantity);
            } else {
                _stock.AddOrUpdate(new StockedProduct(p.product, p.quantity, MaxCapacityPerProduct));
            }
        }

        public void NewIncomingStreamOfOrders(IObservable < (Customer customer, IReadOnlyCollection < (IProduct product, uint quantity) > cart) > incomingOrders) {
            incomingOrders.Subscribe(ProcessOrder);
        }

        private void ProcessOrder((Customer customer, IReadOnlyCollection < (IProduct product, uint quantity) > cart)incomingOrder) {
            Customer customer = incomingOrder.customer;
            var cart = incomingOrder.cart;
            if (cart.Count <= 0) {
                throw new Exception("Can't process an empty order!");
            }
            var productsToDeliver = new List<IProduct>();
            foreach (var item in cart) {
                var stockedProduct = _stock.Lookup(item.product.GetHashCode()).ValueOrThrow(() => new MissingKeyException($"{item.product}"));
                productsToDeliver.Add(stockedProduct.PackageProductFor(customer, item.quantity));
                _stock.AddOrUpdate(stockedProduct);
            }
            List<Order> customerOrders;

            if (!orders.TryGetValue(customer, out customerOrders)) {
                var newList = new List<Order>();
                orders[customer] = newList;
                customerOrders = newList;
            }
            var order = new Order(cart, customer, VAT);
            customerOrders.Add(order);
            Income += order.TotalPrice; //Excluding VAT
            customer.ReceivePurchase(productsToDeliver);
            customer.ConsumeEverything();

        }

    }

}
