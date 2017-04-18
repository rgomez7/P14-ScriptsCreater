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
    class ScriptDSDM
    {
        Acciones a = new Acciones();
        ScriptComun sc = new ScriptComun();

        //Table
        public string table(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean incremental, Boolean CreateTable, Boolean ChangeTrack)
            {

                string dev  = ds(archivo, csv, ruta, ref nombrearchivo, incremental, CreateTable,ChangeTrack);
                return "OK";
            }

        //DataStore
        public string ds(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean incremental, Boolean CreateTable, Boolean ChangeTrack)
        {
            string nombrearchivoexec = "";
            string fichero;
            string tab = "";
            string bd = "dbn1_norm_dhyf";
            string clave = "";
            string campos = "";
            string campospk = "";
            string camposfilter = "";
            string schema = "dbo";
            string[] csv2 = new string[0];

            int i = 0;
            DataRow[] dr;

            foreach (string d in csv)
            {
                string[] j = d.Split(new Char[] { ';' });
                if (j[0].ToLower().Contains("#nombre"))
                {
                    tab = j[1].ToString();
                    bd = j[2].ToString();
                    clave = j[3].ToString();
                }
                else if (!j[0].Contains("#"))
                {
                    campos = campos + "'" + j[0] + "',";
                    if (j[4].ToString() == "#")
                    {
                        campospk = campospk + "t_" + j[0] + ",";
                    }
                    if (j[9].ToString() == "#")
                    {
                        camposfilter = camposfilter + j[0] + ",";
                    }

                    Array.Resize(ref csv2, csv2.Length + 1);
                    if (j[2].ToString() != "")
                    {
                        csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[4].ToString() + ";#";
                    }
                    else
                    {
                        csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[4].ToString() + ";";
                    }
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            campospk = campospk.Substring(0, campospk.Length - 1);
            if (camposfilter.Length > 0)
            { 
                camposfilter = camposfilter.Substring(0, camposfilter.Length - 1);
            }
            //Si no tenemos valor clave, lo generamos
            if (clave == "")
            {
                clave = "id_" + tab;
            }
            else if (campos.Contains(clave))
            {
                clave = "";
            }

            //Indicamos la BBDD de normalizado
            if (bd == "")
            {
                bd = "dbn1_norm_dhyf";
            }

            //Generamos nombre fichero y obtenemos lineas, renombrando fichero actual
            nombrearchivo = "Script normalizado_" + tab + ".sql";
            nombrearchivoexec = "Exec normalizado_" + tab + ".sql";
            string[] lineas = new string[0];
            string dev = a.comprobarficheros(ref lineas, ruta, nombrearchivo, 1);
            DataTable valorquery = a.valorQuery(lineas, csv, "ds", incremental, tab);

            fichero = ruta + nombrearchivo;
            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);
                
                file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                file_exec.WriteLine("GO");
                a.generar_file_exec(file_exec, bd + ".dbo.tbn1_" + tab, "dbn1_stg_dhyf", "dbo", "spn1_cargar_normalizado_" + tab);

                file.WriteLine("PRINT '" + nombrearchivo + "'");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Generado versión vb " + a.version);
                file.WriteLine("");
                file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                file.WriteLine("GO");
                file.WriteLine("");

                #region "Tabla"

                if (CreateTable == true)
                {
                    file.WriteLine("/*--------------------------------------");
                }
                else
                {
                    file.WriteLine("--------------------------------------");
                }
                file.WriteLine("--Begin table create/prepare -> tbn1_" + tab);
                file.WriteLine("");
                //Desactivamos CT
                string ctd = sc.changetracking(file, "tbn1_" + tab, bd, "dbo", "des");

                //Drop FKs
                sc.borrarFK(file, bd, schema, tab);

                //Create Table
                sc.regTablas(file, bd, schema, "tbn1_" + tab, clave, campos, campospk, csv2, false, "ds");

                if (CreateTable == false)
                {
                    file.WriteLine("--------------------------------------");
                }
                else
                {
                    file.WriteLine("--------------------------------------*/");
                }
                file.WriteLine("");
                //Activamos CT
                if (ChangeTrack == true)
                {
                    file.WriteLine("/*--------------------------------------");
                }
                else
                {
                    file.WriteLine("--------------------------------------");
                }
                string cta = sc.changetracking(file, "tbn1_" + tab, bd, "dbo", "act");
                if (ChangeTrack == false)
                {
                    file.WriteLine("--------------------------------------");
                }
                else
                {
                    file.WriteLine("--------------------------------------*/");
                }
                file.WriteLine("--End table create/prepare -> tbn1_" + tab );
                file.WriteLine("");
                #endregion "Tabla"

                #region "SP"

                #region "CabeceraSP"
                //SP Creamos SP
                file.WriteLine("USE dbn1_stg_dhyf");
                file.WriteLine("GO");
                file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_stg_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'spn1_cargar_normalizado_" + tab + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                file.WriteLine("    DROP PROCEDURE dbo.spn1_cargar_normalizado_" + tab);
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("CREATE PROCEDURE dbo.spn1_cargar_normalizado_" + tab + "(@p_id_carga int) AS");
                file.WriteLine("BEGIN");
                file.WriteLine("");
                
                //SP Cabecera
                string cab2 = sc.cabeceraLogSP(file, "dbn1_stg_dhyf", "dbo", "spn1_cargar_normalizado_" + tab, incremental, false);

                if (incremental == true)
                {
                    //SP Registro del SP en tabla de control de cargas incrementales y obtención de datos en variables
                    string sp_inc = sc.regSP_Incremental(file);
                }

                //SP Creamos Object Temporal
                file.WriteLine("            ALTER TABLE " + bd + ".dbo.tbn1_" + tab + " NOCHECK CONSTRAINT ALL");
                file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_q_" + tab + "') IS NOT NULL");
                file.WriteLine("                DROP TABLE #tmp_q_" + tab + "");
                file.WriteLine("            CREATE table #tmp_q_" + tab + "(");
                file.WriteLine("                cc int default 1,");
                //--Campos--//
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString());
                        }
                        else
                        {
                            file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("            );");
                file.WriteLine("");

                //SP Creamos segundo Object temporal
                file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_" + tab + "') IS NOT NULL");
                file.WriteLine("                DROP TABLE #tmp_" + tab + "");
                file.WriteLine("            CREATE table #tmp_" + tab + "(");
                file.WriteLine("                rr_mode varchar(1),");
                file.WriteLine("                cc int,");
                file.WriteLine("                " + clave + " int,");
                //--Claves--//
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (j[4].Contains("#"))
                        {
                            file.WriteLine("                t_" + j[0].ToString() + " " + j[1].ToString() + ",");
                        }
                    }
                }
                //--Campos--//
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString());
                        }
                        else
                        {
                            file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("            );");
                file.WriteLine("");
                #endregion "CabeceraSP"

                //SP Insertamos carga de datos a Maestro
                #region "Control cambios SP"
                //SP si es incremental incluimos la comprobación del registro en tipo de carga y metemos la parte comun de Full / Incremental
                if (incremental == true)
                {
                    file.WriteLine("            SELECT @es_carga_completa = es_carga_completa");
                    file.WriteLine("            FROM dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro");
                    file.WriteLine("            WHERE objeto = @objeto;");
                    file.WriteLine("");
                    file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_keys_" + tab + "') IS NOT NULL");
                    file.WriteLine("                DROP TABLE #tmp_keys_" + tab + "");
                    file.WriteLine("            CREATE table #tmp_keys_" + tab + "(");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[9].Contains("#"))
                            {
                                i++;
                                if (i == camposfilter.Split(new Char[] { ',' }).Length)
                                {
                                    file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString());
                                }
                                else
                                {
                                    file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + ",");
                                }
                            }
                        }
                    }
                    file.WriteLine("               )");
                    file.WriteLine("");

                    dr = null;
                    dr = valorquery.Select("codScript=1", "codScript ASC, orden ASC");
                    foreach (DataRow l2 in dr)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                    file.WriteLine("");
                    
                    file.WriteLine("    IF @es_carga_completa = 0");
                    file.WriteLine("    BEGIN");
                    file.WriteLine("--------BLOQUE INCREMENTAL--------");
                    file.WriteLine("");

                    //SP Creamos la insert de carga de datos
                    dr = null;
                    dr = valorquery.Select("codScript=2", "codScript ASC, orden ASC");
                    foreach (DataRow l2 in dr)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                    file.WriteLine("");
                    file.WriteLine("        INSERT INTO #tmp_keys_" + tab);
                    file.WriteLine("            (" + camposfilter + ")");
                    file.WriteLine("        SELECT ");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[9].Contains("#"))
                            {
                                i++;
                                if (i == camposfilter.Split(new Char[] { ',' }).Length)
                                {
                                    file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString());
                                }
                                else
                                {
                                    file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString() + ",");
                                }
                            }
                        }
                    }
                    file.WriteLine("        FROM query");
                    file.WriteLine("");
                    //
                    dr = null;
                    dr = valorquery.Select("codScript=3", "codScript ASC, orden ASC");
                    foreach (DataRow l2 in dr)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                    file.WriteLine("");
                    file.WriteLine("        INSERT INTO #tmp_q_" + tab);
                    file.WriteLine("        (" + campos.Replace("'","") + ")");
                    file.WriteLine("        SELECT    ");
                    i = 0;
                    foreach (string d in csv)
                    {
                        i++;
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv.Length)
                            {
                                file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM    query");
                    file.WriteLine("");

                    //SP Registramos los datos en la tabla temporal
                    //SP Comparamos registros para indicar la acción a realizar
                    file.WriteLine("        INSERT INTO #tmp_" + tab + " (rr_mode,cc,id_" + tab + ", " + campospk + "," + campos.Replace("'", "") + ")");
                    file.WriteLine("        SELECT");
                    file.WriteLine("            rr_mode=");
                    file.WriteLine("                CASE");
                    file.WriteLine("                    WHEN t." + clave + " IS NULL THEN 'I'");
                    file.WriteLine("                    WHEN cc IS NULL THEN 'D'");
                    file.WriteLine("                ELSE 'U' END,");
                    file.WriteLine("            cc AS cc,");
                    file.WriteLine("            t." + clave + " AS " + clave + ",");
                    //--//Incluimos las claves
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (j[4].ToString() == "#")
                            {
                                file.WriteLine("            t." + j[0].ToString() + " AS t_" + j[0].ToString() + ",");
                            }
                        }
                    }
                    //--//Incluimos los campos
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv.Length)
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM " + bd + "." + schema + ".tbn1_" + tab + " AS t");
                    file.WriteLine("        INNER JOIN #tmp_keys_" + tab + " AS keys on (");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[9].ToString() == "#")
                            {
                                i++;
                                if (i == camposfilter.Split(new Char[] { ',' }).Length)
                                {
                                    file.WriteLine("            (keys." + j[0].ToString() + " = t." + j[0].ToString() + " OR (keys." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                                }
                                else
                                {
                                    file.WriteLine("            (keys." + j[0].ToString() + " = t." + j[0].ToString() + " OR (keys." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                                }
                            }
                        }
                    }
                    file.WriteLine("        FULL JOIN #tmp_q_" + tab + " AS query on (");
                    //--//Realizamos las comparaciones de claves
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[4].ToString() == "#")
                            {
                                i++;
                                if (i == campospk.Split(new Char[] { ',' }).Length)
                                {
                                    file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                                }
                                else
                                {
                                    file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                                }
                            }
                        }
                    }
                    file.WriteLine("        WHERE ");
                    file.WriteLine("            t." + clave + " IS NULL OR");
                    file.WriteLine("            cc IS NULL");
                    //--//Realizamos las comparaciones de claves
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            i++;
                            if (i == 1)
                            {
                                file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                            else if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                            }
                            else
                            {
                                file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                        }
                    }
                    file.WriteLine("        END");
                    file.WriteLine("--------BLOQUE FULL--------");
                    file.WriteLine("        ELSE");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("");

                    dr = null;
                    dr = valorquery.Select("codScript=4", "codScript ASC, orden ASC");
                    foreach (DataRow l2 in dr)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                }
                else
                {
                    //SP Creamos la insert de carga de datos
                    dr = null;
                    dr = valorquery.Select("codScript=1", "codScript ASC, orden ASC");
                    foreach (DataRow l2 in dr)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                }
                file.WriteLine("");
                //SP Montamos campos para tabla Query
                file.WriteLine("INSERT INTO #tmp_q_" + tab + " (" + campos.Replace("'", "") + ")");
                file.WriteLine("SELECT");
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("    query." + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("    query." + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("FROM query");
                file.WriteLine("");

                //SP Comparamos registros para indicar la acción a realizar
                file.WriteLine("        INSERT INTO #tmp_" + tab + " (rr_mode,cc,id_" + tab + ", " + campospk + "," + campos.Replace("'","")+ ")");
                file.WriteLine("        SELECT");
                file.WriteLine("            rr_mode=");
                file.WriteLine("                CASE");
                file.WriteLine("                    WHEN t." + clave + " IS NULL THEN 'I'");
                file.WriteLine("                    WHEN cc IS NULL THEN 'D'");
                file.WriteLine("                    ELSE 'U' END,");
                file.WriteLine("            cc AS cc,");
                file.WriteLine("            t." + clave + " AS " + clave + ",");
                //--//Incluimos las claves
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (j[4].ToString() == "#")
                        {
                            file.WriteLine("            t." + j[0].ToString() + " AS t_" + j[0].ToString() + ",");
                        }
                    }
                }
                //--//Incluimos los campos
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("        FROM " + bd + "." + schema + ".tbn1_" + tab + " AS t");
                file.WriteLine("        FULL JOIN #tmp_q_" + tab + " AS query on (");
                //--//Realizamos las comparaciones de claves
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        if (j[4].ToString() == "#")
                        {
                            i++;
                            if (i == campospk.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("        (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                            }
                            else
                            {
                                file.WriteLine("        (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                            }
                        }
                    }
                }
                file.WriteLine("        WHERE ");
                file.WriteLine("            t." + clave + " IS NULL OR");
                file.WriteLine("            cc IS NULL");
                //--//Realizamos las comparaciones de claves
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        i++;
                        if (i == 1)
                        {
                            file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                            file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                        }
                        else if (i == campos.Split(new Char[] { ',' }).Length)
                        {
                            file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                            file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                        }
                        else
                        {
                            file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                            file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                        }
                    }
                }
                file.WriteLine("");

                if (incremental == true)
                {
                    file.WriteLine("           END");
                    file.WriteLine("--------FIN BLOQUES--------");
                    file.WriteLine("");
                }
                #endregion "Control cambios SP"

                #region "ModificacionesTablasSP"
                //SP Insertamos datos en variables
                file.WriteLine("        SET @idx_reclim = 10000");
                file.WriteLine("        SELECT @count_all = count(1) from #tmp_" + tab);
                file.WriteLine("        SELECT @count_ins = count(1) from #tmp_" + tab + " where rr_mode='I'");
                file.WriteLine("");
                file.WriteLine("        --IF @count_all >= @idx_reclim --Si hay indices no unique se desactivan");
                file.WriteLine("        --BEGIN");
                file.WriteLine("        --END");
                file.WriteLine("");
                //SP Actualizamos los registros existentes con cambios
                file.WriteLine("        UPDATE " + bd + "." + schema + ".tbn1_" + tab + "");
                file.WriteLine("            SET");
                //--//Realizamos el Update, comparamos los campos
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("                " + j[0].ToString() + " = s." + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("                " + j[0].ToString() + " = s." + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("        FROM (");
                file.WriteLine("            SELECT");
                //--//Insertamos los campos a modificar
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("                " + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("                " + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("            FROM #tmp_" + tab);
                file.WriteLine("            WHERE rr_mode='U') s");
                //--//Realizamos las comparaciones de claves
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        if (j[4].ToString() == "#")
                        {
                            if (i == 0)
                            {
                                file.WriteLine("            WHERE (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + "=s." + j[0].ToString() + " OR (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL))");
                            }
                            else
                            {
                                file.WriteLine("                AND (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + "=s." + j[0].ToString() + " OR (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL))");
                            }
                            i++;
                        }
                    }
                }
                file.WriteLine("        SET @rc=@@ROWCOUNT;");
                file.WriteLine("");
                //SP Borramos registros que no existan
                file.WriteLine("        DELETE " + bd + "." + schema + ".tbn1_" + tab);
                file.WriteLine("        FROM " + bd + "." + schema + ".tbn1_" + tab + " AS tbn1_" + tab);
                file.WriteLine("        INNER JOIN #tmp_" + tab + " AS tmp");
                //--//Realizamos las comparaciones de claves
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        if (j[4].ToString() == "#")
                        {
                            i++;
                            if (i == 1)
                            {
                                file.WriteLine("        ON ((tbn1_" + tab + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL))");
                            }
                            else if (i == campospk.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("            AND (tbn1_" + tab + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL)))");
                            }
                            else
                            {
                                file.WriteLine("            AND (tbn1_" + tab + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL))");
                            }
                        }
                    }
                }
                file.WriteLine("        WHERE tmp.rr_mode = 'D'");
                file.WriteLine("        SET @rc = @rc + @@ROWCOUNT;");
                file.WriteLine("");

                //SP Desactivamos Indice de la tabla principal
                file.WriteLine("        IF @count_ins >= @idx_reclim");
                file.WriteLine("        BEGIN");
                file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.indexes WHERE name = 'IX_tbn1_" + tab + "_unique')");
                file.WriteLine("            ALTER INDEX IX_tbn1_" + tab + "_unique ON " + bd + "." + schema + ".tbn1_" + tab + " DISABLE;");
                file.WriteLine("        END");
                file.WriteLine("");
                //SP Insertamos nuevos registros
                file.WriteLine("        INSERT INTO " + bd + "." + schema + ".tbn1_" + tab + " WITH(TABLOCK) (" + campos.Replace("'","")+ ")");
                file.WriteLine("        SELECT");
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                       if (i == csv.Length)
                       {
                           file.WriteLine("            " + j[0].ToString());
                       }
                       else
                       {
                           file.WriteLine("            " + j[0].ToString() + ",");
                       }
}
                }
                file.WriteLine("            FROM #tmp_" + tab);
                file.WriteLine("            WHERE rr_mode='I';");
                file.WriteLine("        SET @rc = @rc + @@ROWCOUNT;");
                file.WriteLine("");
                file.WriteLine("        --IF @count_all >= @idx_reclim --Si hay indices no unique -> rebuild");
                file.WriteLine("        --BEGIN");
                file.WriteLine("        --END");
                file.WriteLine("");
                //SP Activamos indice de la tabla principal
                file.WriteLine("        IF @count_ins >= @idx_reclim");
                file.WriteLine("        BEGIN");
                file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.indexes WHERE name = 'IX_tbn1_" + tab + "_unique')");
                file.WriteLine("            ALTER INDEX IX_tbn1_" + tab + "_unique ON " + bd + "." + schema + ".tbn1_" + tab + " REBUILD;");
                file.WriteLine("        END");
                file.WriteLine("");
                if (incremental == true)
                {
                    file.WriteLine("--Insertar registros de incrementalidad en tabla ct_procesado");
                    file.WriteLine("        UPDATE dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado");
                    file.WriteLine("        SET ct_stg  = @ct_stg_final,");
                    file.WriteLine("            ct_norm = @ct_norm_final,");
                    file.WriteLine("            ct_dmr = @ct_dmr_final");
                    file.WriteLine("        WHERE procedimiento = @objeto; ");
                    file.WriteLine("");
                }
                file.WriteLine("        ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " WITH CHECK CHECK CONSTRAINT ALL");
                file.WriteLine("");
                #endregion "ModificacionesTablasSP"

                //SP Pie
                string pie = sc.pieLogSP(file, "ds");

                file.WriteLine("GO");
                file.WriteLine("");
                #endregion "SP"

                file.Close();
                file_exec.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al escribir en archivo " + nombrearchivo + "\r\n" + ex.Message, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }

            return "OK";
        }

        //DataMarts
        public string dm(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean incremental, Boolean CreateTable, Boolean ChangeTrack, Boolean IndexColumnStore)
        {
            string nombrearchivoexec = "";
            string fichero;
            string prefijo_tab = "";
            string tabNRM = "";
            string tabDM = "";
            string tabFact = "";
            string bd = "dbn1_dmr_dhyf";
            string bdnrm = "dbn1_norm_dhyf";
            string bdSP = "dbn1_stg_dhyf";
            string clave = "";
            string claveDim = "";
            string campos = "";
            string campospk = "";
            int indices = 0;
            string coma = ",";
            string[] csv2 = new string[0];

            int i = 0;

            foreach (string d in csv)
            {
                string[] j = d.Split(new Char[] { ';' });
                if (j[0].ToLower().Contains("#nombre"))
                {
                    tabNRM = j[1].ToString();
                    clave = j[3].ToString();
                }
                else if (j[0].ToLower().Contains("#prefijo"))
                {
                    prefijo_tab = j[1].ToString();
                }
                else if (!j[0].Contains("#"))
                {
                    if (!tabDM.Contains(j[5].ToString()))
                    {
                        tabDM = tabDM + j[5].ToString() + ";";
                    }
                    if (j[5].ToString().ToLower() == "f" || j[5].ToString().ToLower().Contains("fact"))
                    {
                        tabFact = j[5].ToString();
                    }
                }
            }
            tabDM = tabDM.Substring(0, tabDM.Length - 1);
            tabDM = tabDM.Replace(";" + tabFact, "").ToLower();

            //Generamos nombre fichero y obtenemos lineas, renombrando fichero actual
            nombrearchivo = "Script dimensional_" + prefijo_tab + "_dm.sql";
            nombrearchivoexec = "Exec dimensional_" + prefijo_tab + "_dm.sql";
            string[] lineas = new string[0];
            string dev = a.comprobarficheros(ref lineas, ruta, nombrearchivo, 1);

            fichero = ruta + nombrearchivo;
            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                file_exec.WriteLine("GO");
                a.generar_file_exec(file_exec, bd + ".dbo.tbn1_" + prefijo_tab + "_fact", bdSP, "dbo", "spn1_cargar_dm_" + prefijo_tab);

                //Documentación Exec
                file_exec.WriteLine("");
                file_exec.WriteLine("");
                file_exec.WriteLine("/*-----------------------------------------------");
                file_exec.WriteLine("---DOCUMENTACIÓN TABLAS---");
                file_exec.WriteLine("");

                //Script carga
                file.WriteLine("PRINT '" + nombrearchivo + "'");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Generado versión vb " + a.version);
                file.WriteLine("");
                file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                file.WriteLine("GO");
                file.WriteLine("");

                //Create Table Dimensiones
                #region Tablas_dim
                string[] tD = tabDM.Split(new Char[] { ';' });
                foreach (string tabDim in tD)
                {
                    //Obtenemos los campos de la tabla dimensional
                    Array.Resize(ref csv2, 0);
                    campos = "";
                    foreach (string d in csv)
                    {
                        claveDim = claveDim + "id_dim_" + tabDim;
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower().ToLower() == tabDim)
                        {
                            campos = campos + "'" + j[0] + "',";
                            Array.Resize(ref csv2, csv2.Length + 1);
                            csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[2].ToString() + ";";
                        }
                    }
                    campos = campos.Substring(0, campos.Length - 1);
                    claveDim = claveDim.Substring(0, claveDim.Length - 1);

                    //realizamos llamada a funciones para carga automatica
                    if (CreateTable == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("/*--------------------------------------");
                    }
                    file.WriteLine("--Begin table create/prepare -> tbn1_" + prefijo_tab + "_dim_" + tabDim);
                    file_exec.WriteLine("tbn1_" + prefijo_tab + "_dim_" + tabDim);
                    file.WriteLine("");

                    //Desactivamos CT
                    string ctdf = sc.changetracking(file, "tbn1_" + prefijo_tab + "_dim_" + tabDim, bd, "dbo", "des");

                    //Drop FKs
                    sc.borrarFK(file, bd, "dbo", prefijo_tab + "_dim_" + tabDim);

                    campospk = campos;
                    sc.regTablas(file, bd, "dbo", "tbn1_" + prefijo_tab + "_dim_" + tabDim, "id_dim_" + tabDim, campos, campospk, csv2, false, "dm");

                    if (CreateTable == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("--------------------------------------*/");
                    }
                    file.WriteLine("");

                    #region Activar CT comentado Dim
                    ////Activamos CT
                    //if (ChangeTrack == false)
                    //{
                    //    file.WriteLine("--------------------------------------");
                    //}
                    //else
                    //{
                    //    file.WriteLine("/*--------------------------------------");
                    //}
                    //string ctpf = sc.changetracking(file, "tbn1_" + prefijo_tab + "_dim_" + tabDim, bd, "dbo", "act");
                    //if (ChangeTrack == false)
                    //{
                    //    file.WriteLine("--------------------------------------");
                    //}
                    //else
                    //{
                    //    file.WriteLine("--------------------------------------*/");
                    //}
                    #endregion Activar CT comentado Dim

                    file.WriteLine("--End table create/prepare -> tbn1_" + prefijo_tab + "_dim_" + tabDim);
                    file.WriteLine("");
                }
                #endregion Tablas_dim

                //Creamos Table Fact
                #region Tabla_Fact
                if (CreateTable == false)
                {
                    file.WriteLine("--------------------------------------");
                }
                else
                {
                    file.WriteLine("/*--------------------------------------");
                }
                file.WriteLine("--Begin table create/prepare -> tbn1_" + prefijo_tab + "_fact");
                file_exec.WriteLine("tbn1_" + prefijo_tab + "_fact");
                file.WriteLine("");

                //Obtenemos los campos de la tabla dimensional
                Array.Resize(ref csv2, 0);
                campos = "";
                campospk = "";
                //Cargamos las claves de las tablas Dimensionales en la fact
                foreach (string tabDim in tD)
                {
                    campos = campos + "'id_dim_" + tabDim + "',";
                    campospk = campospk + "'xxxid_dim_" + tabDim + "',";
                    Array.Resize(ref csv2, csv2.Length + 1);
                    csv2[csv2.Length - 1] = "id_dim_" + tabDim + ";int;#;#;";
                }
                //Cargamos la PK de negocio (Campo Filter CSV)
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#") && j[9].ToString() == "#")
                    {
                        campos = campos + "'" + j[0].ToString() + "',";
                        campospk = campospk + "'xxx" + j[0].ToString() + "',";
                        Array.Resize(ref csv2, csv2.Length + 1);
                        csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[2].ToString() + ";" + j[9].ToString() + ";" + j[9].ToString();
                    }
                }
                //Cargamos los campos de la fact que hay en CSV
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#") && j[5].ToString() == tabFact)
                    {
                        campos = campos + "'" + j[0] + "',";
                        Array.Resize(ref csv2, csv2.Length + 1);
                        csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[2].ToString() + ";" + j[9].ToString() + ";" + j[9].ToString();
                    }
                }
                campos = campos.Substring(0, campos.Length - 1);
                campospk = campospk.Substring(0, campospk.Length - 1);

                //Desactivamos CT
                string ctd = sc.changetracking(file, "tbn1_" + prefijo_tab + "_fact", bd, "dbo", "des");

                //Drop FKs
                sc.borrarFK(file, bd, "dbo", prefijo_tab + "_fact");

                //Cargar tabla
                sc.regTablas(file, bd, "dbo", "tbn1_" + prefijo_tab + "_fact", "id", campos, campospk.Replace("xxx",""), csv2, false, "dm");

                if (CreateTable == false)
                {
                    file.WriteLine("--------------------------------------");
                }
                else
                {
                    file.WriteLine("--------------------------------------*/");
                }
                file.WriteLine("");

                #region Activar CT comentado fact
                ////Activamos CT
                //if (ChangeTrack == false)
                //{
                //    file.WriteLine("--------------------------------------");
                //}
                //else
                //{
                //    file.WriteLine("/*--------------------------------------");
                //}
                //string ctp = sc.changetracking(file, "tbn1_" + prefijo_tab + "_fact", bd, "dbo", "act");
                //if (ChangeTrack == false)
                //{
                //    file.WriteLine("--------------------------------------");
                //}
                //else
                //{
                //    file.WriteLine("--------------------------------------*/");
                //}
                #endregion Activar CT comentado fact

                file.WriteLine("--End table create/prepare -> tbn1_" + prefijo_tab + "_fact");
                file.WriteLine("");
                #endregion Tabla_Fact

                file_exec.WriteLine("");
                file_exec.WriteLine("---DOCUMENTACIÓN SP---");
                file_exec.WriteLine("");

                //SP Dimensiones
                #region "SP Dim"
                //SP Creamos Object temporal
                foreach (string tbDim in tD)
                {
                    file_exec.WriteLine("spn1_cargar_" + prefijo_tab + "_dim_" + tbDim);

                    file.WriteLine("USE " + bdSP);
                    file.WriteLine("GO");
                    file.WriteLine("IF EXISTS (SELECT 1 FROM " + bdSP + ".INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'spn1_cargar_" + prefijo_tab + "_dim_" + tbDim + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                    file.WriteLine("    DROP PROCEDURE dbo.spn1_cargar_" + prefijo_tab + "_dim_" + tbDim);
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("CREATE PROCEDURE dbo.spn1_cargar_" + prefijo_tab + "_dim_" + tbDim + "(@p_id_carga int) AS");
                    file.WriteLine("BEGIN");
                    file.WriteLine("");

                    //Cabecera
                    string cab = sc.cabeceraLogSP(file, bdSP, "dbo", "spn1_cargar_" + prefijo_tab + "_dim_" + tbDim, false,false);

                    //Cuerpo
                    file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_" + prefijo_tab + "_dim_" + tbDim + "') IS NOT NULL");
                    file.WriteLine("                DROP TABLE #tmp_" + prefijo_tab + "_dim_" + tbDim);
                    file.WriteLine("            CREATE table #tmp_" + prefijo_tab + "_dim_" + tbDim + "(");
                    file.WriteLine("                rr_mode varchar(1),");
                    file.WriteLine("                cc int,");
                    file.WriteLine("                id_dim_" + tbDim + " int,");
                    //--Claves--//
                    i = 0;
                    campos = "";
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            file.WriteLine("                t_" + j[0].ToString() + " " + j[1].ToString() + ",");
                            campos = campos + "xx." + j[0].ToString() + ",";
                        }
                    }
                    campos = campos.Substring(0, campos.Length - 1);
                    //--Campos--//
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            i++;
                            if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString());
                            }
                            else
                            {
                                file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("            );");
                    file.WriteLine("");
                
                    //Generamos la carga de datos de Dimensionales
                    file.WriteLine("    SELECT 1 AS cc, " + campos.Replace("xx.", "st."));
                    file.WriteLine("    INTO #result_extract");
                    file.WriteLine("    FROM (");
                    file.WriteLine("        SELECT " + campos.Replace("xx.", "t."));
                    file.WriteLine("        FROM " + bdnrm + ".dbo.tbn1_" + tabNRM + " AS t");
                    file.WriteLine("        GROUP BY " + campos.Replace("xx.", "t."));
                    file.WriteLine("    ) st");
                    file.WriteLine("    GROUP BY " + campos.Replace("xx.", "st."));
                    file.WriteLine("");
                    file.WriteLine("        ;WITH");
                    file.WriteLine("        query AS (");
                    file.WriteLine("            SELECT cc," + campos.Replace("xx.", ""));
                    file.WriteLine("            FROM #result_extract");
                    file.WriteLine("        )");
                    file.WriteLine("");
                    //SP Comparamos registros para indicar la acción a realizar
                    file.WriteLine("        INSERT INTO #tmp_" + prefijo_tab + "_dim_" + tbDim + " (rr_mode,cc,id_dim_" + tbDim + ","  + campos.Replace("xx.", "t_") + "," + campos.Replace("xx.", "") + ")");
                    file.WriteLine("        SELECT");
                    file.WriteLine("            rr_mode=");
                    file.WriteLine("            CASE");
                    file.WriteLine("                WHEN t.id_dim_" + tbDim + " IS NULL THEN 'I'");
                    file.WriteLine("                WHEN cc IS NULL THEN 'D'");
                    file.WriteLine("                ELSE 'U' END,");
                    file.WriteLine("            cc AS cc,");
                    file.WriteLine("            t.id_dim_" + tbDim + " AS id_dim_" + tbDim + ",");
                    //--//Incluimos las claves
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            file.WriteLine("            t." + j[0].ToString() + " AS t_" + j[0].ToString() + ",");
                        }
                        i++;
                    }
                    //--//Incluimos los campos
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            i++;
                            if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " AS t");
                    file.WriteLine("        FULL JOIN query on (");
                    //--//Realizamos las comparaciones de claves
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            i++;
                            if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                            }
                            else
                            {
                                file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                            }
                        }
                    }
                    file.WriteLine("        WHERE ");
                    file.WriteLine("            t.id_dim_" + tbDim + " IS NULL OR");
                    file.WriteLine("            cc IS NULL");
                    //--//Realizamos las comparaciones de los campos
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            i++;
                            if (campos.Split(new Char[] { ',' }).Length == 1)
                            {
                                file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                            }
                            else if (i == 1)
                            {
                                file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                            else if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                            }
                            else
                            {
                                file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                        }
                    }
                    file.WriteLine("");

                    // SP Insertamos datos en variables
                    file.WriteLine("        SET @idx_reclim = 10000");
                    file.WriteLine("        SELECT @count_all = count(1) from #tmp_" + prefijo_tab + "_dim_" + tbDim);
                    file.WriteLine("        SELECT @count_ins = count(1) from #tmp_" + prefijo_tab + "_dim_" + tbDim + " where rr_mode='I'");
                    file.WriteLine("");

                    //Desactivamos indices no unicos
                    indices = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[2].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            indices++;
                        }
                    }
                    if (indices == 0)
                    {
                        file.WriteLine("--        IF @count_all >= @idx_reclim --Si hay indices no unique se desactivan");
                        file.WriteLine("--        BEGIN");
                        file.WriteLine("--        END");
                    }
                    else
                    {
                        file.WriteLine("        IF @count_all >= @idx_reclim --Si hay indices no unique se desactivan");
                        file.WriteLine("        BEGIN");
                        i = 0;
                        foreach (string d in csv)
                        {
                            string[] j = d.Split(new Char[] { ';' });
                            if (!j[0].Contains("#") && j[2].Contains("#") && j[5].ToString().ToLower() == tbDim)
                            {
                                i++;
                                file.WriteLine("        IF EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_" + i + "') ");
                                file.WriteLine("            ALTER INDEX  IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_" + i + " ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " DISABLE; ");
                            }
                        }
                        file.WriteLine("        END");
                    }
                    file.WriteLine("");

                    //SP Borramos registros que no existan
                    file.WriteLine("        DELETE " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim);
                    file.WriteLine("        FROM " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " AS tbn1_" + prefijo_tab + "_dim_" + tbDim);
                    file.WriteLine("        INNER JOIN #tmp_" + prefijo_tab + "_dim_" + tbDim + " AS tmp");
                    //--//Realizamos las comparaciones de claves
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#") && j[5].ToString().ToLower() == tbDim)
                        {
                            i++;
                            if (i == campos.Split(new Char[] { ',' }).Length && i == 1)
                            {
                                file.WriteLine("        ON (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL))");
                            }
                            else if (i == 1)
                            {
                                file.WriteLine("        ON ((tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL))");
                            }
                            else if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("            AND (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL)))");
                            }
                            else
                            {
                                file.WriteLine("            AND (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + "=tmp.t_" + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_dim_" + tbDim + "." + j[0].ToString() + " IS NULL AND tmp.t_" + j[0].ToString() + " IS NULL))");
                            }
                        }
                    }
                    file.WriteLine("        WHERE tmp.rr_mode = 'D'");
                    file.WriteLine("        SET @rc=@@ROWCOUNT;");
                    file.WriteLine("");

                    //SP Desactivamos Indice de la tabla principal
                    file.WriteLine("        IF @count_ins >= @idx_reclim");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.indexes WHERE name = 'IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_unique')");
                    file.WriteLine("            ALTER INDEX IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_unique ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " DISABLE;");
                    file.WriteLine("        END");
                    file.WriteLine("");

                    //SP Insertamos nuevos registros
                    file.WriteLine("        INSERT INTO " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " WITH(TABLOCK) (" + campos.Replace("xx.", "") + ")");
                    file.WriteLine("        SELECT " + campos.Replace("xx.", ""));
                    file.WriteLine("            FROM #tmp_" + prefijo_tab + "_dim_" + tbDim);
                    file.WriteLine("            WHERE rr_mode='I';");
                    file.WriteLine("        SET @rc = @rc + @@ROWCOUNT;");
                    file.WriteLine("");

                    //SP Reconstruimos Indices de la tabla principal
                    if (indices == 0)
                    {
                        file.WriteLine("--        IF @count_all >= @idx_reclim --Si hay indices no unique -> rebuild");
                        file.WriteLine("--        BEGIN");
                        file.WriteLine("--        END");
                    }
                    else
                    {
                        file.WriteLine("        IF @count_all >= @idx_reclim --Si hay indices no unique -> rebuild");
                        file.WriteLine("        BEGIN");
                        i = 0;
                        foreach (string d in csv)
                        {
                            string[] j = d.Split(new Char[] { ';' });
                            if (!j[0].Contains("#") && j[2].Contains("#") && j[5].ToString().ToLower() == tbDim)
                            {
                                i++;
                                file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_" + i + "') ");
                                file.WriteLine("                ALTER INDEX IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_" + i + " ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " REBUILD; ");
                            }
                        }
                        file.WriteLine("        END");
                    }
                    file.WriteLine("");
                    file.WriteLine("        IF @count_ins >= @idx_reclim");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_unique') ");
                    file.WriteLine("                ALTER INDEX IX_tbn1_" + prefijo_tab + "_dim_" + tbDim + "_unique ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tbDim + " REBUILD; ");
                    file.WriteLine("        END");
                    file.WriteLine("");

                    //Pie
                    string pie = sc.pieLogSP(file, "dm");

                    file.WriteLine("GO");
                    file.WriteLine("");
                }

                #endregion "SP Dim"

                //SP Fact
                #region "SP Fact"
                file_exec.WriteLine("spn1_cargar_" + prefijo_tab + "_fact");

                file.WriteLine("USE " + bdSP);
                file.WriteLine("GO");
                file.WriteLine("IF EXISTS (SELECT 1 FROM " + bdSP + ".INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'spn1_cargar_" + prefijo_tab + "_fact' AND ROUTINE_TYPE = 'PROCEDURE')");
                file.WriteLine("    DROP PROCEDURE dbo.spn1_cargar_" + prefijo_tab + "_fact");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("CREATE PROCEDURE dbo.spn1_cargar_" + prefijo_tab + "_fact(@p_id_carga int) AS");
                file.WriteLine("BEGIN");
                file.WriteLine("");

                //Cabecera
                string cabF = sc.cabeceraLogSP(file, bdSP, "dbo", "spn1_cargar_" + prefijo_tab + "_fact", incremental, false);

                //Introducimos información para Metodo Incremental
                if (incremental == true)
                {
                    sc.regSP_Incremental(file);

                    //Cargamos Tabla q Temporal
                    file.WriteLine("            ALTER TABLE " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact NOCHECK CONSTRAINT ALL");
                    file.WriteLine("");
                }
                file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_q_" + prefijo_tab + "_fact') IS NOT NULL");
                file.WriteLine("                DROP TABLE #tmp_q_" + prefijo_tab + "_fact");
                file.WriteLine("            CREATE table #tmp_q_" + prefijo_tab + "_fact(");
                file.WriteLine("                cc int default 1,");
                //--Campos--//
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv2.Length)
                        {
                            file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString());
                        }
                        else
                        {
                            file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("            );");
                file.WriteLine("");

                //Cuerpo
                file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_" + prefijo_tab + "_fact') IS NOT NULL");
                file.WriteLine("                DROP TABLE #tmp_" + prefijo_tab + "_fact");
                file.WriteLine("            CREATE table #tmp_" + prefijo_tab + "_fact(");
                file.WriteLine("                rr_mode varchar(1),");
                file.WriteLine("                cc int,");
                file.WriteLine("                id int,");
                //--Claves--//
                //Cargamos las claves de las tablas Dimensionales en la fact
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[3].ToString() == "#")
                    {
                        file.WriteLine("                t_" + j[0].ToString() + " " + j[1].ToString() + ",");
                    }
                }
                //--Campos--//
                i = 0;
                coma = ",";
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        i++;
                        if (i == csv2.Length)
                        {
                            coma = ""; 
                        }
                        file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + coma);
                    }
                }
                file.WriteLine("            );");
                file.WriteLine("");

                if (incremental == true)
                {
                    file.WriteLine("            SELECT @es_carga_completa = es_carga_completa");
                    file.WriteLine("            FROM dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro");
                    file.WriteLine("            WHERE objeto = 'spn1_cargar_dm_" + prefijo_tab + "';");
                    file.WriteLine("");
                    file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_keys_" + prefijo_tab  + "_fact') IS NOT NULL");
                    file.WriteLine("                DROP TABLE #tmp_keys_" + prefijo_tab + "_fact");
                    file.WriteLine("            CREATE table #tmp_keys_" + prefijo_tab + "_fact(");
                    claveDim = "";
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[4].ToString() == "#")
                        {
                            claveDim = claveDim + j[0].ToString() + " " + j[1].ToString() + ", ";
                        }
                    }
                    claveDim = claveDim.Substring(0, claveDim.Length - 2);
                    file.WriteLine("                " + claveDim + ");");
                    file.WriteLine("");
                    file.WriteLine("--- Inicio Bloque común para Incremental y Full");
                    file.WriteLine("");
                    file.WriteLine("--- Fin Bloque común para Incremental y Full");
                    file.WriteLine("");
                    file.WriteLine("--------BLOQUE INCREMENTAL--------");
                    file.WriteLine("    IF @es_carga_completa = 0");
                    file.WriteLine("    BEGIN");
                    file.WriteLine("");
                    file.WriteLine("    ;WITH");
                    file.WriteLine("    query AS (");
                    //Generamos la carga de datos de Dimensionales
                    file.WriteLine("    SELECT  1 AS cc, ");
                    //--Campos--//
                    i = 0;
                    coma = ",";
                    campos = "";
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            i++;
                            if (i == csv2.Length)
                            {
                                coma = "";
                            }
                            campos = campos + "'" + j[0].ToString() + "',";
                            if (j[2].ToString() == "#" && j[3].ToString() == "#")
                            {
                                file.WriteLine("            tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + j[0].ToString() + " AS " + j[0].ToString() + coma);
                            }
                            else if (j[2].ToString() == "" && j[3].ToString() == "#")
                            {
                                file.WriteLine("            t." + j[0].ToString() + " AS " + j[0].ToString() + coma);
                            }
                            else
                            {
                                file.WriteLine("            sum(t." + j[0].ToString() + ") AS " + j[0].ToString() + coma);
                            }
                        }
                    }
                    campos = campos.Substring(0, campos.Length - 1);

                    file.WriteLine("    FROM " + bdnrm + ".dbo.tbn1_" + tabNRM + " t");
                    clave = "";
                    claveDim = "";
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[4].ToString() == "#")
                        {
                            clave = clave + j[0].ToString() + ", ";
                            claveDim = claveDim + "t." + j[0].ToString() + " = " + "CT." + j[0].ToString() + " AND ";
                        }
                    }
                    clave = clave.Substring(0, clave.Length - 2);
                    claveDim = claveDim.Substring(0, claveDim.Length - 5);
                    file.WriteLine("        INNER JOIN (SELECT " + clave + " FROM CHANGETABLE(CHANGES " + bdnrm + ".dbo.tbn1_" + tabNRM + ", @ct_norm_inicial) AS CT GROUP BY " + clave + ") as CT ON (" + claveDim + ")");
                    claveDim = "";
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[2].ToString() == "#")
                        {
                            i = 0;
                            file.WriteLine("        INNER JOIN " + bd + ".dbo.tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + " AS tbn1_" + prefijo_tab + j[0].ToString().Replace("id", ""));
                            foreach (string h in csv)
                            {
                                string[] k = h.Split(new Char[] { ';' });
                                if (k[5].ToString().ToLower() == j[0].ToString().Replace("id_dim_", ""))
                                {
                                    if (i == 0)
                                    {
                                        claveDim = claveDim + "(tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " = t." + k[0].ToString() + " OR (tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " IS NULL AND t." + k[0].ToString() + " IS NULL))";
                                    }
                                    else
                                    {
                                        claveDim = claveDim + "AND (tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " = t." + k[0].ToString() + " OR (tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " IS NULL AND t." + k[0].ToString() + " IS NULL))";
                                    }
                                    i++;
                                }
                            }
                            file.WriteLine("            ON (" + claveDim + ")");
                            claveDim = "";
                        }
                    }
                    file.WriteLine("        GROUP BY");
                    coma = ",";
                    i = 0;
                    foreach (string d in csv2)
                    {
                        i++;
                        if (i == campospk.Split(new Char[] { ',' }).Length)
                        {
                            coma = "";
                        }
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[2].ToString() == "#" && j[3].ToString() == "#")
                        {
                            file.WriteLine("                 tbn1_" + prefijo_tab + j[0].ToString().ToLower().Replace("id", "") + "." + j[0].ToString() + coma);
                        }
                        else if (j[2].ToString() == "" && j[3].ToString() == "#")
                        {
                            file.WriteLine("                 t." + j[0].ToString() + coma);
                        }
                    }
                    file.WriteLine("        )");
                    file.WriteLine("");

                    //SP Cargamos los registos en un Object Temporal
                    file.WriteLine("        INSERT INTO #tmp_q_" + prefijo_tab + "_fact");
                    file.WriteLine("        (" + campos.Replace("'", "") + ")");
                    file.WriteLine("        SELECT    ");
                    i = 0;
                    foreach (string d in csv2)
                    {
                        i++;
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv2.Length)
                            {
                                file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM    query");
                    file.WriteLine("");

                    //SP Comparamos registros para indicar la acción a realizar
                    file.WriteLine("        INSERT INTO #tmp_" + prefijo_tab + "_fact (rr_mode,cc,id," + campospk.Replace("xxx", "t_").Replace("'", "") + "," + campos.Replace("'", "") + ")");
                    file.WriteLine("        SELECT");
                    file.WriteLine("            rr_mode=");
                    file.WriteLine("            CASE");
                    file.WriteLine("                WHEN t.id IS NULL THEN 'I'");
                    file.WriteLine("                WHEN cc IS NULL THEN 'D'");
                    file.WriteLine("                ELSE 'U' END,");
                    file.WriteLine("            cc AS cc,");
                    file.WriteLine("            t.id AS id,");
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[3].ToString() == "#")
                        {
                            file.WriteLine("            t." + j[0].ToString() + " AS t_" + j[0].ToString() + ",");
                        }
                    }
                    //--//Incluimos los campos
                    i = 0;
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            i++;
                            if (i == csv2.Length)
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact AS t");
                    clave = "";
                    claveDim = "";
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[4].ToString() == "#")
                        {
                            clave = clave + j[0].ToString() + ", ";
                            claveDim = claveDim + "t." + j[0].ToString() + " = " + "CT." + j[0].ToString() + " AND ";
                        }
                    }
                    clave = clave.Substring(0, clave.Length - 2);
                    claveDim = claveDim.Substring(0, claveDim.Length - 5);
                    file.WriteLine("        INNER JOIN (SELECT " + clave + " FROM CHANGETABLE(CHANGES " + bdnrm + ".dbo.tbn1_" + tabNRM + ", @ct_norm_inicial) AS CT GROUP BY " + clave + ") as CT ON (" + claveDim + ")");
                    file.WriteLine("        FULL JOIN  #tmp_q_" + prefijo_tab + "_fact AS query ON ");
                    i = 0;
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (j[3].ToString() == "#")
                        {
                            i++;
                            if (i == 1)
                            {
                                file.WriteLine("            ((query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL))");
                            }
                            else if (i == campospk.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("            AND (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                            }
                            else
                            {
                                file.WriteLine("            AND (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL))");
                            }
                        }
                    }
                    file.WriteLine("        WHERE ");
                    file.WriteLine("            t.id IS NULL OR");
                    file.WriteLine("            cc IS NULL");
                    //--//Realizamos las comparaciones de los campos
                    i = 0;
                    foreach (string d in csv2)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            i++;
                            if (i == 1)
                            {
                                file.WriteLine("                OR (t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                            else if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                file.WriteLine("                OR t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                            }
                            else
                            {
                                file.WriteLine("                OR t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                        }
                    }
                    file.WriteLine("");
                    file.WriteLine("    END");
                    file.WriteLine("");
                    file.WriteLine("--------BLOQUE FULL--------");
                    file.WriteLine("    ELSE");
                    file.WriteLine("    BEGIN");
                    file.WriteLine("");
                }

                file.WriteLine("    ;WITH");
                file.WriteLine("    query AS (");
                //Generamos la carga de datos de Dimensionales
                file.WriteLine("    SELECT  1 AS cc, ");
                //--Campos--//
                i = 0;
                coma = ",";
                campos = "";
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        i++;
                        if (i == csv2.Length)
                        {
                            coma = "";
                        }
                        campos = campos + "'" + j[0].ToString() + "',";
                        if (j[2].ToString() == "#" && j[3].ToString() == "#")
                        {
                            file.WriteLine("            tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + j[0].ToString() + " AS " + j[0].ToString() + coma);
                        }
                        else if (j[2].ToString() == "" && j[3].ToString() == "#")
                        {
                            file.WriteLine("            t." + j[0].ToString() + " AS " + j[0].ToString() + coma);
                        }
                        else
                        {
                            file.WriteLine("            sum(t." + j[0].ToString() + ") AS " + j[0].ToString() + coma);
                        }
                    }
                }
                campos = campos.Substring(0, campos.Length - 1);

                file.WriteLine("    FROM " + bdnrm + ".dbo.tbn1_" + tabNRM + " t");
                claveDim = "";
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[2].ToString() == "#")
                    {
                        i = 0;
                        file.WriteLine("        INNER JOIN " + bd + ".dbo.tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + " AS tbn1_" + prefijo_tab + j[0].ToString().Replace("id", ""));
                        foreach (string h in csv)
                        {
                            string[] k = h.Split(new Char[] { ';' });
                            if (k[5].ToString().ToLower() == j[0].ToString().Replace("id_dim_", ""))
                            {
                                if (i == 0)
                                {
                                    claveDim = claveDim + "(tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " = t." + k[0].ToString() + " OR (tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " IS NULL AND t." + k[0].ToString() + " IS NULL))";
                                }
                                else
                                {
                                    claveDim = claveDim + "AND (tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " = t." + k[0].ToString() + " OR (tbn1_" + prefijo_tab + j[0].ToString().Replace("id", "") + "." + k[0].ToString() + " IS NULL AND t." + k[0].ToString() + " IS NULL))";
                                }
                                i++;
                            }
                        }
                        file.WriteLine("            ON (" + claveDim + ")");
                        claveDim = "";
                    }
                }
                file.WriteLine("    GROUP BY");
                coma = ",";
                i = 0;
                foreach (string d in csv2)
                {
                    i++;
                    if (i == campospk.Split(new Char[] { ',' }).Length)
                    {
                        coma = "";
                    }
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[2].ToString() == "#" && j[3].ToString() == "#")
                    {
                        file.WriteLine("        tbn1_" + prefijo_tab + j[0].ToString().ToLower().Replace("id","") + "." + j[0].ToString() + coma);
                    }
                    else if (j[2].ToString() == "" && j[3].ToString() == "#")
                    {
                        file.WriteLine("        t." + j[0].ToString() + coma);
                    }
                }
                file.WriteLine("        )");
                file.WriteLine("");

                //SP Insertamos los registros en un Object Temporal Inicial
                file.WriteLine("        INSERT INTO #tmp_q_" + prefijo_tab + "_fact");
                file.WriteLine("        (" + campos.Replace("'", "") + ")");
                file.WriteLine("        SELECT    ");
                i = 0;
                foreach (string d in csv2)
                {
                    i++;
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv2.Length)
                        {
                            file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("            query." + j[0].ToString() + " as " + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("        FROM    query");
                file.WriteLine("");

                //SP Comparamos registros para indicar la acción a realizar
                file.WriteLine("        INSERT INTO #tmp_" + prefijo_tab + "_fact (rr_mode,cc,id," + campospk.Replace("xxx", "t_").Replace("'","") + "," + campos.Replace("'", "") + ")");
                file.WriteLine("        SELECT");
                file.WriteLine("            rr_mode=");
                file.WriteLine("            CASE");
                file.WriteLine("                WHEN t.id IS NULL THEN 'I'");
                file.WriteLine("                WHEN cc IS NULL THEN 'D'");
                file.WriteLine("                ELSE 'U' END,");
                file.WriteLine("            cc AS cc,");
                file.WriteLine("            t.id AS id,");
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[3].ToString() == "#")
                    {
                        file.WriteLine("            t." + j[0].ToString() + " AS t_" + j[0].ToString() + ",");
                    }
                }
                //--//Incluimos los campos
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        i++;
                        if (i == csv2.Length)
                        {
                            file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("        FROM " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact AS t");
                file.WriteLine("        FULL JOIN  #tmp_q_" + prefijo_tab + "_fact AS query ON");
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[3]. ToString() == "#")
                    {
                        i++;
                        if (i == 1)
                        {
                            file.WriteLine("            ((query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL))");
                        }
                        else if (i == campospk.Split(new Char[] { ',' }).Length)
                        {
                            file.WriteLine("            AND (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                        }
                        else
                        {
                            file.WriteLine("            AND (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL))");
                        }
                    }
                }
                file.WriteLine("        WHERE ");
                file.WriteLine("            t.id IS NULL OR");
                file.WriteLine("            cc IS NULL");
                //--//Realizamos las comparaciones de los campos
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        i++;
                        if (i == 1)
                        {
                            file.WriteLine("                OR (t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                            file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                        }
                        else if (i == campos.Split(new Char[] { ',' }).Length)
                        {
                            file.WriteLine("                OR t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                            file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                        }
                        else
                        {
                            file.WriteLine("                OR t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                            file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                        }
                    }
                }
                file.WriteLine("");

                //SP se acaba las diferencias entre Full e Incremental
                if (incremental == true)
                {
                    file.WriteLine("    END");
                    file.WriteLine("--------FINAL BLOQUES--------");
                    file.WriteLine("");
                }

                //SP Insertamos datos en variables
                file.WriteLine("--Insertamos registros en variables");
                file.WriteLine("        SET @idx_reclim = 10000");
                file.WriteLine("        SELECT @count_all = count(1) from #tmp_" + prefijo_tab + "_fact");
                file.WriteLine("        SELECT @count_ins = count(1) from #tmp_" + prefijo_tab + "_fact" + " where rr_mode='I'");
                file.WriteLine("");

                //Desactivamos indices no unicos
                file.WriteLine("        IF @count_all >= @idx_reclim --Si hay indices no unique se desactivan");
                file.WriteLine("        BEGIN");
                i = 0;
                foreach (string tabDim in tD)
                {
                    i++;
                        file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_tbn1_" + prefijo_tab + "_fact_" + i + "') ");
                        file.WriteLine("                ALTER INDEX  IX_tbn1_" + prefijo_tab + "_fact_"+ i + " ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact DISABLE; ");
                }
                file.WriteLine("        END");
                file.WriteLine("");

                //SP Actualizamos registros existentes que se hayan modificado en DS
                file.WriteLine("        UPDATE " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact");
                file.WriteLine("        SET");
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        i++;
                        if (i == csv2.Length)
                        {
                            file.WriteLine("            " + j[0].ToString() + " = s." + j[0].ToString());
                        }
                        else
                        {
                            file.WriteLine("            " + j[0].ToString() + " = s." + j[0].ToString() + ",");
                        }
                    }
                }
                file.WriteLine("        FROM (");
                file.WriteLine("            SELECT " + campos.Replace("'",""));
                file.WriteLine("            FROM #tmp_" + prefijo_tab + "_fact");
                file.WriteLine("            WHERE rr_mode='U') s");
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[3].ToString() == "#")
                    {
                        if (i == 0)
                        {
                            file.WriteLine("        WHERE  (" + bd + ".dbo.tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " = s." + j[0].ToString() + " OR (" + bd + ".dbo.tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL)) ");
                        }
                        else
                        {
                            file.WriteLine("        	AND (" + bd + ".dbo.tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " = s." + j[0].ToString() + " OR (" + bd + ".dbo.tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL)) ");
                        }
                        i++;
                    }
                }
                file.WriteLine("        SET @rc = @@ROWCOUNT;");
                file.WriteLine("");

                //SP Borramos registros que no existan
                file.WriteLine("        DELETE " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact");
                file.WriteLine("        FROM " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact" + " AS tbn1_" + prefijo_tab + "_fact");
                file.WriteLine("        INNER JOIN #tmp_" + prefijo_tab + "_fact AS tmp");
                //--//Realizamos las comparaciones de claves
                i = 0;
                foreach (string d in csv2)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[3].ToString() == "#")
                    {
                        i++;
                        if (i == 1)
                        {
                            file.WriteLine("            ON ((tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " = tmp." + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " IS NULL AND tmp." + j[0].ToString() + " IS NULL))");
                        }
                        else if (i == campospk.Split(new Char[] { ',' }).Length)
                        {
                            file.WriteLine("                AND (tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " = tmp." + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " IS NULL AND tmp." + j[0].ToString() + " IS NULL)))");
                        }
                        else
                        {
                            file.WriteLine("                AND (tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " = tmp." + j[0].ToString() + " OR (tbn1_" + prefijo_tab + "_fact." + j[0].ToString() + " IS NULL AND tmp." + j[0].ToString() + " IS NULL))");
                        }
                    }
                }
                file.WriteLine("        WHERE tmp.rr_mode = 'D'");
                file.WriteLine("        SET @rc = @rc + @@ROWCOUNT;");
                file.WriteLine("");

                //SP Desactivamos Indice de la tabla principal
                file.WriteLine("        IF @count_ins >= @idx_reclim");
                file.WriteLine("        BEGIN");
                file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.indexes WHERE name = 'IX_tbn1_" + prefijo_tab + "_fact_unique')");
                file.WriteLine("            ALTER INDEX IX_tbn1_" + prefijo_tab + "_fact_unique ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact DISABLE;");
                file.WriteLine("        END");
                file.WriteLine("");

                //SP Insertamos nuevos registros
                file.WriteLine("        INSERT INTO " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact WITH(TABLOCK) (" + campos.Replace("'", "") + ")");
                file.WriteLine("        SELECT " + campos.Replace("'", ""));
                file.WriteLine("            FROM #tmp_" + prefijo_tab + "_fact");
                file.WriteLine("            WHERE rr_mode='I';");
                file.WriteLine("        SET @rc = @rc + @@ROWCOUNT;");
                file.WriteLine("");

                //SP Reconstruimos Indices de la tabla principal
                file.WriteLine("        IF @count_all >= @idx_reclim --Si hay indices no unique -> rebuild");
                file.WriteLine("        BEGIN");
                i = 0;
                foreach (string tabDim in tD)
                {
                    i++;
                    file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_tbn1_" + prefijo_tab + "_fact_" + i + "') ");
                    file.WriteLine("                ALTER INDEX  IX_tbn1_" + prefijo_tab + "_fact_" + i + " ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact REBUILD; ");
                }
                file.WriteLine("        END");
                file.WriteLine("");
                file.WriteLine("        IF @count_ins >= @idx_reclim");
                file.WriteLine("        BEGIN");
                file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_tbn1_" + prefijo_tab + "_fact_unique') ");
                file.WriteLine("                ALTER INDEX IX_tbn1_" + prefijo_tab + "_fact_unique " + " ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact REBUILD; ");
                file.WriteLine("        END");
                file.WriteLine("");

                if (incremental == true)
                {
                    file.WriteLine("        UPDATE dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado");
                    file.WriteLine("        SET ct_stg  = @ct_stg_final,");
                    file.WriteLine("            ct_norm = @ct_norm_final,");
                    file.WriteLine("            ct_dmr  = @ct_dmr_final");
                    file.WriteLine("        WHERE procedimiento = @objeto;");
                    file.WriteLine("");
                    file.WriteLine("        ALTER TABLE dbn1_dmr_dhyf.dbo.tbn1_" + prefijo_tab + "_fact WITH CHECK CHECK CONSTRAINT ALL");
                    file.WriteLine("");
                }

                //Pie
                string pieF = sc.pieLogSP(file, "dm");

                file.WriteLine("GO");
                file.WriteLine("");

                #endregion "SP Fact"

                //SP Ejecuta los SP
                #region SP_carga_dm
                file_exec.WriteLine("");
                file_exec.WriteLine("--SP Ejecuta todos SP--");
                file_exec.WriteLine("");
                file_exec.WriteLine("spn1_cargar_dm_" + prefijo_tab);
                file_exec.WriteLine("");

                file.WriteLine("USE " + bdSP);
                file.WriteLine("GO");
                file.WriteLine("IF EXISTS (SELECT 1 FROM " + bdSP + ".INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'spn1_cargar_dm_" + prefijo_tab + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                file.WriteLine("    DROP PROCEDURE dbo.spn1_cargar_dm_" + prefijo_tab);
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("CREATE PROCEDURE dbo.spn1_cargar_dm_" + prefijo_tab + "(@p_id_carga int) AS");
                file.WriteLine("BEGIN");
                file.WriteLine("");

                //Cabecera
                string cabSP = sc.cabeceraLogSP(file, bdSP, "dbo", "spn1_cargar_dm_" + prefijo_tab, false, false);

                //Cuerpo SP
                //NOCHECK CONSTRAINT ALL
                foreach (string tabDim in tD)
                {
                    file.WriteLine("    ALTER TABLE " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tabDim + " NOCHECK CONSTRAINT ALL");
                }
                file.WriteLine("    ALTER TABLE " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact NOCHECK CONSTRAINT ALL");
                file.WriteLine("");

                //EXEC SP
                foreach (string tabDim in tD)
                {
                    file.WriteLine("    EXEC " + bdSP + ".dbo.spn1_cargar_" + prefijo_tab + "_dim_" + tabDim + " @p_id_carga");
                }
                file.WriteLine("");
                
                //Desactivamos indice ColumnStore
                file.WriteLine("    --Borramos todos los indices NonCluster ColumnStore de la fact antes de cargarla");
                file.WriteLine("    DECLARE @ncindex nvarchar(128), @nmindex nvarchar(128), @sqlcmd nvarchar(max)");
                file.WriteLine("    DECLARE index_cursor CURSOR FOR");
                file.WriteLine("    SELECT name, type_desc FROM " + bd + ".SYS.INDEXES WHERE object_id = OBJECT_ID('" + bd + ".dbo.tbn1_" + prefijo_tab + "_fact')");
                file.WriteLine("    OPEN index_cursor");
                file.WriteLine("    FETCH NEXT FROM index_cursor INTO @nmindex, @ncindex");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        IF (@ncindex = 'NONCLUSTERED COLUMNSTORE')");
                file.WriteLine("        BEGIN");
                file.WriteLine("            SET @sqlcmd = 'DROP INDEX ' + @nmindex + ' ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact'");
                file.WriteLine("            EXEC (@sqlcmd)");
                file.WriteLine("        END");
                file.WriteLine("        FETCH NEXT FROM index_cursor INTO @nmindex, @ncindex");
                file.WriteLine("    END");
                file.WriteLine("    CLOSE index_cursor");
                file.WriteLine("    DEALLOCATE index_cursor");
                file.WriteLine("");

                //Ejectumos la carga de datos en la Fact
                file.WriteLine("    EXEC " + bdSP + ".dbo.spn1_cargar_" + prefijo_tab + "_fact @p_id_carga");
                file.WriteLine("");
                if (IndexColumnStore == true)
                {
                    file.WriteLine("");
                    //Activamos indice ColumnStore
                    file.WriteLine("    --Creamos de nuevo el índice ColumnStore si no existe");
                    file.WriteLine("    IF	EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='tbn1_" + prefijo_tab + "_fact')");
                    file.WriteLine("        AND NOT EXISTS (SELECT 1 FROM " + bd + ".sys.indexes WHERE name='IDX_CS_tbn1_" + prefijo_tab + "_fact' AND object_id = OBJECT_ID('" + bd + ".dbo.tbn1_" + prefijo_tab + "_fact'))");
                    file.WriteLine("    CREATE NONCLUSTERED COLUMNSTORE INDEX IDX_CS_tbn1_" + prefijo_tab + "_fact ON " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact");
                    file.WriteLine("        (id," + campos.Replace("'",""));
                    file.WriteLine("        )WITH (DROP_EXISTING = OFF)");
                    file.WriteLine("");
                }
                //WITH CHECK CHECK CONSTRAINT ALL
                foreach (string tabDim in tD)
                {
                    file.WriteLine("    ALTER TABLE " + bd + ".dbo.tbn1_" + prefijo_tab + "_dim_" + tabDim + " WITH CHECK CHECK CONSTRAINT ALL");
                }
                file.WriteLine("    ALTER TABLE " + bd + ".dbo.tbn1_" + prefijo_tab + "_fact WITH CHECK CHECK CONSTRAINT ALL");

                //Pie
                string pieSP = sc.pieLogSP(file, "dmSP");

                file.WriteLine("GO");
                file.WriteLine("");
                #endregion SP_carga_dm

                //Fin Script EXEC
                file_exec.WriteLine("-----------------------------------------------");
                file_exec.WriteLine("---------------------------------------------*/");

                file.Close();
                file_exec.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }

            return "OK";
        }

    }
}
