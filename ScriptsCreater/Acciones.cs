using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace ScriptsCreater
{
    class Acciones
    {
        public string version = "1.0.7";

        public string comprobarficheros(ref string[] lineds, string ruta, string nombrearchivo, int accion)
        {
            string nombrefic;
            int counter = 0;
            string line;
            nombrefic = ruta + nombrearchivo;

            try
            {
                StreamReader file = new StreamReader(nombrefic);
                while ((line = file.ReadLine()) != null)
                {
                    Array.Resize(ref lineds, lineds.Length + 1);
                    lineds[counter] = line;
                    counter++;

                }

                file.Close();

                //Accion: 1 se renombra, 0 no se hace nada
                if (accion == 1)
                {
                    string fecha = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0');
                    File.Move(nombrefic, nombrefic.Replace(".sql", " Copia_" + fecha + ".sql"));

                }

            }
            catch (Exception ex)
            {
                return "NO";
            }
            return "OK";
        }

        public string borrarfichero(string ruta, string nombrearchivo)
        {
            try
            {
                File.Delete(ruta + nombrearchivo.Replace(".sql", " Copia.sql"));
            }
            catch (Exception ex)
            {
                return "NO";
            }
            return "OK";
        }

        public DataTable dtCSV(string[] csvData, int lineaCabecera, Boolean filtradoColumn)
        {
            DataTable csvDataTable = new DataTable();
            char separator = ';';
            bool isRowOneHeader = true;
            string nombrecolumna;
            int intRowIndex = 0;
            string[] headings;

            if (lineaCabecera == 0)
            {
                isRowOneHeader = false;
                lineaCabecera++;
            }

            if (csvData.Length > 0)
            {
                headings = csvData[lineaCabecera - 1].Split(separator);
                //Se la primera linea contiene o no las columnas
                if (isRowOneHeader)
                {
                    intRowIndex = lineaCabecera - 1;

                    for (int i = 0; i < headings.Length; i++)
                    {
                        nombrecolumna = headings[i].ToString();
                        if (filtradoColumn == true)
                        {
                            nombrecolumna = nombrecolumna.ToLower().Replace(" ", "").Replace("?", "").Replace("#", "");
                        }
                        //Se añade el nombre columnas a la tabla
                        csvDataTable.Columns.Add(nombrecolumna);
                    }

                    intRowIndex++;
                }
                //Si no hay cabecera, se añade columnas como "Columna1", "Columna2", etc.
                else
                {
                    for (int i = 0; i < headings.Length; i++)
                    {
                        csvDataTable.Columns.Add("Columna" + (i + 1).ToString());
                    }
                }

                //Resto de valores se añanden a la tabla
                for (int i = intRowIndex; i < csvData.Length; i++)
                {
                    //Crea una nueva linea
                    DataRow row = csvDataTable.NewRow();

                    for (int j = 0; j < headings.Length; j++)
                    {
                        //Añade los valores de cada columna
                        row[j] = csvData[i].Split(separator)[j];
                    }

                    //Añade la linea en DataTable
                    csvDataTable.Rows.Add(row);
                }
            }
            return csvDataTable;
        }

        public string[] ordenarCSV(string[] csvData)
        {
            string[] csv = new string[0];
            DataTable dt;
            int cab = 0;
            string valordatos = "";

            foreach (string d in csvData)
            {
                string[] j = d.Split(new Char[] { ';' });
                if (j[0].Contains("#"))
                {
                    cab++;
                    Array.Resize(ref csv, csv.Length + 1);
                    csv[csv.Length - 1] = d;
                }
                else
                {
                    break;
                }
            }
            
            if (cab == 0)
            {
                return csvData;
            }
            else
            {
                dt = dtCSV(csvData, cab, true);

                foreach (DataRow dr in dt.Rows)
                {
                    valordatos = "";

                    valordatos = dr.ItemArray[dt.Columns.IndexOf("campo")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("tipo")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("tablasid")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("sid")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("clave")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("dim")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("campocruce")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("siddim")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("autosid")].ToString() + ";" +
                        dr.ItemArray[dt.Columns.IndexOf("filter")].ToString();

                    Array.Resize(ref csv, csv.Length + 1);
                    csv[csv.Length - 1] = valordatos;
                }

                return csv;
            }
        }

        public DataTable valorQuery(string[] fichero, string[] csv, string tipo, Boolean inc, string tab)
        {
            DataTable dtfic = new DataTable();
            dtfic.Columns.Add("linSript", typeof(String));
            dtfic.Columns.Add("codScript", typeof(int));
            dtfic.Columns.Add("orden", typeof(int));

            DataTable dtini = new DataTable();
            dtini.Columns.Add("linSript", typeof(String));
            dtini.Columns.Add("codScript", typeof(int));
            dtini.Columns.Add("orden", typeof(int));

            DataTable dtfinal = new DataTable();
            dtfinal.Columns.Add("linSript", typeof(String));
            dtfinal.Columns.Add("codScript", typeof(int));
            dtfinal.Columns.Add("orden", typeof(int));

            DataRow[] dr;
            int i = 0;
            int orden = 0;
            int lon = 0;
            int cods = 0;
            string etiqueta = "";
            string lineaAnt = "";
            int valorIni = 0;
            string nombreTabTemp = "";

            //Leyendo de un script previo
            if (fichero.Length > 0)
            {
                foreach (string lin in fichero)
                {
                    //Iniciamos el almacenamiento por registros que se van a mantener
                    if (lin == "--- Business Logic Start" && lon == 0)
                    {
                        lon = 1;
                        cods++;
                        etiqueta = "--- Business Logic End";
                    }
                    else if (lin == "--- Inicio Bloque común para Incremental y Full" && lon == 0)
                    {
                        lon = 1;
                        cods++;
                        etiqueta = "--- Fin Bloque común para Incremental y Full";
                    }
                    else if (lin.ToLower().Contains(";with") && lon == 0)
                    {
                        //En este caso, si encontramos la linea de with sin estar grabando registros, grabamos un nuevo registro 
                        //puesto que es el comienzo de nuevo bloque en generador antiguo
                        lon = 1;
                        cods++;
                        dtfic.Rows.Add("--- Business Logic Start", cods, orden++);
                        if (lineaAnt.Contains("--"))
                        {
                            dtfic.Rows.Add(lineaAnt, cods, orden++);
                        }
                        dtfic.Rows.Add("", cods, orden++);
                        etiqueta = "--- Business Logic End";
                    }

                    //Finalizamos almacenamiento de los registros que se van a mantener
                    else if (lin == "--- Business Logic End" && lon == 1)
                    {
                        //Generamos la última linea de Query y cerramos la grabación
                        dtfic.Rows.Add(lin, cods, orden++);
                        lon = 0;
                    }
                    else if (lin == "--- Fin Bloque común para Incremental y Full" && lon == 1)
                    {
                        //Generamos la última linea de Query y cerramos la grabación
                        dtfic.Rows.Add(lin, cods, orden++);
                        lon = 0;
                    }
                    else if (lin.ToLower().Contains("insert into #tmp") && lon == 1)
                    {
                        //En este caso, si encontramos la linea de insert en la tabla Temporal
                        //Generamos la última linea de Query y cerramos la grabación
                        dtfic.Rows.Add(etiqueta, cods, orden++);
                        lon = 0;
                    }

                    if (lon == 1)
                    {
                        if (lin.ToLower().Contains("#tmp_keys_"))
                        {
                            valorIni = lin.ToLower().IndexOf("#tmp_keys");
                            nombreTabTemp = lin.Substring(valorIni, lin.IndexOf(" ", valorIni) - valorIni);
                            dtfic.Rows.Add(lin.Replace(nombreTabTemp, "#tmp_keys_" + tab), cods, orden++);
                        }
                        else
                        {
                            dtfic.Rows.Add(lin, cods, orden++);
                        }
                    }
                    lineaAnt = lin;
                }
            }

            //Montando registros tipos para Script nuevos
            if (tipo == "maestro")
            {
                dtini.Rows.Add("--- Business Logic Start", 1, orden++);
                dtini.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 1, orden++);
                dtini.Rows.Add("", 1, orden++);
                dtini.Rows.Add("        ;WITH", 1, orden++);
                dtini.Rows.Add("        query AS (", 1, orden++);
                dtini.Rows.Add("SELECT", 1, orden++);
                //Montamos los campos del Select
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            dtini.Rows.Add("    CAST(NULL AS " + j[1].ToString() + ") AS " + j[0].ToString(), 1, orden++);
                        }
                        else
                        {
                            dtini.Rows.Add("    CAST(NULL AS " + j[1].ToString() + ") AS " + j[0].ToString() + ",", 1, orden++);
                        }
                    }
                }

                dtini.Rows.Add("--FROM table t", 1, orden++);
                dtini.Rows.Add("WHERE 1 = 0", 1, orden++);
                dtini.Rows.Add("", 1, orden++);
                dtini.Rows.Add("        )", 1, orden++);
                dtini.Rows.Add("--- Business Logic End", 1, orden++);
            }
            else if (tipo == "ds")
            {
                //Bloque Comun Incremental y Full
                dtini.Rows.Add("--- Inicio Bloque común para Incremental y Full", 1, orden++);
                dtini.Rows.Add("", 1, orden++);
                dtini.Rows.Add("--- Fin Bloque común para Incremental y Full", 1, orden++);

                //Bloque Datos
                //--------------Bloque Incremental PK
                dtini.Rows.Add("--- Business Logic Start", 2, orden++);
                dtini.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 2, orden++);
                dtini.Rows.Add("", 2, orden++);
                dtini.Rows.Add("        ;WITH", 2, orden++);
                dtini.Rows.Add("        query AS (", 2, orden++);
                dtini.Rows.Add("            SELECT null", 2, orden++);
                dtini.Rows.Add("       )", 2, orden++);
                dtini.Rows.Add("--- Business Logic End", 2, orden++);
                //---------------Bloque Incremental Datos
                dtini.Rows.Add("--- Business Logic Start", 3, orden++);
                dtini.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 3, orden++);
                dtini.Rows.Add("", 3, orden++);
                dtini.Rows.Add("        ;WITH", 3, orden++);
                dtini.Rows.Add("        query AS (", 3, orden++);
                dtini.Rows.Add("            SELECT null", 3, orden++);
                dtini.Rows.Add("       )", 3, orden++);
                dtini.Rows.Add("--- Business Logic End", 3, orden++);
                //---------------Bloque Full
                dtini.Rows.Add("--- Business Logic Start", 4, orden++);
                dtini.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 4, orden++);
                dtini.Rows.Add("", 4, orden++);
                dtini.Rows.Add("        ;WITH", 4, orden++);
                dtini.Rows.Add("        query AS (", 4, orden++);
                dtini.Rows.Add("            SELECT null", 4, orden++);
                dtini.Rows.Add("       )", 4, orden++);
                dtini.Rows.Add("--- Business Logic End", 4, orden++);
            }
            else
            {
                dtini.Rows.Add("--- Business Logic End", 1, orden++);
                dtini.Rows.Add("--Lo que haya entre Query se respeta si se regenera el Script", 1, orden++);
                dtini.Rows.Add("", 1, orden++);
                dtini.Rows.Add("        ;WITH", 1, orden++);
                dtini.Rows.Add("        query AS (", 1, orden++);
                dtini.Rows.Add("            SELECT null", 1, orden++);
                dtini.Rows.Add("        )", 1, orden++);
                dtini.Rows.Add("--- Business Logic End", 1, orden++);
            }

            try
            {
                //Pasamos datos al DT Final
                //Para el caso DS
                if (tipo == "ds")
                {
                    //Carga Incremental
                    if (inc == true)
                    {
                        //Si el DT fichero tiene datos, comprobamos de donde proceden
                        if (dtfic.Rows.Count > 0)
                        {
                            //Si eran cargas Full/Incremental, pasamos el fichero tal cual
                            if (dtfic.Select("codScript = 4", "codScript ASC, orden ASC").Length > 0)
                            {
                                dtfinal = dtfic;
                            }
                            //En caso contrario, tendremos que cargar todos los registros iniciales excepto el perteneciente a full
                            else
                            {
                                //Datos carga Inicial
                                dr = null;
                                dr = dtini.Select("codScript < 4", "codScript ASC, orden ASC");
                                foreach (DataRow l2 in dr)
                                {
                                    dtfinal.Rows.Add(l2.ItemArray[0].ToString(), l2.ItemArray[1].ToString(), l2.ItemArray[2].ToString());
                                }
                                //Datos carga Full
                                dr = null;
                                dr = dtfic.Select("codScript = 1", "codScript ASC, orden ASC");
                                foreach (DataRow l2 in dr)
                                {
                                    dtfinal.Rows.Add(l2.ItemArray[0].ToString(), 4, l2.ItemArray[2].ToString());
                                }
                            }
                        }
                        //Si no tiene datos Pasamos el DT inicial
                        else
                        {
                            dtfinal = dtini;
                        }
                    }
                    //Carga Full
                    else
                    {
                        //Comprobamos si tiene valores el DT de ficheros
                        if (dtfic.Rows.Count > 0)
                        {
                            //Si viene de incremental, solo pasamos el registro full
                            if (dtfic.Select("codScript = 4", "codScript ASC, orden ASC").Length > 0)
                            {
                                dr = null;
                                dr = dtfic.Select("codScript = 4", "codScript ASC, orden ASC");
                                foreach (DataRow l2 in dr)
                                {
                                    dtfinal.Rows.Add(l2.ItemArray[0].ToString(), 1, l2.ItemArray[2].ToString());
                                }
                            }
                            //Si no, pasamos todo el fichero
                            else
                            {
                                dtfinal = dtfic;
                            }
                        }
                        //Si no tiene valores el DT de ficheros, pasamos el valor Full de DT inicial
                        else
                        {
                            dr = null;
                            dr = dtini.Select("codScript = 4", "codScript ASC, orden ASC");
                            foreach (DataRow l2 in dr)
                            {
                                dtfinal.Rows.Add(l2.ItemArray[0].ToString(), 1, l2.ItemArray[2].ToString());
                            }
                        }
                    }
                }
                //Resto de casos
                else
                {
                    //Comprobamos si tiene datos DT fichero para pasarlo al DT final
                    if (dtfic.Rows.Count > 0)
                    {
                        dtfinal = dtfic;
                    }
                    //En caso contrario, pasamos el DT inicial al DT final
                    else
                    {
                        dtfinal = dtini;
                    }
                }
            }
            catch (Exception ex)
            { }

            //Retornamos el DT final
            return dtfinal;
        }

        public string generar_file_exec(StreamWriter file_exec, string tabla, string sp_bd, string sp_sch, string sp, Boolean incremental)
        {
            if (incremental == true)
            {
                file_exec.WriteLine("-------------------------------------------------------------------");
                file_exec.WriteLine("--Comprueba tipo de carga");
                file_exec.WriteLine("SELECT es_carga_completa FROM dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "'");
                file_exec.WriteLine("");
                file_exec.WriteLine("--Carga Full");
                file_exec.WriteLine("--UPDATE dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro SET es_carga_completa = 1 where  bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "'");
                file_exec.WriteLine("--Carga Incremental");
                file_exec.WriteLine("--UPDATE dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro SET es_carga_completa = 0 where  bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "'");
                file_exec.WriteLine("");
            }
            file_exec.WriteLine("-------------------------------------------------------------------");
            file_exec.WriteLine("--Ver estado Precondiciones");
            file_exec.WriteLine("SELECT estado_precondicion FROM dbn1_norm_dhyf.audit.tbn1_precondiciones_carga_dwh WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "'");
            file_exec.WriteLine("");
            file_exec.WriteLine("-------------------------------------------------------------------");
            file_exec.WriteLine("--Pasos de Ejecución");
            file_exec.WriteLine("SELECT COUNT(1) FROM " + tabla);
            file_exec.WriteLine("");
            file_exec.WriteLine("EXEC " + sp_bd + "." + sp_sch + "." + sp + " NULL");
            file_exec.WriteLine("GO");
            file_exec.WriteLine("SELECT COUNT(1) FROM " + tabla);
            file_exec.WriteLine("SELECT TOP 1 * FROM dbn1_norm_dhyf.audit.tbn1_logs_carga_dwh WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "' order by id desc");
            file_exec.WriteLine("");
            file_exec.WriteLine("EXEC " + sp_bd + "." + sp_sch + "." + sp + " NULL");
            file_exec.WriteLine("GO");
            file_exec.WriteLine("SELECT COUNT(1) FROM " + tabla);
            file_exec.WriteLine("SELECT TOP 1 * FROM dbn1_norm_dhyf.audit.tbn1_logs_carga_dwh WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "' order by id desc");
            file_exec.WriteLine("");
            file_exec.WriteLine("SELECT TOP 10000 * FROM " + tabla);
            file_exec.WriteLine("GO");
            file_exec.WriteLine("");

            return "OK";
        }

        public string[] leerCSV(string archivo, string ruta)
        {
            string[] csv = new string[0];
            string line;
            int i = 0;

            //Pasamos valor CSV a variables
            try
            {
                StreamReader file = new StreamReader(ruta + archivo);
                while ((line = file.ReadLine()) != null)
                {
                    Array.Resize(ref csv, csv.Length + 1);
                    csv[i] = line;
                    i++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el archivo " + archivo + "\n\r" + ex.Message, "Error abrir fichero", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Array.Resize(ref csv, 0);
            }
            return csv;
        }
    }
}
