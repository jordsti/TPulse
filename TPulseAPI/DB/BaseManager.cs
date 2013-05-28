using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using TPulseAPI.Extensions;

namespace TPulseAPI.DB
{
    public abstract class BaseManager
    {
        protected IDbConnection Database;
        public String TableName { get; protected set; }

        protected BaseManager(IDbConnection database, string tableName)
        {
            Database = database;
            TableName = tableName;
        }

        protected IQueryBuilder QueryBuilder
        {
            get
            {
                if (Database.GetSqlType() == SqlType.Sqlite)
                    return new SqliteQueryCreator();
                else
                    return new MysqlQueryCreator();
            }
        }


    }
}
