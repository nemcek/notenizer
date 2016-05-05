﻿using nsEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsNotenizerObjects
{
    public class NotenizerAndRule : NotenizerRule
    {
        #region Variables

        private int _setsPosition;
        private int _sentenceTerminator;

        #endregion Variables

        #region Constructors

        public NotenizerAndRule(NotenizerDependencies dependencies, int setsPosition, int sentenceEnd)
           : base(String.Empty, dependencies)
        {
            _setsPosition = setsPosition;
            _sentenceTerminator = sentenceEnd;
        }

        public NotenizerAndRule(String id, String structureId, DateTime createdAt, DateTime updatedAt, int setsPosition, int sentenceTerminator)
            : base(id, structureId, createdAt, updatedAt)
        {
            this._setsPosition = setsPosition;
            this._sentenceTerminator = sentenceTerminator;
        }

        #endregion Constuctors

        #region Properties

        public int SetsPosition
        {
            get { return _setsPosition; }
            set { _setsPosition = value; }
        }

        public int SentenceTerminator
        {
            get { return _sentenceTerminator; }
            set { _sentenceTerminator = value; }
        }

        #endregion Properties

        #region Methods

        #endregion Methods
    }
}
