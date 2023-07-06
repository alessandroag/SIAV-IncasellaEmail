using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SIAV_IncasellaEmail
{
    class HelperGestisciEML
    {
        public static void IncasellaEmail(string emailAddress, string emlFile, string directoryOutputPath)
        {
            if (!string.IsNullOrEmpty(emailAddress))
            {
                // Create a folder with the email address as its name (if it doesn't exist already)
                string folderEmailPath = Path.Combine(directoryOutputPath, emailAddress);
                if (!Directory.Exists(folderEmailPath))
                {
                    Directory.CreateDirectory(folderEmailPath);
                }

                // Move the .eml file to the folder
                string newFilePath = System.IO.Path.Combine(folderEmailPath, Path.GetFileName(emlFile));
                
                
                if (newFilePath.Length > 260)
                {
                    // The full path is too long, so we need to shorten it

                    string shortFileName = Path.GetFileName(emlFile);
                    string shortFullPath = Path.Combine(folderEmailPath, shortFileName);

                    if (shortFullPath.Length > 260)
                    {
                        // The full path is still too long, so we need to shorten the directory path

                        int directoryLength = folderEmailPath.Length;
                        int fileNameLength = shortFileName.Length;
                        int maxLength = 260 - fileNameLength - 1; // Subtract 1 for the backslash

                        if (directoryLength > maxLength)
                        {
                            // The directory path is too long, so we need to shorten it

                            string shortDirectoryPath = folderEmailPath.Substring(0, maxLength);
                            shortFullPath = Path.Combine(shortDirectoryPath, shortFileName);
                        }
                        else
                        {
                            // The directory path is short enough, so we can use the original path with the short file name
                            shortFullPath = Path.Combine(folderEmailPath, shortFileName);
                        }
                    }
                    newFilePath = shortFullPath;
                }
                newFilePath = Path.Combine(folderEmailPath, Path.GetFileNameWithoutExtension(emlFile));
                newFilePath = newFilePath.Length > 220 ? newFilePath.Substring(0, 220) : newFilePath;
                newFilePath = newFilePath + Path.GetExtension(emlFile);

                if (File.Exists(newFilePath))
                {
                    int suffix = 0;
                    while (File.Exists(newFilePath))
                    {
                        suffix++;
                        newFilePath = Path.Combine(folderEmailPath, Path.GetFileNameWithoutExtension(newFilePath)) + "(" + suffix + ")" + Path.GetExtension(emlFile);
                        //newFilePath = Path.Combine(folderEmailPath, Path.GetFileNameWithoutExtension(emlFile));
                        //newFilePath = newFilePath.Length > 220 ? newFilePath.Substring(0, 220) : newFilePath;
                       // newFilePath = newFilePath + "(" + suffix + ")" + Path.GetExtension(emlFile);
                    }
                }
                //if(newFilePath.Length>= 255)
                ////    newFilePath = Path.Combine(folderEmailPath) + Path.GetFileNameWithoutExtension(emlFile).Substring(15) + Path.GetExtension(emlFile);
                //}
                
                File.Copy(emlFile, newFilePath);
            }
        }
    }
}
