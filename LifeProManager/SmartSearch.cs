/// <file>SmartSearch.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 4th, 2026</date>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeProManager
{
    /// <summary>
    /// SmartSearch is the natural-language search engine.
    /// It reuses the global SQLite connection.
    ///
    /// To extend SmartSearch with a new language:
    /// 1) Add ordinal suffixes in OrdinalSuffixes[]
    /// 2) Add day words in DayWords[]
    /// 3) Add before/after keywords in BeforeWords[] / AfterWords[]
    /// 4) Add month names in monthDictionary (inside ParseNaturalDates)
    ///
    /// No need to modify the core logic.
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
        /// The condition combines text, date and month filters. 
        /// All parts are joined with And so the database returns only tasks
        /// that match the text and the time constraints without including the Where keyword,
        /// which is added later in the query execution method.
        /// </summary>

        private string BuildSqlWhere(List<string> lstExpandedTokens, DateTime? startDate,
            DateTime? endDate, DateTime? detectedMonth)
        {
            // Stores all SQL fragments before joining them
            List<string> lstSqlConditions = new List<string>();

            if (lstExpandedTokens != null && lstExpandedTokens.Count > 0)
            {
                List<string> lstTokenConditions = new List<string>();

                foreach (string token in lstExpandedTokens)
                {
                    string sqlTitleCondition = "title LIKE '%" + token + "%'";
                    string sqlDescriptionCondition = "description LIKE '%" + token + "%'";

                    lstTokenConditions.Add("(" + sqlTitleCondition + " OR " + sqlDescriptionCondition + ")");
                }

                // Joins all token conditions with OR
                string sqlTokensCombined = "(" + string.Join(" OR ", lstTokenConditions) + ")";

                // Adds the token block to the global condition list
                lstSqlConditions.Add(sqlTokensCombined);
            }

            // Handles explicit date range (from ParseNaturalDates)
            if (startDate.HasValue && endDate.HasValue)
            {
                // Builds a Between condition for the deadline field
                string sqlDateRangeCondition =
                    "(deadline >= '" + startDate.Value.ToString("yyyy-MM-dd") +
                    "' AND deadline <= '" + endDate.Value.ToString("yyyy-MM-dd") + "')";

                // Adds the date range condition
                lstSqlConditions.Add(sqlDateRangeCondition);
            }

            // Handles detected month (from DetectMonth)
            if (detectedMonth.HasValue)
            {
                // Stores the first day of the detected month
                DateTime monthStart = detectedMonth.Value;

                // Stores the last day of the detected month
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Builds a BETWEEN condition for the month
                string sqlMonthCondition =
                    "(deadline >= '" + monthStart.ToString("yyyy-MM-dd") +
                    "' AND deadline <= '" + monthEnd.ToString("yyyy-MM-dd") + "')";

                // Adds the month condition
                lstSqlConditions.Add(sqlMonthCondition);
            }

            if (lstSqlConditions.Count == 0)
            {
                return string.Empty;
            }

            // Joins all conditions with AND
            string finalSqlWhere = string.Join(" AND ", lstSqlConditions);

            return finalSqlWhere;
        }

        /// <summary>
        /// Computes the Levenshtein distance between two strings.
        /// The Levenshtein distance represents the minimum number of
        /// single‑character edits (insertions, deletions, substitutions)
        /// required to transform one string into the other.
        /// </summary>
        private int CalculatesLevenshteinDistance(string source, string target)
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
        private string NormalizeQuery(string cleanedQuery)
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
        /// Parses natural-language date expressions from tokens and returns a date range.
        /// This compact version delegates each expression family to a dedicated handler:
        /// - Absolute keywords (today, tomorrow, yesterday…)
        /// - Month expressions (this month, nextToken month, last month…)
        /// - Year expressions (this year, nextToken year, last year…)
        /// - Relative expressions (in X days, X weeks before…)
        /// - Ordinal dates (1st March, 2do abril, 7eme jour…)
        /// - Explicit years (2024, 2025…)
        /// - Weekday expressions (nextToken Monday, lundi prochain…)
        /// Each handler is isolated, making the system easy to extend.
        /// </summary>
        private (DateTime? startDate, DateTime? endDate) ParseNaturalDates(List<string> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return (null, null);
            }

            DateTime now = DateTime.Now;
            DateTime? startDateTime = null;
            DateTime? endDateTime = null;

            for (int i = 0; i < tokens.Count; i++)
            {
                string currentToken = tokens[i];

                // 1) Absolute keywords (today, tomorrow, yesterday, etc.)
                if (TryAbsoluteKeyword(currentToken, now, out startDateTime, out endDateTime))
                {
                    continue;
                }

                // 2) Month expressions (this month, nextToken month…)
                if (TryMonthExpression(tokens, i, now, out startDateTime, out endDateTime))
                {
                    continue;
                }

                // 3) Year expressions (this year, nextToken year…)
                if (TryYearExpression(tokens, i, now, out startDateTime, out endDateTime))
                {
                    continue;
                }

                // 4) Relative expressions (in X days, X weeks before…)
                if (TryRelativeExpression(tokens, i, now, out startDateTime, out endDateTime))
                {
                    continue;
                }

                // 5) Ordinal dates (1st March, 2do abril, 7eme jour…)
                if (TryOrdinalDate(tokens, i, now, out startDateTime, out endDateTime))
                {
                    continue;
                }

                // 6) Explicit years (2024, 2025…)
                if (TryExplicitYear(currentToken, out startDateTime, out endDateTime))
                {
                    continue;
                }

                // 7) Weekday expressions (nextToken Monday, lundi prochain…)
                if (TryWeekdayExpression(tokens, i, now, out startDateTime, out endDateTime))
                {
                    continue;
                }
            }

            return (startDateTime, endDateTime);
        }

        /// <summary>
        /// Computes a relevance score for each task using a compact scoring model.
        /// All scoring weights are centralized and integrate exact matches, 
        /// typo‑tolerant matches, Levenshtein proximity and match density.
        /// </summary>
        private List<ScoredTask> ScoreCandidates(List<Tasks> lstCandidates,
            List<string> lstTokens, List<string> lstExpandedTokens)
        {
            // If there are no candidates to score, returns an empty list
            // to prevent null reference errors.
            if (lstCandidates == null || lstCandidates.Count == 0)
            {
                return new List<ScoredTask>();
            }

            // Defines the scoring weights for each type of match
            var scoringWeight = new Dictionary<string, int>
            {
                ["ExactMatchTitle"] = 40,
                ["ExactMatchDescription"] = 25,
                ["ExpandedMatchTitle"] = 12,
                ["ExpandedMatchDescription"] = 8,
                ["LevenshteinDistance1"] = 6,
                ["LevenshteinDistance2"] = 3,
                ["ExactMatchDensity"] = 4
            };

            List<ScoredTask> lstScoredTasks = new List<ScoredTask>();

            foreach (Tasks task in lstCandidates)
            {
                // Normalizes the title and description of the task 
                string normalizedTitle = NormalizeQuery(CleanQuery(task.Title ?? string.Empty));

                // Normalizes the description of the task
                string normalizedDescription = NormalizeQuery(CleanQuery(task.Description ?? string.Empty));

                // Combines all words from the title and description into a single list for density calculation.
                var allWords = normalizedTitle.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Concat(normalizedDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                              .ToList();

                // Starts with a density count of 0, which will be incremented
                // for each exact match found in the title or description.
                int densityCount = 0;

                // Calculates the score for this task by summing the contributions of each expanded token.
                int totalScore = lstExpandedTokens.Sum(currentToken =>
                {
                    // Determines if the current token is an exact match
                    // (present in the original token list) or an expanded match
                    // (only in the expanded list).
                    bool isExactMatching = lstTokens.Contains(currentToken);

                    // Calculates the score contribution of this token based on its presence
                    // in the title and description,
                    int score =
                        (normalizedTitle.Contains(currentToken) ? scoringWeight[isExactMatching ?
                        "ExactMatchTitle" : "ExpandedMatchTitle"] : 0) +
                        (normalizedDescription.Contains(currentToken) ? scoringWeight[isExactMatching ?
                        "ExactMatchDescription" : "ExpandedMatchDescription"] : 0);

                    // If this token is an exact match and is found in either the title or description,
                    // it contributes to the density count.
                    if (isExactMatching && (normalizedTitle.Contains(currentToken) ||
                    normalizedDescription.Contains(currentToken)))
                    {
                        densityCount++;
                    }

                    // If this token is an exact match, we also check for near matches
                    // using Levenshtein distance to capture typos.
                    if (isExactMatching)
                    {
                        int bestDistance = allWords.Select(word => CalculatesLevenshteinDistance(currentToken, word))
                                                   .DefaultIfEmpty(int.MaxValue)
                                                   .Min();

                        // Adds a small score for near matches with a distance of 1 or 2,
                        // which indicates a likely typo.
                        if (bestDistance == 1)
                        {
                            score += scoringWeight["LevenshteinDistance1"];
                        }

                        else if (bestDistance == 2)
                        {
                            score += scoringWeight["LevenshteinDistance2"];
                        }
                    }

                    return score;
                });

                // Adds a score contribution based on the density of exact matches in the task.
                totalScore += densityCount * scoringWeight["ExactMatchDensity"];

                // Adds a new ScoredTask object to the list with the computed total score for this task.
                lstScoredTasks.Add(new ScoredTask
                {
                    Task = task,
                    Score = totalScore
                });
            }

            return lstScoredTasks;
        }

        /// <summary>
        /// Runs the full natural-language search pipeline:
        /// - Preprocesses the query (clean, normalize, tokenize, typo expansion)
        /// - Extracts optional date filters from tokens
        /// - Builds the SQL WHERE clause
        /// - Retrieves matching tasks from the database
        /// - Scores and sorts them by relevance
        /// </summary>
        public List<Tasks> Search(string rawQuery)
        {
            // Cleans the raw query (remove punctuation, extra spaces, etc.)
            string cleanedQuery = CleanQuery(rawQuery);

            // Normalizes the cleaned query (lowercase, remove accents, etc.)
            string normalizedQuery = NormalizeQuery(cleanedQuery);

            // Tokenizes the normalized query into individual words/tokens
            List<string> tokens = TokenizeQuery(normalizedQuery);

            // Expands tokens with Levenshtein distance to account for typos
            tokens = ExpandTokensLevenshtein(tokens);

            // Token expansion with Levenshtein
            List<string> expandedTokens = ExpandTokensLevenshtein(tokens);

            // Natural date parsing (“tomorrow”, “next week”, “1st March”)
            (DateTime? detectedStartDate, DateTime? detectedEndDate) = ParseNaturalDates(tokens);

            // Month detection (for queries like "in March", "en mars", "en marzo")
            DateTime? detectedMonth = DetectMonth(tokens);

            // Builds the Sql Where clause based on the tokens and detected date filters
            string sqlWhereClause = BuildSqlWhere(tokens, detectedStartDate, detectedEndDate, detectedMonth);

            // Retrieves candidate tasks from the database matching the Sql Where clause
            List<Tasks> dbResults = dbConn.SearchTasks(sqlWhereClause);

            // Scores and sort results by relevance
            List<ScoredTask> scoredResults = ScoreCandidates(dbResults, tokens, expandedTokens);

            // Returns only the tasks, sorted by relevance
            return scoredResults.Select(score => score.Task).ToList();
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
        /// Tries to match month expressions (this month, nextToken month, last month) in multiple languages.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="i"></param>
        /// <param name="now"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        private bool TryMonthExpression(List<string> tokens, int tokenIndex, DateTime now, out DateTime? startDateTime, out DateTime? endDateTime)
        {
            startDateTime = endDateTime = null;
            string currentToken = tokens[tokenIndex];
            string previousToken = tokenIndex > 0 ? tokens[tokenIndex - 1] : "";
            string nextToken = tokenIndex + 1 < tokens.Count ? tokens[tokenIndex + 1] : "";

            // this month in multiple language
            if ((currentToken == "mois" && previousToken == "ce") || (currentToken == "month" && previousToken == "this") || (currentToken == "mes" && previousToken == "este"))
            {
                startDateTime = new DateTime(now.Year, now.Month, 1);
                endDateTime = startDateTime.Value.AddMonths(1).AddDays(-1);
                return true;
            }

            // nextToken month in multiple language
            if ((currentToken == "mois" && nextToken == "suivant") || (currentToken == "nextToken" && nextToken == "month") || (currentToken == "mes" && nextToken == "siguiente"))
            {
                startDateTime = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                endDateTime = startDateTime.Value.AddMonths(1).AddDays(-1);
                return true;
            }

            // last month in multiple language
            if ((currentToken == "mois" && (nextToken is "passé" || nextToken is "dernier")) || (currentToken == "last" && nextToken == "month") || (currentToken == "mes" && nextToken == "pasado"))
            {
                startDateTime = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
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

            if (!TryParseOrdinalDay(tokens[tokenIndex], out int dayNumber, out bool hasOrdinalSuffix))
            {
                return false;
            }

            // Case 1 — "7eme jour" / "3rd day" / "2do dia"
            if (tokenIndex + 1 < tokens.Count &&
                (tokens[tokenIndex + 1] == "jour" ||
                 tokens[tokenIndex + 1] == "day" ||
                 tokens[tokenIndex + 1] == "dia"))
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

                // Explicit date chosen from ordinal day + month name
                DateTime explicitDateChosen = new DateTime(now.Year, monthNumber, dayNumber);

                startDateTime = explicitDateChosen;
                endDateTime = explicitDateChosen;
                return true;
            }

            // Case 3 — "7eme" / "3rd" / "2do" alone → interpret as day of current month
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
        /// Attempts to parse an ordinal day token in a fully language‑agnostic way.
        /// The method does not assume any alphabet: suffixes may come from any Unicode script.
        /// 
        /// How it works:
        /// - If the token is purely numeric ("1", "2", "15"), it is accepted.
        /// - Otherwise, the method checks whether the token ends with any known ordinal suffix.
        /// - If a suffix matches, the suffix is removed and the remaining part is parsed as a number.
        /// 
        /// Extensible:
        /// Simply add new suffixes to the list (e.g., Japanese "日").
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
                if (!int.TryParse(quantityToken, out int quantity))
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
            if (int.TryParse(tokens[tokenIndex], out int relativeQuantity) &&
                tokenIndex + 2 < tokens.Count)
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

