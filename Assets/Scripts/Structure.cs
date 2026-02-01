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

        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            if (i == childCount - 1)
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = baseLayer + 4;
            }
            else
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = baseLayer;
            }
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
