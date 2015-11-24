﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using java.util;
using java.io;
using edu.stanford.nlp.pipeline;
using Console = System.Console;
using edu.stanford.nlp.ling;
using System.Globalization;
using System.Threading;
using edu.stanford.nlp.trees;
using nsExtensions;
using nsNotenizerObjects;
using nsConstants;
using nsDB;
using nsEnums;

namespace nsNotenizer
{
    public class Notenizer
    {
		public Notenizer()
		{

		}

        public void RunCoreNLP(String text = null)
        {
            String jarRoot = @"stanford-corenlp-3.5.2-models";

            // Text for processing
            if (text == null)
                text = "Kosgi Santosh sent an email to Stanford University. He didn't get a reply.";

            // Annotation pipeline configuration
            Properties properties = new Properties();
            //props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner, parse, dcoref");
            // Zvolíme, ktoré nástroje chceme použiť.
            // pos = part-of-speech tagger
            // ssplit = sentence split
            // atd.
            properties.setProperty("annotators", "tokenize, ssplit, pos, parse");
            properties.setProperty("sutime.binders", "0");
            properties.setProperty("ner.useSUTime", "false");

            // bugfix for time exception
            CultureInfo cultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // We should change current directory, so StanfordCoreNLP could find all the model files automatically
            String currentDirectory = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(jarRoot);
            StanfordCoreNLP pipeline = new StanfordCoreNLP(properties);
            Directory.SetCurrentDirectory(currentDirectory);

            // Annotation
            Annotation annotation = new Annotation(text);
            pipeline.annotate(annotation);

            // Result - Pretty Print
            using (ByteArrayOutputStream stream = new ByteArrayOutputStream())
            {
                pipeline.prettyPrint(annotation, new PrintWriter(stream));
                Console.WriteLine(stream.toString());
                stream.close();
            }

            Console.WriteLine("======================================================");

            // these are all the sentences in this document
            // a CoreMap is essentially a Map that uses class objects as keys and has values with custom types
            //var sentences = annotation.get(typeof(CoreAnnotations.SentencesAnnotation));
            //if (sentences == null)
            //{
            //	return;
            //}

            //foreach (Annotation sentence in sentences as ArrayList)
            //{
            //	Console.WriteLine(sentence);
            //}

            //Test(annotation);
            PrintNotes(Parse(annotation));
        }

        /// <summary>
        /// Parses the sentence.
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public List<Note> Parse(Annotation annotation)
        {
            List<Note> sentencesNoted = new List<Note>();

            // ================== REFACTORED PART HERE ======================
            foreach (Annotation sentenceLoop in annotation.get(typeof(CoreAnnotations.SentencesAnnotation)) as ArrayList)
            {
                NotePart firstPartNotePart = new NotePart();
                NotePart mainPartNotePart = new NotePart();
                NotePart lastPartNotePart = new NotePart();
                NotenizerSentence sentence = new NotenizerSentence(sentenceLoop);
                Note note = new Note(sentence);
                Note noteLoop;

                //NotenizerRule rule = DocumentParser.ParseNoteDependencies(
                //    DB.GetFirst(DBConstants.NotesCollectionName, DocumentCreator.CreateFilterByDependencies(sentence)).Result);

                NotenizerRule rule = DocumentParser.GetHeighestMatch(sentence,
                    DB.GetAll(DBConstants.NotesCollectionName, DocumentCreator.CreateFilterByDependencies(sentence)).Result);

                if (rule.RuleDependencies != null && rule.RuleDependencies.Count > 0)
                {
                    Note parsedNote = ApplyRule(sentence, rule);
                    Console.WriteLine("Parsed note: " + parsedNote.OriginalSentence + " ===> " + parsedNote.Value);

                    continue;
                }

                foreach (NotenizerDependency dependencyLoop in sentence.Dependencies)
                {
                    if (dependencyLoop.Relation.IsRelation(GrammaticalConstants.NominalSubject)
                        || dependencyLoop.Relation.IsRelation(GrammaticalConstants.NominalSubjectPassive))
                    {
                        NotePart notePart = new NotePart(sentence);

                        noteLoop = new Note();

                        NoteParticle nsubj = new NoteParticle(dependencyLoop.Dependent.Word, dependencyLoop.Dependent, dependencyLoop);
                        notePart.Add(nsubj);

                        String pos = dependencyLoop.Governor.POS;
                        if (POSConstants.NounLikePOS.Contains(pos))
                        {
                            NotenizerDependency compound = sentence.GetDependencyByShortName(
                                dependencyLoop,
                                ComparisonType.DependantToGovernor,
                                GrammaticalConstants.CompoudModifier);

                            if (compound != null)
                            {
                                NoteParticle compoundObj = new NoteParticle(compound.Dependent.Word, compound.Dependent, compound);
                                notePart.Add(compoundObj);
                            }

                            NotenizerDependency aux = sentence.GetDependencyByShortName(
                                dependencyLoop,
                                ComparisonType.GovernorToGovernor,
                                GrammaticalConstants.AuxModifier,
                                GrammaticalConstants.AuxModifierPassive);

                            if (aux != null)
                            {
                                NoteParticle auxObj = new NoteParticle(aux.Dependent.Word, aux.Dependent, aux);
                                notePart.Add(auxObj);
                            }

                            NotenizerDependency cop = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.GovernorToGovernor, GrammaticalConstants.Copula);

                            if (cop != null)
                            {
                                NoteParticle copObj = new NoteParticle(cop.Dependent.Word, cop.Dependent, cop);
                                notePart.Add(copObj);
                            }

                            List<NotenizerDependency> conjuctions = sentence.GetDependenciesByShortName(
                                dependencyLoop,
                                ComparisonType.GovernorToGovernor,
                                GrammaticalConstants.Conjuction);


                            String specific = String.Empty;
                            if (conjuctions != null && conjuctions.Count > 0)
                            {
                                List<NotenizerDependency> filteredConjs = FilterByPOS(conjuctions, POSConstants.ConjustionPOS);

                                foreach (NotenizerDependency filteredConjLoop in filteredConjs)
                                {
                                    NotenizerDependency cc = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.GovernorToGovernor, GrammaticalConstants.CoordinatingConjuction);

                                    if (cc.Dependent.Word == filteredConjLoop.Relation.Specific
                                        && sentence.DependencyIndex(filteredConjLoop) > sentence.DependencyIndex(cc))
                                    {
                                        NoteParticle ccObj = new NoteParticle(cc.Dependent.Word, cc.Dependent, cc);
                                        NoteParticle filteredConjObj = new NoteParticle(filteredConjLoop.Dependent.Word, filteredConjLoop.Dependent, filteredConjLoop);

                                        notePart.Add(ccObj);
                                        notePart.Add(filteredConjObj);
                                    }
                                }
                            }

                            // <== NMODS ==>
                            List<NotenizerDependency> nmodsList = sentence.GetDependenciesByShortName(
                                dependencyLoop, ComparisonType.GovernorToGovernor, GrammaticalConstants.NominalModifier);

                            if (nmodsList != null && nmodsList.Count > 0)
                            {
                                NotenizerDependency neg = sentence.GetDependencyByShortName(
                                    nmodsList.First(), ComparisonType.DependantToGovernor, GrammaticalConstants.NegationModifier);

                                if (neg == null)
                                {
                                    NoteParticle firstObj = new NoteParticle(nmodsList.First().Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodsList.First().Dependent.Word, nmodsList.First().Dependent, nmodsList.First());
                                    notePart.Add(firstObj);
                                }
                                else
                                {
                                    NoteParticle negObj = new NoteParticle(neg.Dependent.Word, neg.Dependent, neg);
                                    NoteParticle firstObj = new NoteParticle(nmodsList.First().Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodsList.First().Dependent.Word, nmodsList.First().Dependent, nmodsList.First());

                                    notePart.Add(negObj);
                                    notePart.Add(firstObj);
                                }

                                // second nmod depending on first one
                                NotenizerDependency nmodSecond = sentence.GetDependencyByShortName(
                                    nmodsList.First(), ComparisonType.DependantToGovernor, GrammaticalConstants.NominalModifier);

                                if (nmodSecond != null)
                                {
                                    neg = sentence.GetDependencyByShortName(
                                    nmodsList.First(), ComparisonType.GovernorToGovernor, GrammaticalConstants.NegationModifier);

                                    if (neg == null)
                                    {
                                        NoteParticle secondObj = new NoteParticle(nmodSecond.Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodSecond.Dependent.Word, nmodSecond.Dependent, nmodSecond);
                                        notePart.Add(secondObj);
                                    }
                                    else
                                    {
                                        NoteParticle negObj = new NoteParticle(neg.Dependent.Word, neg.Dependent, neg);
                                        NoteParticle secondObj = new NoteParticle(nmodSecond.Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodSecond.Dependent.Word, nmodSecond.Dependent, nmodSecond);

                                        notePart.Add(negObj);
                                        notePart.Add(secondObj);
                                    }
                                }
                            }
                            else
                            {
                                // <== AMODS ==>
                                NotenizerDependency amod1 = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.DependantToGovernor, GrammaticalConstants.AdjectivalModifier);

                                // <== AMODS ==>
                                NotenizerDependency amod2 = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.GovernorToGovernor, GrammaticalConstants.AdjectivalModifier);

                                if (amod1 != null || amod2 != null)
                                {
                                    if (amod1 != null)
                                    {
                                        NoteParticle amod1Obj = new NoteParticle(amod1.Dependent.Word, amod1.Dependent, amod1);
                                        notePart.Add(amod1Obj);
                                    }

                                    if (amod2 != null)
                                    {
                                        NoteParticle amod2Obj = new NoteParticle(amod2.Dependent.Word, amod2.Dependent, amod2);
                                        notePart.Add(amod2Obj);
                                    }
                                }
                                else
                                {
                                    // <== NUMMODS ==>
                                    NotenizerDependency nummod1 = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.DependantToGovernor, GrammaticalConstants.NumericModifier);

                                    // <== NUMMODS ==>
                                    NotenizerDependency nummod2 = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.GovernorToGovernor, GrammaticalConstants.NumericModifier);

                                    if (nummod1 != null)
                                    {
                                        NoteParticle nummod1Obj = new NoteParticle(nummod1.Dependent.Word, nummod1.Dependent, nummod1);
                                        notePart.Add(nummod1Obj);
                                    }

                                    if (nummod2 != null)
                                    {
                                        NoteParticle nummod2Obj = new NoteParticle(nummod2.Dependent.Word, nummod2.Dependent, nummod2);
                                        notePart.Add(nummod2Obj);
                                    }
                                }
                            }

                            NoteParticle governorObj = new NoteParticle(dependencyLoop.Governor.Word, dependencyLoop.Governor, dependencyLoop);
                            notePart.Add(governorObj);
                        }
                        else if (POSConstants.VerbLikePOS.Contains(pos))
                        {
                            noteLoop = new Note();

                            NoteParticle gov = new NoteParticle(dependencyLoop.Governor.Word, dependencyLoop.Governor, dependencyLoop);
                            notePart.Add(gov);

                            NotenizerDependency dobj = sentence.GetDependencyByShortName(dependencyLoop, ComparisonType.GovernorToGovernor, GrammaticalConstants.DirectObject);

                            if (dobj != null)
                            {
                                NoteParticle dobjObj = new NoteParticle(dobj.Dependent.Word, dobj.Dependent, dobj);
                                notePart.Add(dobjObj);

                                NotenizerDependency neg = sentence.GetDependencyByShortName(dobj, ComparisonType.DependantToGovernor, GrammaticalConstants.NegationModifier);

                                if (neg != null)
                                {
                                    NoteParticle negObj = new NoteParticle(neg.Dependent.Word, neg.Dependent, neg);
                                    notePart.Add(negObj);
                                }
                            }

                            NotenizerDependency aux = sentence.GetDependencyByShortName(
                                dependencyLoop,
                                ComparisonType.GovernorToGovernor,
                                GrammaticalConstants.AuxModifier,
                                GrammaticalConstants.AuxModifierPassive);

                            if (aux != null)
                            {
                                NoteParticle auxObj = new NoteParticle(aux.Dependent.Word, aux.Dependent, aux);
                                notePart.Add(auxObj);
                            }

                            // <== NMODS ==>
                            List<NotenizerDependency> nmodsList = sentence.GetDependenciesByShortName(
                                dependencyLoop,
                                ComparisonType.GovernorToGovernor,
                                GrammaticalConstants.NominalModifier);

                            if (nmodsList != null && nmodsList.Count > 0)
                            {
                                NotenizerDependency neg = sentence.GetDependencyByShortName(nmodsList.First(), ComparisonType.DependantToGovernor, GrammaticalConstants.NegationModifier);

                                if (neg == null)
                                {
                                    NoteParticle firstObj = new NoteParticle(nmodsList.First().Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodsList.First().Dependent.Word, nmodsList.First().Dependent, nmodsList.First());
                                    notePart.Add(firstObj);
                                }
                                else
                                {
                                    NoteParticle negObj = new NoteParticle(neg.Dependent.Word, neg.Dependent, neg);
                                    NoteParticle firstObj = new NoteParticle(nmodsList.First().Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodsList.First().Dependent.Word, nmodsList.First().Dependent, nmodsList.First());
                                    notePart.Add(firstObj);
                                    notePart.Add(negObj);
                                }

                                // second nmod depending on first one
                                NotenizerDependency nmodSecond = sentence.GetDependencyByShortName(nmodsList.First(), ComparisonType.DependantToGovernor, GrammaticalConstants.NominalModifier);

                                if (nmodSecond != null)
                                {
                                    neg = sentence.GetDependencyByShortName(nmodsList.First(), ComparisonType.GovernorToGovernor, GrammaticalConstants.NegationModifier);

                                    if (neg == null)
                                    {
                                        NoteParticle secondObj = new NoteParticle(nmodSecond.Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodSecond.Dependent.Word, nmodSecond.Dependent, nmodSecond);
                                        notePart.Add(secondObj);
                                    }
                                    else
                                    {
                                        NoteParticle negObj = new NoteParticle(neg.Dependent.Word, neg.Dependent, neg);
                                        NoteParticle secondObj = new NoteParticle(nmodSecond.Relation.AdjustedSpecific + NotenizerConstants.WordDelimeter + nmodSecond.Dependent.Word, nmodSecond.Dependent, nmodSecond);
                                        notePart.Add(secondObj);
                                        notePart.Add(negObj);
                                    }
                                }
                            }
                        }
                        note.Add(notePart);
                    }
                }
                //String _id = DB.InsertToCollection("notes", DocumentCreator.CreateNoteDocument(note, -1)).Result;
                sentencesNoted.Add(note);
            }

            return sentencesNoted;
        }

        /// <summary>
        /// Prints notes to console.
        /// </summary>
        /// <param name="notes"></param>
        public void PrintNotes(List<Note> notes)
        {
            Console.WriteLine();
            Console.WriteLine("<======== NOTES ========>");

            foreach (Note noteLoop in notes)
            {
                Console.WriteLine(noteLoop.OriginalSentence + " ---> " + noteLoop.Value);
            }
        }

        /// <summary>
        /// Filters dependencies by POS tag.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="poses"></param>
        /// <returns></returns>
        public List<NotenizerDependency> FilterByPOS(List<NotenizerDependency> dependencies, String[] poses)
        {
            List<NotenizerDependency> filteredDependencies = new List<NotenizerDependency>();

            foreach (NotenizerDependency dependencyLoop in dependencies)
            {
                if (poses.Contains(dependencyLoop.Dependent.POS))
                    filteredDependencies.Add(dependencyLoop);
            }

            return filteredDependencies;
        }

        /// <summary>
        /// Applies rule.
        /// Parses sentence by applied rule.
        /// </summary>
        /// <param name="sentence">Sentence to apply rule to</param>
        /// <param name="rule">Rule for parsing to apply</param>
        /// <returns></returns>
        private Note ApplyRule(NotenizerSentence sentence, NotenizerRule rule)
        {
            Note note = new Note(sentence);
            NotePart notePart = new NotePart(sentence);

            foreach (NotenizerDependency ruleLoop in rule.RuleDependencies)
            {
                ApplyRule(sentence, ruleLoop, notePart);
            }

            note.Add(notePart);

            note.SplitToSentences(rule.SentencesEnds);

            return note;
        }

        /// <summary>
        /// Applies rule.
        /// Parses sentence by applied rule and part of note of original sentence.
        /// </summary>
        /// <param name="sentence">Sentence to apply rule to</param>
        /// <param name="rule">Rule for parsing to apply</param>
        /// <param name="notePart">Part of note</param>
        private void ApplyRule(NotenizerSentence sentence, NotenizerDependency rule, NotePart notePart)
        {
            NotenizerDependency dependency = sentence.FindDependency(rule);

            if (dependency != null)
            {
                NoteParticle dependencyObj = new NoteParticle(dependency.Dependent, dependency);
                notePart.Add(dependencyObj);

                if (dependency.Relation.IsNominalSubject())
                {
                    NoteParticle govObj = new NoteParticle(dependency.Governor, dependency);
                    notePart.Add(govObj);
                }
            }
        }
    }
}
