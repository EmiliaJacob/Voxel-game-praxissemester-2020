using System;
using System.Collections.Generic;

namespace NewWorldVoxel
{
    class NetworkTransactions
    {
        private static NetworkTransactions networkTransactions_;

        private static Game game_ = Game.GetInstance();

        private static Queue<(string markerModel, string frameModel)> availableColors_ = new Queue<(string, string)>();

        private static Dictionary<long, (string markerModel, string frameModel)> modelNamesByIp_ = new Dictionary<long, (string markerModel, string frameModel)>();

        public static bool CustomSettings = false;

        public static Dictionary<string, string> PlayerRanking = new Dictionary<string, string>();

        private enum NetworkChannels
        {
            moveMarker = 1,
            buildCity = 2,
            destroyCity = 3,
            buildProduction = 4,
            destroyProduction = 5,
            pausePlayGame = 6,
            buildResearch = 7,
            destroyResearch = 8,
            buildMarketHall = 9,
            destroyMarketHall = 10,
            sellLandWithMachine = 11,
            sellLandWithResearch = 12,
            sellLandWithMarketHall = 13,
            otherClientDisconnected = 14,
            clientReceivedDisconMsg = 15,
            endOfGame = 28,
            gamePreparations = 30,
        };

        public NetworkTransactions()
        {
            ResetColorQueue();
        }

        public static NetworkTransactions GetInstance()
        {
            if (networkTransactions_ != null)
                return networkTransactions_;
            else
            {
                networkTransactions_ = new NetworkTransactions();
                return networkTransactions_;
            }
        }

        public void StartMultiplayerGame(string settings)
        {
            if (settings == "custom")
                UiCollection.SetSettings(game_.ActivePlayer);
            else
                game_.SetDefaultSettings();

            game_.LoadNewWorld();

            foreach (var client in Network.GetInstance().GetClients().Keys)
                Objects.GetRandomStartFields(client);

            foreach (var client in Objects.ClientStartFields.Keys)
            {
                var (xPos, zPos) = Objects.ClientStartFields[client];

                if (client == Network.GetInstance().HostName)
                {
                    Marker.XCoordinate = Objects.XStart = xPos;
                    Marker.ZCoordinate = Objects.ZStart = zPos;
                }

                Network.GetInstance().SendMessage($"startFields {client} {xPos} {zPos}", (int)NetworkChannels.gamePreparations);
                Objects.CreateObjectFromAnimationSet(xPos, zPos, "start", "Start");
            }

            if (settings == "custom")
            {
                IDictionary<string, object> settingsDict = game_.UIBuilder.ElementValues;
                int chunkSize;

                if (Network.GetInstance().GetClients().Count <= 4)
                    chunkSize = 20;
                else
                    chunkSize = 40;

                Network.GetInstance().SendMessage($"settings " +
                    $"{chunkSize} " +
                    $"{settingsDict["Rundenzeit"]} " +
                    $"{settingsDict["Finalrunde"]} " +
                    $"{settingsDict["Preiserhoehung"]} " +
                    $"{settingsDict["Geld"]} " +
                    $"{settingsDict["Bevoelkerung"]} " +
                    $"{settingsDict["Wohnungen"]} " +
                    $"{settingsDict["Nahrung"]} " +
                    $"{settingsDict["Oel"]} " +
                    $"{settingsDict["Metall"]} " +
                    $"{settingsDict["Kristall"]} " +
                    $"{settingsDict["landPreis"]} " +
                    $"{settingsDict["nahrungsPreis"]} " +
                    $"{settingsDict["oelPreis"]} " +
                    $"{settingsDict["metallPreis"]} " +
                    $"{settingsDict["kristallPreis"]} " +
                    $"{settingsDict["bauKosten"]} " +
                    $"{settingsDict["Verkaufspreis"]}"
                    , 30);
            }

            Network.GetInstance().SendMessage($"seed {game_.GameSeed}", (int)NetworkChannels.gamePreparations);
            game_.StartGame(game_.GameSeed);
        }

        public void StartGameClientSide()
        {
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.gamePreparations).Count > 0)
            {
                string message = Network.GetInstance().PopFirstMessage(30);
                string[] split = message.Split(' ');

                if (split[0] == "startFields")
                    GetStartFields(split);
                else if (split[0] == "settings")
                    GetHostSettings(split);
                else if (split[0] == "seed")
                    GetHostSeed(split);
            }
        }

        private void GetStartFields(string[] split)
        {
            if (split[1] == Network.GetInstance().UserName)
            {
                Marker.XCoordinate = Objects.XStart = int.Parse(split[2]);
                Marker.ZCoordinate = Objects.ZStart = int.Parse(split[3]);
            }

            Objects.StartFields.Add((int.Parse(split[2]), int.Parse(split[3])));
            Objects.CreateObjectFromAnimationSet(int.Parse(split[2]), int.Parse(split[3]), "start", "Start");
        }

        private void GetHostSettings(string[] split)
        {
            CustomSettings = true;
            game_.SetChunkSize(int.Parse(split[1]));

            RoundTimer.GetInstance().SetupTimer(int.Parse(split[2]) > (int)game_.DefaultSettings["gameRoundTime"] ? int.Parse(split[2]) : (int)game_.DefaultSettings["gameRoundTime"]);
            RoundTimer.GetInstance().SetupTimer(int.Parse(split[2]) > (int)game_.DefaultSettings["gameRoundTime"] ? int.Parse(split[2]) : (int)game_.DefaultSettings["gameRoundTime"]);
            RoundTimer.GetInstance().WinningRound = int.Parse(split[3]) > (int)game_.DefaultSettings["winningRound"] ? int.Parse(split[3]) : (int)game_.DefaultSettings["winningRound"];
            RoundTimer.GetInstance().MarketPriceIncreaseRound = int.Parse(split[4]);

            int money = int.Parse(split[5]) > (int)game_.DefaultSettings["money"] ? int.Parse(split[5]) : (int)game_.DefaultSettings["money"];
            int citizens = int.Parse(split[6]) > (int)game_.DefaultSettings["citizens"] ? int.Parse(split[6]) : (int)game_.DefaultSettings["citizens"];
            int housings = int.Parse(split[7]) > (int)game_.DefaultSettings["initialHousings"] ? int.Parse(split[7]) : (int)game_.DefaultSettings["initialHousings"];
            int food = int.Parse(split[8]) > (int)game_.DefaultSettings["food"] ? int.Parse(split[8]) : (int)game_.DefaultSettings["food"];
            int oil = int.Parse(split[9]) > (int)game_.DefaultSettings["oil"] ? int.Parse(split[9]) : (int)game_.DefaultSettings["oil"];
            int metal = int.Parse(split[10]) > (int)game_.DefaultSettings["metal"] ? int.Parse(split[10]) : (int)game_.DefaultSettings["metal"];
            int crystal = int.Parse(split[11]) > (int)game_.DefaultSettings["crystal"] ? int.Parse(split[11]) : (int)game_.DefaultSettings["crystal"];

            int landPrice = int.Parse(split[12]) > (int)game_.DefaultSettings["landPrice"] ? int.Parse(split[12]) : (int)game_.DefaultSettings["landPrice"];
            int foodPrice = int.Parse(split[13]) > (int)game_.DefaultSettings["foodPrice"] ? int.Parse(split[13]) : (int)game_.DefaultSettings["foodPrice"];
            int oilPrice = int.Parse(split[14]) > (int)game_.DefaultSettings["oilPrice"] ? int.Parse(split[14]) : (int)game_.DefaultSettings["oilPrice"];
            int metalPrice = int.Parse(split[15]) > (int)game_.DefaultSettings["metalPrice"] ? int.Parse(split[15]) : (int)game_.DefaultSettings["metalPrice"];

            int crystalPrice = int.Parse(split[16]) > (int)game_.DefaultSettings["crystalPrice"] ? int.Parse(split[16]) : (int)game_.DefaultSettings["crystalPrice"];
            int buildCost = int.Parse(split[17]) > (int)game_.DefaultSettings["buildCost"] ? int.Parse(split[17]) : (int)game_.DefaultSettings["buildCost"];
            float saleLoss = float.Parse(split[18]) != (float)game_.DefaultSettings["saleLoss"] ? float.Parse(split[18]) : (float)game_.DefaultSettings["saleLoss"];

            game_.ActivePlayer.SetPlayerResources(money, citizens, housings, food, oil, metal, crystal);
            Prices.SetPrices(landPrice, foodPrice, oilPrice, metalPrice, crystalPrice, buildCost, saleLoss);
        }

        private void GetHostSeed(string[] split)
        {
            if (!Network.GetInstance().IsServer)
            {
                game_.MultiplayerRunning = true;

                if (CustomSettings == false)
                    game_.SetDefaultSettings();

                game_.LoadNewWorld();
            }

            game_.StartGame(int.Parse(split[1]));
        }

        public void ResetColorQueue()
        {
            availableColors_.Clear();
            availableColors_.Enqueue(("BlueMarker", "BlueFrame"));
            availableColors_.Enqueue(("PinkMarker", "PinkFrame"));
            availableColors_.Enqueue(("YellowMarker", "YellowFrame"));
            availableColors_.Enqueue(("GreenMarker", "GreenFrame"));
            availableColors_.Enqueue(("OrangeMarker", "OrangeFrame"));
        }

        public void GameRunning()
        {
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.moveMarker).Count > 0)
                MoveMarker();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.buildCity).Count > 0)
                BuildCity();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.destroyCity).Count > 0)
                DestroyCity();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.buildProduction).Count > 0)
                BuildProduction();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.destroyProduction).Count > 0)
                DestroyProduction();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.pausePlayGame).Count > 0)
                GamePausePlay();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.buildResearch).Count > 0)
                BuildResearch();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.destroyResearch).Count > 0)
                DestroyResearch();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.buildMarketHall).Count > 0)
                BuildMarketHall();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.destroyMarketHall).Count > 0)
                DestroyMarketHall();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.sellLandWithMachine).Count > 0)
                SellLandWithMachine();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.sellLandWithResearch).Count > 0)
                SellLandWithResearch();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.sellLandWithMarketHall).Count > 0)
                SellLandWithMarketHall();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.otherClientDisconnected).Count > 0)
                PeerDisconnected();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.clientReceivedDisconMsg).Count > 0)
                Network.GetInstance().Disconnect();
            if (Network.GetInstance().GetChannelBuffer((int)NetworkChannels.endOfGame).Count > 0 && UiCollection.EndOfGameActive)
                EndOfGame();
        }

        private void PeerDisconnected()
        {
            string message = Network.GetInstance().PopFirstMessage(14); //TODO: Send entfernen
            Console.WriteLine("hi1");
            if (message == "host")
            {
                Console.WriteLine("hi2");
                UiCollection.ServerDisconnected = true;
            }
            else
                UiCollection.ClientIsDisconnected(message);

            Network.GetInstance().SendMessage("Recieved Disconnection Status",15);

            if (Network.GetInstance().IsServer && Network.GetInstance().GetClients().Count < 2)
                Network.GetInstance().ConnectionEstablished = false;
        }

        private void MoveMarker()
        {
            string message = Network.GetInstance().PopFirstMessage(1);
            long playerID = long.Parse(message.Split(' ')[0]);
            int x = int.Parse(message.Split(' ')[1]);
            int z = int.Parse(message.Split(' ')[2]);

            if (modelNamesByIp_.ContainsKey(playerID) == false)
            {
                (string marker, string frame) assignedColour = availableColors_.Dequeue();
                modelNamesByIp_.Add(playerID, assignedColour);

                // Setzt StartFelder von anderen Spielerx
                string frameModelMarker = modelNamesByIp_[playerID].frameModel;
                Objects.CreateObjectFromAnimationSet(x, z, $"start{playerID} x: {x} z: {z}", "Start");
                Objects.CreateObjectFromAnimationSet(x, z, frameModelMarker, frameModelMarker);
            }

            string markerModel = modelNamesByIp_[playerID].markerModel;
            Objects.DeleteOldMarker(markerModel);
            Objects.CreateObjectFromAnimationSet(x, z, markerModel, markerModel);
        }

        private void BuildCity()
        {
            string message = Network.GetInstance().PopFirstMessage(2);
            long playerID = long.Parse(message.Split(' ')[0]);
            int x = int.Parse(message.Split(' ')[1]);
            int z = int.Parse(message.Split(' ')[2]);

            if (Objects.CheckIfObjectNameExists("buyable", x, z))
                Objects.DeleteObject($"buyable x: {x} z: {z}");

            Transactions.CityConstruction(x, z);

            string frameName = modelNamesByIp_[playerID].frameModel;
            Objects.CreateObjectFromAnimationSet(x, z, frameName, frameName);
        }

        private void DestroyCity()
        {
            string message = Network.GetInstance().PopFirstMessage(3);
            long playerID = long.Parse(message.Split(' ')[0]);
            int x = int.Parse(message.Split(' ')[1]);
            int z = int.Parse(message.Split(' ')[2]);

            Objects.ChangeAnimation($"city x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"city x: {x} z: {z}");

            string frameName = modelNamesByIp_[playerID].frameModel;
            Objects.DeleteObject($"{frameName} x: {x} z: {z}");
        }

        private void BuildProduction()
        {
            string message = Network.GetInstance().PopFirstMessage(4);
            int x = int.Parse(message.Split(' ')[1]);
            int z = int.Parse(message.Split(' ')[2]);
            string modelName = message.Split(' ')[3];
            string machineType = message.Split(' ')[4];

            Objects.DeleteObject($"city x: {x} z: {z}");
            Objects.CreateObjectFromAnimationSet(x, z, machineType, modelName);
            Objects.ChangeAnimation($"{machineType} x: {x} z: {z}", "Construction", false);
        }

        private void DestroyProduction()
        {
            string message = Network.GetInstance().PopFirstMessage(5);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);
            string machineType = message.Split(' ')[2];

            Objects.DeleteObject($"{machineType} x: {x} z: {z}");
            Transactions.CityConstruction(x, z);
        }

        private void BuildResearch()
        {
            string message = Network.GetInstance().PopFirstMessage(7);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);
            string model = message.Split(' ')[2];

            Objects.DeleteObject($"city x: {x} z: {z}");
            Objects.CreateObjectFromAnimationSet(x, z, "research", model);
            Objects.ChangeAnimation($"research x: {x} z: {z}", "Construction", false);
        }

        private void DestroyResearch()
        {
            string message = Network.GetInstance().PopFirstMessage(8);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);

            Objects.DeleteObject($"research x: {x} z: {z}");
            Transactions.CityConstruction(x, z);
        }

        private void BuildMarketHall()
        {
            string message = Network.GetInstance().PopFirstMessage(9);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);

            Objects.DeleteObject($"city x: {x} z: {z}");
            Objects.CreateObjectFromAnimationSet(x, z, "marketHall", "MarketHall");
            Objects.ChangeAnimation($"marketHall x: {x} z: {z}", "Construction", false);
            Objects.SetAnimationTempo($"marketHall x: {x} z: {z}", "Idle", 1000);
        }

        private void DestroyMarketHall()
        {
            string message = Network.GetInstance().PopFirstMessage(10);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);

            Objects.DeleteObject($"marketHall x: {x} z: {z}");
            Transactions.CityConstruction(x, z);
        }

        private void SellLandWithMarketHall()
        {
            string message = Network.GetInstance().PopFirstMessage(13);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);

            Objects.ChangeAnimation($"marketHall x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"marketHall x: {x} z: {z}");
            Objects.DeleteObject($"frame x: {x} z: {z}");
        }

        private void SellLandWithResearch()
        {
            string message = Network.GetInstance().PopFirstMessage(12);
            int x = int.Parse(message.Split(' ')[0]);
            int z = int.Parse(message.Split(' ')[1]);

            Objects.ChangeAnimation($"research x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"research x: {x} z: {z}");
            Objects.DeleteObject($"frame x: {x} z: {z}");
        }

        private void SellLandWithMachine()
        {
            string message = Network.GetInstance().PopFirstMessage(11);
            string machineType = message.Split(' ')[0];
            int x = int.Parse(message.Split(' ')[1]);
            int z = int.Parse(message.Split(' ')[2]);

            Objects.ChangeAnimation($"{machineType} x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"{machineType} x: {x} z: {z}");
            Objects.DeleteObject($"frame x: {x} z: {z}");
        }

        private void GamePausePlay()
        {
            string message = Network.GetInstance().PopFirstMessage(6);
            string playPause = message.Split(' ')[0];
            int inGameTime = int.Parse(message.Split(' ')[1]);

            if (playPause == "pause")
            {
                RoundTimer.GetInstance().Stop();
                RoundTimer.GetInstance().Set(inGameTime - 1);
                UiCollection.PauseMenu = true;

                if (Network.GetInstance().IsServer)
                    Network.GetInstance().SendMessage($"Servertime {UiCollection.ServerTime}", 6);
            }

            if (playPause == "Servertime" && !Network.GetInstance().IsServer)
                UiCollection.ServerTime = inGameTime;

            if (playPause == "play")
            {
                RoundTimer.GetInstance().Start();
                UiCollection.PauseMenu = false;
            }
        }

        private void EndOfGame()
        {
            string message = Network.GetInstance().PopFirstMessage((int)NetworkChannels.endOfGame);

            PlayerRanking.Add(message.Split(' ')[0], message.Split(' ')[1]);

            foreach (var player in PlayerRanking)
                Console.WriteLine(player);
        }
    }
}


