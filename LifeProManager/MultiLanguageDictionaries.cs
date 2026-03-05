/// <file>MultiLanguageDictionaries.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 5th, 2026</date>

using System;
using System.Collections.Generic;

namespace LifeProManager
{
    /// <summary>
    /// Central repository for all multilingual dictionaries used by SmartSearch.
    /// These lists can be extended to support new languages.
    /// </summary>
    public static class MultiLanguageDictionaries
    {
        // ---------------------------------------------------------------------
        // MONTHS
        // ---------------------------------------------------------------------
        public static readonly Dictionary<string, int> MonthDictionary =
            new Dictionary<string, int>
        {
            // January
            { "january", 1 }, { "janvier", 1 }, { "enero", 1 },

            // February
            { "february", 2 }, { "fevrier", 2 }, { "février", 2 }, { "febrero", 2 },

            // March
            { "march", 3 }, { "mars", 3 }, { "marzo", 3 },

            // April
            { "april", 4 }, { "avril", 4 }, { "abril", 4 },

            // May
            { "may", 5 }, { "mai", 5 }, { "mayo", 5 },

            // June
            { "june", 6 }, { "juin", 6 }, { "junio", 6 },

            // July
            { "july", 7 }, { "juillet", 7 }, { "julio", 7 },

            // August
            { "august", 8 }, { "aout", 8 }, { "août", 8 }, { "agosto", 8 },

            // September
            { "september", 9 }, { "septembre", 9 }, { "septiembre", 9 },

            // October
            { "october", 10 }, { "octobre", 10 }, { "octubre", 10 },

            // November
            { "november", 11 }, { "novembre", 11 }, { "noviembre", 11 },

            // December
            { "december", 12 }, { "decembre", 12 }, { "décembre", 12 }, { "diciembre", 12 }
        };

        // ---------------------------------------------------------------------
        // WEEKDAYS
        // ---------------------------------------------------------------------
        public static readonly Dictionary<string, DayOfWeek> WeekdayDictionary =
            new Dictionary<string, DayOfWeek>
        {
            // Monday
            { "monday", DayOfWeek.Monday }, { "lundi", DayOfWeek.Monday }, { "lunes", DayOfWeek.Monday },

            // Tuesday
            { "tuesday", DayOfWeek.Tuesday }, { "mardi", DayOfWeek.Tuesday }, { "martes", DayOfWeek.Tuesday },

            // Wednesday
            { "wednesday", DayOfWeek.Wednesday }, { "mercredi", DayOfWeek.Wednesday }, { "miercoles", DayOfWeek.Wednesday },

            // Thursday
            { "thursday", DayOfWeek.Thursday }, { "jeudi", DayOfWeek.Thursday }, { "jueves", DayOfWeek.Thursday },

            // Friday
            { "friday", DayOfWeek.Friday }, { "vendredi", DayOfWeek.Friday }, { "viernes", DayOfWeek.Friday },

            // Saturday
            { "saturday", DayOfWeek.Saturday }, { "samedi", DayOfWeek.Saturday }, { "sabado", DayOfWeek.Saturday },

            // Sunday
            { "sunday", DayOfWeek.Sunday }, { "dimanche", DayOfWeek.Sunday }, { "domingo", DayOfWeek.Sunday }
        };

        // ---------------------------------------------------------------------
        // ORDINAL SUFFIXES
        // ---------------------------------------------------------------------
        public static readonly HashSet<string> OrdinalSuffixes =
            new HashSet<string>
        {
            // French
            "er", "eme", "ème", "e",

            // English
            "st", "nd", "rd", "th",

            // Spanish
            "ro", "do", "to"
        };

        // ---------------------------------------------------------------------
        // RELATIVE START KEYWORDS ("in", "dans", "en")
        // ---------------------------------------------------------------------
        public static readonly HashSet<string> RelativeStartKeywords =
            new HashSet<string>
        {
            "in", "dans", "en"
        };

        // ---------------------------------------------------------------------
        // RELATIVE UNITS ("days", "jours", "dias", etc.)
        // Maps all variants to standard units: "day", "week", "month", "year"
        // ---------------------------------------------------------------------
        public static readonly Dictionary<string, string> RelativeUnits =
            new Dictionary<string, string>
        {
            // Days
            { "day", "day" }, { "days", "day" }, { "jour", "day" }, { "jours", "day" },
            { "dia", "day" }, { "dias", "day" },

            // Weeks
            { "week", "week" }, { "weeks", "week" }, { "semaine", "week" }, { "semaines", "week" },
            { "semana", "week" }, { "semanas", "week" },

            // Months
            { "month", "month" }, { "months", "month" }, { "mois", "month" },
            { "mes", "month" }, { "meses", "month" },

            // Years
            { "year", "year" }, { "years", "year" }, { "annee", "year" }, { "annees", "year" },
            { "an", "year" }, { "ans", "year" }, { "año", "year" }, { "años", "year" }
        };

        // ---------------------------------------------------------------------
        // RELATIVE DIRECTIONS ("before", "after", "avant", "después", etc.)
        // -1 = before / past
        // +1 = after / future
        // ---------------------------------------------------------------------
        public static readonly Dictionary<string, int> RelativeDirections =
            new Dictionary<string, int>
        {
            // BEFORE
            { "before", -1 }, { "avant", -1 }, { "antes", -1 },

            // AFTER
            { "after", +1 }, { "apres", +1 }, { "après", +1 },
            { "despues", +1 }, { "después", +1 }
        };

        // ---------------------------------------------------------------------
        // WEEKDAY DIRECTION — NEXT
        // ---------------------------------------------------------------------
        public static readonly HashSet<string> NextWeekdayKeywords =
            new HashSet<string>
        {
            // French
            "prochain", "prochaine", "suivant", "suivante",

            // English
            "next", "upcoming", "following",

            // Spanish
            "proximo", "próximo", "siguiente", "subsiguiente", "posterior"
        };

        // ---------------------------------------------------------------------
        // WEEKDAY DIRECTION — PREVIOUS
        // ---------------------------------------------------------------------
        public static readonly HashSet<string> PreviousWeekdayKeywords =
            new HashSet<string>
        {
            // French
            "passe", "passé", "passee", "passée",
            "dernier", "derniere",
            "precedent", "précédent", "precedente", "précédente",

            // English
            "last", "past", "previous",

            // Spanish
            "pasado", "anterior", "previo"
        };
    }
}
