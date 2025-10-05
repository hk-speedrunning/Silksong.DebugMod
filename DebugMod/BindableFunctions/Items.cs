namespace DebugMod
{
    public static partial class BindableFunctions
    {
        /*
        [BindableMethod(name = "Give Pale Ore", category = "Consumables")]
        public static void GivePaleOre()
        {
            PlayerData.instance.ore = 6;
            Console.AddLine("Set player pale ore to 6");
        }

        [BindableMethod(name = "Give Simple Keys", category = "Consumables")]
        public static void GiveSimpleKey()
        {
            PlayerData.instance.simpleKeys = 3;
            Console.AddLine("Set player simple keys to 3");
        }

        // slab keys

        // white key, surgeon's key, architect's key, diving bell key, craw summons

        // melodies

        // memory hearts

        // everbloom

        [BindableMethod(name = "Give Map & Quill", category = "Items")]
        public static void ToggleMapQuill()
        {
            if (!PlayerData.instance.hasQuill || !PlayerData.instance.hasMap)
            {
                PlayerData.instance.hasQuill = true;
                PlayerData.instance.hasMap = true;
                PlayerData.instance.mapDirtmouth = true;
                Console.AddLine("Giving player map & quill");
            }
            else
            {
                PlayerData.instance.hasQuill = false;
                PlayerData.instance.hasMap = false;
                Console.AddLine("Taking away map & quill");
            }
        }

        [BindableMethod(name = "Give All Maps", category = "Consumables")]
        public static void GiveAllMaps()
        {
            PlayerData.instance.hasMap = true;
            PlayerData.instance.mapAllRooms = true;
            PlayerData.instance.mapCrossroads = true;
            PlayerData.instance.mapGreenpath = true;
            PlayerData.instance.mapFogCanyon = true;
            PlayerData.instance.mapRoyalGardens = true;
            PlayerData.instance.mapFungalWastes = true;
            PlayerData.instance.mapCity = true;
            PlayerData.instance.mapWaterways = true;
            PlayerData.instance.mapMines = true;
            PlayerData.instance.mapDeepnest = true;
            PlayerData.instance.mapCliffs = true;
            PlayerData.instance.mapOutskirts = true;
            PlayerData.instance.mapRestingGrounds = true;
            PlayerData.instance.mapAbyss = true;
        }

        [BindableMethod(name = "Open All Stags", category = "Items")]
        public static void AllStags()
        {
            PlayerData playerData = PlayerData.instance;
            playerData.SetBool("openedTown",true);
            playerData.SetBool("openedTownBuilding", true);
            playerData.SetBool("openedCrossroads", true);
            playerData.SetBool("openedGreenpath", true);
            playerData.SetBool("openedRuins1", true);
            playerData.SetBool("openedRuins2", true);
            playerData.SetBool("openedFungalWastes", true);
            playerData.SetBool("openedRoyalGardens", true);
            playerData.SetBool("openedRestingGrounds", true);
            playerData.SetBool("openedDeepnest", true);
            playerData.SetBool("openedStagNest", true);
            playerData.SetBool("openedHiddenStation", true);
            playerData.SetBool("gladeDoorOpened", true);
            playerData.SetBool("troupeInTown", true);
            
            Console.AddLine("Unlocked all stags");
        }
        */
    }
}