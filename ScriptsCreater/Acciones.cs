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
        public string version = "1.0.1";

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

        public DataTable dtCSV(string[] csvData, int lineaCabecera)
        {
            DataTable csvDataTable = new DataTable();
            char separator = ';';
            bool isRowOneHeader = true;
            
            if (csvData.Length > 0)
            {
                String[] headings = csvData[lineaCabecera-1].Split(separator);
                int intRowIndex = 0;

                //Se la primera linea contiene o no las columnas
                if (isRowOneHeader)
                {
                    for (int i = 0; i < headings.Length; i++)
                    {
                        //Se añade el nombre columnas a la tabla
                        csvDataTable.Columns.Add(headings[i].ToString());
                    }

                    intRowIndex++;
                }
                //Si no hay cabecera, 
                //se añade columnas como "Columna1", "Columna2", etc.
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
                    //Crea una nuva linea
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

        public string[] cabeceraLogSP(string bd, string sch, string tab, Boolean incremental)
        {
            int inc_cabSP = 0;
            string[] cabSP = new string[15];

            cabSP[0] = "--Inicio cabecera--";
            cabSP[1] = "    SET NOCOUNT ON";
            cabSP[2] = "";
            cabSP[3] = "    BEGIN TRY";
            cabSP[4] = "";
            cabSP[5] = "        --declaración de variables";
            cabSP[6] = "        DECLARE    @bd varchar(50) = '" + bd + "',";
            cabSP[7] = "                @esquema varchar(50) = '" + sch + "',";
            cabSP[8] = "                @objeto varchar(200) = '" + tab + "',";
            cabSP[9] = "                @fecha_inicio datetime = GETDATE(),";
            cabSP[10] = "                @num_registros int = NULL,";
            cabSP[11] = "                @id int = NULL,";
            cabSP[12] = "                @rc int,";
            cabSP[13] = "                @count_all int,";
            cabSP[14] = "                @count_ins int,";
            if (incremental == true)
            {
                Array.Resize(ref cabSP, cabSP.Length + 10);
                cabSP[15] = "                @idx_reclim int,";
                cabSP[16] = "                @texto nvarchar(1000),";
                cabSP[17] = "                @definicionParametro nvarchar(100),";
                cabSP[18] = "                @ct_stg_inicial bigint,";
                cabSP[19] = "                @ct_norm_inicial bigint,";
                cabSP[20] = "                @ct_dmr_inicial bigint,";
                cabSP[21] = "                @ct_stg_final bigint,";
                cabSP[22] = "                @ct_norm_final bigint,";
                cabSP[23] = "                @ct_dmr_final bigint,";
                cabSP[24] = "                @es_carga_completa bit";
            }
            else
            {
                Array.Resize(ref cabSP, cabSP.Length + 1);
                cabSP[15] = "                @idx_reclim int";
            }
            inc_cabSP = cabSP.Length;
            Array.Resize(ref cabSP, inc_cabSP + 26);
            cabSP[inc_cabSP] = "";
            cabSP[inc_cabSP + 1] = "        --insertar en el log el comienzo de la ejecución de este SP";
            cabSP[inc_cabSP + 2] = "        EXEC dbn1_norm_dhyf.audit.spn1_insertar_log";
            cabSP[inc_cabSP + 3] = "                @p_id_carga = @p_id_carga,";
            cabSP[inc_cabSP + 4] = "                @p_bd = @bd,";
            cabSP[inc_cabSP + 5] = "                @p_esquema = @esquema,";
            cabSP[inc_cabSP + 6] = "                @p_objeto = @objeto,";
            cabSP[inc_cabSP + 7] = "                @p_fecha_inicio = @fecha_inicio,";
            cabSP[inc_cabSP + 8] = "                --@p_descripcion_warning solo informarlo en los SP de warnings, para el resto insertará null";
            cabSP[inc_cabSP + 9] = "                @p_out_id = @id OUT";
            cabSP[inc_cabSP + 10] = "";
            cabSP[inc_cabSP + 11] = "        --ejecutar la lógica de negocio solo si todas las precondiciones están en estado 'OK'";
            cabSP[inc_cabSP + 12] = "        IF dbn1_norm_dhyf.audit.fnn1_precondiciones_ok(@bd,@esquema,@objeto) = 1";
            cabSP[inc_cabSP + 13] = "        BEGIN";
            cabSP[inc_cabSP + 14] = "";
            cabSP[inc_cabSP + 15] = "            --actualizar la tabla de precondiciones para indicar que este SP se está ejecutando y evitar que se lancen SP dependientes de él";
            cabSP[inc_cabSP + 16] = "            EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh";
            cabSP[inc_cabSP + 17] = "                    @p_estado_precondicion = 'EJECUTANDO',";
            cabSP[inc_cabSP + 18] = "                    @p_fecha_inicio_precondicion = @fecha_inicio,";
            cabSP[inc_cabSP + 19] = "                    @p_fecha_fin_precondicion = NULL,";
            cabSP[inc_cabSP + 20] = "                    @p_bd_precondicion = @bd,";
            cabSP[inc_cabSP + 21] = "                    @p_esquema_precondicion = @esquema,";
            cabSP[inc_cabSP + 22] = "                    @p_objeto_precondicion = @objeto";
            cabSP[inc_cabSP + 23] = "";
            cabSP[inc_cabSP + 24] = "--Fin Cabecera--";
            cabSP[inc_cabSP + 25] = "";

            return cabSP;
        }

        public string[] pieLogSP(string tipo)
        {
            string[] pieSP = new string[1];
            int lon =0;

            pieSP[0] = "--Inicio Pie--";
            if (tipo == "maestro")
            {
                //No se genera código
            }
            else
            {
                Array.Resize(ref pieSP, pieSP.Length + 3);
                pieSP[pieSP.Length - 1] = "";
                pieSP[pieSP.Length] = "            --el llamador de un DM no carga registros";
                pieSP[pieSP.Length + 1] = "            SET @rc = 0";
            }

            lon = pieSP.Length;
            Array.Resize(ref pieSP, lon + 69);
            pieSP[lon] = "";
            pieSP[lon + 1] = "            --actualizar la tabla de precondiciones para indicar que la ejecución de este SP ha finalizado 'OK' y que así los SP dependientes de él puedan lanzarse";
            pieSP[lon + 2] = "            DECLARE @fecha_fin_ok_precondicion datetime = GETDATE()";
            pieSP[lon + 3] = "            EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh";
            pieSP[lon + 4] = "                    @p_estado_precondicion = 'OK',";
            pieSP[lon + 5] = "                    @p_fecha_inicio_precondicion = NULL,";
            pieSP[lon + 6] = "                    @p_fecha_fin_precondicion = @fecha_fin_ok_precondicion,";
            pieSP[lon + 7] = "                    @p_bd_precondicion = @bd,";
            pieSP[lon + 8] = "                    @p_esquema_precondicion = @esquema,";
            pieSP[lon + 9] = "                    @p_objeto_precondicion = @objeto";
            pieSP[lon + 10] = "        END";
            pieSP[lon + 11] = "";
            pieSP[lon + 12] = "        ELSE --si no todas las precondiciones están en estado 'OK'";
            pieSP[lon + 13] = "        BEGIN";
            pieSP[lon + 14] = "";
            pieSP[lon + 15] = "            --actualizar la tabla de precondiciones para indicar que la carga de este SP queda pendiente y evitar que se lancen los SP dependientes de él";
            pieSP[lon + 16] = "            DECLARE @fecha_fin_pdte_precondicion datetime = GETDATE()";
            pieSP[lon + 17] = "            EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh";
            pieSP[lon + 18] = "                    @p_estado_precondicion = 'PDTE',";
            pieSP[lon + 19] = "                    @p_fecha_inicio_precondicion = NULL,";
            pieSP[lon + 20] = "                    @p_fecha_fin_precondicion = @fecha_fin_pdte_precondicion,";
            pieSP[lon + 21] = "                    @p_bd_precondicion = @bd,";
            pieSP[lon + 22] = "                    @p_esquema_precondicion = @esquema,";
            pieSP[lon + 23] = "                    @p_objeto_precondicion = @objeto";
            pieSP[lon + 24] = "";
            pieSP[lon + 25] = "            --insertar warning avisando de que este SP no ha cargado datos";
            pieSP[lon + 26] = "            DECLARE @id_warning_precondiones int";
            pieSP[lon + 27] = "            DECLARE @descripcion_warning_precondiciones varchar(4000) = @bd + '.' + @esquema + '.' + @objeto + ' no ha cargado ningún registro porque no se han cumplido todas sus precondiciones'";
            pieSP[lon + 28] = "            EXEC dbn1_norm_dhyf.audit.spn1_insertar_log";
            pieSP[lon + 29] = "                    @p_id_carga = @p_id_carga,";
            pieSP[lon + 30] = "                    @p_bd = @bd,";
            pieSP[lon + 31] = "                    @p_esquema = @esquema,";
            pieSP[lon + 32] = "                    @p_objeto = @objeto,";
            pieSP[lon + 33] = "                    @p_fecha_inicio = @fecha_inicio,";
            pieSP[lon + 34] = "                    @p_descripcion_warning = @descripcion_warning_precondiciones,";
            pieSP[lon + 35] = "                    @p_out_id = @id_warning_precondiones OUT";
            pieSP[lon + 36] = "        END";
            pieSP[lon + 37] = "";
            pieSP[lon + 38] = "        --actualizar el log para indicar que este SP ha finalizado 'OK'";
            pieSP[lon + 39] = "        EXEC dbn1_norm_dhyf.audit.spn1_actualizar_log_correcto";
            pieSP[lon + 40] = "                @p_id = @id,";
            pieSP[lon + 41] = "                @p_num_registros = @rc";
            pieSP[lon + 42] = "";
            pieSP[lon + 43] = "    END TRY";
            pieSP[lon + 44] = "";
            pieSP[lon + 45] = "    BEGIN CATCH";
            pieSP[lon + 46] = "";
            pieSP[lon + 47] = "        --actualizar la tabla de precondiciones para indicar que este SP ha fallado y evitar que se lancen SP dependientes de él";
            pieSP[lon + 48] = "        DECLARE @fecha_fin_error_precondicion datetime = GETDATE()";
            pieSP[lon + 49] = "        EXEC dbn1_norm_dhyf.audit.spn1_actualizar_tbn1_precondiciones_carga_dwh";
            pieSP[lon + 50] = "                @p_estado_precondicion = 'ERROR',";
            pieSP[lon + 51] = "                @p_fecha_inicio_precondicion = NULL,";
            pieSP[lon + 52] = "                @p_fecha_fin_precondicion = @fecha_fin_error_precondicion,";
            pieSP[lon + 53] = "                @p_bd_precondicion = @bd,";
            pieSP[lon + 54] = "                @p_esquema_precondicion = @esquema,";
            pieSP[lon + 55] = "                @p_objeto_precondicion = @objeto";
            pieSP[lon + 56] = "";
            pieSP[lon + 57] = "        --actualizar el log para indicar que este SP ha finalizado con error";
            pieSP[lon + 58] = "        DECLARE @linea_error int,";
            pieSP[lon + 59] = "                @objeto_error varchar(200),";
            pieSP[lon + 60] = "                @descripcion_error varchar(4000)";
            pieSP[lon + 61] = "        SELECT    @linea_error = ERROR_LINE(),";
            pieSP[lon + 62] = "                @objeto_error = ERROR_PROCEDURE(),";
            pieSP[lon + 63] = "                @descripcion_error = SUBSTRING(ERROR_MESSAGE(),1,4000)";
            pieSP[lon + 64] = "        EXEC dbn1_norm_dhyf.audit.spn1_actualizar_log_error";
            pieSP[lon + 65] = "                @p_id = @id,";
            pieSP[lon + 66] = "                @p_linea_error = @linea_error,";
            pieSP[lon + 67] = "                @p_objeto_error = @objeto_error,";
            pieSP[lon + 68] = "                @p_descripcion_error = @descripcion_error";
            if (tipo == "maestro")
            {
                //No se genera código
            }
            else
            {
                Array.Resize(ref pieSP, pieSP.Length + 3);
                pieSP[pieSP.Length - 1] = "";
                pieSP[pieSP.Length] = "        --elevar el error al padre para que vaya directamente al catch";
                pieSP[pieSP.Length + 1] = "        ;THROW";
            }
            lon = pieSP.Length;
            Array.Resize(ref pieSP, lon + 7);
            pieSP[lon] = "";
            pieSP[lon + 1] = "    END CATCH";
            pieSP[lon + 2] = "";
            pieSP[lon + 3] = "--Fin Pie--";
            pieSP[lon + 4] = "";
            pieSP[lon + 5] = "END";
            pieSP[lon + 6] = "";

            return pieSP;
        }

        public DataTable valorQuery(string[] fichero, string[] csv, string tipo, Boolean inc)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("linSript", typeof(String));
            dt.Columns.Add("codScript", typeof(int));
            dt.Columns.Add("orden", typeof(int));

            int i = 0;
            int orden = 0;
            int lon = 0;
            int cods = 0;
            
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
                    }
                    else if (lin == "--- Inicio Bloque común para Incremental y Full" && lon == 0)
                    {
                        lon = 1;
                        cods++;
                    }
                    //Finalizamos almacenamiento de los registros que se van a mantener
                    else if (lin == "--- Business Logic End" && lon == 1)
                    {
                        //Generamos la última linea de Query y cerramos la grabación
                        dt.Rows.Add(lin, cods, orden++);
                        lon = 0;
                    }
                    else if (lin == "--- Fin Bloque común para Incremental y Full" && lon == 1)
                    {
                        //Generamos la última linea de Query y cerramos la grabación
                        dt.Rows.Add(lin, cods, orden++);
                        lon = 0;
                    }

                    if (lon == 1)
                    {
                        dt.Rows.Add(lin, cods, orden++);
                    }
                }
            }

            //Montando registros tipos para Script nuevos
            else
            {
                if (tipo == "maestro")
                {
                    dt.Rows.Add("--- Business Logic Start", 1, orden++);
                    dt.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 1, orden++);
                    dt.Rows.Add("", 1, orden++);
                    dt.Rows.Add("        ;WITH", 1, orden++);
                    dt.Rows.Add("        query AS (", 1, orden++);
                    dt.Rows.Add("SELECT", 1, orden++);
                    //Montamos los campos del Select
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv.Length)
                            {
                                dt.Rows.Add("    CAST(NULL AS " + j[1].ToString() + ") AS " + j[0].ToString(), 1, orden++);
                            }
                            else
                            {
                                dt.Rows.Add("    CAST(NULL AS " + j[1].ToString() + ") AS " + j[0].ToString() + ",", 1, orden++);
                            }
                        }
                    }

                    dt.Rows.Add("--FROM table t", 1, orden++);
                    dt.Rows.Add("WHERE 1 = 0", 1, orden++);
                    dt.Rows.Add("", 1, orden++);
                    dt.Rows.Add("        )", 1, orden++);
                    dt.Rows.Add("--- Business Logic End", 1, orden++);
                }
                else if (tipo == "ds")
                {
                    if (inc == true)
                    {
                        //Bloque Incremental y Full
                        dt.Rows.Add("--- Inicio Bloque común para Incremental y Full", 1, orden++);
                        dt.Rows.Add("", 1, orden++);
                        dt.Rows.Add("--- Fin Bloque común para Incremental y Full", 1, orden++);

                        //Bloque Datos
                        dt.Rows.Add("--- Business Logic Start", 2, orden++);
                        dt.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 2, orden++);
                        dt.Rows.Add("", 2, orden++);
                        dt.Rows.Add("        ;WITH", 2, orden++);
                        dt.Rows.Add("        query AS (", 2, orden++);
                        dt.Rows.Add("            SELECT null", 2, orden++);
                        dt.Rows.Add("       )", 2, orden++);
                        dt.Rows.Add("--- Business Logic End", 2, orden++);
                        //---------------
                        dt.Rows.Add("--- Business Logic Start", 3, orden++);
                        dt.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 3, orden++);
                        dt.Rows.Add("", 3, orden++);
                        dt.Rows.Add("        ;WITH", 3, orden++);
                        dt.Rows.Add("        query AS (", 3, orden++);
                        dt.Rows.Add("            SELECT null", 3, orden++);
                        dt.Rows.Add("       )", 3, orden++);
                        dt.Rows.Add("--- Business Logic End", 3, orden++);
                        //---------------
                        dt.Rows.Add("--- Business Logic Start", 4, orden++);
                        dt.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 4, orden++);
                        dt.Rows.Add("", 4, orden++);
                        dt.Rows.Add("        ;WITH", 4, orden++);
                        dt.Rows.Add("        query AS (", 4, orden++);
                        dt.Rows.Add("            SELECT null", 4, orden++);
                        dt.Rows.Add("       )", 4, orden++);
                        dt.Rows.Add("--- Business Logic End", 4, orden++);
                    }
                    else
                    {
                        //Bloque Datos
                        dt.Rows.Add("--- Business Logic Start", 1, orden++);
                        dt.Rows.Add("--Lo que haya entre 'Business Logic' se respeta si se regenera el Script", 1, orden++);
                        dt.Rows.Add("", 1, orden++);
                        dt.Rows.Add("        ;WITH", 1, orden++);
                        dt.Rows.Add("        query AS (", 1, orden++);
                        dt.Rows.Add("            SELECT null", 1, orden++);
                        dt.Rows.Add("       )", 1, orden++);
                        dt.Rows.Add("--- Business Logic End", 1, orden++);
                    }
                }
                else
                {
                    dt.Rows.Add("--- Business Logic End", 1, orden++);
                    dt.Rows.Add("--Lo que haya entre Query se respeta si se regenera el Script", 1, orden++);
                    dt.Rows.Add("", 1, orden++);
                    dt.Rows.Add("        ;WITH", 1, orden++);
                    dt.Rows.Add("        query AS (", 1, orden++);
                    dt.Rows.Add("            SELECT null", 1, orden++);
                    dt.Rows.Add("        )", 1, orden++);
                    dt.Rows.Add("--- Business Logic End", 1, orden++);
                }
            }

            return dt;
        }

        public string[] changetracking(string tab, string bd, string sch, string act_des)
        {
            string[] ct = new string[7];

            //Desactivar CT
            if (act_des == "des")
            {
                ct[0] = "--Drop CT";
                ct[1] = "IF EXISTS (";
            }
            else
            {
                ct[0] = "--Add CT";
                ct[1] = "IF NOT EXISTS (";
            }            
            ct[2] = "    SELECT 1 FROM " + bd + ".sys.change_tracking_tables tt";
            ct[3] = "    INNER JOIN " + bd + ".sys.objects obj ON obj.object_id = tt.object_id";
            ct[4] = "    WHERE obj.name = '" + tab + "' )";
            if (act_des == "des")
            {
                ct[5] = "ALTER TABLE " + bd + "." + sch + "." + tab + " DISABLE CHANGE_TRACKING";
            }
            else
            {
                ct[5] = "ALTER TABLE " + bd + "." + sch + "." + tab + " ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)";
            }
            ct[6] = "";


            return ct;
        }

        public string[] regSP_Incremental()
        {
            //string[] ct = new string[37];
            string[] ct = new string[30];

            ct[0] = "            --Si el procedimiento no está registrado en la tabla de control de cargas incrementales, lo metemos inicializando la version inicial procesada a 0 en todos los casos";
            ct[1] = "            IF NOT EXISTS (SELECT 1 AS expr1 from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)";
            ct[2] = "            BEGIN";
            ct[3] = "                set @ct_stg_inicial = 0;";
            ct[4] = "                set @ct_norm_inicial = 0;";
            ct[5] = "                set @ct_dmr_inicial = 0;";
            ct[6] = "                INSERT INTO dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado (procedimiento, ct_stg, ct_norm, ct_dmr)";
            ct[7] = "                    values (@objeto,@ct_stg_inicial, @ct_norm_inicial, @ct_dmr_inicial);";
            ct[8] = "            END";
            ct[9] = "            ELSE";
            ct[10] = "            BEGIN";
            ct[11] = "             --Recuperamos la última version de cada BB.DD. procesada por la última ejecución de este procedimiento";
            ct[12] = "                set @ct_stg_inicial = (select ct_stg from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)";
            ct[13] = "                set @ct_norm_inicial = (select ct_norm from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)";
            ct[14] = "                set @ct_dmr_inicial = (select ct_dmr from dbn1_norm_dhyf.audit.tbn1_procedimientos_ct_procesado where procedimiento = @objeto)";
            ct[15] = "            END";
            ct[16] = "";
            ct[17] = "            --Recuperamos las versiones ACTUALES de las 3 instancias de BB.DD. de las que podemos leer";
            ct[18] = "            SET @definicionParametro = '@version_ct bigint OUTPUT'";
            ct[19] = "            SET @texto = 'use dbn1_stg_dhyf ' + CHAR(13) + 'select @version_ct = change_tracking_current_version()';";
            ct[20] = "            EXEC sp_executesql @texto, @definicionParametro, @ct_stg_final OUTPUT";
            ct[21] = "            SET @texto = 'use dbn1_norm_dhyf ' + CHAR(13) + 'select @version_ct = change_tracking_current_version()';";
            ct[22] = "            EXEC sp_executesql @texto, @definicionParametro, @ct_norm_final OUTPUT";
            ct[23] = "            SET @texto = 'use dbn1_dmr_dhyf ' + CHAR(13) + 'select @version_ct = change_tracking_current_version()';";
            ct[24] = "            EXEC sp_executesql @texto, @definicionParametro, @ct_dmr_final OUTPUT";
            ct[25] = "";
            ct[26] = "            set @ct_stg_final = isnull(@ct_stg_final,0)";
            ct[27] = "            set @ct_norm_final = isnull(@ct_norm_final,0)";
            ct[28] = "            set @ct_dmr_final = isnull(@ct_dmr_final,0)";
            ct[29] = "";
            //ct[30] = "            select @es_carga_completa = es_carga_completa";
            //ct[31] = "                from dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro";
            //ct[32] = "                where objeto = @objeto;";
            //ct[33] = "";
            //ct[34] = "            --Esta es la fecha que identifica el momento en que se carga en la BB.DD. de Trace Log los registros grababos por el CT desde la última vez que corrio este Trace Log";
            //ct[35] = "            set @fec_procesado = getdate();";
            //ct[36] = "";

            return ct;
        }
    }
}