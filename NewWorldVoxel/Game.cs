using System;
using System.Collections.Generic;
using System.Numerics;
using VoxelEngine;
using VoxelEngine.UI;

namespace NewWorldVoxel
{
    class Game : Engine
    {
        private static Game _game;

        public Game(EngineCreateInfo createInfo) : base(createInfo) { }
        public CameraAnchor PlayerViewPoint;   

        public int XChunks { get; set; }
        public int YChunks { get; set; }
        public int ZChunks { get; set; }

        public delegate void OnUpdate(Object sender, EventArgs e);
        public event OnUpdate GameUpdates;

        public int GameSeed;
        public bool GameIsActive;
        public bool MultiplayerRunning;
        
        public Vector3 Radius;
        public Player ActivePlayer;
        public Dictionary<string, Object> DefaultSettings;

        protected override void Configurations()
        {
            DefaultRenderer defaultRenderer = new DefaultRenderer();
            RenderBase = defaultRenderer;
            defaultRenderer.EnableColorTransparency();
            defaultRenderer.EnableBloom();
        }

        protected override void Init()
        {
            PlayerViewPoint = new CameraAnchor();
            GameUpdates = new OnUpdate(Objects.DeleteObjectAfterDestruction);

            GameSeed = (int)DateTime.Now.Ticks;
            GameIsActive = false;
            MultiplayerRunning = false;

            Radius = new Vector3(32, 16, 32);
            ActivePlayer = new Player();
            DefaultSettings = new Dictionary<string, Object>()
            {
                { "gameRoundTime", 15 },
                { "winningRound", 30 },
                { "marketPriceIncreaseRound", 10 },
                { "money", 2000 }, // Pretzel
                { "citizens", 100 },
                { "initialHousings", 205 },
                { "food", 50 },
                { "oil", 20 },
                { "metal", 20 },
                { "crystal", 0 },
                { "landPrice", 50 },
                { "foodPrice", 25 },
                { "oilPrice", 30 },
                { "metalPrice", 45 },
                { "crystalPrice", 60 },
                { "buildCost", 5 },
                { "saleLoss", 0.75f }
            };

            LoadAnimationSets();
            LoadUiVoxImages();
            LoadVoxelColorPalette();
            LoadSun();
            LoadCamera();
            LoadSettingWorld();
            NetworkTransactions.GetInstance().ResetColorQueue();
        }

        protected override void Draw()
        {
            DrawSun();
            DrawConsoleOutputs();

            if (!GameIsActive)
            {
                UiCollection.DrawGameStartMenu();
                UiCollection.DrawSingleplayerMenu();
                UiCollection.DrawSettings();
                UiCollection.DrawMultiplayerMenu();
                UiCollection.DrawCreateLobby();
                UiCollection.DrawJoinLobby();
                UiCollection.DrawLobby();
                UiCollection.UpdateOpenHosts();
            }
            else
            {
                UiCollection.DrawMainMenuBar();
                UiCollection.DrawMachineMenuBar();

                UiCollection.DrawHelp();
                UiCollection.DrawCameraHelp();
                UiCollection.DrawGameHelp();
                UiCollection.DrawInterfaceHelp();
                UiCollection.DrawResearchHelp();

                UiCollection.DrawResearch();
                UiCollection.DrawLand();
                UiCollection.DrawMachineAssign();
                UiCollection.DrawGameError();

                UiCollection.DrawMarket();
                UiCollection.DrawMarketBuy();
                UiCollection.DrawMarketBuyResource();
                UiCollection.DrawMarketSell();
                UiCollection.DrawMarketSellResource();
                UiCollection.DrawSellCheck();
                UiCollection.DrawEndOfGame();
                UiCollection.DrawPauseMenu();
                UiCollection.DrawClientDisconnected();
                UiCollection.DrawServerDisconnected();
            }

            Objects.RotateObjects();
        }

        protected override void Update()
        {
            Network.GetInstance().ReceiveMessages();

            if (!GameIsActive)
            {
                KeyboardControls.GameStartMenu();
                KeyboardControls.SingleplayerMenu();
                KeyboardControls.Settings();
                KeyboardControls.MultiplayerMenu();

                NetworkTransactions.GetInstance().StartGameClientSide();
            }
            else
            {
                KeyboardControls.Camera();
                KeyboardControls.MoveMarker();
                KeyboardControls.MainControls();

                KeyboardControls.LandMenu();
                KeyboardControls.AssignMachine();
                KeyboardControls.ResearchMenu();

                KeyboardControls.MarketMenu();
                KeyboardControls.MarketBuy();
                KeyboardControls.MarketBuyResource();
                KeyboardControls.MarketSell();
                KeyboardControls.MarketSellResource();
                KeyboardControls.SellCheck();
                KeyboardControls.DrawError();

                NetworkTransactions.GetInstance().GameRunning();

                GameUpdates.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void Shutdown()
        {

        }

        public static Game GetInstance()
        {
            if (_game != null)
                return _game;
            else
            {
                _game = new Game(new EngineCreateInfo("NewVoxelWorld", 720, 512, DefaultFrameTime));
                return _game;
            }
        }

        public void SetDefaultSettings()
        {
            SetChunkSize(20);
            RoundTimer.GetInstance().SetupTimer((int)DefaultSettings["gameRoundTime"]);
            RoundTimer.GetInstance().WinningRound = (int)DefaultSettings["winningRound"];
            RoundTimer.GetInstance().MarketPriceIncreaseRound = (int)DefaultSettings["marketPriceIncreaseRound"];

            //Default values for game
            ActivePlayer.SetPlayerResources(
                (int)DefaultSettings["money"],
                (int)DefaultSettings["citizens"],
                (int)DefaultSettings["initialHousings"],
                (int)DefaultSettings["food"],
                (int)DefaultSettings["oil"],
                (int)DefaultSettings["metal"],
                (int)DefaultSettings["crystal"]
                );

            //For Testing Activate this!!
            //ActivePlayer.SetPlayerResources(10000, 10000, 10000, 10000, 10000, 10000, 10000);

            Prices.SetPrices(
                (int)DefaultSettings["landPrice"],
                (int)DefaultSettings["foodPrice"],
                (int)DefaultSettings["oilPrice"],
                (int)DefaultSettings["metalPrice"],
                (int)DefaultSettings["crystalPrice"],
                (int)DefaultSettings["buildCost"],
                (float)DefaultSettings["saleLoss"]
                );
        }

        public void SetChunkSize(int chunks)
        {
            XChunks = chunks;
            YChunks = 1;
            ZChunks = chunks;
        }

        private void LoadCamera()
        {
            CameraControl.CreateAndAddTrackingCamera("playerCamera", PlayerViewPoint, new Vector3(0, 0, 0), new Vector3(40, 0, 0));
            CameraControl.SelectActiveCamera("playerCamera");
        }

        public void BackToMainMenu()
        {
            if (MultiplayerRunning)
            {
                MultiplayerRunning = false;
                //Network.GetInstance().Disconnect();
                Objects.StartFields.Clear();
                Objects.ClientStartFields.Clear();

                if (Network.GetInstance().IsServer)
                    NetworkTransactions.PlayerRanking.Clear();

                NetworkTransactions.GetInstance().ResetColorQueue();
            }

            Objects.RotatingObjects.Clear();

            foreach (var item in ObjectControl.ObjectNames)
                Objects.DeleteObject(item);

            WorldControl.RemoveWorld("newWorld"); // TODO: in GameWorld renamen

            LoadSettingWorld();
            KeyboardControls.GameMenuActive = true;
        }

        public void LoadSettingWorld() // tODO: umbenennen
        {
            GameIsActive = false;
            WorldControl.AddWorld("settingWorld", new Vector3(2, 2, 2));
            WorldControl.SetRenderWorld("settingWorld");

            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    for (int z = 0; z < 2; z++)
                        WorldControl.GetWorld("settingWorld").AddNewChunk(new Vector3(x, y, z));

            LoadSettingsObjects();
        }

        public void LoadNewWorld()
        {
            WorldControl.AddWorld("newWorld", new Vector3(XChunks, YChunks, ZChunks));
            WorldControl.SetRenderWorld("newWorld");
            WorldControl.RemoveWorld("settingWorld");

            for (int x = 0; x < XChunks; x++)
                for (int y = 0; y < YChunks; y++)
                    for (int z = 0; z < ZChunks; z++)
                        WorldControl.GetWorld("newWorld").AddNewChunk(new Vector3(x, y, z));

            ObjectControl.DeleteObject("World");
            Objects.RotatingObjects.Remove(("World", 3));
        }

        private void LoadAnimationSets()
        {
            ModelControl.LoadAnimationSet("Start", "StartField");
            ModelControl.LoadAnimationSet("City", "City");

            ModelControl.LoadAnimationSet("FoodMachine", "Machines/FoodMachine");
            ModelControl.LoadAnimationSet("OilMachine", "Machines/OilMachine");
            ModelControl.LoadAnimationSet("MetalMachine", "Machines/MetalMachine");
            ModelControl.LoadAnimationSet("CrystalMachine", "Machines/CrystalMachine");

            ModelControl.LoadAnimationSet("RegPlain0", "Plains/RegPlain0");
            ModelControl.LoadAnimationSet("RegPlain1", "Plains/RegPlain1");
            ModelControl.LoadAnimationSet("RegPlain2", "Plains/RegPlain2");
            ModelControl.LoadAnimationSet("RegPlain3", "Plains/RegPlain3");
            ModelControl.LoadAnimationSet("RegPlain4", "Plains/RegPlain4");
            ModelControl.LoadAnimationSet("FoodPlain", "Plains/FoodPlain");
            ModelControl.LoadAnimationSet("OilPlain", "Plains/OilPlain");
            ModelControl.LoadAnimationSet("MetalPlain", "Plains/MetalPlain");
            ModelControl.LoadAnimationSet("CrystalPlain", "Plains/CrystalPlain");
            ModelControl.LoadAnimationSet("FoodEmpty", "Plains/FoodEmpty");
            ModelControl.LoadAnimationSet("OilEmpty", "Plains/OilEmpty");
            ModelControl.LoadAnimationSet("MetalEmpty", "Plains/MetalEmpty");
            ModelControl.LoadAnimationSet("CrystalEmpty", "Plains/CrystalEmpty");

            ModelControl.LoadAnimationSet("Research1", "Research/Research1");
            ModelControl.LoadAnimationSet("Research2", "Research/Research2");
            ModelControl.LoadAnimationSet("Research0", "Research/Research0");

            ModelControl.LoadAnimationSet("MarketHall", "Market");

            ModelControl.LoadAnimationSet("Unusable0", "UnusableFields/Unusable0");
            ModelControl.LoadAnimationSet("Unusable1", "UnusableFields/Unusable1");
            ModelControl.LoadAnimationSet("Unusable2", "UnusableFields/Unusable2");
            ModelControl.LoadAnimationSet("VortexBase", "UnusableFields/VortexBase");
            ModelControl.LoadAnimationSet("VortexWater", "UnusableFields/VortexWater");

            ModelControl.LoadAnimationSet("Buyable", "Markers/Buyable");
            ModelControl.LoadAnimationSet("Marker", "Markers/Marker");

            ModelControl.LoadAnimationSet("RedMarker", "Markers/RedMarker");
            ModelControl.LoadAnimationSet("BlueMarker", "Markers/BlueMarker");
            ModelControl.LoadAnimationSet("PinkMarker", "Markers/PinkMarker");
            ModelControl.LoadAnimationSet("GreenMarker", "Markers/GreenMarker");
            ModelControl.LoadAnimationSet("OrangeMarker", "Markers/OrangeMarker");
            ModelControl.LoadAnimationSet("FMarkerOne", "Markers/FMarkerOne");

            ModelControl.LoadAnimationSet("BlueFrame", "MpFrames/BlueFrame");
            ModelControl.LoadAnimationSet("YellowFrame", "MpFrames/YellowFrame");
            ModelControl.LoadAnimationSet("PinkFrame", "MpFrames/PinkFrame");
            ModelControl.LoadAnimationSet("OrangeFrame", "MpFrames/OrangeFrame");
            ModelControl.LoadAnimationSet("GreenFrame", "MpFrames/GreenFrame");

            ModelControl.LoadAnimationSet("World", "World");
        }

        private void LoadUiVoxImages()
        {
            UiCollection.UnemployedImageID = UIBuilder.LoadTexture("UiSymbols/unemployed.jpg");
            UiCollection.PopsImageID = UIBuilder.LoadTexture("UiSymbols/pops.jpg");
            UiCollection.HousingsImageID = UIBuilder.LoadTexture("UiSymbols/housings.jpg");
            UiCollection.MoneyImageID = UIBuilder.LoadTexture("UiSymbols/money.jpg");
            UiCollection.FoodImageID = UIBuilder.LoadTexture("UiSymbols/food.jpg");
            UiCollection.OilImageID = UIBuilder.LoadTexture("UiSymbols/oil.jpg");
            UiCollection.MetalImageID = UIBuilder.LoadTexture("UiSymbols/metal.jpg");
            UiCollection.CrystalImageID = UIBuilder.LoadTexture("UiSymbols/crystal.jpg");
        }

        private void LoadVoxelColorPalette()
        {
            ModelControl.LoadVoxelModel("Palette", "Markers/Buyable/Idle/Buyable0.vox");
            var ColorPaletteFromMagica = VoxFileControl.Get("Palette").ColorPalette; 
            ColorPaletteControl.AddColorPaletteWithMagicaVoxelPalette("MagicaModelColorPalette", ColorPaletteFromMagica);
            ColorPaletteControl.SetRenderColorPalette("MagicaModelColorPalette");

            //ColorPaletteControl.OverrideColor("MagicaModelColorPalette", 102, 255, 255, 127, 108);  // WINDOW BLUE
            //ColorPaletteControl.OverrideColor("MagicaModelColorPalette", 255, 0, 204, 127, 31);     // CRYSTAL PINK
            //ColorPaletteControl.OverrideColor("MagicaModelColorPalette", 0, 204, 255, 127, 186);    // WATER BLUE
            //ColorPaletteControl.OverrideColor("MagicaModelColorPalette", 0, 255, 255, 127, 180);    // MARKER BLUE
        }

        private void LoadSun()
        {
            LightControl.CreateAndAddSun("defaultSun", 0.0f, -55.0f, new Vector3(0.9f, 0.9f, 0.9f));
            LightControl.SelectActiveSun("defaultSun");
        }

        private void LoadSettingsObjects()
        {
            PlayerViewPoint.Transform.Position = new Vector3(70, 90, -50);

            ObjectControl.CreateObjectFromAnimationSet("World", new Vector3(32, 32, 32), "World");
            Objects.RotatingObjects.Add(("World", 3));
        }

        private void LoadNewWorldObjects(Random rand)
        {
            if (!MultiplayerRunning)
                Objects.GetRandomStartField();

            int zOffset = 75;

            if (Marker.ZCoordinate > 0)
                zOffset = 150;

            PlayerViewPoint.Transform.Position = new Vector3(Marker.XCoordinate * 64 + 32, 150, (Marker.ZCoordinate * 64) - zOffset);

            for (int x = 0; x < XChunks / 2; x++)
                for (int z = 0; z < ZChunks / 2; z++)
                    Objects.CreateMapDesign(rand.Next(0, 100), x, z);

            HighlightFields.BuyableLandAfterPurchase(Marker.XCoordinate, Marker.ZCoordinate);
            Objects.CreateObjectFromAnimationSet(Marker.XCoordinate, Marker.ZCoordinate, "marker", "Marker");

            Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {Marker.XCoordinate} {Marker.ZCoordinate}", 1);
        }

        private void DrawConsoleOutputs()
        {
            //Console.WriteLine(Timer.FramesPerSecond);
            //CameraControl.PrintActiveCameraData();
        }

        private void DrawSun()
        {
            LightControl.ChangeYaw("defaultSun", (float)KeyboardControls.CameraRotation);
        }

        public void StartGame(int seed)
        {
            if (!MultiplayerRunning)
            {
                SetDefaultSettings();
                LoadNewWorld();
            }

            GameIsActive = true;
            KeyboardControls.LobbyActive = false;
            LoadNewWorldObjects(new Random(seed));
            //new RoundTimer();
            RoundTimer.GetInstance().Start();
            KeyboardControls.GameMenuActive = false;
        }

        public void StartGameSettings(int seed, int[] settings)
        {
            if (MultiplayerRunning)
            {
                SetChunkSize(settings[0]);
                RoundTimer.GetInstance().SetupTimer(settings[1]);
                RoundTimer.GetInstance().WinningRound = settings[2];
                RoundTimer.GetInstance().MarketPriceIncreaseRound = settings[3];

                ActivePlayer.SetPlayerResources(settings[4], settings[5], settings[6], settings[7], settings[8], settings[9], settings[10]);
                Prices.SetPrices(settings[11], settings[12], settings[13], settings[14], settings[15], settings[16], settings[17]);
            }
            else
                UiCollection.SetSettings(ActivePlayer);

            GameIsActive = true;
            LoadNewWorld();
            LoadNewWorldObjects(new Random(seed));
            //new RoundTimer();
            RoundTimer.GetInstance().Start();
            KeyboardControls.GameSettingsActive = false;
        }
    }
}