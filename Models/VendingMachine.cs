using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using DynamicData;
using static DynamicData.Kernel.OptionExtensions;

namespace VendingMachine {

    public enum Currency {
        Euros,
        Dollars,
        SEK
    }

    public class VendingMachine {
        private readonly IVendingMachineManager manager;
        private SourceCache<StockedProduct, int> _stock = new SourceCache<StockedProduct, int>((p) => p.Product.GetHashCode());
        public IObservable<IChangeSet<StockedProduct, int>> StockStatus => _stock.AsObservableCache().Connect();

        private Dictionary<Customer, List<Order>> orders = new Dictionary<Customer, List<Order>>();
        public IReadOnlyCollection<Order> OrdersBy(Customer customer) {
            return orders[customer].AsReadOnly();
        }
        private readonly Currency Currency;
        private readonly uint MaxCapacityPerProduct;

        public VendingMachine(IVendingMachineManager manager, Currency currency, uint maxCapacityPerProduct) {
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
                productsToDeliver.Add(stockedProduct.DeliverProduct(item.quantity));
            }
            List<Order> customerOrders;

            if (!orders.TryGetValue(customer, out customerOrders)) {
                var newList = new List<Order>();
                orders[customer] = newList;
                customerOrders = newList;
            }
            customerOrders.Add(new Order(cart, customer, 0.25M));
            customer.StartConsuming(productsToDeliver);
        }

    }

}
