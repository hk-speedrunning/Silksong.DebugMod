using UnityEngine;

namespace DebugMod.Hitbox;

public partial class HitboxRender
{
    private static partial bool CategorizeGameSpecific(Collider2D collider2D, out HitboxType result)
    {
        if (collider2D.GetComponent<CogMultiHitter>()
            || (collider2D.GetComponent<TriggerEnterEvent>() && collider2D.transform.parent && collider2D.transform.parent.GetComponent<RangeAttackGroup>())) // This is a messed up heuristic but it works for now. Workaround for sand worm regions.
        {
            result = HitboxType.Enemy;
            return true;
        }

        if (collider2D.GetComponent<HarpoonHook>() || collider2D.GetComponent<Lever_tk2d>() || collider2D.GetComponent<Lever>())
        {
            result = HitboxType.Trigger;
            return true;
        }

        result = null;
        return false;
    }

    private static partial bool CategorizeGameSpecificDetailed(Collider2D collider2D, out HitboxType result)
    {
        var noClamber = collider2D.GetComponent<NoClamberRegion>();
        if (noClamber && noClamber.enabled)
        {
            result = HitboxType.NonMantleRegion;
            return true;
        }

        result = null;
        return false;
    }
}
