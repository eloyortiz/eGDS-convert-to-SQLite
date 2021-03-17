using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

using System.Text.Json;

namespace testing_fileIO
{
    public class Pantalla
    {
        public int Index { get; set; }
        public List<string> TerminalText { get; set; }
        public List<string> InfoText { get; set; }
        public List<string> Solutions { get; set; }

        public Pantalla()
        {
            InfoText = new List<string>();
            TerminalText = new List<string>();
            Solutions = new List<string>();
            Index = -1;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            //COMPROBACION DEL SO PARA LA RUTA DE LOS FICHEROS
            OperatingSystem osInfo = Environment.OSVersion;
            Console.WriteLine($"SO - Platform: {osInfo.Platform}, Version: {osInfo.Version}");

            string winPath = @"C:\GDS\GDS-UCO\testing-fileIO\testing-fileIO\data\";
            string macPath = @"/Users/eortiz/GDS/GDS-UCO/testing-fileIO/testing-fileIO/data/";

            string sPathFiles = (osInfo.Platform.ToString().ToLower().Contains("win") ? winPath : macPath);
            string sPathInputFiles = sPathFiles + "_PANTALLAS/";
            string sPathOutputFiles = sPathFiles + "_output/";
            string sFilePantallas = "pantallasTodas.txt";
            var sqliteDbPath = "Data Source=" + sPathFiles + "egds.db";


            List<Pantalla> Listado = new List<Pantalla>();

            #region SQLITE
            
            using (var connection = new SqliteConnection( sqliteDbPath ) )
            {
                connection.Open();
                var idCode = "222222";

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM Users
                    WHERE idCode = $idCode                    
                ";
                command.Parameters.AddWithValue("$idCode", idCode);


                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(1);

                        Console.WriteLine($"Hello, {name}!");
                    }
                }
            }

            #endregion SQLITE

            #region LECTURA FICHEROS
            string[] pantallas = File.ReadAllLines(sPathFiles + sFilePantallas);
            foreach (string pantalla in pantallas)
            {
                //SON TODAS LAS LINEAS DEL FICHERO DE PANTALLA
                string[] lines = File.ReadAllLines(sPathInputFiles + pantalla);

                Pantalla oPantalla = new Pantalla();
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
                            Listado.Add(oPantalla);
                            oPantalla = new Pantalla();
                        }

                        currentIndex = lineIndex;
                        oPantalla.Index = currentIndex;
                    }

                    switch ( command )
                    {
                        case 1: //terminalText
                            oPantalla.TerminalText.Add(content);
                            break;

                        case 2: //infoText
                            oPantalla.InfoText.Add(content);
                            break;

                        case 5: //solutions
                            oPantalla.Solutions.Add(content);
                            break;

                        case 9: //fin de pantalla
                            Listado.Add(oPantalla);
                            oPantalla = new Pantalla();

                            string jsonString = JsonSerializer.Serialize(Listado);

                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };

                            jsonString = JsonSerializer.Serialize(Listado, options);
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
