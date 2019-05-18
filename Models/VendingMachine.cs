using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using DynamicData;

namespace VendingMachine {
    public interface IVendingMachine {

    }

    public enum Currency {
        Euros,
        Dollars,
        SEK
    }

    public class VendingMachine : IVendingMachine {
        private readonly IVendingMachineManager _op;
        private SourceCache<StockedProduct, int> _stock = new SourceCache<StockedProduct, int>((p) => p.Product.GetHashCode());
        public IObservable<IChangeSet<StockedProduct, int>> StockStatus => _stock.AsObservableCache().Connect();

        private readonly Currency Currency;
        private readonly uint MaxCapacityPerProduct;

        public VendingMachine(IVendingMachineManager op, Currency currency, uint maxCapacityPerProduct) {
            Currency = currency;
            MaxCapacityPerProduct = maxCapacityPerProduct;

            _op = op;
            //No need to worry about null because of C# 8.0.
            //   (It would be caught at compile-time)
            //To have IVendingMachineOperator be nullable,
            //I'd have to declare it as IVendingMachineOperator?
            _op.SetupVendingMachine(_stock.AsObservableCache()).Subscribe(StockProduct);
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
                _stock.Lookup(p.product.GetHashCode()).Value.DeliverProduct(15);

            }
        }
    }

}
