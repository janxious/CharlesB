﻿using System;
using System.Collections.Generic;
using System.IO;
using BattleTech;

namespace CharlesB
{
    public class HandleFall
    {
        private static List<string> phrases = new List<string>();
        private static bool _fileLoaded = false;
 
        /// <summary>
        ///     displays a pithy floatie message over the supplied mech
        /// </summary>
        /// <param name="mech"></param>
        public static void Say(Mech mech)
        {
            if (!Settings.EnableKnockdownPhrases) return;
            if (!mech.IsFlaggedForKnockdown) return;

            if (!_fileLoaded)
                try
                {
                    var phraseFile = Path.Combine(Core.ModDirectory, "phrases.txt");
                    if (!File.Exists(phraseFile))
                    {
                        Logger.Error(new FileNotFoundException($"Unable to locate {phraseFile}"));
                    }

                    phrases = new List<string>();
                    var reader = new StreamReader(phraseFile);
                    using (reader)
                    {
                        while (!reader.EndOfStream) phrases.Add(reader.ReadLine());
                    }

                    _fileLoaded = true;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

            var random = new Random();
            var knockdownMessage = phrases[random.Next(0, phrases.Count - 1)];
            mech.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(
                new ShowActorInfoSequence(mech, knockdownMessage, FloatieMessage.MessageNature.Debuff, false))); // false leaves camera unlocked from floatie
        }
    }
}
