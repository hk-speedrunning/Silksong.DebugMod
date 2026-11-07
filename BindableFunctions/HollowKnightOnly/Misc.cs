namespace DebugMod;

public static partial class BindableFunctions
{
    private static partial void ClearSceneData(string sceneName)
    {
        SceneData.instance.persistentBoolItems.RemoveAll(data => data.sceneName == sceneName);
        SceneData.instance.persistentIntItems.RemoveAll(data => data.sceneName == sceneName);
        SceneData.instance.geoRocks.RemoveAll(data => data.sceneName == sceneName);
    }
}
