using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsCreator
{
    class ScriptComun
    {
        public string cabeceraLogSP(StreamWriter file, string bd, string sch, string tab, Boolean incremental, bool esHist)
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
            file.WriteLine("                @rc int = 0,");
            file.WriteLine("                @count_all int,");
            file.WriteLine("                @count_ins int,");
            // es historificación
            if (esHist)
            {
                file.WriteLine("                @fec_procesado datetime,");
            }
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
            if (tipo == "dmSP")
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
            file.WriteLine("");
            file.WriteLine("    PRINT 'Warning en ' + @objeto + ': ' + @descripcion_warning_precondiciones");
            file.WriteLine("");
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
            file.WriteLine("    PRINT 'Error en ' + @objeto_error + ': (Linea ' + Convert(nvarchar(10), @linea_error) + ') ' + @descripcion_error");
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
                file.WriteLine("-- CUIDADO!!! Descomentar el DROP CT solo en caso estrictamente necesario (en caso de que se necesite cambiar PK) ");
                file.WriteLine("-- dado que sino se pierden las trazas utilizadas por otros procedimientos incrementales que puedan leer de esta tabla.");
                file.WriteLine("");
                file.WriteLine("--IF EXISTS (");
                file.WriteLine("--    SELECT 1 FROM " + bd + ".sys.change_tracking_tables tt");
                file.WriteLine("--    INNER JOIN " + bd + ".sys.objects obj ON obj.object_id = tt.object_id");
                file.WriteLine("--    WHERE obj.name = '" + tab + "' )");
                file.WriteLine("--ALTER TABLE " + bd + "." + sch + "." + tab + " DISABLE CHANGE_TRACKING");
            }
            else
            {
                file.WriteLine("--Add CT");
                file.WriteLine("IF NOT EXISTS (");
                file.WriteLine("    SELECT 1 FROM " + bd + ".sys.change_tracking_tables tt");
                file.WriteLine("    INNER JOIN " + bd + ".sys.objects obj ON obj.object_id = tt.object_id");
                file.WriteLine("    WHERE obj.name = '" + tab + "' )");
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
            string tipodato = "";
            string coma = "";
            string ValorDato = "";

            file.WriteLine("--Crear la tabla");
            file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "')");
            #region Create Table
            file.WriteLine("CREATE TABLE " + bd + "." + schema + "." + tab);
            file.WriteLine("(");
            if (clave != "")
            {
                file.WriteLine("        " + clave + " int IDENTITY(1,1) NOT NULL,");
            }
            if (tiposcript == "historificacion")
            {
                if (claveAuto == true)
                {
                    file.WriteLine("       " + clave.Replace("_tracelog", "") + " int NOT NULL,");
                }
                file.WriteLine("        ctct_fec_procesado datetime NOT NULL,");
                file.WriteLine("        ctct_tipo_operacion varchar(15) NOT NULL,");
            }
            else if (tiposcript == "maestro")
            {
                file.WriteLine("        origen varchar(10) NOT NULL,");
            }
            else if (tiposcript == "extraccion")
            {
                file.WriteLine("\tSYS_CHANGE_OPERATION char(1) NOT NULL,");
            }
            i = 0;
            coma = ",";
            if (tiposcript == "historificacion")
            {
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            coma = "";
                        }
                        if (j[2].Contains("#"))
                        {
                            file.WriteLine("\t" + j[0].ToString() + " " + j[1].ToString() + " NOT NULL" + coma);
                        }
                        else
                        {
                            file.WriteLine("\t" + j[0].ToString() + " " + j[1].ToString() + coma);
                        }
                    }
                }
            }
            else
            {
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (i == csv.Length)
                        {
                            coma = "";
                        }
                        #region Comprobar tipo campo COMENTADO
                        //if (tiposcript == "maestro")
                        //{
                        //    tipodato = " NULL";
                        //}
                        //else
                        //{
                        //    if (j[2].ToString() == "#")
                        //    {
                        //        tipodato = " NOT NULL";
                        //    }
                        //    else if (j[3].ToString() == "#")
                        //    {
                        //        tipodato = " NOT NULL";
                        //    }
                        //    else
                        //    {
                        //        tipodato = " NULL";
                        //    }
                        //}
                        #endregion Comprobar tipo campo COMENTADO

                        if (tiposcript == "extraccion")
                        {
                            if (campospk.Contains(j[0].ToString()))
                            {
                                tipodato = " NOT NULL";
                            }
                            else
                            {
                                tipodato = "";
                            }
                        }
                        else
                        {
                            tipodato = " NOT NULL";
                        }
                        file.WriteLine("\t" + j[0].ToString() + " " + j[1].ToString() + tipodato + coma);
                    }
                }

            }
            file.WriteLine(")");
            file.WriteLine("WITH (DATA_COMPRESSION=PAGE)");
            file.WriteLine("GO");
            file.WriteLine("");
            #endregion Create Table

            #region Borrado Constraint
            //Borramos Constraint
            file.WriteLine("--Borrar constraints");
            file.WriteLine("DECLARE @constraint nvarchar(128)");
            file.WriteLine("DECLARE @cursor CURSOR");
            file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
            file.WriteLine("BEGIN");
            file.WriteLine("\tSET @cursor = CURSOR FOR");
            file.WriteLine("\t\tSELECT\tconstraint_name");
            file.WriteLine("\t\tFROM\t" + bd + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS");
            file.WriteLine("\t\tWHERE\ttable_schema = '" + schema + "'");
            file.WriteLine("\t\tAND\t\ttable_name = '" + tab + "'");
            if (tiposcript != "extraccion")
            {
                file.WriteLine("\t\t--Comentar la siguiente línea para borrar también la PK");
                file.WriteLine("\t\tAND\tCONSTRAINT_TYPE <> 'PRIMARY KEY'");
            }
            file.WriteLine("\tOPEN @cursor");
            file.WriteLine("\tFETCH NEXT FROM @cursor INTO @constraint");
            file.WriteLine("\tWHILE @@FETCH_STATUS = 0");
            file.WriteLine("\tBEGIN");
            file.WriteLine("\t\tSET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + "." + tab + " DROP CONSTRAINT ' + @constraint");
            file.WriteLine("\t\tEXEC (@sqlcmd)");
            file.WriteLine("\t\tFETCH NEXT FROM @cursor INTO @constraint");
            file.WriteLine("\tEND");
            file.WriteLine("END");
            file.WriteLine("CLOSE @cursor");
            file.WriteLine("DEALLOCATE @cursor");
            file.WriteLine("GO");
            file.WriteLine("");
            #endregion Borrado Constraint

            #region Borrado Indices
            //Borramos Indices
            if (tiposcript != "extraccion")
            {
                file.WriteLine("--Drop all non-clustered index");
                file.WriteLine("USE " + bd);
                file.WriteLine("GO");
                file.WriteLine("");
                file.WriteLine("DECLARE @pk varchar(200) = ''");
                file.WriteLine("--Comentar la siguiente select de variable para borrar todos los non-clustered index, sino solo se borran aquellos que no sean Primary Key--");
                file.WriteLine("SELECT @pk = constraint_name FROM " + bd + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS");
                file.WriteLine("    WHERE table_schema = '" + schema + "' AND table_name = '" + tab + "' AND CONSTRAINT_TYPE = 'PRIMARY KEY'");
                file.WriteLine("");
                file.WriteLine("DECLARE @ncindex nvarchar(128)");
                file.WriteLine("DECLARE @cursor CURSOR");
                file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
                file.WriteLine("BEGIN");
                file.WriteLine("    SET @cursor = CURSOR FOR");
                file.WriteLine("    SELECT name FROM " + bd + ".dbo.SYSINDEXES");
                file.WriteLine("    WHERE id = OBJECT_ID('" + tab + "')");
                file.WriteLine("        AND indid > 1 AND indid < 255 ");
                file.WriteLine("        AND INDEXPROPERTY(id, name, 'IsStatistics') = 0");
                file.WriteLine("        AND name != @pk");
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
            }
            #endregion Borrado Indices

            #region Quitar Valores NULL

            //file.WriteLine("--Quitamos valores nulos");
            //foreach (string d in csv)
            //{
            //    string[] j = d.Split(new Char[] { ';' });
            //    i++;
            //    if (!j[0].Contains("#"))
            //    {
            //        if (j[1].ToString().Contains("decimal") || j[1].ToString().Contains("numeric"))
            //        {
            //            ValorDato = "0";
            //        }
            //        else
            //        {
            //            ValorDato = "''";
            //        }

            //        if (tiposcript == "maestro")
            //        {
            //            file.WriteLine("IF EXISTS(SELECT 1 FROM " + bd + "." + schema + "." + tab + " WHERE " + j[0].ToString() + " is null)");
            //            file.WriteLine("    UPDATE " + bd + "." + schema + "." + tab + " SET " + j[0].ToString() + " = " + ValorDato  + " WHERE " + j[0].ToString() + " is null");
            //            file.WriteLine("GO");
            //        }
            //        else if (j[2].ToString() == "#" || j[3].ToString() == "#")
            //        {
            //            //No se hace nada
            //        }
            //        else
            //        {
            //            file.WriteLine("IF EXISTS(SELECT 1 FROM " + bd + "." + schema + "." + tab + " WHERE " + j[0].ToString() + " is null)");
            //            file.WriteLine("    UPDATE " + bd + "." + schema + "." + tab + " SET " + j[0].ToString() + " = " + ValorDato + " WHERE " + j[0].ToString() + " is null");
            //            file.WriteLine("GO");
            //        }
            //    }
            //}
            //if (tiposcript == "historificacion")
            //{
            //    if (claveAuto == true)
            //    {
            //        file.WriteLine("IF EXISTS(SELECT 1 FROM " + bd + "." + schema + "." + tab + " WHERE " + clave.Replace("_tracelog", "") + " is null)");
            //        file.WriteLine("    UPDATE " + bd + "." + schema + "." + tab + " SET " + clave.Replace("_tracelog", "") + " = '' WHERE " + clave.Replace("_tracelog", "") + " is null");
            //        file.WriteLine("GO");
            //    }
            //    file.WriteLine("IF EXISTS(SELECT 1 FROM " + bd + "." + schema + "." + tab + " WHERE ctct_fec_procesado is null)");
            //    file.WriteLine("    UPDATE " + bd + "." + schema + "." + tab + " SET ctct_fec_procesado = '' WHERE ctct_fec_procesado is null");
            //    file.WriteLine("GO");
            //    file.WriteLine("IF EXISTS(SELECT 1 FROM " + bd + "." + schema + "." + tab + " WHERE ctct_tipo_operacion is null)");
            //    file.WriteLine("    UPDATE " + bd + "." + schema + "." + tab + " SET ctct_tipo_operacion = '' WHERE ctct_tipo_operacion is null");
            //    file.WriteLine("GO");
            //}
            //file.WriteLine("");

            #endregion Quitar Valores NULL

            #region Añadimos Columnas si no existen
            //Añadimos columnas si no existen
            file.WriteLine("--Añadir columnas nuevas");
            if (tiposcript == "extraccion")
            {
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='SYS_CHANGE_OPERATION')");
                file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD SYS_CHANGE_OPERATION char(1) NOT NULL" + ValorDato);
                file.WriteLine("GO");
            }
            foreach (string d in csv)
            {
                string[] j = d.Split(new Char[] { ';' });
                i++;
                if (!j[0].Contains("#"))
                {
                    //if (j[1].ToString().ToLower().Contains("decimal") || j[1].ToString().ToLower().Contains("numeric"))
                    //{
                    //    ValorDato = " DEFAULT(0)";
                    //}
                    //else
                    //{
                    //    ValorDato = " DEFAULT('')";
                    //}
                    ValorDato = "";
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "')");
                    if (tiposcript == "maestro")
                    {
                        file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL" + ValorDato);
                    }
                    else if (j[2].ToString() == "#" || j[3].ToString() == "#")
                    {
                        file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL" + ValorDato);
                    }
                    else if (tiposcript == "historificacion" )
                    {
                        file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString() + " " + ValorDato);
                    }
                    else if (tiposcript == "extraccion")
                    {
                        if (campospk.Contains(j[0].ToString()))
                        {
                            file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL" + ValorDato);
                        }
                        else
                        {
                            file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString() + " " + ValorDato);
                        }
                    }
                    else
                    {
                        file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL" + ValorDato);
                    }
                    file.WriteLine("GO");
                }
            }
            if (tiposcript == "historificacion")
            {
                //ValorDato = " DEFAULT('')";
                ValorDato = "";
                if (claveAuto == true)
                {
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + clave.Replace("_tracelog", "") + "')");
                    file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD " + clave.Replace("_tracelog", "") + " int " + ValorDato);
                    file.WriteLine("GO");
                }
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='ctct_fec_procesado')");
                file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD ctct_fec_procesado datetime NOT NULL" + ValorDato);
                file.WriteLine("GO");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='ctct_tipo_operacion')");
                file.WriteLine("ALTER TABLE " + bd + "." + schema + "." + tab + " ADD ctct_tipo_operacion varchar(15) NOT NULL" + ValorDato);
                file.WriteLine("GO");
            }
            file.WriteLine("");
            #endregion Añadimos Columnas si no existen

            #region Borrado Columnas si no existen CSV
            //Borramos columnas que no existan el CSV
            file.WriteLine("--Borrar columnas obsoletas");
            file.WriteLine("DECLARE @column nvarchar(128)");
            file.WriteLine("DECLARE @cursor CURSOR");
            file.WriteLine("DECLARE @sqlcmd nvarchar(max)");
            file.WriteLine("BEGIN");
            file.WriteLine("\tSET @cursor = CURSOR FOR");
            file.WriteLine("\t\tSELECT\tcolumn_name");
            file.WriteLine("\t\tFROM\t" + bd + ".INFORMATION_SCHEMA.COLUMNS");
            file.WriteLine("\t\tWHERE\ttable_schema = '" + schema + "'");
            file.WriteLine("\t\tAND\t\ttable_name = '" + tab + "'");
            if (tiposcript == "historificacion")
            {
                file.WriteLine("\t\tAND\t\tcolumn_name NOT IN ('" + clave + "'," + campos + ",'ctct_fec_procesado','ctct_tipo_operacion')");
            }
            else if (tiposcript == "maestro")
            {
                file.WriteLine("\t\tAND\t\tcolumn_name NOT IN ('" + clave + "'," + campos + ",'origen')");
            }
            else if (tiposcript == "extraccion")
            {
                file.WriteLine("\t\tAND\t\tcolumn_name NOT IN\t(");
                file.WriteLine("\t\t\t\t\t\t\t\t\t'id_" + tab + "',");
                file.WriteLine("\t\t\t\t\t\t\t\t\t" + campos);
                file.WriteLine("\t\t\t\t\t\t\t\t\t'SYS_CHANGE_OPERATION'");
                file.WriteLine("\t\t\t\t\t\t\t\t\t)");
            }
            else
            {
                file.WriteLine("\t\tAND\t\tcolumn_name NOT IN ('" + clave + "'," + campos + ")");
            }
            file.WriteLine("\tOPEN @cursor");
            file.WriteLine("\tFETCH NEXT FROM @cursor INTO @column");
            file.WriteLine("\tWHILE @@FETCH_STATUS = 0");
            file.WriteLine("\tBEGIN");
            file.WriteLine("\t\tSET @sqlcmd = 'ALTER TABLE " + bd + "." + schema + "." + tab + " DROP COLUMN ' + @column");
            file.WriteLine("\t\tEXEC(@sqlcmd)");
            file.WriteLine("\t\tFETCH NEXT FROM @cursor INTO @column");
            file.WriteLine("\tEND");
            file.WriteLine("END");
            file.WriteLine("CLOSE @cursor");
            file.WriteLine("DEALLOCATE @cursor");
            file.WriteLine("GO");
            file.WriteLine("");
            #endregion Borrado Columnas si no existen CSV

            #region Adjuntamos Tipos Campos
            //Ajustamos tipos de Campos
            if (tiposcript != "maestro")
            {
                file.WriteLine("--Ajustar los tipos de columna (OJO: no modifica la longitud/precisión)");
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    i++;
                    if (!j[0].Contains("#"))
                    {
                        if (tiposcript == "historificacion")
                        {
                            //obtener el tipo de dato de la columna (dejando solo el nombre del tipo de dato, sin la longitud/precisión)
                            string tipoDato = "";
                            if (j[1].ToString().IndexOf("(") > 0)
                            {
                                //tipoDato = j[1].ToString().Remove(j[1].ToString().IndexOf("("));
                                tipoDato = j[1].ToString().Substring(0, j[1].ToString().IndexOf("(")).Trim();
                            }
                            else
                            {
                                tipoDato = j[1].ToString().Trim();
                            }
                            if (j[2].Contains("#"))
                            {
                                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "' AND DATA_TYPE='" + tipoDato + "' AND IS_NULLABLE='NO')");
                                file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                                file.WriteLine("GO");
                            }
                            else
                            {
                                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "' AND DATA_TYPE='" + tipoDato + "' AND IS_NULLABLE='NO')");
                                file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString());
                                file.WriteLine("GO");
                            }
                        }
                        else if (tiposcript == "extraccion")
                        {
                            //obtener el tipo de dato de la columna (dejando solo el nombre del tipo de dato, sin la longitud/precisión)
                            string tipoDato = "";
                            if (j[1].ToString().IndexOf("(") > 0)
                            {
                                //tipoDato = j[1].ToString().Remove(j[1].ToString().IndexOf("("));
                                tipoDato = j[1].ToString().Substring(0, j[1].ToString().IndexOf("(")).Trim();
                            }
                            else
                            {
                                tipoDato = j[1].ToString().Trim();
                            }
                            if (campospk.Contains(j[0].ToString()))
                            {
                                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "' AND DATA_TYPE='" + tipoDato + "' AND IS_NULLABLE='NO')");
                                file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                                file.WriteLine("GO");
                            }
                            else
                            {
                                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "' AND DATA_TYPE='" + tipoDato + "' AND IS_NULLABLE='YES')");
                                file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString());
                                file.WriteLine("GO");
                            }
                        }
                        else
                        {
                            file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + j[0].ToString() + "' AND IS_NULLABLE='NO')");
                            file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                            file.WriteLine("GO");
                        }
                    }
                }
                if (tiposcript == "historificacion")
                {
                    if (claveAuto == true)
                    {
                        file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='" + clave.Replace("_tracelog", "") + "' AND IS_NULLABLE='NO')");
                        file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN " + clave.Replace("_tracelog", "") + " int ");
                        file.WriteLine("GO");
                    }
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='ctct_fec_procesado' AND IS_NULLABLE='NO')");
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN ctct_fec_procesado datetime NOT NULL");
                    file.WriteLine("GO");
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='ctct_tipo_operacion' AND IS_NULLABLE='NO')");
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN ctct_tipo_operacion varchar(15) NOT NULL");
                    file.WriteLine("GO");
                }
                else if (tiposcript == "extraccion")
                {
                    file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='" + schema + "' AND TABLE_NAME='" + tab + "' AND COLUMN_NAME='SYS_CHANGE_OPERATION' AND DATA_TYPE='CHAR' AND CHARACTER_MAXIMUM_LENGTH=1 AND IS_NULLABLE='NO')");
                    file.WriteLine("\tALTER TABLE " + bd + "." + schema + "." + tab + " ALTER COLUMN SYS_CHANGE_OPERATION char(1) NOT NULL");
                    file.WriteLine("GO");
                }
                file.WriteLine("");
            }
            #endregion Adjuntamos Tipos Campos

            #region Añadimos PK
            //Añadimos PK siempre que pasemos valor en el parametro "campospk"
            if (campospk != "")
            {
                file.WriteLine("--Crear PK si no existe");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_CATALOG = '" + bd + "' AND TABLE_SCHEMA = '" + schema + "' AND TABLE_NAME = '" + tab + "' AND CONSTRAINT_NAME = 'PK_" + tab + "' AND CONSTRAINT_TYPE = 'PRIMARY KEY')");
                if (tiposcript == "historificacion")
                {
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ADD CONSTRAINT PK_" + tab + " PRIMARY KEY NONCLUSTERED (" + campospk.Replace("xxx_", "") + ",ctct_fec_procesado)");
                }
                else if (tiposcript == "dm")
                {
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ADD CONSTRAINT PK_" + tab + " PRIMARY KEY CLUSTERED (" + clave + ")");
                }
                else if (tiposcript == "extraccion")
                {
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ADD CONSTRAINT PK_" + tab + " PRIMARY KEY CLUSTERED (" + campospk.Replace("xxx_", "t_") + ")");
                }
                else
                {
                    file.WriteLine("    ALTER TABLE " + bd + "." + schema + "." + tab + " ADD CONSTRAINT PK_" + tab + " PRIMARY KEY NONCLUSTERED (" + campospk.Replace("xxx_", "") + ")");
                }
                file.WriteLine("GO");
                file.WriteLine("");
            }
            #endregion Añadimos PK

            #region Añadimos Index
            //Añadimos Index
            if (tiposcript == "dm")
            {
                file.WriteLine("--Create indexes if not exist");
                i = 0;
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (!j[0].Contains("#") && j[2].Length > 0)
                    {
                        i++;
                        file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_" + tab + "_" + i + "') ");
                        file.WriteLine("    CREATE NONCLUSTERED INDEX IX_" + tab + "_" + i + " ON " + bd + "." + schema + "." + tab + "(" + j[0].ToString() + ")");
                        file.WriteLine("");
                    }
                }
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_" + tab + "_unique') ");
                file.WriteLine("    CREATE UNIQUE NONCLUSTERED INDEX IX_" + tab + "_unique ON " + bd + "." + schema + "." + tab + "(" + campospk.Replace("'", "").Replace("xxx_", "t_") + ") INCLUDE (" + clave + ")");
                file.WriteLine("");
            }
            else if (tiposcript == "extraccion")
            { 
                //No se genera index
            }
            else
            {
                file.WriteLine("--Create indexes if not exist");
                file.WriteLine("IF NOT EXISTS (SELECT 1 FROM " + bd + ".sys.INDEXES WHERE name = 'IX_" + tab + "_cluster') ");
                file.WriteLine("    CREATE UNIQUE CLUSTERED INDEX IX_" + tab + "_cluster ON " + bd + "." + schema + "." + tab + " (" + clave + ")");
            }
            file.WriteLine("");
            if (tiposcript == "ds")
            { 
                file.WriteLine("--Add FKs if necessary");
                file.WriteLine("");
            }
            #endregion Añadimos Index

            return "OK";
        }

        public string generar_file_exec(StreamWriter file_exec, string tabla, string sp_bd, string sp_sch, string sp, Boolean incremental, Boolean precondicion)
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
            if (precondicion == true)
            {
                file_exec.WriteLine("-------------------------------------------------------------------");
                file_exec.WriteLine("--Ver estado Precondiciones");
                file_exec.WriteLine("SELECT estado_precondicion FROM dbn1_norm_dhyf.audit.tbn1_precondiciones_carga_dwh WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "'");
                file_exec.WriteLine("");
            }
            if (tabla.Contains("_tracelog"))
            {
                file_exec.WriteLine("-------------------------------------------------------------------");
                file_exec.WriteLine("--Borrar table historificación");
                file_exec.WriteLine("--DROP TABLE " + tabla);
            }
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
            if (sp.Contains("dm"))
            {
                file_exec.WriteLine("SELECT TOP 20 * FROM dbn1_norm_dhyf.audit.tbn1_logs_carga_dwh WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND (objeto = '" + sp + "' or objeto like '" + sp.Replace("_dm_", "_") + "_%') order by id desc");
            }
            else
            {
                file_exec.WriteLine("SELECT TOP 1 * FROM dbn1_norm_dhyf.audit.tbn1_logs_carga_dwh WHERE bd = '" + sp_bd + "' AND esquema = '" + sp_sch + "' AND objeto = '" + sp + "' order by id desc");
            }
            file_exec.WriteLine("");
            file_exec.WriteLine("SELECT TOP 10000 * FROM " + tabla);
            file_exec.WriteLine("GO");
            file_exec.WriteLine("");

            return "OK";
        }

    }
}
