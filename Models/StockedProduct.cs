using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace VendingMachine {
    public class StockedProduct : ReactiveObject {
        [Reactive] public uint Quantity { get; private set; }

        public IObservable<int> QuantityAsInt {
            get {
                return this.WhenAnyValue(x => x.Quantity).Select(x => ((int)x));
            }
        }
        public readonly uint MaxCapacity;
        public IProduct Product { get; }

        public StockedProduct(IProduct product, uint quantity, uint maxCapacity = UInt32.MaxValue) {
            Product = product;
            Quantity = quantity;
            MaxCapacity = maxCapacity;
        }

        public void Restock(uint quantity) {
            if (Quantity + quantity > MaxCapacity) {
                throw new ArgumentOutOfRangeException();
            }
            Quantity += quantity;
        }

        public IEnumerable<IProduct> PackageProductFor(ILegalEntity customer, uint quantity) {
            var products = new List<IProduct>();
            if (quantity > Quantity) {
                throw new ArgumentOutOfRangeException("You don't have that many stocked!");
            }
            Quantity -= quantity;
            for (int i = 0; i < quantity; i++) {
                products.Add(Product.CloneForNewOwner(customer));
            }
            return products;
        }
    }
}
