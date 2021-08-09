using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class ConnectionString
    {
        public static readonly string WritingBoard = "Database='WritingBoard';Data Source='sh-cdb-am03pru4.sql.tencentcdb.com'; Port = 59791; User Id='root';Password='jinao123!@#';charset='utf8';pooling=true;Allow User Variables=True";

        public static readonly string WritingBoardInNet = "Database='WritingBoard';Data Source='172.17.0.15:3306'; User Id='root';Password='jinao123!@#';charset='utf8';pooling=true;Allow User Variables=True";
    }
}