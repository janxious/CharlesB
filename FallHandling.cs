using System;
using System.Collections.Generic;
using System.IO;
using BattleTech;

namespace CharlesB
{
    public class FallHandling
    {
        private static readonly string KnockdownPhrasePath = Path.Combine(Core.ModDirectory, "phrases.txt");

        /// <summary>
        ///     displays a pithy floatie message over the supplied mech
        /// </summary>
        /// <param name="mech"></param>
        public static void SaySomethingPithy(Mech mech)
        {
            if (!Settings.EnableKnockdownPhrases) return;
            if (!mech.IsFlaggedForKnockdown) return;

            var fileLoaded = false; // only initialize the list once, only if it's needed
            var phrases = new List<string>();
            var random = new Random();
            if (!fileLoaded)
                try
                {
                    var reader = new StreamReader(KnockdownPhrasePath);
                    using (reader)
                    {
                        while (!reader.EndOfStream) phrases.Add(reader.ReadLine());
                    }

                    fileLoaded = true;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

            var knockdownMessage = phrases[random.Next(0, phrases.Count - 1)];
            mech.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(
                new ShowActorInfoSequence(mech, knockdownMessage, FloatieMessage.MessageNature.Debuff, false))); // false leaves camera unlocked from floatie
        }
    }
}