using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod.SaveStates;

public partial class SaveState
{
    private partial IEnumerator PrepareSceneLoad(bool abortDeath)
    {
        // TODO(port): unverified if necessary
        yield return ScenePreloader.FinishPendingOperations();
        ScenePreloader.Cleanup();
    }

    private partial IEnumerator LoadDummyScene()
    {
        const string dummyScene = "Room_Mender_House";
        USceneManager.LoadSceneAsync(dummyScene);
        yield return new WaitUntil(() => USceneManager.GetActiveScene().name == dummyScene);
    }

    private partial IEnumerator LoadDupedScene(string sceneName)
    {
        yield return USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    private partial IEnumerator ResetGameSpecificState(bool loadSafely)
    {
        yield break;
    }

    private partial void GameSpecificSaveStateGlitchFixes() { }

    private partial void HUDFixes() { }
}
