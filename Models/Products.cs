using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VendingMachine;

namespace VendingMachine {

    public interface IProduct {
        bool StillUsable { get; }
        string Name { get; }
        string Description { get; }
        decimal ManufacturingCost { get; }
        decimal WholesalePrice { get; }
        decimal RecommendedRetailPrice { get; }
        decimal RetailPrice { get; }
        decimal RetailPriceWithVAT { get; }
        string Use();
        IEnumerable<string> UseItAll();
        IProduct CloneForNewOwner(ILegalEntity owner);
    }

    public class ProductUseException : Exception {
        public ProductUseException() { }

        public ProductUseException(string message) : base(message) { }

        public ProductUseException(string message, Exception inner) : base(message, inner) { }
    }

    public abstract class Product : IProduct {
        public ILegalEntity ? Owner { get; private set; } = null;
        public readonly double Servings;
        protected double RemainingServings { get; set; }
        public bool StillUsable { get; protected set; }
        public string Name { get; }
        public string Description { get; }
        protected virtual decimal WholesaleMarkupFactor { get; } = 1.2m;
        protected virtual decimal SuggestedStoreMarkupFactor { get; } = 1.2m;
        protected virtual decimal ExtraStoreMarkupFactor { get; } = 1.0m;
        public decimal ManufacturingCost { get; }
        public decimal WholesalePrice { get; }
        public decimal RecommendedRetailPrice { get; }
        public decimal RetailPrice { get; }
        public decimal RetailPriceWithVAT { get; }

        protected readonly bool Template;

        public abstract string Use();

        public IEnumerable<string> UseItAll() {
            List<string> useDescriptions = new List<string>();
            do {
                useDescriptions.Add(Use());
            } while (StillUsable);
            return useDescriptions;
        }

        public Product(string name, string description, decimal manufacturingCost, double servings, [Optional] decimal extraStoreMarkupFactor, [Optional] ILegalEntity ? owner) {
            this.Name = name;
            this.Description = description;
            this.ManufacturingCost = manufacturingCost;
            this.ExtraStoreMarkupFactor = extraStoreMarkupFactor;
            this.WholesalePrice = ManufacturingCost * WholesaleMarkupFactor;
            this.RecommendedRetailPrice = WholesalePrice * SuggestedStoreMarkupFactor;
            this.RetailPrice = RecommendedRetailPrice * ExtraStoreMarkupFactor;
            this.Servings = servings;
            this.Owner = owner;
            if ((Owner != null && Owner.GetType().IsAssignableFrom(typeof(VendingMachine)))) {
                RetailPriceWithVAT = RetailPrice * VendingMachine.VAT;
            } else {
                RetailPriceWithVAT = RetailPrice * VendingMachine.VAT;
            }
            RemainingServings = servings;
            StillUsable = RemainingServings > 0;
            Template = true;
        }

        protected Product(Product product, ILegalEntity owner) : this(product.Name, product.Description, product.ManufacturingCost, product.Servings, product.ExtraStoreMarkupFactor, owner) {
            Template = false;
        }

        public override string ToString() {
            return Name;
        }

        public abstract IProduct CloneForNewOwner(ILegalEntity owner);
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

        public Drink(string name, string description, decimal manufacturingCost, double servings, [Optional] decimal extraStoreMarkupFactor, [Optional] ILegalEntity ? owner) : base(name, description, manufacturingCost, servings, extraStoreMarkupFactor, owner!) { }

        protected Drink(Drink drink, ILegalEntity owner) : base((Product)drink, owner) { }

        public override IProduct CloneForNewOwner(ILegalEntity owner) {
            if (!base.Template) {
                throw new ProductUseException("Owners are not allowed to materialize Products out of nothing!");
            }
            return new Drink(this, owner);
        }
    }
}
