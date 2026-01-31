using UnityEngine;
using UnityEngine.U2D.Animation;

public class Structure : MonoBehaviour
{
    public Vector2Int size;
    public Vector2Int bottomLeft;

    [SerializeField] private SpriteLibraryAsset[] libraryAssets;

    public void SetLayer(int baseLayer)
    {
        GetComponent<SpriteRenderer>().sortingOrder = baseLayer;

        int childIndex = 0;
        foreach (Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().sortingOrder = baseLayer + childIndex * 4;
            childIndex++;
        }
    }

    public void SetupStructure(int baseLayer, int season)
    {
        SetLayer(baseLayer);
        ChangeSeason(season);
    }

    public void ChangeSeason(int season)
    {
        if (libraryAssets.Length > 0)
        {
            int childIndex = 0;
            foreach (Transform child in transform)
            {
                SpriteLibrary library = child.GetComponent<SpriteLibrary>();
                library.spriteLibraryAsset = libraryAssets[season];
                childIndex++;
            }
        }
    }
}
