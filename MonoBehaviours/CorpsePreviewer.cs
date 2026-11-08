using System.Collections.Generic;
using DebugMod.UI;
using UnityEngine;

namespace DebugMod.MonoBehaviours;

public partial class CorpsePreviewer : MonoBehaviour
{
    public bool previewEnabled;

    private static readonly List<GameObject> activeCorpsePreviews = [];

    private GameObject corpse;
    private GameObject compass;

    private partial bool CalculatePosition(out Vector2 position, out bool atHeroCorpseMarker);
    private partial GameObject SpawnPreview(Vector2 position);
    private partial void PlacePreview(GameObject corpse, Vector2 position, bool atHeroCorpseMarker);

    public static void Unload()
    {
        foreach (GameObject corpse in activeCorpsePreviews)
        {
            if (corpse) Destroy(corpse);
        }
        activeCorpsePreviews.Clear();

        if (GameManager._instance is { } gameManager && gameManager.TryGetComponent(out CorpsePreviewer previewer))
        {
            Destroy(previewer);
        }
    }

    public void Update()
    {
        if (!previewEnabled || !HeroController.instance)
        {
            Destroy(corpse);
            return;
        }

        if (CalculatePosition(out Vector2 position, out bool atHeroCorpseMarker))
        {
            if (!corpse)
            {
                corpse = SpawnPreview(position);
                activeCorpsePreviews.Add(corpse);

                compass = new GameObject("Compass", typeof(SpriteRenderer));
                compass.transform.parent = corpse.transform;

                // arrow facing right because the arctan calculates angle relative to +x so using the right arrow means no extra correction
                Texture2D texture = UICommon.images["Arrow"];
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

            foreach (SpriteRenderer renderer in corpse.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.gameObject.name != "Compass")
                {
                    renderer.color = new Color(0.8f, 1f, 1f, 0.4f);
                }
            }

            PlacePreview(corpse, position, atHeroCorpseMarker);

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
            Destroy(corpse);
        }
    }
}
