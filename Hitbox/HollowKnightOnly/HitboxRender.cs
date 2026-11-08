using UnityEngine;

namespace DebugMod.Hitbox;

public partial class HitboxRender
{
    private static partial bool CategorizeGameSpecific(Collider2D collider2D, out HitboxType result)
    {
        result = null;
        return false;
    }

    private static partial bool CategorizeGameSpecificDetailed(Collider2D collider2D, out HitboxType result)
    {
        result = null;
        return false;
    }
}
