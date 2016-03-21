﻿using MongoDB.Bson;
using nsConstants;
using nsDB;
using nsEnums;
using nsExtensions;
using nsNotenizerObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nsNotenizer
{
    public static class DocumentParser
    {
        /// <summary>
        /// Makes rule for parsing the sentence from entry from database.
        /// </summary>
        /// <param name="dbEntry">Entry from database</param>
        /// <returns></returns>
        public static NotenizerNoteRule ParseNoteRule(BsonDocument dbEntry)
        {
            NotenizerDependencies dependencies;

            CreatedBy createdBy = dbEntry[DBConstants.CreatedByFieldName].AsInt32.ToEnum<CreatedBy>();
            String _id = dbEntry[DBConstants.IdFieldName].AsObjectId.ToString();

            dependencies = ParseDependencies(dbEntry, DBConstants.NoteDependenciesFieldName);

            List<int> sentencesEnds = new List<int>();
            foreach (BsonInt32 endLoop in dbEntry[DBConstants.SentencesEndsFieldName].AsBsonArray)
                sentencesEnds.Add((int)endLoop);

            return new NotenizerNoteRule(_id, dependencies, sentencesEnds, createdBy);
        }

        public static NotenizerAndParserRule ParseAndParserRule(BsonDocument dbEntry)
        {
            NotenizerDependencies dependencies;
            int sentenceEnd;
            CreatedBy createdBy;
            int setsPosition;
            String id;

            createdBy = dbEntry[DBConstants.CreatedByFieldName].AsInt32.ToEnum<CreatedBy>();
            id = dbEntry[DBConstants.IdFieldName].AsObjectId.ToString();
            dependencies = ParseDependencies(dbEntry, DBConstants.NoteDependenciesFieldName);
            setsPosition = dbEntry[DBConstants.AndSetsPositionsFieldName].AsInt32;
            sentenceEnd = dbEntry[DBConstants.SentenceEndFieldname].AsInt32;

            return new NotenizerAndParserRule(id, dependencies, createdBy, setsPosition, sentenceEnd);
        }

        private static NotenizerDependencies ParseDependencies(BsonDocument dbEntry, String noteFieldName)
        {
            NotenizerDependencies dependencies = new NotenizerDependencies();

            // foreach note dependency
            foreach (BsonDocument documentLoop in dbEntry[noteFieldName].AsBsonArray)
            {
                NotenizerRelation relation = new NotenizerRelation(documentLoop[DBConstants.DependencyNameFieldName].AsString);

                // foreach dependency in array of dependencies with same relation name
                foreach (BsonDocument dependencyDocLoop in documentLoop[DBConstants.DependenciesFieldName].AsBsonArray)
                {
                    BsonDocument governorDoc = dependencyDocLoop[DBConstants.GovernorFieldName].AsBsonDocument;
                    BsonDocument dependantDoc = dependencyDocLoop[DBConstants.DependentFieldName].AsBsonDocument;
                    int position = dependencyDocLoop[DBConstants.PositionFieldName].AsInt32;
                    ComparisonType comparisonType = dependencyDocLoop[DBConstants.ComparisonTypeFieldName].AsInt32.ToEnum<ComparisonType>();
                    TokenType tokenType = dependencyDocLoop[DBConstants.TokenTypeFieldName].AsInt32.ToEnum<TokenType>();

                    NotenizerWord governor = new NotenizerWord(governorDoc[DBConstants.POSFieldName].AsString, governorDoc[DBConstants.IndexFieldName].AsInt32);
                    NotenizerWord dependent = new NotenizerWord(dependantDoc[DBConstants.POSFieldName].AsString, dependantDoc[DBConstants.IndexFieldName].AsInt32);

                    NotenizerDependency dependency = new NotenizerDependency(governor, dependent, relation, position, comparisonType, tokenType);

                    dependencies.Add(dependency);
                }
            }

            return dependencies;
        }

        public static Note ParseNote(BsonDocument dbEntry)
        {
            String id;
            String originalSentence;
            String note;
            DateTime createdAt;
            DateTime updatedAt;
            CreatedBy createdBy;
            String andParserRuleRefId;

            id = dbEntry[DBConstants.IdFieldName].AsObjectId.ToString();
            originalSentence = dbEntry[DBConstants.OriginalSentenceFieldName].AsString;
            note = dbEntry[DBConstants.NoteFieldName].AsString;
            createdAt = dbEntry[DBConstants.CreatedAtFieldName].ToUniversalTime();
            updatedAt = dbEntry[DBConstants.UpdatedAtFieldName].ToUniversalTime();
            createdBy = dbEntry[DBConstants.CreatedByFieldName].AsInt32.ToEnum<CreatedBy>();
            andParserRuleRefId = dbEntry[DBConstants.AndParserRuleRefIdFieldName].ToString();

            return new Note(id, originalSentence, note, createdAt, updatedAt, createdBy, andParserRuleRefId);
        }

        /// <summary>
        /// Gets rule for parsing with the heighest match with original sentence.
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="dbEntries"></param>
        /// <returns></returns>
        public static NotenizerNoteRule GetHeighestMatch(NotenizerSentence sentence, List<BsonDocument> dbEntries)
        {
            NotenizerNoteRule rule = null;

            foreach (BsonDocument bsonDocLoop in dbEntries)
            {
                BsonDocument ruleDocument = DB.GetFirst(DBConstants.NoteRulesCollectionName, DocumentCreator.CreateFilterById(bsonDocLoop[DBConstants.NoteRuleRefIdFieldName].AsObjectId.ToString())).Result;
                NotenizerNoteRule r = ParseNoteRule(ruleDocument);

                r.Note = ParseNote(bsonDocLoop);
                r.Match = CalculateMatch(sentence, r.RuleDependencies, bsonDocLoop);

                if (rule == null || rule.Match < r.Match || (rule.Match == r.Match && rule.CreatedBy == CreatedBy.Notenizer && r.CreatedBy == CreatedBy.User))
                    rule = r;
            }

            return rule;
        }

        /// <summary>
        /// Calculates the match between original sentence (from DB) and sentence that is being parsed.
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="parsedDependencies"></param>
        /// <param name="dbEntry"></param>
        /// <returns></returns>
        private static Double CalculateMatch(NotenizerSentence sentence, List<NotenizerDependency> parsedDependencies, BsonDocument dbEntry)
        {
            Double compareCount = 8.0;
            Double oneCompareType = 100.0 / compareCount;
            Double oneCompareTypeIter;
            Double counter = 0.0;

            int c = 0;

            oneCompareTypeIter = oneCompareType / Double.Parse(dbEntry[DBConstants.OriginalSentenceDependenciesFieldName].AsBsonArray.Count.ToString());
            foreach (BsonDocument origDepDocLoop in dbEntry[DBConstants.OriginalSentenceDependenciesFieldName].AsBsonArray)
            {
                String depName = origDepDocLoop[DBConstants.DependencyNameFieldName].AsString;

                if (sentence.CompressedDependencies[depName].Count == origDepDocLoop[DBConstants.DependenciesFieldName].AsBsonArray.Count)
                {
                    counter += oneCompareTypeIter;
                }

                c += origDepDocLoop[DBConstants.DependenciesFieldName].AsBsonArray.Count;
            }

            // Goes over all dependencies of original sentence
            // and gets the name of dependency (for example: compound)
            // and checks, if there is, in sentence that is parsed right now,
            // the dependency with same POS tag or same index at governor or dependent.
            oneCompareTypeIter = oneCompareType / (double)(c);
            foreach (BsonDocument origDepDocLoop in dbEntry[DBConstants.OriginalSentenceDependenciesFieldName].AsBsonArray)
            {
                String depName = origDepDocLoop[DBConstants.DependencyNameFieldName].AsString;

                foreach (BsonDocument depLoop in origDepDocLoop[DBConstants.DependenciesFieldName].AsBsonArray)
                {
                    if (sentence.CompressedDependencies[depName].Where(
                        x => x.Dependent.POS == depLoop[DBConstants.DependentFieldName].AsBsonDocument[DBConstants.POSFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }

                    if (sentence.CompressedDependencies[depName].Where(
                        x => x.Dependent.Index == depLoop[DBConstants.DependentFieldName].AsBsonDocument[DBConstants.IndexFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }

                    if (sentence.CompressedDependencies[depName].Where(
                        x => x.Governor.POS == depLoop[DBConstants.GovernorFieldName].AsBsonDocument[DBConstants.POSFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }

                    if (sentence.CompressedDependencies[depName].Where(
                        x => x.Governor.Index == depLoop[DBConstants.GovernorFieldName].AsBsonDocument[DBConstants.IndexFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }

                    if (sentence.CompressedDependencies[depName].Where(
                        x =>x.Governor.POS == depLoop[DBConstants.GovernorFieldName].AsBsonDocument[DBConstants.POSFieldName] 
                        && x.Governor.Index == depLoop[DBConstants.GovernorFieldName].AsBsonDocument[DBConstants.IndexFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }

                    if (sentence.CompressedDependencies[depName].Where(
                        x => x.Dependent.POS == depLoop[DBConstants.DependentFieldName].AsBsonDocument[DBConstants.POSFieldName]
                        && x.Dependent.Index == depLoop[DBConstants.DependentFieldName].AsBsonDocument[DBConstants.IndexFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }

                    if (sentence.CompressedDependencies[depName].Where(
                        x => x.Dependent.POS == depLoop[DBConstants.DependentFieldName].AsBsonDocument[DBConstants.POSFieldName]
                        && x.Dependent.Index == depLoop[DBConstants.DependentFieldName].AsBsonDocument[DBConstants.IndexFieldName]
                        && x.Governor.POS == depLoop[DBConstants.GovernorFieldName].AsBsonDocument[DBConstants.POSFieldName]
                        && x.Governor.Index == depLoop[DBConstants.GovernorFieldName].AsBsonDocument[DBConstants.IndexFieldName]).FirstOrDefault() != null)
                    {
                        counter += oneCompareTypeIter;
                    }
                }
            }

            return Math.Round(counter);
        }
    }
}
