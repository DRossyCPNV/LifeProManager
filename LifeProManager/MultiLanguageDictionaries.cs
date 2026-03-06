/// <file>MultiLanguageDictionaries.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 6th, 2026</date>

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
        public static readonly HashSet<string> DayWords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // FR
            "jour", "journée",

            // EN
            "day",

            // ES
            "dia", "día"
        };

        public static readonly Dictionary<string, int> Multipliers =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // FR
            { "cent", 100 }, { "cents", 100 },
            { "centaine", 100 },
            { "mille", 1000 },

            // EN
            { "hundred", 100 }, { "hundreds", 100 },
            { "thousand", 1000 },

            // ES
            { "cien", 100 }, { "ciento", 100 },
            { "mil", 1000 }
        };

        // Months
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

        public static readonly Dictionary<string, string> MonthRangeKeywords =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // this month
            { "ce mois", "this" },
            { "this month", "this" },
            { "este mes", "this" },

            // next month
            { "mois suivant", "next" },
            { "next month", "next" },
            { "mes siguiente", "next" },

            // last month
            { "mois passé", "last" },
            { "mois dernier", "last" },
            { "last month", "last" },
            { "mes pasado", "last" }
        };

        // Weekday direction — Next
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

        // Weekday direction — Previous
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

        // Semantic priority filters
        // Maps multilingual keywords to priority IDs.
        public static readonly Dictionary<string, string> PrioritiesMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Important / urgent
            { "important", "important" },
            { "importante", "important" },
            { "importantes", "important" },
            { "urgent", "important" },
            { "urgente", "important" },
            { "urgentes", "important" },
            { "essential", "important" },
            { "essentiel", "important" },
            { "essentielle", "important" },
            { "essentielles", "important" },
            { "essentials", "important" },
            { "critical", "important" },
            { "critique", "important" },
            { "crucial", "important" },
            { "priority", "important" },
            { "priorite", "important" },
            { "priorité", "important" },
            { "high", "important" },

            // Anniversary
            { "anniversaire", "anniversary" },
            { "anniversaires", "anniversary" },
            { "anniv", "anniversary" },
            { "anni", "anniversary" },
            { "fete", "anniversary" },
            { "fêtes", "anniversary" },
            { "anniversary", "anniversary" },
            { "birthday", "anniversary" },
            { "birthdays", "anniversary" },
            { "bday", "anniversary" },
            { "b-day", "anniversary" },
            { "cumpleanos", "anniversary" },
            { "cumpleaños", "anniversary" }
         };

        // Ordinal suffixes
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

        // Relative start keywords ("in", "dans", "en")
        public static readonly HashSet<string> RelativeStartKeywords =
            new HashSet<string>
        {
            "in", "dans", "en"
        };

        // Relative units ("days", "jours", "dias", etc.)
        // Maps all variants to standard units: "day", "week", "month", "year"
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

        // Relative directions ("before", "after", "avant", "después", etc.)
        // -1 = before / past
        // +1 = after / future
        public static readonly Dictionary<string, int> RelativeDirections =
            new Dictionary<string, int>
        {
            // Before
            { "before", -1 }, { "avant", -1 }, { "antes", -1 },

            // After
            { "after", +1 }, { "apres", +1 }, { "après", +1 },
            { "despues", +1 }, { "después", +1 }
        };

        public static readonly Dictionary<string, int> Tens =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // FR
            { "dix", 10 },
            { "onze", 11 }, { "douze", 12 }, { "treize", 13 }, { "quatorze", 14 },
            { "quinze", 15 }, { "seize", 16 },
            { "vingt", 20 },
            { "trente", 30 },
            { "quarante", 40 },
            { "cinquante", 50 },
            { "soixante", 60 },

            // Switzerland specific terms
            { "septante", 70 },
            { "huitante", 80 },
            { "nonante", 90 },

            // France specific terms
            { "quatrevingt", 80 },
            { "quatre-vingt", 80 },
            { "quatre vingt", 80 },

            // EN
            { "ten", 10 }, { "eleven", 11 }, { "twelve", 12 },
            { "twenty", 20 }, { "thirty", 30 }, { "forty", 40 }, { "fifty", 50 },
            { "sixty", 60 }, { "seventy", 70 }, { "eighty", 80 }, { "ninety", 90 },

            // ES
            { "diez", 10 }, { "once", 11 }, { "doce", 12 }, { "trece", 13 },
            { "catorce", 14 }, { "quince", 15 }, { "dieciseis", 16 }, { "dieciséis", 16 },
            { "veinte", 20 }, { "treinta", 30 }, { "cuarenta", 40 },
            { "cincuenta", 50 }, { "sesenta", 60 }, { "setenta", 70 },
            { "ochenta", 80 }, { "noventa", 90 }
        };

        public static readonly Dictionary<string, int> Units =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // FR
            { "zero", 0 }, { "zéro", 0 },
            { "un", 1 }, { "une", 1 },
            { "deux", 2 },
            { "trois", 3 },
            { "quatre", 4 },
            { "cinq", 5 },
            { "six", 6 },
            { "sept", 7 },
            { "huit", 8 },
            { "neuf", 9 },

            // EN
            { "zero", 0 },
            { "one", 1 },
            { "two", 2 },
            { "three", 3 },
            { "four", 4 },
            { "five", 5 },
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 },
            { "nine", 9 },

            // ES
            { "cero", 0 },
            { "uno", 1 }, { "una", 1 },
            { "dos", 2 },
            { "tres", 3 },
            { "cuatro", 4 },
            { "cinco", 5 },
            { "seis", 6 },
            { "siete", 7 },
            { "ocho", 8 },
            { "nueve", 9 }
        };

        // Weekdays
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
    }
}
