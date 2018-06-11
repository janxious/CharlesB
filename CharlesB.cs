using System;
using System.Reflection;
using BattleTech;
using Harmony;
using Newtonsoft.Json;
using UnityEngine;

namespace CharlesB
{
    public static class CharlesB
    {
        internal static Settings ModSettings = new Settings();
        internal static string ModDirectory;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;
            try
            {
                ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ModSettings = new Settings();
            }

            var harmony = HarmonyInstance.Create("com.joelmeador.CharlesB");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    
    // this is the function that gets called after dfa hit/miss has been resolved but before the turn is over
    [HarmonyPatch(typeof(MechDFASequence), "OnMeleeComplete")]
    public static class MechDFASequence_OnMeleeComplete_Patch
    {
        static void Postfix(ref MessageCenterMessage message, MechMeleeSequence __instance)
        {
            if (CharlesB.ModSettings.DfaMissInstability)
            {
                Logger.Debug($"checking for miss: {(message as AttackCompleteMessage).attackSequence.attackCompletelyMissed}");
                var attackCompleteMessage = (AttackCompleteMessage) message;
                var attacker = __instance.OwningMech;
                if (attackCompleteMessage.attackSequence.attackCompletelyMissed)
                {
                    Logger.Debug("We missed!");
                    Logger.Debug($"flagged for knockdown? {attacker.IsFlaggedForKnockdown}");
                    var rawInstabilityToAdd = attacker.IsLegged
                        ? CharlesB.ModSettings.DfaMissInstabilityLeggedPercent
                        : CharlesB.ModSettings.DfaMissInstabilityPercent;
                    var instabilityToAdd = rawInstabilityToAdd;
                    if (CharlesB.ModSettings.pilotingSkillInstabilityMitigation)
                    {
                        // pilot skill can mitigate up to skill level * 10% of instability
                        var pilotSkill = attacker.SkillPiloting;
                        var mitigationMax = (float) Mathf.Min(pilotSkill, 10) / 10;
                        var mitigation = UnityEngine.Random.Range(0, mitigationMax);
                        var mitigationPercent = Mathf.RoundToInt(mitigation * 100);
                        instabilityToAdd = rawInstabilityToAdd - rawInstabilityToAdd * mitigation;
                        Logger.Debug($"dfa miss numbers\npilotSkill: {pilotSkill}\nmitigationMax: {mitigationMax}\nmitigation: {mitigation}\nrawInstabilityToAdd: {rawInstabilityToAdd}\nmitigationPercent: {mitigationPercent}\ninstabilityToAdd: {instabilityToAdd}");
                        attacker.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(attacker, $"Pilot Check: Avoided {mitigationPercent}% Instability!", FloatieMessage.MessageNature.Neutral, true)));
                    }

                    attacker.AddRelativeInstability(instabilityToAdd, StabilityChangeSource.DFA, attacker.GUID);
                    attacker.NeedsInstabilityCheck = true;
                    attacker.CheckForInstability();
                    Logger.Debug($"flagged for knockdown? {attacker.IsFlaggedForKnockdown}");
                    if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
                    {
                        attacker.NeedsInstabilityCheck = true;
                        attacker.CheckForInstability();
                        Logger.Debug($"flagged for knockdown? {attacker.IsFlaggedForKnockdown}");
                    }
                }
            }
        }
    }

    // this is called just before relinquishing control in a dfa attack
    [HarmonyPatch(typeof(MechDFASequence), "CompleteOrders", new Type[] { })]
    public static class MechDFASequence_CompleteOrders_Patch
    {
        static bool Prefix(MechMeleeSequence __instance)
        {
            // allow DFA to knock down target in single hit
            if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
            {
                if (!__instance.MeleeTarget.IsDead)
                {
                    var mech = __instance.MeleeTarget as Mech;
                    Logger.Debug($"opponent stability: {mech.CurrentStability}");
                    var target = __instance.MeleeTarget as AbstractActor;
                    if (target != null)
                    {
                        Logger.Debug($"found aa. unsteady? {mech.IsUnsteady} : {mech.IsProne}");
                        target.NeedsInstabilityCheck = true;
                        target.CheckForInstability();
                        Logger.Debug($"found aa. unsteady? {mech.IsUnsteady} : {mech.IsProne}");
                        target.NeedsInstabilityCheck = true;
                        target.CheckForInstability();
                        Logger.Debug($"found aa. downed? {mech.IsUnsteady} : {mech.IsProne}");
                        var attacker = __instance.OwningMech;
                        target.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence), "OnMeleeComplete")]
    public static class MechMeleeSequence_OnMeleeComplete_Patch
    {
        static void Postfix(ref MessageCenterMessage message, MechMeleeSequence __instance)
        {
            Logger.Debug($"checking for miss: {(message as AttackCompleteMessage).attackSequence.attackCompletelyMissed}");

            if (CharlesB.ModSettings.AttackMissInstability)
            {
                // Deal with attacker missing
                var attackCompleteMessage = (AttackCompleteMessage) message;
                var attacker = __instance.OwningMech;
                if (attackCompleteMessage.attackSequence.attackCompletelyMissed)
                {
                    Logger.Debug($"melee pre-miss stability: {attacker.CurrentStability}");
                    var rawInstabilityToAdd = attacker.IsLegged
                        ? CharlesB.ModSettings.AttackMissInstabilityLeggedPercent
                        : CharlesB.ModSettings.AttackMissInstabilityPercent;
                    var instabilityToAdd = rawInstabilityToAdd;
                    if (CharlesB.ModSettings.pilotingSkillInstabilityMitigation)
                    {
                        // pilot skill can mitigate up to skill level * 10% of instability
                        var pilotSkill = attacker.SkillPiloting;
                        var mitigationMax = (float) Mathf.Min(pilotSkill, 10) / 10;
                        var mitigation = UnityEngine.Random.Range(0, mitigationMax);
                        var mitigationPercent = Mathf.RoundToInt(mitigation * 100);
                        instabilityToAdd = rawInstabilityToAdd - rawInstabilityToAdd * mitigation;
                        Logger.Debug($"melee miss numbers\npilotSkill: {pilotSkill}\nmitigationMax: {mitigationMax}\nmitigation: {mitigation}\nrawInstabilityToAdd: {rawInstabilityToAdd}\nmitigationPercent: {mitigationPercent}\ninstabilityToAdd: {instabilityToAdd}");
                        attacker.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(attacker, $"Pilot Check: Avoided {mitigationPercent}% Instability!", FloatieMessage.MessageNature.Neutral, true)));
                    }

                    attacker.AddRelativeInstability(instabilityToAdd, StabilityChangeSource.Attack, attacker.GUID);
                    Logger.Debug($"melee post-miss stability: {attacker.CurrentStability}");
                    attacker.NeedsInstabilityCheck = true;
                    if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
                    {
                        attacker.CheckForInstability();
                        attacker.NeedsInstabilityCheck = true;
                    }
                }
            }

            // Deal with target needing additional checks if we want to be able to knock over
            // mechs in a single round from one attack.
            if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
            {
                if (!__instance.MeleeTarget.IsDead)
                {
                    var target = __instance.MeleeTarget as AbstractActor;
                    if (target != null)
                    {
                        target.NeedsInstabilityCheck = true;
                        target.CheckForInstability();
                        var attacker = __instance.OwningMech;
                        target.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence), "CompleteOrders", new Type[] { })]
    public static class MechMeleeSequence_CompleteOrders_Patch
    {
        static bool Prefix(MechMeleeSequence __instance)
        {
            // attacker second instability check during melee whiff
            var attacker = __instance.OwningMech;
            attacker.CheckForInstability();
            attacker.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);

            // second target instability check during melee hit if can go to ground in one hit
            if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
            {
                if (!__instance.MeleeTarget.IsDead)
                {
                    var target = __instance.MeleeTarget as AbstractActor;
                    if (target != null)
                    {
                        target.NeedsInstabilityCheck = true;
                        target.CheckForInstability();
                        target.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
                    }
                }
            }
            Logger.Debug($"did we fall? {attacker.IsProne}");
            return true;
        }
    }
}