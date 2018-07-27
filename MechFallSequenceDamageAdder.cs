using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleTech;
using Harmony;
using UnityEngine;

namespace CharlesB
{
    public static class MechFallSequenceDamageAdder
    {
        static IEnumerable<CodeInstruction> AddDamageCall(IEnumerable<CodeInstruction> instructions)
        {
            if (!Core.ModSettings.FallingDamage) return instructions;

            var instructionList = instructions.ToList();
            var insertionIndex = 
                instructionList.FindIndex(
                    instruction => instruction.opcode == OpCodes.Ret                      // find early return
                ) + 2;
            var nopLabelReplaceIndex = insertionIndex - 1;
            instructionList[nopLabelReplaceIndex].opcode = OpCodes.Nop;                   // preserve jump label
            instructionList.Insert(insertionIndex, new CodeInstruction(OpCodes.Ldarg_0)); // replace code we just mangled

            var stateField = AccessTools.Field(typeof(MechFallSequence), "state");
            var owningMechGetter = AccessTools.Property(typeof(MechFallSequence), "OwningMech").GetGetMethod();
            var calculator = AccessTools.Method(
                typeof(MechFallSequenceDamageAdder), "DoDamage", new Type[] {typeof(MechFallSequence), typeof(int), typeof(int)}
            );
            var damageMethodCalloutInstructions = new List<CodeInstruction>();
            damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));               // this
            damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));               // this
            damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldfld, stateField));     // this.state
            damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldarg_1));               // newState (Argument)
            damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Call, calculator));      // MechFallSequenceDamageAdder.DoDamage(this, this.state, newState)
            instructionList.InsertRange(insertionIndex, damageMethodCalloutInstructions);
            return instructionList;
        }

        private const int FinishedState = 3;

        private static readonly ArmorLocation[] possibleLocations = new[]
        {
            ArmorLocation.Head,
            ArmorLocation.CenterTorsoRear,
            ArmorLocation.LeftTorsoRear,
            ArmorLocation.RightTorsoRear,
            ArmorLocation.LeftArm,
            ArmorLocation.RightArm,
            ArmorLocation.RightLeg,
            ArmorLocation.LeftLeg
        };

        static void DoDamage(MechFallSequence sequence, int oldState, int newState)
        {
            if (newState != FinishedState) return;
            Logger.Debug($"falling happening: {oldState} -> {newState}");
            var locationTakingDamage = possibleLocations[UnityEngine.Random.RandomRange(0, possibleLocations.Length)];
            Logger.Debug($"location taking damage: {locationTakingDamage}");
            var hitInfo = new WeaponHitInfo(0, sequence.SequenceGUID, 0, 0, "FELL DOWN", sequence.OwningMech.GUID, 1, null, null, null, null, null, null, null, AttackDirection.FromBack, default(Vector2), null);
            sequence.OwningMech.ApplyArmorStatDamage(locationTakingDamage, Core.ModSettings.FallingDamageAmount, hitInfo);
            //sequence.OwningMech.TakeWeaponDamage(hitInfo, (int) locationTakingDamage, sequence.OwningMech.MeleeWeapon, Core.ModSettings.FallingDamageAmount, 0);
        }
    }
}