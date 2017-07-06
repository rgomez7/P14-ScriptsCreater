using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsCreater
{
    class ScriptGen_MF_STG
    {
        Acciones a = new Acciones();
        Consultas c = new Consultas();
        ScriptComun sc = new ScriptComun();


        public int ExisteTabla(string table, ref string bd)
        {

            if (bd.ToString() == "")
            {
                bd = "DB" + table.Substring(2, 2);
            }
            string cadena = a.cadena(bd);
            DataTable dt = a.conexion(cadena, c.ComprobarTabla(table));

            return Convert.ToInt16(dt.Rows[0].ItemArray[0]);
        }
        
        public string createtable_stgFinal(string table, string ruta, ref string nombrearchivo, ref string camposPK, string bd, ref int activoCT)
        {
            activoCT = 1;

            string cadena = a.cadena(bd);
            string[] lineas = new string[0];
            string valorcampo = "";
            int valorpk = 0;

            string camposPK1 = "";
            string camposPK2 = "";
            string camposPK3 = "";
            
            DataTable datosColumnas = a.conexion(cadena, c.Columns(table));
            //DataTable datosClaves = a.conexion(cadena, "EXEC sp_helpindex N'DB2PROD." + table + "'  ");
            DataTable datosClaves = a.conexion(cadena, c.ColumnsClaves("DB2PROD." + table));
            DataTable datosExtended = a.conexion(cadena, c.PropiedadesExtendidas("DB2PROD", table));

            //MONTAR SCRIPTS
            string nombretab = "tbn1" + table.Substring(4, 4).ToLower() + "_" + table.Substring(2, 2).ToLower();
            //string fecar = table.Substring(2, 6).ToLower() + "_fecar";
            string fecar = componer_fecar(table, datosColumnas);
            nombrearchivo = "Script staging_" + nombretab + "_tablastg.sql";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.CreateNew), Encoding.UTF8);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
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
                    file.WriteLine("DROP TABLE dbo." + nombretab);
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
                            valorcampo = " " + dr.ItemArray[2].ToString().ToLower() + "(" + dr.ItemArray[4].ToString() + ", " + dr.ItemArray[5].ToString()  + ") not null";
                        }
                        else
                        {
                            valorcampo = " " + dr.ItemArray[2].ToString().ToLower() + " not null";
                        }

                        file.WriteLine("    " + dr.ItemArray[0].ToString().ToLower() + valorcampo + ",");
                    }
                    file.WriteLine("    " + fecar + " date not null");
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
                            camposPK1 = camposPK1 + dr.ItemArray[9].ToString().ToLower() + ", ";
                        }
                        //Si es cluster e index único
                        else if (Convert.ToBoolean(dr.ItemArray[3]) == true && dr.ItemArray[2].ToString().ToUpper() == "CLUSTERED")
                        {
                            camposPK2 = camposPK2 + dr.ItemArray[9].ToString().ToLower() + ", ";
                        }
                        //Si es index único
                        else if (Convert.ToBoolean(dr.ItemArray[3]) == true)
                        {
                            if (valorpk == 0 )
                            {
                                camposPK3 = camposPK3 + dr.ItemArray[9].ToString().ToLower() + ", ";
                                valorpk = Convert.ToInt32(dr.ItemArray[6]);
                            }
                            else if (valorpk == Convert.ToInt32(dr.ItemArray[6]))
                            {
                                camposPK3 = camposPK3 + dr.ItemArray[9].ToString().ToLower() + ", ";
                            }
                        }
                    }

                    if (camposPK1.Length == 0 && camposPK2.Length == 0 && camposPK3.Length == 0)
                    {
                        file.WriteLine("/*-------------------------*/");
                        file.WriteLine("/*------PROBLEMAS PK-------*/");
                        file.WriteLine("/*-------------------------*/");
                        file.WriteLine("");
                    }
                    else
                    {
                        if (camposPK1 != "")
                        {
                            camposPK = camposPK1;
                        }
                        else if (camposPK2 != "")
                        {
                            camposPK = camposPK2;
                        }
                        else if (camposPK3 != "")
                        {
                            camposPK = camposPK3;
                        }
                        camposPK = camposPK.Substring(0, camposPK.Length - 2);
                        file.WriteLine("ALTER TABLE dbo." + nombretab + " ADD CONSTRAINT PK_" + nombretab);
                        file.WriteLine("    PRIMARY KEY NONCLUSTERED (" + camposPK + ")");
                        file.WriteLine("GO");
                        file.WriteLine("");
                    }


                    //Propiedades Extendidas
                    file.WriteLine("IF NOT EXISTS (SELECT count(8) AS extprop FROM sys.extended_properties WHERE [major_id] = OBJECT_ID('dbo." + nombretab + "')) ");
                    file.WriteLine("BEGIN");
                    foreach (DataRow dr in datosExtended.Rows)
                    {
                        if (dr.ItemArray[0].ToString().ToUpper() == "COLUMN")
                        {
                            file.WriteLine(" EXEC sys.sp_addextendedproperty @name = N'" + dr.ItemArray[2].ToString() + "', @value = N'" + dr.ItemArray[3].ToString().ToUpper() + "' , @level0type = N'SCHEMA',@level0name = N'dbo', @level1type = N'TABLE',@level1name = N'" + nombretab + "', @level2type = N'" + dr.ItemArray[0].ToString().ToUpper() + "',@level2name = N'" + dr.ItemArray[1].ToString().ToLower() + "'");
                        }
                        else if (dr.ItemArray[0].ToString().ToUpper() == "TABLE")
                        {
                            file.WriteLine(" EXEC sys.sp_addextendedproperty @name = N'" + dr.ItemArray[2].ToString() + "', @value = N'" + dr.ItemArray[3].ToString().ToUpper() + "' , @level0type = N'SCHEMA',@level0name = N'dbo', @level1type = N'TABLE',@level1name = N'" + nombretab + "'");
                        }
                        else
                        { }
                    }
                    file.WriteLine("    EXEC sys.sp_addextendedproperty 'Caption', 'Fecha última carga', 'Schema', 'dbo', 'Table',  '" + nombretab + "', 'Column', '" + fecar + "'");
                    file.WriteLine("END");
                    file.WriteLine("GO");
                    file.WriteLine("");

                    //ADD CT
                    file.WriteLine("--Add CT");
                    file.WriteLine("IF NOT EXISTS(");
                    file.WriteLine("SELECT 1 FROM sys.change_tracking_tables tt");
                    file.WriteLine("    INNER JOIN sys.objects obj ON obj.object_id = tt.object_id");
                    file.WriteLine("    WHERE obj.name = '" + nombretab + "' )");
                    file.WriteLine("ALTER TABLE dbo." + nombretab + " ENABLE CHANGE_TRACKING WITH(TRACK_COLUMNS_UPDATED = ON)");

                    file.WriteLine("SET ANSI_PADDING OFF");
                    file.WriteLine("GO");

                    file.Close();

                    //Comprueba si tiene CT Activo
                    DataTable datosCT = a.conexion(cadena, c.ChangeTrackingActivo(table));
                    if (datosCT.Rows.Count == 0)
                    {
                        nombrearchivo = "//** " + table + " No tiene CHANGE_TRACKING ACTIVO**\\" + "\n\r" + nombrearchivo;
                        activoCT = 0;
                    }
                }
                catch (Exception ex)
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

        public string gencsv(string table, string ruta, ref string nombrearchivo, string camposPK)
        {
            //TBMFPH46
            string bd = "DB" + table.Substring(2, 2);
            string cadena = a.cadena(bd);
            string[] lineas = new string[0];
            string valorcampo = "";

            string csvRow;

            DataTable datosColumnas = a.conexion(cadena, "SELECT column_name, is_nullable, data_type, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + table + "'");
            DataTable datosClaves = a.conexion(cadena, c.ColumnsClaves("DB2PROD." + table));

            //MONTAR SCRIPTS
            string nombretab = "tbn1" + table.Substring(4, 4).ToLower() + "_" + table.Substring(2, 2).ToLower();
            //string fecar = table.Substring(2, 6).ToLower() + "_fecar";
            string fecar = componer_fecar(table, datosColumnas);
            nombrearchivo = nombretab + ".csv";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", "#nombre_DS", nombretab.Replace("tbn1", ""), "dbn1_stg_dhyf", "", "", "", "", "", "", "");
                    file.WriteLine(csvRow);
                    csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", "#prefijo_DM", "", "", "", "", "", "", "", "", "");
                    file.WriteLine(csvRow);
                    csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", "#campo", "Tipo", "TablaSID", "SID", "Clave?", "Dim", "Campocruce", "SIDDim", "AutoSID?", "Filter");
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

                        if (camposPK.Contains(dr.ItemArray[0].ToString().ToLower()))
                        {
                            csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", dr.ItemArray[0].ToString().ToLower(), valorcampo, "", "", "#", "", "", "", "", "");
                            file.WriteLine(csvRow);
                        }
                        else
                        {
                            csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", dr.ItemArray[0].ToString().ToLower(), valorcampo, "", "", "", "", "", "", "", "");
                            file.WriteLine(csvRow);
                        }
                    }

                    csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", fecar, "date", "", "", "", "", "", "", "", "");
                    file.WriteLine(csvRow);

                    file.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo CSV " + nombrearchivo, "Error escritura archivo CSV", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return "NO";
                }

                return "OK";
            }
            else
            {
                MessageBox.Show("No se ha podido generar el fichero CSV " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero CSV", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }
        }

        public string createtable_extraccion(string csv, string ruta, ref string nombrearchivo)
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string campos = "";
            string camposPK = "";
            string bd = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            valorescsv = a.ordenarCSV(valorescsv);

            foreach (string v in valorescsv)
            {
                string[] datos = v.Split(new Char[] { ';' });
                if (v.ToLower().Contains("#nombre"))
                {
                    nombretab = "tbn1" + datos[1];
                    bd = datos[2];
                }
                if (!datos[0].Contains("#"))
                {
                    campos = campos + "'" + datos[0] + "', ";
                    if (datos[4].Contains("#"))
                    { camposPK = camposPK + datos[0] + ", "; }
                }
            }
            campos = campos.Substring(0, campos.Length - 2);
            if (camposPK.Length > 0)
            {
                camposPK = camposPK.Substring(0, camposPK.Length - 2);
            }

            nombrearchivo = "Script staging_" + nombretab + "_tablatmp.sql";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");

                    //Create Table
                    file.WriteLine("--------------------------------------");

                    file.WriteLine("");
                    file.WriteLine("--Begin table create/prepare -> " + nombretab + "_tmp");
                    file.WriteLine("");

                    sc.regTablas(file, "dbn1_stg_dhyf", "extracciones_tmp", nombretab + "_tmp", "", campos, camposPK, valorescsv, false, "extraccion");
                    
                    file.WriteLine("--------------------------------------*/");

                    file.WriteLine("--End table create/prepare -> " + nombretab + "_tmp");
                    file.WriteLine("");

                    file.WriteLine("GO");

                    file.Close();
                }
                catch (Exception ex)
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

        public string createSP_extraccion(string csv, string ruta, ref string nombrearchivo)
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string bd = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            valorescsv = a.ordenarCSV(valorescsv);
            string schema = "extracciones_tmp";
            string campos = "";
            string camposPK= "";
            int i = 0;

            foreach (string v in valorescsv)
            {
                string[] datos = v.Split(new Char[] { ';' });
                if (datos[0].ToLower().Contains("#nombre"))
                {
                    nombretab = "tbn1" + datos[1];
                    bd = datos[2];
                }
                else if (!datos[0].Contains("#"))
                {
                    campos = campos + datos[0].ToString() + ",";
                    if (datos[4].Contains("#"))
                    {
                        camposPK = camposPK + datos[0].ToString() + ",";
                    }
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            camposPK = camposPK.Substring(0, camposPK.Length - 1);
            string[] campos2 = campos.Split(',');
            string[] camposPK2 = camposPK.Split(',');

            nombrearchivo = "Script staging_" + nombretab + "_sptmp.sql";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");


                    //Cambiamos a otra BBDD y empezamos la nueva tarea
                    file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_stg_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='" + schema + "' AND ROUTINE_NAME='spn1_cargar_" + nombretab + "' AND ROUTINE_TYPE='PROCEDURE')");
                    file.WriteLine("    DROP PROCEDURE " + schema + ".spn1_cargar_" + nombretab + ";");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    //Generamos el SP
                    file.WriteLine("CREATE PROCEDURE " + schema + ".spn1_cargar_" + nombretab + " AS");
                    file.WriteLine("BEGIN");
                    file.WriteLine("");
                    file.WriteLine("SET NOCOUNT ON");
                    file.WriteLine("");

                    file.WriteLine("--ACTUALIZACIONES");
                    file.WriteLine("    ;WITH actualizaciones AS");
                    file.WriteLine("    ( SELECT " + campos);
                    file.WriteLine("    FROM    " + schema + "." + nombretab + "_tmp");
                    file.WriteLine("    WHERE sys_change_operation = 'U')");
                    file.WriteLine("");
                    file.WriteLine("    UPDATE destino");
                    file.WriteLine("    SET");
                    i = 0;
                    foreach (string c in campos2)
                    {
                        i++;
                        if (campos2.Length == i)
                        {
                            file.WriteLine("        destino." + c + " = actu." + c);
                        }
                        else
                        {
                            file.WriteLine("        destino." + c + " = actu." + c + ",");
                        }
                    }
                    file.WriteLine("    FROM dbo." + nombretab + " destino");
                    file.WriteLine("    INNER JOIN actualizaciones actu ON");
                    i = 0;
                    foreach (string c in camposPK2)
                    {
                        i++;
                        if (camposPK2.Length == i)
                        {
                            file.WriteLine("        destino." + c + " = actu." + c);
                        }
                        else
                        {
                            file.WriteLine("        destino." + c + " = actu." + c + " AND ");
                        }
                    }
                    file.WriteLine("");


                    file.WriteLine("--BORRADOS");
                    file.WriteLine("    ;WITH borrados AS");
                    file.WriteLine("    ( SELECT " + camposPK);
                    file.WriteLine("    FROM    " + schema +   "." + nombretab + "_tmp");
                    file.WriteLine("    WHERE sys_change_operation = 'D')");
                    file.WriteLine("");
                    file.WriteLine("    DELETE destino");
                    file.WriteLine("    FROM dbo." + nombretab + " destino");
                    file.WriteLine("    INNER JOIN borrados actu ON");
                    i = 0;
                    foreach (string c in camposPK2)
                    {
                        i++;
                        if (camposPK2.Length == i)
                        {
                            file.WriteLine("        destino." + c + " = actu." + c);
                        }
                        else
                        {
                            file.WriteLine("        destino." + c + " = actu." + c + " AND");
                        }
                    }
                    file.WriteLine("");

                    file.WriteLine("--INSERCCIONES");
                    file.WriteLine("    ;WITH inserciones AS");
                    file.WriteLine("    ( SELECT " + camposPK);
                    file.WriteLine("    FROM    " + schema + "." + nombretab + "_tmp");
                    file.WriteLine("    WHERE sys_change_operation = 'I'");
                    file.WriteLine("    EXCEPT");
                    file.WriteLine("    SELECT " + camposPK);
                    file.WriteLine("    FROM    dbo." + nombretab + ")");
                    file.WriteLine("");
                    file.WriteLine("    INSERT INTO dbo." + nombretab);
                    file.WriteLine("        (" + campos + ")");
                    file.WriteLine("    SELECT");
                    i = 0;
                    foreach (string c in campos2)
                    {
                        i++;
                        if (campos2.Length == i)
                        {
                            file.WriteLine("        tmp." + c);
                        }
                        else
                        {
                            file.WriteLine("        tmp." + c + ",");
                        }
                    }
                    file.WriteLine("    FROM inserciones ins");
                    file.WriteLine("    INNER JOIN " + schema + "." + nombretab + "_tmp tmp  ON");
                    i = 0;
                    foreach (string c in camposPK2)
                    {
                        i++;
                        if (camposPK2.Length == i)
                        {
                            file.WriteLine("        ins." + c + " = tmp." + c);
                        }
                        else
                        {
                            file.WriteLine("        ins." + c + " = tmp." + c + " AND");
                        }
                    }
                    file.WriteLine("    WHERE   sys_change_operation = 'I'");
                    file.WriteLine("");

                    file.WriteLine("END");
                    file.WriteLine("GO");

                    file.Close();
                }
                catch (Exception ex)
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

        public string createSnippet_ext(string csv, string ruta, ref string nombrearchivo)
        {
            string[] lineas = new string[0];
            string nombretab = "";
            string bd = "";
            string[] valorescsv = a.leerCSV(csv, ruta);
            int i = 0;
            valorescsv = a.ordenarCSV(valorescsv);

            foreach (string v in valorescsv)
            {
                if (v.ToLower().Contains("#nombre"))
                {
                    string[] datos = v.Split(new Char[] { ';' });
                    nombretab = "tbn1" + datos[1];
                    bd = datos[2];
                }
            }

            nombrearchivo = "Snippet_var_SSIS_" + nombretab + ".txt";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    file.WriteLine(Convert.ToChar(34) + ", CAST(t.SYS_CHANGE_OPERATION AS CHAR(1)) AS SYS_CHANGE_OPERATION");
                    file.WriteLine("     FROM  (SELECT ct.SYS_CHANGE_OPERATION");
                    i = 0;
                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });
                        if (!datos[0].Contains("#"))
                        {                            
                            if (datos[4].Contains("#"))
                            {
                                file.WriteLine("               ,ct." + datos[0].ToUpper());
                            }
                            else
                            {
                                if (datos[0].ToLower().Contains("fecar"))
                                { }
                                else
                                { 
                                file.WriteLine("               ,origen." + datos[0].ToUpper());
                                }
                            }
                        }
                    }
                    file.WriteLine("            FROM    CHANGETABLE(CHANGES " + Convert.ToChar(34) + " + @[User::TablaConEsquema] + " + Convert.ToChar(34) + "," + Convert.ToChar(34) + " + (DT_STR, 20, 1252) @[User::punto_inicial_ct_extracciones] +" + Convert.ToChar(34) + ") AS ct");
                    file.WriteLine("                    LEFT JOIN " + Convert.ToChar(34) + " + @[User::TablaConEsquema] + " + Convert.ToChar(34) + " origen ON");

                    foreach (string v in valorescsv)
                    {
                        string[] datos = v.Split(new Char[] { ';' });
                        if (!datos[0].Contains("#"))
                        {
                            if (datos[4].Contains("#"))
                            {
                                if (i == 0)
                                { 
                                file.WriteLine("                 ct." + datos[0].ToUpper() + " = origen." + datos[0].ToUpper());
                                }
                                else
                                {
                                    file.WriteLine("                 AND ct." + datos[0].ToUpper() + " = origen." + datos[0].ToUpper());
                                }
                                i++;
                            }
                        }
                    }
                    file.WriteLine("            ) t" + Convert.ToChar(34));

                    file.Close();
                }
                catch (Exception ex)
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

        public string activarCT_microfocus(string table, string ruta, ref string nombrearchivo, string camposPK, string bd, string tipogen)
        {
            string nombreBD = "";
            if (bd == "")
            {
                nombreBD = table.Substring(2, 2);
            }
            else
            {
                nombreBD = bd.Substring(2, 2);
            }

            string[] lineas = new string[0];
            string nombretab = table.Substring(4, 4);

            nombrearchivo = "Script preparacion_activar_ct_microfocus_" + nombreBD + "_" + nombretab + "_" + tipogen + ".sql";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.CreateNew), Encoding.UTF8);

                    file.WriteLine("Declare @bbdd nvarchar(10) = '" + nombreBD + "'");
                    file.WriteLine("Declare @tabla nvarchar(50) = '" + nombretab + "'");
                    //Esto es para PRE
                    if (tipogen.ToLower() == "pre")
                    {
                        file.WriteLine("Declare @esquema nvarchar(50) = 'DB2DESA'");
                        file.WriteLine("Declare @grantview nvarchar(50) = 'WDN1JDB0'");
                    }
                    //Esto es para PROD
                    else if(tipogen.ToLower()=="pro")
                    {
                        file.WriteLine("Declare @esquema nvarchar(50) = 'DB2PROD'");
                        file.WriteLine("Declare @grantview nvarchar(50) = 'WEN1JDB0'");
                    }

                    file.WriteLine("Declare @campos nvarchar(200) = '" +camposPK + "'");
                    file.WriteLine("Declare @sql nvarchar(2000)");
                    file.WriteLine("");

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("");

                    file.WriteLine("--Activamos CT en las 3 BB.DD. en caso de que no esté activado ");
                    file.WriteLine("IF NOT EXISTS(select 1 FROM sys.change_tracking_databases WHERE database_id = DB_ID('DB' + UPPER(@bbdd))) ");
                    file.WriteLine("SET @sql = 'ALTER DATABASE DB' + UPPER(@bbdd) + ' SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 30 DAYS, AUTO_CLEANUP = ON)' ");
                    file.WriteLine("exec(@sql)");
                    file.WriteLine("");

                    file.WriteLine("--Creamos la PK a partir del índice único existente en la tabla");
                    file.WriteLine("SET @sql = 'IF NOT EXISTS (SELECT 1 FROM (");
                    file.WriteLine("    SELECT TABLE_SCHEMA, TABLE_NAME, CONSTRAINT_NAME,CONSTRAINT_TYPE from DB' + UPPER(@bbdd) + '.INFORMATION_SCHEMA.TABLE_CONSTRAINTS ");
                    file.WriteLine("    UNION SELECT schemas.name TABLE_SCHEMA,  tables.name TABLE_NAME, default_constraints.name,' + char(39) + 'DEFAULT' + char(39) + ' ");
                    file.WriteLine("    FROM DB' + UPPER(@bbdd) + '.sys.default_constraints ");
                    file.WriteLine("    INNER JOIN DB' + UPPER(@bbdd) + '.sys.all_columns ON all_columns.default_object_id = default_constraints.object_id ");
                    file.WriteLine("    INNER JOIN DB' + UPPER(@bbdd) + '.sys.tables ON all_columns.object_id = tables.object_id ");
                    file.WriteLine("    INNER JOIN DB' + UPPER(@bbdd) + '.sys.schemas ON tables.schema_id = schemas.schema_id ");
                    file.WriteLine(") all_constraints WHERE TABLE_SCHEMA=' + char(39) + UPPER(@esquema) + char(39) + ' AND TABLE_NAME=' + char(39) + 'TB' + UPPER(@bbdd) + UPPER(@tabla) + char(39) + ' AND CONSTRAINT_NAME='  + char(39) + 'PK_tb' + LOWER(@bbdd) + LOWER(@tabla) + char(39) + ') ");
                    file.WriteLine("ALTER TABLE DB' + UPPER(@bbdd) + '.'+ UPPER(@esquema) +'.TB' + UPPER(@bbdd) + UPPER(@tabla) + ' ADD CONSTRAINT PK_tb' + LOWER(@bbdd) + LOWER(@tabla) + ' PRIMARY KEY NONCLUSTERED ('+@campos+')'");
                    file.WriteLine("exec(@sql)");
                    file.WriteLine("");

                    file.WriteLine("--Añadimos la propiedad extendida del índice creado por la PK ");
                    file.WriteLine("SET @sql = 'IF NOT EXISTS ( ");
                    file.WriteLine("SELECT 1 FROM DB' + UPPER(@bbdd) + '.sys.fn_listextendedproperty(' + char(39) + 'MS_Description' + char(39) + ', ' + char(39) + 'SCHEMA' + char(39) + ', ' + char(39) + UPPER(@esquema) + char(39) + ', ' + char(39) + 'TABLE' + char(39) + ', ' + char(39) + 'TB'+ UPPER(@bbdd) + UPPER(@tabla) + char(39) + ', ' + char(39) + 'CONSTRAINT' + char(39) + ', ' + char(39) + 'PK_tb'+ lower(@bbdd) + lower(@tabla)  + char(39) + ')) " );
                    file.WriteLine("EXEC DB' + UPPER(@bbdd) + '.sys.sp_addextendedproperty @name = N' + char(39) + 'MS_Description' + char(39) + ', @value = N' + char(39) + '[NO-CLUSTER][UNICO]' + char(39) + ', @level0type=N' + char(39) + 'SCHEMA' + char(39) + ',@level0name=N' + char(39) + UPPER(@esquema) + char(39) + ',@level1type=N' + char(39) + 'TABLE' + char(39) + ',@level1name=N' + char(39) + 'TB'+ UPPER(@bbdd) + UPPER(@tabla) + char(39) + ',@level2type=N' + char(39) + 'CONSTRAINT' + char(39) + ', @level2name=N' + char(39) + 'PK_tb'+ lower(@bbdd) + lower(@tabla) + char(39) + ';'");
                    file.WriteLine("exec(@sql)");
                    file.WriteLine("");

                    file.WriteLine("--Activamos CT en cada una de las tablas en caso de que no esté activado... ");
                    file.WriteLine("SET @sql = 'IF NOT EXISTS ( select 1 ");
                    file.WriteLine("    FROM DB' + UPPER(@bbdd) +'.sys.change_tracking_tables tt " );
                    file.WriteLine("    INNER JOIN DB' + UPPER(@bbdd) + '.sys.objects obj on obj.object_id = tt.object_id ");
                    file.WriteLine("    WHERE obj.name = ' + char(39) + 'TB' + UPPER(@bbdd) + UPPER(@tabla) + char(39) + ') ");
                    file.WriteLine("ALTER TABLE DB' + UPPER(@bbdd) +'.' + UPPER(@esquema) +'.TB' + UPPER(@bbdd) + UPPER(@tabla) + '  ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)'");
                    file.WriteLine("exec(@sql)");
                    file.WriteLine("");

                    file.WriteLine("SET @sql = 'USE[DB' + UPPER(@bbdd) +'] GRANT VIEW CHANGE TRACKING ON ' + UPPER(@esquema) + '.TB' + UPPER(@bbdd) + UPPER(@tabla) + ' TO ' + UPPER(@grantview)");
                    file.WriteLine("exec(@sql)");
                    file.WriteLine("");

                    file.WriteLine("GO");
                    file.WriteLine("");

                    file.Close();
                }
                catch (Exception ex)
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
            string prefijo = table.Substring(2, 6).ToLower() + "_";
            
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
                    //Otras opción de generar el prefijo
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
                    else if (valorCampo.IndexOf(table.Substring(5, 3).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(5, 3).ToLower()), 3);
                    }
                    else if (valorCampo.IndexOf(table.Substring(6, 2).ToLower()) == 0)
                    {
                        prefijo = valorCampo.Substring(valorCampo.IndexOf(table.Substring(6, 2).ToLower()), 2);
                    }
                    break;
                }

            }

            return prefijo + "fecar";
        }
    }
}
