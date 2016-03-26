﻿using nsEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsNotenizerObjects
{
    public class PartOfSpeech
    {
        private String _tag;
        private PartOfSpeechType _posType;

        public PartOfSpeech(String tag)
        {
            _tag = tag;
            _posType = GetPOSTypeFromPOSTag(tag);
        }

        private PartOfSpeechType GetPOSTypeFromPOSTag(String posTag)
        {
            switch (posTag)
            {
                case "NN":
                case "NNS":
                case "NNP":
                case "NNPS":
                    return PartOfSpeechType.Noun;
                case "VB":
                case "VBD":
                case "VBG":
                case "VBN":
                case "VBP":
                case "VBZ":
                    return PartOfSpeechType.Verb;
                case "RB":
                case "RBR":
                case "RBS":
                    return PartOfSpeechType.Adverb;
                case "JJ":
                case "JJR":
                case "JJS":
                    return PartOfSpeechType.Adjective;
                case "PRP":
                case "PRP$":
                    return PartOfSpeechType.Pronoun;
                case "CC":
                    return PartOfSpeechType.CoordinatingConujction;
                case "CD":
                    return PartOfSpeechType.CardinalNumber;
                case "DT":
                    return PartOfSpeechType.Determiner;
                case "EX":
                    return PartOfSpeechType.ExistentailThere;
                case "FW":
                    return PartOfSpeechType.ForeignWord;
                case "IN":
                    return PartOfSpeechType.PrepositionOrSubordinatingConjuction;
                case "LS":
                    return PartOfSpeechType.ListItemMarker;
                case "MD":
                    return PartOfSpeechType.Modal;

                case "PDT":
                    return PartOfSpeechType.Predeterminer;
                case "POS":
                    return PartOfSpeechType.PossesiveEnding;
                case "RP":
                    return PartOfSpeechType.Particle;
                case "SYM":
                    return PartOfSpeechType.Symbol;
                case "TO":
                    return PartOfSpeechType.To;
                case "UH":
                    return PartOfSpeechType.Interjection;
                case "WDT":
                case "WP":
                case "WP$":
                case "WRB":
                    return PartOfSpeechType.Wh;
                default:
                    return PartOfSpeechType.Unidetified;
            }
        }

        public String Tag
        {
            get { return _tag; }
        }

        public PartOfSpeechType Type
        {
            get { return _posType; }
        }
    }
}