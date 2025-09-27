using System;

namespace DebugMod
{
    public static class RoomSpecific
    {
        //This class is intended to recreate some scenarios, with more accuracy than that of the savestate class. 
        //This should be eventually included to compatible with savestates, stored in the same location for easier access.

        //TODO: Add functionality for checking ALL room specifics :(
        internal static (string value, int index) SaveRoomSpecific(string scene)
        {
            scene = scene.ToLower();
            //insert room specifics here
            return ("0", 0);
        }
        internal static void DoRoomSpecific(string scene, string options, int specialIndex)//index currently used for panth functionality (options is the sequencer, index is boss index, this cant be done by iteration because bench rooms repeat)
        {
            // caps in scene names change across versions
            int legacyOptions = 0;
            scene = scene.ToLower();
            
            try 
            {
                legacyOptions = int.Parse(options); 
            }
            catch (Exception e)
            {
                Console.AddLine("Invalid Room Specific: \n" + e);
            }

            switch (scene)
            {
                // TODO: add cases
                default:
                    Console.AddLine("No Room Specific Function Found In: " + scene);
                    break;
            }
        }
    }
}
