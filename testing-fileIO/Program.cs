using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace testing_fileIO
{
    public class Rutas
    {
        string winPath { get; set; }
        string macPath { get; set; }


        public string sPathFiles { get; set; }
        public string sPathInputFiles { get; set; }
        public string sPathOutputFiles { get; set; }
        public string sFilePantallas { get; set; }
        public string sqliteDbPath { get; set; }
         
        public string[] sFileCSV { get; set; }

        public Rutas()
        {
            //COMPROBACION DEL SO PARA LA RUTA DE LOS FICHEROS
            OperatingSystem osInfo = Environment.OSVersion;
            Console.WriteLine($"SO - Platform: {osInfo.Platform}, Version: {osInfo.Version}");

            winPath = @"C:\GDS\GDS-UCO\testing-fileIO\testing-fileIO\data\";
            macPath = @"/Users/eortiz/GDS/GDS-UCO/testing-fileIO/testing-fileIO/data/";

            sPathFiles = (osInfo.Platform.ToString().ToLower().Contains("win") ? winPath : macPath);
            sPathInputFiles = sPathFiles + "_PANTALLAS/";
            sPathOutputFiles = sPathFiles + "_output/";
            sFilePantallas = sPathFiles + "pantallasTodas.txt";
            sqliteDbPath = "Data Source=" + sPathFiles + "egds.db";

            sFileCSV = new string[]{ sPathFiles + "countries.csv", sPathFiles + "aircrafts.csv", sPathFiles + "airlines.csv", sPathFiles + "airports.csv" };
        }
    }

    public class Chapter
    {
        public int Index { get; set; }
        public List<string> TerminalText { get; set; }
        public List<string> InfoText { get; set; }
        public List<string> Solutions { get; set; }

        public Chapter()
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
        public List<Chapter> ChapterList { get; set; }

        public Classroom()
        {
            Name = string.Empty;
            Description = string.Empty;
            ChapterList = new List<Chapter>();
            Index = 0;
        }
    }

    public class CSVtoSQLite
    {
        public static List<Classroom> ConvertToList(string sSourceFile, string sPathInputFiles, string sPathOutputFiles)
        {
            List<Classroom> ClassroomList = new List<Classroom>();

            #region LECTURA FICHEROS PANTALLAS -> CONVERSION A LISTA DE OBJETOS
            string[] pantallas = File.ReadAllLines(sSourceFile);
            foreach (string pantalla in pantallas)
            {
                Classroom oClassroom = new Classroom();
                oClassroom.Index = ClassroomList.Count + 1;


                //SON TODAS LAS LINEAS DEL FICHERO DE PANTALLA
                string[] lines = File.ReadAllLines(sPathInputFiles + pantalla);

                Chapter oSection = new Chapter();
                int currentIndex = -1;

                foreach (string line in lines)
                {
                    string[] cols = line.Split(",");

                    if (cols.Length > 3)
                    {
                        var auxArr = string.Join(", ", cols, 2, cols.Length - 2);
                        cols[2] = auxArr;
                    }

                    var lineIndex = int.Parse(cols[0].Trim(new Char[] { '"' }));
                    var command = int.Parse(cols[1]);
                    var content = cols[2].Trim(new Char[] { '"' });

                    if (currentIndex < lineIndex)
                    {
                        if (currentIndex > -1)
                        {
                            oClassroom.ChapterList.Add(oSection);
                            oSection = new Chapter();
                        }

                        currentIndex = lineIndex;
                        oSection.Index = currentIndex;
                    }

                    switch (command)
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

                            oClassroom.ChapterList.Add(oSection);
                            ClassroomList.Add(oClassroom);

                            oSection = new Chapter();

                            string jsonString = JsonSerializer.Serialize(oClassroom.ChapterList);

                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };

                            jsonString = JsonSerializer.Serialize(oClassroom.ChapterList, options);
                            File.WriteAllText(sPathOutputFiles + pantalla.Split('.')[0] + ".json", jsonString);


                            break;

                        default:
                            break;
                    }


                }
            }
            return ClassroomList;
            #endregion FIN LECTURA FICHEROS
        }

        public static int AddClassroom(List<Classroom> classrooms, string sqliteDbPath)
        {
            int _total = -1;
            

            foreach (Classroom classroom in classrooms)
            {
                _total = 0;

                var _classGUID = Guid.NewGuid();

                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    
                    var cmdInsert = @"INSERT INTO 'main'.'Classrooms' ('id', 'name', 'description', 'index') VALUES ($id, $name, $description, $index);";
                    var command = connection.CreateCommand();

                    command.CommandText = cmdInsert;

                    command.Parameters.AddWithValue("$id", _classGUID);
                    command.Parameters.AddWithValue("$name", classroom.Name);
                    command.Parameters.AddWithValue("$description", classroom.Description);
                    command.Parameters.AddWithValue("$index", classroom.Index);

                    int result = command.ExecuteNonQuery();
                    _total += result;
                    Console.WriteLine($"Classroom Result: {result}");

                    // SE AÑADEN LOS CHAPTERS
                    if( result > 0 && classroom.ChapterList.Count > 0)
                    {
                        foreach (Chapter chapter in classroom.ChapterList)
                        {
                            cmdInsert = @"INSERT INTO 'main'.'Chapters' ('id', 'idClass', 'index', 'infoText', 'terminalText', 'solutions') VALUES ($id, $idClass, $index, $infoText, $terminalText, $solutions);";

                            command = connection.CreateCommand();
                            command.CommandText = cmdInsert;

                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("$id", Guid.NewGuid());
                            command.Parameters.AddWithValue("$idClass", _classGUID);
                            command.Parameters.AddWithValue("$index", chapter.Index);
                            command.Parameters.AddWithValue("$infoText", string.Join(";", chapter.InfoText));
                            command.Parameters.AddWithValue("$terminalText", string.Join(";", chapter.TerminalText)); 
                            command.Parameters.AddWithValue("$solutions", string.Join(";", chapter.Solutions));

                            int _chapterResult = command.ExecuteNonQuery();
                            
                            Console.WriteLine($"Chapter Result: {_chapterResult}");
                        }
                    }
                }  
            }

            return _total;
        }

        public static int AddCountries(string sFileCSV, string sqliteDbPath)
        {
            int _total = -1;
            string[] lines = File.ReadAllLines(sFileCSV);

            foreach (string line in lines)
            {
                _total = 0;
                string[] cols = line.Split(";");

                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    var id = cols[0].Trim();
                    var code = cols[1].Trim();
                    var name = cols[2].Trim().Replace(".", string.Empty);

                    if (name.Contains("'"))
                    {
                        name = name.Replace("'", "\'");

                    }

                    var cmdInsert = @"INSERT INTO 'main'.'Countries' ('id', 'code', 'name') VALUES ($id, $code, $name);";

                    var command = connection.CreateCommand();
                    command.CommandText = cmdInsert;

                    command.Parameters.AddWithValue("$id", id);
                    command.Parameters.AddWithValue("$code", code);
                    command.Parameters.AddWithValue("$name", name);

                    int result = command.ExecuteNonQuery();
                    _total += result;
                    Console.WriteLine($"Rows Inserted - Result: {result}");
                }
            }
            return _total;
        }

        public static int AddAircrafts(string sFileCSV, string sqliteDbPath)
        {
            int _total = -1;
            string[] lines = File.ReadAllLines(sFileCSV);
            foreach (string line in lines)
            {
                _total = 0;
                string[] cols = line.Split(";");

                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    var id = cols[0].Trim();
                    var code = cols[1].Trim();
                    var manufacturer = cols[2].Trim();
                    var type = cols[3].Trim();
                    var wake = cols[4].Trim();

                    var cmdInsert = @"INSERT INTO 'main'.'Aircrafts' ('id', 'code', 'manufacturer', 'typeModel', 'wake') VALUES ($id, $code, $manufacturer, $type, $wake);";

                    var command = connection.CreateCommand();
                    command.CommandText = cmdInsert;

                    command.Parameters.AddWithValue("$id", id);
                    command.Parameters.AddWithValue("$code", code);
                    command.Parameters.AddWithValue("$manufacturer", manufacturer);
                    command.Parameters.AddWithValue("$type", type);
                    command.Parameters.AddWithValue("$wake", wake);

                    int result = command.ExecuteNonQuery();
                    _total += result;
                    Console.WriteLine($"Rows Inserted - Result: {result}");

                }

                Console.WriteLine("FIN");

            }

            return _total;

        }

        public static int AddAirlines(string sFileCSV, string sqliteDbPath)
        {
            int _total = -1;
            string[] lines = File.ReadAllLines(sFileCSV);

            foreach (string line in lines)
            {
                _total = 0;
                string[] cols = line.Split(";");

                string id = cols[0].Trim();
                string code = cols[1].Trim();
                string name = cols[2].Trim();

                string countryName = string.Empty;
                int idCountry = 0;

                //FUCK-YOU: NO FUNCIONA CORRECTAMENTE CON LINQ
                //var countryName = name.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                //                        .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                //                        .ToList();

                //EL NOMBRE DEL PAIS SE EXTRAE DE LA NAME, DONDE IMPLICITO 
                Regex regex = new Regex(@"\(([^()]+)\)*");
                foreach (System.Text.RegularExpressions.Match match in regex.Matches(name))
                {
                    countryName = match.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                }


                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    // SE EXTRAE EL IDCOUNTRY MEDIANTE EL NOMBRE PARA AÑADIRLO COMO FK
                    var cmdSelect = @"SELECT id FROM 'main'.'Countries' WHERE name like $countryName;";

                    var command = connection.CreateCommand();
                    command.CommandText = cmdSelect;

                    command.Parameters.AddWithValue("$countryName", countryName);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            idCountry = int.Parse(reader.GetString(0));
                        }
                    }

                    //INSERT
                    var cmdInsert = @"INSERT INTO 'main'.'Airlines' ('id', 'code', 'name', 'idCountry', 'Country') VALUES ($id, $code, $name, $idCountry, $Country);";

                    command.CommandText = cmdInsert;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$id", id);
                    command.Parameters.AddWithValue("$code", code);
                    command.Parameters.AddWithValue("$name", name);
                    command.Parameters.AddWithValue("$idCountry", idCountry);
                    command.Parameters.AddWithValue("$Country", countryName); //SE GUARDA DIRECTAMENTE EL COUNTRY EXTRAIDO DEL NAME PQ EN ALGUNOS CASOS TIENE DIFERENTE ESCRITURA QUE LA BBDD DE COUNTRIES


                    int result = command.ExecuteNonQuery();
                    _total += result;
                    Console.WriteLine($"Rows Inserted - Result: {result}");

                }

                Console.WriteLine($"FIN >> Rows Inserted: {_total}");

            }

            return _total;
        }

        public static int AddAirports(string sFileCSV, string sqliteDbPath)
        {
            int _total = -1;
            string[] lines = File.ReadAllLines(sFileCSV);
            foreach (string line in lines)
            {
                _total = 0;

                string[] cols = line.Split(";");

                string id = cols[0].Trim();
                string code = cols[1].Trim();
                string name = cols[2].Trim();
                string location = cols[3].Trim();
                int idCountry = 0;
                string countryName = location.Split(",")[1].Trim();

                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    // SE EXTRAE EL IDCOUNTRY MEDIANTE EL NOMBRE PARA AÑADIRLO COMO FK
                    var cmdSelect = @"SELECT id FROM 'main'.'Countries' WHERE name like $countryName;";

                    var command = connection.CreateCommand();
                    command.CommandText = cmdSelect;

                    command.Parameters.AddWithValue("$countryName", countryName);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            idCountry = int.Parse(reader.GetString(0));

                            if (countryName.Contains("Rica"))
                            {
                                Console.WriteLine($"CountryId: {idCountry} - {countryName}");
                            }
                        }
                    }

                    //INSERT
                    var cmdInsert = @"INSERT INTO 'main'.'Airports' ('id', 'code', 'name', 'location', 'idCountry', 'country') VALUES ($id, $code, $name, $location, $idCountry, $country);";

                    command.CommandText = cmdInsert;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$id", id);
                    command.Parameters.AddWithValue("$code", code);
                    command.Parameters.AddWithValue("$name", name);
                    command.Parameters.AddWithValue("$location", location);
                    command.Parameters.AddWithValue("$idCountry", idCountry);
                    command.Parameters.AddWithValue("$country", countryName); //SE GUARDA DIRECTAMENTE EL COUNTRY EXTRAIDO DEL NAME PQ EN ALGUNOS CASOS TIENE DIFERENTE ESCRITURA QUE LA BBDD DE COUNTRIES


                    int result = command.ExecuteNonQuery();
                    _total += result;
                    Console.WriteLine($"Rows Inserted - Result: {result}");

                }

                Console.WriteLine($"FIN >> Rows Inserted: {_total}");
            }

            return _total;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            int _result;

            Rutas oRutas = new Rutas();
            CSVtoSQLite oCSV = new CSVtoSQLite();

            List<Classroom> classrooms = CSVtoSQLite.ConvertToList(oRutas.sFilePantallas, oRutas.sPathInputFiles, oRutas.sPathOutputFiles);

            _result = CSVtoSQLite.AddClassroom(classrooms, oRutas.sqliteDbPath);

            
            //_result = oCSV.AddCountries(oRutas.sFileCSV[0], oRutas.sqliteDbPath);
            //_result = oCSV.AddAircrafts(oRutassFileCSV[1], oRutassqliteDbPath);
            //_result = oCSV.AddAirlines(oRutassFileCSV[2], oRutassqliteDbPath);
            //_result = oCSV.AddAirports(oRutassFileCSV[3], oRutassqliteDbPath);

        }
    }
}
