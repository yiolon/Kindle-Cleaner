using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static readonly string[] ebookExtensions = new[] { ".txt", ".azw3", ".mobi", ".pdf", ".epub" };

    public static void Main()
    {
        Console.WriteLine("Kindle Cleaner Utility");
        Console.WriteLine("======================");

        string folderPath = "";
        while (true)
        {
            Console.WriteLine("Please input the full path to your Kindle's 'documents' folder:");
            folderPath = Console.ReadLine();

            if (Directory.Exists(folderPath))
                break;
            Console.WriteLine("Path not found. Please try again.\n");
        }

        while (true)
        {
            Console.WriteLine("\nChoose an operation:");
            Console.WriteLine("1 - Delete orphan .sdr folders (across all folders)");
            Console.WriteLine("2 - Delete duplicate ebook files in root (keep sorted version)");
            Console.WriteLine("e - Exit");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine()?.Trim().ToLower();

            switch (choice)
            {
                case "1":
                    DeleteOrphanedSdrs(folderPath);
                    break;
                case "2":
                    DeleteRootDuplicates(folderPath);
                    break;
                case "e":
                    Console.WriteLine("Exiting. Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid input. Please enter 1, 2, or e.\n");
                    break;
            }
        }
    }

    static void DeleteOrphanedSdrs(string documentsFolder)
    {
        Console.WriteLine("\n--- Deleting orphaned .sdr folders ---");

        var allEbookFiles = Directory.GetFiles(documentsFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => ebookExtensions.Contains(Path.GetExtension(f).ToLower()))
            .ToHashSet();

        var ebookBaseNames = new HashSet<string>(
            allEbookFiles.Select(f => Path.GetFileNameWithoutExtension(f))
        );

        var sdrFolders = Directory.GetDirectories(documentsFolder, "*.sdr", SearchOption.AllDirectories);
        int deletedCount = 0;

        foreach (var sdr in sdrFolders)
        {
            string sdrName = Path.GetFileNameWithoutExtension(sdr);
            if (!ebookBaseNames.Contains(sdrName))
            {
                try
                {
                    Directory.Delete(sdr, true);
                    Console.WriteLine($"Deleted orphaned SDR folder: {sdr}");
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting {sdr}: {ex.Message}");
                }
            }
        }

        Console.WriteLine($"\nDone. Total orphaned .sdr folders deleted: {deletedCount}");
    }

    static void DeleteRootDuplicates(string documentsFolder)
    {
        Console.WriteLine("\n--- Deleting root-level duplicate ebook files ---");

        // 1. Get ebook files in the root of /documents
        var rootFiles = Directory.GetFiles(documentsFolder, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => ebookExtensions.Contains(Path.GetExtension(f).ToLower()))
            .ToDictionary(Path.GetFileName, f => f);

        // 2. Get ebook files in subfolders
        var sortedFiles = Directory.GetFiles(documentsFolder, "*.*", SearchOption.AllDirectories)
            .Where(f =>
                ebookExtensions.Contains(Path.GetExtension(f).ToLower()) &&
                Path.GetDirectoryName(f) != documentsFolder)
            .GroupBy(f => Path.GetFileName(f)) // Group files with same name
            .ToDictionary(g => g.Key, g => g.First()); // Just pick the first one


        var duplicates = rootFiles.Keys.Intersect(sortedFiles.Keys);
        int deletedCount = 0;

        foreach (var filename in duplicates)
        {
            string rootPath = rootFiles[filename];
            string sdrPath = Path.Combine(documentsFolder, Path.GetFileNameWithoutExtension(rootPath) + ".sdr");

            try
            {
                File.Delete(rootPath);
                deletedCount++;
                Console.WriteLine($"Deleted duplicate root file: {rootPath}");

                if (Directory.Exists(sdrPath))
                {
                    Directory.Delete(sdrPath, true);
                    Console.WriteLine($"Deleted associated SDR folder: {sdrPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {rootPath} or its SDR: {ex.Message}");
            }
        }

        Console.WriteLine($"\nDone. Total duplicate ebook files deleted: {deletedCount}");
    }
}
