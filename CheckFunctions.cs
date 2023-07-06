using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SIAV_IncasellaEmail
{
    class CheckFunctions
    {
        public static String Check_PrimoTipo_SecondaRiga(string emlFile)
        {
            using (StreamReader sr = new StreamReader(emlFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Check lines starting with "To:"
                    if (line.StartsWith("Delivered-To:"))
                    {
                        MatchCollection matches = Regex.Matches(line, @"[^\s,<>]+@[^\s,<>]+\.[^\s,<>]+");
                        if (matches.Count > 0)
                        {
                            // If email addresses are found, extract them and return the first one
                            string emailAddress = matches[0].Value;
                            return emailAddress;
                        }
                    }
                }
            }
            // If no email address is found, return null
            return null;
        }

        public static String Check_SecondoTipo_TuttaLaMail(string emlFile)
        {
            using (StreamReader sr = new StreamReader(emlFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Check lines starting with "To:"
                    if (line.StartsWith("To:"))
                    {
                        Match match = Regex.Match(line, @"[^\s,<>]+@[^\s,<>]+\.[^\s,<>]+");
                        if (match.Success)
                        {
                            // If an email address is found, extract it and return it
                            string emailAddress = match.Groups[1].Value.Trim('<', '>');
                            MessageBox.Show(emailAddress);
                            return emailAddress;
                        }
                    }
                }
            }
            // If no email address is found, return null
            return null;
        }

        public static List<string> Check_SecondoTipo_MultiMail_ListaCSV_ReturnList(string emlFile)
        {
            List<string> toEmailAddresses = new List<string>();

            // Get the path to the CSV file in the same directory as the executable
            string csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ABACO-CaselleCensite.txt");

            // Read the list of email addresses from the CSV file
            List<string> allowedEmailAddresses = File.ReadAllLines(csvFilePath).ToList();

            using (StreamReader sr = new StreamReader(emlFile))
            {
                string line;
                bool inToField = false;
                while ((line = sr.ReadLine()) != null)
                {
                    // Check lines starting with "To:"
                    if (line.StartsWith("To:"))
                    {
                        // If we're already in the "To" field, we've found a multi-row "To:"
                        if (inToField)
                        {
                            // Extract the email addresses from the line and add them to the list
                            List<string> emailAddresses = ExtractEmailAddresses(line);
                            toEmailAddresses.AddRange(emailAddresses);
                        }
                        else
                        {
                            // Extract the email address from the line and add it to the list
                            string emailAddress = ExtractEmailAddress(line);
                            if (emailAddress != null)
                            {
                                toEmailAddresses.Add(emailAddress);
                            }

                            // Check if the "To:" field spans multiple rows
                            if (line.EndsWith(","))
                            {
                                inToField = true;
                            }
                        }
                    }
                    else if (inToField)
                    {
                        // If we're in the "To" field and the line doesn't start with "To:",
                        // assume it's a continuation of the previous line and extract the email
                        // addresses from it and add them to the list
                        List<string> emailAddresses = ExtractEmailAddresses(line);
                        toEmailAddresses.AddRange(emailAddresses);

                        // Check if the line ends with a comma, indicating that the "To" field
                        // spans multiple rows
                        if (!line.EndsWith(","))
                        {
                            inToField = false;
                        }
                    }
                }
            }

            // Remove duplicates from the list of email addresses
            toEmailAddresses = toEmailAddresses.Distinct().ToList();

            // Check if each email address in the list is allowed
            List<string> allowedEmailAddressesLower = allowedEmailAddresses.Select(e => e.ToLowerInvariant()).ToList();
            List<string> invalidEmailAddresses = toEmailAddresses.Where(e => !allowedEmailAddressesLower.Contains(e.ToLowerInvariant())).ToList();

            // If there are invalid email addresses, return null
            if (invalidEmailAddresses.Count > 0)
            {
                //Move to error folder 
                //File.Copy(emlFile, )
                return null;
            }

            return toEmailAddresses;
        }

        private static string ExtractEmailAddress(string line)
        {
            Match match = Regex.Match(line, @"^To:\s*(?:.+<)?(.+@.+\..+)>?$");
            if (match.Success)
            {
                // If an email address is found, extract it and return it
                string emailAddress = match.Groups[1].Value.Trim('<', '>', ',');
                return emailAddress;
            }
            return null;
        }

        private static List<string> ExtractEmailAddresses(string line)
        {
            List<string> emailAddresses = new List<string>();
            string pattern = @"
        # Match an email address enclosed in angle brackets
        (?<=<)                # Lookbehind for the left angle bracket
        [^\s<>]+@[^\s<>]+\.[^\s<>]+  # Match the email address
        (?=>)                 # Lookahead for the right angle bracket
        |
        # Match an email address not enclosed in angle brackets
        [^\s,<>]+@[^\s,<>]+\.[^\s,<>]+  # Match the email address
        ";
            MatchCollection matches = Regex.Matches(line, pattern, RegexOptions.IgnorePatternWhitespace);
            foreach (Match match in matches)
            {
                string emailAddress = match.Value;
                emailAddresses.Add(emailAddress);
            }
            return emailAddresses;
        }
    }
}
