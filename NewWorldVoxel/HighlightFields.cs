namespace NewWorldVoxel
{
    class HighlightFields
    {
        private static Game _game = Game.GetInstance();

        public static void BuyableLandAfterPurchase(int x, int z)
        {
            if(Objects.CheckIfObjectNameExists("buyable", x, z))
                Objects.DeleteObject($"buyable x: {x} z: {z}");

            //Überprüfung der rechten Landfläche
            if (x > 0) // Checkt ob Ausgewählte Spielfläche am Rand liegt
                if (!Objects.CheckIfObjectNameExists("buyable", x-1,z)) // Checkt ob bereits eine 'kaufbar' - Markierung existiert
                    if (Objects.CheckForPlainField(x - 1, z))  // Checkt ob an dieser Stelle ein braches Land existiert
                    {
                        //Vector3 position = (Vector3) game.ObjectControl.GetObjectPosition(game.GetObjectNameByMarkerPos(xMarkerPos - 1, zMarkerPos)); Anschauungsobjekt für alten Workarround
                        Objects.CreateObjectFromAnimationSet(x-1 ,z,"buyable","Buyable");
                    }

            //Überprüfung der linken Landfläche
            if (x < _game.XChunks / 2 - 1)
                if (!Objects.CheckIfObjectNameExists("buyable", x + 1, z))
                    if (Objects.CheckForPlainField(x + 1, z))
                        Objects.CreateObjectFromAnimationSet(x + 1, z, "buyable", "Buyable");
        

            //Überprüfung der oberen Landfläche
            if (z < _game.ZChunks / 2 - 1)
                if (!Objects.CheckIfObjectNameExists("buyable", x, z + 1))
                    if (Objects.CheckForPlainField(x , z + 1))
                        Objects.CreateObjectFromAnimationSet(x, z + 1, "buyable", "Buyable");

            //Überprüfung der unteren Landfläche
            if (z > 0)
                if (!Objects.CheckIfObjectNameExists("buyable", x, z - 1))
                    if (Objects.CheckForPlainField(x, z - 1))
                        Objects.CreateObjectFromAnimationSet(x, z - 1, "buyable", "Buyable");

            StartAndSynchAmSetAnimations();
        }

        public static void BuyableLandAfterSale(int x, int z)
        {

            //Überprüfung ob die zu verkaufende Fläche and eine bebaute Landfläche anschließt 
            if (CheckForNonPlainField((x + 1, z), (x - 1, z), (x, z + 1))
                || Objects.CheckIfObjectNameExists("city", x, z - 1)
                || Objects.CheckIfObjectNameExists("start", x, z - 1)
                || Objects.CheckForMachineField(x, z - 1))
            { 
                Objects.CreateObjectFromAnimationSet(x, z, "buyable", "Buyable");
            }

            //Überprüfung der rechten Landfläche
            if (x > 0)  //same
                if (Objects.CheckIfObjectNameExists("buyable", x - 1, z)) 
                    if (! CheckForNonPlainField((x - 1, z + 1), (x - 1, z - 1), (x - 2, z))) 
                        _game.ObjectControl.DeleteObject($"buyable x: {x - 1} z: {z}");

            //Überprüfung der linken Landfläche
            if (x < _game.XChunks / 2 - 1)
                if (Objects.CheckIfObjectNameExists("buyable", x + 1, z))
                    if (!CheckForNonPlainField((x + 1, z + 1), (x + 1, z - 1), (x + 2, z)))
                        _game.ObjectControl.DeleteObject($"buyable x: {x + 1} z: {z}");

            //Überprüfung der oberen Landfläche
            if (z < _game.ZChunks / 2 - 1)
                if (Objects.CheckIfObjectNameExists("buyable", x, z + 1))
                    if (!CheckForNonPlainField((x + 1, z + 1), (x - 1, z + 1), (x, z + 2)))
                        _game.ObjectControl.DeleteObject($"buyable x: {x} z: {z + 1}"); //TODO: Durch Objects Methode ersetzen

            if (z > 0)
                if (Objects.CheckIfObjectNameExists("buyable", x, z - 1))
                    if (!CheckForNonPlainField((x + 1, z - 1), (x - 1, z - 1), (x, z - 2)))
                        _game.ObjectControl.DeleteObject($"buyable x: {x} z: {z - 1}");
            
            StartAndSynchAmSetAnimations();
        }

        private static bool CheckForNonPlainField((int,int) fieldOne, (int,int) fieldTwo, (int,int) fieldThree) //TODO: --> durch Komplement ersetzen --> && Verknüfung mit !buyable
        {
            return Objects.CheckIfObjectNameExists("city", fieldOne.Item1, fieldOne.Item2)
                   || Objects.CheckIfObjectNameExists("city", fieldTwo.Item1, fieldTwo.Item2)
                   || Objects.CheckIfObjectNameExists("city", fieldThree.Item1, fieldThree.Item2)
                   || Objects.CheckIfObjectNameExists("start", fieldOne.Item1, fieldOne.Item2)
                   || Objects.CheckIfObjectNameExists("start", fieldTwo.Item1, fieldTwo.Item2)
                   || Objects.CheckIfObjectNameExists("start", fieldThree.Item1, fieldThree.Item2)
                   || Objects.CheckForMachineField(fieldOne.Item1, fieldOne.Item2)
                   || Objects.CheckForMachineField(fieldTwo.Item1, fieldTwo.Item2)
                   || Objects.CheckForMachineField(fieldThree.Item1, fieldThree.Item2);
        }
        
        private static void StartAndSynchAmSetAnimations()
        {
            foreach (var name in _game.ObjectControl.ObjectNames)
            {
                if(name.Split(' ')[0] == "buyable")
                {
                    _game.AnimationControl.ChangeAnimation(name, "Reset", false);
                    _game.AnimationControl.ChangeAnimation(name, "Idle", true);
                    _game.AnimationControl.SetAnimationTempo(name, "Idle", 750);
                }
            }
        }
    }
}
