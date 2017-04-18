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

        public string ScMaestro(string archivo, string[] csv, string ruta, ref string nombrearchivo,Boolean CreateTable)
        {
            string nombrearchivoexec = "";
            int i = 0;
            string tab = "";
            string clave = "";
            string tclave = "";
            string campos = "";
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
                    if (j[2] == "#")
                    {
                        clave = clave + j[0] + ",";
                        tclave = tclave + "t_" + j[0] + ",";
                    }
                }
            }
            campos = campos.Substring(0, campos.Length - 1);
            clave = clave.Substring(0, clave.Length - 1);
            tclave = tclave.Substring(0, tclave.Length - 1);

            string[] lineas = new string[0];
            nombrearchivo = "Script maestro_" + tab + ".sql"; 
            nombrearchivoexec = "Exec maestro_" + tab + ".sql";
            string dev = a.comprobarficheros(ref lineas, ruta, nombrearchivo, 1);
            DataTable valorquery = a.valorQuery(lineas, csv, "maestro", false, "");

            string fichero = ruta + nombrearchivo;
            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);
                StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                file_exec.WriteLine("GO");
                a.generar_file_exec(file_exec, "dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab, "dbn1_stg_dhyf", "dbo", "spn1_cargar_maestro_" + tab);

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

                sc.regTablas(file, "dbn1_dmr_dhyf", "dbo", "tbn1_mae_" + tab, "id_mae_" + tab, campos, "", csv, false, "maestro");

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
                string cab = sc.cabeceraLogSP(file, "dbn1_stg_dhyf", "dbo","spn1_cargar_maestro_" + tab, false, false);

                //SP Cuerpo
                //SP Añadimos registro valor -1
                file.WriteLine("        SET IDENTITY_INSERT dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " ON");
                file.WriteLine("        IF NOT EXISTS(SELECT 1 FROM dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + " WHERE id_mae_" + tab + "=-1)");
                file.WriteLine("            INSERT INTO dbn1_dmr_dhyf.dbo.tbn1_mae_" + tab + "(id_mae_" + tab + "," + clave + ",origen)");
                file.WriteLine("            VALUES(-1,NULL,'MAESTRO')");
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
                                file.WriteLine("         t_" + j[0].ToString() + " " + j[1].ToString());
                            }
                            file.WriteLine("        " + j[0].ToString() + " " + j[1].ToString());
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
    }
}
