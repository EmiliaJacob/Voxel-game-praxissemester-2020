using System.Numerics;
using VoxelEngine.UI;

namespace NewWorldVoxel
{
    class MarketUI
    {
        private static bool _showBuyMenu = false;
        private static bool _showAmountToBuyMenu = false;
        private static bool _showAmountToSellMenu = false;
        private static bool _showSellMenu = false;
        private static string _chosenWareName = "none";
        private static int _amountCounter = 0;
        
        public static void DrawUI(Game game, Player player, Vector3Int radius)
        {
            game.UIBuilder.Begin("Market Menu", UIWindowFlags.AlwaysAutoResize)
                .Text(true, "Willkommen im Markt")
                .Button("Waren einkaufen", true)
                .Button("Waren verkaufen", true)
                .Button("Zurueck", true)
                .End();

            if ((bool) game.UIBuilder.ElementValues["Zurueck"])
                Game.MarketUiDraw = false;

            BuyMenu(game);
            SellMenu(game);
            AmountToBuyMenu(game, player);
            AmountToSellMenu(game, player);
        }

        private static void BuyMenu(Game game)
        {
            if ((bool)game.UIBuilder.ElementValues["Waren einkaufen"])
                _showBuyMenu = true;

            if (!_showBuyMenu) return;

            game.UIBuilder.Begin("Waren einkaufen", UIWindowFlags.AlwaysAutoResize)
                .Text(true, "Waehle gewuenschte Waren aus")
                .Button("Nahrung", true)
                .Button("Oel", true)
                .Button("Metall", true)
                .Button("Kristall", true)
                .Button("Zurueck", true)
                .End();

            if ((bool)game.UIBuilder.ElementValues["Zurueck"])
                _showBuyMenu = false;
        }

        private static void AmountToBuyMenu(Game game, Player player)
        {
            if (!_showBuyMenu) return;

            if ((bool)game.UIBuilder.ElementValues["Nahrung"])
                SelectWareToBuy("Nahrung");

            if ((bool)game.UIBuilder.ElementValues["Oel"])
                SelectWareToBuy("Oel");

            if ((bool)game.UIBuilder.ElementValues["Metall"])
                SelectWareToBuy("Metall");

            if ((bool)game.UIBuilder.ElementValues["Kristall"])
                SelectWareToBuy("Kristall");

            if (!_showAmountToBuyMenu) return;

            game.UIBuilder.Begin("Menge auswaehlen", UIWindowFlags.AlwaysAutoResize)
                .Text(true, $"Wie viel {_chosenWareName} moechtest du kaufen?")
                .Button("+", true)
                .Button("-", true)
                .Text(true, $"Anzahl: {_amountCounter.ToString()}")
                .Button("Bestaetigen", true)
                .Button("Zurueck", true)
                .End();

            if ((bool)game.UIBuilder.ElementValues["+"])
                _amountCounter += 1;

            if ((bool)game.UIBuilder.ElementValues["-"])
            {
                if (_amountCounter > 0)
                    _amountCounter -= 1;
            }

            if ((bool)game.UIBuilder.ElementValues["Bestaetigen"])
            {
                Transactions.BuyWare(player, ReturnEnglishName(_chosenWareName), _amountCounter);
                _showAmountToBuyMenu = false;
            }

            if ((bool)game.UIBuilder.ElementValues["Zurueck"])
                _showAmountToBuyMenu = false;
        }

        private static void SelectWareToBuy(string wareName)
        {
            _chosenWareName = wareName;
            _amountCounter = 0;
            _showAmountToBuyMenu = true;
        }
        
        private static void SellMenu(Game game)
        {
            if ((bool)game.UIBuilder.ElementValues["Waren verkaufen"])
                _showSellMenu = true;

            if (!_showSellMenu) return;

            game.UIBuilder.Begin("Waren verkaufen", UIWindowFlags.AlwaysAutoResize)
                .Text(true, "Waehle gewuenschte Waren aus")
                .Button("NahrungS", true)
                .Button("Oel", true)
                .Button("Metall", true)
                .Button("Kristall", true)
                .Button("Zurueck", true)
                .End();

            if ((bool)game.UIBuilder.ElementValues["Zurueck"])
                _showSellMenu = false;
        }

        private static void AmountToSellMenu(Game game, Player player)
        {
            if (!_showSellMenu) return;

            if ((bool)game.UIBuilder.ElementValues["Nahrung"])
                SelectWareToSell("Nahrung");

            if ((bool)game.UIBuilder.ElementValues["Oel"])
                SelectWareToSell("Oel");

            if ((bool)game.UIBuilder.ElementValues["Metall"])
                SelectWareToSell("Metall");

            if ((bool)game.UIBuilder.ElementValues["Kristall"])
                SelectWareToSell("Kristall");

            if (!_showAmountToSellMenu) return;

            game.UIBuilder.Begin("Menge auswaehlen", UIWindowFlags.AlwaysAutoResize)
                .Text(true, $"Wie viel {_chosenWareName} moechtest du verkaufen?")
                .Button("+", true)
                .Button("-", true)
                .Text(true, $"Anzahl: {_amountCounter.ToString()}")
                .Button("Bestaetigen", true)
                .Button("Zurueck", true)
                .End();

            if ((bool)game.UIBuilder.ElementValues["+"])
                _amountCounter += 1;

            if ((bool)game.UIBuilder.ElementValues["-"])
                _amountCounter -= 1;

            if ((bool)game.UIBuilder.ElementValues["Bestaetigen"])
            {
                Transactions.SellWare(player, ReturnEnglishName(_chosenWareName), _amountCounter);
                _showAmountToSellMenu = false;
            }

            if ((bool)game.UIBuilder.ElementValues["Zurueck"])
                _showAmountToSellMenu = false;
        }

        private static void SelectWareToSell(string wareName)
        {
            _chosenWareName = wareName;
            _amountCounter = 0;
            _showAmountToSellMenu = true;
        }

        private static string ReturnEnglishName(string germanName)
        {
            switch (germanName)
            {
                case "Nahrung":
                    return "food";
                case "Oel":
                    return "oil";
                case "Metall":
                    return "metal";
                case "Kristall":
                    return "crystal";
            }

            return "Not Found";
        }
    }
}


