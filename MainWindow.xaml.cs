using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SIAV_IncasellaEmail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string directoryInputPath;
        public string directoryOutputPath;
        public string directoryErroriPath;
        string[] emlFiles;
        long numeroFileErrati = 0;

        public MainWindow()
        {
            InitializeComponent();
            /*
            var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = configBuilder.Build();

            // Get input and output directory paths from configuration
            string inputDirectoryPath = configuration["InputDirectoryPath"];
            string outputDirectoryPath = configuration["OutputDirectoryPath"];
            
            // Set the text of the corresponding TextBox controls
            txtDirectory.Text = inputDirectoryPath;
            txtOutputDirectory.Text = outputDirectoryPath;

            if(txtDirectory.Text != "")
            {
                emlFiles = System.IO.Directory.GetFiles(txtDirectory.Text, "*.eml", SearchOption.TopDirectoryOnly);

                listFiles.ItemsSource = (emlFiles.Select(x => Path.GetFileName(x)));

                directoryErroriPath = Path.Combine(txtDirectory.Text, "ERRORI");
            }
            */

        }


        private void Select_input_folder(object sender, RoutedEventArgs e)
        {
            // Create a new open file dialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Select a directory";
            dialog.Filter = "EML Files|*.EML";
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Posizionati nella cartella e premi OK";

            // Show the dialog and check if the user clicked OK
            if (dialog.ShowDialog() == true)
            {
                // Get the selected directory path
                directoryInputPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                txtDirectory.Text = directoryInputPath;
                emlFiles = System.IO.Directory.GetFiles(directoryInputPath, "*.eml", SearchOption.TopDirectoryOnly);

                listFiles.ItemsSource = (emlFiles.Select(x => Path.GetFileName(x)));

                if (txtOutputDirectory.Text == "")
                {
                    directoryOutputPath = Path.Combine(directoryInputPath, "OUTPUT");
                    txtOutputDirectory.Text = directoryOutputPath;
                }

                directoryErroriPath = Path.Combine(directoryInputPath, "ERRORI");
            }
        }
        private void Select_output_folder(object sender, RoutedEventArgs e)
        {
            // Create a new open file dialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Select a directory";
            dialog.Filter = "EML Files|*.EML";
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Posizionati nella cartella di Output";

            // Show the dialog and check if the user clicked OK
            if (dialog.ShowDialog() == true)
            {
                // Get the selected directory path
                directoryOutputPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                txtOutputDirectory.Text = directoryOutputPath;


            }
        }
        private void Incasella_Emails(object sender, RoutedEventArgs e)
        {
            //LOAD THE VALID EMAILS
            directoryInputPath = txtDirectory.Text;
            directoryOutputPath = txtOutputDirectory.Text;
            string csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ABACO-CaselleCensite.txt");
            List<string> allowedEmailAddresses = File.ReadAllLines(csvFilePath).ToList();

            directoryErroriPath = Path.Combine(directoryOutputPath, "ERRORI");

            if (emlFiles == null || emlFiles.Length == 0) {
                MessageBox.Show("Nessun File Selezionato");
                return;
            }

            //Creo gli ambienti
            if (!(Directory.Exists(directoryOutputPath))) { Directory.CreateDirectory(directoryOutputPath); }

            progressBar.Maximum = emlFiles.Length;
            long contaFiles = 0;

            // Read each .eml file and extract the email address from the second row
            foreach (var emlFile in emlFiles)
            {
                List<string> emailAddressesList = new List<string>();

                //CHECK
                emailAddressesList = HelperFunctions.Check_SecondoTipo_MultiMail_ListaCSV_ReturnList(emlFile, directoryOutputPath);
                Boolean IsOK = false;
                if (emailAddressesList.Count > 0)
                {
                    foreach (string emailAddressInList in emailAddressesList)
                    {
                        if (allowedEmailAddresses.Any(element => element.Equals(emailAddressInList, StringComparison.OrdinalIgnoreCase))){
                            HelperGestisciEML.IncasellaEmail(emailAddressInList, emlFile, directoryOutputPath);
                            IsOK = true;

                        }
                        /*else
                        {
                            HelperGestisciEML.IncasellaEmail(emailAddressInList, emlFile, directoryErroriPath);
                        }*/

                       
                    }
                    if (IsOK == false) {
                        HelperGestisciEML.IncasellaEmail("errore@errore.it", emlFile, directoryErroriPath);
                    }
                }
                else
                {
                    //NESSUNA CASISTICA => LO SPOSTO SEMPLICEMENTE IN "ERRORI"
                    directoryErroriPath = Path.Combine(directoryOutputPath, "ERRORI");
                    if (!Directory.Exists(directoryErroriPath)) { Directory.CreateDirectory(directoryErroriPath); };
                    Console.WriteLine(Path.Combine(directoryErroriPath));
                    //err file
                    File.Move(emlFile, Path.Combine(directoryErroriPath, Path.GetFileName(emlFile)));
                    numeroFileErrati++;
                }
                File.Delete(emlFile);


                contaFiles += 1;
                progressBar.Value = contaFiles;
            } //fine for each

            emlFiles = null;
            listFiles.ItemsSource = null;
            if (numeroFileErrati == 0) { listFiles.Items.Add("Files Correttamente Elaborati"); }
            else { listFiles.Items.Add("File elaborati. Sono stati trovati " + numeroFileErrati + " email errate"); }
            MessageBox.Show("Elaborazione completata!");
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {         
            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var json = File.ReadAllText(appSettingsPath);
            var jsonSettings = new JsonSerializerSettings();

            jsonSettings.Converters.Add(new ExpandoObjectConverter());
            jsonSettings.Converters.Add(new StringEnumConverter());

            dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);

            config.InputDirectoryPath = txtDirectory.Text;
            config.OutputDirectoryPath = txtOutputDirectory.Text;

            var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

            File.WriteAllText(appSettingsPath, newJson);

        }
    }

    #region FUNZIONI D'AIUTO
    public static class HelperFunctions
    {
        public static List<string> Check_SecondoTipo_MultiMail_ListaCSV_ReturnList(string emlFile, string directoryOutputPath)
        {
            List<string> toEmailAddresses = new List<string>();

            using (StreamReader sr = new StreamReader(emlFile))
            {
                string line;
                bool inToField = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (inToField)
                    {
                        if (line.StartsWith("Cc:") | (line.StartsWith("Subject:"))){
                            break;
                        }
                    }
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
                            List<string> emailAddressList = ExtractEmailAddresses(line);
                            if (emailAddressList.Count > 0)
                            {
                                toEmailAddresses.AddRange(emailAddressList);
                            }

                            // Check if the "To:" field spans multiple rows
                            if (line.Contains(","))
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
                        if (!line.Contains(","))
                        {
                            inToField = false;
                            break;
                        }
                    }
                }
            }

            // Remove duplicates from the list of email addresses
            toEmailAddresses = toEmailAddresses.Distinct().ToList();

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
    #endregion
}



