/// <file>LangDict.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 12th, 2026</date>

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
        // --------------------------------------------------------------------------
        // User-editable keyword lists — new languages or synonyms can be added here
        // --------------------------------------------------------------------------

        private static readonly (string key, string value)[] lstMonthRangeKeywords =
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
            ("mes próximo", "next"),
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
            ("mois precedent", "last"),
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

        private static readonly (string key, string value)[] lstYearRangeKeywords =
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
            ("annee passee", "last"),
            ("annee precedente", "last"),
            ("lannee precedente", "last"),
            ("annee d avant", "last"),
            ("lannee d avant", "last"),
            ("an dernier", "last"),
            ("lan dernier", "last"),
            ("ano pasado", "last"),
            ("año pasado", "last"),
            ("ano anterior", "last"),
            ("año anterior", "last"),
            ("el ano pasado", "last"),
            ("el año pasado", "last"),
            ("el ano anterior", "last"),
            ("el año anterior", "last")
        };

        // Relative units (day/week/month/year)
        private static readonly (string key, string value)[] lstRelativeUnits =
        {
            ("day", "day"), ("days", "day"), ("jour", "day"), ("jours", "day"),
            ("dia", "day"), ("dias", "day"), ("día", "day"), ("días", "day"),
            ("día", "day"), ("días", "day"), ("journée", "day"), ("journee", "day"),

            ("week", "week"), ("weeks", "week"), ("semaine", "week"), ("semaines", "week"),
            ("semana", "week"), ("semanas", "week"),

            ("month", "month"), ("months", "month"), ("mois", "month"),
            ("mes", "month"), ("meses", "month"),

            ("year", "year"), ("years", "year"), ("annee", "year"), ("annees", "year"),
            ("an", "year"), ("ans", "year"), ("año", "year"), ("años", "year")
        };

        // Relative directions (before / after)
        private static readonly (string key, int value)[] lstRelativeDirections =
        {
            ("before", -1), ("avant", -1), ("antes", -1),
            ("after", +1), ("apres", +1), ("après", +1),
            ("despues", +1), ("después", +1), ("earlier", -1), 
            ("later", +1), ("plus tot", -1), ("plus tôt", -1),
            ("plus tard", +1)
        };

        // Start keywords (in / dans / en)
        private static readonly string[] lstRelativeStartKeywords =
        {
            "in", "within", "dans", "en", "dici", "dentro", "dentro de"
        };

        // Next weekday keywords
        private static readonly string[] lstNextWeekdayKeywords =
        {
            "next", "upcoming", "following", "coming", "the next",
            "prochain", "prochaine", "suivant", "suivante",
            "proximo", "próximo", "siguiente", "subsiguiente", "posterior",
             "el siguiente"

        };

        // Previous weekday keywords
        private static readonly string[] lstPreviousWeekdayKeywords =
        {
            "last", "past", "previous", "the previous",
            "passe", "passé", "passee", "passée", "dernier", "derniere",
            "precedent", "précédent", "precedente", "précédente",
            "pasado", "anterior", "previo", "el previo"

        };

        // Priority keywords
        private static readonly (string key, string value)[] lstPriorities =
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

        // Day words
        private static readonly string[] lstDayWords =
        {
            "day", "jour", "journée", "journee", "dia", "día"
        };

        // Ordinal suffixes
        private static readonly string[] lstOrdinalSuffixes =
        {
            "er", "eme", "ème", "e",
            "st", "nd", "rd", "th",
            "ro", "do", "to"
        };

        // -----------------------------------------------------------------------------
        // Language-specific dictionaries - new languages or synonyms can be added here
        // -----------------------------------------------------------------------------

        private static readonly (string key, int value)[] monthDict =
        {
            ("january", 1), ("janvier", 1), ("enero", 1),
            ("february", 2), ("fevrier", 2), ("février", 2), ("febrero", 2),
            ("march", 3), ("mars", 3), ("marzo", 3),
            ("april", 4), ("avril", 4), ("abril", 4),
            ("may", 5), ("mai", 5), ("mayo", 5),
            ("june", 6), ("juin", 6), ("junio", 6),
            ("july", 7), ("juillet", 7), ("julio", 7),
            ("august", 8), ("aout", 8), ("août", 8), ("agosto", 8),
            ("september", 9), ("septembre", 9), ("septiembre", 9),
            ("october", 10), ("octobre", 10), ("octubre", 10),
            ("november", 11), ("novembre", 11), ("noviembre", 11),
            ("december", 12), ("decembre", 12), ("décembre", 12), ("diciembre", 12)
        };

        private static readonly (string key, DayOfWeek value)[] weekdayDict =
        {
            ("monday", DayOfWeek.Monday), ("lundi", DayOfWeek.Monday), ("lunes", DayOfWeek.Monday),
            ("tuesday", DayOfWeek.Tuesday), ("mardi", DayOfWeek.Tuesday), ("martes", DayOfWeek.Tuesday),
            ("wednesday", DayOfWeek.Wednesday), ("mercredi", DayOfWeek.Wednesday), ("miercoles", DayOfWeek.Wednesday),
            ("thursday", DayOfWeek.Thursday), ("jeudi", DayOfWeek.Thursday), ("jueves", DayOfWeek.Thursday),
            ("friday", DayOfWeek.Friday), ("vendredi", DayOfWeek.Friday), ("viernes", DayOfWeek.Friday),
            ("saturday", DayOfWeek.Saturday), ("samedi", DayOfWeek.Saturday), ("sabado", DayOfWeek.Saturday),
            ("sunday", DayOfWeek.Sunday), ("dimanche", DayOfWeek.Sunday), ("domingo", DayOfWeek.Sunday)
        };

        private static readonly (string key, int value)[] numberUnitDict =
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

        private static readonly (string key, int value)[] numberTenDict =
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
            ("dieciseis", 16), ("dieciséis", 16),
            ("veinte", 20), ("treinta", 30), ("cuarenta", 40),
            ("cincuenta", 50), ("sesenta", 60), ("setenta", 70),
            ("ochenta", 80), ("noventa", 90)
        };

        private static readonly (string key, int value)[] numberMultiplierDict =
        {
            ("cent", 100), ("cents", 100), ("centaine", 100),
            ("mille", 1000),
            ("hundred", 100), ("hundreds", 100), ("thousand", 1000),
            ("cien", 100), ("ciento", 100), ("mil", 1000)
        };

        // --------------------------------------------------------------------------------------
        // Public normalized dictionaries and sets
        //    core logic dictionaries — do not modify unless you know exactly what you are doing
        // --------------------------------------------------------------------------------------
        // 
        public static readonly HashSet<string> DayWordSet = BuildNormalizedSet(lstDayWords);
        public static readonly Dictionary<string, string> MonthRangeDict = BuildNormalizedDictionary(lstMonthRangeKeywords);
        public static readonly Dictionary<string, int> MonthNumberDict = BuildNormalizedDictionary(monthDict);
        public static readonly HashSet<string> NextWeekdayKeywordSet = BuildNormalizedSet(lstNextWeekdayKeywords);
        public static readonly Dictionary<string, int> NumberMultiplierDict = BuildNormalizedDictionary(numberMultiplierDict);
        public static readonly Dictionary<string, int> NumberTenDict = BuildNormalizedDictionary(numberTenDict);
        public static readonly Dictionary<string, int> NumberUnitDict = BuildNormalizedDictionary(numberUnitDict);
        public static readonly HashSet<string> OrdinalSuffixSet = BuildNormalizedSet(lstOrdinalSuffixes);
        public static readonly HashSet<string> PreviousWeekdayKeywordSet = BuildNormalizedSet(lstPreviousWeekdayKeywords);
        public static readonly Dictionary<string, string> PriorityKeywordDict = BuildNormalizedDictionary(lstPriorities);
        public static readonly Dictionary<string, int> TimeDirectionDict = BuildNormalizedDictionary(lstRelativeDirections);
        public static readonly HashSet<string> TimeStartKeywordSet = BuildNormalizedSet(lstRelativeStartKeywords);
        public static readonly Dictionary<string, string> TimeUnitDict = BuildNormalizedDictionary(lstRelativeUnits);
        public static readonly Dictionary<string, string> YearRangeDict = BuildNormalizedDictionary(lstYearRangeKeywords);
        public static readonly Dictionary<string, DayOfWeek> WeekdayNumberDict = BuildNormalizedDictionary(weekdayDict);

        // -------------------------------------------------------------------------------------
        // Key normalization methods — do not modify unless you know exactly what you are doing
        // -------------------------------------------------------------------------------------
        //
        
        private static Dictionary<string, TValue> BuildNormalizedDictionary<TValue>(
        IEnumerable<(string key, TValue value)> source)
        {
            // Normalizes all keys (lowercase, remove accents, Unicode‑safe)
            // and group entries that collapse to the same normalized form.
            var groupEntries = source.GroupBy(entry => NormalizeKey(entry.key));

            // For each normalized key, keep the first declared value.
            // This ensures collision tolerance: duplicates or synonyms collapse cleanly.
            var dictionary = groupEntries.ToDictionary(group => group.Key, group => group.First().value,
                StringComparer.OrdinalIgnoreCase);

            return dictionary;
        }


        private static HashSet<string> BuildNormalizedSet(IEnumerable<string> source)
        {
            return source
                .Select(x => NormalizeKey(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// This method converts any input keyword into a canonical, comparable form.
        /// </summary>
        /// <param name="inputKey"></param>
        /// <returns>the cleaned keyword</returns>

        public static string NormalizeKey(string inputKey)
        {
            // Protects against null, empty, or whitespace-only input.
            if (string.IsNullOrWhiteSpace(inputKey))
            {
                return string.Empty;
            }

            // Converts to lowercase using invariant culture
            // to ensure consistent behavior across all languages.
            string normalizedKey = inputKey.ToLowerInvariant();

            // Decomposes Unicode characters (FormD)
            // so accents become separate "non-spacing marks".
            normalizedKey = normalizedKey.Normalize(NormalizationForm.FormD);

            // Removes all non-spacing marks (accents, diacritics, etc.)
            // Example: "é" becomes "e" and "ñ" becomes "n".
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char keyChar in normalizedKey)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(keyChar) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(keyChar);
                }
            }

            // Recomposes the cleaned characters by combining base letters back into
            // their standard Unicode, ensuring the final string is canonical and safe for
            // dictionary lookups.
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}


