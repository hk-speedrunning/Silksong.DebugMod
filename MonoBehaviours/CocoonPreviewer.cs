using UnityEngine;

namespace DebugMod.MonoBehaviours;

public class CocoonPreviewer : MonoBehaviour
{
    public bool previewEnabled;

    private GameObject cocoon;
    private GameObject compass;

    public void Update()
    {
        if (!previewEnabled || !HeroController.instance)
        {
            Destroy(cocoon);
            return;
        }

        if (CalculatePosition(out Vector2 position, out bool atHeroCorpseMarker))
        {
            if (!cocoon)
            {
                GameObject prefab = FindAnyObjectByType<CustomSceneManager>().heroCorpsePrefab;
                cocoon = Instantiate(prefab, new Vector3(position.x, position.y, prefab.transform.position.z), Quaternion.identity);

                Destroy(cocoon.GetComponent<BoxCollider2D>());
                Destroy(cocoon.GetComponent<HarpoonHook>());
                Destroy(cocoon.GetComponent<HitResponse>());
                Destroy(cocoon.GetComponent<EnemyHitEffectsRegular>());
                Destroy(cocoon.GetComponent<PlayRandomAudioEvent>());

                compass = new GameObject("Compass", typeof(SpriteRenderer));
                compass.transform.parent = cocoon.transform;

                //right arrow was used because the arctan calculates angle relative to +x so using the right arrow means no extra correction
                Texture2D texture = GUIController.Instance.images["ScrollBarArrowRight"];
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    64);

                SpriteRenderer compassRenderer = compass.GetComponent<SpriteRenderer>();
                compassRenderer.sprite = sprite;
                compassRenderer.sortingOrder = 32767;

                compass.transform.localScale *= 2;
            }

            foreach (SpriteRenderer renderer in cocoon.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.gameObject.name != "Compass")
                {
                    renderer.color = new Color(0.8f, 1f, 1f, 0.4f);
                }
            }

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

            Vector2 compassPos = HeroController.instance.transform.position + new Vector3(0f, 2f, 0f);
            Vector2 distance = position - compassPos;

            //make sure the compass stays over the player
            compass.transform.position = compassPos;

            //rotate the image to point to the nearest spawn location
            //the rotation on z axis is what we need hence we keep x and y 0
            // the calculation is arctan(y/x) which gives us the angle we need
            compass.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg);
        }
        else
        {
            Destroy(cocoon);
        }
    }

    private bool CalculatePosition(out Vector2 position, out bool atHeroCorpseMarker)
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
}