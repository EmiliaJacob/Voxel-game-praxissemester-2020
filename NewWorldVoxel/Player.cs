using System.Collections.Generic;

namespace NewWorldVoxel
{
    class Player
    {
        public int Money { get; set; } 
        public int Land { get; set; }
        public int Citizens { get; set; }
        public int UnassignedCitizens { get; set; }
        public int EmptyFreeHousings { get; set; }
        public int ResearchFacility { get; set; }
        public int MarketHall { get; set; }
        public Dictionary<string, int> Resources { get; private set; }
        public Dictionary<string, int > Machines { get; private set; }
        public Dictionary<string, int > SpMachines { get; private set; }
        public List<string> ResourceList { get; private set; }

        public void SetPlayerResources(int money, int citizens, int initialHousings, int food, int oil, int metal, int crystal)
        {
            Money = money;
            Land = 0;
            Citizens = citizens;
            UnassignedCitizens = Citizens;
            EmptyFreeHousings = initialHousings / 5;
            ResearchFacility = 0;
            MarketHall = 0;

            Resources = new Dictionary<string, int>
            {
                { "food", food },
                { "oil", oil },
                { "metal", metal },
                { "crystal", crystal }
            };

            Machines = new Dictionary<string, int>
            {
                { "food", 0 },
                { "oil", 0 },
                { "metal", 0 },
                { "crystal", 0 }
            };

            SpMachines = new Dictionary<string, int>
            {
                { "food", 0 },
                { "oil", 0 },
                { "metal", 0 },
                { "crystal", 0 }
            };

            ResourceList = new List<string>();
            foreach (var key in Resources.Keys)
                ResourceList.Add(key);
        }
    }
}
