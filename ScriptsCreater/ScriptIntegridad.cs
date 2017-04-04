﻿using System;
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

        public string ScIntegridad(string archivo, string[] csv, string ruta, ref string nombrearchivo)
        {

            int i = 0;
            string bdinteg = "";
            string tipobdinteg = "";
            string tabinteg = "";
            string camposinteg = "";
            int camposinteg_num = 0;
            string columnasinteg = "";
            int columnasinteg_num = 0;
            DataTable dtcsv = a.dtCSV(csv, 1);

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
            string dev = a.comprobarficheros(ref lineas, ruta, nombrearchivo, 1);
            string fichero = ruta + nombrearchivo;
            //Escribimos en el fichero
            try
            {
                StreamWriter file = new StreamWriter(new FileStream(fichero, FileMode.CreateNew), Encoding.UTF8);


                file.WriteLine("PRINT 'Script integridad_" + tipobdinteg + "_" + tabinteg + "'");
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
                string[] cab2 = a.cabeceraLogSP("dbn1_stg_dhyf", "dbo", "spn1_integridad_" + tipobdinteg + "_" + tabinteg, false);

                foreach (string l1 in cab2)
                {
                    file.WriteLine(l1);
                }

                //SP Insertamos el valor -1 si no existe
                //Solo tenemos en cuenta si tiene un campo
                if (camposinteg_num == 1)
                {
                    file.WriteLine("--Valor -1 en id si no existe para la tabla Maestros");
                    file.WriteLine("IF NOT EXISTS(SELECT 1 FROM " + bdinteg + ".dbo." + tabinteg + " WHERE " + camposinteg.ToLower().Replace("cod", "id") + " = -1)");
                    file.WriteLine("BEGIN");
                    file.WriteLine("    SET IDENTITY_INSERT " + bdinteg + ".dbo." + tabinteg + " ON");
                    file.WriteLine("    INSERT INTO " + bdinteg + ".dbo." + tabinteg + "(" + camposinteg.ToLower().Replace("cod", "id") + "," + camposinteg + "," + columnasinteg + ")");
                    file.WriteLine("        VALUES(-1,NULL,'N/A')");
                    file.WriteLine("    SET IDENTITY_INSERT " + bdinteg + ".dbo." + tabinteg + " OFF");
                    file.WriteLine("END");
                    file.WriteLine("");
                }
                
                //SP Acciones
                file.WriteLine("--Generamos Query");
                file.WriteLine("; WITH");
                file.WriteLine(" query AS(");
                file.WriteLine("     SELECT " + camposinteg + ", " + columnasinteg);
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
                            file.WriteLine("                    SELECT " + dr.ItemArray[2].ToString() + " AS " + camposinteg + ", " + dr.ItemArray[2].ToString() + " AS " + columnasinteg);
                            file.WriteLine("                    FROM " + dr.ItemArray[0].ToString() + ".dbo." + dr.ItemArray[1].ToString() + "");
                            file.WriteLine("                    GROUP BY " + dr.ItemArray[2].ToString());

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
                    file.WriteLine("            INSERT " + bdinteg + ".dbo." + tabinteg + "(" + camposinteg + ", " + columnasinteg + ")");
                    file.WriteLine("            SELECT query." + camposinteg + ", query." + columnasinteg);
                    file.WriteLine("            FROM query");
                    file.WriteLine("            LEFT JOIN " + bdinteg + ".dbo." + tabinteg + " " + tabinteg + " ON (" + tabinteg + "." + camposinteg + "=query." + camposinteg + ")");
                    file.WriteLine("            WHERE " + tabinteg + "." + camposinteg + " IS NULL");
                    file.WriteLine("            SET @rc = @@ROWCOUNT");
                    file.WriteLine("");
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
