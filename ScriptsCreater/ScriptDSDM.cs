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

        //Table
        public string table(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean incremental)
            {

                string dev  = ds(archivo, csv, ruta, ref nombrearchivo, incremental);
                return "OK";
            }

        //DataStore
        public string ds(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean incremental)
        {
            string fichero;
            string tab = "";
            string bd = "";
            string clave = "";
            string campos = "";
            string campospk = "";
            string camposfilter = "";
            string schema = "dbo";

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
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            campospk = campospk.Substring(0, campospk.Length - 1);
            if (camposfilter.Length > 0)
            { 
                camposfilter = camposfilter.Substring(0, camposfilter.Length - 1);
            }
            //Si no tenemos valor clave, lo generamos
            if (campos.Contains(clave))
            {
                clave = "";
            }
            else if (clave == "")
            {
                clave = "id_tbn1_" + tab;
            }

            //Generamos nombre fichero y obtenemos lineas, renombrando fichero actual
            nombrearchivo = "Script normalizado_" + tab + ".sql";
            string[] lineas = new string[0];
            string dev = a.comprobarficheros(ref lineas, ruta, nombrearchivo, 1);
            DataTable valorquery = a.valorQuery(lineas, csv, "ds", incremental);

            fichero = ruta + nombrearchivo;
            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);


                file.WriteLine("PRINT 'Script normalizado_" + tab + ".sql'");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Generado versión vb " + a.version);
                file.WriteLine("");
                file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                file.WriteLine("GO");
                file.WriteLine("");

                #region "Tabla"

                file.WriteLine("/*--Begin table create/prepare -> tbn1_" + tab);
                file.WriteLine("");
                //Desactivamos CT
                string[] ctd = a.changetracking("tbn1_" + tab, bd, "dbo", "des");
                foreach (string lin in ctd)
                {
                    file.WriteLine(lin);
                }

                //Drop FKs
                file.WriteLine("--Drop FKs");
                file.WriteLine("DECLARE @fk_name nvarchar(150)");
                file.WriteLine("DECLARE @t_name nvarchar(150)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("        SELECT  obj.name AS fk_name,");
                file.WriteLine("        --sch.name AS [schema_name],");
                file.WriteLine("        tab1.name AS table_name");
                file.WriteLine("        --col1.name AS [column],");
                file.WriteLine("        --tab2.name AS [referenced_table],");
                file.WriteLine("        --col2.name AS [referenced_column]");
                file.WriteLine("        FROM " + bd + ".sys.foreign_key_columns fkc");
                file.WriteLine("            INNER JOIN " + bd + ".sys.objects obj");
                file.WriteLine("                ON obj.object_id = fkc.constraint_object_id");
                file.WriteLine("            INNER JOIN " + bd + ".sys.tables tab1");
                file.WriteLine("                ON tab1.object_id = fkc.parent_object_id");
                file.WriteLine("            INNER JOIN " + bd + ".sys.schemas sch");
                file.WriteLine("                ON tab1.schema_id = sch.schema_id");
                file.WriteLine("            INNER JOIN " + bd + ".sys.columns col1");
                file.WriteLine("                ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id");
                file.WriteLine("            INNER JOIN " + bd + ".sys.tables tab2");
                file.WriteLine("                ON tab2.object_id = fkc.referenced_object_id");
                file.WriteLine("            INNER JOIN " + bd + ".sys.columns col2");
                file.WriteLine("                ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id");
                file.WriteLine("        WHERE tab2.name = 'tbn1_" + tab + "'");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @fk_name, @t_name");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + ".' + @t_name + ' DROP CONSTRAINT ' + @fk_name");
                file.WriteLine("        EXEC (@sqlcmd)");
                file.WriteLine("        FETCH NEXT FROM @cursor INTO @fk_name, @t_name");
                file.WriteLine("    END");
                file.WriteLine("END");
                file.WriteLine("CLOSE @cursor");
                file.WriteLine("DEALLOCATE @cursor");
                file.WriteLine("GO");
                file.WriteLine("");

                //Create Table
                file.WriteLine("--Create Table");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='tbn1_" + tab + "')");
                file.WriteLine("CREATE TABLE " + bd + "." + schema + ".tbn1_" + tab + "(");
                if (clave != "")
                {
                    file.WriteLine("    " + clave + " int IDENTITY(1,1),");
                }
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            file.WriteLine("    " + j[0].ToString() + " " + j[1].ToString());
                        }
                        else
                        {
                            file.WriteLine("        " + j[0].ToString() + " " + j[1].ToString() + ",");
                        }
                    }
                }
                file.WriteLine(")");
                file.WriteLine("WITH (DATA_COMPRESSION=PAGE)");
                file.WriteLine("GO");
                file.WriteLine("");

                //Borramos Constraint
                file.WriteLine("--Drop all Constraints");
                file.WriteLine("DECLARE @constraint nvarchar(128)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("    SELECT constraint_name FROM " + bd + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS");
                file.WriteLine("    WHERE table_schema = 'dbo'");
                file.WriteLine("    AND table_name = 'tbn1_" + tab + "'");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @constraint");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " DROP CONSTRAINT ' + @constraint");
                file.WriteLine("        EXEC (@sqlcmd)");
                file.WriteLine("        FETCH NEXT FROM @cursor INTO @constraint");
                file.WriteLine("    END");
                file.WriteLine("END");
                file.WriteLine("CLOSE @cursor");
                file.WriteLine("DEALLOCATE @cursor");
                file.WriteLine("GO");
                file.WriteLine("--Drop all non-clustered index");
                file.WriteLine("");

                //Borramos Indices
                file.WriteLine("--Borramos Indices");
                file.WriteLine("USE " + bd);
                file.WriteLine("GO");
                file.WriteLine("DECLARE @ncindex nvarchar(128)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("    SELECT name FROM " + bd + "." + schema + ".SYSINDEXES");
                file.WriteLine("    WHERE id = OBJECT_ID('tbn1_" + tab + "')");
                file.WriteLine("    AND indid > 1 AND indid < 255 ");
                file.WriteLine("    AND INDEXPROPERTY(id, name, 'IsStatistics') = 0");
                file.WriteLine("    ORDER BY indid DESC");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @ncindex");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'DROP INDEX ' + @ncindex + ' ON " + bd + "." + schema + ".tbn1_" + tab + "'");
                file.WriteLine("        EXEC (@sqlcmd)");
                file.WriteLine("        FETCH NEXT FROM @cursor INTO @ncindex");
                file.WriteLine("    END");
                file.WriteLine("END");
                file.WriteLine("CLOSE @cursor");
                file.WriteLine("DEALLOCATE @cursor");
                file.WriteLine("GO");
                file.WriteLine("");

                //Añadimos columnas si no existen
                file.WriteLine("--Add all Columns (if not exist)");
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='tbn1_" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "')");
                        file.WriteLine("ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " ADD " + j[0].ToString() + " " + j[1].ToString());
                        file.WriteLine("GO");
                    }
                }
                file.WriteLine("");

                //Borramos columnas que no existan el CSV
                file.WriteLine("--Drop not used columns");
                file.WriteLine("DECLARE @column nvarchar(128)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("    SELECT column_name FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS");
                file.WriteLine("    WHERE table_schema = 'dbo'");
                file.WriteLine("    AND table_name = 'tbn1_" + tab + "'");
                file.WriteLine("    AND column_name NOT IN ('" + clave + "'," + campos + ")");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @column");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " DROP COLUMN ' + @column");
                file.WriteLine("        EXEC (@sqlcmd)");
                file.WriteLine("        FETCH NEXT FROM @cursor INTO @column");
                file.WriteLine("    END");
                file.WriteLine("END");
                file.WriteLine("CLOSE @cursor");
                file.WriteLine("DEALLOCATE @cursor");
                file.WriteLine("GO");
                file.WriteLine("");

                //Adjuntamos tipos de Campos
                file.WriteLine("--Adjust column types");
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (j[4].ToString() == "#")
                        {
                            file.WriteLine("ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                        }
                        else
                        {
                            file.WriteLine("ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NULL");
                        }
                        file.WriteLine("GO");
                    }
                }
                file.WriteLine("");

                //Añadimos PK e Index
                file.WriteLine("--Add PK if not exists");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_CATALOG = '" + bd + "' AND TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'tbn1_" + tab + "' AND CONSTRAINT_NAME = 'PK_tbn1_" + tab + "' AND CONSTRAINT_TYPE = 'PRIMARY KEY')");
                file.WriteLine("    ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " ADD CONSTRAINT PK_tbn1_" + tab + " PRIMARY KEY NONCLUSTERED (" + campospk.Replace("t_", "") + ")");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Create indexes if not exist");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + "." + schema + ".SYSINDEXES WHERE name = 'IX_tbn1_" + tab + "_cluster') ");
                file.WriteLine("    CREATE UNIQUE CLUSTERED INDEX IX_tbn1_" + tab + "_cluster ON " + bd + "." + schema + ".tbn1_" + tab + " (" + clave + ")");
                file.WriteLine("");
                file.WriteLine("--Add FKs if necessary");
                file.WriteLine("");

                //Activamos CT
                string[] cta = a.changetracking("tbn1_" + tab, bd, "dbo", "act");
                foreach (string lin in cta)
                {
                    file.WriteLine(lin);
                }
                file.WriteLine("--End table create/prepare -> tbn1_" + tab + "*/");
                file.WriteLine("");
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
                string[] cab2 = a.cabeceraLogSP("dbn1_stg_dhyf", "dbo", "spn1_cargar_normalizado_" + tab, incremental);

                foreach (string l1 in cab2)
                {
                    file.WriteLine(l1);
                }

                if (incremental == true)
                {
                    //SP Registro del SP en tabla de control de cargas incrementales y obtención de datos en variables
                    string[] sp_inc = a.regSP_Incremental();
                    foreach (string l1 in sp_inc)
                    {
                        file.WriteLine(l1);
                    }
                }

                //SP Creamos Object Temporal
                file.WriteLine("            ALTER TABLE " + bd + "." + schema + ".tbn1_" + tab + " NOCHECK CONSTRAINT ALL");
                file.WriteLine("            IF OBJECT_ID('tempdb..#tmp_q_" + tab + "') IS NOT NULL");
                file.WriteLine("                DROP TABLE #tmp_q_" + tab + "");
                file.WriteLine("            CREATE table #tmp_q_" + tab + "(");
                file.WriteLine("                cc int default 1,");
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
                //SP si es incremental incluimos la comprobación del registro en tipo de carga y metemos la parte comun de Full / Incremental
                if (incremental == true)
                {
                    file.WriteLine("            select @es_carga_completa = es_carga_completa");
                    file.WriteLine("            from dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro");
                    file.WriteLine("            where objeto = @objeto;");
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
                    dr = valorquery.Select("codScript=1", "orden ASC");
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
                    dr = valorquery.Select("codScript=2", "orden ASC");
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
                    dr = valorquery.Select("codScript=3", "orden ASC");
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
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv.Length)
                                file.WriteLine("                OR t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                            }
                    }
                    file.WriteLine("        END");
                    file.WriteLine("--------BLOQUE FULL--------");
                    file.WriteLine("        ELSE");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("");

                    dr = null;
                    dr = valorquery.Select("codScript=4", "orden ASC");
                    foreach (DataRow l2 in dr)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                }
                else
                {
                    //SP Creamos la insert de carga de datos
                    dr = null;
                    dr = valorquery.Select("codScript=1", "orden ASC");
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
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        file.WriteLine("                OR t." + j[0].ToString() + "<>query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                        file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                    }
                }
                file.WriteLine("");

                if (incremental == true)
                {
                    file.WriteLine("           END");
                    file.WriteLine("--------FIN BLOQUES--------");
                    file.WriteLine("");
                }
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
                file.WriteLine("        SET @rc=@rc + @@ROWCOUNT;");
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
                file.WriteLine("        SET @rc=@rc + @@ROWCOUNT;");
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
                string[] pie = a.pieLogSP("maestro");

                foreach (string l1 in pie)
                {
                    file.WriteLine(l1);
                }

                file.WriteLine("GO");
                file.WriteLine("");
                #endregion "SP"

                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al escribir en archivo " + nombrearchivo + "\r\n" + ex.Message, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }

            return "OK";
        }

        //DataMarts
        public string dm(string archivo, string[] csv, string ruta, ref string nombrearchivo)
        {


            string fichero = ruta + nombrearchivo;
            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);


                file.WriteLine("PRINT '" + archivo + "'");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Generado versión vb " + a.version);
                file.WriteLine("");
                file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("USE dbn1_stg_dhyf");
                file.WriteLine("GO");


                //SP Cabecera
                string[] cab2 = a.cabeceraLogSP("dbn1_stg_dhyf", "dbo", "spn1_" + archivo, false);

                foreach (string l1 in cab2)
                {
                    file.WriteLine(l1);
                }




                //SP Pie
                string[] pie = a.pieLogSP("maestro");

                foreach (string l1 in pie)
                {
                    file.WriteLine(l1);
                }

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

    }
}
