using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace testing_fileIO
{
    public class Section
    {
        public int Index { get; set; }
        public List<string> TerminalText { get; set; }
        public List<string> InfoText { get; set; }
        public List<string> Solutions { get; set; }

        public Section()
        {
            InfoText = new List<string>();
            TerminalText = new List<string>();
            Solutions = new List<string>();
            Index = -1;
        }

    }
    public class Classroom
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Section> SectionsList { get; set; }

        public Classroom()
        {
            Name = string.Empty;
            Description = string.Empty;
            SectionsList = new List<Section>();
            Index = 0;
        }
    }

    class Program
    {
        static void Program2(string[] args)
        {
            #region VARIABLES
            //COMPROBACION DEL SO PARA LA RUTA DE LOS FICHEROS
            OperatingSystem osInfo = Environment.OSVersion;
            Console.WriteLine($"SO - Platform: {osInfo.Platform}, Version: {osInfo.Version}");

            string winPath = @"C:\GDS\GDS-UCO\testing-fileIO\testing-fileIO\data\";
            string macPath = @"/Users/eortiz/GDS/GDS-UCO/testing-fileIO/testing-fileIO/data/";

            string sPathFiles = (osInfo.Platform.ToString().ToLower().Contains("win") ? winPath : macPath);
            string sPathInputFiles = sPathFiles + "_PANTALLAS/";
            string sPathOutputFiles = sPathFiles + "_output/";
            string sFilePantallas = "pantallasTodas.txt";
            string sqliteDbPath = "Data Source=" + sPathFiles + "egds.db";

            string sCountriesFileCSV = sPathFiles + "countries.csv";

            List<Classroom> ClassroomList = new List<Classroom>();

            #endregion VARIABLES



            #region LECTURA FICHEROS PANTALLAS -> CONVERSION A LISTA DE OBJETOS
            string[] pantallas = File.ReadAllLines(sPathFiles + sFilePantallas);
            foreach (string pantalla in pantallas)
            {
                Classroom oClassroom = new Classroom();
                oClassroom.Index = ClassroomList.Count + 1;
              

                //SON TODAS LAS LINEAS DEL FICHERO DE PANTALLA
                string[] lines = File.ReadAllLines(sPathInputFiles + pantalla);

                Section oSection = new Section();
                int currentIndex = -1;

                foreach (string line in lines)
                {
                    string[] cols = line.Split(",");

                    if ( cols.Length > 3)
                    {
                        var auxArr = string.Join(", ",cols, 2, cols.Length-2);
                        cols[2] = auxArr;
                    }

                    var lineIndex = int.Parse(cols[0].Trim(new Char[] { '"' }));
                    var command = int.Parse(cols[1]);
                    var content = cols[2].Trim(new Char[] { '"' });

                    if ( currentIndex < lineIndex)
                    {
                        if ( currentIndex > -1)
                        {
                            oClassroom.SectionsList.Add(oSection);
                            oSection = new Section();
                        }

                        currentIndex = lineIndex;
                        oSection.Index = currentIndex;
                    }

                    switch ( command )
                    {
                        case 22: //Classroom Name
                            oClassroom.Name = content.Trim();
                            break;

                        case 1: //terminalText
                            oSection.TerminalText.Add(content);
                            break;

                        case 2: //infoText
                            oSection.InfoText.Add(content);
                            break;

                        case 5: //solutions
                            oSection.Solutions.Add(content);
                            break;

                        case 9: //fin de pantalla
                            
                            oClassroom.SectionsList.Add(oSection);
                            ClassroomList.Add(oClassroom);

                            oSection = new Section();

                            string jsonString = JsonSerializer.Serialize(oClassroom.SectionsList);

                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };

                            jsonString = JsonSerializer.Serialize(oClassroom.SectionsList, options);
                            File.WriteAllText(sPathOutputFiles + pantalla.Split('.')[0] + ".json", jsonString);
                            

                            break;

                        default:
                            break;
                    }

                    
                }

            }

            #endregion FIN LECTURA FICHEROS


         
        }
    }
}
