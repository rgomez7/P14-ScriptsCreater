using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsCreator
{
    class Consultas
    {
        public string Columns(string tabla)
        {
            return "SELECT  DISTINCT " +
                            "colu.column_name, " +
                            "colu.is_nullable, " +
                            "colu.data_type, " +
                            "colu.CHARACTER_MAXIMUM_LENGTH, " +
                            "colu.NUMERIC_PRECISION, " +
                            "colu.NUMERIC_SCALE, " +
                            "columnproperty(object_id(colu.TABLE_NAME), colu.column_name, 'IsIdentity') as es_identity, " +
                            "COALESCE(COALESCE(capt.value, ms_d.value), '') AS comentario, " +
                            "colu.ORDINAL_POSITION " +
                   "FROM    INFORMATION_SCHEMA.COLUMNS colu " +
                            "LEFT JOIN sys.extended_properties ms_d " +
                                    "ON  OBJECT_ID(colu.TABLE_SCHEMA +'.' + colu.TABLE_NAME) = ms_d.major_id " +
                                    "AND colu.ORDINAL_POSITION = ms_d.minor_id " +
                                    "AND ms_d.name = 'MS_Description' " +
                                    "AND ms_d.class = 1 " +
                            "LEFT JOIN sys.extended_properties capt " +
                                    "ON  OBJECT_ID(colu.TABLE_SCHEMA + '.' + colu.TABLE_NAME) = capt.major_id " +
                                    "AND colu.ORDINAL_POSITION = capt.minor_id " +
                                    "AND capt.name = 'Caption' " +
                                    "AND capt.class = 1 " +
                   "WHERE   colu.TABLE_NAME = '" + tabla + "' " +
                   "ORDER BY colu.ORDINAL_POSITION";
        }

        public string ColumnsClaves(string tabla)
        {
            return "SELECT i.name as nameindex, i.type as typeindex, i.type_desc as descIndex, " +
                   "    i.is_unique, i.is_primary_key, i.is_unique_constraint, i.index_id, " +
                   "    ic.index_column_id, ic.column_id, c.name as namecolumn " +
                   "FROM sys.indexes as i " +
                   "    INNER JOIN sys.index_columns as ic ON ic.object_id = i.object_id and ic.index_id = i.index_id " +
                   "    INNER JOIN sys.columns as c ON c.object_id = ic.object_id and ic.column_id = c.column_id " +
                   "WHERE i.object_id = OBJECT_ID('" + tabla + "') " +
                   "ORDER BY index_id, index_column_id";
        }

        public string PropiedadesExtendidas(string esquema, string tabla)
        {
            return "SELECT objtype, objname, name, value FROM fn_listextendedproperty(NULL, 'schema', '" + esquema + "', 'table', '" + tabla + "', 'column', default) " +
                   "UNION " +
                   "SELECT objtype, objname, name, value FROM fn_listextendedproperty(NULL, 'schema', '" + esquema + "', 'table', '" + tabla + "', default, default)";
        }

        public string ChangeTrackingActivo(string tabla)
        {
            return "SELECT Count(1) as Activo FROM sys.change_tracking_tables tt " +
                        "INNER JOIN sys.objects obj ON obj.object_id = tt.object_id " +
                        "WHERE obj.name = '" + tabla + "'";
        }

        public string ComprobarTabla(string tabla, string schema)
        {
            //return "SELECT count(id) as contador FROM sysobjects WHERE type = 'U' AND name = '" + tabla + "'";
            return "SELECT count(1) FROM sys.tables as t INNER JOIN sys.schemas as s ON t.schema_id = s.schema_id WHERE t.name = '" + tabla + "' and s.name = '" + schema + "'";
        }

        public string ComprobarTL(string tabla)
        {
            return "SELECT count(1) as contador FROM sys.tables WHERE name = '" + tabla + "_tracelog'";
        }

        public string ComprobarSP(string sp)
        {
            return "SELECT name FROM sys.objects WHERE type = 'P' and name = '" + sp + "' " +
                    " UNION " +
                   "SELECT name FROM sys.objects WHERE type = 'P' and name = '" + sp + "_ssis'"+
                   " UNION " +
                   "SELECT name FROM sys.objects WHERE type = 'P' and name = '" + sp.Replace("mae_", "maestro_") + "' " +
                   " UNION " +
                   "SELECT name FROM sys.objects WHERE type = 'P' and name = '" + sp.Replace("mae_", "maestro_") + "_ssis'";
        }

        public string ComentarioTabla(string tabla)
        {
            return "SELECT TOP 1 prop.value FROM INFORMATION_SCHEMA.TABLES tabl LEFT JOIN sys.extended_properties prop ON OBJECT_ID(tabl.TABLE_SCHEMA + '.' + tabl.TABLE_NAME) = prop.major_id AND prop.minor_id = 0 AND prop.name = 'MS_Description' AND prop.class = 1 WHERE TABLE_NAME = '" + tabla + "'";
        }
    }
}
