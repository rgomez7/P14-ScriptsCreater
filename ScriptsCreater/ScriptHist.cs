using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsCreater
{
    class ScriptHist
    {
        Acciones a = new Acciones();

        public string hist(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean claveAuto)
        {
            int i = 0;
            string bd = "";
            string tab = "";
            string clave = "";
            string campospk = "";
            string campos = "";
            string tipobd = "";
            string schema = "";
            string cabtab = "";
            string[] lineas = new string[0];
            string dev = "";

            foreach (string d in csv)
            {
                string[] j = d.Split(new Char[] { ';' });
                if (j[0].Contains("#nombre"))
                {
                    bd = j[2];
                    tab = j[1];
                    clave = j[3];
                }
                else if (!j[0].Contains("#"))
                {
                    campos = campos + "'" + j[0] + "',";
                    if (j[4] == "#")
                    {
                        campospk = campospk + "t_" + j[0] + ",";
                    }
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            campospk = campospk.Substring(0, campospk.Length - 1);
            //Si no tenemos valor clave, lo generamos
            clave = "id_" + tab;
            if (claveAuto == true)
            {
                campos = campos + ",'" + clave + "'";
            }

            //Asignamos nombre al nombrearchivo
            if (bd.Contains("dmr"))
            {
                tipobd = "dimensional";
                schema = "dmr";
            }
            else if (bd.Contains("stg"))
            {
                tipobd = "staging";
                schema = "stg";
            }
            else if (bd.Contains("norm"))
            {
                tipobd = "normalizado";
                schema = "norm";
            }
            else
            {
                tipobd = "";
                schema = "";
            }

            //Comprobamos la cabecera de la tabla
            if (archivo.Substring(0, 4) != "tbn1")
            {
                cabtab = "tbn1_";
            }
            else if (archivo.Substring(4, 1) == "_")
            {
                cabtab = archivo.Substring(0, 5);
            }
            else
            {
                cabtab = archivo.Substring(0, 4);
            }

            nombrearchivo = "Script TL " + tipobd + "_" + cabtab + tab + "_tracelog_TL.sql";
            //nombrearchivo = nombrearchivo.Replace("xxx", tipobd);
            string fichero = ruta + nombrearchivo;
            dev = a.comprobarficheros(ref lineas, ruta, nombrearchivo, 1);

            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);


                file.WriteLine("PRINT '" + nombrearchivo + "'");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Generado versión vb " + a.version);
                file.WriteLine("");
                file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                file.WriteLine("GO");
                file.WriteLine("");

                file.WriteLine("--Begin table create/prepare -> " + cabtab + tab + "_tracelog");
                file.WriteLine("");

#region "Registramos nuevo DS"
                //Create Table
                file.WriteLine("--Create Table");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + cabtab + tab + "_tracelog')");
                file.WriteLine("CREATE TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog(");
                file.WriteLine("        " + clave + "_tracelog int IDENTITY(1,1),");
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        file.WriteLine("        " + j[0].ToString() + " " + j[1].ToString() + ",");
                    }
                }
                if (claveAuto == true)
                {
                    file.WriteLine("       " + clave + " int,");
                }
                file.WriteLine("       ctct_fec_procesado datetime,");
                file.WriteLine("       ctct_tipo_operacion varchar(15)");

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
                file.WriteLine("    SELECT constraint_name FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.TABLE_CONSTRAINTS");
                file.WriteLine("    WHERE table_schema = '" + schema + "'");
                file.WriteLine("    AND table_name = '" + cabtab + tab + "_tracelog'");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @constraint");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog DROP CONSTRAINT ' + @constraint");
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
                file.WriteLine("USE dbn1_hist_dhyf");
                file.WriteLine("GO");
                file.WriteLine("DECLARE @ncindex nvarchar(128)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("    SELECT name FROM dbn1_hist_dhyf." + schema + ".SYSINDEXES");
                file.WriteLine("    WHERE id = OBJECT_ID('" + cabtab + tab + "_tracelog')");
                file.WriteLine("    AND indid > 1 AND indid < 255 ");
                file.WriteLine("    AND INDEXPROPERTY(id, name, 'IsStatistics') = 0");
                file.WriteLine("    ORDER BY indid DESC");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @ncindex");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'DROP INDEX ' + @ncindex + ' ON dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog'");
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
                        file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + cabtab + tab + "_tracelog' AND COLUMN_NAME='" + j[0].ToString() + "')");
                        file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ADD " + j[0].ToString() + " " + j[1].ToString());
                        file.WriteLine("GO");
                    }
                }
                if (claveAuto == true)
                {
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + cabtab + tab + "_tracelog' AND COLUMN_NAME='" + clave + "')");
                    file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ADD " + clave + " int");
                    file.WriteLine("GO");
                }
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + cabtab + tab + "_tracelog' AND COLUMN_NAME='ctct_fec_procesado')");
                file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ADD ctct_fec_procesado datetime");
                file.WriteLine("GO");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + cabtab + tab + "_tracelog' AND COLUMN_NAME='ctct_tipo_operacion')");
                file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ADD ctct_tipo_operacion varchar(15)");
                file.WriteLine("GO");
                file.WriteLine("");

                //Borramos columnas que no existan el CSV
                file.WriteLine("--Drop not used columns");
                file.WriteLine("DECLARE @column nvarchar(128)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("    SELECT column_name FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.COLUMNS");
                file.WriteLine("    WHERE table_schema = '" + schema + "'");
                file.WriteLine("    AND table_name = '" + cabtab + tab + "_tracelog'");
                file.WriteLine("    AND column_name NOT IN ('" + clave + "_tracelog'," + campos + ",'ctct_fec_procesado','ctct_tipo_operacion')");
                file.WriteLine("    OPEN @cursor");
                file.WriteLine("    FETCH NEXT FROM @cursor INTO @column");
                file.WriteLine("    WHILE @@FETCH_STATUS = 0");
                file.WriteLine("    BEGIN");
                file.WriteLine("        SET @sqlcmd = 'ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog DROP COLUMN ' + @column");
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
                            file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                        }
                        else
                        {
                            file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NULL");
                        }
                        file.WriteLine("GO");
                    }
                }
                if (claveAuto == true)
                {
                    file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ALTER COLUMN " + clave + " int NULL");
                    file.WriteLine("GO");
                }
                file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ALTER COLUMN ctct_fec_procesado datetime NULL");
                file.WriteLine("GO");
                file.WriteLine("ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ALTER COLUMN ctct_tipo_operacion varchar(15) NULL");
                file.WriteLine("GO");
                file.WriteLine("");

                //Añadimos PK e Index
                file.WriteLine("--Add PK if not exists");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_CATALOG = 'dbn1_hist_dhyf' AND TABLE_SCHEMA = '" + schema + "' AND TABLE_NAME = '" + cabtab + tab + "_tracelog' AND CONSTRAINT_NAME = 'PK_" + cabtab + tab + "_tracelog' AND CONSTRAINT_TYPE = 'PRIMARY KEY')");
                file.WriteLine("    ALTER TABLE dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog ADD CONSTRAINT PK_" + cabtab + tab + "_tracelog PRIMARY KEY NONCLUSTERED (" + campospk.Replace("t_", "") + ",ctct_fec_procesado)");
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("--Create indexes if not exist");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM dbn1_hist_dhyf." + schema + ".SYSINDEXES WHERE name = 'IX_" + cabtab + tab + "_tracelog_cluster') ");
                file.WriteLine("    CREATE UNIQUE CLUSTERED INDEX IX_" + cabtab + tab + "_tracelog_cluster ON dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog (" + clave + "_tracelog)");
                file.WriteLine("");
                #endregion "Registramos nuevo DS"

                #region "Stored Procedure"
                //SP Creamos SP
                file.WriteLine("");
                file.WriteLine("--//Stored Procedure");
                file.WriteLine("USE dbn1_hist_dhyf");
                file.WriteLine("GO");
                file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '" + schema + "' AND ROUTINE_NAME = 'spn1_cargar_tracelog_" + tipobd + "_" + tab + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                file.WriteLine("    DROP PROCEDURE " + schema + ".spn1_cargar_tracelog_" + tipobd + "_" + tab);
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("CREATE PROCEDURE stg.spn1_cargar_tracelog_" + tipobd + "_" + tab + "(@p_id_carga int) AS");
                file.WriteLine("BEGIN");
                file.WriteLine("");
                
                //SP Cabecera
                string[] cab2 = a.cabeceraLogSP("dbn1_hist_dhyf", schema, "spn1_cargar_tracelog_" + tipobd + "_" + tab, true);

                foreach (string l1 in cab2)
                {
                    file.WriteLine(l1);
                }

                //SP Registro del SP en tabla de control de cargas incrementales y obtención de datos en variables
                string[] sp_inc = a.regSP_Incremental();
                    foreach (string l1 in sp_inc)
                {
                    file.WriteLine(l1);
                }

                //SP Etiquetas            
                file.WriteLine("---Inicio Bloque común para Incremental y Full");
                file.WriteLine("");
                file.WriteLine("            IF @es_carga_completa = 0");
                file.WriteLine("            BEGIN");

                //SP Carga Incremental
                file.WriteLine("--- Inicio Bloque Carga Incremental");
                file.WriteLine("");
                file.WriteLine("            SELECT " + campospk.Replace("t_", "") + ", sys_change_operation");
                file.WriteLine("            INTO #CT_TMP");
                file.WriteLine("            FROM changetable(changes " + bd + ".dbo." + cabtab + tab + ",@ct_" + schema + "_inicial) as CT");
                file.WriteLine("");
                file.WriteLine("            INSERT INTO dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog(" + campos.Replace("'","") + ", ctct_fec_procesado, ctct_tipo_operacion)");
                file.WriteLine("            SELECT");
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (j[4] == "#")
                        {
                            file.WriteLine("                ct." + j[0].ToString() + ", ");
                        }
                        else
                        {
                            file.WriteLine("                s." + j[0].ToString() + ", ");
                        }
                    }
                }
                if (claveAuto == true)
                {
                    file.WriteLine("                s." + clave + ", ");
                }
                file.WriteLine("                @fec_procesado,");
                file.WriteLine("                CASE ct.sys_change_operation");
                file.WriteLine("                    WHEN 'I' then 'INSERT'");
                file.WriteLine("                    WHEN 'U' then 'UPDATE'");
                file.WriteLine("                    WHEN 'D' then 'DELETE'");
                file.WriteLine("                END AS ctct_tipo_operacion");
                file.WriteLine("            FROM #CT_TMP ct");
                file.WriteLine("            LEFT JOIN " + bd + ".dbo." + cabtab + tab + " s");
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#"))
                    {
                        if (j[4] == "#")
                        {
                            if (i == 0)
                            {
                                file.WriteLine("                ON ct." + j[0].ToString() + " =  s." + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("                    AND ct." + j[0].ToString() + " =  s." + j[0].ToString());
                            }
                            i++;
                        }
                    }
                }
                file.WriteLine("");
                file.WriteLine("            SET @rc=@@ROWCOUNT;");
                file.WriteLine("");
                file.WriteLine("        END");
                file.WriteLine("");

                //SP Carga Full
                file.WriteLine("--Inicio Bloque Carga Full");
                file.WriteLine("        ELSE");
                file.WriteLine("        BEGIN");
                file.WriteLine("            IF NOT EXISTS (SELECT 1 FROM (SELECT count(1) AS num_registros FROM dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog) a WHERE num_registros=0)");
                file.WriteLine("            BEGIN");
                file.WriteLine("                -- EXECUTE dbn1_stg_dhyf.dbo.spn1_apuntar_warning @log,'No se puede ejecutar la carga inicial de Trace Log porque la Tabla no está vacía!!'");
                file.WriteLine("                DECLARE @id_warning_1 int");
                file.WriteLine("                EXEC dbn1_norm_dhyf.audit.spn1_insertar_log @p_id_carga= @p_id_carga,@p_bd= @bd,@p_esquema= @esquema,@p_objeto= @objeto,@p_fecha_inicio= @fecha_inicio,@p_descripcion_warning='No se puede ejecutar la carga inicial de Trace Log porque la Tabla no está vacía!!',@p_out_id= @id_warning_1 OUT");
                file.WriteLine("            END");
                file.WriteLine("            ELSE");
                file.WriteLine("            BEGIN");
                file.WriteLine("            INSERT INTO dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog(" + campos.Replace("'", "") + ", ctct_fec_procesado, ctct_tipo_operacion)");
                file.WriteLine("            SELECT");
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        file.WriteLine("                s." + j[0].ToString() + ", ");
                    }
                }
                if (claveAuto == true)
                {
                    file.WriteLine("                s." + clave + ", ");
                }
                file.WriteLine("                @fec_procesado,");
                file.WriteLine("                'INSERT'");
                file.WriteLine("            FROM " + bd + ".dbo." + cabtab + tab + " s");
                file.WriteLine("");
                file.WriteLine("                SET @rc=@@ROWCOUNT;");
                file.WriteLine("            END");
                file.WriteLine("        END");
                file.WriteLine("");

                //SP incluimos los nuevos marcadores a la tabla de ct procesado
                file.WriteLine("---Fin Bloque común para Incremental y Full");
                file.WriteLine("");
                file.WriteLine("        update dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado");
                file.WriteLine("            set ct_stg  = @ct_stg_final,");
                file.WriteLine("            ct_norm = @ct_norm_final,");
                file.WriteLine("            ct_dmr  = @ct_dmr_final");
                file.WriteLine("            where procedimiento = @objeto;");
                file.WriteLine("");

                //SP Pie
                string[] pie = a.pieLogSP("maestro");

                foreach (string l1 in pie)
                {
                    file.WriteLine(l1);
                }

                file.WriteLine("GO");
                file.WriteLine("");
#endregion "Stored Procedure"

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
