using System;
using System.Reflection;
using BattleTech;
using Harmony;
using Newtonsoft.Json;

namespace CharlesB
{
    public class Core
    {
        public const string ModName = "CharlesB";
        public const string ModId   = "com.joelmeador.CharlesB";

        internal static Settings ModSettings = new Settings();
        internal static string ModDirectory;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;
            Logger.Setup();
            try
            {
                ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            }
            catch (Exception ex)
            {
                FileLog.Log($"oopsie {ex.Message}");
                Logger.Error(ex);
                ModSettings = new Settings();
            }

            HarmonyInstance.DEBUG = ModSettings.debug;
            var harmony = HarmonyInstance.Create(ModId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // patch in mech damage on fall spescifically, which requires some voodoo
            // TODO: doesn't require voodoo of the type I expected, so move to annotation patch
            var mechFallSetState = AccessTools.Method(typeof(MechFallSequence), "setState");
            // var mechFallPrefix = AccessTools.Method(typeof(MechFallSequenceDamageAdder), "GimmeALog");
            // var mechFallPostfix = AccessTools.Method(typeof(MechFallSequenceDamageAdder), "AddDamageToFall");
            var mechFallTranspiler = AccessTools.Method(typeof(MechFallSequenceDamageAdder), "AddDamageCall");
            harmony.Patch(mechFallSetState, null, null, new HarmonyMethod(mechFallTranspiler));
            HarmonyInstance.DEBUG = false;
        }
    }
}