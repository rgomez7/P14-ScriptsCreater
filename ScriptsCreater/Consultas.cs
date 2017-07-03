using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsCreater
{
    class Consultas
    {
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
            return "SELECT 1 as Activo FROM sys.change_tracking_tables tt " +
                        "INNER JOIN sys.objects obj ON obj.object_id = tt.object_id " +
                        "WHERE obj.name = '" + tabla + "'";
        }
    }
}
