using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(PlayerCharacterController))]
    public class PlayerDash : MonoBehaviour
    {
        [Header("Dash")]
        public float DashForce = 20f;
        public float DashCooldown = 1.0f;

        float m_LastDashTime = -999f;

        PlayerCharacterController m_Controller;
        PlayerInputHandler m_Input;

        void Awake()
        {
            m_Controller = GetComponent<PlayerCharacterController>();
            m_Input = GetComponent<PlayerInputHandler>();
        }

        void Update()
        {
            var skills = GetComponent<Unity.FPS.Game.PlayerSkills>();
            if (skills == null || !skills.HasDash) return;

            if (Time.timeScale == 0f)
                return;

            if (Time.time < m_LastDashTime + DashCooldown)
                return;

            if (m_Input.GetSprintInputHeld() && m_Input.GetJumpInputDown())
            {
                DoDash();
            }
        }

        void DoDash()
        {
            Vector3 dashDir = transform.forward;
            dashDir.y = 0f;
            dashDir.Normalize();

            m_Controller.CharacterVelocity += dashDir * DashForce;
            m_LastDashTime = Time.time;

            Debug.Log("[Skill] Dash!");
        }
    }
}