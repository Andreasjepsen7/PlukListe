using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Plukliste
{
    class PluklisteProgram
    {
        static ConsoleColor standardColor;
        static List<string> files;
        static int index = -1;

        static void Main()
        {
            standardColor = Console.ForegroundColor;
            files = LoadFiles();

            char readKey = ' ';

            while (readKey != 'Q')
            {
                if (files.Count == 0)
                {
                    Console.WriteLine("No files found.");
                    Console.ReadKey();
                }
                else
                {
                    if (index == -1) index = 0;

                    DisplayPlukliste(index);
                    DisplayOptions();

                    readKey = Console.ReadKey().KeyChar;
                    if (readKey >= 'a') readKey -= (char)('a' - 'A'); // Convert to uppercase
                    Console.Clear();

                    Console.ForegroundColor = ConsoleColor.Red;
                    switch (readKey)
                    {
                        case 'G':
                            files = LoadFiles();
                            index = -1;
                            Console.WriteLine("Pluklister genindlæst");
                            break;
                        case 'F':
                            if (index > 0) index--;
                            break;
                        case 'N':
                            if (index < files.Count - 1) index++;
                            break;
                        case 'A':
                            CompletePlukliste(ref index);
                            break;
                    }
                    Console.ForegroundColor = standardColor;
                }
            }
        }

        static List<string> LoadFiles()
        {
            Directory.CreateDirectory("import");

            if (!Directory.Exists("export"))
            {
                Console.WriteLine("Directory \"export\" not found");
                Console.ReadLine();
                Environment.Exit(0);
            }

            var xmlFiles = Directory.EnumerateFiles("export", "*.xml").ToList();
            var csvFiles = Directory.EnumerateFiles("export", "*.csv").ToList();

            // Combine XML and CSV file lists
            var combinedFiles = new List<string>();
            combinedFiles.AddRange(xmlFiles);
            combinedFiles.AddRange(csvFiles);

            return combinedFiles;
        }



        static void DisplayPlukliste(int index)
        {
            Console.WriteLine($"Plukliste {index + 1} af {files.Count}");
            Console.WriteLine($"\nfile: {files[index]}");

            // Determine file type and call appropriate function to display content
            if (files[index].EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                DisplayXmlContent(files[index]);
            else if (files[index].EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                DisplayCsvContent(files[index]);
            else
                Console.WriteLine("Unsupported file type.");
        }

        static void DisplayXmlContent(string filePath)
        {
            using FileStream file = File.OpenRead(filePath);
            var xmlSerializer = new XmlSerializer(typeof(Pluklist));
            var plukliste = (Pluklist?)xmlSerializer.Deserialize(file);

            if (plukliste != null && plukliste.Lines != null)
            {
                Console.WriteLine("\n{0, -13}{1}", "Name:", plukliste.Name);
                Console.WriteLine("{0, -13}{1}", "Forsendelse:", plukliste.Forsendelse);

                Console.WriteLine("\n{0,-7}{1,-9}{2,-20}{3}", "Antal", "Type", "Produktnr.", "Navn");
                foreach (var item in plukliste.Lines)
                {
                    Console.WriteLine("{0,-7}{1,-9}{2,-20}{3}", item.Amount, item.Type, item.ProductID, item.Title);
                }
            }
        }

        static void DisplayCsvContent(string filePath)
        {
            // Read all lines from the CSV file
            string[] lines = File.ReadAllLines(filePath);

            // Assuming the CSV has a header and data
            if (lines.Length < 2)
            {
                Console.WriteLine("Invalid CSV format: At least header and one data line expected.");
                return;
            }

            // Split the header and data into columns
            string[] header = lines[0].Split(';');
            List<string[]> data = new List<string[]>();

            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(';');
                data.Add(columns);
            }

            // Display header
            Console.WriteLine("CSV Header:");
            Console.WriteLine(string.Join("; ", header));

            // Display data
            Console.WriteLine("CSV Data:");
            foreach (var row in data)
            {
                Console.WriteLine(string.Join("; ", row));
            }
        }
        

        static void DisplayOptions()
        {
            Console.WriteLine("\n\nOptions:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Q");
            Console.ForegroundColor = standardColor;
            Console.WriteLine("uit");
            if (index >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("A");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("fslut plukseddel");
            }
            if (index > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("F");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("orrige plukseddel");
            }
            if (index < files.Count - 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("N");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("æste plukseddel");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("G");
            Console.ForegroundColor = standardColor;
            Console.WriteLine("enindlæs pluksedler");
        }

        static void CompletePlukliste(ref int index)
        {
            var fileWithoutPath = files[index].Substring(files[index].LastIndexOf('\\'));
            try
            {
                File.Move(files[index], $"import{fileWithoutPath}");
            }
            catch (IOException e)
            {
                if (e.HResult == -2147024713) // A file that already exists cannot be created
                    File.Delete(files[index]);
            }
            Console.WriteLine($"Plukseddel {files[index]} afsluttet.");
            files.Remove(files[index]);
            if (index == files.Count) index--;
        }
    }
}
