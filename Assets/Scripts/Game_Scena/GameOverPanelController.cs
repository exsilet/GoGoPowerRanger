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
            Time.timeScale = 0f;
        }
        else
        {
            RestartLevel();
        }
    }

    private void OnRewardCallback()
    {
        Debug.Log("Реклама просмотрена полностью. Вторая попытка активирована.");
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
        }

        if (heartManager != null)
            heartManager.ResetHearts();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        _menuButton.gameObject.SetActive(true);

        string deviceStr = YG2.envir.deviceType;
        if (deviceStr == "desktop") _skillsPC.gameObject.SetActive(true);
        else if (deviceStr == "mobile") _skillsMobile.gameObject.SetActive(true);

        if (pauseMenu != null)
            pauseMenu.ResumeGame();
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
            retryButton.GetComponent<Image>().sprite = icon5;
        else
            retryButton.GetComponent<Image>().sprite = icon1;
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