﻿using MongoDB.Bson;
using MongoDB.Driver;
using nsConstants;
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
            doc.Add(DBConstants.OriginalSentenceFieldName, new BsonString(note.OriginalSentence.ToString()));
            doc.Add(DBConstants.NoteFieldName, new BsonString(note.Value));
            doc.Add(DBConstants.CreatedByFieldName, new BsonInt32((int)note.CreatedBy));
            doc.Add(DBConstants.ArticleIdFieldName, new BsonInt32(articleId));

            BsonArray originalDepencenciesArr = new BsonArray();
            foreach (NotenizerDependency dependencyLoop in note.OriginalSentence.Dependencies)
            {
                BsonDocument dependencyDoc = CreateDependencyDocument(dependencyLoop, dependencyPosition);

                AppendDependencyDocument(dependencyLoop, dependencyDoc, originalDepencenciesArr, dependencies);

                dependencyPosition++;
            }

            doc.Add(DBConstants.OriginalSentenceDependenciesFieldName, originalDepencenciesArr);

            dependencyPosition = 0;
            dependencies = new Dictionary<String, BsonArray>();
            BsonArray noteDependenciesArr = new BsonArray();
            foreach (NotePart notePartLoop in note.NoteParts)
            {
                foreach (NoteObject noteObjectLoop in notePartLoop.NoteObjects)
                {
                    if (noteObjectLoop == null)
                        continue;

                    BsonDocument dependencyDoc = CreateDependencyDocument(noteObjectLoop.NoteDependency, dependencyPosition);

                    AppendDependencyDocument(noteObjectLoop.NoteDependency, dependencyDoc, noteDependenciesArr, dependencies);

                    dependencyPosition++;
                }
            }

            doc.Add(DBConstants.NoteDependenciesFieldName, noteDependenciesArr);

            return doc;
        }

        private static BsonDocument CreateWordDocument(NotenizerWord word)
        {
            return new BsonDocument
            {
                { DBConstants.POSFieldName, word.POS },
                { DBConstants.IndexFieldName, word.Index }
            };
        }

        private static BsonDocument CreateDependencyDocument(NotenizerDependency dependency, int position)
        {
            return CreateDependencyDocument(
                CreateWordDocument(dependency.Governor),
                CreateWordDocument(dependency.Dependent),
                position);
        }

        private static BsonDocument CreateDependencyDocument(BsonDocument governorDoc, BsonDocument dependentDoc, int poition)
        {
            return new BsonDocument
                {
                    { DBConstants.GovernorFieldName, governorDoc },
                    { DBConstants.DependentFieldName, dependentDoc },
                    { DBConstants.PositionFieldName, new BsonInt32(poition) }
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
            return CreateFilterByDependencies(sentence.Dependencies);
        }

        public static FilterDefinition<BsonDocument> CreateFilterByDependencies(List<NotenizerDependency> dependencies)
        {
            return Builders<BsonDocument>.Filter.All(DBConstants.OriginalSentenceDependenciesFieldName + "." + DBConstants.DependencyNameFieldName,
                dependencies.Select(x=>x.Relation.ShortName));
        }
    }
}