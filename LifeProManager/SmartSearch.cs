/// <file>SmartSearch.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 24th, 2026</date>

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
        /// <param name="ExpandedTokensSet">Set of normalized and typo‑expanded tokens.</param>
        /// <param name="startDate">Start date constraint if detected.</param>
        /// <param name="endDate">End date constraint if detected.</param>
        /// <param name="detectedMonth">Month filter if a month name was detected.</param>
        /// <param name="priorityCategory">Semantic priority category if detected.</param>
        /// <param name="onlyTemporalTokens">True if the query is purely temporal.</param>
        /// <param name="lstSqliteParameters">Output list of generated SQLite parameters.</param>
        /// <returns>Sql Where clause combining all applicable filters.</returns>
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
        /// <returns>The Levenshtein edit distance between the two strings.</returns>
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
        /// Determines whether the query should be interpreted as a pure temporal search.
        /// A pure temporal search is something like:
        /// "next week", "last month", "in 3 days", "tomorrow", "yesterday", "2026".
        /// A standalone weekday ("lunes", "monday", "jeudi") is treated as an 
        /// hybrid search (lexical and temporal).
        /// </summary>
        private bool ContainsOnlyTemporalTokens(HashSet<string> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return false;
            }

            // Standalone weekday activates temporal mode.
            // Outlook-style behaviour: a query like "lunes" is interpreted primarily
            // as a temporal request (return Monday tasks first), but lexical scoring
            // still applies afterwards so tasks containing the word "lunes" also appear.
            if (tokens.Count == 1 && LangDict.WeekdayDict.ContainsKey(tokens.First()))
            {
                return true;
            }

            // For multi-token queries, we only consider it pure temporal if
            // every token belongs to the temporal domain and at least one token
            // is a true temporal operator (next, last, in, ago, etc.)
            bool containsTemporalOperator = false;

            foreach (var token in tokens)
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
                {
                    return false;
                }

                // Detects if the query contains a true temporal operator
                if (LangDict.RelativeDirectionDict.ContainsKey(token) ||
                    LangDict.RelativePrepositionSet.Contains(token) ||
                    LangDict.TimeDirectionDict.ContainsKey(token) ||
                    LangDict.TimeAgoKeywordSet.Contains(token))
                {
                    containsTemporalOperator = true;
                }
            }

            // Pure temporal mode only if all tokens are temporal and at least one token is a temporal operator
            return containsTemporalOperator;
        }

        /// <summary>
        /// Detects whether the user query contains a month name in all supported languages
        /// and returns the first day of the detected month, or null if none is found.
        /// </summary>
        /// <param name="TokensSet">Set of normalized tokens extracted from the user query.</param>
        /// <returns>The first day of the detected month, or null if none is found.</returns>
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
        /// <param name="normalizedTokens">Set of normalized tokens extracted from the query.</param>
        /// <returns>The detected priority category, or null if none matches.</returns>

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
        /// <returns>A new set including typo‑tolerant variants.</returns>

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
        /// <param name="token">Single token for which typo‑tolerant variants are generated.</param>
        /// <returns>List of generated character‑level variants.</returns>

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
        /// Cleans and normalizes the raw user query:
        /// - trims and collapses whitespace
        /// - removes punctuation and separators
        /// - protects logical operators (AND / OR)
        /// - converts to lowercase
        /// - removes accents and diacritics
        /// </summary>
        /// <param name="rawQuery">Raw user query</param>
        /// <returns>Lowercased, accent‑stripped version of the query.</returns>
        private string NormalizeQuery(string rawQuery)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
            {
                return string.Empty;
            }

            // Basic cleaning

            // Trims leading/trailing spaces
            rawQuery = rawQuery.Trim();

            // Collapses multiple spaces
            rawQuery = Regex.Replace(rawQuery, @"\s+", " ");

            // Normalizes separators
            rawQuery = rawQuery.Replace(",", " ").Replace(";", " ").Replace("/", " ");

            // Removes punctuation that has no semantic meaning
            rawQuery = Regex.Replace(rawQuery, @"[!?:()\[\]{}""'’]", "");

            // Protects logical operators by isolating them
            rawQuery = Regex.Replace(rawQuery, @"\bAND\b", " AND ", RegexOptions.IgnoreCase);
            rawQuery = Regex.Replace(rawQuery, @"\bOR\b", " OR ", RegexOptions.IgnoreCase);

            // Collapses spaces again after replacements
            rawQuery = Regex.Replace(rawQuery, @"\s+", " ");

            // Normalization

            // Lowercase
            rawQuery = rawQuery.ToLowerInvariant();

            // Decomposes accents (FormD)
            rawQuery = rawQuery.Normalize(NormalizationForm.FormD);

            // Removes diacritics
            var stringBuilder = new StringBuilder();
            
            foreach (char rawQueryChar in rawQuery)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(rawQueryChar) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(rawQueryChar);
                }
            }

            // Recomposes (FormC)
            rawQuery = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            return rawQuery.Trim();
        }

        /// <summary>
        /// Creates a dummy task indicating that the search crashed.
        /// This ensures the UI always receives a valid, non-empty result list.
        /// </summary>
        /// <returns>A fallback list containing a single crash‑indicator task.</returns>
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
        /// <param name="titleTokens">Normalized tokens extracted from the task title.</param>
        /// <param name="description">Full task description text to analyze.</param>
        /// <param name="now">Reference date used as the anchor for all parsing operations.</param>
        /// <returns>A tuple containing the resolved start and end dates.</returns>
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
        /// <param name="candidateTasks">List of tasks pre‑filtered by lexical or temporal mode.</param>
        /// <param name="tokens">Normalized tokens extracted from the query.</param>
        /// <param name="expandedTokens">Token set including typo‑tolerant variants.</param>
        /// <returns>List of scored tasks sorted by descending relevance.</returns>
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
                string taskTitle = NormalizeQuery(task.Title ?? "");
                string taskDescription = NormalizeQuery(task.Description ?? "");

                // Builds a rich word list for Levenshtein comparisons
                // Includes: split words, concatenated forms, and camelCase splits.
                // This ensures fuzzy tokens can match reliably.

                // Splits by whitespace
                var titleWords = taskTitle.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var descWords = taskDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Adds concatenated forms (e.g., "clean kitchen" → "cleankitchen")
                string titleConcat = taskTitle.Replace(" ", "");
                string descConcat = taskDescription.Replace(" ", "");

                // Add camelCase splits (e.g., "CleanKitchen" → "Clean Kitchen")
                string titleCamel = Regex.Replace(taskTitle, "([a-z])([A-Z])", "$1 $2");
                string descCamel = Regex.Replace(taskDescription, "([a-z])([A-Z])", "$1 $2");

                var camelWords = titleCamel.Split(' ').Concat(descCamel.Split(' '))
                    .Where(w => !string.IsNullOrWhiteSpace(w));

                // Final allWords list
                List<string> allWords = titleWords
                    .Concat(descWords)
                    .Concat(camelWords)
                    .Concat(new[] { titleConcat, descConcat })
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int exactMatchDensity = 0;          // Counts how many original tokens have an exact match in title/description
                bool tokensRespectOrder = true;     // Checks if tokens appear in the same order as the query
                int lastTokenIndex = -1;            // Tracks the index of the last matched token to verify order

                // Token scoring
                int totalScore = 0;

                foreach (string expandedToken in expandedTokens)
                {
                    bool isExactToken = tokens.Contains(expandedToken);
                    int tokenScore = 0;

                    int posTitle = taskTitle.IndexOf(expandedToken, StringComparison.Ordinal);
                    int posDesc = taskDescription.IndexOf(expandedToken, StringComparison.Ordinal);

                    if (posTitle >= 0)
                    {
                        // Exact or expanded match in title
                        tokenScore += scoringWeight[isExactToken ? "ExactTitle" : "ExpandedTitle"];
                        
                        // Strong boost for early matches in the title:
                        // The earlier the token appears (posTitle close to 0), the larger the bonus.
                        // As the position increases, the bonus decreases smoothly.
                        // This makes title‑prefix matches rank higher, similar to Outlook.
                        tokenScore += (int)(20.0 / (1 + posTitle));

                        // Exact-match dominance
                        if (isExactToken)
                        {
                            tokenScore += 15;
                        }
                    }
                    
                    else if (posDesc >= 0)
                    {
                        // Exact or expanded match in description
                        tokenScore += scoringWeight[isExactToken ? "ExactDescription" : "ExpandedDescription"];

                        // Smaller boost for early matches in description
                        tokenScore += (int)(10.0 / (1 + posDesc));
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

                        // Adaptive Levenshtein tolerance
                        // Short tokens (<4 chars) only allow distance 1.
                        // Longer tokens allow distance 1 or 2.
                        int allowedDistance = expandedToken.Length < 4 ? 1 : 2;

                        if (bestDistance <= allowedDistance)
                        {
                            string levelKey = bestDistance == 1 ? "Lev1" : "Lev2";
                            tokenScore += scoringWeight[levelKey];
                        }
                    }

                    // Token order detection
                    int indexInTitle = taskTitle.IndexOf(expandedToken);
                    int indexInDesc = taskDescription.IndexOf(expandedToken);
                    int tokenIndex = (indexInTitle >= 0 ? indexInTitle : indexInDesc);

                    if (tokenIndex >= 0)
                    {
                        if (lastTokenIndex > tokenIndex)
                        {
                            tokensRespectOrder = false;
                        }

                        lastTokenIndex = tokenIndex;
                    }

                    totalScore += tokenScore;
                }

                // Density bonus
                totalScore += exactMatchDensity * scoringWeight["Density"];

                // FullCoverage even if token order is reversed
                // We check whether all query tokens appear somewhere in the task title or description.
                // tokens.All(...) means: "for every token in the query, the condition must be true".
                if (tokens.All(token =>

                        // Checks if the token appears anywhere in the title, ignoring uppercase/lowercase.
                        // IndexOf(...) returns -1 when the token is NOT found, so >= 0 means "found".
                        taskTitle.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0 ||

                        // Same check for the task description.
                        // If the token is found in either title or description, the condition is satisfied.
                        taskDescription.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0
                    ))
                {
                    // If all tokens were found, we add the FullCoverage bonus.
                    totalScore += scoringWeight["FullCoverage"];
                }

                // Token order bonus
                if (tokensRespectOrder)
                {
                    totalScore += scoringWeight["TokenOrder"];
                }

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
        /// <param name="rawQuery">Raw user query before normalization and tokenization.</param>
        /// <returns>List of tasks matching the unified smart search pipeline.</returns>
        public List<Tasks> Search(string rawQuery)
        {
            try
            {
                // Cleans and normalizes
                string normalizedQuery = NormalizeQuery(rawQuery);

                // Tokenization
                HashSet<string> NormalizedTokensSet = TokenizeQuery(normalizedQuery);

                // Explicit "show all" commands ("*", "all", "todos", "toutes")
                if (NormalizedTokensSet.Count == 1 && LangDict.ShowAllKeywords.Contains(NormalizedTokensSet.First()))
                {
                    return dbConn.ReadTask("");
                }

                // Removes short or non-informative tokens to reduce lexical false positives
                NormalizedTokensSet = NormalizedTokensSet
                    .Where(token => token.Length >= 3)   // ignores tokens of length 1–2
                    .ToHashSet();

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

                // Detects standalone year (e.g., "2026", "2025 tax", "year 2027")
                int parsedYear;

                foreach (string token in NormalizedTokensSet)
                {
                    if (token.Length == 4 && int.TryParse(token, out parsedYear))
                    {
                        // Builds a full-year interval
                        startDate = new DateTime(parsedYear, 1, 1);
                        endDate = new DateTime(parsedYear, 12, 31);
                        break;
                    }
                }

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
                List<Tasks> lstCandidateTasks = dbConn.SearchTasks(sqlWhere, sqlParams);

                // Strict temporal filtering before scoring
                if (startDate.HasValue && endDate.HasValue)
                {
                    lstCandidateTasks = lstCandidateTasks
                        .Where(task =>
                        {
                            if (string.IsNullOrWhiteSpace(task.Deadline))
                            {
                                return false;
                            }

                            if (!DateTime.TryParse(task.Deadline, out DateTime deadline))
                            {
                                return false;
                            }

                            // Strict interval: task must fall fully inside the parsed range
                            return deadline.Date >= startDate.Value.Date && deadline.Date <= endDate.Value.Date;
                        })
                        .ToList();
                }

                // Unified scoring (lexical and temporal signals)
                List<ScoredTask> lstScoredTasks = ScoreCandidates(lstCandidateTasks, NormalizedTokensSet, ExpandedTokensSet);

                // Strict filtering of time intervals
                if (startDate.HasValue && endDate.HasValue)
                {
                    lstScoredTasks = lstScoredTasks
                        .Where(s =>
                        {
                            // Converts deadline string to DateTime
                            if (!DateTime.TryParse(s.Task.Deadline, out DateTime deadline))
                            {
                                return false;
                            }

                            // Keeps only tasks whose deadline falls strictly inside the parsed time interval
                            return deadline >= startDate.Value && deadline <= endDate.Value;
                        })
                        .ToList();
                }

                // Final output depending on the mode

                // Pure temporal mode
                if (onlyTemporalTokens)
                {
                    // Ignores score and sorts by date
                    return lstScoredTasks
                        .Select(s => s.Task)
                        .OrderBy(t => t.Deadline)
                        .ToList();
                }

                // Lexical mode
                else
                {
                    // Applies threshold(score > 0) and sorts by relevance
                    return lstScoredTasks
                        .Where(s => s.Score > 0)
                        .OrderByDescending(s => s.Score)
                        .Select(s => s.Task)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Always return a safe fallback result
                Console.WriteLine($"[SmartSearch Crash] {ex}");
                return NotifyCrashResult();
            }
        }

        /// <summary>
        /// Splits the user query into normalized tokens.
        /// Removes punctuation, trims spaces, lowercases everything.
        /// </summary>
        /// <param name="query">Raw user query to split into normalized tokens.</param>
        /// <returns>Set of normalized tokens extracted from the query.</returns>
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
        /// Tries to match absolute date keywords (today, tomorrow, yesterday)
        /// in supported languages.
        /// </summary>
        /// <param name="token">Single normalized token to evaluate.</param>
        /// <param name="now">Reference date used as the anchor.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if the token matches an absolute keyword.</returns>
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
        /// </summary>
        /// <param name="offset">Positive or negative quantity representing the offset to apply.</param>
        /// <param name="normalizedUnit">Normalized time unit</param>
        /// <param name="now">Reference date used as the origin for the calculation.</param>
        /// <param name="startDateTime">Output: the computed start date of the resulting interval.</param>
        /// <param name="endDateTime">Output: the computed end date of the resulting interval.</param>
        /// <returns> True if the unit is supported and the offset was applied; otherwise false.</returns>

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
        /// Matches month‑range expressions such as:
        /// "this month", "next month", "last month"
        /// in any supported language.
        /// </summary>
        /// <param name="TokensSet">Set of normalized tokens extracted from the query.</param>
        /// <param name="tokenIndex">Current token index used as the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if a valid month‑range expression is detected.</returns>
        private bool TryMonthExpression(HashSet<string> TokensSet, int tokenIndex, DateTime now,
        out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            string rangeType = null;

            // Scans all tokens
            foreach (string rawToken in TokensSet)
            {
                string normalizedToken = LangDict.NormalizeKey(rawToken);

                if (LangDict.MonthRangeDict.TryGetValue(normalizedToken, out rangeType))
                {
                    break;
                }
            }

            if (rangeType == null)
            {
                return false;
            }

            int offset;

            switch (rangeType)
            {
                case "this":
                    offset = 0;
                    break;

                case "next":
                    offset = +1;
                    break;

                case "last":
                    offset = -1;
                    break;

                default:
                    offset = 0;
                    break;
            }

            int currentYear = now.Year;
            int monthWithOffsetApplied = now.Month + offset;

            while (monthWithOffsetApplied < 1) 
            { 
                monthWithOffsetApplied += 12; currentYear--; 
            }

            while (monthWithOffsetApplied > 12) 
            { 
                monthWithOffsetApplied -= 12; currentYear++; 
            }

            startDateTime = new DateTime(currentYear, monthWithOffsetApplied, 1);
            endDateTime = new DateTime(currentYear, monthWithOffsetApplied, DateTime.DaysInMonth(currentYear, monthWithOffsetApplied));

            return true;
        }

        /// <summary>
        /// Handles ordinal day expressions combined with day or month:
        /// - "7eme jour", "3rd day", "2do dia"
        /// - "7eme mars", "3rd april", "2do abril"
        /// - "7eme", "3rd", "2do" alone → day of current month
        /// Uses TryParseOrdinalDay() and monthDictionary.
        /// </summary>
        /// <param name="TokensSet">Set of normalized tokens extracted from the query.</param>
        /// <param name="tokenIndex">Current token index used as the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if a valid ordinal‑day expression is detected.</returns>
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
        /// Attempts to parse an absolute date range from a list of tokens.
        /// Supports formulations such as: "from 3 to 7", "3 to 7", "3-7",
        /// "period from 3 to 7".
        /// </summary>
        /// <param name="tokensSet">Set of normalized tokens extracted from the query.</param>
        /// <param name="now">Reference date used as the anchor for range construction.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date if parsing succeeds.</param>
        /// <returns>True if a valid absolute numeric range is detected.</returns>
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
        /// Attempts to parse a directional "between X and Y" date range in a language‑agnostic way.
        /// The method relies on user‑editable keyword lists (lstBetweenKeywords, lstAndKeywords).
        /// Both X and Y are parsed using existing date handlers.
        /// Returns true only if both sides produce valid dates.
        /// </summary>
        /// <param name="input">Full normalized input string reconstructed from tokens.</param>
        /// <param name="now">Reference date used as the anchor for both parsed sides.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date if parsing succeeds.</param>
        /// <returns>True if a valid “between X and Y” date range is detected.</returns>
        public bool TryParseBetweenExpression(string input, DateTime now, out DateTime? startDate,
        out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // Normalizes for keyword matching
            string normalizedInput = input.Trim().ToLowerInvariant();

            // Scans all "between" keywords (multi‑language)
            foreach (var betweenEntry in LangDict.lstBetweenKeywords)
            {
                string betweenKeyword = betweenEntry.value;
                string prefix = betweenKeyword + " ";

                // Input must start with the "between" keyword
                if (!normalizedInput.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Finds the matching "and" keyword for the same language
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

                // Locates the "and" separator
                string separator = " " + andKeyword + " ";
                int separatorIndex = normalizedInput.IndexOf(separator, StringComparison.OrdinalIgnoreCase);

                // Must appear after the "between" prefix
                if (separatorIndex <= prefix.Length)
                {
                    continue;
                }

                // Extracts left and right segments
                string leftSegment = normalizedInput.Substring(prefix.Length, separatorIndex - prefix.Length).Trim();
                string rightSegment = normalizedInput.Substring(separatorIndex + separator.Length).Trim();

                // Parses both sides using the existing date segment parser
                if (TryParseDateSegment(leftSegment, now, out DateTime? leftStart, out DateTime? leftEnd) &&
                    TryParseDateSegment(rightSegment, now, out DateTime? rightStart, out DateTime? rightEnd))
                {
                    // Uses resolved dates (start or end depending on handler)
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
        /// <param name="dateSegment">Raw substring representing a potential date expression.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDate">Resolved start date if parsing succeeds.</param>
        /// <param name="endDate">Resolved end date if parsing succeeds.</param>
        /// <returns>True if the segment contains a valid date expression.</returns>
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

        /// <summary>
        /// Detects explicit 4‑digit year tokens.
        /// If a token is exactly four digits and represents a valid year,
        /// the method returns the full January‑to‑December interval for that year.
        /// </summary>
        /// <param name="tokensSet">Set of normalized tokens extracted from the query.</param>
        /// <param name="now">Reference date used as the anchor for all parsing operations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date if parsing succeeds.</param>
        /// <returns>True if a valid 4‑digit year token is detected.</returns>
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
            string fullTokenInput = string.Join(" ", tokensList); 
            
            if (TryParseBetweenExpression(fullTokenInput, now,
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

                // Directional composite expressions like
                // "2 weeks and 3 days before" and "after 2 weeks and 3 days"
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

            // Explicit 4‑digit year fallback (e.g., "2026").
            // Runs only after all other parsers: avoids overriding "next year",
            // numeric dates like "14/03/2026", etc. Order doesn't matter here,
            // so we scan tokensSet and pick any standalone 4‑digit year.
            foreach (string rawToken in tokensSet)
            {
                string normalizedToken = LangDict.NormalizeKey(rawToken);

                if (normalizedToken.Length == 4 && int.TryParse(normalizedToken, out int explicitYear))
                {
                    startDateTime = new DateTime(explicitYear, 1, 1);
                    endDateTime = new DateTime(explicitYear, 12, 31);
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
        /// <param name="parsedResult">The parsed number</param>
        /// <returns>True if the word represents a valid number in natural language.</returns>
        public static bool TryParseNumberWord(string inputWord, out int parsedResult)
        {
            parsedResult = 0;

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
            parsedResult = finalValue;
            return true;
        }

        /// <summary>
        /// Attempts to parse a single token as an explicit numeric date.
        /// Supported formats:
        ///   - dd/MM/yyyy, dd‑MM‑yyyy
        ///   - yyyy/MM/dd, yyyy‑MM‑dd
        ///   - dd/MM, dd‑MM  (year defaults to the current year)
        ///
        /// This handler is lightweight and fast by using regex matching
        /// instead of culture‑dependent parsing.
        /// It is designed to be called inside the unified date‑token pipeline
        /// (TryParseDateTokens), so it only handles numeric formats.
        /// All textual month/day formats are handled by the other TryXXX handlers.
        /// </summary>
        /// <param name="token">Single token to evaluate as a numeric date.</param>
        /// <param name="now">Reference date used when the year is omitted.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if the token represents a valid numeric date.</returns>
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
        /// - Otherwise, the method checks whether the token ends with any known ordinal suffix.
        /// - If a suffix matches, the suffix is removed and the remaining part is parsed as a number.
        /// </summary>
        /// <param name="token">Raw token potentially containing an ordinal day.</param>
        /// <param name="dayNumber">Extracted day number if parsing succeeds.</param>
        /// <param name="hasOrdinalSuffix">True if the token contained a recognized ordinal suffix.</param>
        /// <returns>True if the token contains a valid ordinal day.</returns>
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
        /// or reversed directional forms like "2 weeks and 3 days before".
        ///
        /// This handler detects multi‑unit relative structures composed of:
        /// - a first quantity + time unit (e.g., "2 months")
        /// - a connector ("and", "et", "y")
        /// - a second quantity + time unit (e.g., "3 days")
        /// - an optional directional keyword ("before", "after", "avant", "después")
        ///
        /// The method applies both relative offsets in sequence, starting from 'now'.
        /// If a direction is present, both quantities are signed accordingly.
        /// The result is a single resolved date (start = end).
        /// </summary>
        /// <param name="tokens">Normalized token list extracted from the query.</param>
        /// <param name="tokenIndex">Current token index used as the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if a valid composite relative expression is detected.</returns>
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
            
            // Ensures enough tokens for this pattern
            if (tokenIndex + 5 < tokens.Count)
            {
                // Normalizes the starting keyword ("in", "within", etc.)
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
        /// "3 days ago", "2 weeks ago", "il y a 5 jours", "hace 3 semanas",
        /// or any equivalent structure supported by LangDict.
        ///
        /// This handler detects:
        /// - a quantity (digit or number word)
        /// - a time unit ("day", "week", "month", "year", etc.)
        /// - a backward‑direction keyword ("ago", "il y a", "hace", ...)
        ///
        /// The method applies the negative offset to the reference date 'now'
        /// and returns a single resolved date (start = end).
        /// </summary>
        /// <param name="tokens">Normalized token list extracted from the query.</param>
        /// <param name="tokenIndex">Current token index used as the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if a valid “ago” relative expression is detected.</returns>
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
        /// Parses composite directional relative expressions such as:
        /// "after 2 weeks and 3 days", "2 months and 5 days before",
        /// or any equivalent multi‑unit structure supported by LangDict.
        ///
        /// This handler detects:
        /// - a first quantity + time unit (e.g., "2 weeks")
        /// - a connector ("and", "et", "y")
        /// - a second quantity + time unit (e.g., "3 days")
        /// - a final directional keyword ("before", "after", "avant", "después")
        ///
        /// The method applies both relative offsets in sequence, starting from 'now',
        /// and returns a single resolved date (start = end).
        /// </summary>
        /// <param name="tokens">Normalized token list extracted from the query.</param>
        /// <param name="tokenIndex">Current token index used as the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date (same as start for this handler).</param>
        /// <returns>True if a valid directional composite relative expression is detected.</returns>
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
        /// <param name="tokens">Ordered list of normalized tokens.</param>
        /// <param name="tokenIndex">Index of the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date if parsing succeeds.</param>
        /// <returns>True if a valid directional relative expression is detected.</returns>
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

            // Pattern: direction + "de" + quantity + unit (Spanish)
            // Examples: "antes de 3 dias", "despues de 2 semanas"
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
        /// <param name="tokensSet">Set of normalized tokens extracted from the query.</param>
        /// <param name="tokenIndex">Index of the potential start of the pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date if parsing succeeds.</param>
        /// <returns>True if a valid year‑range expression is detected.</returns>
        private bool TryYearExpression(HashSet<string> tokensSet, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;

            string rangeType = null;

            // Scans all tokens
            foreach (string rawToken in tokensSet)
            {
                string normalizedToken = LangDict.NormalizeKey(rawToken);

                if (LangDict.YearRangeDict.TryGetValue(normalizedToken, out rangeType))
                {
                    break;
                }
            }

            // No year‑range keyword found
            if (rangeType == null)
            {
                return false;
            }

            int targetYear;

            // Determines the target year based on the range type
            switch (rangeType)
            {
                case "this":
                    targetYear = now.Year;
                    break;

                case "next":
                    targetYear = now.Year + 1;
                    break;

                case "last":
                    targetYear = now.Year - 1;
                    break;

                default:
                    targetYear = now.Year;
                    break;
            }

            // Builds the full year interval
            startDateTime = new DateTime(targetYear, 1, 1);
            endDateTime = new DateTime(targetYear, 12, 31);

            return true;
        }

        /// <summary>
        /// Matches weekday expressions such as:
        /// "next monday", "last friday", "lundi prochain", "viernes pasado"
        /// in any supported language.
        /// </summary>
        /// <param name="tokensSet">Set of normalized tokens extracted from the query.</param>
        /// <param name="tokenIndex">Index of the potential start of the weekday pattern.</param>
        /// <param name="now">Reference date used as the anchor for relative calculations.</param>
        /// <param name="startDateTime">Resolved start date if parsing succeeds.</param>
        /// <param name="endDateTime">Resolved end date if parsing succeeds.</param>
        ///  <returns>True if a valid weekday expression is detected.</returns>
        private bool TryWeekdayExpression(HashSet<string> tokensSet, int tokenIndex, DateTime now,
            out DateTime? startDateTime, out DateTime? endDateTime)
        {

            startDateTime = endDateTime = null;

            DayOfWeek? parsedWeekday = null;
            string detectedDirection = null;

            // Scans all tokens to find a weekday
            foreach (string rawToken in tokensSet)
            {
                string normalizedToken = LangDict.NormalizeKey(rawToken);

                if (LangDict.WeekdayDict.TryGetValue(normalizedToken, out DayOfWeek weekday))
                {
                    parsedWeekday = weekday;
                    break;
                }
            }

            // No weekday found: not a match
            if (!parsedWeekday.HasValue)
            {
                return false;
            }

            // Scans all tokens again to find "next" / "last" direction
            foreach (string rawToken in tokensSet)
            {
                string normalizedToken = LangDict.NormalizeKey(rawToken);

                if (LangDict.NextWeekdayKeywordSet.Contains(normalizedToken))
                {
                    detectedDirection = "next";
                    break;
                }

                if (LangDict.PreviousWeekdayKeywordSet.Contains(normalizedToken))
                {
                    detectedDirection = "last";
                    break;
                }
            }

            // Computes delta from current weekday
            int currentDay = (int)now.DayOfWeek;
            int targetDay = (int)parsedWeekday.Value;

            int deltaDays = targetDay - currentDay;

            // Applies direction if present
            if (detectedDirection == "next")
            {
                if (deltaDays <= 0)
                {
                    deltaDays += 7;
                }
            }
            else if (detectedDirection == "last")
            {
                if (deltaDays >= 0)
                {
                    deltaDays -= 7;
                }
            }

            DateTime result = now.Date.AddDays(deltaDays);

            startDateTime = result;
            endDateTime = result;

            return true;
        }
    }
}

