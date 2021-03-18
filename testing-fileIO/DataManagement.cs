using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace testing_fileIO
{
    class DataManagement
    {
        static void Main(string[] args)
        {
            #region VARIABLES
            //COMPROBACION DEL SO PARA LA RUTA DE LOS FICHEROS
            OperatingSystem osInfo = Environment.OSVersion;
            Console.WriteLine($"SO - Platform: {osInfo.Platform}, Version: {osInfo.Version}");

            string winPath = @"C:\GDS\GDS-UCO\testing-fileIO\testing-fileIO\data\";
            string macPath = @"/Users/eortiz/GDS/GDS-UCO/testing-fileIO/testing-fileIO/data/";

            string sPathFiles = (osInfo.Platform.ToString().ToLower().Contains("win") ? winPath : macPath);
      
            string sqliteDbPath = "Data Source=" + sPathFiles + "egds.db";

            string[] sFileCSV = { sPathFiles + "countries.csv", sPathFiles + "aircrafts.csv", sPathFiles + "airlines.csv", sPathFiles + "airports.csv" };
            
            #endregion VARIABLES

            //AddCountries(sFileCSV[0], sqliteDbPath);
            //AddAircrafts(sFileCSV[1], sqliteDbPath);
            AddAirlines(sFileCSV[2], sqliteDbPath);
            //TODO AddAirports(sFileCSV[3], sqliteDbPath);

            #region SQLITE READER TESTING

            //using (var connection = new SqliteConnection(sqliteDbPath))
            //{
            //    connection.Open();
            //    var idCode = "222222";

            //    var command = connection.CreateCommand();
            //    command.CommandText =
            //    @"
            //    SELECT *
            //    FROM Users
            //    WHERE idCode = $idCode                    
            //";
            //    command.Parameters.AddWithValue("$idCode", idCode);


            //    using (var reader = command.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            var name = reader.GetString(1);

            //            Console.WriteLine($"Hello, {name}!");
            //        }
            //    }
            //}

            #endregion 
        }

        static public void AddCountries(string sFileCSV, string sqliteDbPath)
        {
            string[] lines = File.ReadAllLines(sFileCSV);
            foreach (string line in lines)
            {
                string[] cols = line.Split(";");

                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    var id = cols[0].Trim();
                    var code = cols[1].Trim();
                    var name = cols[2].Trim().Replace(".",string.Empty);

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
                    Console.WriteLine($"Rows Inserted - Result: {result}");
                }
            }
        }

        static public void AddAircrafts(string sFileCSV, string sqliteDbPath)
        {
            string[] lines = File.ReadAllLines(sFileCSV);
            foreach (string line in lines)
            {
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
                    Console.WriteLine($"Rows Inserted - Result: {result}");
                    
                }

                Console.WriteLine("FIN");

            }

        }

        static public void AddAirlines(string sFileCSV, string sqliteDbPath)
        {
            int total = 0;
            string[] lines = File.ReadAllLines(sFileCSV);
            foreach (string line in lines)
            {
                string[] cols = line.Split(";");

                var id = cols[0].Trim();
                var code = cols[1].Trim();
                var name = cols[2].Trim();

                //var countryName = name.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                //                        .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                //                        .ToList();

                string countryName = string.Empty;
                int idCountry = 0;

                Regex regex = new Regex(@"\(([^()]+)\)*");
                foreach (Match match in regex.Matches(name))
                {
                    countryName = match.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                }

                if (countryName.Contains("Rica"))
                {
                    Console.WriteLine($"{countryName}");
                }

                using (var connection = new SqliteConnection(sqliteDbPath))
                {
                    connection.Open();
                    
                    var cmdSelect = @"SELECT id FROM 'main'.'Countries' WHERE name like $countryName;";

                    var command = connection.CreateCommand();
                    command.CommandText = cmdSelect;

                    command.Parameters.AddWithValue("$countryName", countryName );

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

                    //EOP:INSERT
                    //var cmdInsert = @"INSERT INTO 'main'.'Airlines' ('id', 'code', 'name', 'idCountry') VALUES ($id, $code, $name, $idCountry);";

                    ////command = connection.CreateCommand();
                    //command.CommandText = cmdInsert;
                    //command.Parameters.Clear();
                    //command.Parameters.AddWithValue("$id", id);
                    //command.Parameters.AddWithValue("$code", code);
                    //command.Parameters.AddWithValue("$name", name);
                    //command.Parameters.AddWithValue("$idCountry", idCountry);

                    //int result = command.ExecuteNonQuery();
                    //total += result;
                    //Console.WriteLine($"Rows Inserted - Result: {result}");

                }

                //Console.WriteLine($"FIN >> Rows Inserted: {total}");

            }
        }
    }

    
}
