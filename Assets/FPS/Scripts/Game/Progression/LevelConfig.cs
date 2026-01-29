using UnityEngine;

namespace Unity.FPS.Game
{
    [CreateAssetMenu(menuName = "FPS/Progression/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Caps")]
        [Min(1)] public int MaxLevel = 20;

        [Header("XP Formula")]
        [Tooltip("XP required to reach Level 2 (incremental requirement).")]
        [Min(1)] public int BaseXpToLevel2 = 50;

        [Tooltip("Percent growth of XP requirement per level. Example: 0.20 = +20% per level.")]
        [Range(0f, 5f)] public float GrowthPercentPerLevel = 0.20f;

        [Tooltip("Rounds per-level requirement up to this step. 1 = no rounding, 5/10/25 etc.")]
        [Min(1)] public int RequirementRoundingStep = 5;

        [Header("Passive Scaling (Risk of Rain-ish)")]
        [Min(0f)] public float HpBonusPerLevel = 5f;

        [Min(0f)] public float DamageBonusPerLevel = 0.05f;

        public int GetTotalXpRequiredForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            if (level <= 1) return 0;

            double total = 0;
            double baseReq = BaseXpToLevel2;
            double g = GrowthPercentPerLevel;

            for (int targetLevel = 2; targetLevel <= level; targetLevel++)
            {
                double pow = System.Math.Pow(1.0 + g, targetLevel - 2);
                double inc = baseReq * pow;
                total += RoundUpToStep((int)System.Math.Ceiling(inc), RequirementRoundingStep);
            }

            return (int)System.Math.Ceiling(total);
        }

        public int GetIncrementalXpForLevel(int level)
        {
            level = Mathf.Clamp(level, 2, MaxLevel);

            double inc = BaseXpToLevel2 * System.Math.Pow(1.0 + GrowthPercentPerLevel, level - 2);
            return RoundUpToStep((int)System.Math.Ceiling(inc), RequirementRoundingStep);
        }

        static int RoundUpToStep(int value, int step)
        {
            if (step <= 1) return value;
            int rem = value % step;
            return rem == 0 ? value : value + (step - rem);
        }
    }
}