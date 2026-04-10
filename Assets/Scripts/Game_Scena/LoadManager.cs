using UnityEngine;
using YG;

public class LoadManager : MonoBehaviour
{
    public PlayerAbilities playerAbilities;
    public SkillLevelManager skillLevelManager;
    public LevelManager levelManager;

    private void Start()
    {
        if (YG2.saves != null)
        {
            playerAbilities.healSkillCount = YG2.saves.healSkillCount;
            playerAbilities.slowMoSkillCount = YG2.saves.slowMoSkillCount;
            playerAbilities.shieldSkillCount = YG2.saves.shieldSkillCount;
            skillLevelManager.soulCount = YG2.saves.soulCount;
            levelManager.unlockedLevel = YG2.saves.unlockedLevel;
            levelManager.unlockedSubLevel = YG2.saves.unlockedSubLevel;

            if (YG2.saves.completedSubLevels != null && YG2.saves.completedSubLevels.Length == levelManager.subLevels.Length)
            {
                levelManager.completedSubLevels = (bool[])YG2.saves.completedSubLevels.Clone();
            }
            else
            {
                levelManager.completedSubLevels = new bool[levelManager.subLevels.Length];
                for (int i = 0; i <= levelManager.unlockedSubLevel; i++)
                {
                    levelManager.completedSubLevels[i] = true;
                }
            }

            levelManager.UpdateLevelAccess();
            skillLevelManager.UpdateSoulCountUI();
            playerAbilities.UpdateHealCounterUI();
            Debug.Log("Данные загружены из облака и UI обновлён.");
        }
        else
        {
            Debug.Log("Нет сохранённых данных.");
        }
    }
}