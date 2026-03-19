/// <file>LangDict.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 19th, 2026</date>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LifeProManager
{
    /// <summary>
    /// Central repository for multilingual keyword dictionaries used by SmartSearch.
    /// </summary>
    public static class LangDict
    {
        // ------------------------------------------------------------
        // User-editable keyword lists.
        // New keywords for additional languages can be inserted here.
        // ------------------------------------------------------------

        // Keywords that separate the left and right segments of a range expression.
        // Examples: "2 weeks and 3 days", "2 semaines et 3 jours", "2 semanas y 3 dias".
        internal static readonly (string key, string value)[] lstAndKeywords =
        {
            ("fr", "et"),
            ("en", "and"),
            ("es", "y")
        };

        // Keywords that introduce a "between X and Y" range expression.
        internal static readonly (string key, string value)[] lstBetweenKeywords =
        {
            ("fr", "entre"),
            ("en", "between"),
            ("es", "entre")
        };

        internal static readonly string[] lstDayWords =
       {
            "day", "jour", "journée", "journee", "dia", "día"
        };

        // Month names mapped to their numeric value (1–12).
        // Includes full names and common variants across supported vocabularies.
        internal static readonly (string key, int value)[] lstMonthNames =
        {
            ("january", 1), ("janvier", 1), ("enero", 1),

            ("february", 2), ("fevrier", 2), ("febrero", 2),

            ("march", 3), ("mars", 3), ("marzo", 3),

            ("april", 4), ("avril", 4), ("abril", 4),

            ("may", 5), ("mai", 5), ("mayo", 5),

            ("june", 6), ("juin", 6), ("junio", 6),

            ("july", 7), ("juillet", 7), ("julio", 7),

            ("august", 8), ("aout", 8), ("agosto", 8),

            ("september", 9), ("septembre", 9), ("septiembre", 9),

            ("october", 10), ("octobre", 10), ("octubre", 10),

            ("november", 11), ("novembre", 11), ("noviembre", 11),

            ("december", 12), ("decembre", 12), ("diciembre", 12)
        };


        // Absolute time keywords
        internal static readonly (string key, string value)[] lstMonthRangeKeywords =
        {
            // this month
            ("ce mois", "this"),
            ("ce mois ci", "this"),
            ("ce mois-ci", "this"),
            ("mois courant", "this"),
            ("mois actuel", "this"),
            ("mois en cours", "this"),
            ("le mois courant", "this"),
            ("le mois actuel", "this"),
            ("le mois en cours", "this"),

            ("this month", "this"),
            ("current month", "this"),
            ("thismonth", "this"),

            ("este mes", "this"),
            ("el mes actual", "this"),
            ("el mes en curso", "this"),
            ("mes actual", "this"),
            ("mes en curso", "this"),

            // next month
            ("mois prochain", "next"),
            ("le mois prochain", "next"),
            ("mois suivant", "next"),
            ("mois suivante", "next"),
            ("mois qui vient", "next"),
            ("mois qui arrive", "next"),
            ("mois qui suit", "next"),
            ("mois d apres", "next"),
            ("mois d'après", "next"),
            ("le mois suivant", "next"),
            ("le mois d apres", "next"),

            ("next month", "next"),
            ("following month", "next"),
            ("the month after", "next"),
            ("themonthafter", "next"),

            ("mes proximo", "next"),
            ("mes siguiente", "next"),
            ("el mes proximo", "next"),
            ("el mes próximo", "next"),
            ("el mes siguiente", "next"),
            ("el mes que viene", "next"),
            ("el mes que sigue", "next"),

            // last month
            ("mois dernier", "last"),
            ("le mois dernier", "last"),
            ("dernier mois", "last"),
            ("mois passe", "last"),
            ("mois passé", "last"),
            ("mois precedent", "last"),
            ("mois precedente", "last"),
            ("mois d avant", "last"),
            ("mois d'avant", "last"),
            ("mois avant", "last"),
            ("mois ecoule", "last"),
            ("mois écoulé", "last"),
            ("mois termine", "last"),
            ("mois terminé", "last"),

            ("last month", "last"),
            ("previous month", "last"),
            ("the month before", "last"),
            ("themonthbefore", "last"),

            ("mes pasado", "last"),
            ("mes anterior", "last"),
            ("mes previo", "last"),
            ("el mes pasado", "last"),
            ("el mes anterior", "last")
        };

        // Weekday-relative keywords
        internal static readonly string[] lstNextWeekdayKeywords =
        {
            "next", "upcoming", "following", "coming", "the next",
            "prochain", "prochaine", "suivant", "suivante",
            "proximo", "próximo", "siguiente", "subsiguiente", "posterior",
            "el siguiente"
        };

        internal static readonly (string key, int value)[] lstNumberMultipliers =
        {
            ("cent", 100), ("cents", 100), ("centaine", 100),
            ("mille", 1000),
            ("hundred", 100), ("hundreds", 100), ("thousand", 1000),
            ("cien", 100), ("ciento", 100), ("mil", 1000)
        };

        // Number words used for tens and related forms (10–90),
        // including teens (10–16) and base components for composite forms
        internal static readonly (string key, int value)[] lstNumberTens =
        {
            ("dix", 10), ("onze", 11), ("douze", 12), ("treize", 13),
            ("quatorze", 14), ("quinze", 15), ("seize", 16),
            ("vingt", 20), ("trente", 30), ("quarante", 40),
            ("cinquante", 50), ("soixante", 60),
            ("septante", 70), ("huitante", 80), ("nonante", 90),
            ("quatrevingt", 80), ("quatre-vingt", 80), ("quatre vingt", 80),

            ("ten", 10), ("eleven", 11), ("twelve", 12),
            ("twenty", 20), ("thirty", 30), ("forty", 40),
            ("fifty", 50), ("sixty", 60), ("seventy", 70),
            ("eighty", 80), ("ninety", 90),

            ("diez", 10), ("once", 11), ("doce", 12), ("trece", 13),
            ("catorce", 14), ("quince", 15),
            ("dieciseis", 16),
            ("veinte", 20), ("treinta", 30), ("cuarenta", 40),
            ("cincuenta", 50), ("sesenta", 60), ("setenta", 70),
            ("ochenta", 80), ("noventa", 90)
        };

        // Basic number words
        internal static readonly (string key, int value)[] lstNumberUnits =
        {
            ("zero", 0), ("zéro", 0),
            ("un", 1), ("une", 1),
            ("deux", 2), ("trois", 3), ("quatre", 4),
            ("cinq", 5), ("six", 6), ("sept", 7),
            ("huit", 8), ("neuf", 9),

            ("one", 1), ("two", 2), ("three", 3),
            ("four", 4), ("five", 5),
            ("seven", 7), ("eight", 8), ("nine", 9),

            ("cero", 0), ("uno", 1), ("una", 1),
            ("dos", 2), ("tres", 3), ("cuatro", 4),
            ("cinco", 5), ("seis", 6), ("siete", 7),
            ("ocho", 8), ("nueve", 9)
        };

        // Optional prepositions in relative expressions.
        internal static readonly string[] lstOptionalPrepositions =
        {
            "of",                // English
            "de", "du", "des",   // French
            "del", "de"          // Spanish
        };


        // Ordinal suffixes
        internal static readonly string[] lstOrdinalSuffixes =
        {
            "er", "eme", "ème", "e",
            "st", "nd", "rd", "th",
            "ro", "do", "to"
        };

        internal static readonly string[] lstPreviousWeekdayKeywords =
        {
            "last", "past", "previous", "the previous",
            "passe", "passee", "dernier", "derniere",
            "precedent", "précédent", "precedente", "précédente",
            "pasado", "anterior", "previo", "el previo"
        };

        // Priority and category keywords
        internal static readonly (string key, string value)[] lstPriorities =
        {
            ("important", "important"), ("importante", "important"),
            ("importantes", "important"), ("urgent", "important"),
            ("urgente", "important"), ("urgentes", "important"),
            ("essential", "important"), ("essentiel", "important"),
            ("essentielle", "important"), ("essentielles", "important"),
            ("essentials", "important"), ("critical", "important"),
            ("critique", "important"), ("crucial", "important"),
            ("priority", "important"), ("priorite", "important"),
            ("priorité", "important"), ("high", "important"),

            ("anniversaire", "anniversary"), ("anniversaires", "anniversary"),
            ("anniv", "anniversary"), ("anni", "anniversary"),
            ("fete", "anniversary"), ("fêtes", "anniversary"),
            ("anniversary", "anniversary"), ("birthday", "anniversary"),
            ("birthdays", "anniversary"), ("bday", "anniversary"),
            ("b-day", "anniversary"), ("cumpleanos", "anniversary"),
            ("cumpleaños", "anniversary")
        };

        // Words that may appear before a range.
        internal static readonly string[] lstRangeOptionalPrefixes =
        {
            // EN
            "from", "starting", "start", "beginning", "period",
            // FR
            "du", "de", "depuis", "période", "periode", "a", "à", "partir", "depuis le",
            // ES
            "del", "desde", "periodo", "a", "a partir", "a partir de"
        };

        // Words that act as separators between left and right bounds.
        internal static readonly string[] lstRangeSeparators =
        {
           // English
           "to", "until", "till", "-",
           
           // French
           "au", "a", "à", "jusqu", "jusquau", "jusqu'au",

           // Spanish
           "al", "hasta"
        };


        // Relative directions (before / after)
        internal static readonly (string key, int value)[] lstRelativeDirections =
        {
            ("before", -1), ("avant", -1), ("antes", -1),
            ("after", +1), ("apres", +1),
            ("despues", +1), ("después", +1), ("earlier", -1),
            ("later", +1), ("plus tot", -1),
            ("plus tard", +1)
        };

        // Relative prepositions and start keywords
        internal static readonly string[] lstRelativePrepositions =
        {
            "dans",
            "en",
            "d'ici",
            "a partir de",
            "in",
            "within",
            "from now",
            "starting in",
            "dentro de",
            "a partir de",
            "dici",
            "dentro"
        };

        // Relative units (day/week/month/year)
        internal static readonly (string key, string value)[] lstRelativeUnits =
        {
            ("day", "day"), ("days", "day"), ("jour", "day"), ("jours", "day"),
            ("dia", "day"), ("dias", "day"),
            ("journée", "day"), ("journee", "day"),

            ("week", "week"), ("weeks", "week"), ("semaine", "week"), ("semaines", "week"),
            ("semana", "week"), ("semanas", "week"),

            ("month", "month"), ("months", "month"), ("mois", "month"),
            ("mes", "month"), ("meses", "month"),

            ("year", "year"), ("years", "year"), ("annee", "year"), ("annees", "year"),
            ("an", "year"), ("ans", "year"), ("año", "year"), ("años", "year")
        };

        // Time-ago constructions
        internal static readonly string[] lstTimeAgoKeywords =
        {
            "ago",   // English
            "hace"   // Spanish
        };

        internal static readonly string[] lstTimeAgoPrefix =
        {
            "il",   // French prefix
            "hace"  // Spanish single-token prefix
        };

        internal static readonly string[] lstTimeAgoMiddle =
        {
            "y"     // French "il y a"
        };

        internal static readonly string[] lstTimeAgoSuffix =
        {
            "a"     // French "il y a"
        };

        internal static readonly (string key, DayOfWeek value)[] lstWeekdays =
        {
            ("monday", DayOfWeek.Monday), ("mon", DayOfWeek.Monday),
            ("tuesday", DayOfWeek.Tuesday), ("tue", DayOfWeek.Tuesday),
            ("wednesday", DayOfWeek.Wednesday), ("wed", DayOfWeek.Wednesday),
            ("thursday", DayOfWeek.Thursday), ("thu", DayOfWeek.Thursday),
            ("friday", DayOfWeek.Friday), ("fri", DayOfWeek.Friday),
            ("saturday", DayOfWeek.Saturday), ("sat", DayOfWeek.Saturday),
            ("sunday", DayOfWeek.Sunday), ("sun", DayOfWeek.Sunday),

            ("lundi", DayOfWeek.Monday),
            ("mardi", DayOfWeek.Tuesday),
            ("mercredi", DayOfWeek.Wednesday),
            ("jeudi", DayOfWeek.Thursday),
            ("vendredi", DayOfWeek.Friday),
            ("samedi", DayOfWeek.Saturday),
            ("dimanche", DayOfWeek.Sunday),

            ("lunes", DayOfWeek.Monday),
            ("martes", DayOfWeek.Tuesday),
            ("miercoles", DayOfWeek.Wednesday), ("miércoles", DayOfWeek.Wednesday),
            ("jueves", DayOfWeek.Thursday),
            ("viernes", DayOfWeek.Friday),
            ("sabado", DayOfWeek.Saturday), ("sábado", DayOfWeek.Saturday),
            ("domingo", DayOfWeek.Sunday)
        };

        internal static readonly (string key, string value)[] lstYearRangeKeywords =
        {
            // this year
            ("this year", "this"),
            ("current year", "this"),
            ("thisyear", "this"),
            ("cette annee", "this"),
            ("lannee courante", "this"),
            ("annee courante", "this"),
            ("lannee actuelle", "this"),
            ("annee actuelle", "this"),
            ("este ano", "this"),
            ("este año", "this"),
            ("ano actual", "this"),
            ("año actual", "this"),

            // next year
            ("next year", "next"),
            ("the year after", "next"),
            ("theyearafter", "next"),
            ("annee prochaine", "next"),
            ("l annee prochaine", "next"),
            ("lannee prochaine", "next"),
            ("annee suivante", "next"),
            ("lannee suivante", "next"),
            ("annee d apres", "next"),
            ("lannee d apres", "next"),
            ("an prochain", "next"),
            ("lan prochain", "next"),
            ("ano proximo", "next"),
            ("ano siguiente", "next"),
            ("año proximo", "next"),
            ("año siguiente", "next"),
            ("el ano proximo", "next"),
            ("el año proximo", "next"),
            ("el ano siguiente", "next"),
            ("el año siguiente", "next"),

            // last year
            ("last year", "last"),
            ("the year before", "last"),
            ("theyearbefore", "last"),
            ("previous year", "last"),
            ("annee passee", "last"),
            ("annee precedente", "last"),
            ("lannee precedente", "last"),
            ("annee d avant", "last"),
            ("lannee d avant", "last"),
            ("an dernier", "last"),
            ("lan dernier", "last"),
            ("ano pasado", "last"),
            ("ano anterior", "last"),
            ("el ano pasado", "last"),
            ("el año pasado", "last"),
            ("el ano anterior", "last"),
            ("el año anterior", "last")
        };        

        // ----------------------------------------------------------------
        // Normalized dictionaries and sets
        // Do not modify this section, unless you know what you're doing.
        // ----------------------------------------------------------------

        internal static readonly HashSet<string> AndKeywordSet =
            BuildNormalizedHashSet(lstAndKeywords.Select(x => x.value));

        public static readonly HashSet<string> DayWordSet =
            BuildNormalizedHashSet(lstDayWords);

        public static readonly HashSet<string> NextWeekdayKeywordSet =
            BuildNormalizedHashSet(lstNextWeekdayKeywords);

        public static readonly Dictionary<string, int> NumberMultiplierDict =
            BuildNormalizedDictionary(lstNumberMultipliers);

        public static readonly Dictionary<string, int> NumberTenDict =
            BuildNormalizedDictionary(lstNumberTens);

        public static readonly Dictionary<string, int> NumberUnitDict =
            BuildNormalizedDictionary(lstNumberUnits);

        public static readonly HashSet<string> OptionalPrepositionSet =
            BuildNormalizedHashSet(lstOptionalPrepositions);

        public static readonly HashSet<string> OrdinalSuffixSet =
            BuildNormalizedHashSet(lstOrdinalSuffixes);

        public static readonly HashSet<string> PreviousWeekdayKeywordSet =
            BuildNormalizedHashSet(lstPreviousWeekdayKeywords);

        public static readonly Dictionary<string, string> PriorityKeywordDict =
            BuildNormalizedDictionary(lstPriorities);

        public static readonly HashSet<string> RangeOptionalPrefixSet =
            BuildNormalizedHashSet(lstRangeOptionalPrefixes);

        public static readonly HashSet<string> RangeSeparatorSet =
            BuildNormalizedHashSet(lstRangeSeparators);

        public static readonly Dictionary<string, int> RelativeDayOffsetDict =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "today", 0 },
                { "tomorrow", 1 },
                { "yesterday", -1 },
                { "aftertomorrow", 2 },
                { "apresdemain", 2 },
                { "dayaftertomorrow", 2 },
                { "beforeyesterday", -2 },
                { "avant-hier", -2 }
            };

        public static readonly Dictionary<string, int> RelativeDirectionDict =
            NextWeekdayKeywordSet.ToDictionary(keyword => keyword, keyword => +1)
                .Concat(PreviousWeekdayKeywordSet.ToDictionary(keyword => keyword, keyword => -1))
                .ToDictionary(element => element.Key, element => element.Value, StringComparer.OrdinalIgnoreCase);

        public static readonly HashSet<string> RelativePrepositionSet =
            BuildNormalizedHashSet(lstRelativePrepositions);

        public static readonly Dictionary<string, string> TimeUnitDict =
            BuildNormalizedDictionary(lstRelativeUnits);

        public static readonly Dictionary<string, int> TimeDirectionDict =
            BuildNormalizedDictionary(lstRelativeDirections);

        public static readonly HashSet<string> TimeAgoKeywordSet =
            BuildNormalizedHashSet(lstTimeAgoKeywords);

        public static readonly HashSet<string> TimeAgoMiddleSet =
            BuildNormalizedHashSet(lstTimeAgoMiddle);

        public static readonly HashSet<string> TimeAgoPrefixSet =
            BuildNormalizedHashSet(lstTimeAgoPrefix);

        public static readonly HashSet<string> TimeAgoSuffixSet =
            BuildNormalizedHashSet(lstTimeAgoSuffix);

        public static readonly Dictionary<string, int> MonthNumberDict =
            BuildNormalizedDictionary(lstMonthNames);

        public static readonly Dictionary<string, string> MonthRangeDict =
            BuildNormalizedDictionary(lstMonthRangeKeywords);

        public static readonly Dictionary<string, string> YearRangeDict =
            BuildNormalizedDictionary(lstYearRangeKeywords);

        public static readonly Dictionary<string, DayOfWeek> WeekdayDict =
            BuildNormalizedDictionary(lstWeekdays);

        public static readonly Dictionary<string, DayOfWeek> WeekdayNameToDayOfWeekDict =
            WeekdayDict.ToDictionary(
                element => element.Key,
                element => element.Value,
                StringComparer.OrdinalIgnoreCase);

        // -----------------------------------------------------------------------------
        // Key normalization helpers
        // These functions take raw data (strings or tuples) and convert them into
        // normalized, lookup‑friendly collections.
        // Do not modify unless you fully understand how normalization works.
        // -----------------------------------------------------------------------------

        private static Dictionary<string, TValue> BuildNormalizedDictionary<TValue>(IEnumerable<(string key, TValue value)> source)
        {
            var groupEntries = source.GroupBy(entry => NormalizeKey(entry.key));

            var normalizedDictionary = groupEntries.ToDictionary(
                group => group.Key,
                group => group.First().value,
                StringComparer.OrdinalIgnoreCase
            );

            return normalizedDictionary;
        }

        private static HashSet<string> BuildNormalizedHashSet(IEnumerable<string> source)
        {
            return source
                .Select(x => NormalizeKey(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalizes any input keyword into a canonical, comparable form.
        /// This makes tokens like "28th", "3rd", "1er", "2ème", "3º" all normalize
        /// to their numeric base ("28", "3", "1", "2", "3").
        /// </summary>
        public static string NormalizeKey(string inputKey)
        {
            if (string.IsNullOrWhiteSpace(inputKey))
            {
                return string.Empty;
            }

            string normalizedKey = inputKey.ToLowerInvariant().Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (char normalizedChar in normalizedKey)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(normalizedChar) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(normalizedChar);
                }
            }

            string cleanedKey = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            while (cleanedKey.Length > 0 &&
                   char.IsPunctuation(cleanedKey[cleanedKey.Length - 1]) &&
                   !char.IsDigit(cleanedKey[cleanedKey.Length - 1]))
            {
                cleanedKey = cleanedKey.Substring(0, cleanedKey.Length - 1);
            }

            int i = 0;

            while (i < cleanedKey.Length && char.IsDigit(cleanedKey[i]))
            {
                i++;
            }

            if (i > 0 && i < cleanedKey.Length)
            {
                cleanedKey = cleanedKey.Substring(0, i);
            }

            return cleanedKey;
        }
    }
}
