using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using YG;

public class ObjectVisibilityTracker : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject[] objectsToTrack;
    public GameObject[] panelsToShow;
    public GameObject continueButton;

    private bool[] objectHasBeenSeen;
    private int currentIndex = -1;
    private HashSet<string> seenObjectIDs;

    public PlayerController playerController;

    void Start()
    {
        objectHasBeenSeen = new bool[objectsToTrack.Length];
        seenObjectIDs = new HashSet<string>();

        LoadState();

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
    }

    public void NotifyObjectVisible(string objectID, GameObject obj)
    {
        if (seenObjectIDs.Contains(objectID))
        {
            Debug.Log("Объект с ID " + objectID + " уже был обработан.");
            return;
        }

        for (int i = 0; i < objectsToTrack.Length; i++)
        {
            if (objectsToTrack[i].GetComponent<UniqueObjectIdentifier>().objectID == objectID)
            {
                if (!objectHasBeenSeen[i])
                {
                    ShowPanel(i);
                    objectHasBeenSeen[i] = true;
                    seenObjectIDs.Add(objectID);
                    SaveState();
                    Time.timeScale = 0;
                }
                break;
            }
        }
    }

    void ShowPanel(int index)
    {
        if (index < 0 || index >= panelsToShow.Length)
        {
            return;
        }

        if (panelsToShow[index] != null)
        {
            panelsToShow[index].SetActive(true);
            currentIndex = index;

            if (playerController != null)
            {
                playerController.DisableControlForDuration(float.MaxValue, false);
            }

            if (continueButton != null)
            {
                continueButton.SetActive(true);
                Button buttonComponent = continueButton.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.RemoveAllListeners();
                    buttonComponent.onClick.AddListener(() => ContinueGame());
                }
            }
        }
        else
        {
            Debug.LogError("Panel is null for index: " + index);
        }
    }

    void ContinueGame()
    {
        if (currentIndex >= 0 && panelsToShow[currentIndex] != null)
        {
            panelsToShow[currentIndex].SetActive(false);
            currentIndex = -1;
        }

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        Time.timeScale = 1;

        if (playerController != null)
        {
            playerController.DisableControlForDuration(0, false);
        }
    }

    private void SaveState()
    {
        if (YG2.saves != null)
        {
            foreach (string id in seenObjectIDs)
            {
                if (!YG2.saves.seenObjectIDs.Contains(id))
                {
                    YG2.saves.seenObjectIDs.Add(id);
                }
            }

            YG2.SaveProgress();
            Debug.Log("Состояние сохранено.");
        }
        else
        {
            Debug.LogWarning("Нет сохранений для обновления.");
        }
    }

    private void LoadState()
    {
        if (YG2.saves != null && YG2.saves.seenObjectIDs != null)
        {
            foreach (string id in YG2.saves.seenObjectIDs)
            {
                seenObjectIDs.Add(id);
            }
            Debug.Log("Состояния объектов загружены.");
        }
        else
        {
            Debug.LogWarning("Нет сохранённых данных для загрузки.");
        }
    }
}