using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NewWorldVoxel
{
    class Objects
    {
        private static Game game_ = Game.GetInstance();
        private static List<string> _deletionQueue = new List<string>();

        public static int XStart { get; set; }
        public static int ZStart { get; set; }

        public static List<string> NextObjectsToGo = new List<string>();
        public static List<(string objectName, int speed)> RotatingObjects = new List<(string objectName, int speed)>();
        public static List<(int x, int z)> StartFields = new List<(int x, int z)>();
        public static Dictionary<(int, int), string> SpecialFields = new Dictionary<(int, int), string>();
        public static Dictionary<string, (int x, int z)> ClientStartFields = new Dictionary<string, (int x, int z)>();

        public static void GetRandomStartField()
        {
            Marker.ZCoordinate = ZStart = new Random(DateTime.Now.Second).Next(0, game_.XChunks / 2);
            Marker.XCoordinate = XStart = new Random(DateTime.Now.Millisecond).Next(0, game_.ZChunks / 2);

            CreateObjectFromAnimationSet(Marker.XCoordinate, Marker.ZCoordinate, "start", "Start");

            StartFields.Add((XStart, ZStart));
        }

        public static void GetRandomStartFields(string client)
        {
            bool isValid = false;

            while (!isValid)
            {
                int counter = 0;

                XStart = new Random(DateTime.Now.Second).Next(0, game_.XChunks / 2);
                ZStart = new Random(DateTime.Now.Millisecond).Next(0, game_.ZChunks / 2);

                if(client == Network.GetInstance().HostName)
                    isValid = true; 
                
                foreach(var position in StartFields)
                {
                        double xDiff = (XStart - position.x);
                        double zDiff = (ZStart - position.z);

                        double dist = Math.Sqrt(xDiff*xDiff + zDiff*zDiff);

                        if (dist < 5)
                            break;
                        else
                            counter++;

                        if (counter == StartFields.Count)
                            isValid = true;
                }              
            }

            StartFields.Add((XStart, ZStart));
            ClientStartFields.Add(client, (XStart, ZStart));
        }

        public static void CreateObjectFromAnimationSet(int x, int z, string objectName, string animationSet)
        {
            game_.ObjectControl.CreateObjectFromAnimationSet($"{objectName} x: {x} z: {z}", 
                new Vector3(x * game_.Radius.X * 2 + game_.Radius.X, 0, z * game_.Radius.Z * 2 + game_.Radius.Z), animationSet);

            game_.AnimationControl.SetAnimationTempo($"{objectName} x: {x} z: {z}", "Idle", 1000);
        }

        public static void CreateMapDesign(int rand, int x, int z)
        {
            if (CheckIfObjectNameExists("start", x, z)) return;

                if (rand <= 50)
                    CreateObjectFromAnimationSet(x, z, "plain", GetRandomPlainModelName(new Random(rand).Next(0, 100)));
                else if (rand > 50 && rand <= 85)
                    CreateObjectFromAnimationSet(x, z, "plain", GetRandomSpecialFieldName(new Random(rand).Next(0, 100), x, z));
                else if (rand > 85 && rand <= 100)
                {
                    if (StartFields.Contains((x, z)) || StartFields.Contains((x + 1, z)) || StartFields.Contains((x - 1, z)) || StartFields.Contains((x, z + 1)) || StartFields.Contains((x, z - 1)))
                        CreateObjectFromAnimationSet(x, z, "plain", GetRandomPlainModelName(new Random(rand).Next(0, 100)));
                    else
                        CreateRandomUnusableField(x, z, new Random(rand).Next(0, 100));
                }
                
        }

        public static string GetRandomPlainModelName(int rand)
        {
            if (rand < 20)
                return "RegPlain0";
            else if (rand < 40)
                return "RegPlain1";
            else if (rand < 60)
                return "RegPlain2";
            else if (rand < 80)
                return "RegPlain3";
            else
                return "RegPlain4";
        }

        private static string GetRandomSpecialFieldName(int rand, int x, int z) 
        {
            if (rand < 30)
            {
                SpecialFields[(x, z)] = "food";
                return "FoodPlain";
            }
            else if (rand < 60)
            {
                SpecialFields[(x, z)] = "oil";
                return "OilPlain";
            }
            else if (rand < 85)
            {
                SpecialFields[(x, z)] = "metal";
                return "MetalPlain";
            }
            else
            {
                SpecialFields[(x, z)] = "crystal";
                return "CrystalPlain";
            }
        }

        private static void CreateRandomUnusableField(int x, int z, int rand)
        {
            if (rand < 30)
                CreateObjectFromAnimationSet(x, z, "unusable", "Unusable0");
            else if (rand < 60)
                CreateObjectFromAnimationSet(x, z, "unusable", "Unusable1");
            else if (rand < 85)
                CreateObjectFromAnimationSet(x, z, "unusable", "Unusable2");
            else
            {
                CreateObjectFromAnimationSet(x, z, "unusable", "VortexBase");
                CreateObjectFromAnimationSet(x, z, "unusableExtra", "VortexWater");
                RotatingObjects.Add(($"unusableExtra x: {x} z: {z}", 3)); // TODO: Unusable abfragen genauer gestalten
            }
        }

        public static bool CheckIfObjectNameExists(string firstWord, int x, int z)
        {
            string objectName = $"{firstWord} x: {x} z: {z}";
            return game_.ObjectControl.ObjectNames.Any(name => name == objectName);
        }

        public static bool CheckForMachineField(int x, int z) => // TODO:Durch andere MaschinenMethode ersetzen
            CheckIfObjectNameExists("food", x, z)
            || CheckIfObjectNameExists("oil", x, z)
            || CheckIfObjectNameExists("metal", x, z)
            || CheckIfObjectNameExists("crystal", x, z)
            || CheckIfObjectNameExists("spProdFood", x, z)
            || CheckIfObjectNameExists("spProdOil", x, z)
            || CheckIfObjectNameExists("spProdMetal", x, z)
            || CheckIfObjectNameExists("spProdCrystal", x, z);

        public static bool CheckForPlainField(int x, int z)
        {
            bool plainFieldThere = false;

            foreach (var objectName in game_.ObjectControl.ObjectNames)
            {
                if (objectName == $"start x: {x} z: {z}")
                    return false;

                if (objectName.Split(' ')[0] != "marker" && objectName.Split(' ')[0] != "start" && objectName.Split(' ')[0] != "unusable" && objectName.Split(' ')[0] != "unusableExtra")
                    if (Int32.Parse(objectName.Split(' ')[2]) == x && Int32.Parse(objectName.Split(' ')[4]) == z)
                        plainFieldThere = !plainFieldThere; // Es kann maximal 2 Objekte an dieser Position mit Koordinaten im Namen geben
            }

            return plainFieldThere;
        }

        public static void ChangeAnimation(string objectName, string animationType, bool isLooped)
        {
            game_.AnimationControl.ChangeAnimation(objectName, animationType, isLooped);
        }

        public static void SetAnimationTempo(string objectName, string animationType, uint ticksPerFrame)
        {
            game_.AnimationControl.SetAnimationTempo(objectName, animationType, ticksPerFrame);
        }

        public static string GetCurrentAnimationType(string objectName)
        {
            return game_.AnimationControl.GetCurrentAnimationType(objectName);
        }

        public static bool DeleteObject(string objectName)
        {
            try
            {
                game_.ObjectControl.DeleteObject(objectName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //public static void ExchangeModel(int x, int z, string newName, string newModel)
        //{
        //    string oldModel = GetObjectNameByMarkerPos(x, z);
        //    DeleteObject(oldModel);

        //    if (CheckIfObjectNameExists("plain", x, z))
        //        DeleteObject($"plain x: {x} z: {z}");

        //    CreateObjectFromAnimationSet(x, z, $"{newName} x: {x} z:{z}", $"{newModel}");
        //}

        //private static string GetObjectNameByMarkerPos(int x, int z)
        //{
        //    foreach (var objectName in game_.ObjectControl.ObjectNames)
        //    {
        //        if (Int32.Parse(objectName.Split(' ')[2]) == x)
        //            if (Int32.Parse(objectName.Split(' ')[4]) == z)
        //                if (CheckIfObjectNameExists("buyable", x, z))
        //                    return $"buyable x: {x} z: {z}";
        //                else
        //                    return objectName;
        //    }

        //    return "not found";
        //}

        public static void DeleteObjectAfterDestruction(Object sender, EventArgs e) // TODO: Für Maschinen erweitern
        {
            _deletionQueue.AddRange(NextObjectsToGo);

            foreach (var item in _deletionQueue)
            {
                if (GetCurrentAnimationType(item) != "Destruction")
                {
                    DeleteObject(item);
                    HighlightFields.BuyableLandAfterSale(int.Parse(item.Split(' ')[2]), int.Parse(item.Split(' ')[4]));
                    NextObjectsToGo.Remove(item);
                }
            }

            _deletionQueue.Clear();
        }

        public static string GetMachineType(int x, int z)
        {
            foreach (var objectName in game_.ObjectControl.ObjectNames)
            {
                if(objectName.Split(' ')[2] == x.ToString() && objectName.Split(' ')[4] == z.ToString())
                    switch (objectName.Split(' ')[0])
                    {
                        case "food":
                            return $"food";
                        case "oil":
                            return $"oil";
                        case "metal":
                            return $"metal";
                        case "crystal":
                            return $"crystal";
                    }
            }
            return "none";
        }

        public static void DeleteOldMarker(string markerModel)
        {           
            foreach (var item in game_.ObjectControl.ObjectNames)
                if(item.Split(' ')[0] == markerModel)
                    DeleteObject(item);
        }

        public static void RotateObjects()
        {
            foreach (var (objectName, speed) in RotatingObjects)
                game_.ObjectControl.RotateObject(objectName, speed);
        }
    }
}
