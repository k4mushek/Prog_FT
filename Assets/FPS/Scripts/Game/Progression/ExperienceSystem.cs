using UnityEngine;

namespace Unity.FPS.Game
{
    public class ExperienceSystem : MonoBehaviour
    {
        public static ExperienceSystem Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private LevelConfig levelConfig;

        float m_BaseMaxHealth;
        bool m_BaseCaptured;

        [Header("Runtime")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentXp = 0;

        public int CurrentLevel => currentLevel;
        public int CurrentXp => currentXp;

        public event System.Action<int, int> OnXpChanged;  // xp, xpToNext
        public event System.Action<int> OnLevelChanged;    // newLevel
        public event System.Action<int> OnLevelUp;         // newLevel

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void Start()
        {
            Load();
            ApplyPassiveScaling();
            NotifyUi();
        }

        private void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (evt == null || evt.Enemy == null)
                return;

            int gainedXp = 10;
            if (evt.Enemy.TryGetComponent<XPValue>(out var xpValue))
                gainedXp = xpValue.Xp;

            AddXp(gainedXp);
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;

            if (levelConfig == null)
            {
                Debug.LogError("ExperienceSystem: LevelConfig is not assigned.");
                return;
            }

            currentXp += amount;

            bool leveledUp = false;

            while (currentLevel < levelConfig.MaxLevel &&
                   currentXp >= levelConfig.GetTotalXpRequiredForLevel(currentLevel + 1))
            {
                currentLevel++;
                leveledUp = true;

                OnLevelChanged?.Invoke(currentLevel);
                OnLevelUp?.Invoke(currentLevel);
            }

            ApplyPassiveScaling();
            NotifyUi();
            Save();

            if (leveledUp)
                Debug.Log($"LEVEL UP -> {currentLevel} (XP: {currentXp})");
                Debug.Log($"[XP] Lvl={currentLevel}, XP={currentXp}, NextTotal={levelConfig.GetTotalXpRequiredForLevel(currentLevel + 1)}");
        }

        private int GetXpToNext()
        {
            if (levelConfig == null) return 0;
            if (currentLevel >= levelConfig.MaxLevel) return 0;

            int nextThreshold = levelConfig.GetTotalXpRequiredForLevel(currentLevel + 1);
            return Mathf.Max(0, nextThreshold - currentXp);
        }

        private void NotifyUi()
        {
            OnXpChanged?.Invoke(currentXp, GetXpToNext());
            // уровень тоже дергаем, чтобы UI оживал после Load
            OnLevelChanged?.Invoke(currentLevel);
        }

        private void Save()
        {
            PlayerPrefs.SetInt("XP_CurrentXp", currentXp);
            PlayerPrefs.SetInt("XP_CurrentLevel", currentLevel);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            currentXp = PlayerPrefs.GetInt("XP_CurrentXp", 0);
            currentLevel = Mathf.Max(1, PlayerPrefs.GetInt("XP_CurrentLevel", 1));

            if (levelConfig != null)
            {
                currentLevel = Mathf.Clamp(currentLevel, 1, levelConfig.MaxLevel);

                while (currentLevel > 1 && currentXp < levelConfig.GetTotalXpRequiredForLevel(currentLevel))
                    currentLevel--;
            }
        }

        [ContextMenu("Reset Progress")]
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey("XP_CurrentXp");
            PlayerPrefs.DeleteKey("XP_CurrentLevel");
            currentXp = 0;
            currentLevel = 1;
            NotifyUi();
        }

        private void ApplyPassiveScaling()
        {
            if (levelConfig == null)
            {
                Debug.LogError("[XP][Scaling] LevelConfig is not assigned");
                return;
            }

            var player = GameObject.FindWithTag("Player");
            if (!player) return;

            var health = player.GetComponent<Health>();
            if (!health) return;

            var dmgMod = player.GetComponent<PlayerDamageMod>();
            if (!dmgMod) dmgMod = player.AddComponent<PlayerDamageMod>();

            // запоминаем базу один раз (уровень 1)
            if (!m_BaseCaptured)
            {
                m_BaseMaxHealth = health.MaxHealth;
                m_BaseCaptured = true;
            }

            int lvlMinus1 = Mathf.Max(0, currentLevel - 1);

            // HP: растёт линейно
            float newMaxHp = m_BaseMaxHealth + lvlMinus1 * levelConfig.HpBonusPerLevel;

            // сохраним процент текущего хп, чтобы не “случайно” хилить/дамэжить игрока
            float ratio = health.GetRatio();
            health.MaxHealth = newMaxHp;
            health.CurrentHealth = Mathf.Clamp(newMaxHp * ratio, 0f, newMaxHp);

            // Damage: множитель растёт линейно
            dmgMod.damageMultiplier = 1f + lvlMinus1 * levelConfig.DamageBonusPerLevel;

            Debug.Log(
        $"[XP][Scaling] Level={currentLevel} | " +
        $"MaxHP={health.MaxHealth} | " +
        $"DamageMultiplier={dmgMod.damageMultiplier}"
             );
        }
    }
}