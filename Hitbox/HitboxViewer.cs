using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DebugMod.Hitbox;

public class HitboxViewer
{
    public static int State { get; private set; }
    private HitboxRender hitboxRender;

    public void Load()
    {
        State = DebugMod.settings.ShowHitBoxes;
        Unload();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += AddSceneHitboxes;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += CreateHitboxRender;


        ModHooks.ColliderCreateHook += UpdateHitboxRender;

        CreateHitboxRender();
    }

    public void Unload()
    {
        State = DebugMod.settings.ShowHitBoxes;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= AddSceneHitboxes;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= CreateHitboxRender;

        ModHooks.ColliderCreateHook -= UpdateHitboxRender;
        DestroyHitboxRender();
    }
    
    // Order on room transition:
    //   sceneLoaded Greymoor_08 (skipped)
    //   activeSceneChanged Greymoor_08
    //   (later) sceneLoaded Greymoor_08_boss
    private void AddSceneHitboxes(Scene scene, LoadSceneMode mode)
    {
        if (hitboxRender == null || scene.name == GameManager.instance.nextSceneName) return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            hitboxRender.UpdateHitbox(root);
        }
    }

    private void CreateHitboxRender(Scene current, Scene next) => CreateHitboxRender();

    private void CreateHitboxRender()
    {
        DestroyHitboxRender();
        if (GameManager.instance.IsGameplayScene())
        {
            hitboxRender = new GameObject().AddComponent<HitboxRender>();
        }
    }

    private void DestroyHitboxRender()
    {
        if (hitboxRender != null)
        {
            Object.Destroy(hitboxRender);
            hitboxRender = null;
        }
    }

    private void UpdateHitboxRender(GameObject go)
    {
        if (hitboxRender != null)
        {
            hitboxRender.UpdateHitbox(go);
        }
    }
}