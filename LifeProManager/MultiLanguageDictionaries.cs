/// <file>MultiLanguageDictionaries.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 10th, 2026</date>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LifeProManager
{
    /// <summary>
    /// Central repository for multilingual keyword dictionaries used by SmartSearch.
    /// All raw entries are stored in private lists to avoid duplicate-key exceptions.
    /// All public dictionaries are normalizedKey and built through GroupBy to ensure
    /// collision tolerance. 
    /// New languages can be added easily by extending the RAW lists below.
    /// </summary>
    public static class MultiLanguageDictionaries
    {
        // ---------------------------------------------------------------------
        //  Key normalization
        //  If new language entries are added, no need to normalize
        //  manually. The NormalizeKey method ensures consistent matching across
        //  accents, casing, and Unicode variations.
        // ---------------------------------------------------------------------

        public static string NormalizeKey(string inputKey)
        {
            if (string.IsNullOrWhiteSpace(inputKey))
            {
                return string.Empty;
            }

            string normalizedKey = inputKey.ToLowerInvariant();
            normalizedKey = normalizedKey.Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (char currentChar in normalizedKey)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(currentChar) != UnicodeCategory.NonSpacingMark)
                { 
                    stringBuilder.Append(currentChar);   
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static Dictionary<string, TValue> BuildNormalizedDictionary<TValue>(
            IEnumerable<(string key, TValue value)> source)
        {
            return source
                .GroupBy(x => NormalizeKey(x.key))
                .ToDictionary(
                    g => g.Key,
                    g => g.First().value,
                    StringComparer.OrdinalIgnoreCase
                );
        }

        private static HashSet<string> BuildNormalizedSet(IEnumerable<string> source)
        {
            return source
                .Select(x => NormalizeKey(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------------
        //  Raw data
        //  New language entries for extra languages can be added here. Duplicates are allowed.
        //  The build methods above will automatically resolve collisions.
        // ---------------------------------------------------------------------

        private static readonly (string key, string value)[] RawMonthRangeKeywords =
        {
            ("ce mois", "this"),
            ("this month", "this"),
            ("este mes", "this"),

            ("mois suivant", "next"),
            ("next month", "next"),
            ("mes siguiente", "next"),

            ("mois passé", "last"),
            ("mois dernier", "last"),
            ("last month", "last"),
            ("mes pasado", "last")
        };

        private static readonly (string key, int value)[] RawMonthDictionary =
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

        private static readonly (string key, int value)[] RawUnits =
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

        private static readonly (string key, int value)[] RawTens =
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

        private static readonly (string key, int value)[] RawMultipliers =
        {
            ("cent", 100), ("cents", 100), ("centaine", 100),
            ("mille", 1000),
            ("hundred", 100), ("hundreds", 100), ("thousand", 1000),
            ("cien", 100), ("ciento", 100), ("mil", 1000)
        };

        private static readonly (string key, string value)[] RawRelativeUnits =
        {
            ("day", "day"), ("days", "day"), ("jour", "day"), ("jours", "day"),
            ("dia", "day"), ("dias", "day"),

            ("week", "week"), ("weeks", "week"), ("semaine", "week"), ("semaines", "week"),
            ("semana", "week"), ("semanas", "week"),

            ("month", "month"), ("months", "month"), ("mois", "month"),
            ("mes", "month"), ("meses", "month"),

            ("year", "year"), ("years", "year"), ("annee", "year"), ("annees", "year"),
            ("an", "year"), ("ans", "year"), ("año", "year"), ("años", "year")
        };

        private static readonly (string key, int value)[] RawRelativeDirections =
        {
            ("before", -1), ("avant", -1), ("antes", -1),
            ("after", +1), ("apres", +1), ("après", +1),
            ("despues", +1), ("después", +1)
        };

        private static readonly (string key, string value)[] RawPrioritiesMap =
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

        private static readonly string[] RawDayWords =
        {
            "jour", "journée", "day", "dia", "día"
        };

        private static readonly string[] RawOrdinalSuffixes =
        {
            "er", "eme", "ème", "e",
            "st", "nd", "rd", "th",
            "ro", "do", "to"
        };

        private static readonly string[] RawRelativeStartKeywords =
        {
            "in", "dans", "en"
        };

        private static readonly string[] RawNextWeekdayKeywords =
        {
            "prochain", "prochaine", "suivant", "suivante",
            "next", "upcoming", "following",
            "proximo", "próximo", "siguiente", "subsiguiente", "posterior"
        };

        private static readonly string[] RawPreviousWeekdayKeywords =
        {
            "passe", "passé", "passee", "passée",
            "dernier", "derniere",
            "precedent", "précédent", "precedente", "précédente",
            "last", "past", "previous",
            "pasado", "anterior", "previo"
        };

        private static readonly (string key, DayOfWeek value)[] RawWeekdayDictionary =
        {
            ("monday", DayOfWeek.Monday), ("lundi", DayOfWeek.Monday), ("lunes", DayOfWeek.Monday),
            ("tuesday", DayOfWeek.Tuesday), ("mardi", DayOfWeek.Tuesday), ("martes", DayOfWeek.Tuesday),
            ("wednesday", DayOfWeek.Wednesday), ("mercredi", DayOfWeek.Wednesday), ("miercoles", DayOfWeek.Wednesday),
            ("thursday", DayOfWeek.Thursday), ("jeudi", DayOfWeek.Thursday), ("jueves", DayOfWeek.Thursday),
            ("friday", DayOfWeek.Friday), ("vendredi", DayOfWeek.Friday), ("viernes", DayOfWeek.Friday),
            ("saturday", DayOfWeek.Saturday), ("samedi", DayOfWeek.Saturday), ("sabado", DayOfWeek.Saturday),
            ("sunday", DayOfWeek.Sunday), ("dimanche", DayOfWeek.Sunday), ("domingo", DayOfWeek.Sunday)
        };

        // ---------------------------------------------------------------------------------------
        //  Public normalizedKey dictionaries
        //  New entries for new languages should be added in RAW lists only.
        // ----------------------------------------------------------------------------------------

        public static readonly HashSet<string> DayWords = BuildNormalizedSet(RawDayWords);
        public static readonly HashSet<string> OrdinalSuffixes = BuildNormalizedSet(RawOrdinalSuffixes);
        public static readonly HashSet<string> RelativeStartKeywords = BuildNormalizedSet(RawRelativeStartKeywords);
        public static readonly HashSet<string> NextWeekdayKeywords = BuildNormalizedSet(RawNextWeekdayKeywords);
        public static readonly HashSet<string> PreviousWeekdayKeywords = BuildNormalizedSet(RawPreviousWeekdayKeywords);

        public static readonly Dictionary<string, int> Units = BuildNormalizedDictionary(RawUnits);
        public static readonly Dictionary<string, int> Tens = BuildNormalizedDictionary(RawTens);
        public static readonly Dictionary<string, int> Multipliers = BuildNormalizedDictionary(RawMultipliers);
        public static readonly Dictionary<string, int> MonthDictionary = BuildNormalizedDictionary(RawMonthDictionary);
        public static readonly Dictionary<string, string> MonthRangeKeywords = BuildNormalizedDictionary(RawMonthRangeKeywords);
        public static readonly Dictionary<string, string> RelativeUnits = BuildNormalizedDictionary(RawRelativeUnits);
        public static readonly Dictionary<string, int> RelativeDirections = BuildNormalizedDictionary(RawRelativeDirections);
        public static readonly Dictionary<string, DayOfWeek> WeekdayDictionary = BuildNormalizedDictionary(RawWeekdayDictionary);
        public static readonly Dictionary<string, string> PrioritiesMap = BuildNormalizedDictionary(RawPrioritiesMap);
    }
}

