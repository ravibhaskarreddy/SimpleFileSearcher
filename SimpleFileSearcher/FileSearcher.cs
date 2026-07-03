using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFileSearcher
{
    // Result object for a file that matched the search
    public class SearchResult
    {
        public string Path { get; set; }
        public string Preview { get; set; }
        public int MatchCount { get; set; }
    }

    public class FileSearcher
    {
        /// <summary>
        /// Searches for files within given date range and matching file types, then checks for text matches.
        /// </summary>
        public static ConcurrentBag<SearchResult> SearchFiles(
            string folderPath, string searchText, string[] fileTypes,
            DateTime from, DateTime to, Func<bool> isCancelled)
        {
            var results = new ConcurrentBag<SearchResult>();

            try
            {
                Console.WriteLine($"Searching from: {from} to {to}");

                // Filter all files in folder and subfolders based on file type and last modified date
                var allFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                    .Where(f =>
                    {
                        if (fileTypes.Length == 0 || fileTypes.Contains("*.*"))
                            return true;

                        string lowerFile = f.ToLowerInvariant();
                        return fileTypes.Any(ext =>
                        {
                            string cleanExt = ext.Trim().ToLowerInvariant().TrimStart('*');
                            return lowerFile.EndsWith(cleanExt);
                        });
                    })
                    .Where(f =>
                    {
                        // Use UTC → LocalTime for consistent filtering across shares
                        DateTime modified = File.GetLastWriteTimeUtc(f).ToLocalTime();
                        Console.WriteLine($"[{Path.GetFileName(f)}] Modified: {modified}, From: {from}, To: {to}");
                        return modified >= from && modified <= to;
                    });

                // Parallel file reading
                Parallel.ForEach(allFiles, (file, state) =>
                {
                    if (isCancelled()) return;

                    Console.WriteLine("Checking file: " + file);

                    try
                    {
                        var lines = new List<string>();

                        // Read all lines from file
                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var sr = new StreamReader(fs))
                        {
                            while (!sr.EndOfStream)
                                lines.Add(sr.ReadLine());
                        }

                        // Grouping blocks of logs starting with "EventDateTime:"
                        var block = new List<string>();
                        var matchingBlocks = new List<string>();
                        bool insideBlock = false;

                        foreach (var line in lines)
                        {
                            if (line.StartsWith("EventDateTime:"))
                            {
                                if (block.Count > 0)
                                {
                                    string blockText = string.Join("\n", block);
                                    if (blockText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                                        matchingBlocks.Add(blockText);

                                    block.Clear();
                                }
                                insideBlock = true;
                            }

                            if (insideBlock)
                                block.Add(line);
                        }

                        // Handle last block if it matches
                        if (block.Count > 0)
                        {
                            string blockText = string.Join("\n", block);
                            if (blockText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                                matchingBlocks.Add(blockText);
                        }

                        // Store results if matches found
                        if (matchingBlocks.Count > 0)
                        {
                            string combinedPreview = string.Join("\n\n===========================\n\n", matchingBlocks);

                            // Count total matches (case-insensitive)
                            int totalMatches = 0;
                            foreach (var matchBlock in matchingBlocks)
                            {
                                int count = System.Text.RegularExpressions.Regex.Matches(
                                    matchBlock,
                                    System.Text.RegularExpressions.Regex.Escape(searchText),
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                ).Count;

                                totalMatches += count;
                            }

                            results.Add(new SearchResult
                            {
                                Path = file,
                                Preview = combinedPreview,
                                MatchCount = totalMatches
                            });
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"[SKIPPED] {file} - {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Search Error: " + ex.Message);
            }

            return results;
        }
    }
}
 