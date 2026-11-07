using UnityEngine;

namespace DebugMod.Helpers;

public static partial class VisualMaskHelper
{
    private static partial bool IsGameSpecificMask(GameObject go) => go.GetComponent<MaskerBase>() || go.GetComponent<OverMaskerBase>();
}
