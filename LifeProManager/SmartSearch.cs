/// <file>SmartSearch.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 16th, 2026</date>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeProManager
{
    /// <summary>
    /// Multilingual natural‑language search engine.
    /// All language‑specific data is stored in LangDict.cs, which is the only file
    /// that needs to be edited to add support for a new language.
    /// SmartSearch itself is fully language‑agnostic and relies entirely on the
    /// dictionaries for linguistic variations.
    /// </summary>
    public class SmartSearch
    {
        // Reuses the global DB connection created in Program.cs
        public DBConnection dbConn => Program.DbConn;

        public SmartSearch()
        {

        }

        /// <summary>
        /// Applies a relative offset based on unit and direction.
        /// </summary>
        private static DateTime ApplyRelativeOffset(DateTime origin, int quantity, string unitKey,
            int direction)
        {
            int signedValue = direction >= 0 ? +1 : -1;

            switch (unitKey)
            {
                case "day":
                {
                    return origin.AddDays(signedValue * quantity);
                }

                case "week":
                {
                    return origin.AddDays(signedValue * 7 * quantity);
                }

                case "month":
                {
                    return origin.AddMonths(signedValue * quantity);
                }

                case "year":
                {
                    return origin.AddYears(signedValue * quantity);
                }

                default:
                {
                    return origin;
                }
            }
        }

        /// <summary>
        /// Applies a relative quantity to a standard time unit ("day", "week", "month", "year").
        /// The quantity can be positive (after) or negative (before).
        /// </summary>
        /// <returns>
        /// True if the relative offset could be applied successfully.  
        /// The resulting date (or interval) is returned through the out parameters
        /// </returns>
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

                if (LangDict.MonthNumberDict.ContainsKey(normalizedToken))
                {
                    // Stores the month number corresponding to the detected month name
                    int monthNumber = LangDict.MonthNumberDict[normalizedToken];

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
                if (LangDict.PriorityKeywordDict.TryGetValue(token, out string priorityDefined))
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
                List<String> allWords = taskTitle.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
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
                    ParseNaturalDates(lstExpandedTokens, rawQuery, DateTime.Today);
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
                scoredResults = ScoreCandidates(dbResults, lstTokens, lstExpandedTokens);
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
        /// Detects a relative direction (before/after).
        /// Returns -1 for "before"/"previous", +1 for "after"/"next".
        /// </summary>
        private static bool TryDetectDirection(string[] tokens, int startIndex, out int direction)
        {
            direction = 0;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            for (int i = Math.Max(0, startIndex); i < tokens.Length; i++)
            {
                string normalizedKey = LangDict.NormalizeKey(tokens[i]);

                if (LangDict.TimeDirectionDict.TryGetValue(normalizedKey, out int computedDirection))
                {
                    direction = computedDirection;
                    return true;
                }

                if (normalizedKey == "precedent" || normalizedKey == "precedente")
                {
                    direction = -1;
                    return true;
                }

                if (normalizedKey == "suivant" || normalizedKey == "suivante")
                {
                    direction = +1;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Detects the first relative preposition in the token array.
        /// </summary>
        /// <returns>True if a relative preposition has been found and outputs its index.</returns>
        private static bool TryDetectRelativePreposition(string[] tokens,
            out int prepositionIndex)
        {
            prepositionIndex = -1;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                string normalizedKey = LangDict.NormalizeKey(tokens[i]);

                if (LangDict.RelativePrepositionSet.Contains(normalizedKey))
                {
                    prepositionIndex = i;
                    return true;
                }
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
        /// Matches month‑range expressions such as:
        /// "this month", "next month", "last month"
        /// in any supported language.
        /// </summary>
        private bool TryMonthExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            // Normalizes token (lowercase, remove accents, Unicode‑safe)
            string token = LangDict.NormalizeKey(tokens[tokenIndex]);

            // Checks dictionary
            if (!LangDict.MonthRangeDict.TryGetValue(token, out string rangeType))
            {
                return false;
            }

            int offset;
            
            if (rangeType == "this")
            {
                offset = 0;
            }
            else if (rangeType == "next")
            {
                offset = +1;
            }
            else if (rangeType == "last")
            {
                offset = -1;
            }
            else
            {
                offset = 0;
            }

            int year = now.Year;
            int month = now.Month + offset;

            // Normalize year/month overflow
            while (month < 1) { month += 12; year--; }
            while (month > 12) { month -= 12; year++; }

            startDateTime = new DateTime(year, month, 1);
            endDateTime = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            return true;
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
                LangDict.DayWordSet.Contains(tokens[tokenIndex + 1]))
            {
                DateTime explicitDateChosen = new DateTime(now.Year, now.Month, dayNumber);
                startDateTime = explicitDateChosen;
                endDateTime = explicitDateChosen;
                return true;
            }

            // Case 2 — "7eme mars" / "3rd april" / "2do abril"
            if (tokenIndex + 1 < tokens.Count &&
                LangDict.MonthNumberDict.ContainsKey(tokens[tokenIndex + 1]))
            {
                int monthNumber = LangDict.MonthNumberDict[tokens[tokenIndex + 1]];

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
        /// Attempts to parse an absolute calendar date from the token array.
        /// Supports formats such as:
        /// - "15 mars", "15 March"
        /// - "20/04", "20-04", "20.04"
        /// - "2025-03-15", "15-03-2025"
        /// </summary>
        /// <returns>
        /// True if a valid absolute date is found.
        /// The resulting date is returned through anchorDateTime.
        /// The number of consumed tokens is returned through consumedTokens.
        /// </returns>
        private static bool TryParseAbsoluteDate(string[] tokens, DateTime now, out DateTime anchorDateTime, out int consumedTokens)
        {
            anchorDateTime = default;
            consumedTokens = 0;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            // Tries single-token formats: "20/04", "2025-03-15", "15.03.2025"
            string token0 = tokens[0];

            if (DateTime.TryParse(token0, out DateTime parsedSingle))
            {
                anchorDateTime = parsedSingle.Date;
                consumedTokens = 1;
                return true;
            }

            // Tries two-token formats: "15 mars", "15 March"
            if (tokens.Length >= 2)
            {
                string dayToken = tokens[0];
                string monthToken = tokens[1];

                if (int.TryParse(dayToken, out int dayValue))
                {
                    string normalizedMonth = LangDict.NormalizeKey(monthToken);

                    if (LangDict.MonthNumberDict.TryGetValue(normalizedMonth, out int monthValue))
                    {
                        int year = now.Year;

                        if (DateTime.DaysInMonth(year, monthValue) >= dayValue)
                        {
                            anchorDateTime = new DateTime(year, monthValue, dayValue);
                            consumedTokens = 2;
                            return true;
                        }
                    }
                }
            }

            // Try three-token formats: "15 mars 2025", "15 March 2025"
            if (tokens.Length >= 3)
            {
                string dayToken = tokens[0];
                string monthToken = tokens[1];
                string yearToken = tokens[2];

                if (int.TryParse(dayToken, out int dayValue) && int.TryParse(yearToken, out int yearValue))
                {
                    string normalizedMonth = LangDict.NormalizeKey(monthToken);

                    if (LangDict.MonthNumberDict.TryGetValue(normalizedMonth, out int monthValue))
                    {
                        if (DateTime.DaysInMonth(yearValue, monthValue) >= dayValue)
                        {
                            anchorDateTime = new DateTime(yearValue, monthValue, dayValue);
                            consumedTokens = 3;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse an absolute date range from a list of tokens.
        /// Supports all formulations such as: "from 3 to 7", "3 to 7", "3-7",
        /// "period from 3 to 7".
        /// The method delegates date parsing of each bound to TryParseDateTokens.
        /// </summary>
        public bool TryParseAbsoluteRangeTokens(List<string> tokens, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            if (tokens == null || tokens.Count == 0)
            {
                return false;
            }

            // Normalizes tokens
            List<string> normalizedTokens = tokens
                .Select(token => LangDict.NormalizeKey(token))
                .ToList();

            // Ensures normalized range keyword sets are available
            if (LangDict.RangeOptionalPrefixSet == null || LangDict.RangeSeparatorSet == null)
            {
                return false;
            }

            // Detects separators ("au", "à", "to", etc.)
            int separatorIndex = -1;

            for (int i = 0; i < normalizedTokens.Count; i++)
            {
                if (LangDict.RangeSeparatorSet.Contains(normalizedTokens[i]))
                {
                    separatorIndex = i;
                    break;
                }
            }

            if (separatorIndex <= 0 || separatorIndex >= normalizedTokens.Count - 1)
            {
                return false;
            }

            // Left and right segments
            List<string> leftTokens = tokens.Take(separatorIndex).ToList();
            List<string> rightTokens = tokens.Skip(separatorIndex + 1).ToList();

            // Removes optional prefixes ("du", "from", "del", "période", "period", "periodo", etc.)
            leftTokens = leftTokens.Where(leftToken => 
            !LangDict.RangeOptionalPrefixSet.Contains(LangDict.NormalizeKey(leftToken))).ToList();

            rightTokens = rightTokens
                .Where(rightToken => !LangDict.RangeOptionalPrefixSet.Contains(LangDict.NormalizeKey(rightToken)))
                .ToList();

            // Parses left bound
            if (!TryParseDateTokens(leftTokens, now, out DateTime? leftStart, out DateTime? leftEnd))
            {
                return false;
            }

            // Parses right bound
            if (!TryParseDateTokens(rightTokens, now, out DateTime? rightStart, out DateTime? rightEnd))
            {
                return false;
            }

            // Chooses the start of each parsed date
            DateTime parsedStartDateTime = leftStart ?? leftEnd ?? default;
            DateTime parsedEndDateTime = rightStart ?? rightEnd ?? default;

            // Applies implicit month/year if needed
            if (parsedStartDateTime.Month == now.Month && leftTokens.Count == 1)
            {
                parsedStartDateTime = new DateTime(now.Year, now.Month, parsedStartDateTime.Day);
            }

            if (parsedEndDateTime.Month == now.Month && rightTokens.Count == 1)
            {
                parsedEndDateTime = new DateTime(now.Year, now.Month, parsedEndDateTime.Day);
            }

            // Rollover (e.g., 28th February to 3rd March, 30th December to 2nd January)
            if (parsedEndDateTime < parsedStartDateTime)
            {
                if (parsedEndDateTime.Month < parsedStartDateTime.Month)
                {
                    parsedEndDateTime = parsedEndDateTime.AddYears(1);
                }
            }

            startDateTime = parsedStartDateTime;
            endDateTime = parsedEndDateTime;
            return true;
        }

        /// <summary>
        /// Attempts to parse any anchor expression, absolute or relative.
        /// Supported anchors include:
        /// - Absolute dates: "15 mars", "20/04", "2025-03-15"
        /// - Simple relative anchors: "tomorrow", "yesterday", "today"
        /// - Compound relative anchors: "after tomorrow"
        /// - Relative-week anchors: "next Monday", "last Friday"
        /// - Relative-month/year anchors: "next month", "last year"
        /// </summary>
        /// <returns>True if a valid anchor is found.
        /// The resulting date is returned through anchorDateTime.
        /// The number of consumed tokens is returned through consumedTokens.
        /// </returns>
        private static bool TryParseAnyAbsoluteOrRelativeAnchor(string[] tokens, DateTime now,
            out DateTime anchorDateTime, out int consumedTokens)
        {
            anchorDateTime = default;
            consumedTokens = 0;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            // Tries absolute date first
            if (TryParseAbsoluteDate(tokens, now, out DateTime absDate, out int absConsumed))
            {
                anchorDateTime = absDate;
                consumedTokens = absConsumed;
                return true;
            }

            // Tries simple relative anchors: "tomorrow", "yesterday", "today"
            string normalizedKeyIndex0 = LangDict.NormalizeKey(tokens[0]);

            if (LangDict.RelativeDayOffsetDict.TryGetValue(normalizedKeyIndex0, out int dayOffset))
            {
                anchorDateTime = now.Date.AddDays(dayOffset);
                consumedTokens = 1;
                return true;
            }

            // Tries compound relative anchors: "after tomorrow"
            if (tokens.Length >= 2)
            {
                string normalizedKeyIndex1 = LangDict.NormalizeKey(tokens[1]);

                if (normalizedKeyIndex0 == "after" && normalizedKeyIndex1 == "tomorrow")
                {
                    anchorDateTime = now.Date.AddDays(2);
                    consumedTokens = 2;
                    return true;
                }
            }

            // Tries relative-week anchors: "next Monday", "last Friday"
            if (tokens.Length >= 2)
            {
                string directionToken = LangDict.NormalizeKey(tokens[0]);
                string weekdayToken = LangDict.NormalizeKey(tokens[1]);

                if (LangDict.RelativeDirectionDict.TryGetValue(directionToken, out int directionSigned))
                {
                    if (LangDict.WeekdayNameToDayOfWeekDict.TryGetValue(weekdayToken, out DayOfWeek targetDay))
                    {
                        DateTime baseDate = now.Date;

                        // Computes distance to the target weekday
                        int delta = ((int)targetDay - (int)baseDate.DayOfWeek + 7) % 7;

                        if (directionSigned > 0)
                        {
                            delta += 7;
                        }
                        else if (directionSigned < 0)
                        {
                            delta -= 7;
                        }

                        anchorDateTime = baseDate.AddDays(delta);
                        consumedTokens = 2;
                        return true;
                    }
                }
            }

            // Tries relative-month/year anchors: "next month", "last year"
            if (tokens.Length >= 2)
            {
                string directionToken = LangDict.NormalizeKey(tokens[0]);
                string unitToken = LangDict.NormalizeKey(tokens[1]);

                if (LangDict.RelativeDirectionDict.TryGetValue(directionToken, out int directionSigned))
                {
                    if (unitToken == "month")
                    {
                        anchorDateTime = new DateTime(now.Year, now.Month, 1).AddMonths(directionSigned);
                        consumedTokens = 2;
                        return true;
                    }

                    if (unitToken == "year")
                    {
                        anchorDateTime = new DateTime(now.Year + directionSigned, 1, 1);
                        consumedTokens = 2;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a directional "between X and Y" date range in a language‑agnostic way.
        /// Supported patterns:
        /// - FR: "entre X et Y"
        /// - EN: "between X and Y"
        /// - ES: "entre X y Y"
        /// The method relies on user-editable keyword lists (lstBetweenKeywords, lstAndKeywords).
        /// Both X and Y are parsed using existing date handlers.
        /// Returns true only if both sides produce valid dates.
        /// </summary>
        public bool TryParseBetweenExpression(string input, DateTime now, out DateTime? startDate,
            out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            string normalizedInput = input.Trim().ToLowerInvariant();

            foreach (var betweenEntry in LangDict.lstBetweenKeywords)
            {
                string betweenKeyword = betweenEntry.value;
                string prefix = betweenKeyword + " ";

                if (!normalizedInput.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Find matching "and" keyword for the same language
                string betweenEntryKey = betweenEntry.key;
                string andKeyword = null;

                foreach (var andEntry in LangDict.lstAndKeywords)
                {
                    if (andEntry.key == betweenEntryKey)
                    {
                        andKeyword = andEntry.value;
                        break;
                    }
                }

                if (andKeyword == null)
                {
                    continue;
                }

                string separator = " " + andKeyword + " ";
                int separatorIndex = normalizedInput.IndexOf(separator, StringComparison.OrdinalIgnoreCase);

                if (separatorIndex <= prefix.Length)
                {
                    continue;
                }

                string leftSegment = normalizedInput.Substring(prefix.Length, separatorIndex - prefix.Length).Trim();
                string rightSegment = normalizedInput.Substring(separatorIndex + separator.Length).Trim();

                if (TryParseDateSegment(leftSegment, now, out DateTime? leftStart, out DateTime? leftEnd) &&
                    TryParseDateSegment(rightSegment, now, out DateTime? rightStart, out DateTime? rightEnd))
                {
                    startDate = leftStart ?? leftEnd;
                    endDate = rightStart ?? rightEnd;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tokenizes a raw text segment and delegates date parsing to TryParseDateTokens.
        /// </summary>
        private bool TryParseDateSegment(string dateSegment, DateTime now,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            if (string.IsNullOrWhiteSpace(dateSegment))
            {
                return false;
            }

            List<string> segmentTokens = TokenizeQuery(dateSegment);

            if (segmentTokens == null || segmentTokens.Count == 0)
            {
                return false;
            }

            return TryParseDateTokens(segmentTokens, now, out startDate, out endDate);
        }

        /// <summary>
        /// Tries to extract a date or date range from a list of tokens.
        /// This method is language‑agnostic and delegates all parsing
        /// to the specialized TryXXX handlers.
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
                // Advanced relative composite expressions:
                // "in 2 months and 3 days", "dans 2 mois et 3 jours"
                if (TryRelativeCompositeExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Ago expressions:
                // "5 days ago", "il y a 5 jours", "hace 3 semanas"
                if (TryRelativeAgoExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Directional composite expressions:
                // "2 weeks and 3 days before", "after 2 weeks and 3 days"
                if (TryRelativeDirectionalCompositeExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Directional simple expressions:
                // "3 days before", "2 weeks after"
                if (TryRelativeDirectionalExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Explicit numeric dates like 14/03/2026, 2026‑04‑21, 14/03
                if (TryParseNumericDateToken(tokens[tokenIndex], now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Month expressions like this month, next month, last month
                if (TryMonthExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Relative expressions like in 3 days, in 2 months
                if (TryRelativeExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Between expressions like between X and Y
                if (TryParseBetweenExpression(tokens[tokenIndex], now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Absolute ranges like from 3 to 7
                if (TryParseAbsoluteRangeTokens(tokens, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Ordinal dates like 3rd April
                if (TryOrdinalDate(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Weekday expressions like next Tuesday
                if (TryWeekdayExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Explicit years like 2026, next year
                if (TryYearExpression(tokens, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Absolute keywords like today, tomorrow, yesterday
                if (TryAbsoluteKeyword(tokens[tokenIndex], now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles direction-only expressions:
        /// - "before" => previous days of the current week
        /// - "after" => remaining days of the current month (starting tomorrow)
        /// </summary>
        private static bool TryParseDirectionOnly(string[] tokens, DateTime now, out DateTime startDateTime,
            out DateTime endDateTime)
        {
            startDateTime = default;
            endDateTime = default;

            if (tokens.Length != 1)
            {
                return false;
            }

            if (!TryDetectDirection(tokens, 0, out int direction))
            {
                return false;
            }

            if (direction < 0)
            {
                // Computes how many days separate today from Monday of the current week.
                // Examples:
                // - Monday    => delta = 0
                // - Tuesday   => delta = 1
                // - Wednesday => delta = 2
                // - Sunday    => delta = 6
                int daysSinceMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

                // Moves backward to reach Monday of the current week.
                DateTime mondayOfCurrentWeek = now.Date.AddDays(-daysSinceMonday);

                // "Before" means: all previous days of the current week, ending yesterday.
                DateTime yesterday = now.Date.AddDays(-1);

                // If today is Monday, yesterday is Sunday (previous week),
                // which would produce an inverted interval.
                // In that case, the interval is collapsed to a single day (Monday).
                if (yesterday < mondayOfCurrentWeek)
                {
                    yesterday = mondayOfCurrentWeek;
                }

                startDateTime = mondayOfCurrentWeek;
                endDateTime = yesterday;
                return true;
            }

            else
            {
                startDateTime = now.Date.AddDays(1);
                endDateTime = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

                if (startDateTime > endDateTime)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Attempts to convert a number written in natural language
        /// into its integer value. Supports units, tens, and multipliers
        /// (hundred, thousand) as well as hyphenated or spaced forms.
        /// </summary> 
        /// <param name="inputWord">The word to process</param>
        /// <param name="result">The parsed number</param>
        /// <returns>Returns true if the input is a valid number word.</returns>
        /// </summary>
        public static bool TryParseNumberWord(string inputWord, out int result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(inputWord))
            {
                return false;
            }

            // Normalizes: remove hyphens, collapse spaces and lowercase
            string normalizedWord = inputWord.Replace("-", " ").Replace("‑", " ").Replace("  ", " ").Trim().ToLowerInvariant();
            
            // Normalizes French composite forms (70–99)
            normalizedWord = normalizedWord
                // 80 forms
                .Replace("quatre vingt", "quatre-vingt")
                .Replace("quatre‑vingt", "quatre-vingt")
                .Replace("quatrevingt", "quatre-vingt")
                // 70 forms
                .Replace("soixante dix", "soixante-dix")
                .Replace("soixante‑dix", "soixante-dix")
                .Replace("soixantedix", "soixante-dix");

            // Tokenizes after normalization
            string[] tokens = normalizedWord.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int currentGroupValue = 0; // Accumulates units/tens before a multiplier
            int finalValue = 0;        // Accumulates the full parsed number

            foreach (string token in tokens)
            {
                int unitValue;
                int tensValue;
                int multiplierValue;

                // Units (one, two, un, dos…)
                if (LangDict.NumberUnitDict.TryGetValue(token, out unitValue))
                {
                    currentGroupValue += unitValue;
                    continue;
                }

                // Tens (twenty, trente, treinta…)
                if (LangDict.NumberTenDict.TryGetValue(token, out tensValue))
                {
                    // French composite: 60 + (10–19) becomes 70–79
                    if (currentGroupValue == 60 && tensValue >= 10 && tensValue <= 19)
                    {
                        currentGroupValue = 60 + tensValue;
                        continue;
                    }

                    // French composite: 80 + (10–19) becomes 90–99
                    if (currentGroupValue == 80 && tensValue >= 10 && tensValue <= 19)
                    {
                        currentGroupValue = 80 + tensValue;
                        continue;
                    }

                    currentGroupValue += tensValue;
                    continue;
                }

                // Multipliers (hundred, cent, mille, thousand…)
                if (LangDict.NumberMultiplierDict.TryGetValue(token, out multiplierValue))
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

                // Unknown token: not a number word
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
        /// This handler is lightweight and fast by using regex matching
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

            // Pattern: dd/MM/yyyy or dd-MM-yyyy
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

            // Pattern: yyyy/MM/dd or yyyy-MM-dd
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

            // Pattern: dd/MM or dd-MM : year defaults to now.Year
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
        /// Parses composite relative expressions such as:
        /// "in 2 weeks and 3 days".
        /// The result is a single point in time: now + (sum of all relative offsets).
        /// </summary>
        /// <returns>True if at least one valid pair is found.</returns>
        private static bool TryParseCompositeRelativeExpression(string[] tokens, DateTime now, out DateTime resultDateTime)
        {
            resultDateTime = default;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            int index = 0;
            int totalDaysOffset = 0;
            int totalMonthsOffset = 0;
            int totalYearsOffset = 0;

            while (index < tokens.Length)
            {
                // Skips connectors such as "and", "et", "y"
                string normalizedKey = LangDict.NormalizeKey(tokens[index]);
                if (LangDict.AndKeywordSet.Contains(normalizedKey))
                {
                    index++;
                    continue;
                }

                // Tries to parse a quantity and unit pair
                if (TryParseQuantityAndUnit(tokens, index - 1, out int quantity, out string unitKey))
                {
                    switch (unitKey)
                    {
                        case "day":
                            {
                                totalDaysOffset += quantity;
                                break;
                            }

                        case "week":
                            {
                                totalDaysOffset += quantity * 7;
                                break;
                            }

                        case "month":
                            {
                                totalMonthsOffset += quantity;
                                break;
                            }

                        case "year":
                            {
                                totalYearsOffset += quantity;
                                break;
                            }
                    }

                    // Moves index forward by the number of tokens consumed
                    // QuantityAndUnit always consumes 2 tokens (quantity + unit)
                    index += 2;
                    continue;
                }

                index++;
            }

            // If nothing was parsed, fails
            if (totalDaysOffset == 0 && totalMonthsOffset == 0 && totalYearsOffset == 0)
            {
                return false;
            }

            // Applies offsets in calendar order: years, months, days
            DateTime computedDateTime = now;

            if (totalYearsOffset != 0)
            {
                computedDateTime = computedDateTime.AddYears(totalYearsOffset);
            }

            if (totalMonthsOffset != 0)
            {
                computedDateTime = computedDateTime.AddMonths(totalMonthsOffset);
            }

            if (totalDaysOffset != 0)
            {
                computedDateTime = computedDateTime.AddDays(totalDaysOffset);
            }

            resultDateTime = computedDateTime;
            return true;
        }

        /// <summary>
        /// Attempts to extract a day number from an ordinal token in a fully language‑agnostic way.
        /// The method does not assume any alphabet or language: ordinal suffixes may come from
        /// any Unicode script (e.g., "st", "ème", "º", "日").
        ///
        /// Parsing strategy:
        /// - If the token is purely numeric ("1", "2", "15"), it is accepted directly.
        /// - Otherwise, the method checks whether the token ends with any known ordinal suffix
        /// - If a suffix matches, the suffix is removed and the remaining part is parsed as a number.
        /// </summary>
        private bool TryParseOrdinalDay(string token, out int dayNumber, out bool hasOrdinalSuffix)
        {
            dayNumber = 0;
            hasOrdinalSuffix = false;

            // Pure numeric day ("1", "2", "15")
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
            foreach (string suffix in LangDict.OrdinalSuffixSet)
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
        /// Parses mixed expressions combining a relative offset with an absolute-relative anchor:
        /// "2 days after tomorrow", "3 weeks before next month", "4 dias despues del lunes pasado".
        /// The result is a single point in time: anchorDate + relativeOffset.
        /// </summary>
        /// <returns>True if both the anchor and the relative offset are valid.</returns>
        private static bool TryParseRelativeAfterRelativeAbsolute(string[] tokens, DateTime now,
            out DateTime resultDateTime)
        {
            resultDateTime = default;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            // Detects direction (before/after)
            if (!TryDetectDirection(tokens, 0, out int directionSigned))
            {
                return false;
            }

            // Parses the relative quantity + unit
            if (!TryParseQuantityAndUnit(tokens, -1, out int quantity, out string unitKey))
            {
                return false;
            }

            // Parses the anchor (which itself may be relative: "tomorrow", "next Monday", "next month")
            if (!TryParseAnyAbsoluteOrRelativeAnchor(tokens, now, out DateTime anchorDateTime, out int consumedTokens))
            {
                return false;
            }

            // Applies offset
            DateTime computedOffsetDateTime = anchorDateTime;

            switch (unitKey)
            {
                case "day":
                    {
                        computedOffsetDateTime = anchorDateTime.AddDays(directionSigned * quantity);
                        break;
                    }

                case "week":
                    {
                        computedOffsetDateTime = anchorDateTime.AddDays(directionSigned * quantity * 7);
                        break;
                    }

                case "month":
                    {
                        computedOffsetDateTime = anchorDateTime.AddMonths(directionSigned * quantity);
                        break;
                    }

                case "year":
                    {
                        computedOffsetDateTime = anchorDateTime.AddYears(directionSigned * quantity);
                        break;
                    }
            }

            resultDateTime = computedOffsetDateTime;
            return true;
        }


        /// <summary>
        /// Parses expressions of the form:
        /// "3 days before March 15", "2 weeks after next Monday", "4 dias antes del 20 de abril".
        /// The result is a single point in time: anchorDate + relativeOffset.
        /// </summary>
        /// <returns>True if both the anchor date and the relative offset are valid.</returns>
        private static bool TryParseRelativeBeforeAfterAbsolute(string[] tokens, DateTime now,
            out DateTime resultDateTime)
        {
            resultDateTime = default;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            // Detects direction (before/after)
            if (!TryDetectDirection(tokens, 0, out int directionSigned))
            {
                return false;
            }

            // Finds the first quantity and unit pair
            if (!TryParseQuantityAndUnit(tokens, -1, out int quantity, out string unitKey))
            {
                return false;
            }

            // Finds the absolute anchor date in the tokens
            if (!TryParseAbsoluteDate(tokens, now, out DateTime anchorDate, out int consumedTokens))
            {
                return false;
            }

            // Computes the offset
            DateTime computedOffsetDateTime = anchorDate;

            switch (unitKey)
            {
                case "day":
                    {
                        computedOffsetDateTime = anchorDate.AddDays(directionSigned * quantity);
                        break;
                    }

                case "week":
                    {
                        computedOffsetDateTime = anchorDate.AddDays(directionSigned * quantity * 7);
                        break;
                    }

                case "month":
                    {
                        computedOffsetDateTime = anchorDate.AddMonths(directionSigned * quantity);
                        break;
                    }

                case "year":
                    {
                        computedOffsetDateTime = anchorDate.AddYears(directionSigned * quantity);
                        break;
                    }
            }

            resultDateTime = computedOffsetDateTime;
            return true;
        }

        /// <summary>
        /// Parses a quantity and unit sequence starting after a given index.
        /// Example: "in 3 days".
        /// </summary>
        /// <returns>True if both quantity and unit are found.</returns>
        private static bool TryParseQuantityAndUnit(string[] tokens, int startIndex, out int parsedQuantity,
            out string unitKey)
        {
            parsedQuantity = 0;
            unitKey = null;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            int index = Math.Max(0, startIndex + 1);

            // Numeric quantity
            if (index < tokens.Length && int.TryParse(tokens[index], out int numericValue))
            {
                parsedQuantity = numericValue;
                index++;
            }
            else
            {
                // Spelled quantity
                if (!TryParseSpelledNumber(tokens, index, out parsedQuantity, out int consumed))
                {
                    return false;
                }

                index += consumed;
            }

            if (parsedQuantity <= 0)
            {
                return false;
            }

            // Unit
            if (index >= tokens.Length)
            {
                return false;
            }

            string normalizedUnit = LangDict.NormalizeKey(tokens[index]);

            if (!LangDict.TimeUnitDict.TryGetValue(normalizedUnit, out string resolvedUnit))
            {
                return false;
            }

            unitKey = resolvedUnit;
            return true;
        }

        /// <summary>
        /// Handles quantity-only expressions:
        /// - numeric: "2" => the 2nd day of the current month
        /// - spelled: "deux" => in two days
        /// </summary>
        private static bool TryParseQuantityOnly(string[] tokens, DateTime now, out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime = default;
            endDateTime = default;

            if (tokens.Length != 1)
            {
                return false;
            }

            string normalizedKey = LangDict.NormalizeKey(tokens[0]);

            // Numeric => day of current month
            if (int.TryParse(normalizedKey, out int numericValue))
            {
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

                if (numericValue >= 1 && numericValue <= daysInMonth)
                {
                    DateTime computedDay = new DateTime(now.Year, now.Month, numericValue);
                    startDateTime = computedDay;
                    endDateTime = computedDay;
                    return true;
                }

                return false;
            }

            // Spelled => relative days

            // Calls TryParseSpelledNumber() with two inline out variables (value and consumedTokens).
            // The method returns true only if a spelled number is detected starting at index 0,
            // and the second condition ensures that exactly one token was consumed (e.g. "two", not "twenty five").
            if (TryParseSpelledNumber(tokens, 0, out int value, out int consumedTokens) && consumedTokens == 1)
            {
                startDateTime = now;
                endDateTime = now.AddDays(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses simple relative expressions:
        /// Examples: "in 3 days", "2 weeks before"
        /// Also delegates to edge-case handlers 
        /// quantity only, unit only and direction only.
        /// </summary>
        private static bool TryParseSimpleRelativeExpression(string[] tokens, DateTime now,
            out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime = default;
            endDateTime = default;

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            // Direction only
            if (TryParseDirectionOnly(tokens, now, out startDateTime, out endDateTime))
            {
                return true;
            }

            // Unit only
            if (TryParseUnitOnly(tokens, now, out startDateTime, out endDateTime))
            {
                return true;
            }

            // Quantity only
            if (TryParseQuantityOnly(tokens, now, out startDateTime, out endDateTime))
            {
                return true;
            }

            // Full pattern: preposition + quantity + unit (+ direction)
            if (!TryDetectRelativePreposition(tokens, out int prepIndex))
            {
                prepIndex = -1;
            }

            if (!TryParseQuantityAndUnit(tokens, prepIndex, out int quantity, out string unitKey))
            {
                return false;
            }

            int direction = +1;

            if (TryDetectDirection(tokens, prepIndex, out int detectedDirection))
            {
                direction = detectedDirection;
            }

            startDateTime = now;
            endDateTime = ApplyRelativeOffset(now, quantity, unitKey, direction);

            if (endDateTime < startDateTime)
            {
                DateTime temporaryDateTime = startDateTime;
                startDateTime = endDateTime;
                endDateTime = temporaryDateTime;
            }

            return true;
        }

        /// <summary>
        /// Parses a spelled number using NumberUnitDict, NumberTenDict and NumberMultiplierDict.
        /// Returns true if at least one numeric component is found.
        /// </summary>
        private static bool TryParseSpelledNumber(string[] tokens, int startIndex, out int value,
            out int consumedTokens)
        {
            value = 0;
            consumedTokens = 0;

            if (tokens == null || tokens.Length == 0 || startIndex >= tokens.Length)
            {
                return false;
            }

            int currentValue = 0;
            int index = startIndex;

            while (index < tokens.Length)
            {
                string normalizedKey = LangDict.NormalizeKey(tokens[index]);
                bool keyMatchedValue = false;

                if (LangDict.NumberUnitDict.TryGetValue(normalizedKey, out int unitVal))
                {
                    currentValue += unitVal;
                    keyMatchedValue = true;
                }
                
                else if (LangDict.NumberTenDict.TryGetValue(normalizedKey, out int tenVal))
                {
                    currentValue += tenVal;
                    keyMatchedValue = true;
                }
                
                else if (LangDict.NumberMultiplierDict.TryGetValue(normalizedKey, out int multVal))
                {
                    if (currentValue == 0)
                    {
                        currentValue = 1;
                    }

                    currentValue *= multVal;
                    keyMatchedValue = true;
                }

                if (!keyMatchedValue)
                {
                    break;
                }

                index++;
            }

            if (currentValue <= 0)
            {
                return false;
            }

            value = currentValue;
            consumedTokens = index - startIndex;
            return true;
        }

        /// <summary>
        /// Handles unit-only expressions:
        /// - "days" => next 7 days
        /// - "week" => current week (Mon-Sun)
        /// - "month" => current month
        /// - "year" => current year
        /// </summary>
        private static bool TryParseUnitOnly(string[] tokens, DateTime now, out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime = default;
            endDateTime = default;

            if (tokens.Length != 1)
            {
                return false;
            }

            string normalized = LangDict.NormalizeKey(tokens[0]);

            if (!LangDict.TimeUnitDict.TryGetValue(normalized, out string unitKey))
            {
                return false;
            }

            switch (unitKey)
            {
                case "day":
                {
                    // "day" => return the next 7 days starting from today.
                    // This mirrors the behavior of the weekly panel: a short forward-looking window.
                    startDateTime = now;
                    endDateTime = now.AddDays(7);
                    return true;
                }

                case "week":
                {
                    // "week" => returns the full current week (Monday to Sunday).
                    // Computes how many days have passed since Monday, then rewinds to Monday.
                    int delta = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    startDateTime = now.Date.AddDays(-delta);

                    // End of the interval is Sunday of the same week (Monday + 6 days).
                    endDateTime = startDateTime.AddDays(6);
                    return true;
                }

                case "month":
                {
                    // "month" => return the full current month.
                    // Start at the 1st day of the month.
                    startDateTime = new DateTime(now.Year, now.Month, 1);

                    // End at the last day of the month (next month minus one day).
                    endDateTime = startDateTime.AddMonths(1).AddDays(-1);
                    return true;
                }

                case "year":
                {
                    // "year" => return the full current year.
                    // Start on January 1st.
                    startDateTime = new DateTime(now.Year, 1, 1);

                    // End on December 31st.
                    endDateTime = new DateTime(now.Year, 12, 31);
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Parses composite relative date expressions such as:
        /// "in 2 months and 3 days", "within 1 week and 2 days",
        /// or reversed forms like "2 weeks and 3 days before".
        /// The method supports any language defined in LangDict.
        /// </summary>
        private bool TryRelativeCompositeExpression(List<string> tokens, int tokenIndex,
            DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            // Initializes output interval
            startDateTime = null;
            endDateTime = null;

            // Tracks whether parsing succeeded
            bool parseSuccessful = false;

            // Pattern: "in/within X unit and Y unit"
            // Examples: "in 2 months and 3 days", "within 1 week and 2 days",
            //   "dans 2 mois et 3 jours"
            
            // Ensures enough tokens for this pattern
            if (tokenIndex + 5 < tokens.Count)
            {
                // Normalizes the starting keyword ("in", "within", "dans", etc.)
                string startKeyword = LangDict.NormalizeKey(tokens[tokenIndex]);

                // Checks if the starting keyword is a valid relative start word
                if (LangDict.RelativePrepositionSet.Contains(startKeyword))
                {
                    // Extracts first quantity token
                    string firstQuantityToken = tokens[tokenIndex + 1];
                    
                    // Normalizes first unit token
                    string firstUnitToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);
                    
                    // Normalizes connector token ("and", "et", "y")
                    string connectorToken = LangDict.NormalizeKey(tokens[tokenIndex + 3]);
                    
                    // Extracts second quantity token
                    string secondQuantityToken = tokens[tokenIndex + 4];
                    
                    // Normalizes second unit token
                    string secondUnitToken = LangDict.NormalizeKey(tokens[tokenIndex + 5]);

                    // Validates connector between the two segments
                    bool connectorIsValid = connectorToken == "and" || connectorToken == "et" ||
                        connectorToken == "y";

                    if (connectorIsValid)
                    {
                        // Validates first quantity (word or digit)
                        bool firstQuantityValid = TryParseNumberWord(firstQuantityToken, out int firstQuantity) ||
                            int.TryParse(firstQuantityToken, out firstQuantity);

                        // Validates second quantity (word or digit)
                        bool secondQuantityValid = TryParseNumberWord(secondQuantityToken, out int secondQuantity) ||
                            int.TryParse(secondQuantityToken, out secondQuantity);

                        // Validates first unit ("day", "week", "month", "year")
                        bool firstUnitValid = LangDict.TimeUnitDict.TryGetValue(firstUnitToken, out string firstCanonicalUnit);

                        // Validates second unit
                        bool secondUnitValid = LangDict.TimeUnitDict.TryGetValue(secondUnitToken, out string secondCanonicalUnit);

                        // Ensures all components are valid
                        if (firstQuantityValid && secondQuantityValid && firstUnitValid && secondUnitValid)
                        {
                            // Temporary interval for first segment
                            DateTime? firstSegmentStart;
                            DateTime? firstSegmentEnd;

                            // Applies first relative offset
                            bool firstRelativeOffsetApplied = ApplyRelativeUnitGeneric(firstQuantity,
                                firstCanonicalUnit, now, out firstSegmentStart, out firstSegmentEnd);

                            // Continues only if first offset succeeded
                            if (firstRelativeOffsetApplied && firstSegmentStart.HasValue)
                            {
                                // Intermediate date after first offset
                                DateTime intermediateDate = firstSegmentStart.Value;

                                // Temporary interval for second segment
                                DateTime? secondStart;
                                DateTime? secondEnd;

                                // Applies second relative offset
                                bool secondRelativeOffsetApplied = ApplyRelativeUnitGeneric(secondQuantity, secondCanonicalUnit,
                                    intermediateDate, out secondStart, out secondEnd);

                                // Finalizes result if second offset succeeded
                                if (secondRelativeOffsetApplied && secondStart.HasValue)
                                {
                                    // Composite expression resolves to a single date
                                    startDateTime = secondStart;
                                    endDateTime = secondStart;
                                    parseSuccessful = true;
                                }
                            }
                        }
                    }
                }
            }

            // Pattern: "X unit and Y unit before/after"
            // Examples: "2 weeks and 3 days before", 
            //   "1 month and 2 weeks after", "2 semanas y 3 dias antes"
            // Only evaluate if first pattern failed
            if (!parseSuccessful && tokenIndex + 5 < tokens.Count)
            {
                // Extracts first quantity token
                string firstQuantityToken = tokens[tokenIndex];
                
                // Normalizes first unit token
                string firstUnitToken = LangDict.NormalizeKey(tokens[tokenIndex + 1]);
                
                // Normalizes connector token
                string connectorToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);
                
                // Extracts second quantity token
                string secondQuantityToken = tokens[tokenIndex + 3];
                
                // Normalizes second unit token
                string secondUnitToken = LangDict.NormalizeKey(tokens[tokenIndex + 4]);
                
                // Normalizes direction token ("before", "after", "avant", "despues")
                string directionToken = LangDict.NormalizeKey(tokens[tokenIndex + 5]);

                // Validates connector
                bool connectorIsValid = connectorToken == "and" || connectorToken == "et" ||
                    connectorToken == "y";

                // Validates first quantity
                bool firstQuantityValid = TryParseNumberWord(firstQuantityToken, out int firstQuantity) ||
                    int.TryParse(firstQuantityToken, out firstQuantity);

                // Validates second quantity
                bool secondQuantityValid = TryParseNumberWord(secondQuantityToken, out int secondQuantity) ||
                    int.TryParse(secondQuantityToken, out secondQuantity);

                // Validates first unit
                bool firstUnitValid = LangDict.TimeUnitDict.TryGetValue(firstUnitToken, out string firstCanonicalUnit);

                // Validates second unit
                bool secondUnitValid = LangDict.TimeUnitDict.TryGetValue(secondUnitToken, out string secondCanonicalUnit);

                // Validates direction ("before" = -1, "after" = +1)
                bool directionValid = LangDict.TimeDirectionDict.TryGetValue(directionToken, out int directionSign);

                // Ensures all components are valid
                if (connectorIsValid && firstQuantityValid && secondQuantityValid && firstUnitValid && secondUnitValid &&
                    directionValid)
                {
                    // Applies direction to first quantity
                    int signedFirstQuantity = firstQuantity * directionSign;

                    // Temporary interval for first segment
                    DateTime? firstStart;
                    DateTime? firstEnd;

                    // Applies first offset
                    bool firstApplied = ApplyRelativeUnitGeneric(signedFirstQuantity, firstCanonicalUnit,
                        now, out firstStart, out firstEnd);

                    // Continues only if first offset succeeded
                    if (firstApplied && firstStart.HasValue)
                    {
                        // Intermediate date after first offset
                        DateTime intermediateDate = firstStart.Value;
                        // Applies direction to second quantity
                        int signedSecondQuantity = secondQuantity * directionSign;

                        // Temporary interval for second segment
                        DateTime? secondStart;
                        DateTime? secondEnd;

                        // Applies second offset
                        bool secondApplied = ApplyRelativeUnitGeneric(signedSecondQuantity, secondCanonicalUnit,
                            intermediateDate, out secondStart, out secondEnd);

                        // Finalizes result if second offset succeeded
                        if (secondApplied && secondStart.HasValue)
                        {
                            // Composites expression resolves to a single date
                            startDateTime = secondStart;
                            endDateTime = secondStart;
                            parseSuccessful = true;
                        }
                    }
                }
            }

            // Return final parsing status
            return parseSuccessful;
        }

        /// <summary>
        /// Parses relative expressions using "ago" semantics, such as:
        /// "3 days ago", "2 weeks ago", "il y a 5 jours", "hace 3 semanas".
        /// The method supports any language defined in LangDict.
        /// </summary>
        private bool TryRelativeAgoExpression(List<string> tokens, int tokenIndex,
            DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            // Initializes output interval
            startDateTime = null;
            endDateTime = null;

            // Tracks whether parsing succeeded
            bool parseSuccessful = false;

            // Pattern: "X unit ago"
            // Examples: "3 days ago", "2 weeks ago", "5 months ago"
            if (tokenIndex + 2 < tokens.Count)
            {
                // Extracts quantity token
                string quantityToken = tokens[tokenIndex];
                
                // Normalizes unit token
                string unitToken = LangDict.NormalizeKey(tokens[tokenIndex + 1]);
                
                // Normalizes direction token ("ago", "hace", etc.)
                string directionToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);

                // Validates quantity
                bool quantityValid = TryParseNumberWord(quantityToken, out int quantityValue) ||
                    int.TryParse(quantityToken, out quantityValue);

                // Validates unit
                bool unitValid = LangDict.TimeUnitDict.TryGetValue(unitToken, out string canonicalUnit);

                // Validates direction ("ago" = -1)
                bool directionValid = LangDict.TimeAgoKeywordSet.Contains(directionToken);

                if (quantityValid && unitValid && directionValid)
                {
                    // "ago" always means negative direction
                    int signedQuantity = quantityValue * -1;

                    // Temporary interval
                    DateTime? computedStart;
                    DateTime? computedEnd;

                    // Applies relative offset
                    bool applied = ApplyRelativeUnitGeneric(signedQuantity, canonicalUnit, now,
                        out computedStart, out computedEnd);

                    if (applied && computedStart.HasValue)
                    {
                        // Result is a single date
                        startDateTime = computedStart;
                        endDateTime = computedStart;
                        parseSuccessful = true;
                    }
                }
            }

            // Pattern: "il y a X unit" (French) or similar reversed forms
            // Examples: "il y a 3 jours", "hace 2 semanas"
            if (!parseSuccessful && tokenIndex + 3 < tokens.Count)
            {
                // Normalizes first token ("il", "hace", etc.)
                string firstToken = LangDict.NormalizeKey(tokens[tokenIndex]);
                
                // Normalizes second token ("y", "ya", etc.)
                string secondToken = LangDict.NormalizeKey(tokens[tokenIndex + 1]);
                
                // Normalizes third token ("a", etc.)
                string thirdToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);

                // Extracts quantity token
                string quantityToken = tokens[tokenIndex + 3];
                
                // Normalizes unit token
                string unitToken = LangDict.NormalizeKey(tokens[tokenIndex + 4]);

                // Validates "ago" structure (language‑specific)
                bool agoStructureValid =
                    LangDict.TimeAgoPrefixSet.Contains(firstToken) &&
                    LangDict.TimeAgoMiddleSet.Contains(secondToken) &&
                    LangDict.TimeAgoSuffixSet.Contains(thirdToken);

                // Validates quantity
                bool quantityValid = TryParseNumberWord(quantityToken, out int quantityValue) ||
                    int.TryParse(quantityToken, out quantityValue);

                // Validates unit
                bool unitValid = LangDict.TimeUnitDict.TryGetValue(unitToken, out string canonicalUnit);

                if (agoStructureValid && quantityValid && unitValid)
                {
                    // Always negative direction
                    int signedQuantity = quantityValue * -1;

                    // Temporary interval
                    DateTime? computedStart;
                    DateTime? computedEnd;

                    // Applies relative offset
                    bool applied = ApplyRelativeUnitGeneric(signedQuantity, canonicalUnit, now,
                        out computedStart, out computedEnd);

                    if (applied && computedStart.HasValue)
                    {
                        startDateTime = computedStart;
                        endDateTime = computedStart;
                        parseSuccessful = true;
                    }
                }
            }

            // Return final parsing status
            return parseSuccessful;
        }

        /// <summary>
        /// Parses composite directional expressions such as:
        /// "after 2 weeks and 3 days".
        /// </summary>
        private bool TryRelativeDirectionalCompositeExpression(List<string> tokens,
            int tokenIndex, DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            // Needs: direction + qty1 + unit1 + connector + qty2 + unit2
            if (tokenIndex + 5 >= tokens.Count)
            {
                return false;
            }

            string directionToken = LangDict.NormalizeKey(tokens[tokenIndex]);

            if (!LangDict.TimeDirectionDict.TryGetValue(directionToken, out int directionSign))
            {
                return false;
            }

            int nextIndex = tokenIndex + 1;

            // Optional preposition ("de", "du", "of", "del", etc.)
            string possiblePreposition = LangDict.NormalizeKey(tokens[nextIndex]);

            if (LangDict.OptionalPrepositionSet.Contains(possiblePreposition))
            {
                nextIndex++;
            }

            // First quantity
            int quantity1;

            if (!TryParseNumberWord(tokens[nextIndex], out quantity1) &&
                !int.TryParse(tokens[nextIndex], out quantity1))
            {
                return false;
            }

            nextIndex++;

            // First unit
            string unit1Token = LangDict.NormalizeKey(tokens[nextIndex]);

            if (!LangDict.TimeUnitDict.TryGetValue(unit1Token, out string canonicalUnit1))
            {
                return false;
            }

            nextIndex++;

            // Connector ("and", "et", "y")
            string connectorToken = LangDict.NormalizeKey(tokens[nextIndex]);

            if (!LangDict.AndKeywordSet.Contains(connectorToken))
            {
                return false;
            }

            nextIndex++;

            // Second quantity
            int quantity2;

            if (!TryParseNumberWord(tokens[nextIndex], out quantity2) &&
                !int.TryParse(tokens[nextIndex], out quantity2))
            {
                return false;
            }

            nextIndex++;

            // Second unit
            string unit2Token = LangDict.NormalizeKey(tokens[nextIndex]);

            if (!LangDict.TimeUnitDict.TryGetValue(unit2Token, out string canonicalUnit2))
            {
                return false;
            }

            int signedQuantity1 = quantity1 * directionSign;
            int signedQuantity2 = quantity2 * directionSign;

            DateTime? computedStart = now;
            DateTime? computedEnd = now;

            bool firstOffsetApplied = ApplyRelativeUnitGeneric(signedQuantity1, canonicalUnit1,
                computedStart.Value, out computedStart, out computedEnd);

            if (!firstOffsetApplied || !computedStart.HasValue)
            {
                return false;
            }

            bool secondOffsetApplied = ApplyRelativeUnitGeneric(signedQuantity2, canonicalUnit2,
                computedStart.Value, out computedStart, out computedEnd);

            if (!secondOffsetApplied || !computedStart.HasValue)
            {
                return false;
            }

            startDateTime = computedStart;
            endDateTime = computedStart;

            return true;
        }

        /// <summary>
        /// Parses directional relative expressions where the direction
        /// appears before the quantity, such as:
        /// "before 3 days", "after 2 weeks"
        /// </summary>
        private bool TryRelativeDirectionalExpression(List<string> tokens, int tokenIndex,
            DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            // Initializes output interval
            startDateTime = null;
            endDateTime = null;

            // Tracks whether parsing succeeded
            bool parseSuccessful = false;

            // Pattern: direction + quantity + unit
            // Examples: "before 3 days", "après 2 semaines", "antes 3 dias"
            if (tokenIndex + 2 < tokens.Count)
            {
                // Normalizes direction token
                string directionToken = LangDict.NormalizeKey(tokens[tokenIndex]);
                
                // Extracts quantity token
                string quantityToken = tokens[tokenIndex + 1];
                
                // Normalizes unit token
                string unitToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);

                // Validates direction ("before", "after", "avant", "apres", "antes", "despues")
                bool directionValid = LangDict.TimeDirectionDict.TryGetValue(directionToken, out int directionSign);

                // Validates quantity
                bool quantityValid = TryParseNumberWord(quantityToken, out int quantityValue) ||
                    int.TryParse(quantityToken, out quantityValue);

                // Validates unit
                bool unitValid = LangDict.TimeUnitDict.TryGetValue(unitToken, out string canonicalUnit);

                if (directionValid && quantityValid && unitValid)
                {
                    // Applies direction sign
                    int signedQuantity = quantityValue * directionSign;

                    // Temporary interval
                    DateTime? computedStart;
                    DateTime? computedEnd;

                    // Applies relative offset
                    bool relativeOffsetAppliedSuccessfully = ApplyRelativeUnitGeneric(signedQuantity,
                        canonicalUnit, now,out computedStart, out computedEnd);

                    if (relativeOffsetAppliedSuccessfully && computedStart.HasValue)
                    {
                        startDateTime = computedStart;
                        endDateTime = computedStart;
                        parseSuccessful = true;
                    }
                }
            }

            // ---------------------------------------------------------------------
            // Pattern: direction + "de" + quantity + unit (Spanish)
            // Examples: "antes de 3 dias", "despues de 2 semanas"
            // ---------------------------------------------------------------------
            if (!parseSuccessful && tokenIndex + 3 < tokens.Count)
            {
                // Normalizes direction token
                string directionToken = LangDict.NormalizeKey(tokens[tokenIndex]);
                
                // Normalizes "de"
                string middleToken = LangDict.NormalizeKey(tokens[tokenIndex + 1]);
                
                // Extracts quantity token
                string quantityToken = tokens[tokenIndex + 2];
                
                // Normalizes unit token
                string unitToken = LangDict.NormalizeKey(tokens[tokenIndex + 3]);

                // Validates direction
                bool directionValid = LangDict.TimeDirectionDict.TryGetValue(directionToken, out int directionSign);

                // Validates "de"
                bool middleValid = middleToken == "de";

                // Validates quantity
                bool quantityValid = TryParseNumberWord(quantityToken, out int quantityValue) ||
                    int.TryParse(quantityToken, out quantityValue);

                // Validates unit
                bool unitValid = LangDict.TimeUnitDict.TryGetValue(unitToken, out string canonicalUnit);

                if (directionValid && middleValid && quantityValid && unitValid)
                {
                    int signedQuantity = quantityValue * directionSign;

                    DateTime? computedStart;
                    DateTime? computedEnd;

                    bool applied = ApplyRelativeUnitGeneric(signedQuantity, canonicalUnit, now,
                        out computedStart, out computedEnd);

                    if (applied && computedStart.HasValue)
                    {
                        startDateTime = computedStart;
                        endDateTime = computedStart;
                        parseSuccessful = true;
                    }
                }
            }

            return parseSuccessful;
        }

        /// <summary>
        /// Parses basic relative date expressions such as:
        /// "in 3 days", "within 2 weeks", "dans 5 jours", "en 3 meses",
        /// as well as reversed forms like "3 days before", "2 weeks after", "5 meses antes".
        /// The behavior is fully driven by the dictionaries defined in LangDict.
        /// </summary>
        private bool TryRelativeExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            bool parseSuccessful = false;

            // Pattern: "in/within X unit"
            if (tokenIndex + 2 < tokens.Count)
            {
                string startToken = LangDict.NormalizeKey(tokens[tokenIndex]);

                if (LangDict.RelativePrepositionSet.Contains(startToken))
                {
                    string quantityToken = tokens[tokenIndex + 1];
                    string unitToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);

                    // Validate unit
                    if (LangDict.TimeUnitDict.TryGetValue(unitToken, out string canonicalUnit))
                    {
                        // Validate quantity
                        if (TryParseNumberWord(quantityToken, out int quantity) ||
                            int.TryParse(quantityToken, out quantity))
                        {
                            parseSuccessful = ApplyRelativeUnitGeneric(quantity, canonicalUnit, now,
                                out startDateTime, out endDateTime);
                        }
                    }
                }
            }

            // "X unit before/after"
            if (!parseSuccessful && tokenIndex + 2 < tokens.Count)
            {
                string quantityToken = tokens[tokenIndex];

                if (TryParseNumberWord(quantityToken, out int relativeQuantity) ||
                    int.TryParse(quantityToken, out relativeQuantity))
                {
                    string unitToken = LangDict.NormalizeKey(tokens[tokenIndex + 1]);
                    string directionToken = LangDict.NormalizeKey(tokens[tokenIndex + 2]);

                    // Validates unit
                    if (LangDict.TimeUnitDict.TryGetValue(unitToken, out string canonicalUnit))
                    {
                        // Validates direction
                        if (LangDict.TimeDirectionDict.TryGetValue(directionToken, out int directionSign))
                        {
                            int signedQuantity = relativeQuantity * directionSign;

                            parseSuccessful = ApplyRelativeUnitGeneric(signedQuantity, canonicalUnit, now,
                                out startDateTime, out endDateTime);
                        }
                    }
                }
            }

            return parseSuccessful;
        }

        /// <summary>
        /// Matches year‑range expressions such as:
        /// "this year", "next year", "last year"
        /// in any supported language. 
        /// </summary>
        private bool TryYearExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            // Normalizes the token (lowercase, remove accents, Unicode‑safe)
            string token = LangDict.NormalizeKey(tokens[tokenIndex]);

            // Checks whether the token corresponds to a known year-range keyword in the dictionary
            if (!LangDict.YearRangeDict.TryGetValue(token, out string rangeType))
            {
                return false;
            }

            // "This year": January 1st to December 31st of the current year
            if (rangeType == "this")
            {
                startDateTime = new DateTime(now.Year, 1, 1);
                endDateTime = new DateTime(now.Year, 12, 31);
                return true;
            }

            // "Next year": January 1st to December 31st of next year
            if (rangeType == "next")
            {
                startDateTime = new DateTime(now.Year + 1, 1, 1);
                endDateTime = new DateTime(now.Year + 1, 12, 31);
                return true;
            }

            // "Last year": January 1st to December 31st of previous year
            if (rangeType == "last")
            {
                startDateTime = new DateTime(now.Year - 1, 1, 1);
                endDateTime = new DateTime(now.Year - 1, 12, 31);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Matches weekday expressions such as:
        /// "next monday", "last friday", "lundi prochain", "viernes pasado"
        /// in any supported language.
        /// </summary>
        private bool TryWeekdayExpression(List<string> tokens, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            // Normalizes token (lowercase, remove accents, Unicode‑safe)
            string token = LangDict.NormalizeKey(tokens[tokenIndex]);

            // Checks if token is a weekday
            if (!LangDict.WeekdayDict.TryGetValue(token, out DayOfWeek targetDay))
            {
                return false;
            }

            // Looks ahead for next/last keywords
            string nextToken = (tokenIndex + 1 < tokens.Count)
                ? LangDict.NormalizeKey(tokens[tokenIndex + 1])
                : null;

            int offset = 0;

            if (nextToken != null)
            {
                if (LangDict.NextWeekdayKeywordSet.Contains(nextToken))
                {
                    offset = +1;
                }

                else if (LangDict.PreviousWeekdayKeywordSet.Contains(nextToken))
                {
                    offset = -1;
                }
            }

            // Computes the base date (this week)
            int currentDay = (int)now.DayOfWeek;
            int target = (int)targetDay;

            int delta = target - currentDay;

            // Applies next/last offset
            if (offset == +1)
            {
                if (delta <= 0)
                {
                    delta += 7;
                }
            }
            else if (offset == -1)
            {
                if (delta >= 0)
                {
                    delta -= 7;
                }
            }

            DateTime result = now.Date.AddDays(delta);

            startDateTime = result;
            endDateTime = result;

            return true;
        }
    }
}

