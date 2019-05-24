using System;
using System.Collections.Generic;

namespace VendingMachine {

    public interface IProduct {
        bool StillUsable { get; }
        string Name { get; }
        string Description { get; }
        decimal RecommendedRetailPrice { get; }

        string Use();
        IEnumerable<string> UseItAll();
        IProduct Clone();
    }

    public class ProductUseException : Exception {
        public ProductUseException() { }

        public ProductUseException(string message) : base(message) { }

        public ProductUseException(string message, Exception inner) : base(message, inner) { }
    }

    public abstract class Product : IProduct {
        public readonly double Servings;
        protected double RemainingServings { get; set; }
        public bool StillUsable { get; protected set; }
        public string Name { get; }
        public string Description { get; }
        public decimal RecommendedRetailPrice { get; }
        protected readonly bool Template;

        public abstract string Use();

        public IEnumerable<string> UseItAll() {
            List<string> useDescriptions = new List<string>();
            do {
                useDescriptions.Add(Use());
            } while (StillUsable);
            return useDescriptions;
        }

        public Product(string name, string description, decimal recommendedRetailPrice, double servings) {
            this.Name = name;
            this.Description = description;
            this.RecommendedRetailPrice = recommendedRetailPrice;
            this.Servings = servings;
            RemainingServings = servings;
            StillUsable = RemainingServings > 0;
            Template = true;
        }

        protected Product(Product product) : this(product.Name, product.Description, product.RecommendedRetailPrice, product.Servings) {
            Template = false;
        }

        public override string ToString() {
            return Name;
        }

        public abstract IProduct Clone();
    }

    public class Drink : Product {

        public override string Use() {
            string useDescription;
            if (!base.StillUsable) {
                throw new ProductUseException("It's empty!");
            } else if (base.RemainingServings == 1) {
                useDescription = $"You drink the rest of the {Name}.";
            } else if (base.RemainingServings > 0 && base.RemainingServings < 1) {
                useDescription = $"You drink the very last sip of the {Name}.";
            } else {
                useDescription = $"You drink from the {Name}.";
            }
            base.StillUsable = --base.RemainingServings > 0;
            return useDescription;
        }

        public Drink(string name, string description, decimal recommendedRetailPrice, double servings) : base(name, description, recommendedRetailPrice, servings) { }

        protected Drink(Drink drink) : base((Product)drink) { }

        public override IProduct Clone() {
            if (!base.Template) {
                throw new ProductUseException("Customers are not allowed to materialize Products out of nothing!");
            }
            return new Drink(this);
        }
    }
}
