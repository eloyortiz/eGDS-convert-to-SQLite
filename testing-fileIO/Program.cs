using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            string sPathFiles = @"/Users/eortiz/GDS/GDS-UCO/testing-fileIO/testing-fileIO/data/";
            string sPathInputFiles = @sPathFiles + "_PANTALLAS/";
            string sPathOutoutFiles = @sPathFiles + "_output/";
            string sFilename = "pantallas.txt";

            
            List<Pantalla> Listado = new List<Pantalla>();

            #region TESTING
            //// Example #1
            //// Read the file as one string.
            //string text = System.IO.File.ReadAllText( sPathFile + sFilename) ;

            //// Display the file contents to the console. Variable text is a string.
            //System.Console.WriteLine("Contents of {0} = {1}", text, sFilename);

            //Console.ReadKey();

            // Example #2
            // Read each line of the file into a string array. Each element
            // of the array is one line of the file.
            #endregion

            string[] pantallas = File.ReadAllLines(sPathFiles + sFilename);
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

                            string json = JsonConvert.SerializeObject(Listado, Formatting.Indented);
                            Console.WriteLine(json);

                            WriteCSV(Listado, sPathOutoutFiles + pantalla + ".csv");

                           
                            break;

                        default:
                            break;
                    }

                    
                }

              

            }


            //string json = JsonConvert.SerializeObject(Listado, Formatting.Indented);
            //Console.WriteLine(json);

            //WriteCSV(Listado, sPathOutoutFiles + "pantallas.csv");

            ////LISTADO DE PANTALLAS FORMADO
            //foreach ( Pantalla pantalla in Listado)
            //{
            //    string nuevo = pantalla.Index.ToString() + ".txt";

            //    //ESCRIBE FICHERO
            //    //Task writeContent = WriteLines(sPathOutoutFiles + nuevo, lines);


                
            //}

            //// Display the file contents by using a foreach loop.
            //Console.WriteLine("Contents of {0} = ", sFilename);
            //foreach (string line in lines)
            //{
            //    // Use a tab to indent each line of the file.
            //    Console.WriteLine("\t" + line);
            //    //_ = WriteText(sPathFile, line);
            //}

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();

        }

        public static async Task WriteText(string sPathFileName, string sText)
        {
            await File.WriteAllTextAsync(sPathFileName, sText);
        }

        public static async Task WriteLines(string sPathFileName, string[] sLines)
        {
            await File.WriteAllLinesAsync(sPathFileName, sLines);
        }

        public static void WriteCSV<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name);

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(", ", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(", ", props.Select(p => p.GetValue(item, null))));
                }
            }
        }

    }
}
