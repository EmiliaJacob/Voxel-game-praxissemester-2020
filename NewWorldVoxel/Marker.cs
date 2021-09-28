namespace NewWorldVoxel
{
    class Marker
    {
        private static Game _game = Game.GetInstance();

        public static int XCoordinate;
        public static int ZCoordinate;
       
        public static void MoveMarkerUp(string direction)
        {
            Objects.DeleteObject($"marker x: {XCoordinate} z: {ZCoordinate}");

            if (direction == "front")
                if (ZCoordinate < _game.ZChunks / 2 - 1)
                    ZCoordinate += 1;

            if (direction == "right")
                if (XCoordinate < _game.XChunks / 2 - 1)
                    XCoordinate += 1;

            if (direction == "behind")
                if (ZCoordinate > 0)
                    ZCoordinate -= 1;

            if (direction == "left")
                if (XCoordinate > 0)
                    XCoordinate -= 1;

            Objects.CreateObjectFromAnimationSet(XCoordinate, ZCoordinate, "marker", "Marker");
            Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {XCoordinate} {ZCoordinate}", 1);
        }

        public static void MoveMarkerDown(string direction)
        {
            Objects.DeleteObject($"marker x: {XCoordinate} z: {ZCoordinate}");

            if (direction == "front")
                if (ZCoordinate > 0)
                    ZCoordinate -= 1;

            if (direction == "right")
                if (XCoordinate > 0)
                    XCoordinate -= 1;

            if (direction == "behind")
                if (ZCoordinate < _game.ZChunks / 2 - 1)
                    ZCoordinate += 1;

            if (direction == "left")
                if (XCoordinate < _game.XChunks / 2 - 1)
                    XCoordinate += 1;

            Objects.CreateObjectFromAnimationSet(XCoordinate, ZCoordinate, "marker", "Marker");
            Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {XCoordinate} {ZCoordinate}", 1);
        }

        public static void MoveMarkerLeft(string direction)
        {
            Objects.DeleteObject($"marker x: {XCoordinate} z: {ZCoordinate}");

            if (direction == "front")
                if (XCoordinate < _game.XChunks / 2 - 1)
                    XCoordinate += 1;

            if (direction == "right")
                if (ZCoordinate > 0)
                    ZCoordinate -= 1;

            if (direction == "behind")
                if (XCoordinate > 0)
                    XCoordinate -= 1;

            if (direction == "left")
                if (ZCoordinate < _game.ZChunks / 2 - 1)
                    ZCoordinate += 1;

            Objects.CreateObjectFromAnimationSet(XCoordinate, ZCoordinate, "marker", "Marker");
            Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {XCoordinate} {ZCoordinate}", 1);
        }

        public static void MoveMarkerRight(string direction)
        {
            Objects.DeleteObject($"marker x: {XCoordinate} z: {ZCoordinate}");

            if (direction == "front")
                if (XCoordinate > 0)
                    XCoordinate -= 1;

            if (direction == "right")
                if (ZCoordinate < _game.ZChunks / 2 - 1)
                    ZCoordinate += 1;

            if (direction == "behind")
                if (XCoordinate < _game.XChunks / 2 - 1)
                    XCoordinate += 1;

            if (direction == "left")
                if (ZCoordinate > 0)
                    ZCoordinate -= 1;

            Objects.CreateObjectFromAnimationSet(XCoordinate, ZCoordinate, "marker", "Marker");
            Network.GetInstance().SendMessage($"{Network.GetInstance().GetOwnId()} {XCoordinate} {ZCoordinate}", 1);
        }
    }
}
