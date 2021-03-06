﻿using nsEnums;
using nsInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsNotenizerObjects
{
    /// <summary>
    /// Persistable note.
    /// </summary>
    public class Note : IPersistable
    {
        #region Variables

        private String _id;
        private String _text;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private String _andRuleId;
        private String _ruleId;

        #endregion Variables

        #region Constructors

        public Note(
           String id,
           String text,
           String ruleID,
           String andRuleID,
           DateTime createdAt,
           DateTime updatedAt)
        {
            this._id = id;
            this._text = text;
            this._ruleId = ruleID;
            this._andRuleId = andRuleID;
            this._createdAt = createdAt;
            this._updatedAt = updatedAt;
        }

        public Note(String text)
        {
            this._text = text;
            this._createdAt = DateTime.Now;
            this._updatedAt = DateTime.Now;
        }

        #endregion Constuctors

        #region Properties

        /// <summary>
        /// ID of note document.
        /// </summary>
        public String ID
        {
            get { return _id; }
            set { this._id = value; }
        }

        /// <summary>
        /// Text of note.
        /// </summary>
        public String Text
        {
            get { return _text; }
            set { this._text = value; }
        }

        /// <summary>
        /// Created at timestamp.
        /// </summary>
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { this._createdAt = value; }
        }

        /// <summary>
        /// Updated at timestamp.
        /// </summary>
        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            set { this._updatedAt = value; }
        }

        /// <summary>
        /// ID of and-rule.
        /// </summary>
        public String AndRuleID
        {
            get { return _andRuleId; }
            set { this._andRuleId = value; }
        }

        /// <summary>
        /// ID of rule
        /// </summary>
        public String RuleID
        {
            get { return this._ruleId; }
            set { this._ruleId = value; }
        }

        #endregion Properties

        #region Methods

        #endregion Methods
    }
}
