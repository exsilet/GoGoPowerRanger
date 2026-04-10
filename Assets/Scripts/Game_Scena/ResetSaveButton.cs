using UnityEngine;
using UnityEngine.UI;
using YG;

public class ResetSaveButton : MonoBehaviour
{
    public Button resetSaveButton;

    private void Start()
    {
        if (resetSaveButton != null)
            resetSaveButton.onClick.AddListener(ResetSaves);
        else
            Debug.LogError("Кнопка сброса сохранений не привязана в инспекторе!");
    }

    public void ResetSaves()
    {
        Debug.Log("Сброс сохранений...");

        YG2.saves = new SavesYG();

        YG2.SaveProgress();

        Debug.Log("Сохранения успешно сброшены и сохранены.");
    }
}