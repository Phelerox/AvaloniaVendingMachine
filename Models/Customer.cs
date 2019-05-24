using System;
using System.Collections.Generic;
using System.Reactive;

namespace VendingMachine {
    public class Customer {
        private readonly List<IProduct> consumedProducts = new List<IProduct>();
        public IEnumerable<IProduct> ConsumedProducts { get { return consumedProducts.AsReadOnly(); } }

        public Customer() {

        }

        public string StartConsuming(IEnumerable<IProduct> boughtProducts) {
            string consumptionMessages = "";
            foreach (var product in boughtProducts) {
                string consumptionMessage = string.Concat(product.UseItAll());
                consumptionMessages += consumptionMessage;
                Console.WriteLine(consumptionMessage);
                consumedProducts.Add(product);
            }

            return consumptionMessages;
        }
    }
}
