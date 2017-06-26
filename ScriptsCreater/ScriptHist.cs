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
        ScriptComun sc = new ScriptComun();

        public string hist(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean claveAuto, Boolean CreateTable, Boolean ChangeTrack)
        {
            string nombrearchivoexec = "";
            int i = 0;
            string bd = "";
            string tab = "";
            string clave = "";
            string campospk = "";
            string campos = "";
            string tipobd = "";
            string schema = "";
            string schema_sp = "stg";
            string cabtab = "";
            string[] lineas = new string[0];
            string dev = "";
            string[] csv2 = new string[0];

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
                    Array.Resize(ref csv2, csv2.Length + 1);
                    csv2[csv2.Length - 1] = j[0].ToString() + ";" + j[1].ToString() + ";" + j[4].ToString() + ";";
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
            if (bd.Contains("dmr") || tab.Contains("dm"))
            {
                tipobd = "dimensional";
                schema = "dmr";
                if (bd == "")
                {
                    bd = "dbn1_dmr_dhyf";
                }
            }
            else if (bd.Contains("stg"))
            {
                tipobd = "staging";
                schema = "stg";
            }
            else if (bd.Contains("norm") || tab.Contains("ds"))
            {
                tipobd = "normalizado";
                schema = "norm";
                if (bd == "")
                {
                    bd = "dbn1_norm_dhyf";
                }
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
            nombrearchivoexec = "Exec TL " + tipobd + "_" + cabtab + tab + "_tracelog_TL.sql";
            //nombrearchivo = nombrearchivo.Replace("xxx", tipobd);
            string fichero = ruta + nombrearchivo;
            dev = a.comprobarficheros(ref lineas, fichero, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                //Escribimos en el fichero
                try
                {

                    StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                    StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                    file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                    file_exec.WriteLine("GO");
                    sc.generar_file_exec(file_exec, "dbn1_hist_dhyf." + schema + "." + cabtab + tab + "_tracelog", "dbn1_hist_dhyf", schema, "spn1_cargar_tracelog_" + tipobd + "_" + tab, false, true);

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
                    file.WriteLine("--Begin Add Change Tracking -> " + cabtab + tab);
                    string ctp = sc.changetracking(file, cabtab + tab, bd, "dbo", "act");
                    file.WriteLine("--Begin Add Change Tracking -> " + cabtab + tab);
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

                    sc.regTablas(file, "dbn1_hist_dhyf", schema, cabtab + tab + "_tracelog", clave + "_tracelog", campos, campospk, csv2, claveAuto, "historificacion");

                    file.WriteLine("--End table create/prepare -> " + cabtab + tab + "_tracelog");
                    if (CreateTable == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("--------------------------------------*/");
                    }
                    file.WriteLine("");

                    #region "Stored Procedure"
                    //SP Creamos SP
                    file.WriteLine("");
                    file.WriteLine("--//Stored Procedure");
                    file.WriteLine("USE dbn1_hist_dhyf");
                    file.WriteLine("GO");
                    file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_hist_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '" + schema_sp + "' AND ROUTINE_NAME = 'spn1_cargar_tracelog_" + tipobd + "_" + tab + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                    file.WriteLine("    DROP PROCEDURE " + schema_sp + ".spn1_cargar_tracelog_" + tipobd + "_" + tab);
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("CREATE PROCEDURE " + schema_sp + ".spn1_cargar_tracelog_" + tipobd + "_" + tab + "(@p_id_carga int) AS");
                    file.WriteLine("BEGIN");
                    file.WriteLine("");

                    //SP Cabecera
                    string cab2 = sc.cabeceraLogSP(file, "dbn1_hist_dhyf", schema_sp, "spn1_cargar_tracelog_" + tipobd + "_" + tab, true, true);

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
                    file.WriteLine("            SELECT " + campospk.Replace("t_", "") + ", sys_change_operation");
                    file.WriteLine("            INTO #CT_TMP");
                    file.WriteLine("            FROM changetable(changes " + bd + ".dbo." + cabtab + tab + ",@ct_" + schema + "_inicial) as CT");
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
                    string pie = sc.pieLogSP(file, "historificacion");

                    file.WriteLine("GO");
                    file.WriteLine("");
                    #endregion "Stored Procedure"

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
            else
            {
                MessageBox.Show("No se ha podido generar el fichero " + nombrearchivo + " porque no se encuentra la ruta", "Error ruta fichero", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return "NO";
            }
        }
    }
}
