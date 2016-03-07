﻿using MongoDB.Bson;
using MongoDB.Driver;
using nsConstants;
using nsEnums;
using nsNotenizerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsNotenizer
{
    public static  class DocumentCreator
    {
        public static BsonDocument CreateNoteDocument(Note note, int articleId)
        {
            int dependencyPosition = 0;
            Dictionary<String, BsonArray> dependencies = new Dictionary<String, BsonArray>();

            BsonDocument doc = new BsonDocument();
            BsonDocument additionInformationDoc = new BsonDocument();
            BsonArray sentencesEnds = new BsonArray();

            doc.Add(DBConstants.OriginalSentenceFieldName, new BsonString(note.OriginalSentence.ToString()));
            doc.Add(DBConstants.NoteFieldName, new BsonString(note.Value));
            doc.Add(DBConstants.CreatedByFieldName, new BsonInt32((int)note.CreatedBy));
            doc.Add(DBConstants.ArticleIdFieldName, new BsonInt32(articleId));
            doc.Add(DBConstants.CreatedAtFieldName, new BsonDateTime(note.CreatedAt));
            doc.Add(DBConstants.UpdatedAtFieldName, new BsonDateTime(note.UpdatedAt));
            doc.Add(DBConstants.AdditionalInformationFieldName, additionInformationDoc);
            additionInformationDoc.Add(DBConstants.SentencesEndsFieldName, sentencesEnds);

            BsonArray originalDepencenciesArr = new BsonArray();
            foreach (NotenizerDependency dependencyLoop in note.OriginalSentence.Dependencies)
            {
                BsonDocument dependencyDoc = CreateDependencyDocument(dependencyLoop);

                AppendDependencyDocument(dependencyLoop, dependencyDoc, originalDepencenciesArr, dependencies);

                dependencyPosition++;
            }

            doc.Add(DBConstants.OriginalSentenceDependenciesFieldName, originalDepencenciesArr);

            BsonArray noteDependenciesArr = CreateNoteDependenciesArray(note, sentencesEnds);

            doc.Add(DBConstants.NoteDependenciesFieldName, noteDependenciesArr);
            return doc;
        }

        public static BsonArray CreateNoteDependenciesArray(Note note, BsonArray sentencesEnds)
        {
            int dependencyPosition = 0;
            BsonArray noteDependenciesArr = new BsonArray();
            Dictionary<String, BsonArray> dependencies = new Dictionary<String, BsonArray>();

            foreach (NotePart notePartLoop in note.NoteParts)
            {
                foreach (NoteParticle noteObjectLoop in notePartLoop.NoteParticles)
                {
                    if (noteObjectLoop == null)
                        continue;

                    BsonDocument dependencyDoc = CreateDependencyDocument(noteObjectLoop.NoteDependency, noteObjectLoop.NoteDependency.ComparisonType, noteObjectLoop.NoteDependency.TokenType);

                    AppendDependencyDocument(noteObjectLoop.NoteDependency, dependencyDoc, noteDependenciesArr, dependencies);

                    dependencyPosition++;
                }

                sentencesEnds.Add(new BsonInt32(dependencyPosition));
            }

            return noteDependenciesArr;
        }

        private static BsonDocument CreateWordDocument(NotenizerWord word)
        {
            return new BsonDocument
            {
                { DBConstants.POSFieldName, word.POS },
                { DBConstants.IndexFieldName, word.Index }
            };
        }

        private static BsonDocument CreateDependencyDocument(NotenizerDependency dependency)
        {
            return CreateDependencyDocument(
                CreateWordDocument(dependency.Governor),
                CreateWordDocument(dependency.Dependent),
                dependency.Position);
        }

        public static BsonDocument CreateDependencyDocument(NotenizerDependency dependency, ComparisonType comparisonType, TokenType tokenType)
        {
            BsonDocument doc = CreateDependencyDocument(dependency);
            doc.Add(DBConstants.ComparisonTypeFieldName, new BsonInt32((int)comparisonType));
            doc.Add(DBConstants.TokenTypeFieldName, new BsonInt32((int)tokenType));

            return doc;
        }

        public static BsonDocument CreateDependencyDocument(BsonDocument governorDoc, BsonDocument dependentDoc, int position)
        {
            return new BsonDocument
                {
                    { DBConstants.GovernorFieldName, governorDoc },
                    { DBConstants.DependentFieldName, dependentDoc },
                    { DBConstants.PositionFieldName, new BsonInt32(position) }
                };
        }

        private static void AppendDependencyDocument(NotenizerDependency currentDependency, BsonDocument dependencyDoc, BsonArray destinationArr, Dictionary<String, BsonArray> dependencies)
        {
            if (dependencies.ContainsKey(currentDependency.Relation.ShortName))
                dependencies[currentDependency.Relation.ShortName].Add(dependencyDoc);
            else
            {
                BsonArray arr = new BsonArray { dependencyDoc };

                BsonDocument originalDependencyDoc = CreateNewOriginalDependencieDocumentEntry(dependencyDoc, currentDependency, arr);

                destinationArr.Add(originalDependencyDoc);
                dependencies.Add(currentDependency.Relation.ShortName, arr);
            }
        }

        private static BsonDocument CreateNewOriginalDependencieDocumentEntry(BsonDocument dependencyDoc, NotenizerDependency currentDependency, BsonArray arr)
        {
            return new BsonDocument
            {
                { DBConstants.DependencyNameFieldName, currentDependency.Relation.ShortName },
                { DBConstants.DependenciesFieldName, arr }
            };

        }

        public static FilterDefinition<BsonDocument> CreateFilterByDependencies(NotenizerSentence sentence)
        {
            return CreateFilterByDependencies(sentence.Dependencies, sentence.DistinctDependenciesCount);
        }

        public static FilterDefinition<BsonDocument> CreateFilterByDependencies(List<NotenizerDependency> dependencies, int size)
        {
            return Builders<BsonDocument>.Filter.Size(DBConstants.OriginalSentenceDependenciesFieldName, size) 
                    & Builders<BsonDocument>.Filter.All(DBConstants.OriginalSentenceDependenciesFieldName + "." + DBConstants.DependencyNameFieldName,
                        dependencies.Select(x=>x.Relation.ShortName));
        }
    }
}
