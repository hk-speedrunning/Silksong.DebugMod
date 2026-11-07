using UnityEngine;

namespace DebugMod.MonoBehaviours;

public partial class CorpsePreviewer
{
    private partial bool CalculatePosition(out Vector2 position, out bool atHeroCorpseMarker)
    {
        if (HeroCorpseMarkerProxy.Instance)
        {
            position = HeroCorpseMarkerProxy.Instance.TargetScenePos;
            atHeroCorpseMarker = HeroCorpseMarkerProxy.Instance.TargetGuid != null;
            return HeroCorpseMarkerProxy.Instance.TargetSceneName == GameManager.instance.GetSceneNameString();
        }

        HeroCorpseMarker marker = HeroCorpseMarker.GetClosest(HeroController.instance.transform.position);
        if (marker)
        {
            position = marker.Position;
            atHeroCorpseMarker = true;
        }
        else
        {
            position = HeroController.instance.transform.position;
            atHeroCorpseMarker = false;
        }

        return true;
    }

    private partial GameObject SpawnPreview(Vector2 position)
    {
        GameObject prefab = FindAnyObjectByType<CustomSceneManager>().heroCorpsePrefab;
        GameObject cocoon = Instantiate(prefab, new Vector3(position.x, position.y, prefab.transform.position.z), Quaternion.identity);

        Destroy(cocoon.GetComponent<BoxCollider2D>());
        Destroy(cocoon.GetComponent<HarpoonHook>());
        Destroy(cocoon.GetComponent<HitResponse>());
        Destroy(cocoon.GetComponent<EnemyHitEffectsRegular>());
        Destroy(cocoon.GetComponent<PlayRandomAudioEvent>());

        return cocoon;
    }

    private partial void PlacePreview(GameObject cocoon, Vector2 position, bool atHeroCorpseMarker)
    {
        RepositionFromWalls repositionFromWalls = cocoon.GetComponent<RepositionFromWalls>();
        if (atHeroCorpseMarker)
        {
            repositionFromWalls.enabled = false;
            cocoon.transform.position = new Vector3(position.x, position.y, cocoon.transform.position.z);
        }
        else
        {
            repositionFromWalls.enabled = true;
            repositionFromWalls.Reposition();
        }
    }
}
