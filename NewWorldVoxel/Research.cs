using System;
using System.Collections.Generic;

namespace NewWorldVoxel
{
    class Research
    {
        private static Random _random = new Random();

        public static string ResearchSubject = String.Empty;
        public static bool ResearchIsActive = false;
        public static int ActiveResearchTime = 0;

        public static Dictionary<string, int> ResearchRounds = new Dictionary<string, int>()
        {
            { "food", 4 },
            { "oil", 4 },
            { "metal", 4 },
            { "crystal", 4 },
        };

        public static Dictionary<string, int> ResearchRank = new Dictionary<string, int>()
        {
            { "food", 1 },
            { "oil", 1 },
            { "metal", 1 },
            { "crystal", 1 }
        };

        public static Dictionary<string, int> ResearchCost = new Dictionary<string, int>()
        {
            { "food", 5 },
            { "oil", 5 },
            { "metal", 5 },
            { "crystal", 5 }
        };

        public static Dictionary<string, int> ResearchProductionMultiplier = new Dictionary<string, int>()
        {
            { "food", 2 },
            { "oil", 2 },
            { "metal", 2 },
            { "crystal", 1 }
        };

        public static void CheckPlayerResources(Player player, string resource)
        {
            if (player.ResearchFacility == 3)
                ResearchCost[resource] = (int)(ResearchCost[resource] * 0.85);

            if (player.Resources["crystal"] >= ResearchCost[resource])
            {
                player.Resources["crystal"] -= ResearchCost[resource];
                StartResearch(player, resource);
            }
        }

        private static void StartResearch(Player player, string resource)
        {
            if (ResearchIsActive) return;

            if (ResearchRank[resource] == 5) return;

            if (player.UnassignedCitizens >= 4)
            {
                ResearchIsActive = true;
                ResearchSubject = resource;

                if (player.ResearchFacility >= 2)
                    ResearchRounds[resource] -= (int)(ResearchRounds[resource] * 0.25);

                ActiveResearchTime = ResearchRounds[resource];
                player.UnassignedCitizens -= 4;
            }
        }

        public static void CheckResearchRounds(Player player)
        {
            if (!ResearchIsActive) return;

            ActiveResearchTime -= 1;

            if (ActiveResearchTime == 0)
                EndResearch(player);
        }

        public static void EndResearch(Player player)
        {
            ResearchIsActive = false;
            ResearchRounds[ResearchSubject] += 2;
            ResearchRank[ResearchSubject] += 1;
            ResearchCost[ResearchSubject] += ResearchCost[ResearchSubject];

            Transactions.ProductionMultiplier[ResearchSubject] += ResearchProductionMultiplier[ResearchSubject];
            Transactions.SpecialProductionMultiplier[ResearchSubject] += ResearchProductionMultiplier[ResearchSubject];
            player.UnassignedCitizens += 5;
            ResearchSubject = String.Empty;
        }
    }
}
