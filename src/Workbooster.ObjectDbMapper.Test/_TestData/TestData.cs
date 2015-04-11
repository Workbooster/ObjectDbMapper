using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Workbooster.ObjectDbMapper.Test._TestData
{
    public static class TestData
    {
        public static readonly string TEST_DATA_DIRECTORY = @"_TestData\";
        public static readonly string DATABASE_FILE_NAME = @"data.mdf";
        public static readonly string CONNECTION_STRING_PATTERN = @"Data Source=(LocalDB)\v11.0; AttachDbFilename={0}; Integrated Security=True;";
        public static readonly string CONNECTION_STRING = String.Format(CONNECTION_STRING_PATTERN, Path.Combine(Environment.CurrentDirectory, TEST_DATA_DIRECTORY + DATABASE_FILE_NAME));

        /// <summary>
        /// Creates a backup of the test data. Please call this on test setup.
        /// </summary>
        public static string SetupTempTestDb()
        {
            string guid = Guid.NewGuid().ToString();
            string testDataDirectoryPath = Path.Combine(Environment.CurrentDirectory, TestData.TEST_DATA_DIRECTORY);
            string tempTestDataDirectoryPath = Path.Combine(Environment.CurrentDirectory, @"_TempTestData\" + guid + @"\");

            if (Directory.Exists(tempTestDataDirectoryPath))
            {
                Directory.Delete(tempTestDataDirectoryPath, true);
            }

            CopyDirectory(testDataDirectoryPath, tempTestDataDirectoryPath, true);

            string tempTestDbFilePath = Path.Combine(tempTestDataDirectoryPath, TestData.DATABASE_FILE_NAME);

            return String.Format(CONNECTION_STRING_PATTERN, tempTestDbFilePath);
        }

        private static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
