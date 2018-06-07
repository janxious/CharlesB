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
    
    [HarmonyPatch(typeof(MechDFASequence), "OnMeleeComplete")]
    public static class MechDFASequence_OnMeleeComplete_Patch
    {
        static void Postfix(ref MessageCenterMessage message, MechMeleeSequence __instance)
        {
            Logger.Debug($"checking for miss: {(message as AttackCompleteMessage).attackSequence.attackCompletelyMissed}");
            var attackCompleteMessage = (AttackCompleteMessage) message;
            var attacker = __instance.OwningMech;
            if (attackCompleteMessage.attackSequence.attackCompletelyMissed && CharlesB.ModSettings.DfaMissDoubleAttackerInstability)
            {
                Logger.Debug("We missed!");
                Logger.Debug($"flagged for knowckdown? {attacker.IsFlaggedForKnockdown}");
                attacker.ApplyInstabilityReduction(StabilityChangeSource.DFA);
                attacker.NeedsInstabilityCheck = true;
                attacker.CheckForInstability();
                Logger.Debug($"flagged for knowckdown? {attacker.IsFlaggedForKnockdown}");
                if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
                {
                    attacker.NeedsInstabilityCheck = true;
                    attacker.CheckForInstability();
                    Logger.Debug($"flagged for knowckdown? {attacker.IsFlaggedForKnockdown}");
                }
            }


        }
    }

    [HarmonyPatch(typeof(MechDFASequence), "CompleteOrders", new Type[] { })]
    public static class MechDFASequence_CompleteOrders_Patch
    {
        static bool Prefix(MechMeleeSequence __instance)
        {
            var attacker = __instance.OwningMech;
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
            var attackCompleteMessage = (AttackCompleteMessage) message;
            var attacker = __instance.OwningMech;
            if (attackCompleteMessage.attackSequence.attackCompletelyMissed)
            {
                var instabilityToAdd = attacker.IsLegged
                    ? CharlesB.ModSettings.AttackMissInstabilityLeggedPercent
                    : CharlesB.ModSettings.AttackMissInstabilityPercent;
                attacker.AddRelativeInstability(instabilityToAdd, StabilityChangeSource.Attack, attacker.GUID);
                attacker.NeedsInstabilityCheck = true;
              if (CharlesB.ModSettings.AllowSteadyToKnockdownForMelee)
              {
                  attacker.CheckForInstability();
                  attacker.NeedsInstabilityCheck = true;
              }
            }

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
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence), "CompleteOrders", new Type[] { })]
    public static class MechMeleeSequence_CompleteOrders_Patch
    {
        static bool Prefix(MechMeleeSequence __instance)
        {
            var attacker = __instance.OwningMech;
            attacker.CheckForInstability();
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
            attacker.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
            Logger.Debug($"did we fall? {attacker.IsProne}");
            return true;
        }
    }
}