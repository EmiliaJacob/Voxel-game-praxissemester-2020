using System;
using System.Collections.Generic;

namespace NewWorldVoxel
{
    class Transactions 
    {
        private static Player _player = Game.GetInstance().ActivePlayer;

        private static int _birthrate = 3;
        private static int _birthRoundCounter = 0;
        private static int _actualOil = 0;

        public static int WareAmount = 0;
        public static int CitizensPerHousing= 5;
        public static int ProductionTeamSize = 3;

        public static Dictionary<string, int> ProductionMultiplier = new Dictionary<string, int>()
        {
            { "food", 10 },
            { "oil", 5 },
            { "metal", 2 },
            { "crystal", 1 }
        };

        public static Dictionary<string, int> SpecialProductionMultiplier = new Dictionary<string, int>()
        {
            { "food", 15 },
            { "oil", 7 },
            { "metal", 3 },
            { "crystal", 2 }
        };

        public static Dictionary<string, int> ProducedResources = new Dictionary<string, int>()
        {
            { "food", 0 },
            { "oil", 0 },
            { "metal", 0 },
            { "crystal", 0 }
        };
               
        public static void BuyLand(int x, int z) 
        {
            if (_player.Money >= Prices.Purchase["land"]) 
            {
                if (Objects.CheckIfObjectNameExists("buyable", x, z))
                {
                    CityConstruction(x,z);

                    HighlightFields.BuyableLandAfterPurchase(x, z);

                    _player.Money -= Prices.Purchase["land"];
                    _player.Land += 1;
                    _player.EmptyFreeHousings += 1;

                    Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {x} {z}", 2);
                }
            }
        }

        public static void CityConstruction(int x, int z)
        {
            Objects.DeleteObject($"plain x: {x} z: {z}");
            Objects.CreateObjectFromAnimationSet(x, z, "plain", GetBelongingEmptyField(x, z));
            Objects.CreateObjectFromAnimationSet( x, z, "city","City"); 
            Objects.ChangeAnimation($"city x: {x} z: {z}", "Construction", false);
        } 

        private static string GetBelongingEmptyField(int x, int z)
        {
            if (!Objects.SpecialFields.ContainsKey((x, z)))
                return "RegPlain2";
            else
            {
                if (Objects.SpecialFields[(x, z)] == "food")
                    return "FoodEmpty";
                else if (Objects.SpecialFields[(x, z)] == "oil")
                    return "OilEmpty";
                else if (Objects.SpecialFields[(x, z)] == "metal")
                    return "MetalEmpty";
                else
                    return "CrystalEmpty";
            }
        }

        public static void SellLand(int x, int z)
        {
            if (_player.Land > 0)
            {
                if (Objects.CheckIfObjectNameExists("city", x, z))
                {
                    CityDestruction(x, z);

                    _player.Money += Prices.Sale["land"];
                    _player.Land -= 1;
                    _player.EmptyFreeHousings -= 1;

                    Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {x} {z}", 3);
                }
            }
        }

        public static void CityDestruction(int x, int z) // ToDO: Methode entfernen
        {
            Objects.ChangeAnimation($"city x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"city x: {x} z: {z}");
            Objects.DeleteObject($"plain x: {x} z: {z}");
            Objects.CreateObjectFromAnimationSet(x, z, "plain", GetBelongingPlainField(x, z));
        }

        private static string GetBelongingPlainField(int x, int z)
        {
            if (!Objects.SpecialFields.ContainsKey((x, z)))
                return Objects.GetRandomPlainModelName(new Random().Next(0, 101));
            else
            {
                if (Objects.SpecialFields[(x, z)] == "food")
                    return "FoodPlain";
                else if (Objects.SpecialFields[(x, z)] == "oil")
                    return "OilPlain";
                else if (Objects.SpecialFields[(x, z)] == "metal")
                    return "MetalPlain";
                else
                    return "CrystalPlain";
            }
        }
       
        public static void AssignMachine(string machineType, string ModelName, int x, int z)
        {
            if (Objects.CheckIfObjectNameExists("city", x, z) && _player.Resources["metal"] >= Prices.Purchase["production"] && _player.UnassignedCitizens >= 3)
            {
                Objects.DeleteObject($"city x: {x} z: {z}");

                if (Objects.SpecialFields.ContainsKey((x, z)))
                {
                    if (Objects.SpecialFields[(x, z)] == machineType)
                        _player.SpMachines[machineType] += 1;
                    else
                        _player.Machines[machineType] += 1;
                }
                else
                    _player.Machines[machineType] += 1;

                Objects.CreateObjectFromAnimationSet(x, z, machineType, ModelName);
                Objects.ChangeAnimation($"{machineType} x: {x} z: {z}", "Construction", false);

                _player.Resources["metal"] -= Prices.Purchase["production"];
                _player.EmptyFreeHousings -= 1;
                _player.UnassignedCitizens -= ProductionTeamSize;

                Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {x} {z} {ModelName} {machineType}", 4);
            }
            else
                UiCollection.GameErrorActive = true;
        }
        
        public static void ExemptMachine(int x, int z)
        {
            if (Objects.GetMachineType(x,z) != "none")
            {
                string machineType = Objects.GetMachineType(x, z);

                if (Objects.SpecialFields.ContainsKey((x, z)))
                {
                    if (Objects.SpecialFields[(x, z)] == machineType)
                        _player.SpMachines[machineType] -= 1;
                }
                else
                    _player.Machines[machineType] -= 1;


                Objects.DeleteObject($"{machineType} x: {x} z: {z}");

                CityConstruction(x, z);

                _player.Resources["metal"] += Prices.Sale["production"];
                _player.EmptyFreeHousings += 1;
                _player.UnassignedCitizens += ProductionTeamSize;

                Network.GetInstance().SendMessage($"{x} {z} {machineType}", 5);
            }
        }

        public static void SellLandWithMachine(int x, int z)
        {
            if (_player.Land > 0)
            {
                string machineType = Objects.GetMachineType(x, z);

                if (Objects.SpecialFields.ContainsKey((x, z)))
                {
                    if (Objects.SpecialFields[(x, z)] == machineType)
                        _player.SpMachines[machineType] -= 1;
                }
                else
                    _player.Machines[machineType] -= 1;


                Objects.ChangeAnimation($"{machineType} x: {x} z: {z}", "Destruction", false);
                Objects.NextObjectsToGo.Add($"{machineType} x: {x} z: {z}");

                _player.Resources["metal"] += Prices.Sale["production"];
                _player.EmptyFreeHousings += 1;
                _player.UnassignedCitizens += ProductionTeamSize;

                _player.Money += Prices.Sale["land"];
                _player.Land -= 1;
                _player.EmptyFreeHousings -= 1;

                Network.GetInstance().SendMessage($"{machineType} {x} {z}", 11);
            }
        }

        public static void BuildResearchFacility(int x, int z) 
        {
            if (Objects.SpecialFields.ContainsKey((x, z))) return;

            if (Objects.CheckIfObjectNameExists("research", x, z))
            {
                Objects.DeleteObject($"research x: {x} z: {z}");

                CityConstruction(x, z);

                _player.ResearchFacility -= 1;
                _player.EmptyFreeHousings += 1;
                _player.Resources["metal"] += Prices.Sale["production"];

                Network.GetInstance().SendMessage($"{x} {z}", 8);
            }
            else if (_player.ResearchFacility < 3 && _player.Resources["metal"] >= Prices.Purchase["production"])
            {
                if (_player.Citizens - (1 + _player.ResearchFacility) * 15 >= 0)
                {
                    string researchModel = GetResearchModel();

                    Objects.DeleteObject($"city x: {x} z: {z}");
                    Objects.CreateObjectFromAnimationSet(x, z, "research", researchModel);
                    Objects.ChangeAnimation($"research x: {x} z: {z}", "Construction", false);

                    _player.ResearchFacility += 1;
                    _player.EmptyFreeHousings -= 1;
                    _player.Resources["metal"] -= Prices.Purchase["production"];

                    Network.GetInstance().SendMessage($"{x} {z} {researchModel}", 7);
                }
                else
                    UiCollection.GameErrorActive = true;
            }
        }

        private static string GetResearchModel()
        {
            int rand = new Random().Next(1, 4);

            if (rand == 1)
                return "Research0";
            else if (rand == 2)
                return "Research1";
            else
                return "Research2";
        }

        public static void SellLandWithResearchFacility(int x, int z)
        {

            Objects.ChangeAnimation($"research x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"research x: {x} z: {z}");

            _player.ResearchFacility -= 1;
            _player.Resources["metal"] += Prices.Sale["production"];

            Network.GetInstance().SendMessage($"{x} {z}", 12);
        }

        public static void BuildMarketHall(int x, int z)
        {
            if (Objects.SpecialFields.ContainsKey((x, z))) return;

            if (Objects.CheckIfObjectNameExists("marketHall", x, z))
            {
                Objects.DeleteObject($"marketHall x: {x} z: {z}");
                CityConstruction(x, z);
                Prices.RemoveDiscount();

                _player.EmptyFreeHousings += 1;
                _player.Resources["metal"] += Prices.Sale["production"];
                _player.MarketHall -= 1;

                Network.GetInstance().SendMessage($"{x} {z}", 10);
            }
            else if (_player.Resources["metal"] >= Prices.Purchase["production"] && _player.MarketHall < 1)
            {
                Objects.DeleteObject($"city x: {x} z: {z}");
                Objects.CreateObjectFromAnimationSet(x, z, "marketHall", "MarketHall");
                Objects.ChangeAnimation($"marketHall x: {x} z: {z}", "Construction", false);
                Prices.AddDiscount();

                _player.EmptyFreeHousings -= 1;
                _player.Resources["metal"] -= Prices.Purchase["production"];
                _player.MarketHall += 1;

                Network.GetInstance().SendMessage($"{x} {z}", 9);
            }
            else
                UiCollection.GameErrorActive = true;
        }

        public static void SellLandWithMarketHall(int x, int z)
        {
            Prices.RemoveDiscount();

            Objects.ChangeAnimation($"marketHall x: {x} z: {z}", "Destruction", false);
            Objects.NextObjectsToGo.Add($"marketHall x: {x} z: {z}");

            Network.GetInstance().SendMessage($"{x} {z}", 13);
        }

        public static void OnRoundTimerElapsed(object source, EventArgs e)
        {
            RessourceProduction();
            CheckResearch();
            ServeFood();
            CheckForEnoughHousing();
            GiveBirth();
        }

        private static void RessourceProduction()
        {
            ResetProducedResources();

            foreach (var key in _player.ResourceList)
            {
                ProduceResource(key, _player.SpMachines[key], SpecialProductionMultiplier[key]);
                ProduceResource(key, _player.Machines[key], ProductionMultiplier[key]);
            }

            AddProducedResources();
        }
        
        private static void ResetProducedResources()
        {
            _actualOil = _player.Resources["oil"];

            foreach (var key in _player.ResourceList)
                ProducedResources[key] = 0;
        }

        private static void ProduceResource(string resource, int machineCount, int prodMultiplier)
        {
            int workingCitizens = (_player.Citizens - _player.UnassignedCitizens) / 3;

            if (_actualOil >= machineCount && workingCitizens >= machineCount)
            {
                ProducedResources["oil"] -= machineCount;
                _actualOil -= machineCount;
                ProducedResources[resource] += machineCount * prodMultiplier;
            }
            else
            {
                int min = Math.Min(_actualOil, workingCitizens);
                ProducedResources["oil"] -= min;
                ProducedResources[resource] += min * prodMultiplier;
            }
        }

        private static void AddProducedResources()
        {
            foreach (var key in _player.ResourceList)
                _player.Resources[key] += ProducedResources[key];
        }

        private static void CheckResearch()
        {
            Research.CheckResearchRounds(_player);
        }

        private static void GiveBirth()
        {
            if (_birthRoundCounter == 2 && _player.Citizens >= 2)
            {
                _player.Citizens += _birthrate;
                _player.UnassignedCitizens += _birthrate;
                _birthRoundCounter = 0;
            }
            else
                _birthRoundCounter += 1;
        }

        private static void ServeFood()
        {
            if (_player.Resources["food"] >= _player.Citizens)
                _player.Resources["food"] -= _player.Citizens;
            else
            {
                _player.Citizens -= _player.Citizens - _player.Resources["food"];
                _player.UnassignedCitizens = 0;
                _player.Resources["food"] = 0;
            }
        }

        private static void CheckForEnoughHousing()
        {
            if (_player.EmptyFreeHousings * CitizensPerHousing < _player.Citizens)
            {
                int migrateCitizens = _player.Citizens - _player.EmptyFreeHousings * CitizensPerHousing;
                _player.Citizens -= migrateCitizens;
                _player.UnassignedCitizens -= migrateCitizens;
            }
        }

        public static void BuyWare(string chosenWare)
        {
            if (_player.Money >= WareAmount * Prices.Purchase[chosenWare])
            {
                _player.Money -= WareAmount * Prices.Purchase[chosenWare];
                _player.Resources[chosenWare] += WareAmount;
                WareAmount = 0;
            }
        }

        public static void SellWare(string chosenWare)
        {
            if (_player.Resources[chosenWare] >= WareAmount)
            {
                _player.Money += WareAmount * Prices.Sale[chosenWare];
                _player.Resources[chosenWare] -= WareAmount;
                WareAmount = 0;
            }
        }
    }
}
