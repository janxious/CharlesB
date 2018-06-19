namespace CharlesB
{
    public class Settings
    {

        public const string ModName = "CharlesB";

        public bool debug = false;

        public bool attackMissInstability = true;
        public bool AttackMissInstability => attackMissInstability;

        public int attackMissInstabilityPercent = 30;
        public float AttackMissInstabilityPercent => attackMissInstabilityPercent / 100.0f;

        public int attackMissInstabilityLeggedPercent = 70;
        public float AttackMissInstabilityLeggedPercent => attackMissInstabilityLeggedPercent / 100.0f;

        public bool pilotingSkillInstabilityMitigation = true;
        public bool PilotingSkillInstabilityMitigation => pilotingSkillInstabilityMitigation;

        public bool pilotingSkillDFASelfDamageMitigation = true;
        public bool PilotingSkillDFASelfDamageMitigation => pilotingSkillDFASelfDamageMitigation;

        public bool allowSteadyToKnockdownForMelee = true;
        public bool AllowSteadyToKnockdownForMelee => allowSteadyToKnockdownForMelee;

        public bool dfaMissInstability = true;
        public bool DfaMissInstability => dfaMissInstability;

        public int dfaMissInstabilityPercent = 40;
        public float DfaMissInstabilityPercent => dfaMissInstabilityPercent / 100.0f;

        public int dfaMissInstabilityLeggedPercent = 80;
        public float DfaMissInstabilityLeggedPercent => dfaMissInstabilityLeggedPercent / 100.0f;
    }
}