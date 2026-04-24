using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class GameOverPanelController : MonoBehaviour
{
    [SerializeField] private Button _menuButton;
    [SerializeField] private Transform _skillsPC;
    [SerializeField] private Transform _skillsMobile;
    
    public Button retryButton;
    public Button mainMenuButton;
    public Button settingsButton;
    public Button pauseButton;
    public Sprite icon1;
    public Sprite icon5;
    public Image RewardIcon;
    public GameObject gameOverPanel;
    private bool isSecondChance = false;
    private PauseMenuController pauseMenuController;
    public HeartManager heartManager;
    public LevelManager levelManager;
    private string rewardID = "1";

    private void Start()
    {
        pauseMenuController = FindObjectOfType<PauseMenuController>();
        retryButton.onClick.AddListener(OnRetryClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        _menuButton.gameObject.SetActive(false);
        _skillsPC.gameObject.SetActive(false);
        _skillsMobile.gameObject.SetActive(false);
        UpdateRetryButton();

        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.SaveData();
            saveManager.SaveLevelProgress(levelManager);
            Debug.Log("Сохранение выполнено перед переходом на следующий уровень.");
        }
    }

    private void OnRetryClicked()
    {
        if (!isSecondChance)
        {
            YG2.RewardedAdvShow(rewardID, OnRewardCallback);
        }
        else
        {
            RestartLevel();
        }
    }

    private void OnRewardCallback()
    {
        Debug.Log("Реклама просмотрена полностью. Вторая попытка активирована.");
        StartCoroutine(GrantSecondChanceDelayed());
    }
    
    private IEnumerator GrantSecondChanceDelayed()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(0.05f);

        GrantSecondChance();
    }

    private void GrantSecondChance()
    {
        isSecondChance = true;
        UpdateRetryButton();

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        PlayerController playerController = FindObjectOfType<PlayerController>();
        PlayerAbilities playerAbilities = FindObjectOfType<PlayerAbilities>();
        PauseMenuController pauseMenu = FindObjectOfType<PauseMenuController>();

        if (playerAbilities != null)
            playerAbilities.ResetAbilities();

        if (playerHealth != null)
            playerHealth.RestartGame();

        if (playerController != null)
        {
            playerController.ResetInvincibility();
            playerController.ForceResetControl();
            playerController.UnfreezeAfterRevive();
        }

        if (heartManager != null)
            heartManager.ResetHearts();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (levelManager != null)
        {
            AudioSource audio = levelManager.GetCurrentLevelAudio();
            if (audio != null)
                audio.UnPause();
        }
        
        _menuButton.gameObject.SetActive(true);

        string deviceStr = YG2.envir.deviceType;
        if (deviceStr == "desktop")
            _skillsPC.gameObject.SetActive(true);
        else
            _skillsMobile.gameObject.SetActive(true);

        if (levelManager != null && levelManager.gamePanel != null)
        {
            levelManager.gamePanel.SetActive(true);

            foreach (Transform child in levelManager.gamePanel.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        
        if (pauseMenu != null)
            pauseMenu.ResumeGame();

        Rigidbody2D rb = playerController != null ? playerController.GetComponent<Rigidbody2D>() : null;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.WakeUp();
        }
        
        YG2.GameplayStart();
        Debug.Log("After revive: timeScale = " + Time.timeScale);
    }

    private void OnMainMenuClicked()
    {
        ResetSecondChance();

        if (pauseMenuController != null)
            pauseMenuController.GoToMainMenu();
        else
            Debug.LogError("PauseMenuController не найден на сцене!");

        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

    private void OnSettingsClicked()
    {
        OpenSettingsMenu();
    }

    private void UpdateRetryButton()
    {
        if (isSecondChance)
        {
            retryButton.GetComponent<Image>().sprite = icon5;
            RewardIcon.gameObject.SetActive(false);
        }
        else
        {
            retryButton.GetComponent<Image>().sprite = icon1;
            RewardIcon.gameObject.SetActive(true);
        }
    }

    private void RestartLevel()
    {
        ResetSecondChance();

        if (pauseMenuController != null)
            pauseMenuController.RestartLevel();
        else
            Debug.LogError("PauseMenuController не найден на сцене!");

        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

    private void OpenSettingsMenu()
    {
        Debug.Log("Открываем меню настроек...");
    }

    public void ResetSecondChance()
    {
        isSecondChance = false;
        UpdateRetryButton();
    }
}