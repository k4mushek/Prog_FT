using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.Game
{
    public class ExperienceHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Slider xpSlider;

        void Start()
        {
            var sys = ExperienceSystem.Instance;
            if (sys == null)
            {
                Debug.LogError("ExperienceHud: ExperienceSystem not found.");
                return;
            }

            sys.OnLevelChanged += OnLevelChanged;
            sys.OnXpChanged += OnXpChanged;

            OnLevelChanged(sys.CurrentLevel);
            OnXpChanged(sys.CurrentXp, 0);
        }

        void OnDestroy()
        {
            var sys = ExperienceSystem.Instance;
            if (sys == null) return;

            sys.OnLevelChanged -= OnLevelChanged;
            sys.OnXpChanged -= OnXpChanged;
        }

        private void OnLevelChanged(int level)
        {
            if (levelText) levelText.text = $"LV {level}";
        }

        private void OnXpChanged(int xp, int xpToNext)
        {
            if (xpText)
                xpText.text = xpToNext <= 0 ? $"{xp} XP (MAX)" : $"{xp} XP (to next: {xpToNext})";

            if (xpSlider)
            {
                float max = xp + Mathf.Max(1, xpToNext);
                xpSlider.maxValue = max;
                xpSlider.value = xp;
            }
        }
    }
}