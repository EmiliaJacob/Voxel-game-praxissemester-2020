using System.Collections.Generic;

namespace NewWorldVoxel
{
    public static class Prices //TODO: abklären wo und wie Attribute initialisiert werden
    {
        private static float _saleLoss;

        public static Dictionary<string, int> Purchase = new Dictionary<string, int>();
        public static Dictionary<string, int> Sale = new Dictionary<string, int>();
        public static List<string> PurchaseList { get; private set; }

        public static void SetPrices(int land, int food, int oil, int metal, int crystal, int production, float saleLoss)
        {
            _saleLoss = saleLoss;

            Purchase["land"] = land;
            Purchase["food"] = food;
            Purchase["oil"] = oil;
            Purchase["metal"] = metal;
            Purchase["crystal"] = crystal;
            Purchase["production"] = production;

            UpdateSales();

            PurchaseList = new List<string>();
            foreach (var key in Purchase.Keys)
                PurchaseList.Add(key);
        }

        public static void IncreaseMarketPrices()
        {
            foreach(var key in PurchaseList)
            {
                Purchase[key] = (int)(Purchase[key] * 1.25);
                Sale[key] = (int)(Purchase[key] * _saleLoss);
            }
        }

        public static void AddDiscount()
        {
            Purchase["land"] -= (int)(Purchase["land"] * 0.15);
            Purchase["food"] -= (int)(Purchase["food"] * 0.15);
            Purchase["oil"] -= (int)(Purchase["oil"] * 0.15);
            Purchase["metal"] -= (int)(Purchase["metal"] * 0.15);
            Purchase["crystal"] -= (int)(Purchase["crystal"] * 0.15);
            Purchase["production"] -= (int)(Purchase["production"] * 0.15);

            UpdateSales();
        }

        public static void RemoveDiscount()
        {
            Purchase["land"] += (int)(Purchase["land"] * 0.15);
            Purchase["food"] += (int)(Purchase["food"] * 0.15);
            Purchase["oil"] += (int)(Purchase["oil"] * 0.15);
            Purchase["metal"] += (int)(Purchase["metal"] * 0.15);
            Purchase["crystal"] += (int)(Purchase["crystal"] * 0.15);
            Purchase["production"] += (int)(Purchase["production"] * 0.15);

            UpdateSales();
        }

        private static void UpdateSales()
        {
            Sale["land"] = (int)(Purchase["land"] * _saleLoss);
            Sale["food"] = (int)(Purchase["food"] * _saleLoss);
            Sale["oil"] = (int)(Purchase["oil"] * _saleLoss);
            Sale["metal"] = (int)(Purchase["metal"] * _saleLoss);
            Sale["crystal"] = (int)(Purchase["crystal"] * _saleLoss);
            Sale["production"] = (int)(Purchase["production"] * _saleLoss);

        }
    }
}
