using System.Collections.Generic;
using UnityEngine;
using YG;

public class ObjectInteractionManager : MonoBehaviour
{
    public GameObject[] interactableObjects;

    private HashSet<string> interactedIDs = new HashSet<string>();

    private void Start()
    {
        LoadInteractedIDs();

        Debug.Log($"Всего объектов для проверки: {interactableObjects.Length}");
        CheckAndRemoveInteractedObjects();
    }

    private void CheckAndRemoveInteractedObjects()
    {
        foreach (GameObject obj in interactableObjects)
        {
            if (obj != null)
            {
                InteractableObject interactable = obj.GetComponent<InteractableObject>();
                if (interactable != null && interactedIDs.Contains(interactable.uniqueID))
                {
                    Debug.Log($"Удаляем объект: {obj.name} с ID: {interactable.uniqueID}");
                    Destroy(obj);
                }
            }
        }
    }

    public void MarkAsInteractedByID(string uniqueID)
    {
        if (!string.IsNullOrEmpty(uniqueID) && !interactedIDs.Contains(uniqueID))
        {
            interactedIDs.Add(uniqueID);
            Debug.Log($"ID {uniqueID} добавлен в список взаимодействий");

            SaveInteractedIDs();
        }
    }

    private void SaveInteractedIDs()
    {
        YG2.saves.interactedObjectIDs = new List<string>(interactedIDs);
        YG2.SaveProgress();
        Debug.Log("Список взаимодействий сохранён.");
    }

    private void LoadInteractedIDs()
    {
        if (YG2.saves != null && YG2.saves.interactedObjectIDs != null)
        {
            interactedIDs = new HashSet<string>(YG2.saves.interactedObjectIDs);
            Debug.Log("Список взаимодействий загружен.");
        }
        else
        {
            Debug.Log("Нет сохранённых взаимодействий.");
        }
    }
}