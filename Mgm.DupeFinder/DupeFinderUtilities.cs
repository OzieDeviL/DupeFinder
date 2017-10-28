using System;
using System.IO;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Text;
using System.Linq;

namespace Mgm.DupeFinder
{
    public static class DupeFinderUtilities
    {
        public static void FileCollector(string parentDir, List<string> allFiles)
        {
            //this method retrieves file names with their full path
            List<string> dirFiles = Directory.EnumerateFiles(parentDir).ToList();
            //add that to the list of files
            allFiles.AddRange(dirFiles);
            List<string> subDirs = Directory.EnumerateDirectories(parentDir).ToList();
            if (subDirs.Count() > 0)
            {
                foreach (string subDirectory in subDirs)
                {
                    FileCollector(subDirectory
                                  , allFiles);
                }
            }
        }

        public static void GroupByHash(Dictionary<string, List<string>> fileGroups
                                       , Dictionary<string, List<string>> nonUniqueSizes)
        {
            foreach (KeyValuePair<string, List<string>> uniqueSize in nonUniqueSizes)
            {
                //create a small dictionary to hold the hashed entries
                Dictionary<string, List<string>> uniqueHashes = new Dictionary<string, List<string>>();
                //hash a file
                foreach (string fileName in uniqueSize.Value)
                {
                    string entryHash = FileHasher(fileName);
                    if (uniqueHashes.ContainsKey(entryHash))
                    {
                        uniqueHashes[entryHash].Add(fileName);
                    }
                    else
                    {
                        List<string> newUniqueHashEntry = new List<string>();
                        newUniqueHashEntry.Add(fileName);
                        uniqueHashes.Add(entryHash, newUniqueHashEntry);
                    }
                }
                foreach (KeyValuePair<string, List<string>> uniqueHash in uniqueHashes) {
                    //don't want to convert the byte array, might be big, so just use a GUID
                    fileGroups.Add((Guid.NewGuid()).ToString(), uniqueHash.Value);
                }
                fileGroups.Remove(uniqueSize.Key);
            }
        }

        public static void ConsoleLogFileGroups(List<List<string>> fileGroups, string rootDir)
        {
            Console.Write(Environment.NewLine + "------------------------RESULTS------------------------------------------"
                          + Environment.NewLine + "Files bracketed together have the same content (their metadata may vary)."
                          + Environment.NewLine + "All files are listed relative to the directory: " + rootDir
                          + Environment.NewLine + "NOTE: The list below will include hidden system files such as .Ds_Store that won't show up in Finder, don't worry about them");
            foreach (List<string> fileGroup in fileGroups)
            {
                fileGroup.Sort();
                Console.Write(Environment.NewLine 
                              + "[");
                for (int i = 0; i < fileGroup.Count(); i++)
                    if (i < fileGroup.Count() - 1)
                    {
                    Console.Write(Environment.NewLine 
                                  + "\t" 
                                  + Path.GetRelativePath(rootDir, fileGroup[i]));
                    } else
                    {
                    Console.Write(Environment.NewLine 
                                  + "\t" + Path.GetRelativePath(rootDir, fileGroup[i]) 
                                  + Environment.NewLine 
                                  + "],");
                    }
            }
            Console.Write("\b" + Environment.NewLine + "--------------------------END RESULTS------------------------------");
        }

        private static string FileHasher(string fileName)
        {   
            xxHash xh = new xxHash();
            using (FileStream fs = File.Open(fileName, FileMode.Open))
            {
                byte[] byteHash = xh.ComputeHash(fs);
                string stringHash = BitConverter.ToString(byteHash);
                return stringHash;
            }
        }

        public static void UniqueKeyChecker(string candidateKey
                                            , string candidateFileName
                                             , Dictionary<string, List<string>> uniques)
        {
            //check if there are dupes already caught
            if (uniques.ContainsKey(candidateKey)) {
                uniques[candidateKey].Add(candidateFileName);    
            } else {
                List<string> newUniquesEntry = new List<string>();
                newUniquesEntry.Add(candidateFileName);
                uniques.Add(candidateKey, newUniquesEntry);
            }
        }

        public static Dictionary<string, List<string>> NonUniquesQuery(Dictionary<string, List<string>> groupedFiles)
        {
            return 
                (from entry in groupedFiles
                 where entry.Value.Count() > 1
                 select new { entry.Key, entry.Value })
                    .ToDictionary(x => x.Key, x => x.Value);
            
        }   
    }
}
