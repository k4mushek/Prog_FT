using UnityEngine;

namespace Unity.FPS.Game
{
    public class PlayerSkills : MonoBehaviour
    {
        public bool HasDash { get; private set; }
        public bool HasChargedShot { get; private set; }
        public bool HasDoubleJump { get; private set; }
        public bool HasNoOverheat { get; private set; }

        public void Apply(SkillId skill)
        {
            switch (skill)
            {
                case SkillId.Dash:
                    HasDash = true;
                    break;

                case SkillId.ChargedShot:
                    HasChargedShot = true;
                    // реализацию подключим позже (см. ниже)
                    break;

                case SkillId.DoubleJump:
                    HasDoubleJump = true;
                    // реализацию подключим позже (см. ниже)
                    break;

                case SkillId.NoOverheat:
                    HasNoOverheat = true;
                    // реализацию подключим позже (см. ниже)
                    break;
            }
        }

        T EnsureComponent<T>() where T : Component
        {
            var c = GetComponent<T>();
            if (!c) c = gameObject.AddComponent<T>();
            return c;
        }
    }
}