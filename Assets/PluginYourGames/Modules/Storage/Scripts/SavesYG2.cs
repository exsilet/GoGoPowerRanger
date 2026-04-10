
using System.Collections.Generic;

namespace YG
{
    [System.Serializable]
    public partial class SavesYG
    {
        public int idSave;
        
        public int healSkillCount = 0;
        public int slowMoSkillCount = 0;
        public int shieldSkillCount = 0;
        public int soulCount = 0;
        public int unlockedLevel = 0;
        public int unlockedSubLevel = 0;
        public bool[] completedSubLevels;
    }
}
