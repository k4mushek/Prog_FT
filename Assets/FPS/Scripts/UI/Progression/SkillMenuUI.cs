using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    public class SkillMenuUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] GameObject rootPanel;

        [SerializeField] Button dashButton;
        [SerializeField] Button chargedShotButton;
        [SerializeField] Button doubleJumpButton;
        [SerializeField] Button noOverheatButton;

        [Header("Input")]
        [SerializeField] InputActionReference obtainSkillAction; // Player/Obtain Skill

        bool m_Open;
        float m_PrevTimeScale = 1f;
        Unity.FPS.Gameplay.PlayerInputHandler m_PlayerInput;

        void Awake()
        {
            if (rootPanel) rootPanel.SetActive(false);

            dashButton.onClick.AddListener(() => Pick(SkillId.Dash));
            if (chargedShotButton) chargedShotButton.onClick.AddListener(() => Pick(SkillId.ChargedShot));
            doubleJumpButton.onClick.AddListener(() => Pick(SkillId.DoubleJump));
            noOverheatButton.onClick.AddListener(() => Pick(SkillId.NoOverheat));
        }
        void CachePlayerInput()
        {
            if (m_PlayerInput) return;

            var player = GameObject.FindWithTag("Player");
            if (!player) { Debug.LogError("[SkillMenuUI] Player not found (tag Player)"); return; }

            m_PlayerInput = player.GetComponent<Unity.FPS.Gameplay.PlayerInputHandler>();
            if (!m_PlayerInput) Debug.LogError("[SkillMenuUI] PlayerInputHandler not found on Player");
        }

        void OnEnable()
        {
            if (obtainSkillAction != null && obtainSkillAction.action != null)
            {
                obtainSkillAction.action.Enable();
                obtainSkillAction.action.performed += OnObtainSkillPerformed;
            }
            else
            {
                Debug.LogError("[SkillMenuUI] ObtainSkillAction is not assigned (InputActionReference).");
            }
        }

        void OnDisable()
        {
            if (obtainSkillAction != null && obtainSkillAction.action != null)
            {
                obtainSkillAction.action.performed -= OnObtainSkillPerformed;
                obtainSkillAction.action.Disable();
            }
        }

        void OnObtainSkillPerformed(InputAction.CallbackContext ctx)
        {
            if (Time.timeScale == 0f) return; // если уже пауза, не спамим

            var sys = SkillProgressionSystem.Instance;
            if (sys == null) { Debug.LogError("[SkillMenuUI] SkillProgressionSystem.Instance is null"); return; }

            if (!sys.HasPendingChoice) return;

            Open(sys.GetAvailableSkills());
        }

        void Open(List<SkillId> available)
        {
            m_Open = true;

            if (rootPanel) rootPanel.SetActive(true);

            m_PrevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (m_PlayerInput) m_PlayerInput.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            dashButton.gameObject.SetActive(available.Contains(SkillId.Dash));
            if (chargedShotButton) chargedShotButton.gameObject.SetActive(available.Contains(SkillId.ChargedShot));
            doubleJumpButton.gameObject.SetActive(available.Contains(SkillId.DoubleJump));
            noOverheatButton.gameObject.SetActive(available.Contains(SkillId.NoOverheat));
            Debug.Log($"[SkillMenuUI] OPEN: panel={(rootPanel ? rootPanel.activeSelf : false)} timeScale={Time.timeScale} cursor={Cursor.lockState}/{Cursor.visible}");
        }

        void Close()
        {
            m_Open = false;

            if (rootPanel) rootPanel.SetActive(false);
            Time.timeScale = m_PrevTimeScale <= 0f ? 1f : m_PrevTimeScale;
            if (m_PlayerInput) m_PlayerInput.enabled = true;

            //Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log($"[SkillMenuUI] CLOSE: panel={(rootPanel ? rootPanel.activeSelf : false)} timeScale={Time.timeScale} cursor={Cursor.lockState}/{Cursor.visible}");
        }

        void Pick(SkillId skill)
        {
            Debug.Log($"[SkillMenuUI] PICK: {skill}");

            // 1) Сначала закрыть и вернуть управление, без вариантов
            ForceResume();

            // 2) Потом уже выдаём скилл
            try
            {
                var sys = SkillProgressionSystem.Instance;
                if (sys == null) { Debug.LogError("[SkillMenuUI] SkillProgressionSystem.Instance is null"); return; }

                sys.SelectSkill(skill);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SkillMenuUI] SelectSkill failed: {e}");
            }
        }
    }
}