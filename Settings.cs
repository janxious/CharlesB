namespace CharlesB
{
    public class Settings
    {
        public bool debug = false;

        public int attackMissInstabilityPercent = 40;
        public float AttackMissInstabilityPercent => (float)attackMissInstabilityPercent / 100.0f;

        public int attackMissInstabilityLeggedPercent = 100;
        public float AttackMissInstabilityLeggedPercent => (float)attackMissInstabilityLeggedPercent / 100.0f;

        public bool allowSteadyToKnockdown = true;
        public bool AllowSteadyToKnockdown => allowSteadyToKnockdown;
    }
}