using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsCreator
{
    class ScriptGen_MF_STG
    {
        Acciones a = new Acciones();
        Consultas c = new Consultas();
        ScriptComun sc = new ScriptComun();


        public int ExisteTabla(string table, string schema, ref string bd)
        {

            try
            {
                if (bd.ToString() == "")
                {
                    bd = "DB" + table.Substring(2, table.Length - 2);
                }

                string cadena = a.cadena(bd);
                DataTable dt = a.conexion(cadena, c.ComprobarTabla(table, schema));

                return Convert.ToInt16(dt.Rows[0].ItemArray[0]);
            }
            catch //(Exception ex)
            {
                return 0;
            }
        }
        


        public string createtable_stgFinal(string table, string schema, string ruta, ref string nombrearchivo, ref string camposPK, string bd, ref int activoCT)
        {
            activoCT = 1;

            string cadena = a.cadena(bd);
            string[] lineas = new string[0];
            string valorcampo = "";
            int valorpk = 0;

            string error = "";

            string camposPK1 = "";
            string camposPK2 = "";
            string camposPK3 = "";
            
            DataTable datosColumnas = a.conexion(cadena, c.Columns(table));
            if (datosColumnas.Rows.Count > 0)
            {
                //DataTable datosClaves = a.conexion(cadena, "EXEC sp_helpindex N'" + schema + "." + table + "'  ");
                DataTable datosClaves = a.conexion(cadena, c.ColumnsClaves(schema + "." + table));
                DataTable datosExtended = a.conexion(cadena, c.PropiedadesExtendidas(schema + "", table));

                //MONTAR SCRIPTS
                //table.Substring(2, table.Length - 2)
                string nombretab = "TBN1" + table.Substring(4, table.Length - 4) + "_" + table.Substring(2, 2);
                string fecar = componer_fecar(table, datosColumnas);
                nombrearchivo = "nnn. " + nombretab + ".sql";
                string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

                if (a.comprobarDir(ruta) == "OK")
                {
                    try
                    {
                        StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.CreateNew), Encoding.UTF8);

                        file.WriteLine("PRINT '" + nombretab + "'");
                        file.WriteLine("GO");
                        file.WriteLine("");
                        file.WriteLine("--Generado versión vb " + a.version);
                        file.WriteLine("");
                        file.WriteLine("USE dbn1_stg_dhyf");
                        file.WriteLine("GO");
                        file.WriteLine("SET ANSI_NULLS ON");
                        file.WriteLine("GO");
                        file.WriteLine("SET QUOTED_IDENTIFIER ON");
                        file.WriteLine("GO");
                        file.WriteLine("SET ANSI_PADDING ON");
                        file.WriteLine("GO");
                        file.WriteLine("");
                        //Borra y crea tabla
                        file.WriteLine("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='" + nombretab + "')");
                        file.WriteLine("\tDROP TABLE dbo." + nombretab);
                        file.WriteLine("GO");
                        file.WriteLine("");
                        file.WriteLine("");
                        file.WriteLine("CREATE TABLE dbo." + nombretab + " (");
                        foreach (DataRow dr in datosColumnas.Rows)
                        {
                            if (dr.ItemArray[2].ToString().ToLower().Contains("char"))
                            {
                                if (dr.ItemArray[3].ToString() == "-1")
                                {
                                    valorcampo = " varchar(max) not null";
                                }
                                else
                                {
                                    valorcampo = " varchar(" + dr.ItemArray[3].ToString() + ") not null";
                                }
                            }
                            else if (dr.ItemArray[2].ToString().ToLower() == "numeric" || dr.ItemArray[2].ToString().ToLower() == "decimal")
                            {
                                valorcampo = " " + dr.ItemArray[2].ToString().ToLower() + "(" + dr.ItemArray[4].ToString() + ", " + dr.ItemArray[5].ToString() + ") not null";
                            }
                            else
                            {
                                valorcampo = " " + dr.ItemArray[2].ToString().ToLower() + " not null";
                            }

                        file.WriteLine("\t" + dr.ItemArray[0].ToString() + valorcampo + ",");
                        }
                        file.WriteLine("\t" + fecar + " date not null");
                        file.WriteLine(") ON [PRIMARY]");
                        file.WriteLine("WITH (DATA_COMPRESSION=PAGE)");
                        file.WriteLine("GO");
                        file.WriteLine("");

                        //PK
                        camposPK = "";
                        camposPK1 = "";
                        camposPK2 = "";
                        camposPK3 = "";
                        foreach (DataRow dr in datosClaves.Rows)
                        {
                            //Si es primary Key
                            if (Convert.ToBoolean(dr.ItemArray[4]) == true)
                            {
                                camposPK1 = camposPK1 + dr.ItemArray[9].ToString() + ", ";
                            }
                            //Si es cluster e index único
                            else if (Convert.ToBoolean(dr.ItemArray[3]) == true && dr.ItemArray[2].ToString().ToUpper() == "CLUSTERED")
                            {
                                camposPK2 = camposPK2 + dr.ItemArray[9].ToString() + ", ";
                            }
                            //Si es index único
                            else if (Convert.ToBoolean(dr.ItemArray[3]) == true)
                            {
                                if (valorpk == 0)
                                {
                                    camposPK3 = camposPK3 + dr.ItemArray[9].ToString() + ", ";
                                    valorpk = Convert.ToInt32(dr.ItemArray[6]);
                                }
                                else if (valorpk == Convert.ToInt32(dr.ItemArray[6]))
                                {
                                    camposPK3 = camposPK3 + dr.ItemArray[9].ToString() + ", ";
                                }
                            }
                        }

                        if (camposPK1.Length == 0 && camposPK2.Length == 0 && camposPK3.Length == 0)
                        {
                            file.WriteLine("/*-------------------------*/");
                            file.WriteLine("/*------PROBLEMAS PK-------*/");
                            file.WriteLine("/*-------------------------*/");
                            file.WriteLine("");
                            error = error + "\n\r//** " + table + " PROBLEMAS PK**\\" + "\n\r";
                        }
                        else
                        {
                            if (camposPK1 != "")
                            {
                                camposPK = camposPK1;
                                error = error + "\n\r//** " + table + " PK generada sobre Primary Key**\\" + "\n\r";
                            }
                            else if (camposPK2 != "")
                            {
                                camposPK = camposPK2;
                                error = error + "\n\r//** " + table + " PK generada sobre Cluster e Index único**\\" + "\n\r";
                            }
                            else if (camposPK3 != "")
                            {
                                camposPK = camposPK3;
                                error = error + "\n\r//** " + table + " PK generada sobre Index único**\\" + "\n\r";
                            }
                            camposPK = camposPK.Substring(0, camposPK.Length - 2);
                            file.WriteLine("ALTER TABLE dbo." + nombretab + " ADD CONSTRAINT " + nombretab + "_PK PRIMARY KEY (" + camposPK + ")");
                            file.WriteLine("GO");
                            file.WriteLine("");
                        }

                        //Propiedades Extendidas
                        sc.AnaydirPropiedadesExtendidas(file, nombretab, datosColumnas, fecar, datosExtended);

                        //Activar CT
                        file.WriteLine("--Activar CT");
                        file.WriteLine("IF NOT EXISTS(SELECT 1 FROM sys.change_tracking_tables tt INNER JOIN sys.objects obj ON obj.object_id = tt.object_id WHERE obj.name = '" + nombretab + "')");
                        file.WriteLine("\tALTER TABLE dbo." + nombretab + " ENABLE CHANGE_TRACKING WITH(TRACK_COLUMNS_UPDATED = ON)");

                        file.WriteLine("");
                        file.WriteLine("SET ANSI_PADDING OFF");
                        file.WriteLine("GO");

                        file.Close();

                        //Comprueba si tiene CT Activo
                        DataTable datosCT = a.conexion(cadena, c.ChangeTrackingActivo(table));
                        if (datosCT.Rows.Count == 0)
                        {
                            error = error + "\n\r//** " + table + " No tiene CHANGE_TRACKING ACTIVO**\\" + "\n\r";
                            activoCT = 0;
                        }
                    }

                    catch //(Exception ex)
                    {
                        MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                        return "NO";
                    }

                    nombrearchivo = error + "\n\r" + nombrearchivo;
                    return "OK";
                }
                else
                {
                    MessageBox.Show("No se ha podido generar el fichero " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "NO";
                }
            }
            else
            {
                MessageBox.Show("No se ha podido generar el fichero " + nombrearchivo + " porque no encuentra datos para la tabla indicada", "Error ruta fichero", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "NO_R";
            }
        }

        //pendiente de implemetar
        public string createtable_stgFinal(string csv, string ruta, ref string nombreArchivo, string rutaDestino = "")
        {
            return "OK";
        }

        public string gencsv(string table, string schema, string ruta, ref string nombrearchivo, string camposPK, string bd)
        {
            string cadena = a.cadena(bd);
            string[] lineas = new string[0];
            string valorcampo = "";

            string csvRow;

            DataTable datosColumnas = a.conexion(cadena, c.Columns(table));
            DataTable datosClaves = a.conexion(cadena, c.ColumnsClaves("" + schema + "." + table));
            DataTable datosTabla = a.conexion(cadena, c.ComentarioTabla(table));
            string comentarioTabla = datosTabla.Rows[0].ItemArray[0].ToString();

            //MONTAR SCRIPTS
            string nombretab = "TBN1" + table.Substring(4, table.Length - 4) + "_" + table.Substring(2, 2);
            string fecar = componer_fecar(table, datosColumnas);
            nombrearchivo = nombretab + ".csv";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    csvRow = string.Format("{0};{1};{2};{3}", "#tabla", nombretab, "dbn1_stg_dhyf", comentarioTabla);
                    file.WriteLine(csvRow);
                    csvRow = string.Format("{0};{1};{2};{3}", "#columna", "Tipo", "Clave?", "Comentario");
                    file.WriteLine(csvRow);

                    foreach (DataRow dr in datosColumnas.Rows)
                    {
                        if (dr.ItemArray[2].ToString().ToLower().Contains("char"))
                        {
                            if (dr.ItemArray[3].ToString() == "-1")
                            {
                                valorcampo = " varchar(max)";
                            }
                            else
                            {
                                valorcampo = " varchar(" + dr.ItemArray[3].ToString() + ")";
                            }
                        }
                        else if (dr.ItemArray[2].ToString().ToLower() == "numeric" || dr.ItemArray[2].ToString().ToLower() == "decimal")
                        {
                            valorcampo = dr.ItemArray[2].ToString().ToLower() + "(" + dr.ItemArray[4].ToString() + ", " + dr.ItemArray[5].ToString() + ") ";
                        }
                        else
                        {
                            valorcampo = dr.ItemArray[2].ToString().ToLower() + "";
                        }

                        //Contains puede encontrar campos cuyo nombre sea un substring de otro campo que sí sea PK
                        //Para eviar errores, buscar también por el separador ",". Para ello, crear un nuevo string con otra "," al final
                        string camposPkConComaAlFinal = camposPK + ",";
                        if (camposPkConComaAlFinal.Contains(dr.ItemArray[0].ToString() + ","))
                        {
                            csvRow = string.Format("{0};{1};{2};{3}", dr.ItemArray[0].ToString(), valorcampo, "#", dr.ItemArray[7].ToString());
                            file.WriteLine(csvRow);
                        }
                        else
                        {
                            csvRow = string.Format("{0};{1};{2};{3}", dr.ItemArray[0].ToString(), valorcampo, "", dr.ItemArray[7].ToString());
                            file.WriteLine(csvRow);
                        }
                    }

                    csvRow = string.Format("{0};{1};{2};{3}", fecar, "date", "", "Fecha última carga");
                    file.WriteLine(csvRow);

                    file.Close();
                }
                catch //(Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo CSV " + nombrearchivo, "Error escritura archivo CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "NO";
                }

                return "OK";
            }
            else
            {
                MessageBox.Show("No se ha podido generar el fichero CSV " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "NO";
            }
        }

        public string createtable_extraccion(string csv, string ruta, ref string nombrearchivo, string rutaDestino = "")
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string campos = "";
            string camposPK = "";
            string bd = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            valorescsv = a.ordenarCSV(valorescsv,"extraccion");

            foreach (string v in valorescsv)
            {
                string[] datos = v.Split(new Char[] { ';' });
                if (v.ToLower().Contains("#tabla"))
                {
                    nombretab = datos[1];
                    bd = datos[2];
                }
                if (!datos[0].Contains("#"))
                {
                    campos = campos + "'" + datos[0] + "', ";
                    if (datos[2].Contains("#"))
                    { camposPK = camposPK + datos[0] + ", "; }
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            if (camposPK.Length > 0)
            {
                camposPK = camposPK.Substring(0, camposPK.Length - 2);
            }

            nombrearchivo = "Script " + nombretab + "_tmp.sql";

            //Si la ruta destino de los ficheros no viene informada, cogemos la misma que la ruta del csv origen
            if (rutaDestino == "")
            {
                rutaDestino = ruta;
            }

            string dev = a.comprobarficheros(ref lineas, rutaDestino + nombrearchivo, 1);

            if (a.comprobarDir(rutaDestino) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(rutaDestino + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");

                    //Create Table
                    file.WriteLine("");

                    sc.regTablas(file, "dbn1_stg_dhyf", "extracciones_tmp", nombretab + "_tmp", "", campos, camposPK, valorescsv, false, "extraccion");

                    file.Close();
                }
                catch //(Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return "NO";
                }

                return "OK";
            }
            else
            {
                MessageBox.Show("No se ha podido generar el fichero " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "NO";
            }
        }

        public string createSP_extraccion(string csv, string ruta, ref string nombrearchivo, string rutaDestino = "")
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string bd = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            valorescsv = a.ordenarCSV(valorescsv,"extraccion");
            string schema = "extracciones_tmp";
            string campos = "";
            string camposPK= "";
            int i = 0;

            foreach (string v in valorescsv)
            {
                string[] datos = v.Split(new Char[] { ';' });
                if (datos[0].ToLower().Contains("#tabla"))
                {
                    nombretab = datos[1];
                    bd = datos[2];
                }
                else if (!datos[0].Contains("#"))
                {
                    campos = campos + datos[0].ToString() + ",";
                    if (datos[2].Contains("#"))
                    {
                        camposPK = camposPK + datos[0].ToString() + ",";
                    }
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            camposPK = camposPK.Substring(0, camposPK.Length - 1);
            string[] campos2 = campos.Split(',');
            string[] camposPK2 = camposPK.Split(',');

            nombrearchivo = "Script " + "SPN1Cargar" +  nombretab + ".sql";

            //Si la ruta destino de los ficheros no viene informada, cogemos la misma que la ruta del csv origen
            if (rutaDestino == "")
            {
                rutaDestino = ruta;
            }
            string dev = a.comprobarficheros(ref lineas, rutaDestino + nombrearchivo, 1);

            if (a.comprobarDir(rutaDestino) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(rutaDestino + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("SET QUOTED_IDENTIFIER ON");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");


                    //Cambiamos a otra BBDD y empezamos la nueva tarea
                    file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_stg_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='" + schema + "' AND ROUTINE_NAME='SPN1Cargar" + nombretab + "' AND ROUTINE_TYPE='PROCEDURE')");
                    file.WriteLine("\tDROP PROCEDURE " + schema + ".SPN1Cargar" + nombretab);
                    file.WriteLine("GO");
                    file.WriteLine("");
                    //Generamos el SP
                    file.WriteLine("CREATE PROCEDURE " + schema + ".SPN1Cargar" + nombretab + " AS");
                    file.WriteLine("BEGIN");
                    file.WriteLine("");
                    file.WriteLine("\tSET NOCOUNT ON");
                    file.WriteLine("");
                    file.WriteLine("\tBEGIN TRY");
                    file.WriteLine("");

                    file.WriteLine("\t\t--ACTUALIZACIONES");
                    file.WriteLine("\t\t;WITH actualizaciones AS");
                    file.WriteLine("\t\t(");
                    file.WriteLine("\t\t\tSELECT\t" + campos);
                    file.WriteLine("\t\t\tFROM\t" + schema + "." + nombretab + "_tmp");
                    file.WriteLine("\t\t\tWHERE\tSYS_CHANGE_OPERATION IN ('U','X')");
                    file.WriteLine("\t\t)");
                    file.WriteLine("\t\tUPDATE\tdestino");
                    i = 0;
                    int escribirSet = 1;
                    foreach (string c in campos2)
                    {
                        i++;
                        if (!camposPK2.Contains(c))
                        {

                            if (escribirSet == 1)
                            {
                                file.WriteLine("\t\tSET\t\tdestino." + c + " = actu." + c + ",");
                                escribirSet = 0;
                            }
                            else if (campos2.Length == i)
                        {
                                file.WriteLine("\t\t\t\tdestino." + c + " = actu." + c);
                        }
                        else
                        {
                                file.WriteLine("\t\t\t\tdestino." + c + " = actu." + c + ",");
                        }
                    }

                    }
                    file.WriteLine("\t\tFROM\tdbo." + nombretab + " destino");
                    file.WriteLine("\t\t\t\tINNER JOIN  actualizaciones actu");
                    i = 0;
                    foreach (string c in camposPK2)
                    {
                        i++;
                        //if (camposPK2.Length == i)
                        if (i == 1)
                        {
                            file.WriteLine("\t\t\t\t\t\tON  destino." + c + " = actu." + c);
                        }
                        else
                        {
                            file.WriteLine("\t\t\t\t\t\tAND destino." + c + " = actu." + c);
                        }
                    }
                    file.WriteLine("");


                    file.WriteLine("\t\t--BORRADOS");
                    file.WriteLine("\t\t;WITH borrados AS");
                    file.WriteLine("\t\t(");
                    file.WriteLine("\t\t\tSELECT\t" + camposPK);
                    file.WriteLine("\t\t\tFROM\t" + schema +   "." + nombretab + "_tmp");
                    file.WriteLine("\t\t\tWHERE\tSYS_CHANGE_OPERATION = 'D'");
                    file.WriteLine("\t\t)");
                    file.WriteLine("\t\tDELETE\tdestino");
                    file.WriteLine("\t\tFROM\tdbo." + nombretab + " destino");
                    file.WriteLine("\t\t\t\tINNER JOIN  borrados borr");
                    i = 0;
                    foreach (string c in camposPK2)
                    {
                        i++;
                        //if (camposPK2.Length == i)
                        if (i == 1)
                        {
                            file.WriteLine("\t\t\t\t\t\tON  destino." + c + " = borr." + c);
                        }
                        else
                        {
                            file.WriteLine("\t\t\t\t\t\tAND destino." + c + " = borr." + c);
                        }
                    }
                    file.WriteLine("");

                    file.WriteLine("\t\t--INSERCIONES");
                    file.WriteLine("\t\t;WITH inserciones AS");
                    file.WriteLine("\t\t(");
                    file.WriteLine("\t\t\tSELECT\t" + camposPK);
                    file.WriteLine("\t\t\tFROM\t" + schema + "." + nombretab + "_tmp");
                    file.WriteLine("\t\t\tWHERE\tSYS_CHANGE_OPERATION IN ('I','X')");
                    file.WriteLine("\t\t\tEXCEPT");
                    file.WriteLine("\t\t\tSELECT\t" + camposPK);
                    file.WriteLine("\t\t\tFROM\tdbo." + nombretab);
                    file.WriteLine("\t\t)");
                    file.WriteLine("\t\tINSERT INTO dbo." + nombretab);
                    file.WriteLine("\t\t(");
                    file.WriteLine("\t\t\t" + campos);
                    file.WriteLine("\t\t)");
                    i = 0;
                    foreach (string c in campos2)
                    {
                        i++;
                        if (i == 1)
                        {
                            file.WriteLine("\t\tSELECT\ttmp." + c + ",");
                        }
                        else if (campos2.Length == i)
                        {
                            file.WriteLine("\t\t\t\ttmp." + c);
                        }
                        else
                        {
                            file.WriteLine("\t\t\t\ttmp." + c + ",");
                        }
                    }
                    file.WriteLine("\t\tFROM\tinserciones ins");
                    file.WriteLine("\t\t\t\tINNER JOIN  " + schema + "." + nombretab + "_tmp tmp");
                    i = 0;
                    foreach (string c in camposPK2)
                    {
                        i++;
                        if (i == 1)
                        //if (camposPK2.Length == i)
                        {
                            file.WriteLine("\t\t\t\t\t\tON  ins." + c + " = tmp." + c);
                        }
                        else
                        {
                            file.WriteLine("\t\t\t\t\t\tAND ins." + c + " = tmp." + c);
                        }
                    }
                    file.WriteLine("\t\tWHERE\tSYS_CHANGE_OPERATION IN ('I','X')");
                    file.WriteLine("");

                    file.WriteLine("\tEND TRY");

                    file.WriteLine("");
                    file.WriteLine("\tBEGIN CATCH");
                    file.WriteLine("\t\tSELECT 'ERROR EN LA EXTRACCIÓN'");
                    file.WriteLine("\tEND CATCH");
                    file.WriteLine("");
                    file.WriteLine("END");
                    file.WriteLine("GO");

                    file.WriteLine("");

                    //Propiedades extendidas del SP
                    file.WriteLine("--Propiedades extendidas del SP");
                    file.WriteLine("IF EXISTS (SELECT TOP 1 1 FROM dbn1_stg_dhyf.sys.fn_listextendedproperty('MS_Description', 'schema', '" + schema + "', 'procedure', 'SPN1Cargar" + nombretab + "', null, null))");
                    file.WriteLine("\tEXEC dbn1_stg_dhyf.sys.sp_dropextendedproperty @name = 'MS_Description', @level0type = 'schema', @level0name = '" + schema + "', @level1type = 'procedure', @level1name = 'SPN1Cargar" + nombretab + "', @level2type = null, @level2name = null");
                    file.WriteLine("GO");
                    file.WriteLine("EXEC dbn1_stg_dhyf.sys.sp_addextendedproperty @name = 'MS_Description', @value = 'Procedimiento de carga de la tabla " + nombretab + "', @level0type = 'schema', @level0name = '" + schema + "', @level1type = 'procedure', @level1name = 'SPN1Cargar" + nombretab + "'");
                    file.WriteLine("GO");

                    file.Close();
                }
                catch //(Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
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


        public string crearVariableConsulta(string csv, string ruta, ref string nombrearchivo, string bdOrigen)
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            valorescsv = a.ordenarCSV(valorescsv,"extraccion");

            foreach (string v in valorescsv)
            {
                if (v.ToLower().Contains("#tabla"))
                {
                    string[] datos = v.Split(new Char[] { ';' });
                    nombretab = datos[1];
                }
            }

            nombrearchivo = nombretab + "_Consulta.txt";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine(Convert.ToChar(34));
                    file.WriteLine("SELECT");

                    //escribir los campos del csv que se acaba de generar
                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });

                        if (!datos[0].Contains("#")) //para descartar las cabeceras del csv
                        {
                            if (datos[0].EndsWith("fecar")) //el campo fecha de carga tiene un tratamiento especial
                            {
                                file.WriteLine("\t" + "\t" + "GETDATE() AS " + datos[0].ToUpper());
                            }
                            else
                            {
                                file.WriteLine("\t" + "\t" + datos[0].ToUpper() + ",");
                            }
                        }
                    }

                    file.WriteLine("FROM" + "\t" + Convert.ToChar(34) + "+ @[User::TablaConEsquema] + " + Convert.ToChar(34));
                    file.WriteLine("--filtro si el ejercicio es numérico:WHERE" + "\t" + "SustituyemePorElCampoEjercicio BETWEEN YEAR(GETDATE()) - " + Convert.ToChar(34) + " + @[User::Ejercicios] + " + Convert.ToChar(34) + " AND YEAR(GETDATE())");
                    file.WriteLine("--filtro si el ejercicio es varchar:WHERE" + "\t" + "SustituyemePorElCampoEjercicio BETWEEN CAST(YEAR(GETDATE()) - " + Convert.ToChar(34) + " + @[User::Ejercicios] + " + Convert.ToChar(34) + " AS VARCHAR(4)) AND CAST(YEAR(GETDATE()) AS VARCHAR(4))");
                    file.WriteLine("--considerar otros posibles filtros");
                    file.WriteLine(Convert.ToChar(34));

                    file.Close();
                }
                catch //(Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
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


        public string crearVariableConsulta_CDC(string csv, string ruta, ref string nombrearchivo, string bdOrigen)
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            valorescsv = a.ordenarCSV(valorescsv,"extraccion");
            int i = 0;//para control de los bucles

            foreach (string v in valorescsv)
            {
                if (v.ToLower().Contains("#tabla"))
                {
                    string[] datos = v.Split(new Char[] { ';' });
                    nombretab = datos[1];
                }
            }

            nombrearchivo = nombretab + "_Consulta_CDC.txt";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine(Convert.ToChar(34));
                    file.WriteLine(";WITH cdc AS");
                    file.WriteLine("(");
                    file.WriteLine("\t" + "SELECT DISTINCT --toda la pk o índice único");

                    //cte con los campos de la pk o índice único)
                    i = 0;
                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });
                        
                        if (!datos[0].Contains("#")) //para descartar las cabeceras del csv
                        {
                            if (datos[2].Contains("#")) //para quedarme solo con los campos de la pk o índice único
                            {
                                if (i == 0)
                                {
                                    file.WriteLine("\t" + "\t" + "\t" + " cdc." + datos[0].ToUpper()); //la primera línea no debe escribir la coma
                                    i++;
                                }
                                else
                                {
                                    file.WriteLine("\t" + "\t" + "\t" + ",cdc." + datos[0].ToUpper());
                                }
                            }
                        }
                    }

                    file.WriteLine("\t" + "FROM" + "\t" + Convert.ToChar(34) + " + @[$Project::initialcatalog_conexion_origen] + " + Convert.ToChar(34) + ".cdc.fn_cdc_get_all_changes_" + Convert.ToChar(34) + " + @[$Project::esquema_conexion_origen] + " + Convert.ToChar(34) + "_" + Convert.ToChar(34) + " + @[User::Tabla] + " + Convert.ToChar(34) + "(convert(varbinary(max), '" + Convert.ToChar(34) + " + @[User::punto_inicial_cdc_extracciones] + " + Convert.ToChar(34) + "', 2), convert(varbinary(max), '" + Convert.ToChar(34) + " + @[User::punto_final_cdc_extracciones]+" + Convert.ToChar(34) + "', 2), 'all update old') cdc");
                    file.WriteLine("--filtro si el ejercicio es numérico:" + "\t" +  "WHERE" + "\t" + "SustituyemePorElCampoEjercicio BETWEEN YEAR(GETDATE()) - " + Convert.ToChar(34) + " + @[User::Ejercicios] + " + Convert.ToChar(34) + " AND YEAR(GETDATE())");
                    file.WriteLine("--filtro si el ejercicio es varchar:" + "\t" + "WHERE" + "\t" + "SustituyemePorElCampoEjercicio BETWEEN CAST(YEAR(GETDATE()) - " + Convert.ToChar(34) + " + @[User::Ejercicios] + " + Convert.ToChar(34) + " AS VARCHAR(4)) AND CAST(YEAR(GETDATE()) AS VARCHAR(4))");
                    file.WriteLine("--considerar otros posibles filtros");
                    file.WriteLine(")");

                    file.WriteLine("SELECT");


                    //cruzar la cte anterior con la tabla origen
                    i = 0;
                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });

                        if (!datos[0].Contains("#")) //para descartar las cabeceras del csv
                        {
                            //tratamiento del los campos que son parte de la pk o índice único
                            if (datos[2].Contains("#"))
                            {
                                if (i == 0)
                                {
                                    file.WriteLine("\t" + "\t" + " cdc." + datos[0].ToUpper()); //la primera línea no debe escribir la coma
                                    i++;
                                }
                                else
                                {
                                    file.WriteLine("\t" + "\t" + ",cdc." + datos[0].ToUpper());
                                }
                            }
                            //tratamiento del resto de campos
                            else
                            {
                                if (i == 0)
                                {
                                    file.WriteLine("\t" + "\t" + " origen." + datos[0].ToUpper()); //la primera línea no debe escribir la coma
                                    i++;
                                }
                                else
                                {
                                    if (datos[0].EndsWith("fecar")) //el campo fecha de carga tiene un tratamiento especial
                                    {
                                        file.WriteLine("\t" + "\t" + ",GETDATE() AS " + datos[0].ToUpper());
                                    }
                                    else
                                        file.WriteLine("\t" + "\t" + ",origen." + datos[0].ToUpper());
                                }
                            }
                        }
                    }

                    //para obtener el tipo de cambio SYS_CHANGE_OPERATION, necesito mirar el primer campo de la pk o índice único
                    i = 0;
                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });

                        if (!datos[0].Contains("#")) //para descartar las cabeceras del csv
                        {
                            if (datos[2].Contains("#")) //solo lo proceso si es parte de la pk o índice único
                            {
                                file.WriteLine(",CASE WHEN origen." + datos[0].ToUpper() + " IS NULL THEN 'D' ELSE 'X' END AS SYS_CHANGE_OPERATION --primer campo de la pk o índice único");
                                break; //salgo del bucle forecah, porque solo me interesa el primer campo de la pk o índice único
                            }
                        }
                    }

                    file.WriteLine("FROM" + "\t" + "cdc cdc");
                    file.WriteLine("\t" + "\t" + "LEFT JOIN" + "\t" + Convert.ToChar(34) + " + @[User::TablaConEsquema] +" + Convert.ToChar(34) + " origen");

                    //left join por los campos de la pk o índice único
                    i = 0;
                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });

                        if (!datos[0].Contains("#")) //para descartar las cabeceras del csv
                        {
                            if (datos[2].Contains("#")) //para quedarme solo con los campos de la pk o índice único
                            {
                                if (i == 0)
                                {
                                    file.WriteLine("\t" + "\t" + "\t" + "\t" + "ON" + "\t" + "cdc." + datos[0].ToUpper() + " = origen." + datos[0].ToUpper() + " --join por toda la pk o índice único"); //la primera línea de la join
                                    i++;
                                }
                                else
                                {
                                    file.WriteLine("\t" + "\t" + "\t" + "\t" + "AND" + "\t" + "cdc." + datos[0].ToUpper() + " = origen." + datos[0].ToUpper()); //la primera línea de la join
                                }
                            }
                        }
                    }

                    file.WriteLine(Convert.ToChar(34));

                    file.Close();
                }
                catch //(Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
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


        private string componer_fecar(string table, DataTable campos)
        {
            return "Fecha_Carga";
            /*
            string prefijo = table.Substring(2, 6) + "_";
            
            foreach (DataRow dr in campos.Rows)
            {
                if (!dr.ItemArray[0].ToString().Contains("#"))
                {
                    string valorCampo = valorCampo = dr.ItemArray[0].ToString().ToLower();
                    if (valorCampo.Contains("_"))
                    {
                        string[] valor = valorCampo.Split('_');
                        prefijo = valor[0] + "_";
                    }
                    //Otras opciones para generar el prefijo
                    else if (valorCampo.IndexOf(table.Substring(2, 6).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(2, 6).ToLower()), 6);
                    }
                    else if (valorCampo.IndexOf(table.Substring(3, 5).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(3, 5).ToLower()), 5);
                    }
                    else if (valorCampo.IndexOf(table.Substring(4, 4).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(4, 4).ToLower()), 4);
                    }
                    else if (valorCampo.IndexOf(table.Substring(4, 3).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(4, 3).ToLower()), 3);
                    }
                    else if (valorCampo.IndexOf(table.Substring(5, 3).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(5, 3).ToLower()), 3);
                    }
                    else if (valorCampo.IndexOf(table.Substring(4, 2).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(4, 2).ToLower()), 2);
                    }
                    else if (valorCampo.IndexOf(table.Substring(6, 2).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(6, 2).ToLower()), 2);
                    }
                    break;
                }

            }

            return prefijo + "Fecha_Carga";
            */
        }
    }
}
