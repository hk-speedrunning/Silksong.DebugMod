using HutongGames.PlayMaker;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMod.Helpers;

internal static class Utils
{
    internal static PlayMakerFSM FindFSM(string goName, string fsmName)
    {
        return PlayMakerFSM.FindFsmOnGameObject(GameObject.Find(goName), fsmName);
    }

    internal static GameObject FindChildObject(this GameObject parent, string name)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

    internal static GameObject FindGameObjectByPath(string path)
    {
        string[] parts = path.Split('/');

        GameObject go = SceneManager.GetActiveScene().GetRootGameObjects().First(x => x.name == parts[0]);

        for (int i = 1; i < parts.Length; i++)
        {
            go = go.FindChildObject(parts[i]);
        }

        return go;
    }
#nullable enable
    
    
    internal static FsmState? GetState(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmStates.FirstOrDefault(state => state.Name == name);
    }

    internal static PlayMakerFSM? GetTemplatedFsm(this GameObject go, string name)
    {
        return go.GetComponents<PlayMakerFSM>()?.FirstOrDefault(fsm => fsm.FsmTemplate && fsm.FsmTemplate.name == name);
    }

    internal static PlayMakerFSM? GetNamedFsm(this GameObject go, string name)
    {
        return go.GetComponents<PlayMakerFSM>()?.FirstOrDefault(fsm => fsm.FsmTemplate && fsm.FsmTemplate.name == name);
    }

    internal static T? GetFirstActionOrDefault<T>(this FsmState state) where T : FsmStateAction
    {
        return state.Actions.FirstOrDefault(act => act is T) as T;
    }
}
