using UnityEngine;
using YG;

public class SaveManager : MonoBehaviour
{
    public PlayerAbilities playerAbilities;
    public SkillLevelManager skillLevelManager;
    public LevelManager levelManager;

    public void SaveData()
    {
        YG2.saves.healSkillCount = playerAbilities.healSkillCount;
        YG2.saves.slowMoSkillCount = playerAbilities.slowMoSkillCount;
        YG2.saves.shieldSkillCount = playerAbilities.shieldSkillCount;
        YG2.saves.soulCount = skillLevelManager.soulCount;

        YG2.SaveProgress();
        Debug.Log("Данные сохранены в облако.");
    }

    public void SaveLevelProgress(LevelManager levelManager)
    {
        YG2.saves.unlockedLevel = levelManager.unlockedLevel;
        YG2.saves.unlockedSubLevel = levelManager.unlockedSubLevel;
        YG2.saves.completedSubLevels = (bool[])levelManager.completedSubLevels.Clone();

        YG2.SaveProgress();
        Debug.Log("Прогресс уровней сохранён.");
    }
}