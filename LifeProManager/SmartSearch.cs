/// <file>SmartSearch.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 6th, 2026</date>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace LifeProManager
{
    /// <summary>
    /// SmartSearch is the multilingual natural‑language search engine.
    /// All language‑specific data (months, weekdays, ordinal suffixes,
    /// number words, relative keywords…) is stored in MultiLanguageDictionaries.
    ///
    /// To add support for a new language:
    /// 1) Add ordinal suffixes in MultiLanguageDictionaries.OrdinalSuffixes.
    /// 2) Add weekday names in MultiLanguageDictionaries.WeekdayWords.
    /// 3) Add before/after keywords in MultiLanguageDictionaries.BeforeWords / AfterWords.
    /// 4) Add month names in MultiLanguageDictionaries.MonthWords.
    /// 5) Add number words (units, tens, multipliers) for TryParseNumberWord.
    /// 6) Add absolute keywords (today, tomorrow, yesterday…).
    /// 7) Add relative keywords (in, ago, dans, hace…).
    ///
    /// No modification is required in SmartSearch itself.
    /// The parsing logic is fully language‑agnostic and relies exclusively
    /// on the dictionaries for linguistic variation.
    /// </summary>
    public class SmartSearch
    {
        // Reuses the global DB connection created in Program.cs
        public DBConnection dbConn => Program.DbConn;

        public SmartSearch()
        {

        }

        /// <summary>
        /// Applies a relative quantity to a standard time unit ("day", "week", "month", "year").
        /// The quantity can be positive (after) or negative (before).
        /// </summary>
        private bool ApplyRelativeUnitGeneric(int relativeQty, string standardUnit, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            // Days: simple offset on the current date
            if (standardUnit == "day")
            {
                startDateTime = now.Date.AddDays(relativeQty);
                endDateTime = startDateTime;
                return true;
            }

            // Weeks: relative quantity * 7 days, end date covers the full week (6 extra days)
            if (standardUnit == "week")
            {
                startDateTime = now.Date.AddDays(relativeQty * 7);
                endDateTime = startDateTime.Value.AddDays(6);
                return true;
            }

            // Months: uses AddMonths to keep calendar semantics
            if (standardUnit == "month")
            {
                startDateTime = now.Date.AddMonths(relativeQty);
                endDateTime = startDateTime;
                return true;
            }

            // Years: uses AddYears to keep calendar semantics
            if (standardUnit == "year")
            {
                startDateTime = now.Date.AddYears(relativeQty);
                endDateTime = startDateTime;
                return true;
            }

            // Unknown unit: nothing applied
            return false;
        }

        /// <summary>
        /// Builds the Sql Where clause used by SmartSearch to fetch candidate tasks.
        /// The condition combines text, date, month and semantic priority filters.
        /// All parts are joined with And so the database returns only tasks
        /// that match the text and the time constraints.
        /// </summary>
        private string BuildSqlWhere(List<string> lstExpandedTokens, DateTime? startDate,
            DateTime? endDate, DateTime? detectedMonth, string priorityCategory)
        {
            // Stores all Sql fragments before joining them
            List<string> lstSqlConditions = new List<string>();

            // Text search
            if (lstExpandedTokens != null && lstExpandedTokens.Count > 0)
            {
                List<string> lstTokenConditions = new List<string>();

                foreach (string token in lstExpandedTokens)
                {
                    string sqlTitleCondition = "title LIKE '%" + token + "%'";
                    string sqlDescriptionCondition = "description LIKE '%" + token + "%'";

                    lstTokenConditions.Add("(" + sqlTitleCondition + " OR " + sqlDescriptionCondition + ")");
                }

                // Joins all token conditions
                string sqlTokensCombined = "(" + string.Join(" OR ", lstTokenConditions) + ")";

                // Adds the token block to the global condition list
                lstSqlConditions.Add(sqlTokensCombined);
            }

            // Explicit date range
            if (startDate.HasValue && endDate.HasValue)
            {
                string sqlDateRangeCondition = "(deadline >= '" + startDate.Value.ToString("yyyy-MM-dd") +
                    "' AND deadline <= '" + endDate.Value.ToString("yyyy-MM-dd") + "')";

                lstSqlConditions.Add(sqlDateRangeCondition);
            }

            // MonthFilter
            if (detectedMonth.HasValue)
            {
                DateTime monthStart = detectedMonth.Value;
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                string sqlMonthCondition = "(deadline >= '" + monthStart.ToString("yyyy-MM-dd") +
                    "' AND deadline <= '" + monthEnd.ToString("yyyy-MM-dd") + "')";

                lstSqlConditions.Add(sqlMonthCondition);
            }

            // Semantic priority filters
            if (!string.IsNullOrEmpty(priorityCategory))
            {
                if (priorityCategory == "important")
                {
                    // Important (1) OR important and repeatable (3)
                    lstSqlConditions.Add("Priorities_id IN (1,3)");
                }
                else if (priorityCategory == "anniversary")
                {
                    lstSqlConditions.Add("Priorities_id = 4");
                }
            }

            // Final assembly
            if (lstSqlConditions.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" AND ", lstSqlConditions);
        }

        /// <summary>
        /// Computes the Levenshtein distance between two strings.
        /// The Levenshtein distance represents the minimum number of
        /// single‑character edits (insertions, deletions, substitutions)
        /// required to transform one string into the other.
        /// </summary>
        private static int CalculatesLevenshteinDistance(string source, string target)
        {
            // Returns a very large value if either string is null
            if (source == null || target == null)
            {
                return int.MaxValue;
            }

            int sourceLength = source.Length;
            int targetLength = target.Length;

            // If the source is empty, the distance is the number of insertions needed
            if (sourceLength == 0)
            {
                return targetLength;
            }

            // If the target is empty, the distance is the number of deletions needed
            if (targetLength == 0)
            {
                return sourceLength;
            }

            // Creates a 2D matrix where each cell represents a subproblem
            // of transforming substrings of source and target
            int[,] distanceMatrix = new int[sourceLength + 1, targetLength + 1];

            // Initializes the first column by transforming source into an empty string
            for (int indexSource = 0; indexSource <= sourceLength; indexSource++)
            {
                // Cost of deleting characters from source
                distanceMatrix[indexSource, 0] = indexSource;
            }

            // Initializes the first row by transforming an empty string into target
            for (int indexTarget = 0; indexTarget <= targetLength; indexTarget++)
            {
                // Cost of inserting characters into source
                distanceMatrix[0, indexTarget] = indexTarget;
            }

            // Iterates through each character of the source string
            for (int indexSource = 1; indexSource <= sourceLength; indexSource++)
            {
                // Iterates through each character of the target string
                for (int indexTarget = 1; indexTarget <= targetLength; indexTarget++)
                {
                    // Determines whether the current characters match and sets the substitution cost accordingly
                    int substitutionCost = (source[indexSource - 1] == target[indexTarget - 1]) ? 0 : 1;

                    // Computes the cost of deleting a character from the source string
                    int deletionCost = distanceMatrix[indexSource - 1, indexTarget] + 1;

                    // Computes the cost of inserting a character into the source string
                    int insertionCost = distanceMatrix[indexSource, indexTarget - 1] + 1;

                    // Computes the cost of substituting one character for another in the source string
                    int substitutionTotalCost = distanceMatrix[indexSource - 1, indexTarget - 1] + substitutionCost;

                    // Selects the minimum cost among deletion, insertion, and substitution
                    int minimumCost = Math.Min(Math.Min(deletionCost, insertionCost), substitutionTotalCost);

                    // Stores the computed minimum cost in the matrix
                    distanceMatrix[indexSource, indexTarget] = minimumCost;
                }
            }

            // Returns the final computed distance (bottom‑right cell of the matrix)
            return distanceMatrix[sourceLength, targetLength];
        }

        /// <summary>
        /// Cleans the raw user query by removing extra spaces, trimming,
        /// normalizing separators, and removing useless punctuation.
        /// </summary>
        private string CleanQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            string cleanedQuery = query;

            // Trims leading/trailing spaces
            cleanedQuery = cleanedQuery.Trim();

            // Replaces multiple spaces with a single space
            // \s+ matches one or more whitespace characters
            cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ");

            // Normalizes separators (commas, semicolons, slashes) by replacing them with spaces
            cleanedQuery = cleanedQuery.Replace(",", " ")
                             .Replace(";", " ")
                             .Replace("/", " ");

            // Removes useless punctuation (except + which is meaningful)
            // [!?:()\[\]{}""'’] matches any of the listed characters
            cleanedQuery = Regex.Replace(cleanedQuery, @"[!?:()\[\]{}""'’]", "");

            // Protect AND / OR operators by spacing them
            // \bAND\b matches "AND" as a whole word, ignoring case
            cleanedQuery = Regex.Replace(cleanedQuery, @"\bAND\b", " AND ", RegexOptions.IgnoreCase);
            cleanedQuery = Regex.Replace(cleanedQuery, @"\bOR\b", " OR ", RegexOptions.IgnoreCase);

            // Normalizes multiple spaces again after replacements
            cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ");

            return cleanedQuery.Trim();
        }

        /// <summary>
        /// Detects whether the user query contains a month name in all supported languages
        /// and returns the first day of the detected month, or null if none is found.
        /// </summary>
        private DateTime? DetectMonth(List<string> lstTokens)
        {
            if (lstTokens == null || lstTokens.Count == 0)
            {
                return null;
            }

            foreach (string token in lstTokens)
            {
                string normalizedToken = token.Trim();

                if (MultiLanguageDictionaries.MonthDictionary.ContainsKey(normalizedToken))
                {
                    // Stores the month number corresponding to the detected month name
                    int monthNumber = MultiLanguageDictionaries.MonthDictionary[normalizedToken];

                    // Stores the detected month as a DateTime object, using the current year
                    // and the first day of the month (the day is not relevant for filtering tasksFound by month)
                    DateTime detectedMonth = new DateTime(DateTime.Now.Year, monthNumber, 1);

                    return detectedMonth;
                }
            }

            return null;
        }

        /// <summary>
        /// Generates typo‑tolerant variantTokens for each token using Levenshtein distance (≤ 2).
        /// </summary>
        /// <param name="lstTokens">The list of original tokens extracted from the query.</param>
        private List<string> ExpandTokensLevenshtein(List<string> lstTokens)
        {
            List<string> lstExpandedTokens = new List<string>();

            if (lstTokens == null || lstTokens.Count == 0)
            {
                return lstExpandedTokens;
            }

            foreach (string token in lstTokens)
            {
                lstExpandedTokens.Add(token);

                // Generates variantTokens for the current token
                List<string> lstVariants = GenerateLevenshteinVariants(token);

                foreach (string variant in lstVariants)
                {
                    // Calculates the Levenshtein distance between the original token and the tokenVariant
                    int distance = CalculatesLevenshteinDistance(token, variant);

                    if (distance <= 2 && lstExpandedTokens.Contains(variant) == false)
                    {
                        lstExpandedTokens.Add(variant);
                    }
                }
            }

            return lstExpandedTokens;
        }

        /// <summary>
        /// Generates typo‑tolerant variantTokens for a given token.
        /// This method produces simple character‑level mutations (insert, delete, replace)
        /// which are later filtered by Levenshtein distance ≤ 2.
        /// </summary>
        private List<string> GenerateLevenshteinVariants(string token)
        {
            List<string> variantTokens = new List<string>();

            if (string.IsNullOrWhiteSpace(token))
            {
                return variantTokens;
            }

            string alphabet = "abcdefghijklmnopqrstuvwxyz";

            // Deletions (removes one character)
            for (int charToDelete = 0; charToDelete < token.Length; charToDelete++)
            {
                variantTokens.Add(token.Remove(charToDelete, 1));
            }

            // Insertions (inserts a character at any position)
            for (int charToInsert = 0; charToInsert <= token.Length; charToInsert++)
            {
                foreach (char letter in alphabet)
                {
                    variantTokens.Add(token.Insert(charToInsert, letter.ToString()));
                }
            }

            // Substitutions (replaces one character)
            for (int charToSubstitue = 0; charToSubstitue < token.Length; charToSubstitue++)
            {
                foreach (char letter in alphabet)
                {
                    if (token[charToSubstitue] != letter)
                    {
                        string mutated = token.Substring(0, charToSubstitue) + letter + token.Substring(charToSubstitue + 1);
                        variantTokens.Add(mutated);
                    }
                }
            }

            return variantTokens;
        }

        /// <summary>
        /// Computes the target weekday relative to the current date.
        /// - directionSign = +1 : next weekday (prochain / next / siguiente)
        /// - directionSign = -1 : previous weekday (dernier / last / pasado)
        /// 
        /// Example:
        /// If today is Wednesday and targetDayOfWeek = Monday:
        /// - next Monday returns date + (5 days)
        /// - last Monday returns date - (2 days)
        /// </summary>
        private DateTime GetRelativeWeekday(DateTime now, DayOfWeek targetDayOfWeek, int directionSign)
        {
            // Current weekday as integer (0 = Sunday, 1 = Monday, ...)
            int currentDay = (int)now.DayOfWeek;
            int targetDay = (int)targetDayOfWeek;

            // Computes raw difference
            int dayDifference = targetDay - currentDay;

            if (directionSign > 0)
            {
                // Next weekday
                if (dayDifference <= 0)
                {
                    dayDifference += 7;
                }
            }
            else
            {
                // Previous weekday
                if (dayDifference >= 0)
                {
                    dayDifference -= 7;
                }
            }

            // Returns the date corresponding to the target weekday
            // by adding the day difference to the current date
            return now.Date.AddDays(dayDifference);
        }

        /// <summary>
        /// Normalizes the cleaned query by converting it to lowercase and removing accents.
        /// This ensures consistent token extraction and matching in later stages.
        /// </summary>
        private static string NormalizeQuery(string cleanedQuery)
        {
            if (string.IsNullOrWhiteSpace(cleanedQuery))
            {
                return string.Empty;
            }

            string normalizedQuery = cleanedQuery.ToLowerInvariant();

            // Normalizes to FormD (decomposed) to remove the diacritics while keeping the base characters intact.
            normalizedQuery = normalizedQuery.Normalize(NormalizationForm.FormD);

            // Removes diacritic marks (accents)
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char charFromQuery in normalizedQuery)
            {
                UnicodeCategory unicodeCat = CharUnicodeInfo.GetUnicodeCategory(charFromQuery);

                // Keeps only characters that are not non-spacing marks (accents)
                if (unicodeCat != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(charFromQuery);
                }
            }

            // Recomposes to FormC (standard)
            normalizedQuery = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // French natural date: "demain" → English equivalent "tomorrow"
            if (normalizedQuery.Contains("demain"))
            {
                normalizedQuery += " tomorrow";
            }

            // French natural date: "hier" → English equivalent "yesterday"
            if (normalizedQuery.Contains("hier"))
            {
                normalizedQuery += " yesterday";
            }

            // Generic French typo correction for "bureau" (Levenshtein ≤ 2)
            string[] bureauVariants = { "bureau" };
            foreach (string variant in bureauVariants)
            {
                if (CalculatesLevenshteinDistance(normalizedQuery, variant) <= 2)
                {
                    normalizedQuery += " bureau";
                    break;
                }
            }

            return normalizedQuery;
        }

        /// <summary>
        /// Creates a dummy task indicating that the search crashed.
        /// This ensures the UI always receives a valid, non-empty result list.
        /// </summary>
        private List<Tasks> NotifyCrashResult()
        {
            Tasks crashTask = new Tasks
            {
                Id = -2,
                Title = LocalizationManager.GetString("SearchCrashedNoResults"),
                Description = string.Empty,
                Deadline = null
            };

            return new List<Tasks> { crashTask };
        }

        /// <summary>
        /// Parses natural-language date expressions from both title tokens and description text.
        /// This unified version looks for :
        /// - Numeric dates (14/03/2026, 2026-04-21, 14/03…)
        /// - Absolute keywords (today, tomorrow, yesterday…)
        /// - Month expressions (this month, next month, last month…)
        /// - Year expressions (this year, next year, last year…)
        /// - Relative expressions (in X days, X weeks before…)
        /// - Ordinal dates (1st March, 2do abril, 7eme jour…)
        /// - Explicit years (2024, 2025…)
        /// - Weekday expressions (next Monday, lundi prochain…)
        /// The method first analyzes the title tokens, then the description tokens.
        /// Each handler is language-agnostic and uses MultiLanguageDictionaries as reference.
        /// </summary>
        public (DateTime? startDate, DateTime? endDate) ParseNaturalDates(List<string> titleTokens,
            string description, DateTime now)
        {
            DateTime? startDateTime = null;
            DateTime? endDateTime = null;

            // Tries parsing date from title tokens
            if (TryParseDateTokens(titleTokens, now, out startDateTime, out endDateTime))
            {
                return (startDateTime, endDateTime);
            }

            // Tokenizes description (raw text)
            List<string> descriptionTokens = TokenizeQuery(description);

            // Tries parsing date from description tokens
            if (TryParseDateTokens(descriptionTokens, now, out startDateTime, out endDateTime))
            {
                return (startDateTime, endDateTime);
            }

            return (null, null);
        }

        /// <summary>
        /// Extracts a semantic priority category from the token list based on multilingual
        /// keywords, returning "important", "anniversary", or null when no priority is implied.
        /// </summary>
        private string ParsePriorityFilters(List<string> lstTokens)
        {
            if (lstTokens == null || lstTokens.Count == 0)
            {
                return null;
            }

            foreach (string token in lstTokens)
            {
                if (MultiLanguageDictionaries.PrioritiesMap.TryGetValue(token, out string priorityDefined))
                {
                    return priorityDefined;
                }
            }

            return null;
        }


        /// <summary>
        /// Computes a relevance score for each task using:
        /// - exact matches in title and description
        /// - typo‑tolerant matches (Levenshtein distance)
        /// - match density (how many tokens appear)
        /// - token order (query order preserved)
        /// - deadline proximity (today, overdue, near, same month)
        /// </summary>
        private List<ScoredTask> ScoreCandidates(List<Tasks> candidateTasks,
            List<string> tokens, List<string> expandedTokens)
        {
            if (candidateTasks == null || candidateTasks.Count == 0)
            {
                return new List<ScoredTask>();
            }

            // Scoring weights for each scoring dimension
            var scoringWeight = new Dictionary<string, int>
            {
                ["ExactTitle"] = 40,
                ["ExactDescription"] = 25,
                ["ExpandedTitle"] = 12,
                ["ExpandedDescription"] = 8,
                ["Lev1"] = 6,
                ["Lev2"] = 3,
                ["Density"] = 4,
                ["TokenOrder"] = 10,
                ["FullCoverage"] = 20,
                ["DeadlineToday"] = 25,
                ["DeadlineOverdue"] = 30,
                ["DeadlineNear"] = 18,
                ["DeadlineSameMonth"] = 10
            };

            List<ScoredTask> scoredTasks = new List<ScoredTask>();

            foreach (Tasks task in candidateTasks)
            {
                // Normalize title and description
                string taskTitle = NormalizeQuery(CleanQuery(task.Title ?? ""));
                string taskDescription = NormalizeQuery(CleanQuery(task.Description ?? ""));

                // All words used for Levenshtein comparison
                var allWords = taskTitle.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Concat(taskDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                        .ToList();

                int exactMatchDensity = 0;          // Counts how many original tokens have an exact match in title/description
                bool tokensRespectOrder = true;     // Checks if tokens appear in the same order as the query
                int lastTokenIndex = -1;            // Tracks the index of the last matched token to verify order

                // Token scoring
                int totalScore = expandedTokens.Sum(expandedToken =>
                {
                    bool isExactToken = tokens.Contains(expandedToken);
                    int tokenScore = 0;

                    // Exact / expanded matches
                    if (taskTitle.Contains(expandedToken))
                    {
                        tokenScore += scoringWeight[isExactToken ? "ExactTitle" : "ExpandedTitle"];
                    }

                    if (taskDescription.Contains(expandedToken))
                    {
                        tokenScore += scoringWeight[isExactToken ? "ExactDescription" : "ExpandedDescription"];
                    }

                    // Density count
                    if (isExactToken && (taskTitle.Contains(expandedToken) || taskDescription.Contains(expandedToken)))
                    {
                        exactMatchDensity++;
                    }

                    // Levenshtein typo tolerance
                    if (isExactToken)
                    {
                        int bestDistance = allWords
                            .Select(word => CalculatesLevenshteinDistance(expandedToken, word))
                            .DefaultIfEmpty(int.MaxValue)
                            .Min();

                        if (bestDistance == 1)
                        {
                            tokenScore += scoringWeight["Lev1"];
                        }

                        else if (bestDistance == 2)
                        {
                            tokenScore += scoringWeight["Lev2"];
                        }
                    }

                    // Token order detection
                    int indexInTitle = taskTitle.IndexOf(expandedToken);
                    int indexInDesc = taskDescription.IndexOf(expandedToken);
                    int tokenIndex = (indexInTitle >= 0 ? indexInTitle : indexInDesc);

                    if (tokenIndex >= 0)
                    {
                        if (lastTokenIndex > tokenIndex)
                            tokensRespectOrder = false;

                        lastTokenIndex = tokenIndex;
                    }

                    return tokenScore;
                });

                // Density bonus
                totalScore += exactMatchDensity * scoringWeight["Density"];

                // All tokens appear somewhere
                if (tokens.All(t => taskTitle.Contains(t) || taskDescription.Contains(t)))
                    totalScore += scoringWeight["FullCoverage"];

                // Token order bonus
                if (tokensRespectOrder)
                    totalScore += scoringWeight["TokenOrder"];

                // Deadline scoring
                if (!string.IsNullOrWhiteSpace(task.Deadline) &&
                    DateTime.TryParse(task.Deadline, out DateTime deadline))
                {
                    DateTime today = DateTime.Today;
                    deadline = deadline.Date;

                    if (deadline == today)
                    {
                        totalScore += scoringWeight["DeadlineToday"];
                    }

                    if (deadline < today)
                    {
                        totalScore += scoringWeight["DeadlineOverdue"];
                    }

                    if (Math.Abs((deadline - today).TotalDays) <= 2)
                    {
                        totalScore += scoringWeight["DeadlineNear"];
                    }

                    if (deadline.Month == today.Month)
                    {
                        totalScore += scoringWeight["DeadlineSameMonth"];
                    }
                }

                scoredTasks.Add(new ScoredTask
                {
                    Task = task,
                    Score = totalScore
                });
            }

            return scoredTasks;
        }

        /// <summary>
        /// Executes the search in a controlled manner.  
        ///
        /// Pipeline overview:
        /// - Cleans and normalizes the raw query
        /// - Tokenizes the query into lexical units
        /// - Expands tokens using Levenshtein distance to tolerate typos
        /// - Extracts natural date expressions (absolute and relative)
        /// - Detects semantic priority categories 
        /// - Detects month filters
        /// - Builds a Sql Where clause based on extracted filters
        /// - Retrieves matching tasks from the database
        /// - Scores and sorts tasks by relevance
        ///
        /// Any unexpected failure in any stage results in an immediate safe exit
        /// with a single dummy task describing the crash.
        /// </summary>
        public List<Tasks> Search(string rawQuery)
        {
            // Cleans the raw query by removing punctuation and unnecessary whitespace.
            string cleanedQuery;
            
            try
            {
                cleanedQuery = CleanQuery(rawQuery);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Normalizes the cleaned query (lowercase, accent removal, Unicode normalization).
            string normalizedQuery;
            
            try
            {
                normalizedQuery = NormalizeQuery(cleanedQuery);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Splits the normalized query into individual tokens.
            List<string> lstTokens;
            
            try
            {
                lstTokens = TokenizeQuery(normalizedQuery);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Expands tokens using Levenshtein distance to tolerate spelling mistakes.
            List<string> lstExpandedTokens;
            
            try
            {
                lstExpandedTokens = ExpandTokensLevenshtein(lstTokens);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Attempts to extract natural date expressions from the expanded tokens.
            DateTime? detectedStartDate;
            DateTime? detectedEndDate;
            
            try
            {
                (detectedStartDate, detectedEndDate) =
                    ParseNaturalDates(lstExpandedTokens, rawQuery, DateTime.Now);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Detects semantic priority categories such as "important", "urgent", or "anniversary".
            string priorityCategory;
            
            try
            {
                priorityCategory = ParsePriorityFilters(lstTokens);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Detects month filters (e.g., "in March", "en mars", "en marzo").
            DateTime? detectedMonth;
            
            try
            {
                detectedMonth = DetectMonth(lstExpandedTokens);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Builds the Sql Where clause based on tokens, date filters, month filters,
            // and priority categories.
            string sqlWhereClause;
            
            try
            {
                sqlWhereClause = BuildSqlWhere(lstExpandedTokens, detectedStartDate,
                    detectedEndDate, detectedMonth, priorityCategory);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Retrieves tasks from the database using the generated SQL WHERE clause.
            List<Tasks> dbResults;
            try
            {
                dbResults = dbConn.SearchTasks(sqlWhereClause);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Scores and sorts the retrieved tasks by relevance.
            List<ScoredTask> scoredResults;
            try
            {
                scoredResults = ScoreCandidates(dbResults, lstExpandedTokens, lstTokens);
            }
            catch
            {
                return NotifyCrashResult();
            }

            // Extracts the underlying task objects from the scored results.
            try
            {
                return scoredResults.Select(scoredResult => scoredResult.Task).ToList();
            }
            catch
            {
                return NotifyCrashResult();
            }
        }

        /// <summary>
        /// Splits the user query into normalized tokens.
        /// Removes punctuation, trims spaces, lowercases everything.
        /// </summary>
        public List<string> TokenizeQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<string>();
            }

            // Normalizes punctuation
            char[] separators = new[] { ' ', ',', ';', '.', ':', '!', '?', '/', '\\', '-', '_', '(', ')', '[', ']', '{', '}' };

            List<string> tokens = new List<string>(
                query.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries));

            return tokens;
        }


        /// <summary>
        /// Tries to match absolute date keywords (today, tomorrow, yesterday) in multiple languages
        /// </summary>
        /// <param name="token"></param>
        /// <param name="now"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        private bool TryAbsoluteKeyword(string token, DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            if (token is "today" || token is "aujourdhui" || token is "hoy")
            {
                startDateTime = endDateTime = now.Date;
                return true;
            }

            if (token is "tomorrow" || token is "demain" || token is "manana")
            {
                startDateTime = endDateTime = now.Date.AddDays(1);
                return true;
            }

            if (token is "yesterday" || token is "hier" || token is "ayer")
            {
                startDateTime = endDateTime = now.Date.AddDays(-1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Detects explicit years such as "2025", "2026".
        /// Returns a full-year range if the token is a valid year.
        /// </summary>
        private bool TryExplicitYear(string currentToken,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            // Accepts years between 1900 and 2400
            int parsedYear;
            
            if (int.TryParse(currentToken, out parsedYear) &&
                parsedYear >= 1900 && parsedYear <= 2400)
            {
                startDateTime = new DateTime(parsedYear, 1, 1);
                endDateTime = new DateTime(parsedYear, 12, 31);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Detects month‑range expressions ("this month", "next month", "last month")
        /// in any supported language. The method is fully language‑agnostic: all
        /// linguistic variations are defined in MultiLanguageDictionaries.MonthRangeKeywords.
        ///
        /// Matching strategy:
        /// - Builds two composite keys:
        ///     • previousToken + currentToken  → matches patterns like "ce mois", "this month"
        ///     • currentToken + nextToken      → matches patterns like "mois suivant", "next month"
        /// - Looks up these keys in MonthRangeKeywords, which maps them to:
        ///     • "this"
        ///     • "next"
        ///     • "last"
        /// </summary>
        private bool TryMonthExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            string currentToken = tokens[tokenIndex];
            string previousToken = tokenIndex > 0 ? tokens[tokenIndex - 1] : string.Empty;
            string nextToken = tokenIndex + 1 < tokens.Count ? tokens[tokenIndex + 1] : string.Empty;

            // Composite keys for dictionary lookup
            string compositePreviousCurrent = MultiLanguageDictionaries.NormalizeKey((previousToken + " " + currentToken).Trim());
            string compositeCurrentNext = MultiLanguageDictionaries.NormalizeKey((currentToken + " " + nextToken).Trim());

            string rangeType;

            // Case 1 — "this month" patterns
            if (MultiLanguageDictionaries.MonthRangeKeywords.TryGetValue(compositePreviousCurrent, out rangeType) &&
                rangeType == "this")
            {
                DateTime firstDay = new DateTime(now.Year, now.Month, 1);
                startDateTime = firstDay;
                endDateTime = firstDay.AddMonths(1).AddDays(-1);
                return true;
            }

            // Case 2 — "next month" / "last month" patterns
            if (MultiLanguageDictionaries.MonthRangeKeywords.TryGetValue(compositeCurrentNext, out rangeType))
            {
                DateTime baseMonth = new DateTime(now.Year, now.Month, 1);

                if (rangeType == "next")
                {
                    startDateTime = baseMonth.AddMonths(1);
                }
                else if (rangeType == "last")
                {
                    startDateTime = baseMonth.AddMonths(-1);
                }
                else
                {
                    return false;
                }

                endDateTime = startDateTime.Value.AddMonths(1).AddDays(-1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles ordinal day expressions combined with day or month:
        /// - "7eme jour", "3rd day", "2do dia"
        /// - "7eme mars", "3rd april", "2do abril"
        /// - "7eme", "3rd", "2do" alone → day of current month
        /// Uses TryParseOrdinalDay() and monthDictionary.
        /// </summary>
        private bool TryOrdinalDate(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;
            
            int dayNumber;
            bool hasOrdinalSuffix;
            
            // Accept ordinal day ("3rd", "7ème", "2do") or number word ("three", "trois", "tres") or digits ("3")
            if (!TryParseOrdinalDay(tokens[tokenIndex], out dayNumber, out hasOrdinalSuffix) &&
            !TryParseNumberWord(tokens[tokenIndex], out dayNumber) &&
            !int.TryParse(tokens[tokenIndex], out dayNumber))
            {
                return false;
            }

            // Case 1 — "7eme jour" / "3rd day" / "2do dia"
            if (tokenIndex + 1 < tokens.Count &&
                MultiLanguageDictionaries.DayWords.Contains(tokens[tokenIndex + 1]))
            {
                DateTime explicitDateChosen = new DateTime(now.Year, now.Month, dayNumber);
                startDateTime = explicitDateChosen;
                endDateTime = explicitDateChosen;
                return true;
            }

            // Case 2 — "7eme mars" / "3rd april" / "2do abril"
            if (tokenIndex + 1 < tokens.Count &&
                MultiLanguageDictionaries.MonthDictionary.ContainsKey(tokens[tokenIndex + 1]))
            {
                int monthNumber = MultiLanguageDictionaries.MonthDictionary[tokens[tokenIndex + 1]];

                // Explicit date chosen from ordinal day and month name
                DateTime explicitDateChosen = new DateTime(now.Year, monthNumber, dayNumber);

                startDateTime = explicitDateChosen;
                endDateTime = explicitDateChosen;
                return true;
            }

            // Case 3 — "7eme" / "3rd" / "2do" alone : interprets as day of current month
            if (hasOrdinalSuffix)
            {
                DateTime explicitDateChosen = new DateTime(now.Year, now.Month, dayNumber);
                startDateTime = explicitDateChosen;
                endDateTime = explicitDateChosen;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to extract a date range from a list of tokens in a language‑agnostic way.
        /// This method is reused for both title and description tokens.
        /// It delegates all language variations to MultiLanguageDictionaries and
        /// existing TryXXX handlers (relative, ordinal, month, weekday…).
        /// </summary>
        public bool TryParseDateTokens(List<string> tokens, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            if (tokens == null || tokens.Count == 0)
            {
                return false;
            }

            for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
            {
                // Numeric explicit dates: 14/03/2026, 2026-04-21, 14/03…
                if (TryParseNumericDateToken(tokens[tokenIndex], now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Month expressions: this month, next month, last month…
                if (TryMonthExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Relative expressions: in 3 days, dans 2 semaines, en 5 meses…
                if (TryRelativeExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Ordinal dates: 3rd april, 7eme mars, 2do abril…
                if (TryOrdinalDate(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Weekday expressions: mardi, mardi prochain, next Tuesday, martes siguiente…
                if (TryWeekdayExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Explicit years: 2026, 2027, l’an prochain, next year…
                if (TryYearExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Absolute keywords: today, tomorrow, yesterday, aujourd’hui, mañana…
                if (TryAbsoluteKeyword(tokens[tokenIndex], now, out startDateTime, out endDateTime))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to convert a number written in natural language (FR/EN/ES)
        /// into its integer value. Supports units, tens, and multipliers
        /// (hundred, thousand) as well as hyphenated or spaced forms.
        /// Returns true if the input is a valid number word.
        /// </summary>
        public static bool TryParseNumberWord(string input, out int result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Normalize: remove hyphens, collapse spaces, lowercase
            string normalized = input
                .Replace("-", " ")
                .Replace("‑", " ")
                .Replace("  ", " ")
                .Trim()
                .ToLowerInvariant();

            string[] tokens = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int currentGroupValue = 0; // Accumulates units/tens before a multiplier
            int finalValue = 0;        // Accumulates the full parsed number

            foreach (string token in tokens)
            {
                int unitValue;
                int tensValue;
                int multiplierValue;

                // Units (one, two, un, dos…)
                if (MultiLanguageDictionaries.Units.TryGetValue(token, out unitValue))
                {
                    currentGroupValue += unitValue;
                    continue;
                }

                // Tens (twenty, trente, treinta…)
                if (MultiLanguageDictionaries.Tens.TryGetValue(token, out tensValue))
                {
                    currentGroupValue += tensValue;
                    continue;
                }

                // Multipliers (hundred, cent, mille, thousand…)
                if (MultiLanguageDictionaries.Multipliers.TryGetValue(token, out multiplierValue))
                {
                    // “cent” alone means 100, not 0 × 100
                    if (currentGroupValue == 0)
                    {
                        currentGroupValue = 1;
                    }

                    currentGroupValue *= multiplierValue;
                    finalValue += currentGroupValue;
                    currentGroupValue = 0;
                    continue;
                }

                // Unknown token → not a number word
                return false;
            }

            finalValue += currentGroupValue;
            result = finalValue;
            return true;
        }

        /// <summary>
        /// Attempts to parse a single token as an explicit numeric date.
        /// Supported formats:
        ///   - dd/MM/yyyy, dd-MM-yyyy
        ///   - yyyy/MM/dd, yyyy-MM-dd
        ///   - dd/MM, dd-MM  (year defaults to the current year)
        ///
        /// This handler is intentionally lightweight and fast by using regex matching
        /// instead of culture-dependent parsing.
        /// It is designed to be called inside the unified date‑token pipeline
        /// (TryParseDateTokens), so it only handles numeric formats.
        /// All textual month/day formats are handled by the other TryXXX handlers.
        /// </summary>
        public bool TryParseNumericDateToken(string token, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            string trimmedToken = token.Trim();

            // Pattern 1: dd/MM/yyyy or dd-MM-yyyy
            Match matchDayMonthYear = Regex.Match(trimmedToken,
                @"^(?<day>\d{1,2})[\/\-](?<month>\d{1,2})[\/\-](?<year>\d{4})$");

            if (matchDayMonthYear.Success)
            {
                int parsedDay = int.Parse(matchDayMonthYear.Groups["day"].Value);
                int parsedMonth = int.Parse(matchDayMonthYear.Groups["month"].Value);
                int parsedYear = int.Parse(matchDayMonthYear.Groups["year"].Value);

                DateTime parsedDate = new DateTime(parsedYear, parsedMonth, parsedDay);
                startDateTime = parsedDate;
                endDateTime = parsedDate;
                return true;
            }

            // Pattern 2: yyyy/MM/dd or yyyy-MM-dd
            Match matchYearMonthDay = Regex.Match(trimmedToken,
                @"^(?<year>\d{4})[\/\-](?<month>\d{1,2})[\/\-](?<day>\d{1,2})$");

            if (matchYearMonthDay.Success)
            {
                int parsedYear = int.Parse(matchYearMonthDay.Groups["year"].Value);
                int parsedMonth = int.Parse(matchYearMonthDay.Groups["month"].Value);
                int parsedDay = int.Parse(matchYearMonthDay.Groups["day"].Value);

                DateTime parsedDate = new DateTime(parsedYear, parsedMonth, parsedDay);
                startDateTime = parsedDate;
                endDateTime = parsedDate;
                return true;
            }

            // Pattern 3: dd/MM or dd-MM : year defaults to now.Year
            Match matchDayMonth = Regex.Match(trimmedToken,
                @"^(?<day>\d{1,2})[\/\-](?<month>\d{1,2})$");

            if (matchDayMonth.Success)
            {
                int parsedDay = int.Parse(matchDayMonth.Groups["day"].Value);
                int parsedMonth = int.Parse(matchDayMonth.Groups["month"].Value);
                int parsedYear = now.Year;

                DateTime parsedDate = new DateTime(parsedYear, parsedMonth, parsedDay);
                startDateTime = parsedDate;
                endDateTime = parsedDate;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Attempts to extract a day number from an ordinal token in a fully language‑agnostic way.
        /// The method does not assume any alphabet or language: ordinal suffixes may come from
        /// any Unicode script (e.g., "st", "ème", "º", "日").
        ///
        /// Parsing strategy:
        /// - If the token is purely numeric ("1", "2", "15"), it is accepted directly.
        /// - Otherwise, the method checks whether the token ends with any known ordinal suffix
        ///   defined in MultiLanguageDictionaries.OrdinalSuffixes.
        /// - If a suffix matches, the suffix is removed and the remaining part is parsed as a number.
        ///
        /// Extensibility:
        /// To support a new language, simply add its ordinal suffixes to
        /// MultiLanguageDictionaries.OrdinalSuffixes.
        /// </summary>
        private bool TryParseOrdinalDay(string token, out int dayNumber, out bool hasOrdinalSuffix)
        {
            dayNumber = 0;
            hasOrdinalSuffix = false;

            // Case A — Pure numeric day ("1", "2", "15")
            if (int.TryParse(token, out int numericDay))
            {
                dayNumber = numericDay;
                return true;
            }

            // Iterates through all known ordinal suffixes and checks whether
            // the token ends with one of them.
            // If a suffix matches, the suffix is removed and the remaining part
            // is parsed as the numeric day.
            // This works for any alphabet, since EndsWith() and Substring()
            // are Unicode‑safe.
            foreach (string suffix in MultiLanguageDictionaries.OrdinalSuffixes)
            {
                if (token.EndsWith(suffix, StringComparison.Ordinal))
                {
                    string numericPart = token.Substring(0, token.Length - suffix.Length);

                    if (int.TryParse(numericPart, out int parsedDay))
                    {
                        dayNumber = parsedDay;
                        hasOrdinalSuffix = true;
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Handles relative date expressions such as:
        /// - "in 3 days", "dans 2 semaines", "en 5 meses"
        /// - "3 days before", "2 weeks after", "5 meses antes"
        /// Uses global dictionaries (RelativeStartKeywords, RelativeUnits, RelativeDirections)
        /// so new languages can be added easily.
        /// </summary>
        private bool TryRelativeExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            // ---------------------------------------------------------------------
            // Pattern 1: "in X unit"
            // Examples:
            // EN: "in 3 days"
            // FR: "dans 2 semaines", "en 5 mois"
            // ES: "en 4 dias"
            // ---------------------------------------------------------------------
            if (MultiLanguageDictionaries.RelativeStartKeywords.Contains(tokens[tokenIndex]) &&
                tokenIndex + 2 < tokens.Count)
            {
                string quantityToken = tokens[tokenIndex + 1];
                string timeUnitToken = tokens[tokenIndex + 2];

                // Quantity must be numeric
                if (!TryParseNumberWord(quantityToken, out int quantity) && !int.TryParse(quantityToken, out quantity))
                {
                    return false;
                }

                // Time unit must exist in the dictionary
                if (!MultiLanguageDictionaries.RelativeUnits.TryGetValue(timeUnitToken, out string timeUnitTokenParsed))
                {
                    return false;
                }

                return ApplyRelativeUnitGeneric(quantity, timeUnitTokenParsed, now,
                    out startDateTime, out endDateTime);
            }

            // ---------------------------------------------------------------------
            // Pattern 2: "X unit before/after"
            // Examples:
            // EN: "3 days before", "2 weeks after"
            // FR: "3 jours avant", "2 semaines après"
            // ES: "5 dias antes", "4 semanas después"
            // ---------------------------------------------------------------------
            if ((TryParseNumberWord(tokens[tokenIndex], out int relativeQuantity) ||
            int.TryParse(tokens[tokenIndex], out relativeQuantity)) && tokenIndex + 2 < tokens.Count)
            {
                string timeUnitToken = tokens[tokenIndex + 1];
                string directionToken = tokens[tokenIndex + 2];

                // Time unit must exist in the dictionary
                if (!MultiLanguageDictionaries.RelativeUnits.TryGetValue(timeUnitToken, out string timeUnitTokenParsed))
                {
                    return false;
                }

                // Direction must exist in the dictionary
                if (!MultiLanguageDictionaries.RelativeDirections.TryGetValue(directionToken, out int directionSign))
                {
                    return false;
                }

                return ApplyRelativeUnitGeneric(relativeQuantity * directionSign, timeUnitTokenParsed, now,
                    out startDateTime, out endDateTime);
            }

            return false;
        }

        /// <summary>
        /// Tries to match year expressions (this year, nextToken year, last year) in multiple languages.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="now"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        private bool TryYearExpression(List<string> tokens, int tokenIndex, DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;
            string currentToken = tokens[tokenIndex];
            string previousToken = tokenIndex > 0 ? tokens[tokenIndex - 1] : "";
            string nextToken = tokenIndex + 1 < tokens.Count ? tokens[tokenIndex + 1] : "";

            // this year
            if ((currentToken == "année" && previousToken == "cette") ||
                (currentToken == "year" && previousToken == "this") ||
                (currentToken == "año" && previousToken == "este"))
            {
                startDateTime = new DateTime(now.Year, 1, 1);
                endDateTime = new DateTime(now.Year, 12, 31);
                return true;
            }

            // next year
            if (((currentToken == "année" || currentToken == "an") &&
                 (nextToken == "prochaine" || nextToken == "prochain")) ||
                (currentToken == "next" && nextToken == "year") ||
                (currentToken == "año" && previousToken == "próximo"))
            {
                startDateTime = new DateTime(now.Year + 1, 1, 1);
                endDateTime = new DateTime(now.Year + 1, 12, 31);
                return true;
            }

            // last year
            if (((currentToken == "année" || currentToken == "an") &&
                 (nextToken == "passée" || nextToken == "passé" || nextToken == "précédent")) ||
                (currentToken == "last" && nextToken == "year") ||
                (currentToken == "año" && nextToken == "pasado"))
            {
                startDateTime = new DateTime(now.Year - 1, 1, 1);
                endDateTime = new DateTime(now.Year - 1, 12, 31);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles weekday expressions with direction:
        /// - "lundi prochain", "next monday", "lunes pasado"
        /// Uses weekdayDictionary + NextWeekdayKeywords + PreviousWeekdayKeywords.
        /// </summary>
        private bool TryWeekdayExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            string currentToken = tokens[tokenIndex];

            // Must be a known weekday
            if (!MultiLanguageDictionaries.WeekdayDictionary.ContainsKey(currentToken))
            {
                return false;
            }

            DayOfWeek targetDayOfWeek = MultiLanguageDictionaries.WeekdayDictionary[currentToken];

            string previousToken = tokenIndex > 0 ? tokens[tokenIndex - 1] : string.Empty;
            string nextToken = tokenIndex + 1 < tokens.Count ? tokens[tokenIndex + 1] : string.Empty;

            int weekdayDirectionSign = 0;

            // Next (prochain / next / siguiente / posterior…)
            if (MultiLanguageDictionaries.NextWeekdayKeywords.Contains(previousToken) ||
                MultiLanguageDictionaries.NextWeekdayKeywords.Contains(nextToken))
            {
                weekdayDirectionSign = +1;
            }

            // Previous (dernier / last / pasado / anterior…)
            if (MultiLanguageDictionaries.PreviousWeekdayKeywords.Contains(previousToken) ||
                MultiLanguageDictionaries.PreviousWeekdayKeywords.Contains(nextToken))
            {
                weekdayDirectionSign = -1;
            }

            if (weekdayDirectionSign == 0)
            {
                return false;
            }

            // Computes the target weekday in the given direction
            DateTime weekdayDateChosen = GetRelativeWeekday(now, targetDayOfWeek, weekdayDirectionSign);

            startDateTime = weekdayDateChosen.Date;
            endDateTime = weekdayDateChosen.Date;
            return true;
        }
    }
}

