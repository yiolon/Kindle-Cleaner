using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    public static void Main()
    {
        string folderPath = "";
        bool validPath = false;

        // Prompt the user until a valid path is entered
        while (!validPath)
        {
            Console.WriteLine("Please input the file path to the documents folder in your Kindle:");
            folderPath = Console.ReadLine();
            //folderPath = @"I:\documents";

            if (Directory.Exists(folderPath))
            {
                validPath = true;
                Console.WriteLine("Directory found, proceeding with file operations...");
            }
            else
            {
                Console.WriteLine("Directory not found, please enter a valid path.");
            }
        }

        // Get all ebook files in the directory with specified extensions
        string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                  .Where(f => f.EndsWith(".txt") || f.EndsWith(".azw3") || f.EndsWith(".mobi") || f.EndsWith(".pdf") || f.EndsWith(".epub"))
                                  .ToArray();

        // Get all folders with .sdr extension
        string[] folders = Directory.GetDirectories(folderPath, "*.sdr", SearchOption.TopDirectoryOnly);

        HashSet<string> fileBaseNames = new HashSet<string>();

        foreach (var file in files)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            fileBaseNames.Add(fileNameWithoutExtension);
        }

        foreach (var folder in folders)
        {
            string folderNameWithoutExtension = Path.GetFileNameWithoutExtension(folder);

            if (!fileBaseNames.Contains(folderNameWithoutExtension))
            {
                Console.WriteLine($"Folder '{folder}' does not have a matching file.");
                try
                {
                    Directory.Delete(folder, true); // 'true' means delete the folder and all its contents
                    Console.WriteLine($"Folder '{folder}' deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting folder '{folder}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Folder '{folder}' has a matching file.");
            }
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}
