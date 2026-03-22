/// <file>SmartSearch.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 22th, 2026</date>

using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
        /// Builds the SQL WHERE clause used to retrieve candidate tasks.
        ///
        /// This method unifies all search signals into a single SQL filter:
        /// - Textual fuzzy search
        /// - Explicit date intervals (start/end)
        /// - Month‑based filtering
        /// - Semantic priority categories
        ///
        /// All generated conditions are combined with AND, ensuring that tasks
        /// must satisfy every applicable constraint.  
        ///
        /// The method also produces a list of SQLite parameters corresponding
        /// to all generated @pX placeholders (text tokens, dates, priority).
        /// </summary>
        private string BuildSqlWhere(HashSet<string> ExpandedTokensSet, DateTime? startDate,
        DateTime? endDate, DateTime? detectedMonth, string priorityCategory,
        bool onlyTemporalTokens, out List<SQLiteParameter> lstSqliteParameters)
        {
            // Accumulates all SQL fragments before joining them with AND
            HashSet<string> SqlConditionsSet = new HashSet<string>();
            lstSqliteParameters = new List<SQLiteParameter>();

            // ------------------------------------------------------------
            // TEXT SEARCH (fuzzy, Levenshtein‑expanded tokens)
            // ------------------------------------------------------------
            //
            // For each expanded token, we generate:
            //   (title LIKE @titleX OR description LIKE @descX)
            //
            // All token conditions are OR‑combined inside a single block:
            //   ( (title LIKE ...) OR (title LIKE ...) OR ... )
            //
            // This ensures that tasks match any expanded token.
            // ------------------------------------------------------------
            if (!onlyTemporalTokens && ExpandedTokensSet != null && ExpandedTokensSet.Count > 0)
            {
                HashSet<string> TokenConditionsSet = new HashSet<string>();
                int tokenParamIndex = 0;

                foreach (string token in ExpandedTokensSet)
                {
                    string paramTitle = "@title" + tokenParamIndex;
                    string paramDesc = "@desc" + tokenParamIndex;

                    TokenConditionsSet.Add($"(title LIKE {paramTitle} OR description LIKE {paramDesc})");

                    lstSqliteParameters.Add(new SQLiteParameter(paramTitle, $"%{token}%"));
                    lstSqliteParameters.Add(new SQLiteParameter(paramDesc, $"%{token}%"));

                    tokenParamIndex++;
                }

                // Wrap all OR conditions into a single parenthesized block
                SqlConditionsSet.Add("(" + string.Join(" OR ", TokenConditionsSet) + ")");
            }

            // ------------------------------------------------------------
            // EXPLICIT DATE RANGE (from natural‑language parsing)
            // ------------------------------------------------------------
            //
            // If both start and end dates are detected, we restrict tasks to:
            //   deadline >= @startDate AND deadline <= @endDate
            //
            // This is used for expressions like:
            //   "in 3 days", "between Monday and Friday", "next week", etc.
            // ------------------------------------------------------------
            if (startDate.HasValue && endDate.HasValue)
            {
                string paramStart = "@startDate";
                string paramEnd = "@endDate";

                SqlConditionsSet.Add($"(deadline >= {paramStart} AND deadline <= {paramEnd})");

                lstSqliteParameters.Add(new SQLiteParameter(paramStart, startDate.Value.ToString("yyyy-MM-dd")));
                lstSqliteParameters.Add(new SQLiteParameter(paramEnd, endDate.Value.ToString("yyyy-MM-dd")));
            }

            // ------------------------------------------------------------
            // MONTH FILTER (e.g., "in March", "en mars", "en marzo")
            // ------------------------------------------------------------
            //
            // If a month is detected, we restrict tasks to the entire month:
            //   deadline >= first day of month
            //   deadline <= last day of month
            //
            // This is independent of explicit date ranges.
            // ------------------------------------------------------------
            if (detectedMonth.HasValue)
            {
                DateTime monthStart = detectedMonth.Value;
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                string paramMonthStart = "@monthStart";
                string paramMonthEnd = "@monthEnd";

                SqlConditionsSet.Add($"(deadline >= {paramMonthStart} AND deadline <= {paramMonthEnd})");

                lstSqliteParameters.Add(new SQLiteParameter(paramMonthStart, monthStart.ToString("yyyy-MM-dd")));
                lstSqliteParameters.Add(new SQLiteParameter(paramMonthEnd, monthEnd.ToString("yyyy-MM-dd")));
            }

            // ------------------------------------------------------------
            // PRIORITY FILTER (semantic categories)
            // ------------------------------------------------------------
            //
            // Priority is not a lexical filter.  
            // It is a semantic category detected from the query:
            //   "important", "urgent", "anniversary", etc.
            //
            // Each category maps to one or more Priorities_id values.
            // ------------------------------------------------------------
            if (!string.IsNullOrEmpty(priorityCategory))
            {
                if (priorityCategory == "important")
                {
                    // Important tasks include categories 1 and 3
                    SqlConditionsSet.Add("Priorities_id IN (1,3)");
                }
                else if (priorityCategory == "anniversary")
                {
                    SqlConditionsSet.Add("Priorities_id = 4");
                }
            }

            // ------------------------------------------------------------
            // FINAL ASSEMBLY
            // ------------------------------------------------------------
            //
            // If no conditions were generated, return an empty WHERE clause.
            // Otherwise, join all fragments with AND.
            //
            // This ensures that:
            //   - text filters
            //   - date filters
            //   - month filters
            //   - priority filters
            //
            // all apply simultaneously.
            // ------------------------------------------------------------
            if (SqlConditionsSet.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" AND ", SqlConditionsSet);
        }

        /// <summary>
        /// Computes the Levenshtein distance between two strings.
        /// The Levenshtein distance represents the minimum number of
        /// single‑character edits (insertions, deletions, substitutions)
        /// required to transform one string into the other.
        /// </summary>
        /// <remarks>
        /// This function builds a dynamic programming table that compares
        /// both strings character by character.  
        /// Each cell stores the minimum number of edits required to transform
        /// the prefix of the source string into the prefix of the target string.  
        /// The algorithm fills the entire table, so its time and memory usage
        /// grow proportionally with the lengths of the two input strings.
        /// </remarks>
        /// <param name="source">
        /// The original string from which the transformation begins.
        /// </param>
        /// <param name="target">
        /// The string to which the source string is compared and transformed.
        /// </param>
        private static int CalculateLevenshteinDistance(string source, string target)
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
        /// Cleans and normalizes the raw user query before parsing. 
        /// </summary>
        /// <returns>A stable, predictable input string for the next parsing steps.</returns>
        /// <param name="query"></param>
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

            // Normalizes separators (commas, semicolons, slashes) by replacing them with spaces.
            cleanedQuery = cleanedQuery.Replace(",", " ")
                             .Replace(";", " ")
                             .Replace("/", " ");

            // Removes punctuation that has no meaning.
            // Apostrophes and similar characters are removed because they can break
            // SQL queries when building the WHERE clause.
            cleanedQuery = Regex.Replace(cleanedQuery, @"[!?:()\[\]{}""'’]", "");

            // Protects logical operators by adding spaces around them.
            // This prevents accidental matches inside normal words
            // (e.g., "candy" should not be interpreted as "c AND y").
            cleanedQuery = Regex.Replace(cleanedQuery, @"\bAND\b", " AND ", RegexOptions.IgnoreCase);
            cleanedQuery = Regex.Replace(cleanedQuery, @"\bOR\b", " OR ", RegexOptions.IgnoreCase);

            // Normalizes multiple spaces again after replacements
            cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ");

            return cleanedQuery.Trim();
        }

        /// <summary>
        /// Determines whether the query should be interpreted as a pure temporal search.
        /// Returns true only if all tokens belong to the temporal domain.
        ///
        /// A token is considered temporal if it appears in any of the following:
        /// - Day names (DayWordSet, WeekdayDict, WeekdayNameToDayOfWeekDict)
        /// - Relative expressions (RelativeDayOffsetDict, RelativeDirectionDict)
        /// - Temporal prepositions (RelativePrepositionSet)
        /// - Time units (TimeUnitDict, TimeDirectionDict)
        /// - "Ago" patterns (TimeAgoKeywordSet, TimeAgoPrefixSet, TimeAgoMiddleSet, TimeAgoSuffixSet)
        /// - Month names and ranges (MonthNumberDict, MonthRangeDict)
        /// - Year ranges (YearRangeDict)
        /// - Numeric temporal quantities (NumberUnitDict, NumberTenDict, NumberMultiplierDict)
        ///
        /// If any token does not belong to these sets, the search switches to lexical mode.
        /// </summary>
        private bool ContainsOnlyTemporalTokens(HashSet<string> expandedTokens)
        {
            foreach (var token in expandedTokens)
            {
                bool isTemporal =
                    LangDict.DayWordSet.Contains(token) ||
                    LangDict.NextWeekdayKeywordSet.Contains(token) ||
                    LangDict.PreviousWeekdayKeywordSet.Contains(token) ||
                    LangDict.RelativeDayOffsetDict.ContainsKey(token) ||
                    LangDict.RelativeDirectionDict.ContainsKey(token) ||
                    LangDict.RelativePrepositionSet.Contains(token) ||
                    LangDict.TimeUnitDict.ContainsKey(token) ||
                    LangDict.TimeDirectionDict.ContainsKey(token) ||
                    LangDict.TimeAgoKeywordSet.Contains(token) ||
                    LangDict.TimeAgoPrefixSet.Contains(token) ||
                    LangDict.TimeAgoMiddleSet.Contains(token) ||
                    LangDict.TimeAgoSuffixSet.Contains(token) ||
                    LangDict.MonthNumberDict.ContainsKey(token) ||
                    LangDict.MonthRangeDict.ContainsKey(token) ||
                    LangDict.YearRangeDict.ContainsKey(token) ||
                    LangDict.WeekdayDict.ContainsKey(token) ||
                    LangDict.WeekdayNameToDayOfWeekDict.ContainsKey(token) ||
                    LangDict.NumberUnitDict.ContainsKey(token) ||
                    LangDict.NumberTenDict.ContainsKey(token) ||
                    LangDict.NumberMultiplierDict.ContainsKey(token);

                if (!isTemporal)
                    return false; // Found a lexical token: switch to lexical mode
            }

            return true; // All tokens are temporal: switch to pure temporal mode
        }

        /// <summary>
        /// Detects whether the user query contains a month name in all supported languages
        /// and returns the first day of the detected month, or null if none is found.
        /// </summary>
        /// <param name="TokensSet">
        /// Set of normalized tokens extracted from the user query.  
        /// Each token is checked against the month dictionary to detect
        /// whether the query contains a month name in any supported language.
        /// </param>
        private DateTime? DetectMonth(HashSet<string> TokensSet)
        {
            if (TokensSet == null || TokensSet.Count == 0)
            {
                return null;
            }

            foreach (string token in TokensSet)
            {
                string normalizedToken = token.Trim();

                if (LangDict.MonthNumberDict.ContainsKey(normalizedToken))
                {
                    // Stores the month number corresponding to the detected month name
                    int monthNumber = LangDict.MonthNumberDict[normalizedToken];

                    // Creates a DateTime for the detected month using the current year.
                    // Only the month matters for SmartSearch; the day is always set to 1.
                    DateTime detectedMonth = new DateTime(DateTime.Now.Year, monthNumber, 1);

                    return detectedMonth;
                }
            }

            return null;
        }

        /// <summary>
        /// Detects semantic priority categories based on normalized tokens.
        /// Uses LangDict.PriorityKeywordDict (token → category).
        /// Returns the first matching category, or null if none is found.
        /// </summary>
        private string DetectPriority(HashSet<string> normalizedTokens)
        {
            foreach (var token in normalizedTokens)
            {
                if (LangDict.PriorityKeywordDict.TryGetValue(token, out string category))
                {
                    return category;
                }
            }

            return null; // No priority detected
        }

        /// <summary>
        /// Generates typo‑tolerant variantTokens for each token using Levenshtein distance (≤ 2).
        /// </summary>
        /// <param name="TokensSet">The list of original tokens extracted from the query.</param>
        private HashSet<string> ExpandTokensLevenshtein(HashSet<string> TokensSet)
        {
            /// Uses a HashSet to avoid duplicates and speed up membership checks.
            /// This is important because Levenshtein expansion can generate many variants.
            HashSet<string> ExpandedTokensSet = new HashSet<string>();

            if (TokensSet == null || TokensSet.Count == 0)
            {
                return ExpandedTokensSet;
            }

            // Each variant is compared using Levenshtein distance
            foreach (string token in TokensSet)
            {
                // Always include the original token
                ExpandedTokensSet.Add(token);

                // Skips Levenshtein expansion for temporal keywords
                if (LangDict.MonthNumberDict.ContainsKey(token) ||         // months
                    LangDict.WeekdayDict.ContainsKey(token) ||             // weekdays
                    LangDict.TimeUnitDict.ContainsKey(token) ||            // day, week, month, year
                    LangDict.TimeDirectionDict.ContainsKey(token) ||       // before, after
                    LangDict.RelativePrepositionSet.Contains(token))       // in, dans, en, within...
                {
                    continue;
                }

                // Skips expansion for long tokens (too many variants)
                if (token.Length > 8)
                {
                    continue;
                }

                // Generates variantTokens for the current token
                List<string> lstVariantTokens = GenerateLevenshteinVariants(token);

                foreach (string variant in lstVariantTokens)
                {
                    // Safety limit: prevents SQLite "too many variables" crash
                    if (ExpandedTokensSet.Count >= 50)
                    {
                        break;
                    }

                    int LevenshteinDistance = CalculateLevenshteinDistance(token, variant);

                    if (LevenshteinDistance <= 2 && !ExpandedTokensSet.Contains(variant))
                    {
                        ExpandedTokensSet.Add(variant);
                    }
                }
            }

            return ExpandedTokensSet;
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
        public (DateTime? startDate, DateTime? endDate) ParseNaturalDates(HashSet<string> titleTokens,
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
            HashSet<string> descriptionTokens = TokenizeQuery(description);

            // Tries parsing date from description tokens
            if (TryParseDateTokens(descriptionTokens, now, out startDateTime, out endDateTime))
            {
                return (startDateTime, endDateTime);
            }

            return (null, null);
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
            HashSet<string> tokens, HashSet<string> expandedTokens)
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
                            .Select(word => CalculateLevenshteinDistance(expandedToken, word))
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
        /// Executes a unified smart search pipeline combining:
        /// - lexical fuzzy search (tokens, Levenshtein expansion, scoring)
        /// - temporal parsing (relative dates, weekdays, month filters)
        /// - semantic priority detection
        ///
        /// The method is fully fault‑tolerant: any unexpected exception triggers
        /// a safe fallback via NotifyCrashResult().
        /// </summary>
        public List<Tasks> Search(string rawQuery)
        {
            try
            {
                // Cleans and normalizes the raw query
                string cleanedQuery = CleanQuery(rawQuery);
                string normalizedQuery = NormalizeQuery(cleanedQuery);

                // Tokenization
                HashSet<string> NormalizedTokensSet = TokenizeQuery(normalizedQuery);

                // Fuzzy expansion (Levenshtein-based)
                HashSet<string> ExpandedTokensSet = ExpandTokensLevenshtein(NormalizedTokensSet);

                // Detects semantic priority (important or anniversary)
                string priorityCategory = DetectPriority(NormalizedTokensSet);

                // If the query is purely semantic ("important" or "anniversaire"),
                // we disable lexical LIKE to avoid over-filtering.
                if (!string.IsNullOrEmpty(priorityCategory) && NormalizedTokensSet.Count == 1)
                {
                    ExpandedTokensSet.Clear();
                }

                // Extracts temporal signals
                (DateTime? startDate, DateTime? endDate) =
                    ParseNaturalDates(NormalizedTokensSet, rawQuery, DateTime.Today);

                DateTime? monthFilter = DetectMonth(ExpandedTokensSet);

                // Determines search mode:
                // - Pure temporal mode: only temporal tokens present
                // - Lexical mode: at least one lexical token present
                bool onlyTemporalTokens = ContainsOnlyTemporalTokens(NormalizedTokensSet);

                // Builds a unified SQL Where clause (single query for both modes)
                string sqlWhere;
                List<SQLiteParameter> sqlParams;

                sqlWhere = BuildSqlWhere(ExpandedTokensSet, startDate, endDate, monthFilter,
                priorityCategory, onlyTemporalTokens, out sqlParams);

                // Retrieves candidate tasks from the database
                List<Tasks> dbResults = dbConn.SearchTasks(sqlWhere, sqlParams);

                // Unified scoring (lexical and temporal signals)
                List<ScoredTask> scored = ScoreCandidates(dbResults, NormalizedTokensSet, ExpandedTokensSet);

                // Final output depending on the mode

                // Pure temporal mode
                if (onlyTemporalTokens)
                {
                    // Ignores score and sorts by date
                    return scored
                        .Select(s => s.Task)
                        .OrderBy(t => t.Deadline)
                        .ToList();
                }

                // Lexical mode
                else
                {
                    // Applies threshold(score > 0) and sorts by relevance
                    return scored
                        .Where(s => s.Score > 0)
                        .OrderByDescending(s => s.Score)
                        .Select(s => s.Task)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Always return a safe fallback result
                return NotifyCrashResult();
            }
        }

        /// <summary>
        /// Splits the user query into normalized tokens.
        /// Removes punctuation, trims spaces, lowercases everything.
        /// </summary>
        private static HashSet<string> TokenizeQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new HashSet<string>();
            }

            // Normalizes punctuation
            char[] separators = new[] { ' ', ',', ';', '.', ':', '!', '?', '/', '\\', '-', '_', '(', ')', '[', ']', '{', '}' };

            HashSet<string> tokens = new HashSet<string>(query.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries));

            return tokens;
        }

        /// <summary>
        /// Tries to match absolute date keywords (today, tomorrow, yesterday) in supported languages
        /// </summary>
        /// <param name="token"></param>
        /// <param name="now"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        private static bool TryAbsoluteKeyword(string token, DateTime now, out DateTime? startDateTime,
            out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            if (LangDict.RelativeDayOffsetDict.TryGetValue(token, out int offset))
            {
                startDateTime = endDateTime = now.Date.AddDays(offset);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies a relative offset to a standard time unit ("day", "week", "month", "year").
        /// The method computes the resulting date or date range based on the quantity provided.
        /// Returns <c>true</c> when the unit is recognized and the offset can be applied.
        /// </summary>
        /// <param name="offset">Positive or negative quantity representing the offset to apply.</param>
        /// <param name="normalizedUnit">Normalized time unit</param>
        /// <param name="now">Reference date used as the origin for the calculation.</param>
        /// <param name="startDateTime">Output: the computed start date of the resulting interval.</param>
        /// <param name="endDateTime">Output: the computed end date of the resulting interval.</param>
        /// <returns> true if the unit is supported and the offset was applied; otherwise false.
        /// </returns>

        private static bool TryApplyRelativeUnit(int offset, string normalizedUnit, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            // Days: simple offset on the current date
            if (normalizedUnit == "day")
            {
                startDateTime = now.Date.AddDays(offset);
                endDateTime = startDateTime;
                return true;
            }

            // Weeks: treated as complete intervals
            else if (normalizedUnit == "week")
            {
                startDateTime = now.Date.AddDays(offset * 7);
                endDateTime = startDateTime.Value.AddDays(6);
                return true;
            }

            // Months: uses AddMonths to keep calendar semantics
            else if (normalizedUnit == "month")
            {
                startDateTime = now.Date.AddMonths(offset);
                endDateTime = startDateTime;
                return true;
            }

            // Years: uses AddYears to keep calendar semantics
            else if (normalizedUnit == "year")
            {
                startDateTime = now.Date.AddYears(offset);
                endDateTime = startDateTime;
                return true;
            }

            // Unknown unit: nothing applied
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
        /// Matches month‑range expressions such as:
        /// "this month", "next month", "last month"
        /// in any supported language.
        /// </summary>
        private bool TryMonthExpression(HashSet<string> TokensSet, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            // Normalizes token (lowercase, remove accents, Unicode‑safe)
            string token = LangDict.NormalizeKey(TokensSet.ElementAt(tokenIndex));

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
        private bool TryOrdinalDate(HashSet<string> TokensSet, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;
            
            int dayNumber;
            bool hasOrdinalSuffix;
            
            // Accept ordinal day ("3rd", "7ème", "2do") or number word ("three", "trois", "tres") or digits ("3")
            if (!TryParseOrdinalDay(TokensSet.ElementAt(tokenIndex), out dayNumber, out hasOrdinalSuffix) &&
            !TryParseNumberWord(TokensSet.ElementAt(tokenIndex), out dayNumber) &&
            !int.TryParse(TokensSet.ElementAt(tokenIndex), out dayNumber))
            {
                return false;
            }

            // Case 1 — "7eme jour" / "3rd day" / "2do dia"
            if (tokenIndex + 1 < TokensSet.Count &&
                LangDict.DayWordSet.Contains(TokensSet.ElementAt(tokenIndex + 1)))
            {
                DateTime explicitDateChosen = new DateTime(now.Year, now.Month, dayNumber);
                startDateTime = explicitDateChosen;
                endDateTime = explicitDateChosen;
                return true;
            }

            // Case 2 — "7eme mars" / "3rd april" / "2do abril"
            if (tokenIndex + 1 < TokensSet.Count &&
                LangDict.MonthNumberDict.ContainsKey(TokensSet.ElementAt(tokenIndex + 1)))
            {
                int monthNumber = LangDict.MonthNumberDict[TokensSet.ElementAt(tokenIndex + 1)];

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
        public bool TryParseAbsoluteRangeTokens(HashSet<string> tokensSet, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            if (tokensSet == null || tokensSet.Count == 0)
            {
                return false;
            }

            // Normalizes tokens
            List<string> normalizedTokens = tokensSet
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
            HashSet<string> leftTokens = tokensSet.Take(separatorIndex).ToHashSet();
            HashSet<string> rightTokens = tokensSet.Skip(separatorIndex + 1).ToHashSet();

            // Removes optional prefixes ("du", "from", "del", "période", "period", "periodo", etc.)
            leftTokens = leftTokens.Where(leftToken => 
            !LangDict.RangeOptionalPrefixSet.Contains(LangDict.NormalizeKey(leftToken))).ToHashSet();

            rightTokens = rightTokens.Where(rightToken => !LangDict.RangeOptionalPrefixSet.Contains(LangDict.NormalizeKey(rightToken)))
                .ToHashSet();

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

            HashSet<string> segmentTokens = TokenizeQuery(dateSegment);

            if (segmentTokens == null || segmentTokens.Count == 0)
            {
                return false;
            }

            return TryParseDateTokens(segmentTokens, now, out startDate, out endDate);
        }

        public bool TryParseDateTokens(HashSet<string> tokensSet, DateTime now,
        out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = null;
            endDateTime = null;

            if (tokensSet == null || tokensSet.Count == 0)
            {
                return false;
            }

            // Freezes the order of the tokens
            List<string> tokensList = tokensSet.ToList();

            // Handlers who work on all tokens

            // Absolute ranges like "from 3 to 7", "3-7"
            if (TryParseAbsoluteRangeTokens(tokensSet, now, out startDateTime, out endDateTime))
            {
                return true;
            }

            // Positional handlers (by tokenIndex)

            // Between expressions like "between X and Y"
            string fullInput = string.Join(" ", tokensList); 
            
            if (TryParseBetweenExpression(fullInput, now,
                    out startDateTime, out endDateTime))
            {
                return true;
            }
           
            for (int tokenIndex = 0; tokenIndex < tokensList.Count; tokenIndex++)
            {
                // Advanced relative composite expressions like "in 2 months and 3 days",
                if (TryRelativeCompositeExpression(tokensList, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Ago expressions like "5 days ago"
                if (TryRelativeAgoExpression(tokensList, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Directional composite expressions:
                // "2 weeks and 3 days before", "after 2 weeks and 3 days"
                if (TryRelativeDirectionalCompositeExpression(tokensList, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Directional simple expressions:
                // "3 days before", "2 weeks after"
                if (TryRelativeDirectionalExpression(tokensList, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                string currentToken = tokensList[tokenIndex];

                // Explicit numeric dates like 14/03/2026, 2026‑04‑21, 14/03
                if (TryParseNumericDateToken(currentToken, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Month expressions like this month, next month, last month
                if (TryMonthExpression(tokensSet, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Ordinal dates like 3rd April
                if (TryOrdinalDate(tokensSet, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Weekday expressions like next Tuesday
                if (TryWeekdayExpression(tokensSet, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Explicit years like 2026, next year
                if (TryYearExpression(tokensSet, tokenIndex, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }

                // Absolute keywords like today, tomorrow, yesterday
                if (TryAbsoluteKeyword(currentToken, now,
                        out startDateTime, out endDateTime))
                {
                    return true;
                }
            }

            return false;
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
                            bool firstRelativeOffsetApplied = TryApplyRelativeUnit(firstQuantity,
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
                                bool secondRelativeOffsetApplied = TryApplyRelativeUnit(secondQuantity, secondCanonicalUnit,
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
                    bool firstApplied = TryApplyRelativeUnit(signedFirstQuantity, firstCanonicalUnit,
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
                        bool secondApplied = TryApplyRelativeUnit(signedSecondQuantity, secondCanonicalUnit,
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
            string[] tokensArray = tokens.ToArray();

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
                string quantityToken = tokensArray[tokenIndex];

                // Normalizes unit token
                string normalizedUnit = LangDict.NormalizeKey(tokensArray[tokenIndex + 1]);
                
                // Normalizes direction token ("ago", "hace", etc.)
                string directionToken = LangDict.NormalizeKey(tokensArray[tokenIndex + 2]);

                // Validates quantity
                bool quantityValid = TryParseNumberWord(quantityToken, out int quantityValue) ||
                    int.TryParse(quantityToken, out quantityValue);

                // Validates unit
                bool unitValid = LangDict.TimeUnitDict.TryGetValue(normalizedUnit, out string canonicalUnit);

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
                    bool offsetApplied = TryApplyRelativeUnit(signedQuantity, canonicalUnit, now, out computedStart, out computedEnd);

                    if (offsetApplied && computedStart.HasValue)
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
            if (!parseSuccessful && tokenIndex + 4 < tokens.Count)
            {
                // Normalizes first token ("il", "hace", etc.)
                string firstToken = LangDict.NormalizeKey(tokensArray[tokenIndex]);
                
                // Normalizes second token ("y", "ya", etc.)
                string secondToken = LangDict.NormalizeKey(tokensArray[tokenIndex + 1]);
                
                // Normalizes third token ("a", etc.)
                string thirdToken = LangDict.NormalizeKey(tokensArray[tokenIndex + 2]);

                // Extracts quantity token
                string quantityToken = tokensArray[tokenIndex + 3];
                
                // Normalizes unit token
                string unitToken = LangDict.NormalizeKey(tokensArray[tokenIndex + 4]);

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
                    bool offsetApplied = TryApplyRelativeUnit(signedQuantity, canonicalUnit, now,
                        out computedStart, out computedEnd);

                    if (offsetApplied && computedStart.HasValue)
                    {
                        startDateTime = computedStart;
                        endDateTime = computedStart;
                        parseSuccessful = true;
                    }
                }
            }

            // Returns final parsing status
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

            bool firstOffsetApplied = TryApplyRelativeUnit(signedQuantity1, canonicalUnit1,
                computedStart.Value, out computedStart, out computedEnd);

            if (!firstOffsetApplied || !computedStart.HasValue)
            {
                return false;
            }

            bool secondOffsetApplied = TryApplyRelativeUnit(signedQuantity2, canonicalUnit2,
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
                    bool relativeOffsetAppliedSuccessfully = TryApplyRelativeUnit(signedQuantity,
                        canonicalUnit, now, out computedStart, out computedEnd);

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

                    bool applied = TryApplyRelativeUnit(signedQuantity, canonicalUnit, now,
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
        /// Matches year‑range expressions such as:
        /// "this year", "next year", "last year"
        /// in any supported language. 
        /// </summary>
        private bool TryYearExpression(HashSet<string> tokensSet, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            // Normalizes the token (lowercase, remove accents, Unicode‑safe)
            string token = LangDict.NormalizeKey(tokensSet.ElementAt(tokenIndex));

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
        private bool TryWeekdayExpression(HashSet<string> TokensSet, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            // Normalizes token (lowercase, remove accents, Unicode‑safe)
            string token = LangDict.NormalizeKey(TokensSet.ElementAt(tokenIndex));

            // Checks if token is a weekday
            if (!LangDict.WeekdayDict.TryGetValue(token, out DayOfWeek targetDay))
            {
                return false;
            }

            // Looks ahead for next/last keywords
            string nextToken = (tokenIndex + 1 < TokensSet.Count)
                ? LangDict.NormalizeKey(TokensSet.ElementAt(tokenIndex + 1))
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

