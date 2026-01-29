using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class SkillProgressionSystem : MonoBehaviour
    {
        public static SkillProgressionSystem Instance { get; private set; }

        static readonly int[] k_Milestones = { 5, 10, 15, 20 };

        const string KeySkillMask = "SK_SkillMask";
        const string KeyMilestoneIndex = "SK_MilestoneIndex";
        const string KeyPendingChoice = "SK_PendingChoice"; 

        [SerializeField] private ExperienceSystem xpSystem;
        [SerializeField] private GameObject playerRoot;

        int m_SkillMask; 
        int m_MilestoneIndex;
        bool m_PendingChoice; 

        public bool HasPendingChoice => m_PendingChoice;

        public event Action<List<SkillId>> OnSkillChoiceAvailable;
        public event Action OnSkillChoiceConsumed;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            Load();

            if (!xpSystem)
                xpSystem = ExperienceSystem.Instance;

            if (xpSystem != null)
                xpSystem.OnLevelChanged += OnLevelChanged;

            if (m_PendingChoice)
                RaiseChoiceAvailable();
        }

        void OnDestroy()
        {
            if (xpSystem != null)
                xpSystem.OnLevelChanged -= OnLevelChanged;
        }

        void OnLevelChanged(int newLevel)
        {
            if (CountChosenSkills() >= 4) return;

            if (m_PendingChoice) return;

            if (m_MilestoneIndex >= k_Milestones.Length) return;

            if (newLevel >= k_Milestones[m_MilestoneIndex])
            {
                m_PendingChoice = true;
                Save();
                RaiseChoiceAvailable();
            }
        }

        public List<SkillId> GetAvailableSkills()
        {
            var list = new List<SkillId>(4);
            foreach (SkillId s in Enum.GetValues(typeof(SkillId)))
            {
                if (!IsSkillChosen(s))
                    list.Add(s);
            }
            return list;
        }

        public bool IsSkillChosen(SkillId skill)
        {
            int bit = 1 << (int)skill;
            return (m_SkillMask & bit) != 0;
        }

        public void SelectSkill(SkillId skill)
        {
            if (!m_PendingChoice) return;
            if (IsSkillChosen(skill)) return;

            var player = ResolvePlayer();
            if (!player)
            {
                Debug.LogError("[Skills] Player not assigned/found.");
                return;
            }

            var ps = player.GetComponent<PlayerSkills>();
            if (!ps) ps = player.AddComponent<PlayerSkills>();

            ps.Apply(skill);

            m_SkillMask |= 1 << (int)skill;

            m_PendingChoice = false;
            m_MilestoneIndex = Mathf.Min(m_MilestoneIndex + 1, k_Milestones.Length);

            Save();
            OnSkillChoiceConsumed?.Invoke();
        }

        void RaiseChoiceAvailable()
        {
            OnSkillChoiceAvailable?.Invoke(GetAvailableSkills());
        }

        GameObject ResolvePlayer()
        {
            if (playerRoot) return playerRoot;
            var byTag = GameObject.FindWithTag("Player");
            return byTag;
        }

        int CountChosenSkills()
        {
            int c = 0;
            for (int i = 0; i < 4; i++)
                if ((m_SkillMask & (1 << i)) != 0) c++;
            return c;
        }

        void Save()
        {
            PlayerPrefs.SetInt(KeySkillMask, m_SkillMask);
            PlayerPrefs.SetInt(KeyMilestoneIndex, m_MilestoneIndex);
            PlayerPrefs.SetInt(KeyPendingChoice, m_PendingChoice ? 1 : 0);
            PlayerPrefs.Save();
        }

        void Load()
        {
            m_SkillMask = PlayerPrefs.GetInt(KeySkillMask, 0);
            m_MilestoneIndex = PlayerPrefs.GetInt(KeyMilestoneIndex, 0);
            m_PendingChoice = PlayerPrefs.GetInt(KeyPendingChoice, 0) == 1;
        }

        [ContextMenu("Reset Skills")]
        public void ResetSkills()
        {
            PlayerPrefs.DeleteKey(KeySkillMask);
            PlayerPrefs.DeleteKey(KeyMilestoneIndex);
            PlayerPrefs.DeleteKey(KeyPendingChoice);
            PlayerPrefs.Save();

            m_SkillMask = 0;
            m_MilestoneIndex = 0;
            m_PendingChoice = false;

            OnSkillChoiceConsumed?.Invoke();
        }
    }
}
