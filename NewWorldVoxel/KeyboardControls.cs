using System;
using System.Numerics;
using VoxelEngine.WrapperInput;

namespace NewWorldVoxel
{
    class KeyboardControls
    {
        private static Game game_ = Game.GetInstance();

        public static double CameraRotation = 0;

        public static bool GameMenuActive = true;
        public static bool SingleplayerActive = false;
        public static bool MultiplayerActive = false;
        public static bool CreateLobbyActive = false;
        public static bool JoinLobbyActive = false;
        public static bool LobbyActive = false;
        public static bool GameSettingsActive = false;
        public static bool HelpActive = false;
        public static bool ResearchActive = false;
        public static bool LandMenuActive = false;
        public static bool MarketMenuActive = false;
        public static bool MarketBuyActive = false;
        public static bool MarketBuyResourceActive = false;
        public static bool MarketSellActive = false;
        public static bool MarketSellResourceActive = false;
        public static bool AssignMachineActive = false;
        public static bool SellCheckActive = false;

        public static void GameStartMenu()
        {
            if (!GameMenuActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                SingleplayerActive = true;
                GameMenuActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                MultiplayerActive = true;
                GameMenuActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.F1))
            {
                Network.GetInstance().CreateLobby("Horst", 5);
                GameMenuActive = false;
                LobbyActive = true;
                MultiplayerActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.F2))
            {
                Network.GetInstance().JoinLobby("127.0.0.1", "Kev");
                GameMenuActive = false;
                LobbyActive = true;
                MultiplayerActive = false;
            }
        }

        public static void SingleplayerMenu()
        {
            if (!SingleplayerActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                game_.StartGame(game_.GameSeed);
                SingleplayerActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                GameSettingsActive = true;
                SingleplayerActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Escape))
            {
                GameMenuActive = true;
                SingleplayerActive = false;
            }
        }

        public static void MultiplayerMenu()
        {
            if (!MultiplayerActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                CreateLobbyActive = true;
                MultiplayerActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                JoinLobbyActive = true;
                MultiplayerActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Escape))
            {
                GameMenuActive = true;
                MultiplayerActive = false;
            }
        }

        public static void Settings()
        {
            if (!GameSettingsActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Enter))
            {
                if (!UiCollection.MultiplayerSettings)
                    game_.StartGameSettings(game_.GameSeed, null);
                else
                {
                    LobbyActive = true;
                    GameSettingsActive = false;
                    NetworkTransactions.CustomSettings = true;
                }

                //switch (UiCollection.MultiplayerSettings)
                //{
                //    case false:
                //        game_.StartGameSettings(game_.GameSeed, null);
                //        break;
                //    case true:
                //        LobbyActive = true;
                //        GameSettingsActive = false;
                //        NetworkTransactions.CustomSettings = true;
                //        break;
                //}
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Escape))
            {
                GameSettingsActive = false;

                if (Network.GetInstance().IsServer)
                    LobbyActive = true;
                else
                    SingleplayerActive = true;
            }
        }
        
        public static void Camera()
        {
            if (GameSettingsActive || GameMenuActive) return; 

            CheckCameraRot();

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.W))
                game_.PlayerViewPoint.Transform.Position += game_.PlayerViewPoint.Transform.Forward * 2;

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.S))
                game_.PlayerViewPoint.Transform.Position -= game_.PlayerViewPoint.Transform.Forward * 2;

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.D))
                game_.PlayerViewPoint.Transform.Position += game_.PlayerViewPoint.Transform.Right * 2;

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.A))
                game_.PlayerViewPoint.Transform.Position -= game_.PlayerViewPoint.Transform.Right * 2;

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.NumAdd))
                game_.PlayerViewPoint.Transform.Position -= game_.PlayerViewPoint.Transform.Up;

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.NumSubtract))
                game_.PlayerViewPoint.Transform.Position += game_.PlayerViewPoint.Transform.Up;

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.Q))
            {
                game_.PlayerViewPoint.Transform.Orientation = Quaternion.Concatenate(game_.PlayerViewPoint.Transform.Orientation, Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 192)));
                CameraRotation += 0.9375;
            }

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.E))
            {
                game_.PlayerViewPoint.Transform.Orientation = Quaternion.Concatenate(game_.PlayerViewPoint.Transform.Orientation, Quaternion.CreateFromAxisAngle(Vector3.UnitY, -(float)(Math.PI / 192)));
                CameraRotation -= 0.9375;
            }

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.LeftShift) && game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Space))
            {
                game_.PlayerViewPoint.Transform.Position = new Vector3(Marker.XCoordinate * 64 + 32, 150, Marker.ZCoordinate * 64 - 128);
                game_.PlayerViewPoint.Transform.Orientation = Quaternion.Identity;
                CameraRotation = 0;
            }

            if (game_.WrapInput.IsKeyboardKeyDown(WrappKeyboardKeys.LeftControl) && game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Space))
            {
                game_.PlayerViewPoint.Transform.Position = new Vector3(Objects.XStart * 64 + 32, 150, Objects.ZStart * 64 - 128);
                game_.PlayerViewPoint.Transform.Orientation = Quaternion.Identity;
                CameraRotation = 0;
                Objects.DeleteObject($"marker x: {Marker.XCoordinate} z: {Marker.ZCoordinate}");
                Marker.XCoordinate = Objects.XStart;
                Marker.ZCoordinate = Objects.ZStart;
                Objects.CreateObjectFromAnimationSet(Marker.XCoordinate, Marker.ZCoordinate, "marker", "Marker");
            }
        }

        public static void MainControls()
        {
            if (GameSettingsActive || GameMenuActive) return;

            Player player = Game.GetInstance().ActivePlayer;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.F2))
            {
                LandMenuActive = false;
                AssignMachineActive = false;
                MarketMenuActive = true;
                MarketBuyActive = false;
                MarketBuyResourceActive = false;
                MarketSellActive = false;
                MarketSellResourceActive = false;
                ResearchActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.F4))
            {
                LandMenuActive = false;
                AssignMachineActive = false;
                MarketMenuActive = false;
                MarketBuyActive = false;
                MarketBuyResourceActive = false;
                MarketSellActive = false;
                MarketSellResourceActive = false;

                if (player.ResearchFacility >= 1)
                    ResearchActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.F10))
            {
                if (HelpActive)
                    HelpActive = false;
                else
                    HelpActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Space))
                if (Objects.CheckForPlainField(Marker.XCoordinate, Marker.ZCoordinate) == false && Objects.CheckIfObjectNameExists("start", Marker.XCoordinate, Marker.ZCoordinate) == false)
                {
                    LandMenuActive = true;
                    AssignMachineActive = false;
                    MarketMenuActive = false;
                    MarketBuyActive = false;
                    MarketBuyResourceActive = false;
                    MarketSellActive = false;
                    MarketSellResourceActive = false;
                    ResearchActive = false;
                }

            if (Objects.CheckForPlainField(Marker.XCoordinate, Marker.ZCoordinate) || Objects.CheckIfObjectNameExists("unusable", Marker.XCoordinate, Marker.ZCoordinate)|| Objects.CheckIfObjectNameExists("unusableExtra", Marker.XCoordinate, Marker.ZCoordinate)|| Objects.CheckIfObjectNameExists("start", Marker.XCoordinate, Marker.ZCoordinate))
            {
                LandMenuActive = false;
                AssignMachineActive = false;
            }
        }

        private static void CheckCameraRot()
        {
            if (CameraRotation > 360)
                CameraRotation -= 360;
            else if (CameraRotation < 0)
                CameraRotation += 360;
        }

        private static string ReturnCamPos(double camRot)
        {
            if (camRot < 45 || camRot > 315)
                return "front";
            else if (camRot > 45 && camRot < 135)
                return "right";
            else if (camRot > 135 && camRot < 225)
                return "behind";
            else
                return "left";
        }

        public static void MoveMarker()
        {
            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Up))
                Marker.MoveMarkerUp(ReturnCamPos(CameraRotation));

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Down))
                Marker.MoveMarkerDown(ReturnCamPos(CameraRotation));

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Left))
                Marker.MoveMarkerLeft(ReturnCamPos(CameraRotation));

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Right))
                Marker.MoveMarkerRight(ReturnCamPos(CameraRotation));
        }

        public static void ResearchMenu()
        {
            if (!ResearchActive) return;

            Player player = Game.GetInstance().ActivePlayer;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
                Research.CheckPlayerResources(player, "food");

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
                Research.CheckPlayerResources(player, "oil");

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Three))
                Research.CheckPlayerResources(player, "metal");

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Four))
                Research.CheckPlayerResources(player, "crystal");

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Five))
                ResearchActive = false;
        }

        public static void LandMenu() 
        {
            if (!LandMenuActive || SellCheckActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
                if (Objects.CheckIfObjectNameExists("buyable", Marker.XCoordinate, Marker.ZCoordinate))
                    Transactions.BuyLand(Marker.XCoordinate, Marker.ZCoordinate);
                else if (Objects.CheckForMachineField(Marker.XCoordinate, Marker.ZCoordinate))
                    SellCheckActive = true;
                else if (Objects.CheckIfObjectNameExists("research", Marker.XCoordinate, Marker.ZCoordinate))
                    SellCheckActive = true;
                else if (Objects.CheckIfObjectNameExists("marketHall", Marker.XCoordinate, Marker.ZCoordinate))
                    SellCheckActive = true;
                else
                    Transactions.SellLand(Marker.XCoordinate, Marker.ZCoordinate);

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                if (Objects.CheckIfObjectNameExists("city", Marker.XCoordinate, Marker.ZCoordinate))
                {
                    LandMenuActive = false;
                    AssignMachineActive = true;
                }

                if(Objects.CheckForMachineField(Marker.XCoordinate, Marker.ZCoordinate))
                    Transactions.ExemptMachine(Marker.XCoordinate, Marker.ZCoordinate);
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Three))
            {
                if (Objects.CheckIfObjectNameExists("city", Marker.XCoordinate, Marker.ZCoordinate) || Objects.CheckIfObjectNameExists("research", Marker.XCoordinate, Marker.ZCoordinate))
                    Transactions.BuildResearchFacility(Marker.XCoordinate, Marker.ZCoordinate); 
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Four))
            {
                if (Objects.CheckIfObjectNameExists("city", Marker.XCoordinate, Marker.ZCoordinate) || Objects.CheckIfObjectNameExists("marketHall", Marker.XCoordinate, Marker.ZCoordinate))     
                    Transactions.BuildMarketHall(Marker.XCoordinate, Marker.ZCoordinate);
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Five))
            {
                LandMenuActive = false;
                AssignMachineActive = false;
            }
        }

        public static void MarketMenu()
        {
            if (!MarketMenuActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                MarketMenuActive = false;
                MarketBuyActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                MarketMenuActive = false;
                MarketSellActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Three))
                MarketMenuActive = false;
        }

        public static void MarketBuy()
        {
            if (!MarketBuyActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                UiCollection.SelectedWare = "food";
                MarketBuyActive = false;
                MarketBuyResourceActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                UiCollection.SelectedWare = "oil";
                MarketBuyActive = false;
                MarketBuyResourceActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Three))
            {
                UiCollection.SelectedWare = "metal";
                MarketBuyActive = false;
                MarketBuyResourceActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Four))
            {
                UiCollection.SelectedWare = "crystal";
                MarketBuyActive = false;
                MarketBuyResourceActive = true;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Five))
            {
                MarketMenuActive = true;
                MarketBuyActive = false;
            }
        }

        public static void MarketBuyResource()
        {
            if (!MarketBuyResourceActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                Transactions.BuyWare(UiCollection.SelectedWare);
                MarketBuyActive = true;
                MarketBuyResourceActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                MarketBuyActive = true;
                MarketBuyResourceActive = false;
            }
        }

        public static void MarketSell()
        {
            if (!MarketSellActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                UiCollection.SelectedWare = "food";
                MarketSellResourceActive = true;
                MarketSellActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                UiCollection.SelectedWare = "oil";
                MarketSellResourceActive = true;
                MarketSellActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Three))
            {
                UiCollection.SelectedWare = "metal";
                MarketSellResourceActive = true;
                MarketSellActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Four))
            {
                UiCollection.SelectedWare = "crystal";
                MarketSellResourceActive = true;
                MarketSellActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Five))
            {
                MarketSellActive = false;
                MarketMenuActive = true;
            }
        }

        public static void MarketSellResource()
        {
            if (!MarketSellResourceActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                Transactions.SellWare(UiCollection.SelectedWare);
                MarketSellActive = true;
                MarketSellResourceActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                MarketSellActive = true;
                MarketSellResourceActive = false;
            }
        }

        public static void AssignMachine()
        {
            if (!AssignMachineActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.One))
            {
                Transactions.AssignMachine("food", "FoodMachine", Marker.XCoordinate, Marker.ZCoordinate);
                LandMenuActive = true;
                AssignMachineActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Two))
            {
                Transactions.AssignMachine("oil", "OilMachine", Marker.XCoordinate, Marker.ZCoordinate);
                LandMenuActive = true;
                AssignMachineActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Three))
            {
                Transactions.AssignMachine("metal", "MetalMachine", Marker.XCoordinate, Marker.ZCoordinate);
                LandMenuActive = true;
                AssignMachineActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Four))
            {
                Transactions.AssignMachine("crystal", "CrystalMachine", Marker.XCoordinate, Marker.ZCoordinate);
                LandMenuActive = true;
                AssignMachineActive = false;
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Five) || !Objects.CheckIfObjectNameExists("city", Marker.XCoordinate, Marker.ZCoordinate))
            {
                LandMenuActive = true;
                AssignMachineActive = false;
            }
        }

        public static void DrawError()
        {
            if (!UiCollection.GameErrorActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Enter))
                UiCollection.GameErrorActive = false;
        }
        
        public static void SellCheck()
        {
            if (!SellCheckActive) return;

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Enter))
            {
                if (Objects.CheckIfObjectNameExists("research", Marker.XCoordinate, Marker.ZCoordinate))
                {
                    Transactions.SellLandWithResearchFacility(Marker.XCoordinate, Marker.ZCoordinate);
                    SellCheckActive = false;
                }
                else if(Objects.CheckIfObjectNameExists("marketHall", Marker.XCoordinate, Marker.ZCoordinate))
                {
                    Transactions.SellLandWithMarketHall(Marker.XCoordinate, Marker.ZCoordinate);
                    SellCheckActive = false;
                }
                else
                {
                    Transactions.SellLandWithMachine(Marker.XCoordinate, Marker.ZCoordinate);
                    SellCheckActive = false;
                }
            }

            if (game_.WrapInput.IsKeyPressedOnce(WrappKeyboardKeys.Escape))
                SellCheckActive = false;
        }
    }
}
