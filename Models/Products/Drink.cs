using System;
using System.Collections.Generic;

namespace VendingMachine {
    public class Drink : IProduct {
        private readonly double servings;
        private double remainingServings;
        public bool StillUsable { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public double RecommendedRetailPrice { get; private set; }

        public string Use() {
            string useDescription = "";
            if (!StillUsable) {
                throw new ProductUseException("It's empty!");
            } else if (remainingServings == 1) {
                useDescription = "You drink the rest of the {Name}.";
            } else if (remainingServings > 0 && remainingServings < 1) {
                useDescription = "You drink the very last sip of the {Name}.";
            } else {
                useDescription = "You drink from the {Name}.";
            }
            StillUsable = --remainingServings > 0;
            return useDescription;
        }

        public IEnumerable<string> UseItAll() {
            List<string> useDescriptions = new List<string>();
            do {
                useDescriptions.Add(Use());
            } while (StillUsable);
            return useDescriptions;
        }

        public Drink(string name, string description, double recommendedRetailPrice, double servings) {
            this.Name = name;
            this.Description = description;
            this.RecommendedRetailPrice = recommendedRetailPrice;
            this.servings = servings;
            remainingServings = servings;
            StillUsable = remainingServings > 0;
        }
    }
}
