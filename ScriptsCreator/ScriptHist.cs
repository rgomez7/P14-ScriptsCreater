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
    class ScriptHist
    {
        Acciones a = new Acciones();
        Consultas c = new Consultas();
        ScriptComun sc = new ScriptComun();

        public string hist(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean claveAuto, Boolean CreateTable, Boolean ChangeTrack)
        {
            string nombrearchivoexec = "";
            int i = 0;
            string bbdd = "";
            string tab = "";
            string clave = "";
            string campospk = "";
            bool existe_pk = false;
            string campos = "";
            string tipobbdd = "";
            string schema = "";
            string schema_sp = "stg";
            string cabtab = "";
            string[] lineas = new string[0];
            string dev = "";
            string[] csv2 = new string[0];

            try
            {
                foreach (string d in csv)
                {
                    string[] j = d.Split(new Char[] { ';' });
                    if (j[0].Contains("#nombre"))
                    {
                        bbdd = j[2];
                        tab = j[1];
                        clave = j[3];
                    }
                    else if (!j[0].Contains("#"))
                    {
                        campos = campos + "'" + j[0] + "',";
                        if (j[4] == "#")
                        {
                            campospk = campospk + "xxx_" + j[0] + ",";
                        }
                        Array.Resize(ref csv2, csv2.Length + 1);
                        csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[4].ToString() + ";";
                    }
                }
                campos = campos.Substring(0, campos.Length - 1);

                existe_pk = (campospk.Length > 0);
                if (existe_pk)
                {
                    campospk = campospk.Substring(0, campospk.Length - 1);
                }

            }
            catch //(Exception ex2)
            {
                MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error PK en archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }
            //Si no tenemos valor clave, lo generamos
            clave = "id_" + tab;
            if (claveAuto == true)
            {
                campos = campos + ",'" + clave + "'";
            }

            //Asignamos nombre al nombrearchivo
            if (bbdd.Contains("dmr") || tab.Contains("_dm_"))
            {
                tipobbdd = "dimensional";
                schema = "dmr";
                if (bbdd == "")
                {
                    bbdd = "dbn1_dmr_dhyf";
                }
            }
            else if (bbdd.Contains("stg"))
            {
                tipobbdd = "staging";
                schema = "stg";
            }
            else if (bbdd.Contains("norm") || tab.Contains("_ds_"))
            {
                tipobbdd = "normalizado";
                schema = "norm";
                if (bbdd == "")
                {
                    bbdd = "dbn1_norm_dhyf";
                }
            }
            else
            {
                tipobbdd = "";
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

            //cgs quito tbn1_ porque sobra:    nombrearchivo = "Script TL " + tipobbdd + "_tbn1_" + cabtab + tab + "_tracelog_TL.sql";
            nombrearchivo = "Script TL " + tipobbdd + "_" + cabtab + tab + "_tracelog_TL.sql";
            nombrearchivoexec = "Exec TL " + tipobbdd + "_" + cabtab + tab + "_tracelog_TL.sql";
            //nombrearchivo = nombrearchivo.Replace("xxx", tipobbdd);
            string fichero = ruta + nombrearchivo;
            dev = a.comprobarficheros(ref lineas, fichero, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                //Escribimos en el fichero
                try
                {

                    StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                    //StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                    //file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                    //file_exec.WriteLine("GO");
                    //sc.generar_file_exec(file_exec, "dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog", "dbn1_hist_dhyf", schema, "spn1_cargar_tracelog_" + tipobbdd + "_" + tab, false, true);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                    file.WriteLine("GO");
                    file.WriteLine("");

                    //Activamos CT
                    if (ChangeTrack == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("/*--------------------------------------");
                    }
                    if (existe_pk)
                    {
                        file.WriteLine("--Begin Add Change Tracking -> " + cabtab + tab);
                        string ctp = sc.changetracking(file, cabtab + tab, bbdd, "dbo", "act");
                        file.WriteLine("--------------------------------------*/");
                    }
                    if (ChangeTrack == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("--------------------------------------*/");
                    }
                    file.WriteLine("");

                    //Create Table
                    if (CreateTable == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("/*--------------------------------------");
                    }
                    file.WriteLine("--Begin table create/prepare -> " + cabtab + tab + "_tracelog");
                    file.WriteLine("");

                    string tab_sin_prefijo;
                    if (tab.Contains("tbn1_"))
                    {
                        tab_sin_prefijo = tab.Replace("tbn1_", "");
                    }
                    else if (tab.Contains("tbn1"))
                    {
                        tab_sin_prefijo = tab.Replace("tbn1", "");
                    }
                    else
                    {
                        tab_sin_prefijo = tab;
                    }
                    sc.regTablas(file, "dbn1_hist_dhyf", schema, cabtab + tab + "_tracelog", clave + "_tracelog", campos, campospk, csv2, claveAuto, "historificacion", tab_sin_prefijo);

                    file.WriteLine("--End table create/prepare -> " + cabtab + tab + "_tracelog");
                    if (CreateTable == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("--------------------------------------*/");
                    }
                    file.WriteLine("GO");

                    #region "Stored Procedure"
                    if (existe_pk)
                    {
                        //SP Creamos SP
                        file.WriteLine("");
                        file.WriteLine("--//Stored Procedure");
                        file.WriteLine("USE dbn1_hist_dhyf");
                        file.WriteLine("GO");
                        file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '" + schema_sp + "' AND ROUTINE_NAME = 'spn1_cargar_tracelog_" + tipobbdd + "_" + tab + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                        file.WriteLine("    DROP PROCEDURE " + schema_sp + ".spn1_cargar_tracelog_" + tipobbdd + "_" + tab);
                        file.WriteLine("GO");
                        file.WriteLine("");
                        file.WriteLine("CREATE PROCEDURE " + schema_sp + ".spn1_cargar_tracelog_" + tipobbdd + "_" + tab + "(@p_id_carga int) AS");
                        file.WriteLine("BEGIN");
                        file.WriteLine("");

                        //SP Cabecera
                        string cab2 = sc.cabeceraLogSP(file, "dbn1_hist_dhyf", schema_sp, "spn1_cargar_tracelog_" + tipobbdd + "_" + tab, true, true);

                        //SP Registro del SP en tabla de control de cargas incrementales y obtención de datos en variables
                        string sp_inc = sc.regSP_Incremental(file);
                        ////////
                        file.WriteLine("            SELECT @es_carga_completa = es_carga_completa");
                        file.WriteLine("            FROM dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro");
                        file.WriteLine("            WHERE objeto = @objeto;");
                        file.WriteLine("            --Esta es la fecha que identifica el momento el que se carga en la BB.DD.de Trace Log los registros grababos por el CT desde la Ãºltima vez que corrio este Trace Log");
                        file.WriteLine("           SET @fec_procesado = getdate();");
                        file.WriteLine("");
                        //////
                        //SP Etiquetas            
                        file.WriteLine("---Inicio Bloque común para Incremental y Full");
                        file.WriteLine("");
                        file.WriteLine("            IF @es_carga_completa = 0");
                        file.WriteLine("            BEGIN");

                        //SP Carga Incremental
                        file.WriteLine("--- Inicio Bloque Carga Incremental");
                        file.WriteLine("");
                        file.WriteLine("            SELECT " + campospk.Replace("xxx_", "") + ", sys_change_operation");
                        file.WriteLine("            INTO #CT_TMP");
                        file.WriteLine("            FROM changetable(changes " + bbdd + ".dbo." + cabtab + tab + ",@ct_" + schema + "_inicial) as CT");
                        file.WriteLine("");
                        file.WriteLine("            INSERT INTO dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog(" + campos.Replace("'", "") + ", ctct_fec_procesado, ctct_tipo_operacion)");
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
                        file.WriteLine("            LEFT JOIN " + bbdd + ".dbo." + cabtab + tab + " s");
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
                        file.WriteLine("                EXEC dbn1_norm_dhyf.audit.spn1_insertar_log @p_id_carga= @p_id_carga,@p_bbdd= @bd,@p_esquema= @esquema,@p_objeto= @objeto,@p_fecha_inicio= @fecha_inicio,@p_descripcion_warning='No se puede ejecutar la carga inicial de Trace Log porque la Tabla no está vacía!!',@p_out_id= @id_warning_1 OUT");
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
                        file.WriteLine("            FROM " + bbdd + ".dbo." + cabtab + tab + " s");
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
                        string pie = sc.pieLogSP(file, "historificacion");

                        file.WriteLine("GO");
                        file.WriteLine("");
                    }
                    else
                    {
                        file.WriteLine("--No hay PK en origen, no genero SP");
                    }
                    #endregion "Stored Procedure"

                    file.Close();
                    //file_exec.Close();
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

        public string hist_tabla(string tab, string bbdd, ref string nombrearchivo, ref DataTable dtSP, string ruta)
        {
            //string nombrearchivoexec = "";
            string tipobbdd = "";
            string schema = "";
            string schema_sp = "stg";
            string dev = "";
            string campos = "";
            string campospk = "";
            bool existe_pk = false;
            string error = "";
            string clave = "";
            string valorcampo = "";
            string valorClave = "";
            string nombreSP = "";
            string nombreScript = "";
            string[] lineas = new string[0];
            string[] csv = new string[0];
            string valorReplace = "|";
            int i = 0;
            DataRow newSP = dtSP.NewRow();
           
            DataTable dtColumns = a.conexion(a.cadenad(bbdd), c.Columns(tab));
            DataTable dtClaves = a.conexion(a.cadenad(bbdd), c.ColumnsClaves("dbo." + tab));
            DataTable dtCT = a.conexion(a.cadenad(bbdd), c.ChangeTrackingActivo(tab));
            DataTable dtTLActivado = a.conexion(a.cadenad("dbn1_hist_dhyf"), c.ComprobarTL(tab));

            //Agregamos los campos PK a
            foreach (DataRow dr in dtClaves.Rows)
            {
                if (Convert.ToBoolean(dr.ItemArray[4]) == true)
                {
                    campospk = campospk + dr.ItemArray[9].ToString() + ", ";
                }
            }

            existe_pk = (campospk.Length > 0);

            if (existe_pk)
            {
                campospk = campospk.Substring(0, campospk.Length - 2);
            }


            //Agregamos campos a variable
            foreach (DataRow dr in dtColumns.Rows)
            {
                campos = campos + "'" + dr.ItemArray[0].ToString() + "', ";

                //Valor del Campo
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
                    valorcampo = " " + dr.ItemArray[2].ToString().ToLower() + "(" + dr.ItemArray[4].ToString() + ", " + dr.ItemArray[5].ToString() + ")";
                }
                else
                {
                    valorcampo = " " + dr.ItemArray[2].ToString().ToLower() ;
                }

                //Comprobamos si el valor PK coincide con el valor del campo
                valorClave = "";
                if (campospk.Contains(",") == true)
                {
                    foreach (string valorpk in campospk.Split(','))
                    {
                        if (valorpk.ToLower().Trim() == dr.ItemArray[0].ToString().ToLower())
                        {
                            valorClave = "#";
                        }
                    }
                }
                else
                {
                    if (campospk.ToLower().Trim() == dr.ItemArray[0].ToString().ToLower())
                    {
                        valorClave = "#";
                    }
                }

                //Montamos CSV
                Array.Resize(ref csv, csv.Length + 1);
                csv[csv.Length - 1] = dr.ItemArray[0].ToString() + ";" + valorcampo + ";" + valorClave + ";";
            }
            campos = campos.Substring(0, campos.Length - 2);

            //Componemos la clave de TL
            if (tab.Contains("tbn1_"))
            {
                clave = "id_" + tab.Replace("tbn1_", "");
            }
            else if (tab.Contains("tbn1"))
            {
                clave = "id_" + tab.Replace("tbn1", "");
            }
            else
            {
                clave = "";
            }

            //Asignamos nombre al nombrearchivo
            if (bbdd.Contains("dmr"))
            {
                tipobbdd = "dimensional";
                schema = "dmr";
            }
            else if (bbdd.Contains("stg"))
            {
                tipobbdd = "staging";
                schema = "stg";
            }
            else if (bbdd.Contains("norm"))
            {
                tipobbdd = "normalizado";
                schema = "norm";
            }
            else
            {
                tipobbdd = "";
                schema = "";
            }

            if (tab.Contains("mae_"))
            {
                tipobbdd = "maestro";
            }

            //Incluimos el tbn1 para reemplazarlo al generar el SP
            if (tab.Substring(4, 1) == "_")
            {
                valorReplace = tab.Substring(0, 5);
            }
            else
            {
                valorReplace = tab.Substring(0, 4);
            }

            //Comprobamos si existe SP
            nombreSP = tipobbdd + "_" + tab.Replace(valorReplace, "").Replace("mae_", "");
            nombreScript = nombreSP;
            string valorSP = "spn1_cargar_" + nombreSP;

            DataTable dtComprobarSP = a.conexion(a.cadenad("dbn1_stg_dhyf"), c.ComprobarSP(valorSP));

            if (dtComprobarSP.Rows.Count > 0)
            {
                newSP["SP"] = dtComprobarSP.Rows[0].ItemArray[0].ToString();
            }
            else
            {
                newSP["SP"] = "OJO!!! " + valorSP;
            }
            newSP["SP_TL"] = "spn1_cargar_tracelog_" + tipobbdd + "_" + tab.Replace(valorReplace, "").Replace("mae_", "");

            //Comprobarmos si YA tiene TL generado
            if (dtTLActivado.Rows[0].ItemArray[0].ToString() == "1")
            {
                error = error + "\n\r//** " + tab + " Tiene TRACELOG GENERADO**\\" + "\n\r";
                newSP["TL_gen"] = "OK";
            }
            else
            {
                newSP["TL_gen"] = "";
            }

            // EF02 29/09/2017
            // para que aparezca  "Script TL normalizado_tbn1_documentos_iva_tracelog_TL.sql" ...
            // ... en vez de "Script TL normalizado_documentos_iva_tracelog_TL.sql"
            if (nombreSP.Contains("normalizado"))
            {
                nombreScript = nombreSP.Replace("normalizado", "normalizado_tbn1");
            }
            else
            {
                if (nombreSP.Contains("dimensional"))
                {
                    nombreScript = nombreSP.Replace("dimensional", "dimensional_tbn1");
                }
            }
            ///// fin modif ////////

            //nombrearchivo = "Script TL " + nombreSP + "_tracelog_TL.sql";
            nombrearchivo = "Script TL " + nombreScript + "_tracelog_TL.sql";
            //nombrearchivoexec = "Exec TL " + tipobbdd + "_" + tab + "_tracelog_TL.sql";
            //nombrearchivo = nombrearchivo.Replace("xxx", tipobbdd);
            string fichero = ruta + nombrearchivo;
            dev = a.comprobarficheros(ref lineas, fichero, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                //Escribimos en el fichero
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                    //StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                    //file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                    //file_exec.WriteLine("GO");
                    //sc.generar_file_exec(file_exec, "dbn1_hist_dhyf." + schema + "." + tab + "_tracelog", "dbn1_hist_dhyf", schema, "spn1_cargar_tracelog_" + tipobbdd + "_" + tab, false, true);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                    file.WriteLine("GO");
                    file.WriteLine("");

                    //Activamos CT
                    if (dtCT.Rows[0].ItemArray[0].ToString() == "0")
                    {
                        file.WriteLine("--------------------------------------");
                        error = error + "\n\r//** " + tab + " No tiene CHANGE_TRACKING ACTIVO**\\" + "\n\r";
                        newSP["CT"] = "OK";
                    }
                    else
                    {
                        //file.WriteLine("/*--------------------------------------");
                        newSP["CT"] = "";
                    }
                    if (existe_pk)
                    {
                        file.WriteLine("--Begin Add Change Tracking -> " + tab);
                        string ctp = sc.changetracking(file, tab, bbdd, "dbo", "act");
                        file.WriteLine("--------------------------------------*/");
                    }
                    file.WriteLine("");

                    //Create Table
                    file.WriteLine("--------------------------------------");
                   
                    file.WriteLine("--Begin table create/prepare -> " + tab + "_tracelog");
                    file.WriteLine("");

                    string tab_sin_prefijo;
                    if (tab.Contains("tbn1_"))
                    {
                        tab_sin_prefijo = tab.Replace("tbn1_", "");
                    }
                    else if (tab.Contains("tbn1"))
                    {
                        tab_sin_prefijo = tab.Replace("tbn1", "");
                    }
                    else
                    {
                        tab_sin_prefijo = tab;
                    }

                    sc.regTablas(file, "dbn1_hist_dhyf", schema, tab + "_tracelog", clave + "_tracelog", campos, campospk, csv, false, "historificacion", tab_sin_prefijo);

                    file.WriteLine("--End table create/prepare -> " + tab + "_tracelog");
                    file.WriteLine("--------------------------------------*/");

                    file.WriteLine("GO");

                    #region "Stored Procedure"
                    if (existe_pk)
                    {
                        //SP Creamos SP
                        file.WriteLine("");
                        file.WriteLine("--//Stored Procedure");
                        file.WriteLine("USE dbn1_hist_dhyf");
                        file.WriteLine("GO");
                        file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '" + schema_sp + "' AND ROUTINE_NAME = 'spn1_cargar_tracelog_" + nombreSP + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                        file.WriteLine("    DROP PROCEDURE " + schema_sp + ".spn1_cargar_tracelog_" + nombreSP);
                        file.WriteLine("GO");
                        file.WriteLine("");
                        file.WriteLine("CREATE PROCEDURE " + schema_sp + ".spn1_cargar_tracelog_" + nombreSP + "(@p_id_carga int) AS");
                        file.WriteLine("BEGIN");
                        file.WriteLine("");

                        //SP Cabecera
                        string cab2 = sc.cabeceraLogSP(file, "dbn1_hist_dhyf", schema_sp, "spn1_cargar_tracelog_" + nombreSP, true, true);

                        //SP Registro del SP en tabla de control de cargas incrementales y obtención de datos en variables
                        string sp_inc = sc.regSP_Incremental(file);
                        ////////
                        file.WriteLine("            SELECT @es_carga_completa = es_carga_completa");
                        file.WriteLine("            FROM dbn1_norm_dhyf.audit.tbn1_carga_dwh_maestro");
                        file.WriteLine("            WHERE objeto = @objeto;");
                        file.WriteLine("            --Esta es la fecha que identifica el momento el que se carga en la BB.DD.de Trace Log los registros grababos por el CT desde la Ãºltima vez que corrio este Trace Log");
                        file.WriteLine("           SET @fec_procesado = getdate();");
                        file.WriteLine("");
                        //////
                        //SP Etiquetas            
                        file.WriteLine("---Inicio Bloque común para Incremental y Full");
                        file.WriteLine("");
                        file.WriteLine("            IF @es_carga_completa = 0");
                        file.WriteLine("            BEGIN");

                        //SP Carga Incremental
                        file.WriteLine("--- Inicio Bloque Carga Incremental");
                        file.WriteLine("");
                        file.WriteLine("            SELECT " + campospk + ", sys_change_operation");
                        file.WriteLine("            INTO #CT_TMP");
                        file.WriteLine("            FROM changetable(changes " + bbdd + ".dbo." + tab + ",@ct_" + schema + "_inicial) as CT");
                        file.WriteLine("");
                        file.WriteLine("            INSERT INTO dbn1_hist_dhyf." + schema + "." + tab + "_tracelog(" + campos.Replace("'", "") + ", ctct_fec_procesado, ctct_tipo_operacion)");
                        file.WriteLine("            SELECT");
                        foreach (string d in csv)
                        {
                            string[] j = d.Split(new Char[] { ';' });
                            i++;
                            if (j[2] == "#")
                            {
                                file.WriteLine("                ct." + j[0].ToString() + ", ");
                            }
                            else
                            {
                                file.WriteLine("                s." + j[0].ToString() + ", ");
                            }
                        }
                        file.WriteLine("                @fec_procesado,");
                        file.WriteLine("                CASE ct.sys_change_operation");
                        file.WriteLine("                    WHEN 'I' then 'INSERT'");
                        file.WriteLine("                    WHEN 'U' then 'UPDATE'");
                        file.WriteLine("                    WHEN 'D' then 'DELETE'");
                        file.WriteLine("                END AS ctct_tipo_operacion");
                        file.WriteLine("            FROM #CT_TMP ct");
                        file.WriteLine("            LEFT JOIN " + bbdd + ".dbo." + tab + " s");
                        i = 0;
                        foreach (string d in csv)
                        {
                            string[] j = d.Split(new Char[] { ';' });
                            if (j[2] == "#")
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
                        file.WriteLine("");
                        file.WriteLine("            SET @rc=@@ROWCOUNT;");
                        file.WriteLine("");
                        file.WriteLine("        END");
                        file.WriteLine("");

                        //SP Carga Full
                        file.WriteLine("--Inicio Bloque Carga Full");
                        file.WriteLine("        ELSE");
                        file.WriteLine("        BEGIN");
                        file.WriteLine("            IF NOT EXISTS (SELECT 1 FROM (SELECT count(1) AS num_registros FROM dbn1_hist_dhyf." + schema + "." + tab + "_tracelog) a WHERE num_registros=0)");
                        file.WriteLine("            BEGIN");
                        file.WriteLine("                -- EXECUTE dbn1_stg_dhyf.dbo.spn1_apuntar_warning @log,'No se puede ejecutar la carga inicial de Trace Log porque la Tabla no está vacía!!'");
                        file.WriteLine("                DECLARE @id_warning_1 int");
                        file.WriteLine("                EXEC dbn1_norm_dhyf.audit.spn1_insertar_log @p_id_carga= @p_id_carga,@p_bd= @bd,@p_esquema= @esquema,@p_objeto= @objeto,@p_fecha_inicio= @fecha_inicio,@p_descripcion_warning='No se puede ejecutar la carga inicial de Trace Log porque la Tabla no está vacía!!',@p_out_id= @id_warning_1 OUT");
                        file.WriteLine("            END");
                        file.WriteLine("            ELSE");
                        file.WriteLine("            BEGIN");
                        file.WriteLine("            INSERT INTO dbn1_hist_dhyf." + schema + "." + tab + "_tracelog(" + campos.Replace("'", "") + ", ctct_fec_procesado, ctct_tipo_operacion)");
                        file.WriteLine("            SELECT");
                        foreach (string d in csv)
                        {
                            string[] j = d.Split(new Char[] { ';' });
                            i++;
                            file.WriteLine("                s." + j[0].ToString() + ", ");
                        }
                        file.WriteLine("                @fec_procesado,");
                        file.WriteLine("                'INSERT'");
                        file.WriteLine("            FROM " + bbdd + ".dbo." + tab + " s");
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
                        string pie = sc.pieLogSP(file, "historificacion");

                        file.WriteLine("GO");
                        file.WriteLine("");
                    }
                    else
                    {
                        file.WriteLine("--No hay PK en origen, no genero SP");
                    }
                    #endregion "Stored Procedure"

                    //Agregamos el registro a DT
                    dtSP.Rows.Add(newSP);

                    file.Close();
                    //file_exec.Close();
                }
                catch //(Exception ex)
                {
                    MessageBox.Show("Error al escribir en archivo " + nombrearchivo, "Error escritura archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return "NO";
                }

                if (error == "") 
                {
                    nombrearchivo = "\n\r" + nombrearchivo;
                }
                else
                {
                    nombrearchivo = "\n\r" + error + "\n\r" + nombrearchivo;
                }
                return "OK";
            }
            else
            {
                MessageBox.Show("No se ha podido generar el fichero " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }
        }

        public string csv_precondiciones(DataTable dtSP, string bd, string ruta)
        {
            string[] lineas = new string[0];

            string csvRow;

            string nombrearchivo = "Precondiciones.csv";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.Create), Encoding.UTF8);

                    csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11}", "BD", "ESQUEMA", "OBJETO", "BD_PRECONDICION", "ESQUEMA_PRECONDICION", "OBJETO_PRECONDICION", "ESTADO_PRECONDICION", "FECHA_INICIO_PRECONDICION", "FECHA_FIN_PRECONDICION", "BLOQUE", "GENERA_CT", "TRACELOG_GENERADO");
                    file.WriteLine(csvRow);
                   
                    foreach (DataRow dr in dtSP.Rows)
                    {

                        csvRow = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11}", bd, "dbo", dr.ItemArray[0].ToString(), "dbn1_hist_dhyf", "stg", dr.ItemArray[1].ToString(), "PDTE", "19000101", "19000101", "HISTORIFICACION", dr.ItemArray[2].ToString(), dr.ItemArray[3].ToString());
                        file.WriteLine(csvRow);
                       
                    }

                    file.Close();
                }
                catch //(Exception ex)
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

    }
}

