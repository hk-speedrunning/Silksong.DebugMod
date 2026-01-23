using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DebugMod.Helpers;

public static class VisualMaskHelper
{
    internal static bool masksDisabled = false;
    internal static bool vignetteDisabled = false;

    private static bool reenableVignette = false;

    public static void ToggleAllMasks()
    {
        masksDisabled = !masksDisabled;
        if (masksDisabled)
        {
            DeactivateVisualMasks(Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None));
            DebugMod.LogConsole("Disabled all visual masks");
        }
        else
        {
            DebugMod.LogConsole("No longer disabling all visual masks; reload the room to see changes");
            // Cannot reactivate most visual masks because it's impractical to find all that should be active;
            // Need to manually reenable the vignette, but do not do so immediately for consistency
            reenableVignette = true;
        }
    }

    public static void ToggleVignette()
    {
        vignetteDisabled = !vignetteDisabled;
        if (vignetteDisabled)
        {
            DisableVignette(false);
            DebugMod.LogConsole("Disabled vignette");
        }
        else
        {
            DebugMod.LogConsole("Enabled vignette");
            DebugMod.HC.vignette.enabled = true;
            if (masksDisabled)
            {
                DebugMod.LogConsole("All visual masks were disabled; re-enabling");
                masksDisabled = false;
            }
        }
    }

    public static void OnSceneChange(Scene s)
    {
        if (masksDisabled)
        {
            // Delaying for 2f seems enough - wait for 3 just to be sure though.
            DelayInvoke(3, () => DeactivateVisualMasks(GetGameObjectsInScene(s)));
            return;
        }

        if (vignetteDisabled)
        {
            DelayInvoke(3, () => DisableVignette(false));
        }

        if (reenableVignette)
        {
            reenableVignette = false;

            // Should not wait before reactivating these
            foreach (Renderer r in DebugMod.HC.vignette.GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }
    }

    public static void DelayInvoke(int delay, Action method)
    {
        IEnumerator coro()
        {
            for (int i = 0; i < delay; i++)
            {
                yield return null;
            }
            method();
        }
        GameManager.instance.StartCoroutine(coro());
    }

    public static IEnumerable<GameObject> GetGameObjectsInScene(Scene s)
    {
        if (!s.IsValid())
        {
            yield break;
        }

        foreach (GameObject go in s.GetRootGameObjects())
        {
            yield return go;
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            {
                yield return t.gameObject;
            }
        }
    }


    /// <summary>
    /// Deactivate all renderers attached to objects that DebugMod recongises as being a visual mask.
    /// </summary>
    /// <param name="gos">The game objects to iterate over.</param>
    /// <returns>The number of renderers disabled.</returns>
    public static int DeactivateVisualMasks(IEnumerable<GameObject> gos)
    {
        int ctr = 0;

        bool IsMask(GameObject go)
        {
            if (go.GetComponent<MaskerBase>()) return true;
            if (go.GetComponent<OverMaskerBase>()) return true;
            if (go.GetComponent<MaskerBlackout>()) return true;
            if (go.GetComponent<BlurPlane>()) return true;
            if (go.name.Contains("black_fader")) return true;
            if (go.name.Contains("haze")) return true;
            //if (go.name.Contains("SceneBorder")) return true; <-- does this count as a visual mask?

            return false;
        }

        float knightZ = HeroController.instance.transform.position.z;
        foreach (GameObject go in gos)
        {
            if (IsMask(go))
            {
                go.SetActive(false);
                ctr++;
            }
        }

        DebugMod.LogConsole($"Deactivated {ctr} masks" + (HeroController.instance.vignette.enabled ? " and toggling vignette off" : string.Empty));

        // The vignette counts as a visual mask :)
        DisableVignette(true);

        return ctr;
    }

    /// <summary>
    /// Disable the Vignette, as well as all renderers in its children.
    /// </summary>
    /// <param name="includeChildren">If this is false, do not disable renderers in the vignette's children.</param>
    public static void DisableVignette(bool includeChildren = true)
    {
        DebugMod.HC.vignette.enabled = false;

        if (!includeChildren)
        {
            return;
        }

        // Not suitable for toggle vignette because not easily reversible
        foreach (Renderer r in DebugMod.HC.vignette.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }
}