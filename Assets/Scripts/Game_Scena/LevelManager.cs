using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using YG;

public class LevelManager : MonoBehaviour
{
    public Button[] levels;
    public Button[] subLevels;
    public GameObject[] lockedIcons;
    public GameObject[] unlockedIcons;
    public GameObject[] levelObjects;
    public GameObject gameOverPanel;
    public GameObject mainMenu;
    public GameObject gamePanel;
    public int unlockedLevel = 0;
    public int unlockedSubLevel = 0;
    public GameObject player;
    public GameObject[] startDesignObjects;
    public GameObject road;
    private GameObject currentLevelClone;
    public int currentLevelIndex;
    public bool[] completedSubLevels;
    private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
    private bool isAdForLevel = false;
    public SkillLevelManager skillLevelManager;

    private void Start()
    {
        completedSubLevels = new bool[subLevels.Length];
        unlockedSubLevel = Mathf.Max(0, unlockedSubLevel);
        UpdateLevelAccess();
        SaveInitialPositions();
    }

    public void ResetObjectsToInitialPositions()
    {
        player.transform.position = initialPositions[player];
        foreach (GameObject obj in startDesignObjects)
        {
            obj.transform.position = initialPositions[obj];
        }
        road.transform.position = initialPositions[road];
    }

    public void ActivateLevel()
    {
        ResetObjectsToInitialPositions();
    }

    public void UpdateLevelAccess()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            bool isUnlocked = i <= unlockedLevel;
            levels[i].interactable = isUnlocked;
        }

        for (int i = 0; i < subLevels.Length; i++)
        {
            bool isUnlocked = i == unlockedSubLevel || completedSubLevels[i];
            subLevels[i].interactable = isUnlocked;

            if (lockedIcons[i] != null && unlockedIcons[i] != null)
            {
                lockedIcons[i].SetActive(!(i == unlockedSubLevel || completedSubLevels[i]));
                unlockedIcons[i].SetActive(i == unlockedSubLevel || completedSubLevels[i]);
            }
        }
    }

    public void SetCurrentLevelIndex(int levelIndex)
    {
        currentLevelIndex = levelIndex;
    }

    public int GetCurrentSubLevelIndex()
    {
        return currentLevelIndex;
    }

    public void CompleteSubLevel(int subLevelIndex)
    {
        if (subLevelIndex == unlockedSubLevel)
        {
            Debug.Log($"Завершён подуровень: {subLevelIndex}. Открываем следующий подуровень.");
            completedSubLevels[subLevelIndex] = true;
            unlockedSubLevel++;

            if (subLevelIndex % 5 == 4)
            {
                unlockedLevel++;
            }

            UpdateLevelAccess();
            Debug.Log($"Теперь доступен подуровень: {unlockedSubLevel}.");
        }
        else
        {
            Debug.Log($"Подуровень {subLevelIndex} уже был завершен ранее. Ничего не происходит.");
        }
    }

    public void ActivateLevel(int levelIndex)
    {
        skillLevelManager.UpdateSoulCountUI();
        SetCurrentLevelIndex(levelIndex);
        if (currentLevelClone != null)
        {
            Destroy(currentLevelClone);
        }

        if (levelIndex >= 0 && levelIndex < levelObjects.Length)
        {
            currentLevelClone = Instantiate(levelObjects[levelIndex]);
            currentLevelClone.SetActive(true);
        }

        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
        }

        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
            foreach (Transform child in gamePanel.transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        PlayerAbilities playerAbilities = FindObjectOfType<PlayerAbilities>();
        if (playerAbilities != null)
        {
            playerAbilities.UpdateSkillAvailability();
            playerAbilities.healSkillCount = 2;
            playerAbilities.slowMoSkillCount = 2;
            playerAbilities.shieldSkillCount = 2;
            playerAbilities.UpdateHealCounterUI();
            playerAbilities.UpdateSlowMoCounterUI();
            playerAbilities.UpdateShieldCounterUI();
        }
    }

    public void RemoveCurrentLevel()
    {
        if (currentLevelClone != null)
        {
            Destroy(currentLevelClone);
            currentLevelClone = null;
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void OnPlayerDeath()
    {
        ShowGameOverPanel();
    }

    public void GoToMainMenu()
    {
        if (currentLevelClone != null)
        {
            Destroy(currentLevelClone);
        }

        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }

        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }
    }

    public void LoadNextSubLevel()
    {
        int nextSubLevelIndex = currentLevelIndex + 1;

        if (nextSubLevelIndex < subLevels.Length)
        {
            RemoveCurrentLevel();
            ActivateLevel(nextSubLevelIndex);
            SetCurrentLevelIndex(nextSubLevelIndex);
            Debug.Log($"Загружен следующий подуровень: {nextSubLevelIndex}");
        }
        else
        {
            Debug.LogWarning("Достигнут последний подуровень, дальнейшая загрузка невозможна.");
        }
    }

    public void LoadLevelWithAd()
    {
        isAdForLevel = true;
        YG2.onOpenAnyAdv += OnAdStarted;
        YG2.onCloseInterAdv += OnAdFinished;
        YG2.InterstitialAdvShow();
    }
    
    private void SaveInitialPositions()
    {
        initialPositions[player] = player.transform.position;
        foreach (GameObject obj in startDesignObjects)
        {
            initialPositions[obj] = obj.transform.position;
        }
        initialPositions[road] = road.transform.position;
    }

    private void OnAdStarted()
    {
        Debug.Log("Реклама началась. Ожидание завершения...");
        Time.timeScale = 0;
    }

    private void OnAdFinished()
    {
        if (!isAdForLevel)
        {
            Debug.Log("Реклама завершена, но не для уровня. Игнорируем.");
            return;
        }

        isAdForLevel = false;
        Debug.Log("Реклама завершена. Загружаем уровень...");
        Time.timeScale = 1;
        ActivateLevel();
        YG2.onOpenAnyAdv -= OnAdStarted;
        YG2.onCloseInterAdv -= OnAdFinished;
    }
}