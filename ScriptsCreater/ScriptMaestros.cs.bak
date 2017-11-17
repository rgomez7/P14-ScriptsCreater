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
    class ScriptMaestros
    {
        Acciones a = new Acciones();
        ScriptComun sc = new ScriptComun();

        public string ScMaestro(string archivo, string[] csv, string ruta, ref string nombrearchivo,Boolean CreateTable, Boolean ct)
        {
            string nombrearchivoexec = "";
            int i = 0;
            string tab = "";
            string clave = "";
            string tclave = "";
            string campos = "";
            string camposPK = "";
            string camposCV = "";
            string txtlb = "";

            foreach (string d in csv)
            {
                string[] j = d.Split(new Char[] { ';' });
                if (j[0] == "#nombre")
                {
                    tab = j[1];
                }
                else if (!j[0].Contains("#"))
                {
                    campos = campos + "'" + j[0] + "',";
                    
                    //Campos clave
                    if (j[2].Contains("#"))
                    {
                        camposPK = camposPK + "xxx_" + j[0] + ",";
                    }

                    //Valores para los campos
                    if (j[1].ToString().ToLower().Contains("decimal") || j[1].ToString().ToLower().Contains("numeric") || j[1].ToString().ToLower().Contains("bit"))
                    {
                        camposCV = camposCV + "0,";
                    }
                    //else if (j[1].ToString().ToLower().Contains("date") && j[0].ToString().ToLower().Contains("hasta"))
                    //{
                    //    camposCV = camposCV + "'12/31/9999',";
                    //}
                    //else if (j[1].ToString().ToLower().Contains("date"))
                    //{
                    //    camposCV = camposCV + "'01/01/0001',";
                    //}
                    else if (j[1].ToString().ToLower().Contains("varchar") && j[0].ToString().ToLower().Contains("cod_"))
                    {
                        camposCV = camposCV + "'-',";
                    }
                    else if (j[1].ToString().ToLower().Replace(" ", "").Contains("varchar(1)") || j[1].ToString().ToLower().Replace(" ", "").Contains("varchar(2)"))
                    {
                        camposCV = camposCV + "'-',";
                    }
                    else if (j[1].ToString().ToLower().Contains("varchar"))
                    {
                        camposCV = camposCV + "'N/A',";
                    }
                    else
                    {
                        camposCV = camposCV + "'',";
                    }
                    if (j[2] == "#")
                    {
                        clave = clave + j[0] + ",";
                        tclave = tclave + "t_" + j[0] + ",";
                    }
                }
            }

            if (tab.Contains("mae_"))
            {
                tab = tab.Replace("mae_", "");
            }

            campos = campos.Substring(0, campos.Length - 1);
            camposPK = camposPK.Substring(0, camposPK.Length - 1);
            camposCV = camposCV.Substring(0, camposCV.Length - 1);
            clave = clave.Substring(0, clave.Length - 1);
            tclave = tclave.Substring(0, tclave.Length - 1);

            string[] lineas = new string[0];
            nombrearchivo = "Script maestro_" + tab + ".sql"; 
            nombrearchivoexec = "Exec maestro_" + tab + ".sql";
            string fichero = ruta + nombrearchivo;
            string dev = a.comprobarficheros(ref lineas, fichero, 1);
            DataTable valorquery = a.valorQuery(lineas, csv, "maestro", false, "");

            if (a.comprobarDir(ruta) == "OK")
            {
                //Escribimos en el fichero
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                    StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                    file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                    file_exec.WriteLine("GO");
                    sc.generar_file_exec(file_exec, "dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab, "dbn1_stg_dhyf", "dbo", "spn1_cargar_maestro_" + tab, false, false);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
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
                    file.WriteLine("");
                    file.WriteLine("--Begin table create/prepare -> tbn1_mae_" + tab);
                    file.WriteLine("");

                    //Desactivamos CT
                    if (ct == true)
                    {
                        string ctd = sc.changetracking(file, "tbn1_mae_" + tab, "dbn1_dmr_dhyf", "dbo", "des");
                    }

                    sc.regTablas(file, "dbn1_dmr_dhyf", "dbo", "tbn1_mae_" + tab, "id_mae_" + tab, campos, camposPK, csv, false, "maestro");
                    camposPK = "";

                    //Activamos CT
                    if (ct == true)
                    {
                        string cta = sc.changetracking(file, "tbn1_mae_" + tab, "dbn1_dmr_dhyf", "dbo", "act");
                    }

                    if (CreateTable == false)
                    {
                        file.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        file.WriteLine("--------------------------------------*/");
                    }

                    file.WriteLine("--End table create/prepare -> tbn1_mae_" + tab);
                    file.WriteLine("");

                    //Cambiamos a otra BBDD y empezamos la nueva tarea
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Se crea el SP--");
                    file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_stg_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='dbo' AND ROUTINE_NAME='spn1_cargar_maestro_" + tab + "' AND ROUTINE_TYPE='PROCEDURE')");
                    file.WriteLine("    DROP PROCEDURE dbo.spn1_cargar_maestro_" + tab + ";");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    //Generamos el SP
                    file.WriteLine("CREATE PROCEDURE dbo.spn1_cargar_maestro_" + tab + "(@p_id_carga int)AS");
                    file.WriteLine("BEGIN");

                    //SP Cabecera
                    string cab = sc.cabeceraLogSP(file, "dbn1_stg_dhyf", "dbo", "spn1_cargar_maestro_" + tab, false, false);

                    //SP Cuerpo
                    //SP Añadimos registro valor -1
                    file.WriteLine("        SET IDENTITY_INSERT dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " ON");
                    file.WriteLine("        IF NOT EXISTS(SELECT 1 FROM dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " WHERE id_mae_" + tab + "=-1)");
                    file.WriteLine("            INSERT INTO dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + "(id_mae_" + tab + "," + campos.Replace("'", "") + ",origen)");
                    file.WriteLine("            VALUES(-1," + camposCV + ",'MAESTRO')");
                    file.WriteLine("        SET IDENTITY_INSERT dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " OFF");
                    file.WriteLine("");
                    //SP Creamos objeto temporal
                    file.WriteLine("    IF OBJECT_ID('tempdb..#tmp_mae_" + tab + "') IS NOT NULL");
                    file.WriteLine("        DROP TABLE #tmp_mae_" + tab + "");
                    file.WriteLine("    CREATE table #tmp_mae_" + tab + "(");
                    file.WriteLine("        rr_mode varchar(1),");
                    file.WriteLine("        id_mae_" + tab + " int,");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv.Length)
                            {
                                if (j[2].ToString() == "#")
                                {
                                    file.WriteLine("        t_" + j[0].ToString() + " " + j[1].ToString() + "");
                                }
                                file.WriteLine("        " + j[0].ToString() + " " + j[1].ToString() + "");
                            }
                            else
                            {
                                if (j[2].ToString() == "#")
                                {
                                    file.WriteLine("         t_" + j[0].ToString() + " " + j[1].ToString() + ",");
                                }
                                file.WriteLine("        " + j[0].ToString() + " " + j[1].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("    );");
                    file.WriteLine("");

                    //SP Insertamos carga de datos a Maestro
                    //SP Creamos la insert de carga de datos
                    foreach (DataRow l2 in valorquery.Rows)
                    {
                        file.WriteLine(l2.ItemArray[0].ToString());
                    }
                    file.WriteLine("");

                    //SP Insert en object temporal
                    file.WriteLine("        INSERT INTO #tmp_mae_" + tab + " (rr_mode,id_mae_" + tab + "," + tclave + "," + campos.Replace("'", "") + ")");
                    file.WriteLine("        SELECT");
                    file.WriteLine("            rr_mode=");
                    file.WriteLine("                CASE");
                    file.WriteLine("                    WHEN tbn1_mae_" + tab + ".id_mae_" + tab + " IS NULL THEN 'I'");
                    txtlb = "";
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[2].ToString() == "#")
                            {
                                txtlb = txtlb + " query." + j[0].ToString() + " IS NULL AND";
                            }
                        }
                    }
                    file.WriteLine("                    WHEN " + txtlb + " id_mae_" + tab + "<>-1 THEN 'D'");
                    file.WriteLine("                    ELSE 'U' END,");
                    file.WriteLine("            tbn1_mae_" + tab + ".id_mae_" + tab + " AS id_mae_" + tab + ",");
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[2].ToString() == "#")
                            {
                                file.WriteLine("            tbn1_mae_" + tab + "." + j[0].ToString() + " AS t_" + j[0].ToString() + ",");
                            }
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
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("            query." + j[0].ToString() + " AS " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " AS tbn1_mae_" + tab);
                    file.WriteLine("        FULL JOIN query on (");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[2].ToString() == "#")
                            {
                                if (i > 0)
                                {
                                    file.WriteLine("        AND ");
                                }
                                file.WriteLine("        (query." + j[0].ToString() + "=tbn1_mae_" + tab + "." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND tbn1_mae_" + tab + "." + j[0].ToString() + " IS NULL))");
                                i++;
                            }
                        }
                    }
                    file.WriteLine("        )");
                    file.WriteLine("        WHERE");
                    file.WriteLine("            id_mae_" + tab + " IS NULL OR");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (j[2] == "#")
                            {
                                file.WriteLine("            (query." + j[0].ToString() + " IS NULL AND id_mae_" + tab + "<>-1)");
                            }
                            else
                            {
                                file.WriteLine("            (tbn1_mae_" + tab + "." + j[0].ToString() + "<>query." + j[0].ToString() + " ");
                                file.WriteLine("               OR (tbn1_mae_" + tab + "." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL) ");
                                file.WriteLine("               OR (tbn1_mae_" + tab + "." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                            }

                            if (i < csv.Length)
                            {
                                file.WriteLine("            OR ");
                            }
                        }
                    }

                    file.WriteLine("");
                    file.WriteLine("        SET @idx_reclim = 10000");
                    file.WriteLine("        SELECT @count_all = count(8) from #tmp_mae_" + tab + "");
                    file.WriteLine("        SELECT @count_ins = count(8) from #tmp_mae_" + tab + " where rr_mode='I'");
                    file.WriteLine("");
                    //Actualizamos registros en DMR
                    file.WriteLine("        UPDATE dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab);
                    file.WriteLine("            SET");
                    file.WriteLine("                origen='MAESTRO',");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        i++;
                        if (!j[0].Contains("#"))
                        {
                            if (i == csv.Length)
                            {
                                file.WriteLine("                " + j[0].ToString() + "=s." + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("                " + j[0].ToString() + "=s." + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("        FROM (");
                    file.WriteLine("            SELECT");
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
                    file.WriteLine("            FROM #tmp_mae_" + tab);
                    file.WriteLine("            WHERE rr_mode='U') s");
                    file.WriteLine("        WHERE ");
                    i = 0;
                    foreach (string d in csv)
                    {
                        string[] j = d.Split(new Char[] { ';' });
                        if (!j[0].Contains("#"))
                        {
                            if (j[2].ToString() == "#")
                            {
                                if (i > 0)
                                {
                                    file.WriteLine("        AND ");
                                }
                                file.WriteLine("            (dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + "." + j[0].ToString() + " = s." + j[0].ToString() + " OR (tbn1_mae_" + tab + "." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL))");
                                i++;
                            }
                        }
                    }
                    file.WriteLine("        SET @rc=@@ROWCOUNT");
                    file.WriteLine("");
                    //SP Modificamos el Indice y realizamos el Insert
                    file.WriteLine("        IF @count_ins >= @idx_reclim");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("            IF EXISTS (SELECT 1 FROM dbn1_dmr_dhyf.sys.indexes WHERE name = 'IX_tbn1_mae_" + tab + "_unique')");
                    file.WriteLine("            ALTER INDEX IX_tbn1_mae_" + tab + "_unique ON dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " DISABLE");
                    file.WriteLine("        END");
                    file.WriteLine("");
                    file.WriteLine("        INSERT INTO dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + "(" + campos.Replace("'", "") + ",origen)");
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
                                file.WriteLine("                " + j[0].ToString());
                            }
                            else
                            {
                                file.WriteLine("                " + j[0].ToString() + ",");
                            }
                        }
                    }
                    file.WriteLine("            ,'MAESTRO' as origen");
                    file.WriteLine("            FROM #tmp_mae_" + tab + "");
                    file.WriteLine("            WHERE rr_mode='I';");
                    file.WriteLine("       SET @rc = @rc + @@ROWCOUNT;");
                    file.WriteLine("");
                    file.WriteLine("        IF @count_ins >= @idx_reclim");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("            IF EXISTS (SELECT 1 FROM dbn1_dmr_dhyf.sys.indexes WHERE name = 'IX_tbn1_mae_" + tab + "_unique')");
                    file.WriteLine("            ALTER INDEX IX_tbn1_mae_" + tab + "_unique ON dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " REBUILD");
                    file.WriteLine("        END");
                    file.WriteLine("");
                    //SP Borramos object temporal
                    file.WriteLine("        IF OBJECT_ID('tempdb..#tmp_mae_" + tab + "') IS NOT NULL");
                    file.WriteLine("            DROP TABLE #tmp_mae_" + tab + "");
                    file.WriteLine("");

                    //SP Pie
                    string pie = sc.pieLogSP(file, "maestro");

                    file.WriteLine("GO");
                    file.WriteLine("");


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

        //Maestros DS incremental sin DELETE
        public string ScMaestro_inc(string archivo, string[] csv, string ruta, ref string nombrearchivo, Boolean incremental, Boolean CreateTable, Boolean ChangeTrack)
            {
                string nombrearchivoexec = "";
                string fichero;
                string tab = "";
                string bd = "dbn1_dmr_dhyf";
                string clave = "";
                string campos = "";
                string campospk = "";
                string camposCV = "";
                string camposfilter = "";
                string schema = "dbo";
                string[] csv2 = new string[0];

                int i = 0;
                DataRow[] dr;

                csv = a.ordenarCSV(csv);

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
                            campospk = campospk + "xxx_" + j[0] + ",";
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
                    //Valores para los campos
                    if (j[1].ToString().ToLower().Contains("decimal") || j[1].ToString().ToLower().Contains("numeric") || j[1].ToString().ToLower().Contains("bit"))
                    {
                        camposCV = camposCV + "0,";
                    }
                    //else if (j[1].ToString().ToLower().Contains("date") && j[0].ToString().ToLower().Contains("hasta"))
                    //{
                    //    camposCV = camposCV + "'12/31/9999',";
                    //}
                    //else if (j[1].ToString().ToLower().Contains("date"))
                    //{
                    //    camposCV = camposCV + "'01/01/0001',";
                    //}
                    else if (j[1].ToString().ToLower().Contains("varchar") && j[0].ToString().ToLower().Contains("cod_"))
                    {
                        camposCV = camposCV + "'-',";
                    }
                    else if (j[1].ToString().ToLower().Replace(" ", "").Contains("varchar(1)") || j[1].ToString().ToLower().Replace(" ", "").Contains("varchar(2)"))
                    {
                        camposCV = camposCV + "'-',";
                    }
                    else if (j[1].ToString().ToLower().Contains("varchar"))
                    {
                        camposCV = camposCV + "'N/A',";
                    }
                    else
                    {
                        camposCV = camposCV + "'',";
                    }
                }
                //si tab no lleva el prefijo mae, se lo asignamos
                if (!tab.Contains("mae_"))
                {
                    tab = "mae_" + tab;
                }

                campos = campos.Substring(0, campos.Length - 1);
                campospk = campospk.Substring(0, campospk.Length - 1);
                camposCV = camposCV.Substring(0, camposCV.Length - 1);
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
                nombrearchivo = "Script maestro_" + tab + ".sql";
                nombrearchivoexec = "Exec maestro_" + tab + ".sql";
                string[] lineas = new string[0];
                fichero = ruta + nombrearchivo;
                string dev = a.comprobarficheros(ref lineas, fichero, 1);
                DataTable valorquery = a.valorQuery(lineas, csv, "ds", incremental, tab);

                //Escribimos en el fichero
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                    StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                    file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                    file_exec.WriteLine("GO");
                    sc.generar_file_exec(file_exec, bd + ".dbo.tbn1_" + tab, "dbn1_stg_dhyf", "dbo", "spn1_cargar_maestro_" + tab, incremental, true);

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

                    file.WriteLine("--------------------------------------*/");

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

                    file.WriteLine("--------------------------------------*/");

                    file.WriteLine("--End table create/prepare -> tbn1_" + tab);
                    file.WriteLine("");
                    #endregion "Tabla"

                    #region "SP"

                    #region "CabeceraSP"
                    //SP Creamos SP
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");
                    file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_stg_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'spn1_cargar_maestro_" + tab + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                    file.WriteLine("    DROP PROCEDURE dbo.spn1_cargar_maestro_" + tab);
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("CREATE PROCEDURE dbo.spn1_cargar_maestro_" + tab + "(@p_id_carga int) AS");
                    file.WriteLine("BEGIN");
                    file.WriteLine("");

                    //SP Cabecera
                    string cab2 = sc.cabeceraLogSP(file, "dbn1_stg_dhyf", "dbo", "spn1_cargar_maestro_" + tab, incremental, false);

                    if (incremental == true)
                    {
                        //SP Registro del SP en tabla de control de cargas incrementales y obtención de datos en variables
                        string sp_inc = sc.regSP_Incremental(file);
                    }

                    //SP Añadimos registro valor -1
                    file.WriteLine("        SET IDENTITY_INSERT dbn1_dmr_dhyf.dbo.tbn1_" + tab + " ON");
                    file.WriteLine("        IF NOT EXISTS(SELECT 1 FROM dbn1_dmr_dhyf.dbo.tbn1_" + tab + " WHERE id_" + tab + "=-1)");
                    file.WriteLine("            INSERT INTO dbn1_dmr_dhyf.dbo.tbn1_" + tab + "(id_" + tab + "," + campos.Replace("'", "") + ",origen)");
                    file.WriteLine("            VALUES(-1," + camposCV + ",'MAESTRO')");
                    file.WriteLine("        SET IDENTITY_INSERT dbn1_dmr_dhyf.dbo.tbn1_" + tab + " OFF");
                    file.WriteLine("");

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
                                file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL");
                            }
                            else
                            {
                                file.WriteLine("                " + j[0].ToString() + " " + j[1].ToString() + " NOT NULL,");
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
                        file.WriteLine("        (" + campos.Replace("'", "") + ")");
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
                        file.WriteLine("        INSERT INTO #tmp_" + tab + " (rr_mode,cc," + clave + ", " + campospk.Replace("xxx_","t_") + "," + campos.Replace("'", "") + ")");
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
                                        //file.WriteLine("            (keys." + j[0].ToString() + " = t." + j[0].ToString() + " OR (keys." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                                        file.WriteLine("            (keys." + j[0].ToString() + " = t." + j[0].ToString() + "))");
                                    }
                                    else
                                    {
                                        //file.WriteLine("            (keys." + j[0].ToString() + " = t." + j[0].ToString() + " OR (keys." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                                        file.WriteLine("            (keys." + j[0].ToString() + " = t." + j[0].ToString() + ") AND");
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
                                        //file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                                        file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + "))");
                                    }
                                    else
                                    {
                                        //file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                                        file.WriteLine("            (query." + j[0].ToString() + " = t." + j[0].ToString() + ") AND");
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
                                    //file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                    //file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                                    file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString());
                                }
                                else if (i == campos.Split(new Char[] { ',' }).Length)
                                {
                                    //file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                    //file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                                    file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + ")");
                                }
                                else
                                {
                                    //file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                    //file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                                    file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString());
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
                    file.WriteLine("        INSERT INTO #tmp_" + tab + " (rr_mode,cc," + clave + ", " + campospk.Replace("xxx_", "t_") + "," + campos.Replace("'", "") + ")");
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
                                    //file.WriteLine("        (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)))");
                                    file.WriteLine("        (query." + j[0].ToString() + " = t." + j[0].ToString() + "))");
                                }
                                else
                                {
                                    //file.WriteLine("        (query." + j[0].ToString() + " = t." + j[0].ToString() + " OR (query." + j[0].ToString() + " IS NULL AND t." + j[0].ToString() + " IS NULL)) AND");
                                    file.WriteLine("        (query." + j[0].ToString() + " = t." + j[0].ToString() + ") AND");
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
                                //file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                //file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                                file.WriteLine("                OR (t." + j[0].ToString() + " <> query." + j[0].ToString());
                            }
                            else if (i == campos.Split(new Char[] { ',' }).Length)
                            {
                                //file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                //file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL))");
                                file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + ")");
                            }
                            else
                            {
                                //file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString() + " OR (t." + j[0].ToString() + " IS NULL AND query." + j[0].ToString() + " IS NOT NULL)");
                                //file.WriteLine("                    OR (t." + j[0].ToString() + " IS NOT NULL AND query." + j[0].ToString() + " IS NULL)");
                                file.WriteLine("                OR t." + j[0].ToString() + " <> query." + j[0].ToString());
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
                                    //file.WriteLine("            WHERE (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + "=s." + j[0].ToString() + " OR (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL))");
                                    file.WriteLine("            WHERE (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + "=s." + j[0].ToString() + ")");
                                }
                                else
                                {
                                    //file.WriteLine("                AND (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + "=s." + j[0].ToString() + " OR (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + " IS NULL AND s." + j[0].ToString() + " IS NULL))");
                                    file.WriteLine("                AND (" + bd + "." + schema + ".tbn1_" + tab + "." + j[0].ToString() + "=s." + j[0].ToString() + ")");
                                }
                                i++;
                            }
                        }
                    }
                    file.WriteLine("        SET @rc=@@ROWCOUNT;");
                    file.WriteLine("");

                    //SP Desactivamos Indice de la tabla principal
                    file.WriteLine("        IF @count_ins >= @idx_reclim");
                    file.WriteLine("        BEGIN");
                    file.WriteLine("            IF EXISTS (SELECT 1 FROM " + bd + ".sys.indexes WHERE name = 'IX_tbn1_" + tab + "_unique')");
                    file.WriteLine("            ALTER INDEX IX_tbn1_" + tab + "_unique ON " + bd + "." + schema + ".tbn1_" + tab + " DISABLE;");
                    file.WriteLine("        END");
                    file.WriteLine("");
                    //SP Insertamos nuevos registros
                    file.WriteLine("        INSERT INTO " + bd + "." + schema + ".tbn1_" + tab + " WITH(TABLOCK) (" + campos.Replace("'", "") + ")");
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

    }
}
