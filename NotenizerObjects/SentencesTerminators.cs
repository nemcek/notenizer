﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsNotenizerObjects
{
    public class SentencesTerminators : List<int>
    {
        #region Variables

        #endregion Variables

        #region Constructors

        public SentencesTerminators()
        {
        }

        #endregion Constuctors

        #region Properties

        #endregion Properties

        #region Methods
        public SentencesTerminators(List<NotePart> noteParts)
        {
            int last = 0;
            int particlesCount = 0;

            foreach (NotePart notePartLoop in noteParts)
            {
                particlesCount = notePartLoop.InitializedNoteParticles.Count;
                this.Add(last + particlesCount);
                last = particlesCount;
            }
        }

        public SentencesTerminators(List<int> sentenceTerminators)
        {
            this.AddRange(sentenceTerminators);
        }

        #endregion Methods
    }
}
