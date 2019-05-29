using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using VendingMachine;

namespace VendingMachine {
    public interface ILegalEntity {
        // IEnumerable<IProduct> Owns { get; }
    }

    public class Customer : ReactiveObject, ILegalEntity {
        [Reactive] public decimal Money { get; private set; }
        private readonly List<IProduct> consumedProducts = new List<IProduct>();
        private readonly List<IProduct> unusedProducts = new List<IProduct>();
        public IEnumerable<IProduct> ConsumedProducts { get { return consumedProducts.AsReadOnly(); } }

        public IEnumerable<IProduct> OwnedProducts {
            get {
                var ownedProducts = new List<IProduct>(consumedProducts);
                ownedProducts.AddRange(unusedProducts);
                return ownedProducts.AsReadOnly();
            }
        }

        public Customer(decimal money) {
            Money = money;
        }

        public bool Pay(decimal price) {
            return price <= Money ? (Money -= price) >= 0 : false;
        }

        public void ReceiveChange(List<CashOfDenomination> cash) {
            foreach (var coin in cash) {
                Money += (int)coin;
            }
        }

        public void ReceivePurchase(IEnumerable<IProduct> boughtProducts) {
            unusedProducts.AddRange(boughtProducts);
        }

        public string ConsumeEverything() {
            string consumptionMessages = "";
            List<IProduct> consumed = new List<IProduct>();
            foreach (var product in unusedProducts) {
                string consumptionMessage = string.Concat(product.UseItAll());
                consumptionMessages += consumptionMessage;
                Console.WriteLine(consumptionMessage);
                consumed.Add(product);
            }
            consumed.ForEach(p => unusedProducts.Remove(p));
            consumedProducts.AddRange(consumed);
            return consumptionMessages;
        }
    }
}
