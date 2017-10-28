using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mgm.DupeFinder
{
    class Program
    {
        public static void Main(string[] args)
        {
            string rootDir = null;
            Console.WriteLine("Enter the directory you want to search in this format: /[yourUserName]/Path/To/Directory, or drag and drop the folder from a finder window onto the line below:");
            rootDir = Console.ReadLine().Trim();
            //Create a uniques dictionary
            Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();
            //Collect all the files
            List<string> allFiles = new List<string>();
            DupeFinderUtilities.FileCollector(rootDir, allFiles);

            //group by fileSize
            foreach (string fileName in allFiles)
            {
                //test for unique file size
                FileInfo fileInfo = new FileInfo(fileName);
                string fileSize = fileInfo.Length.ToString();
                DupeFinderUtilities.UniqueKeyChecker(fileSize
                                  , fileName
                                  , fileGroups);
            }
            //query for multi-file groupings
            Dictionary<string, List<string>> nonUniqueSizes = DupeFinderUtilities.NonUniquesQuery(fileGroups);
            //group by hashes
            DupeFinderUtilities.GroupByHash(fileGroups, nonUniqueSizes);
            //sort to put non-uniques at the top of a list
            List<List<string>> uniqueContents =
                (from fileGroup in fileGroups
                 orderby fileGroup.Value.Count descending
                 select fileGroup.Value)
                    .ToList();

            DupeFinderUtilities.ConsoleLogFileGroups(uniqueContents, rootDir);
            //dump all the entries with 0 Count values
            //sort entries by value count
            //group by fileHash
        }
        //grab subdirectories 
        //grab files
        //Search the files for duplicate weights
        //hash the files with duplicated weights
        //search the duplicated weights for duplicate hashes
    }
}


