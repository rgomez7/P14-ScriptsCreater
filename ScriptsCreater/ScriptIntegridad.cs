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
    class ScriptIntegridad
    {
        Acciones a = new Acciones();
        ScriptComun sc = new ScriptComun();

        public string ScIntegridad(string archivo, string[] csv, string ruta, ref string nombrearchivo)
        {
            string nombrearchivoexec = "";
            int i = 0;
            string bdinteg = "";
            string tipobdinteg = "";
            string tabinteg = "";
            string camposinteg = "";
            int camposinteg_num = 0;
            string columnasinteg = "";
            int columnasinteg_num = 0;
            string[] cinteg;
            DataTable dtcsv = a.dtCSV(csv, 1, false);

            foreach (DataColumn dc in dtcsv.Columns)
            {
                if (dc.ColumnName.ToLower().Contains("bd"))
                {
                    bdinteg = dtcsv.Rows[0].ItemArray[dc.Ordinal].ToString();
                    
                }
                else if (dc.ColumnName.ToLower().Contains("tabla"))
                {
                    tabinteg = dtcsv.Rows[0].ItemArray[dc.Ordinal].ToString();
                }
                else if (dc.ColumnName.ToLower().Contains("campo"))
                {
                    camposinteg = camposinteg + dtcsv.Rows[0].ItemArray[dc.Ordinal].ToString() + ", ";
                    camposinteg_num++;
                }
                else if (dc.ColumnName.ToLower().Contains("desc"))
                {
                    columnasinteg = columnasinteg + dtcsv.Rows[0].ItemArray[dc.Ordinal].ToString() + ", ";
                    columnasinteg_num++;
                }
            }
            camposinteg = camposinteg.Substring(0, camposinteg.Length - 2);
            columnasinteg = columnasinteg.Substring(0, columnasinteg.Length - 2);

            if (bdinteg.Contains("norm"))
            {
                tipobdinteg = "normalizado";
            }
            else if (bdinteg.Contains("dmr"))
            {
                tipobdinteg = "dimensional";
            }
            else
            {
                tipobdinteg = "staging";
            }

            string[] lineas = new string[0];
            nombrearchivo = "Script integridad_" + tipobdinteg + "_" + tabinteg + ".sql";
            nombrearchivoexec = "Exec integridad_" + tipobdinteg + "_" + tabinteg + ".sql";
            string dev = a.comprobarficheros(ref lineas, ruta + nombrearchivo, 1);

            if (a.comprobarDir(ruta) == "OK")
            {
                //Escribimos en el fichero
                try
                {
                    StreamWriter file = new StreamWriter(new FileStream(ruta + nombrearchivo, FileMode.CreateNew), Encoding.UTF8);
                    StreamWriter file_exec = new StreamWriter(new FileStream(ruta + nombrearchivoexec, FileMode.Create), Encoding.UTF8);

                    file_exec.WriteLine("PRINT '" + nombrearchivoexec + "'");
                    file_exec.WriteLine("GO");
                    sc.generar_file_exec(file_exec, bdinteg + ".dbo." + tabinteg, "dbn1_stg_dhyf", "dbo", "spn1_integridad_" + tipobdinteg + "_" + tabinteg, false, false);

                    file.WriteLine("PRINT '" + nombrearchivo + "'");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("--Generado versión vb " + a.version);
                    file.WriteLine("");
                    file.WriteLine("SET QUOTED_IDENTIFIER ON;");
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("USE dbn1_stg_dhyf");
                    file.WriteLine("GO");
                    file.WriteLine("IF EXISTS (SELECT 1 FROM dbn1_stg_dhyf.INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'spn1_integridad_" + tipobdinteg + "_" + tabinteg + "' AND ROUTINE_TYPE = 'PROCEDURE')");
                    file.WriteLine("    DROP PROCEDURE dbo.spn1_integridad_" + tipobdinteg + "_" + tabinteg);
                    file.WriteLine("GO");
                    file.WriteLine("");
                    file.WriteLine("CREATE PROCEDURE dbo.spn1_integridad_" + tipobdinteg + "_" + tabinteg + "(@p_id_carga int) AS");
                    file.WriteLine("BEGIN");
                    file.WriteLine("");

                    //SP Cabecera
                    string cab2 = sc.cabeceraLogSP(file, "dbn1_stg_dhyf", "dbo", "spn1_integridad_" + tipobdinteg + "_" + tabinteg, false, false);

                    //SP Insertamos el valor -1 si no existe
                    //Solo tenemos en cuenta si tiene un campo
                    if (camposinteg_num == 1)
                    {
                        file.WriteLine("--Valor -1 en id si no existe para la tabla Maestros");
                        file.WriteLine("IF NOT EXISTS(SELECT 1 FROM " + bdinteg + ".dbo." + tabinteg + " WHERE " + camposinteg.ToLower().Replace("cod", "id") + " = -1)");
                        file.WriteLine("BEGIN");
                        file.WriteLine("    SET IDENTITY_INSERT " + bdinteg + ".dbo." + tabinteg + " ON");
                        file.WriteLine("    INSERT INTO " + bdinteg + ".dbo." + tabinteg + "(" + camposinteg.ToLower().Replace("cod", "id") + "," + camposinteg + "," + columnasinteg + ")");
                        file.WriteLine("        VALUES(-1,'','N/A')");
                        file.WriteLine("    SET IDENTITY_INSERT " + bdinteg + ".dbo." + tabinteg + " OFF");
                        file.WriteLine("END");
                        file.WriteLine("");
                    }
                    else if (camposinteg_num == 2)
                    {
                        cinteg = camposinteg.Split(',');
                        i = 0;
                        foreach (string ci in cinteg)
                        {
                            if (tabinteg.ToLower().Contains(cinteg[i].ToLower().Replace("cod_", "").Replace(" ", "")))
                            {
                                break;
                            }
                            i++;
                        }
                        file.WriteLine("--Valor -1 en id si no existe para la tabla Maestros");
                        file.WriteLine("IF NOT EXISTS(SELECT 1 FROM " + bdinteg + ".dbo." + tabinteg + " WHERE " + cinteg[0].ToLower().Replace("cod", "id") + " = -1 AND " + cinteg[1].ToLower().Replace("cod", "id") + " = -1)");
                        file.WriteLine("BEGIN");
                        file.WriteLine("    SET IDENTITY_INSERT " + bdinteg + ".dbo." + tabinteg + " ON");
                        file.WriteLine("    INSERT INTO " + bdinteg + ".dbo." + tabinteg + "(" + cinteg[i].ToLower().Replace("cod", "id") + "," + camposinteg + "," + columnasinteg + ", ORIGEN)");
                        file.WriteLine("        VALUES(-1,'','','N/A', 'MAESTRO')");
                        file.WriteLine("    SET IDENTITY_INSERT " + bdinteg + ".dbo." + tabinteg + " OFF");
                        file.WriteLine("END");
                        file.WriteLine("");
                    }

                    //SP Acciones
                    file.WriteLine("--Generamos Query");
                    file.WriteLine("); WITH");
                    file.WriteLine(" query AS(");
                    file.WriteLine("     SELECT " + camposinteg + ", " + columnasinteg );
                    file.WriteLine("");
                    file.WriteLine("     FROM(");

                    i = 0;
                    //Solo tenemos en cuenta si tiene un campo
                    if (camposinteg_num == 1)
                    {
                        foreach (DataRow dr in dtcsv.Rows)
                        {
                            if (dr.ItemArray[1].ToString() != tabinteg)
                            {
                                i++;
                                file.WriteLine("                    SELECT " + dr.ItemArray[2].ToString() + " AS " + camposinteg + ", " + dr.ItemArray[2].ToString() + " AS " + columnasinteg );
                                file.WriteLine("                    FROM " + dr.ItemArray[0].ToString() + ".dbo." + dr.ItemArray[1].ToString());
                                file.WriteLine("                    GROUP BY " + dr.ItemArray[2].ToString() + ", " + dr.ItemArray[3].ToString());

                                if (i < dtcsv.Rows.Count)
                                    file.WriteLine("                    UNION");
                            }
                            else
                            {
                                i++;
                            }
                        }
                        file.WriteLine("                ) AS O");
                        file.WriteLine("            )");

                        //Montamos el Insert sobre la Query
                        file.WriteLine("            INSERT " + bdinteg + ".dbo." + tabinteg + "(" + camposinteg + ", " + columnasinteg + ", ORIGEN)");
                        file.WriteLine("            SELECT query." + camposinteg + ", query." + columnasinteg + ", '" + tabinteg + "' as ORIGEN");
                        file.WriteLine("            FROM query");
                        file.WriteLine("            LEFT JOIN " + bdinteg + ".dbo." + tabinteg + " " + tabinteg + " ON (" + tabinteg + "." + camposinteg + "=query." + camposinteg + ")");
                        file.WriteLine("            WHERE " + tabinteg + "." + camposinteg + " IS NULL");
                        file.WriteLine("            SET @rc = @@ROWCOUNT");
                        file.WriteLine("");

                    }
                    //Solo tenemos en cuenta si tiene dos campos
                    else if (camposinteg_num == 2)
                    {
                        cinteg = camposinteg.Split(',');
                        foreach (DataRow dr in dtcsv.Rows)
                        {
                            if (dr.ItemArray[1].ToString() != tabinteg)
                            {
                                i++;
                                file.WriteLine("                    SELECT " + dr.ItemArray[2].ToString() + " AS " + cinteg[0].ToString() + ", " + dr.ItemArray[3].ToString() + " AS " + cinteg[1].ToString() + ", " + dr.ItemArray[2].ToString() + " AS " + columnasinteg );
                                file.WriteLine("                    FROM " + dr.ItemArray[0].ToString() + ".dbo." + dr.ItemArray[1].ToString());
                                file.WriteLine("                    GROUP BY " + dr.ItemArray[2].ToString() + ", " + dr.ItemArray[3].ToString());

                                if (i < dtcsv.Rows.Count)
                                    file.WriteLine("                    UNION");
                            }
                            else
                            {
                                i++;
                            }
                        }
                        file.WriteLine("                ) AS O");
                        file.WriteLine("            )");

                        //Montamos el Insert sobre la Query
                        file.WriteLine("            INSERT " + bdinteg + ".dbo." + tabinteg + "(" + camposinteg + ", " + columnasinteg + ", ORIGEN)");
                        file.WriteLine("            SELECT query." + cinteg[0] + ", query." + cinteg[1] + ", query." + columnasinteg + ", '" + tabinteg + "' as ORIGEN");
                        file.WriteLine("            FROM query");
                        file.WriteLine("            LEFT JOIN " + bdinteg + ".dbo." + tabinteg + " " + tabinteg + " ON (" + tabinteg + "." + cinteg[0] + "=query." + cinteg[0] + " AND " + tabinteg + "." + cinteg[1] + "=query." + cinteg[1] + ")");
                        file.WriteLine("            WHERE " + tabinteg + "." + cinteg[0] + " IS NULL OR " + tabinteg + "." + cinteg[1] + " IS NULL");
                        file.WriteLine("            SET @rc = @@ROWCOUNT");
                        file.WriteLine("");
                    }

                    //SP Pie
                    string pie = sc.pieLogSP(file, "integridad");

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

    }
}
