using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    private void Start()
    {
        loadButton.interactable = SaveManager.HasSaveData();
        deleteButton.interactable = SaveManager.HasSaveData();
    }

    public void NewGame()
    {
        GlobalManager.singleton.saveData = null;
        GlobalManager.singleton.LoadScene("GameScene");
    }

    public void LoadGame()
    {
        GlobalManager.singleton.LoadData();
        GlobalManager.singleton.LoadScene("GameScene");
    }

    public void Delete()
    {
        SaveManager.DeleteData();
        loadButton.interactable = false;
        deleteButton.interactable = false;
    }
}