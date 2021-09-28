using VoxelEngine.UI;
using System.Numerics;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;

namespace NewWorldVoxel
{
    internal class UiCollection
    {
        private static Game game_ = Game.GetInstance();

        private static int _foodProduction = 0;

        private static bool _cameraHelp = false;
        private static bool _gameHelp = false;
        private static bool _uiHelp = false;
        private static bool _researchHelp = false;
        private static bool _updateToggle = false;

        private static int deltaTicks_ = 5000;
        private static readonly int refreshTime_ = 5000;

        private static string _clientName;

        public static string SelectedWare = string.Empty;

        public static uint PopsImageID;
        public static uint UnemployedImageID;
        public static uint HousingsImageID;
        public static uint MoneyImageID;
        public static uint FoodImageID;
        public static uint OilImageID;
        public static uint MetalImageID;
        public static uint CrystalImageID;

        public static bool ClientDisconnected = false;
        public static bool ServerDisconnected = false;
        public static bool EndOfGameActive = false;
        public static bool GameErrorActive = false;
        public static bool ErrorActive = false;
        public static bool PauseMenu = false;
        public static bool MultiplayerSettings = false;

        public static int ServerTime = 0;

        public static void DrawGameStartMenu()
        {
            if (!KeyboardControls.GameMenuActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(550, 125), UICond.Always)
                .Begin(" NewVoxWorld",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld - ein IT-Designers Spiel")
                .NewLine(true)
                .TextWrapped(true, " 1. Startet den NewVoxelWorld Singleplayer.")
                .NewLine(true)
                .TextWrapped(true, " 2. Startet den NewVoxelWorld Multiplayer.")
                .End();
        }

        public static void DrawSingleplayerMenu()
        {
            if (!KeyboardControls.SingleplayerActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(550, 150), UICond.Always)
                .Begin(" Singleplayer",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld - Singleplayer")
                .NewLine(true)
                .TextWrapped(true, " 1. Startet das Spiel mit Default Einstellungen.")
                .NewLine(true)
                .TextWrapped(true, " 2. Alle wichtigen Einstellungen selber bestimmen.")
                .NewLine(true)
                .TextWrapped(true, " 3. [ESC] Zurueck")
                .End();
        }

        public static void DrawMultiplayerMenu()
        {
            if (!KeyboardControls.MultiplayerActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(550, 150), UICond.Always)
                .Begin(" Multiplayer",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld - Multiplayer")
                .NewLine(true)
                .TextWrapped(true, " 1. Eine lokale Lobby erstellen.")
                .NewLine(true)
                .TextWrapped(true, " 2. Einer lokalen Lobby beitreten.")
                .NewLine(true)
                .TextWrapped(true, " 3. [ESC] Zurueck")
                .End();
        }

        public static void DrawCreateLobby()
        {
            if (!KeyboardControls.CreateLobbyActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(550, 225), UICond.Always)
                .Begin(" CreateLobby",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld - Lokale Lobby erstellen")
                .NewLine(true)
                .InputTextWithHint(true, "Nickname", "min. 3 Buchstaben")
                .InputTextWithHint(true, "Spielerzahl", "min 2.")
                .NewLine(true)    
                .Button(true, "Create")
                .NewLine(true)
                .Button(true, "Zurueck")
                .End();

            if ((bool)game_.UIBuilder.ElementValues["Create"])
            {
                string userName = (string)game_.UIBuilder.ElementValues["Nickname"];
                string playerCount = (string)(game_.UIBuilder.ElementValues["Spielerzahl"]);

                Network.GetInstance().ResetHostSearch();
                Network.GetInstance().CreateLobby(userName, int.Parse(playerCount));

                KeyboardControls.CreateLobbyActive = false;
                KeyboardControls.LobbyActive = true;
            }
            else if ((bool)game_.UIBuilder.ElementValues["Zurueck"])
            {
                KeyboardControls.CreateLobbyActive = false;
                KeyboardControls.MultiplayerActive = true;
            }
        }

        public static void DrawJoinLobby()
        {
            if (!KeyboardControls.JoinLobbyActive) return;

            Dictionary<string, IPAddress> openHosts = Network.GetInstance().GetOpenHosts();
            Regex ipAdressRegex = new Regex("^[0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}$");

            game_.UIBuilder.SetNextWindowSize(new Vector2(550, 285), UICond.Always)
                .Begin(" JoinLobby",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld - Lokaler Lobby beitreten")
                .NewLine(true).BeginChild(" openHosts", new Vector2(525, 125), UIWindowFlags.None);

            foreach(var host in openHosts)
            {
                game_.UIBuilder.TextWrapped(true, $"{host.Key} : {host.Value.ToString()}")
                    .IsItemHovered(true, UIHoveredFlags.None, out bool isHovered);

                if (isHovered)
                    game_.UIBuilder.ElementValues["Server IP"] = host.Value.ToString();
            }

            game_.UIBuilder.EndChild()
                .NewLine(true)
                .InputTextWithHint(true, "Nickname", "min. 3 Buchstaben")
                .InputText(true, "Server IP")
                .NewLine(true)
                .Button(true, "Join")
                .SameLine(true).Button(true, "Zurueck")
                .End();

            if ((bool)game_.UIBuilder.ElementValues["Join"])
            {
                string userName = (string)game_.UIBuilder.ElementValues["Nickname"];
                string hostIP = (string)game_.UIBuilder.ElementValues["Server IP"];

                if (userName.Length > 2 && userName != string.Empty)
                    if(ipAdressRegex.IsMatch(hostIP))
                    {
                        Network.GetInstance().ResetHostSearch();
                        Network.GetInstance().JoinLobby(hostIP, userName);

                        KeyboardControls.JoinLobbyActive = false;
                        KeyboardControls.LobbyActive = true;
                    }
                    else
                        System.Console.WriteLine("Die IP ist nicht richtig");
                else
                    System.Console.WriteLine("Der Name ist nicht korrekt");
            }
            else if ((bool)game_.UIBuilder.ElementValues["Zurueck"])
            {
                KeyboardControls.JoinLobbyActive = false;
                KeyboardControls.MultiplayerActive = true;
            }
        }

        public static void UpdateOpenHosts()
        {
            deltaTicks_ += (int)(game_.Timer.TimeBetweenDrawsInSeconds * 1000);

            if (!KeyboardControls.JoinLobbyActive) return;

            if (deltaTicks_ >= refreshTime_ / 2)
                if (!_updateToggle)
                {
                    deltaTicks_ = 0;
                    Network.GetInstance().SearchForHosts();
                    _updateToggle = true;
                }
                else
                {
                    Network.GetInstance().ResetHostSearch();
                    _updateToggle = false;
                }
        }

        public static void DrawLobby()
        {
            if (!KeyboardControls.LobbyActive) return;

            string clients = string.Empty;

            foreach(var client in Network.GetInstance().GetClients())
            {
                clients += " " + client.Key.ToString() + " : " + client.Value;
                if (client.Key == Network.GetInstance().HostName)
                    clients += " (Host)";
                else
                    clients += " (Client)";

                clients += "\n";
            }

            game_.UIBuilder.SetNextWindowSize(new Vector2(550, 275), UICond.Always)
                .Begin(" Lobby",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, $" {Network.GetInstance().HostName}'s Lobby:")
                .NewLine(true).BeginChild("Clients", new Vector2(530, 180), UIWindowFlags.None)
                .TextWrapped(true, clients)
                .EndChild().NewLine(true)
                .Button(Network.GetInstance().IsServer, "Start")
                .SameLine(true).Button(Network.GetInstance().IsServer, "Einstellungen")
                .SameLine(true).Button(true, "Leave Lobby")
                .End();

            if ((bool)game_.UIBuilder.ElementValues["Leave Lobby"])
            {
                //Network.GetInstance().Disconnect();
                KeyboardControls.GameMenuActive = true;
                KeyboardControls.LobbyActive = false;
            }

            if (Network.GetInstance().IsServer && Network.GetInstance().GetClients().Count > 1)
            {
                if ((bool)game_.UIBuilder.ElementValues["Start"])
                {
                    game_.MultiplayerRunning = true;

                    if (NetworkTransactions.CustomSettings)
                        NetworkTransactions.GetInstance().StartMultiplayerGame("custom");
                    else
                        NetworkTransactions.GetInstance().StartMultiplayerGame("default");
                }
                else if ((bool)game_.UIBuilder.ElementValues["Einstellungen"])
                {
                    KeyboardControls.LobbyActive = false;
                    KeyboardControls.GameSettingsActive = true;
                    MultiplayerSettings = true;
                }
            }
        }

        public static void DrawSettings()
        {
            if (!KeyboardControls.GameSettingsActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(600, 700), UICond.Always)
                .Begin(" Settings",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(40, 40), UICond.Always)
                .TextWrapped(true, "Spiel Einstellungen:")
                .NewLine(true)
                .TextWrapped(!MultiplayerSettings, "Weltgroesse muss angegeben werden, bei rest sonst default")
                .InputTextWithHint(!MultiplayerSettings, "Weltgroesse", "x * x Chunks")
                .InputInt(true, "Rundenzeit")
                .InputInt(true, "Finalrunde")
                .TextWrapped(true, "Alle x Runden werden Markt Preise erhoeht.")
                .InputInt(true, "Preiserhoehung")
                .NewLine(true)
                .TextWrapped(true, "Spieler Einstellungen:")
                .NewLine(true)
                .InputInt(true, "Geld")
                .InputInt(true, "Bevoelkerung")
                .InputInt(true, "Wohnungen")
                .InputInt(true, "Nahrung")
                .InputInt(true, "Oel")
                .InputInt(true, "Metall")
                .InputInt(true, "Kristall")
                .NewLine(true)
                .TextWrapped(true, "Markt Einstellungen:")
                .NewLine(true)
                .InputInt(true, "landPreis")
                .InputInt(true, "nahrungsPreis")
                .InputInt(true, "oelPreis")
                .InputInt(true, "metallPreis")
                .InputInt(true, "kristallPreis")
                .TextWrapped(true, "Metall Kosten zum bauen.")
                .InputInt(true, "bauKosten")
                .TextWrapped(true, "0.9 = 90% des Einkaufspreises.")
                .InputFloat(true, "Verkaufspreis")
                .NewLine(true)
                .TextWrapped(true, " Bitte bestaetigen Sie mit Enter.")
                .TextWrapped(true, " [ESC] Zurueck")
                .End();
        }

        public static void SetSettings(Player player)
        {
            IDictionary<string, object> SettingsDict = game_.UIBuilder.ElementValues;

            if (!MultiplayerSettings)
                game_.SetChunkSize(int.Parse((string)SettingsDict["Weltgroesse"]));
            else
            {
                if (Network.GetInstance().GetClients().Count <= 4)
                    game_.SetChunkSize(20);
                else
                    game_.SetChunkSize(40);
            }

            RoundTimer.GetInstance().SetupTimer((int)SettingsDict["Rundenzeit"] > (int)game_.DefaultSettings["gameRoundTime"] 
                ? (int)SettingsDict["Rundenzeit"] : (int)game_.DefaultSettings["gameRoundTime"]);

            RoundTimer.GetInstance().WinningRound = (int)SettingsDict["Finalrunde"] > (int)game_.DefaultSettings["winningRound"]
                ? (int)SettingsDict["Finalrunde"] : (int)game_.DefaultSettings["winningRound"];

            RoundTimer.GetInstance().MarketPriceIncreaseRound = (int)SettingsDict["Preiserhoehung"];

            int money = (int)SettingsDict["Geld"] > (int)game_.DefaultSettings["money"]
                ? (int)SettingsDict["Geld"] : (int)game_.DefaultSettings["money"];

            int citizens = (int)SettingsDict["Bevoelkerung"] > (int)game_.DefaultSettings["citizens"]
                ? (int)SettingsDict["Bevoelkerung"] : (int)game_.DefaultSettings["citizens"];

            int housings = (int)SettingsDict["Wohnungen"] > (int)game_.DefaultSettings["initialHousings"]
                ? (int)SettingsDict["Wohnungen"] : (int)game_.DefaultSettings["initialHousings"];

            int food = (int)SettingsDict["Nahrung"] > (int)game_.DefaultSettings["food"]
                ? (int)SettingsDict["Nahrung"] : (int)game_.DefaultSettings["food"];

            int oil = (int)SettingsDict["Oel"] > (int)game_.DefaultSettings["oil"]
                ? (int)SettingsDict["Oel"] : (int)game_.DefaultSettings["oil"];

            int metal = (int)SettingsDict["Metall"] > (int)game_.DefaultSettings["metal"]
                ? (int)SettingsDict["Metall"] : (int)game_.DefaultSettings["metal"];

            int crystal = (int)SettingsDict["Kristall"] > (int)game_.DefaultSettings["crystal"]
                ? (int)SettingsDict["Kristall"] : (int)game_.DefaultSettings["crystal"];

            int landPrice = (int)SettingsDict["landPreis"] > (int)game_.DefaultSettings["landPrice"]
                ? (int)SettingsDict["landPreis"] : (int)game_.DefaultSettings["landPrice"];

            int foodPrice = (int)SettingsDict["nahrungsPreis"] > (int)game_.DefaultSettings["foodPrice"]
                ? (int)SettingsDict["nahrungsPreis"] : (int)game_.DefaultSettings["foodPrice"];

            int oilPrice = (int)SettingsDict["oelPreis"] > (int)game_.DefaultSettings["oilPrice"]
                ? (int)SettingsDict["oelPreis"] : (int)game_.DefaultSettings["oilPrice"];

            int metalPrice = (int)SettingsDict["metallPreis"] > (int)game_.DefaultSettings["metalPrice"]
                ? (int)SettingsDict["metallPreis"] : (int)game_.DefaultSettings["metalPrice"];

            int crystalPrice = (int)SettingsDict["kristallPreis"] > (int)game_.DefaultSettings["crystalPrice"]
                ? (int)SettingsDict["kristallPreis"] : (int)game_.DefaultSettings["crystalPrice"];

            int buildCost = (int)SettingsDict["bauKosten"] > (int)game_.DefaultSettings["buildCost"]
                ? (int)SettingsDict["bauKosten"] : (int)game_.DefaultSettings["buildCost"];

            float saleLoss = (float)SettingsDict["Verkaufspreis"] != (float)game_.DefaultSettings["saleLoss"]
                ? (float)SettingsDict["Verkaufspreis"] : (float)game_.DefaultSettings["saleLoss"];

            player.SetPlayerResources(money, citizens, housings, food, oil, metal, crystal);
            Prices.SetPrices(landPrice, foodPrice, oilPrice, metalPrice, crystalPrice, buildCost, saleLoss);
        }

        public static void DrawMainMenuBar()
        {
            Player player = game_.ActivePlayer;

            _foodProduction = Transactions.ProducedResources["food"] - player.Citizens;
            ServerTime = RoundTimer.GetInstance().Span.Seconds - RoundTimer.GetInstance().Stopwatch.Elapsed.Seconds;

            game_.UIBuilder.BeginMainMenuBar()
                .Button(true, "Back to Main Menu")
                .TextWrapped(true, "NewVoxelWorld  || ")
                .Image(true, PopsImageID, new Vector2(18,18))
                .TextWrapped(true, $"{player.Citizens}")
                .Image(true, UnemployedImageID, new Vector2(18, 18))
                .TextWrapped(true, $"{player.UnassignedCitizens}")
                .Image(true, HousingsImageID, new Vector2(18, 18))
                .TextWrapped(true, $"{player.EmptyFreeHousings * Transactions.CitizensPerHousing - player.Citizens}  | ") 
                .TextWrapped(true, $"{player.Money}  || ")
                .Image(true, FoodImageID, new Vector2(18, 18))
                .TextWrapped(true, $"{player.Resources["food"]} + {_foodProduction} ")
                .Image(true, OilImageID, new Vector2(18, 18))
                .TextWrapped(true, $"{player.Resources["oil"]} + {Transactions.ProducedResources["oil"]} ")
                .Image(true, MetalImageID, new Vector2(18, 18))
                .TextWrapped(true, $"{player.Resources["metal"]} + {Transactions.ProducedResources["metal"]} ")
                .Image(true, CrystalImageID, new Vector2(18, 18))
                .TextWrapped(true, $"{player.Resources["crystal"]} + {Transactions.ProducedResources["crystal"]}  || ")
                .TextWrapped(true, $"Rundenende in: {ServerTime}  ")
                .Button(true, "play")
                .Button(true, "pause")
                .TextWrapped(true, $"  || Runden: {RoundTimer.GetInstance().GameRoundCounter}/{RoundTimer.GetInstance().WinningRound}")
                .TextWrapped(true, "  || [F10] Help")
                .EndMainMenuBar();

            PlayPausePressed(ServerTime);

            if ((bool)game_.UIBuilder.ElementValues["Back to Main Menu"])
            {
                EndOfGameActive = false;

                if(Network.GetInstance().IsServer)
                    Network.GetInstance().SendMessage($"host", 14); 
                else
                    Network.GetInstance().SendMessage($"{Network.GetInstance().UserName}", 14); 

                game_.BackToMainMenu();
            }
        }

        private static void PlayPausePressed(int inGameTime)
        {
            if ((bool)game_.UIBuilder.ElementValues["pause"])
            {
                Network.GetInstance().SendMessage($"pause {inGameTime}", 6);

                RoundTimer.GetInstance().Stop();
                RoundTimer.GetInstance().Set(inGameTime - 1);
                PauseMenu = true;
            }

            if ((bool)game_.UIBuilder.ElementValues["play"])
            {
                Network.GetInstance().SendMessage($"play {inGameTime}", 6);

                RoundTimer.GetInstance().Start();
                PauseMenu = false;
            }
        }

        public static void DrawPauseMenu()
        {
            if (!PauseMenu) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(125, 25), UICond.Always)
                .Begin(" PauseMenu",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 135, 30), UICond.Always)
                .TextWrapped(true, " Spielpause")
                .End();
        } 

        public static void DrawMachineMenuBar()
        {
            Player player = game_.ActivePlayer;
            string researchActive = "Aktuell kein Forschungsprojekt";

            if (Research.ResearchIsActive)
                researchActive = $"Forschung {Research.ResearchSubject} Rang: {Research.ResearchRank[Research.ResearchSubject] +1} " +
                    $"abgeschlossen in: {Research.ActiveResearchTime} Runden";

            game_.UIBuilder.SetNextWindowSize(new Vector2(500, 50), UICond.Always)
                .Begin("MachineBar",
                    UIWindowFlags.NoTitleBar |
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 510, game_.Resolution.Item2 - 60), UICond.Always)
                .TextWrapped(true, $" Gebauede  ||  Nahrung: {player.Machines["food"] + player.SpMachines["food"]} ")
                .SameLine(true)
                .TextWrapped(true, $"Oel: {player.Machines["oil"] + player.SpMachines["oil"]} ")
                .SameLine(true)
                .TextWrapped(true, $"Metall: {player.Machines["metal"] + player.SpMachines["metal"]} ")
                .SameLine(true)
                .TextWrapped(true, $"Kristall: {player.Machines["crystal"] + player.SpMachines["crystal"]}  || ")
                .TextWrapped(true, $" {researchActive}")
                .End();
        }

        public static void DrawHelp()
        {
            if (!KeyboardControls.HelpActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(450, 300), UICond.Always)
                .Begin(" Help",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 500, 50), UICond.Always)
                .TextWrapped(true, " Spielhilfe")
                .NewLine(true)
                .TextWrapped(true, " NewVoxelWorld ist ein rundenbasiertes Strategiespiel das einen starken fokus auf Economy hat." +
                    " Ihr koennt innerhalb einer Runde mit euren Ressourcen handeln, neue Gebauede errichten und euer Reich weiter ausbreiten." +
                    " Die Produktion und ihr erloes wird immer zum Rundenende berechnet und bearbeitet. Heißt also" +
                    " wenn ihr ein neues Gebauede baut wird das immer erst zur neuen Runde aktualisiert in der GuI angezeigt," +
                    " aber keine sorge eure Produktion wird direkt berechnet nur eben nicht sofort angezeigt ;)")
                .NewLine(true).Bullet(true).SameLine(true)
                .Button(true, "Interface")
                .SameLine(true).Bullet(true).SameLine(true)
                .Button(true, "Kamerasteuerung")
                .Bullet(true).SameLine(true)
                .Button(true, "Spielsteuerung")
                .SameLine(true).Bullet(true).SameLine(true)
                .Button(true, "Forschung")
                .NewLine(true)
                .BulletText(true, "[F10] beendet/oeffnet die Hilfe")
                .End();

            if ((bool)game_.UIBuilder.ElementValues["Interface"])
                _uiHelp = true;
            if ((bool)game_.UIBuilder.ElementValues["Kamerasteuerung"])
                _cameraHelp = true;
            if ((bool)game_.UIBuilder.ElementValues["Spielsteuerung"])
                _gameHelp = true;
            if ((bool)game_.UIBuilder.ElementValues["Forschung"])
                _researchHelp = true;
        }

        public static void DrawCameraHelp()
        {
            if (!_cameraHelp) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(415, 185), UICond.Always)
                .Begin(" CameraHelp",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 550, 125), UICond.Always)
                .TextWrapped(true, " Kamerasteuerung")
                .NewLine(true)
                .BulletText(true, " WASD um die Kamera zu bewegen")
                .BulletText(true, " QE umd die Kamera zu drehen")
                .BulletText(true, " Num+,Num- um die Kamera zu zoomen")
                .BulletText(true, " Shift + Space um die Kamera auf Marker zu zentrieren")
                .BulletText(true, " Strg + Space setzt Kamera und Marker auf Start zurueck")
                .NewLine(true)
                .Button(true, " Zurueck ")
                .End();

            if ((bool)game_.UIBuilder.ElementValues[" Zurueck "])
                _cameraHelp = false;
        }

        public static void DrawGameHelp()
        {
            if (!_gameHelp) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(415, 265), UICond.Always)
                .Begin(" Game.GetInstance()Help",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 550, 125), UICond.Always)
                .TextWrapped(true, " Spielsteuerung")
                .NewLine(true)
                .BulletText(true, " Pfeiltasten bewegen den Marker")
                .BulletText(true, " Leertaste oeffnet das Land Menu fuer Bau optionen")
                .BulletText(true, " Eingabe nicht ueber Numblock")
                .BulletText(true, " Menus funktionieren nur auf entsprechenden feldern")
                .TextWrapped(true, "     --> bebaute oder kaufbare (Plus) Felder")
                .BulletText(true, $" Um Produktion zu bauen werden {Prices.Purchase["production"]} Metall benoetigt")
                .BulletText(true, $" Zum Produzieren werden {Transactions.ProductionTeamSize} freie Bewohner verwendet")
                .BulletText(true, " F2 oeffnet den Markt, F4 oeffnet die Forschung")
                .BulletText(true, " Alle 5 runden werden die Marktpreise erhoeht")
                .BulletText(true, " Unten ist eine Auflistung aller Produktionsstaetten")
                .NewLine(true)
                .Button(true, " Zurueck ")
                .End();

            if ((bool)game_.UIBuilder.ElementValues[" Zurueck "])
                _gameHelp = false;
        }

        public static void DrawInterfaceHelp()
        {
            if (!_uiHelp) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(250, 250), UICond.Always)
                .Begin(" UiHelp",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 550, 125), UICond.Always)
                .TextWrapped(true, " Interface")
                .NewLine(true)
                .Image(true, PopsImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Bewohner")
                .Image(true, UnemployedImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  untaetige Bewohner")
                .Image(true, HousingsImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Wohnungen")
                .Image(true, MoneyImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Gold")
                .Image(true, FoodImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Nahrung")
                .Image(true, OilImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Oel")
                .Image(true, MetalImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Metall")
                .Image(true, CrystalImageID, new Vector2(15, 15))
                .SameLine(true).TextWrapped(true, "  Kristall")
                .NewLine(true)
                .Button(true, " Zurueck ")
                .End();

            if ((bool)game_.UIBuilder.ElementValues[" Zurueck "])
                _uiHelp = false;
        }

        public static void DrawResearchHelp()
        {
            if (!_researchHelp) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(400, 250), UICond.Always)
                .Begin(" ResearchHelp",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(game_.Resolution.Item1 - 550, 125), UICond.Always)
                .TextWrapped(true, " Forschung")
                .NewLine(true)
                .TextWrapped(true, " Um eine Forschung zu starten benoetig man")
                .TextWrapped(true, " mindestens ein Forschungsgebauede und 5 freie")
                .TextWrapped(true, " Bewohner, diese sind fuer den Zeitraum der")
                .TextWrapped(true, " Forschung gesperrt und nicht zur Verfuegung.")
                .TextWrapped(true, " Es koennen max 3 Forschungsgebauede gebaut werden, ")
                .TextWrapped(true, " Das erste ermoeglich es zu forschen, dass zweite")
                .TextWrapped(true, " verringert die Forschungsdauer und das dritte")
                .TextWrapped(true, " verringert die Forschungskosten.")
                .TextWrapped(true, " Forschungsgebaude koennnen nur auf normalen")
                .TextWrapped(true, " Feldern gebaut werden.")
                .NewLine(true)
                .Button(true, " Zurueck ")
                .End();

            if ((bool)game_.UIBuilder.ElementValues[" Zurueck "])
                _researchHelp = false;
        }

        public static void DrawLand()
        {
            if (!KeyboardControls.LandMenuActive) return;

            bool buildProduction = false;
            bool buildResearch = false;
            bool buildMarketHall = false;
            bool landBuy = true;


            if (Objects.CheckIfObjectNameExists("city", Marker.XCoordinate, Marker.ZCoordinate))
            {
                buildProduction = true;
                buildResearch = true;
                buildMarketHall= true;
                landBuy = false;
            }
            else if (Objects.CheckForMachineField(Marker.XCoordinate, Marker.ZCoordinate))
            {
                landBuy = false;
                buildProduction = true;
            }
            else if (Objects.CheckIfObjectNameExists("research", Marker.XCoordinate, Marker.ZCoordinate))
            {
                buildResearch = true;
                landBuy = false;
            }
            else if (Objects.CheckIfObjectNameExists("marketHall", Marker.XCoordinate, Marker.ZCoordinate))
            {
                buildMarketHall = true;
                landBuy = false;
            }

            game_.UIBuilder.SetNextWindowSize(new Vector2(375, 150), UICond.Always)
                .Begin("NVW Landverwaltung's Tool",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld Landverwaltung")
                .NewLine(true)
                .TextWrapped(landBuy, $" 1. [Land] ({Prices.Purchase["land"]} Gold) ..Erweitert das Reich!")
                .TextWrapped(!landBuy, $" 1. [Land] ({Prices.Sale["land"]} Gold) ..Weg mit diesem toten Land!")
                .TextWrapped(buildProduction, " 2. [Produktion]     ..Bebaut es!")
                .TextWrapped(buildResearch, " 3. [Forschung]      ..Erforscht dies alles!")
                .TextWrapped(buildMarketHall, " 4. [Markthalle]     ..Baut den Handel aus")
                .TextWrapped(true, " 5. [Verlassen]      ..Lasst mich in Ruhe!")
                .End();
        }

        public static void DrawGameError()
        {
            if (!GameErrorActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(350, 135), UICond.Always)
                .Begin("Baufehler",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(475, 250), UICond.Always)
                .TextWrapped(true, " Bauvorhaben gescheitert !!")
                .NewLine(true)
                .TextWrapped(true, " Sie haben nicht ausreichend Metall")
                .TextWrapped(true, " oder freie Bewohner um das Gebauede zu  bauen.")
                .NewLine(true)
                .TextWrapped(true, " [ENTER] ..Ok!")
                .End();
        }

        public static void DrawResearch()
        {
            if (!KeyboardControls.ResearchActive) return;

            bool foodResearch = true;
            bool oilResearch = true;
            bool metalResearch = true;
            bool crystalResearch = true;

            if (Research.ResearchRank["food"] == 5)
                foodResearch = false;
            if (Research.ResearchRank["oil"] == 5)
                oilResearch = false;
            if (Research.ResearchRank["metal"] == 5)
                metalResearch = false;
            if (Research.ResearchRank["crystal"] == 5)
                crystalResearch = false;

            game_.UIBuilder.SetNextWindowSize(new Vector2(625, 250), UICond.Always)
                .Begin("NVW Research Menu ",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld Forschungseinrichtung!")
                .TextWrapped(true, " Welche Ressourcenproduktion soll verbessert werden?")
                .NewLine(true)
                .TextWrapped(foodResearch, $" 1. Nahrung  [Rang: {Research.ResearchRank["food"]}]  " +
                    $"[Dauer: {Research.ResearchRounds["food"]} Runde(n)]  " +
                    $"[Kosten: {Research.ResearchCost["food"]} Kristall]  " +
                    $"[Produktion +{Research.ResearchProductionMultiplier["food"]}]")
                .TextWrapped(oilResearch, $" 2. Oel      [Rang: {Research.ResearchRank["oil"]}]  " +
                    $"[Dauer: {Research.ResearchRounds["oil"]} Runde(n)]  " +
                    $"[Kosten: {Research.ResearchCost["oil"]} Kristall]  " +
                    $"[Produktion +{Research.ResearchProductionMultiplier["oil"]}]")
                .TextWrapped(metalResearch, $" 3. Metall   [Rang: {Research.ResearchRank["metal"]}]  " +
                    $"[Dauer: {Research.ResearchRounds["metal"]} Runde(n)]  " +
                    $"[Kosten: {Research.ResearchCost["metal"]} Kristall]  " +
                    $"[Produktion +{Research.ResearchProductionMultiplier["metal"]}]")
                .TextWrapped(crystalResearch, $" 4. Kristall [Rang: {Research.ResearchRank["crystal"]}]  " +
                    $"[Dauer: {Research.ResearchRounds["crystal"]} Runde(n)]  " +
                    $"[Kosten: {Research.ResearchCost["crystal"]} Kristall]  " +
                    $"[Produktion +{Research.ResearchProductionMultiplier["crystal"]}]")
                .TextWrapped(true, " 5. Zurueck")
                .NewLine(true)
                .TextWrapped(true, " Es werden 5 Siedler benoetigt um die Verbesserung zu erforschen.")
                .NewLine(true)
                .TextWrapped(true, $" Fortschritt: {Research.ActiveResearchTime} Runde(n) verbleiben")
                .TextWrapped(true, $" Laufende Forschung: {Research.ResearchSubject}")
                .End();
        }

        public static void DrawMarket()
        {
            if (!KeyboardControls.MarketMenuActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(375, 150), UICond.Always)
                .Begin("NVW Handelskammer",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld Handelskammer")
                .TextWrapped(true, " Hier werdet Ihr sicher fuendig!")
                .NewLine(true)
                .TextWrapped(true, " 1. [Einkauf]    ..Fuellt unsere Kammern!")
                .TextWrapped(true, " 2. [Verkauf]    ..Verkauft es fuer Gold!")
                .TextWrapped(true, " 3. [Verlassen]  ..Hier finden wir nichts!")
                .End();
        }
             
        public static void DrawMarketBuy()
        {
            if (!KeyboardControls.MarketBuyActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(450, 175), UICond.Always)
                .Begin("Gebt euer Geld aus!",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, " Das wird euch einiges kosten!")
                .NewLine(true)
                .TextWrapped(true, $" 1. [Nahrung]  ({Prices.Purchase["food"]} Gold)  ..Mehr Nahrung fuer das Volk!")
                .TextWrapped(true, $" 2. [Oel]      ({Prices.Purchase["oil"]} Gold)  ..Schwarzes Gold!")
                .TextWrapped(true, $" 3. [Metall]   ({Prices.Purchase["metal"]} Gold)  ..Mehr Stahl!")
                .TextWrapped(true, $" 4. [Kristall] ({Prices.Purchase["crystal"]} Gold)  ..Edelsteine, so viele Edelsteine!")
                .TextWrapped(true, " 5. [Verlassen]           ..Das ist Wucher!")
                .End();
        }

        public static void DrawMarketBuyResource()
        {
            if (!KeyboardControls.MarketBuyResourceActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(375, 150), UICond.Always)
                .Begin("Einkauf",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, " Einkauf:")
                .TextWrapped(true, $" Wie viel {SelectedWare} soll ersteigert werden?")
                .InputInt(true, " Menge")
                .SetKeyboardFocusHere(true)
                .NewLine(true)
                .TextWrapped(true, $" Dies kostet Sie {Prices.Purchase[SelectedWare] * Transactions.WareAmount} Gold.")
                .TextWrapped(true, " 1. [Bestaetigen] ..Das ist ein guter Handel!")
                .TextWrapped(true, " 2. [Verlassen]   ..Das lohnt sich nicht fuer uns!")
                .End();

            Transactions.WareAmount = (int)game_.UIBuilder.ElementValues[" Menge"];
        }

        public static void DrawMarketSell()
        {
            if (!KeyboardControls.MarketSellActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(450, 175), UICond.Always)
                .Begin("Bereichert euch!",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, " NewVoxelWorld Handelskammer")
                .TextWrapped(true, " Das wird die Schatzkammer fuellen!")
                .NewLine(true)
                .TextWrapped(true, $" 1. [Nahrung]  ({Prices.Sale["food"]} Gold)  ..Bevor das Volk fett wird!")
                .TextWrapped(true, $" 2. [Oel]      ({Prices.Sale["oil"]} Gold)  ..Die Oeltanks laufen sonst voll!")
                .TextWrapped(true, $" 3. [Metall]   ({Prices.Sale["metal"]} Gold)  ..Altmetall...!")
                .TextWrapped(true, $" 4. [Kristall] ({Prices.Sale["crystal"]} Gold)  ..Kristall minderer qualitaet!")
                .TextWrapped(true, " 5. [Verlassen]            ..Dafuer bekam ich schon mehr!")
                .End();
        }

        public static void DrawMarketSellResource()
        {
            if (!KeyboardControls.MarketSellResourceActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(375, 150), UICond.Always)
                .Begin("Verkauf",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, $" Wie viel {SelectedWare} soll verkauft werden?")
                .InputInt(true, " Menge")
                .SetKeyboardFocusHere(true)
                .NewLine(true)
                .TextWrapped(true, $" Dafuer bekommen Sie {Prices.Sale[SelectedWare] * Transactions.WareAmount} Gold.")
                .TextWrapped(true, " 1. [Bestaetigen] ..Das ist ein guter Handel!")
                .TextWrapped(true, " 2. [Verlassen]   ..Das lohnt sich nicht fuer uns!")
                .End();

            Transactions.WareAmount = (int)game_.UIBuilder.ElementValues[" Menge"];
        }

        public static void DrawMachineAssign()
        {
            if (!KeyboardControls.AssignMachineActive) return;

            bool specialField;

            if(Objects.SpecialFields.ContainsKey((Marker.XCoordinate, Marker.ZCoordinate)))
                specialField = true;
            else
                specialField = false;

            game_.UIBuilder.SetNextWindowSize(new Vector2(400, 175), UICond.Always)
                .Begin("Maschine zuteilen",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(100, 100), UICond.Always)
                .TextWrapped(true, "Welche Maschine soll eingesetzt werden?")
                .NewLine(true)
                .TextWrapped(!specialField, $" 1. [Nahrung]  [+{Transactions.ProductionMultiplier["food"]}] ..Damit die Bauern nicht hungern!")
                .TextWrapped(!specialField, $" 2. [Oel]      [+{Transactions.ProductionMultiplier["oil"]}]  ..Fluessiges Gold!")
                .TextWrapped(!specialField, $" 3. [Metall]   [+{Transactions.ProductionMultiplier["metal"]}]  ..Mehr Metall, mehr Maschinen!")
                .TextWrapped(!specialField, $" 4. [Kristall] [+{Transactions.ProductionMultiplier["crystal"]}]  ..Wir sollten uns bereichern!")
                .TextWrapped(specialField, $" 1. [Nahrung]  [+{Transactions.SpecialProductionMultiplier["food"]}] ..Damit die Bauern nicht hungern!")
                .TextWrapped(specialField, $" 2. [Oel]      [+{Transactions.SpecialProductionMultiplier["oil"]}]  ..Fluessiges Gold!")
                .TextWrapped(specialField, $" 3. [Metall]   [+{Transactions.SpecialProductionMultiplier["metal"]}]  ..Mehr Metall, mehr Maschinen!")
                .TextWrapped(specialField, $" 4. [Kristall] [+{Transactions.SpecialProductionMultiplier["crystal"]}]  ..Wir sollten uns bereichern!")
                .TextWrapped(true, $" 5. [Verlassen]      ..Lassen wir das erst mal ...")
                .End();
        }

        public static void DrawSellCheck()
        {
            if (!KeyboardControls.SellCheckActive) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(350, 150), UICond.Always)
                .Begin("Warnung",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(475, 250), UICond.Always)
                .TextWrapped(true, " WARNUNG! WARNUNG! WARNUNG!")
                .TextWrapped(true, " Das wuerde das Gebauede ebenso entfernen!")
                .NewLine(true)
                .TextWrapped(true, " 1. [Enter] ..Ja, weg damit!")
                .TextWrapped(true, " 2. [ESC]   ..Nein, ich behalte es lieber!")
                .End();
        }

        public static void DrawEndOfGame()
        {
            if (!EndOfGameActive) return;

            if (game_.MultiplayerRunning)
            {
                string rankings = string.Empty;
                int counter = 1;

                foreach(var player in NetworkTransactions.PlayerRanking.OrderByDescending(x => x.Value))
                {
                    rankings += $" {counter}. {player.Key} {player.Value} Land\n";
                    counter++;
                }

                game_.UIBuilder.SetNextWindowSize(new Vector2(350, 250), UICond.Always)
                    .Begin("Rangliste",
                        UIWindowFlags.NoMove |
                        UIWindowFlags.NoCollapse |
                        UIWindowFlags.NoResize |
                        UIWindowFlags.NoTitleBar)
                    .SetWindowPos(new Vector2(475, 250), UICond.Always)
                    .TextWrapped(true, " Spiel zu Ende ")
                    .TextWrapped(true, " Rangliste:")
                    .NewLine(true)
                    .TextWrapped(true, rankings)
                    .End();
            }
            else
            {
                game_.UIBuilder.SetNextWindowSize(new Vector2(350, 250), UICond.Always)
                    .Begin("GEWONNEN!!",
                        UIWindowFlags.NoMove |
                        UIWindowFlags.NoCollapse |
                        UIWindowFlags.NoResize |
                        UIWindowFlags.NoTitleBar)
                    .SetWindowPos(new Vector2(475, 250), UICond.Always)
                    .TextWrapped(true, " Ein neuer Planet fuer das Imperium!")
                    .NewLine(true)
                    .TextWrapped(true, " Ihr habt es geschafft! Ihr habt diesen Planeten erfolgreich besiedelt und so dem Imperium eine weitere Welt hinzugefügt!")
                    .End();
            }
        }

        public static void ClientIsDisconnected(string name)
        {
            _clientName = name;
            ClientDisconnected = true;
        }

        public static void SeverIsDisconnected()
        {
            ServerDisconnected = true;
        }

        public static void DrawClientDisconnected()
        {
            if (!ClientDisconnected) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(350, 250), UICond.Always)
                .Begin("Exit",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(475, 250), UICond.Always)
                .TextWrapped(true, $"{_clientName} Hat das Spiel verlassen")
                .Button(true, "Ok")
                .End();

            if ((bool)game_.UIBuilder.ElementValues["Ok"])
                ClientDisconnected = false;
        }
        public static void DrawServerDisconnected()
        {
            if (!ServerDisconnected) return;

            game_.UIBuilder.SetNextWindowSize(new Vector2(350, 250), UICond.Always)
                .Begin("Exit",
                    UIWindowFlags.NoMove |
                    UIWindowFlags.NoCollapse |
                    UIWindowFlags.NoResize |
                    UIWindowFlags.NoTitleBar)
                .SetWindowPos(new Vector2(475, 250), UICond.Always)
                .SetWindowSize(new Vector2(50, 50), UICond.Always)
                .TextWrapped(true, $"Host hat die Lobby beendet. Offline zu Ende spielen?")
                .Button(true, "Offline spielen")
                .Button(true, "Partie beenden")
                .End();

            if ((bool) game_.UIBuilder.ElementValues["Offline spielen"])
                ServerDisconnected = false;

            if ((bool) game_.UIBuilder.ElementValues["Partie beenden"])
            {
                ServerDisconnected = false;
                game_.BackToMainMenu();
            }
        }


    }
}
