using UnityEngine;

namespace DebugMod;

public static partial class BindableFunctions
{
    private static partial void BreakCorpse()
    {
        int geo = PlayerData.instance.geoPool;
        PlayerData.instance.geoPool = 0;
        PlayerData.instance.shadeScene = "None";
        GameManager.instance.EndSoulLimiter();
        HeroController.instance.AddGeo(geo);
        FSMUtility.SendEventToGameObject(GameCameras.instance.soulOrbFSM.gameObject, "SOUL LIMITER DOWN", true);

        GameObject shade = GameObject.Find("Hollow Shade(Clone)");
        if (shade)
        {
            shade.LocateMyFSM("Shade Control").SendEvent("ZERO HP");
        }
    }

    private static partial void ClearSceneData(string sceneName)
    {
        SceneData.instance.persistentBoolItems.RemoveAll(data => data.sceneName == sceneName);
        SceneData.instance.persistentIntItems.RemoveAll(data => data.sceneName == sceneName);
        SceneData.instance.geoRocks.RemoveAll(data => data.sceneName == sceneName);
    }
}
