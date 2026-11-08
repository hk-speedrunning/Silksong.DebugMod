using UnityEngine;

namespace DebugMod.Helpers;

public static partial class HudHelper
{
    public static partial void RefreshMasks()
    {
        Transform hudCanvas = GameCameras.instance.hudCanvasSlideOut.transform;
        Transform health = hudCanvas.Find("Health");

        foreach (PlayMakerFSM fsm in health.GetComponentsInChildren<PlayMakerFSM>())
        {
            if (fsm.FsmName == "health_display")
            {
                int healthNumber = fsm.FsmVariables.GetFsmInt("Health Number").Value;
                // Going straight to "Empty"/"Idle" leaves the wrong sprite visible
                bool showsFull = fsm.FsmVariables.GetFsmGameObject("Idle Sprite").Value.GetComponent<MeshRenderer>().enabled;
                bool isFull = PlayerData.instance.health >= healthNumber;
                if (healthNumber > PlayerData.instance.maxHealth)
                {
                    fsm.SetState("Inactive");
                }
                else if (fsm.ActiveStateName == "Inactive")
                {
                    fsm.SetState("Idle");
                }
                else if (!isFull && showsFull)
                {
                    fsm.SetState("Break?");
                }
                else if (isFull && fsm.ActiveStateName != "Idle")
                {
                    fsm.SetState("Heal?");
                }
            }
        }
    }

    public static partial void RefreshMana()
    {
        GameCameras.instance.soulOrbFSM.SendEvent("MP SET");

        Transform vessels = GameCameras.instance.soulOrbFSM.transform.Find("Vessels");
        foreach (PlayMakerFSM fsm in vessels.GetComponentsInChildren<PlayMakerFSM>())
        {
            if (fsm.FsmName == "vessel_orb")
            {
                bool isObtained = PlayerData.instance.MPReserveMax >= fsm.FsmVariables.GetFsmInt("Full Amount").Value;
                bool showsObtained = fsm.ActiveStateName != "Not Obtained";
                if (isObtained && !showsObtained)
                {
                    fsm.SetState("Load Anim");
                }
                else if (!isObtained && showsObtained)
                {
                    fsm.SendEvent("UNBIND VESSEL ORB");
                }
            }
        }

        GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE UP");
    }
}
