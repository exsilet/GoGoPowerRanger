using UnityEngine;
using UnityEngine.UI;

public class SkillLevelManager : MonoBehaviour
{
    [Header("Soul Management")]
    public int soulCount = 0; // Копилка душ
    public AudioClip successSound; // Звук успешного улучшения
    public AudioClip errorSound;   // Звук ошибки
    public AudioSource audioSource; // Аудиопроигрыватель
	public TMPro.TextMeshProUGUI[] soulCountTexts;

    [Header("Heal Upgrade")]
    public int healLevel = 0; // Текущий уровень Heal
    public int[] healUpgradeCosts = { 5, 10, 15 }; // Стоимость улучшений Heal
    public GameObject[] healUpgradeImages; // Картинки улучшений Heal
	public Button healUpgradeButton;

    [Header("Shield Upgrade")]
    public int shieldLevel = 0; // Текущий уровень Shield
    public int[] shieldUpgradeCosts = { 5, 10, 15 };
    public GameObject[] shieldUpgradeImages;
	public Button shieldUpgradeButton;

    [Header("Slow-Mo Upgrade")]
    public int slowMoLevel = 0; // Текущий уровень Slow-Mo
    public int[] slowMoUpgradeCosts = { 5, 10, 15 };
    public GameObject[] slowMoUpgradeImages;
	public Button slowMoUpgradeButton;

    [Header("Dependencies")]
    public PlayerAbilities playerAbilities;
    public PlayerHealth playerHealth;
    public HeartManager heartManager;
	
	[Header("UI - Upgrade Costs")]
	public TMPro.TextMeshProUGUI healCostText;
	public TMPro.TextMeshProUGUI shieldCostText;
	public TMPro.TextMeshProUGUI slowMoCostText;
	public TMPro.TextMeshProUGUI[] healDescriptions; // Описания уровней Heal
	public TMPro.TextMeshProUGUI[] shieldDescriptions; // Описания уровней Shield
	public TMPro.TextMeshProUGUI[] slowMoDescriptions; // Описания уровней Slow-Mo
	
	void Start()
	{
		UpdateUpgradeTexts();
		UpdateSoulCountUI();
		// LoadSoulCount();
	}
	
	void Update()
	{
		// UpdateUpgradeTexts();
		// UpdateSoulCountUI();
		// LoadSoulCount();
	}

    // Добавление душ
    public void AddSouls(int amount)
    {
        soulCount += amount;
		UpdateSoulCountUI();
        Debug.Log($"Душ добавлено: {amount}. Всего душ: {soulCount}");
    }

    // Улучшение Heal
    public void UpgradeHeal()
    {
        UpgradeSkill(ref healLevel, healUpgradeCosts, healUpgradeImages, "Heal");
        ApplyHealUpgrade();
		CheckButtonState(healLevel, healUpgradeButton, healUpgradeCosts.Length);
		UpdateUpgradeTexts(); // Обновляем текст
    }

    // Улучшение Shield
    public void UpgradeShield()
    {
        UpgradeSkill(ref shieldLevel, shieldUpgradeCosts, shieldUpgradeImages, "Shield");
		CheckButtonState(shieldLevel, shieldUpgradeButton, shieldUpgradeCosts.Length);
		UpdateUpgradeTexts(); // Обновляем текст
    }

    // Улучшение Slow-Mo
    public void UpgradeSlowMo()
    {
        UpgradeSkill(ref slowMoLevel, slowMoUpgradeCosts, slowMoUpgradeImages, "SlowMo");
		CheckButtonState(slowMoLevel, slowMoUpgradeButton, slowMoUpgradeCosts.Length);
		UpdateUpgradeTexts(); // Обновляем текст
    }

    // Универсальный метод улучшения
	private void UpgradeSkill(ref int skillLevel, int[] costs, GameObject[] objects, string skillName)
	{
		if (skillLevel >= costs.Length)
		{
			Debug.Log($"{skillName} уже максимального уровня!");
			return;
		}

		int cost = costs[skillLevel];

		if (soulCount >= cost)
		{
			soulCount -= cost;
			skillLevel++;
			ActivateUpgradeObject(objects, skillLevel - 1); // Активация объекта
			PlaySound(successSound);
			Debug.Log($"{skillName} улучшен до уровня {skillLevel}. Душ осталось: {soulCount}");
		}
		else
		{
			PlaySound(errorSound);
			Debug.Log($"Недостаточно душ для улучшения {skillName}!");
		}
	}

    private void ApplyHealUpgrade()
	{
		switch (healLevel)
		{
			case 1:
				playerHealth.maxHealth += 1;
				ActivateUpgradeObject(healUpgradeImages, 0);
				break;
			case 2:
				playerHealth.maxHealth += 1;
				ActivateUpgradeObject(healUpgradeImages, 1);
				break;
			case 3:
				playerAbilities.healAmount = 2;
				ActivateUpgradeObject(healUpgradeImages, 2);
				break;
		}

		playerHealth.ResetHealth();
		heartManager.UpdateHeartDisplay();
	}

	private void ActivateUpgradeObject(GameObject[] objects, int index)
	{
		if (objects != null && index >= 0 && index < objects.Length)
		{
			objects[index].SetActive(true);
			Debug.Log($"Объект улучшения с индексом {index} активирован.");
		}
		else
		{
			Debug.LogWarning("Некорректный индекс или массив объектов пуст.");
		}
	}
	
	private void ActivateUpgradeImage(GameObject[] images, int index)
	{
		if (images != null && index >= 0 && index < images.Length)
		{
			images[index].SetActive(true);
			Debug.Log($"Активировано изображение улучшения с индексом {index}.");
		}
		else
		{
			Debug.LogWarning("Некорректный индекс или массив изображений пуст.");
		}
	}

    // Воспроизведение звука
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
	
	
    // Проверка состояния кнопки
    private void CheckButtonState(int skillLevel, Button button, int maxLevel)
    {
        if (skillLevel >= maxLevel && button != null)
        {
            button.interactable = false; // Делаем кнопку неактивной
            Debug.Log("Кнопка улучшения отключена: достигнут максимальный уровень.");
        }
    }
	
	private void UpdateUpgradeTexts()
	{
		// Обновляем текст для Heal
		if (healCostText != null && healLevel < healUpgradeCosts.Length)
		{
			healCostText.text = healLevel < healUpgradeCosts.Length
				? healUpgradeCosts[healLevel].ToString()
				: "Макс.";
		}

		if (healDescriptions != null && healLevel < healDescriptions.Length)
		{
			for (int i = 0; i < healDescriptions.Length; i++)
			{
				healDescriptions[i].gameObject.SetActive(i == healLevel);
			}
		}

		// Обновляем текст для Shield
		if (shieldCostText != null && shieldLevel < shieldUpgradeCosts.Length)
		{
			shieldCostText.text = shieldLevel < shieldUpgradeCosts.Length
				? shieldUpgradeCosts[shieldLevel].ToString()
				: "Макс.";
		}

		if (shieldDescriptions != null && shieldLevel < shieldDescriptions.Length)
		{
			for (int i = 0; i < shieldDescriptions.Length; i++)
			{
				shieldDescriptions[i].gameObject.SetActive(i == shieldLevel);
			}
		}

		// Обновляем текст для Slow-Mo
		if (slowMoCostText != null && slowMoLevel < slowMoUpgradeCosts.Length)
		{
			slowMoCostText.text = slowMoLevel < slowMoUpgradeCosts.Length
				? slowMoUpgradeCosts[slowMoLevel].ToString()
				: "Макс.";
		}

		if (slowMoDescriptions != null && slowMoLevel < slowMoDescriptions.Length)
		{
			for (int i = 0; i < slowMoDescriptions.Length; i++)
			{
				slowMoDescriptions[i].gameObject.SetActive(i == slowMoLevel);
			}
		}
	}
	
    public void UpdateSoulCountUI()
    {
        if (soulCountTexts != null)
        {
            foreach (var textElement in soulCountTexts)
            {
                if (textElement != null)
                {
                    textElement.text = soulCount.ToString();
                }
            }
        }
    }
	
	// public void SaveSoulCount()
	// {
		// PlayerPrefs.SetInt("SoulCount", soulCount);
		// PlayerPrefs.Save();
		// Debug.Log("Данные soulCount сохранены.");
	// }
	
	// public void LoadSoulCount()
	// {
		// soulCount = PlayerPrefs.GetInt("SoulCount", 0);
		// UpdateSoulCountUI();
		// Debug.Log($"Загружено soulCount: {soulCount}");
	// }
}
