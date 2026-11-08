using System.Linq;
using UnityEngine;

namespace DebugMod.MonoBehaviours;

// Mirrors the Set Shade state of the Hero Death Anim FSM
public partial class CorpsePreviewer
{
    private partial bool CalculatePosition(out Vector2 position, out bool atHeroCorpseMarker)
    {
        string sceneName = GameManager.instance.GetSceneNameString();
        switch (GameManager.instance.GetCurrentMapZone())
        {
            case "DREAM_WORLD" or "WHITE_PALACE" or "GODS_GLORY":
                position = default;
                atHeroCorpseMarker = false;
                return false;
            case "COLOSSEUM":
                position = new Vector2(27.2f, 45.6f);
                atHeroCorpseMarker = true;
                return sceneName == "Room_Colosseum_02";
            case "FINAL_BOSS":
                position = new Vector2(219f, 10f);
                atHeroCorpseMarker = true;
                return sceneName == "Room_Final_Boss_Atrium";
        }

        Vector2 heroPosition = HeroController.instance.transform.position;
        GameObject marker = GameObject.FindGameObjectsWithTag("Shade Marker")
            .OrderBy(m => ((Vector2)m.transform.position - heroPosition).sqrMagnitude)
            .FirstOrDefault();
        if (marker)
        {
            position = marker.transform.position;
            if (marker.LocateMyFSM("Spawn Offset") is { } offset)
            {
                position += new Vector2(offset.FsmVariables.GetFsmFloat("X").Value, offset.FsmVariables.GetFsmFloat("Y").Value);
            }
            atHeroCorpseMarker = true;
        }
        else
        {
            position = heroPosition;
            atHeroCorpseMarker = false;
        }

        return true;
    }

    private partial GameObject SpawnPreview(Vector2 position)
    {
        tk2dSprite prefabSprite = Object.FindFirstObjectByType<SceneManager>().hollowShadeObject.GetComponent<tk2dSprite>();
        GameObject shade = new("Shade Preview");
        shade.transform.position = new Vector3(position.x, position.y, 0.006f);

        tk2dSprite sprite = tk2dSprite.AddComponent(shade, prefabSprite.Collection, prefabSprite.spriteId);
        sprite.color = new Color(0.8f, 1f, 1f, 0.4f);

        return shade;
    }

    private partial void PlacePreview(GameObject corpse, Vector2 position, bool atHeroCorpseMarker)
    {
        corpse.transform.position = new Vector3(position.x, position.y, corpse.transform.position.z);
    }
}
