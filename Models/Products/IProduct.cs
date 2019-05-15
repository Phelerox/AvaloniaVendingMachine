using System;
using System.Collections.Generic;

namespace VendingMachine {
    public interface IProduct {
        bool StillUsable { get; }
        string Name { get; }
        string Description { get; }
        double RecommendedRetailPrice { get; }

        string Use();
        IEnumerable<string> UseItAll();
    }

    public class ProductUseException : Exception {
        public ProductUseException() { }

        public ProductUseException(string message) : base(message) { }

        public ProductUseException(string message, Exception inner) : base(message, inner) { }
    }
}
