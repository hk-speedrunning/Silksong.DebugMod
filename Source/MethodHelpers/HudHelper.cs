namespace DebugMod.MethodHelpers;

public static class HudHelper
{
    public static void RefreshMasks()
    {
        // Hides UI for several seconds but better than nothing
        EventRegister.SendEvent("HUD APPEAR RESET");

        // TODO: very broken, fix it
        /*
        int hp = GameManager.instance.playerData.health;
        int maxHP = GameManager.instance.playerData.maxHealth;
        GameObject health = GameObject.Find("Health");

        foreach (PlayMakerFSM fsm in health.GetComponentsInChildren<PlayMakerFSM>())
        {
            if (fsm.FsmName == "health_display")
            {
                int index = fsm.GetVariable<int>("Health Number");
                Transform idle = fsm.transform.Find("Idle");

                if (maxHP >= index)
                {
                    fsm.GetComponent<MeshRenderer>().enabled = true;
                    idle.GetComponent<MeshRenderer>().enabled = false;
                    fsm.SetState("Check if Full");
                }
                else
                {
                    fsm.GetComponent<MeshRenderer>().enabled = false;
                    idle.GetComponent<MeshRenderer>().enabled = false;
                    fsm.SetState("Inactive");
                }
            }
        }
        */
    }
}