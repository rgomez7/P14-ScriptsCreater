using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsCreater
{
    class ScriptComun
    {
        public string cabeceraLogSP(StreamWriter file, string bd, string sch, string tab, Boolean incremental)
        {
            file.WriteLine("--Inicio cabecera--");
            file.WriteLine("    SET NOCOUNT ON");
            file.WriteLine("");
            file.WriteLine("    BEGIN TRY");
            file.WriteLine("");
            file.WriteLine("        --declaración de variables");
            file.WriteLine("        DECLARE    @bd varchar(50) = '" + bd + "',");
            file.WriteLine("                @esquema varchar(50) = '" + sch + "',");
            file.WriteLine("                @objeto varchar(200) = '" + tab + "',");
            file.WriteLine("                @fecha_inicio datetime = GETDATE(),");
            file.WriteLine("                @num_registros int = NULL,");
            file.WriteLine("                @id int = NULL,");
            file.WriteLine("                @rc int,");
            file.WriteLine("                @count_all int,");
            file.WriteLine("                @count_ins int,");
            if (incremental == true)
            {
                file.WriteLine("                @idx_reclim int,");
                file.WriteLine("                @texto nvarchar(1000),");
                file.WriteLine("                @definicionParametro nvarchar(100),");
                file.WriteLine("                @ct_stg_inicial bigint,");
                file.WriteLine("                @ct_norm_inicial bigint,");
                file.WriteLine("                @ct_dmr_inicial bigint,");
                file.WriteLine("                @ct_stg_final bigint,");
                file.WriteLine("                @ct_norm_final bigint,");
                file.WriteLine("                @ct_dmr_final bigint,");
                file.WriteLine("                @es_carga_completa bit");
            }
            else
            {
                file.WriteLine("                @idx_reclim int");
            }
            file.WriteLine("");
            file.WriteLine("        --insertar en el log el comienzo de la ejecución de este SP");
            file.WriteLine("        EXEC dbn1_norm_dhyf.audit.spn1_insertar_log");
            file.WriteLine("                @p_id_carga = @p_id_carga,");
            file.WriteLine("                @p_bd = @bd,");
            file.WriteLine("                @p_esquema = @esquema,");
            file.WriteLine("                @p_objeto = @objeto,");
            file.WriteLine("                @p_fecha_inicio = @fecha_inicio,");
            file.WriteLine("                --@p_descripcion_warning solo informarlo en los SP de warnings, para el resto insertará null");
            file.WriteLine("                @p_out_id = @id OUT");
            file.WriteLine("");
            file.WriteLine("        --ejecutar la lógica de negocio solo si todas las precondiciones están en estado 'OK'");
            file.WriteLine("        IF dbn1_norm_dhyf.audit.fnn1_precondiciones_ok(@bd,@esquema,@objeto) = 1");
            file.WriteLine("        BEGIN");
            file.WriteLine("");
            file.WriteLine("            --actualizar la tabla de precondiciones para indicar que este SP se está ejecutando y evitar que se lancen SP dependientes de él");
            file.WriteLine("            EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh");
            file.WriteLine("                    @p_estado_precondicion = 'EJECUTANDO',");
            file.WriteLine("                    @p_fecha_inicio_precondicion = @fecha_inicio,");
            file.WriteLine("                    @p_fecha_fin_precondicion = NULL,");
            file.WriteLine("                    @p_bd_precondicion = @bd,");
            file.WriteLine("                    @p_esquema_precondicion = @esquema,");
            file.WriteLine("                    @p_objeto_precondicion = @objeto");
            file.WriteLine("");
            file.WriteLine("--Fin Cabecera--");
            file.WriteLine("");

            return "OK";
        }

        public string pieLogSP(StreamWriter file, string tipo)
        {
            file.WriteLine("--Inicio Pie--");
            if (tipo == "dm")
            {
                file.WriteLine("");
                file.WriteLine("            --el llamador de un DM no carga registros");
                file.WriteLine("            SET @rc = 0");
            }
            file.WriteLine("");
            file.WriteLine("            --actualizar la tabla de precondiciones para indicar que la ejecución de este SP ha finalizado 'OK' y que así los SP dependientes de él puedan lanzarse");
            file.WriteLine("            DECLARE @fecha_fin_ok_precondicion datetime = GETDATE()");
            file.WriteLine("            EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh");
            file.WriteLine("                    @p_estado_precondicion = 'OK',");
            file.WriteLine("                    @p_fecha_inicio_precondicion = NULL,");
            file.WriteLine("                    @p_fecha_fin_precondicion = @fecha_fin_ok_precondicion,");
            file.WriteLine("                    @p_bd_precondicion = @bd,");
            file.WriteLine("                    @p_esquema_precondicion = @esquema,");
            file.WriteLine("                    @p_objeto_precondicion = @objeto");
            file.WriteLine("        END");
            file.WriteLine("");
            file.WriteLine("        ELSE --si no todas las precondiciones están en estado 'OK'");
            file.WriteLine("        BEGIN");
            file.WriteLine("");
            file.WriteLine("            --actualizar la tabla de precondiciones para indicar que la carga de este SP queda pendiente y evitar que se lancen los SP dependientes de él");
            file.WriteLine("            DECLARE @fecha_fin_pdte_precondicion datetime = GETDATE()");
            file.WriteLine("            EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh");
            file.WriteLine("                    @p_estado_precondicion = 'PDTE',");
            file.WriteLine("                    @p_fecha_inicio_precondicion = NULL,");
            file.WriteLine("                    @p_fecha_fin_precondicion = @fecha_fin_pdte_precondicion,");
            file.WriteLine("                    @p_bd_precondicion = @bd,");
            file.WriteLine("                    @p_esquema_precondicion = @esquema,");
            file.WriteLine("                    @p_objeto_precondicion = @objeto");
            file.WriteLine("");
            file.WriteLine("            --insertar warning avisando de que este SP no ha cargado datos");
            file.WriteLine("            DECLARE @id_warning_precondiones int");
            file.WriteLine("            DECLARE @descripcion_warning_precondiciones varchar(4000) = @bd + '.' + @esquema + '.' + @objeto + ' no ha cargado ningún registro porque no se han cumplido todas sus precondiciones'");
            file.WriteLine("            EXEC dbn1_norm_dhyf.audit.spn1_insertar_log");
            file.WriteLine("                    @p_id_carga = @p_id_carga,");
            file.WriteLine("                    @p_bd = @bd,");
            file.WriteLine("                    @p_esquema = @esquema,");
            file.WriteLine("                    @p_objeto = @objeto,");
            file.WriteLine("                    @p_fecha_inicio = @fecha_inicio,");
            file.WriteLine("                    @p_descripcion_warning = @descripcion_warning_precondiciones,");
            file.WriteLine("                    @p_out_id = @id_warning_precondiones OUT");
            file.WriteLine("        END");
            file.WriteLine("");
            file.WriteLine("        --actualizar el log para indicar que este SP ha finalizado 'OK'");
            file.WriteLine("        EXEC dbn1_norm_dhyf.audit.spn1_actualizar_log_correcto");
            file.WriteLine("                @p_id = @id,");
            file.WriteLine("                @p_num_registros = @rc");
            file.WriteLine("");
            file.WriteLine("    END TRY");
            file.WriteLine("");
            file.WriteLine("    BEGIN CATCH");
            file.WriteLine("");
            file.WriteLine("        --actualizar la tabla de precondiciones para indicar que este SP ha fallado y evitar que se lancen SP dependientes de él");
            file.WriteLine("        DECLARE @fecha_fin_error_precondicion datetime = GETDATE()");
            file.WriteLine("        EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh");
            file.WriteLine("                @p_estado_precondicion = 'ERROR',");
            file.WriteLine("                @p_fecha_inicio_precondicion = NULL,");
            file.WriteLine("                @p_fecha_fin_precondicion = @fecha_fin_error_precondicion,");
            file.WriteLine("                @p_bd_precondicion = @bd,");
            file.WriteLine("                @p_esquema_precondicion = @esquema,");
            file.WriteLine("                @p_objeto_precondicion = @objeto");
            file.WriteLine("");
            file.WriteLine("        --actualizar el log para indicar que este SP ha finalizado con error");
            file.WriteLine("        DECLARE @linea_error int,");
            file.WriteLine("                @objeto_error varchar(200),");
            file.WriteLine("                @descripcion_error varchar(4000)");
            file.WriteLine("        SELECT    @linea_error = ERROR_LINE(),");
            file.WriteLine("                @objeto_error = ERROR_PROCEDURE(),");
            file.WriteLine("                @descripcion_error = SUBSTRING(ERROR_MESSAGE(),1,4000)");
            file.WriteLine("        EXEC dbn1_norm_dhyf.audit.spn1_actualizar_log_error");
            file.WriteLine("                @p_id = @id,");
            file.WriteLine("                @p_linea_error = @linea_error,");
            file.WriteLine("                @p_objeto_error = @objeto_error,");
            file.WriteLine("                @p_descripcion_error = @descripcion_error");
            if (tipo == "dm")
            {

                file.WriteLine("");
                file.WriteLine("        --elevar el error al padre para que vaya directamente al catch");
                file.WriteLine("        ;THROW");
            }
            file.WriteLine("");
            file.WriteLine("    END CATCH");
            file.WriteLine("");
            file.WriteLine("--Fin Pie--");
            file.WriteLine("");
            file.WriteLine("END");
            file.WriteLine("");

            return "OK";
        }

        public string changetracking(StreamWriter file, string tab, string bd, string sch, string act_des)
        {
            //Desactivar CT
            if (act_des == "des")
            {
                file.WriteLine("--Drop CT");
                file.WriteLine("IF EXISTS (");
            }
            else
            {
                file.WriteLine("--Add CT");
                file.WriteLine("IF NOT EXISTS (");
            }
            file.WriteLine("    SELECT 1 FROM " + bd + ".sys.change_tracking_tables tt");
            file.WriteLine("    INNER JOIN " + bd + ".sys.objects obj ON obj.object_id = tt.object_id");
            file.WriteLine("    WHERE obj.name = '" + tab + "' )");
            if (act_des == "des")
            {
                file.WriteLine("ALTER TABLE " + bd + "." + sch + "." + tab + " DISABLE CHANGE_TRACKING");
            }
            else
            {
                file.WriteLine("ALTER TABLE " + bd + "." + sch + "." + tab + " ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)");
            }
            file.WriteLine("");


            return "OK";
        }

        public string regSP_Incremental(StreamWriter file)
        {
            file.WriteLine("            --Si el procedimiento no está registrado en la tabla de control de cargas incrementales, lo metemos inicializando la version inicial procesada a 0 en todos los casos");
            file.WriteLine("            IF NOT EXISTS (SELECT 1 AS expr1 from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)");
            file.WriteLine("            BEGIN");
            file.WriteLine("                set @ct_stg_inicial = 0;");
            file.WriteLine("                set @ct_norm_inicial = 0;");
            file.WriteLine("                set @ct_dmr_inicial = 0;");
            file.WriteLine("                INSERT INTO dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado (procedimiento, ct_stg, ct_norm, ct_dmr)");
            file.WriteLine("                    values (@objeto,@ct_stg_inicial, @ct_norm_inicial, @ct_dmr_inicial);");
            file.WriteLine("            END");
            file.WriteLine("            ELSE");
            file.WriteLine("            BEGIN");
            file.WriteLine("             --Recuperamos la última version de cada BB.DD. procesada por la última ejecución de este procedimiento");
            file.WriteLine("                set @ct_stg_inicial = (select ct_stg from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)");
            file.WriteLine("                set @ct_norm_inicial = (select ct_norm from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)");
            file.WriteLine("                set @ct_dmr_inicial = (select ct_dmr from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)");
            file.WriteLine("            END");
            file.WriteLine("");
            file.WriteLine("            --Recuperamos las versiones ACTUALES de las 3 instancias de BB.DD. de las que podemos leer");
            file.WriteLine("            SET @definicionParametro = '@version_ct bigint OUTPUT'");
            file.WriteLine("            SET @texto = 'use dbn1_stg_dhyf ' + CHAR(13) + 'select @version_ct = change_tracking_current_version()';");
            file.WriteLine("            EXEC sp_executesql @texto, @definicionParametro, @ct_stg_final OUTPUT");
            file.WriteLine("            SET @texto = 'use dbn1_norm_dhyf ' + CHAR(13) + 'select @version_ct = change_tracking_current_version()';");
            file.WriteLine("            EXEC sp_executesql @texto, @definicionParametro, @ct_norm_final OUTPUT");
            file.WriteLine("            SET @texto = 'use dbn1_dmr_dhyf ' + CHAR(13) + 'select @version_ct = change_tracking_current_version()';");
            file.WriteLine("            EXEC sp_executesql @texto, @definicionParametro, @ct_dmr_final OUTPUT");
            file.WriteLine("");
            file.WriteLine("            set @ct_stg_final = isnull(@ct_stg_final,0)");
            file.WriteLine("            set @ct_norm_final = isnull(@ct_norm_final,0)");
            file.WriteLine("            set @ct_dmr_final = isnull(@ct_dmr_final,0)");
            file.WriteLine("");

            return "OK";
        }

        public string borrarFK(StreamWriter file, string bd, string schema, string tab)
        {
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

            return "OK";
        }

        public string regTablas(StreamWriter file, string bd, string schema, string tab, string clave, string campos, string campospk, string[] csv, Boolean claveAuto, string tiposcript)
        {
            //Formato string[] CSV a pasar (nombreCampo;valorCampo;# [para campos Clave])

            int i = 0;

            file.WriteLine("--Create Table");
            file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "')");
            file.WriteLine("CREATE TABLE " + bd + "." + schema + "." + tab + "(");
            if (tiposcript == "historificacion")
            {
                if (claveAuto == true)
                {
                    file.WriteLine("       " + clave + " int,");
                }
                file.WriteLine("       ctct_fec_procesado datetime,");
                file.WriteLine("       ctct_tipo_operacion varchar(15)");

            }
            else
            {
                if (clave != "")
                {
                    file.WriteLine("    " + clave + " int IDENTITY(1,1),");
                }
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
            file.WriteLine("    WHERE table_schema = '" + schema + "'");
            file.WriteLine("    AND table_name = '" + tab + "'");
            file.WriteLine("    OPEN @cursor");
            file.WriteLine("    FETCH NEXT FROM @cursor INTO @constraint");
            file.WriteLine("    WHILE @@FETCH_STATUS = 0");
            file.WriteLine("    BEGIN");
            file.WriteLine("        SET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + "." + tab + " DROP CONSTRAINT ' + @constraint");
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
            file.WriteLine("    WHERE id = OBJECT_ID('" + tab + "')");
            file.WriteLine("    AND indid > 1 AND indid < 255 ");
            file.WriteLine("    AND INDEXPROPERTY(id, name, 'IsStatistics') = 0");
            file.WriteLine("    ORDER BY indid DESC");
            file.WriteLine("    OPEN @cursor");
            file.WriteLine("    FETCH NEXT FROM @cursor INTO @ncindex");
            file.WriteLine("    WHILE @@FETCH_STATUS = 0");
            file.WriteLine("    BEGIN");
            file.WriteLine("        SET @sqlcmd = 'DROP INDEX ' + @ncindex + ' ON " + bd + "." + schema + "." + tab + "'");
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
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "')");
                    file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString());
                    file.WriteLine("GO");
                }
            }
            if (tiposcript == "historificacion")
            {
                if (claveAuto == true)
                {
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + clave + "')");
                    file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + clave + " int");
                    file.WriteLine("GO");
                }
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='ctct_fec_procesado')");
                file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD ctct_fec_procesado datetime");
                file.WriteLine("GO");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='ctct_tipo_operacion')");
                file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD ctct_tipo_operacion varchar(15)");
                file.WriteLine("GO");
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
            file.WriteLine("    WHERE table_schema = '" + schema + "'");
            file.WriteLine("    AND table_name = '" + tab + "'");
            if (tiposcript == "historificacion")
            {
                if (claveAuto == true)
                {
                    file.WriteLine("    AND column_name NOT IN ('" + clave + "'," + campos + ",'ctct_fec_procesado','ctct_tipo_operacion')");
                }
                else
                {
                    file.WriteLine("    AND column_name NOT IN (" + campos + ",'ctct_fec_procesado','ctct_tipo_operacion')");
                }
            }
            else
            {
                file.WriteLine("    AND column_name NOT IN ('" + clave + "'," + campos + ")");
            }
            file.WriteLine("    OPEN @cursor");
            file.WriteLine("    FETCH NEXT FROM @cursor INTO @column");
            file.WriteLine("    WHILE @@FETCH_STATUS = 0");
            file.WriteLine("    BEGIN");
            file.WriteLine("        SET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + "." + tab + " DROP COLUMN ' + @column");
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
                    if (j[2].ToString() == "#")
                    {
                        file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                    }
                    else
                    {
                        file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NULL");
                    }
                    file.WriteLine("GO");
                }
            }
            if (tiposcript == "historificacion")
            {
                if (claveAuto == true)
                {
                    file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + clave + " int NULL");
                    file.WriteLine("GO");
                }
                file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN ctct_fec_procesado datetime NULL");
                file.WriteLine("GO");
                file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN ctct_tipo_operacion varchar(15) NULL");
                file.WriteLine("GO");
            }
            file.WriteLine("");

            //Añadimos PK siempre que pasemos valor en el parametro "campospk"
            if (campospk != "")
            {
                file.WriteLine("--Add PK if not exists");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_CATALOG = '" + bd + "' AND TABLE_SCHEMA = '" + schema + "' AND TABLE_NAME = '" + tab + "' AND CONSTRAINT_NAME = 'PK_" + tab + "' AND CONSTRAINT_TYPE = 'PRIMARY KEY')");
                if (tiposcript == "historificacion")
                {
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ADD CONSTRAINT PK_" + tab + " PRIMARY KEY NONCLUSTERED (" + campospk.Replace("t_", "") + ",ctct_fec_procesado)");
                }
                else
                {
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ADD CONSTRAINT PK_" + tab + " PRIMARY KEY NONCLUSTERED (" + campospk.Replace("t_", "") + ")");
                }
                file.WriteLine("GO");
                file.WriteLine("");
            }
            //Añadimos Index
            file.WriteLine("--Create indexes if not exist");
            file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + "." + schema + ".SYSINDEXES WHERE name = 'IX_" + tab + "_cluster') ");
            file.WriteLine("    CREATE UNIQUE CLUSTERED INDEX IX_" + tab + "_cluster ON " + bd + "." + schema + "." + tab + " (" + clave + ")");
            file.WriteLine("");
            if (tiposcript == "ds")
            { 
                file.WriteLine("--Add FKs if necessary");
                file.WriteLine("");
            }


            return "OK";
        }

    }
}
