using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsCreater
{
    class ScriptLectorCSV
    {
        Acciones a = new Acciones();

        public string selectUnion(string archivo, string[] csv, string ruta, ref string nombrearchivo)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            string fichero;
            string[] cab = new string[0];
            string[] tam = new string[0];
            string[] lineas = new string[0];
            string valores;
            string campo;
            int i = 0;
            int x = 0;

            nombrearchivo = "Select union_" + archivo.ToLower().Replace(".csv", "") + ".sql";

            fichero = ruta + nombrearchivo;
            string dev = a.comprobarficheros(ref lineas, fichero, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);

                    //calculamos tamaño campos
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (i == 0)
                        {
                            Array.Resize(ref tam, j.Length);
                        }
                        else
                        {
                            x = 0;
                            foreach (string c in j)
                            {
                                if (Convert.ToInt32(tam[x]) < c.Length)
                                {
                                    tam[x] = c.Length.ToString();
                                }
                                x++;
                            }
                        }
                        i++;
                    }

                    i = 0;
                    //Guardamos los registros
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        //Primera linea es cabecera, se guarda en un array
                        if (i == 0)
                        {
                            cab = j;
                        }
                        else
                        {
                            valores = "";
                            x = 0;
                            foreach (string c in j)
                            {
                                campo = String.Format("{0,-" + tam[x] + "}", c);
                                valores = valores + "'" + campo + "' as " + cab[x].ToString() + ", ";
                                x++;
                            }
                            valores = valores.Substring(0, valores.Length - 2);
                            file.WriteLine("SELECT " + valores);

                            if (i + 1 < csv.Length)
                            {
                                file.WriteLine("UNION ALL ");
                            }
                        }
                        i++;
                    }
                    file.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo " + nombrearchivo + "\r\n" + ex.Message, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return "NO";
                }
                return "OK";
            }
            else
            {
                MessageBox.Show("No se ha podido generar el fichero " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }
        }
    }
}
