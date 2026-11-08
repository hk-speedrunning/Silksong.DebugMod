using System.Collections;

namespace DebugMod.SaveStates;

public static partial class RoomSpecific
{
    internal static partial string SaveRoomSpecific(string scene);
    internal static partial void BackwardsCompat(string scene, ref string options);
    internal static partial IEnumerator DoRoomSpecific(string scene, string options);
    internal static partial IEnumerator DoGenericFixes(string scene);
}
