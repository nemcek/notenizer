﻿using nsEnums;
using nsInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsNotenizerObjects
{
    public class NotenizerRule : IPersistable
    {
        protected NotenizerDependencies _dependencies;
        protected Match _match;
        protected CreatedBy _createdBy;
        protected String _id;
        protected Note _note;
        protected DateTime _createdAt;
        protected DateTime _updatedAt;
        protected NotenizerStructure _structure;
        protected String _structureID;
        protected Sentence _sentence;

        public NotenizerRule(String id, NotenizerDependencies dependencies, CreatedBy createdBy)
        {
            _id = id;
            _dependencies = dependencies;
            _createdBy = createdBy;
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public NotenizerRule(CreatedBy createdBy)
        {
            _id = null;
            _dependencies = null;
            _createdBy = createdBy;
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public NotenizerRule(String id, String structureId, DateTime createdAt, DateTime updatedAt)
        {
            this._id = id;
            this._structureID = structureId;
            this._createdAt = createdAt;
            this._updatedAt = updatedAt;
        }

        public NotenizerDependencies RuleDependencies
        {
            get { return _dependencies; }
            set { _dependencies = value; }
        }

        public Match Match
        {
            get { return _match; }
            set { _match = value; }
        }

        public CreatedBy CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        public DateTime CreatedAt
        {
            set { _createdAt = value; }
            get { return _createdAt; }
        }

        public DateTime UpdatedAt
        {
            set { _updatedAt = value; }
            get { return _updatedAt; }
        }

        /// <summary>
        /// ID of corresponding entry in DB, from which this rule is.
        /// </summary>
        public String ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public Note Note
        {
            get { return _note; }
            set { _note = value; }
        }

        public NotenizerStructure Structure
        {
            get { return this._structure; }
            set { this._structure = value; }
        }

        public String StructureID
        {
            get
            {
                return _structureID;
            }

            set
            {
                _structureID = value;
            }
        }

        public Sentence Sentence
        {
            get
            {
                return _sentence;
            }

            set
            {
                _sentence = value;
            }
        }
    }
}
