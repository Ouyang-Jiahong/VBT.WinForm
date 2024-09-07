using System;
using System.Data;
using System.Data.SQLite;

namespace LQWVJE.DAL
{
    public static class SQLiteHelper
    {
        /// <summary>
        /// ConnectionString样例：Data Source=Test.db3;Pooling=true;FailIfMissing=false
        /// </summary>
        public static string ConnectionString = "Data Source=data\\data.db;Pooling=ture;FailIfMissing=false";

        /// <summary>
        /// 参数设置
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="conn"></param>
        /// <param name="sqlStr"></param>
        /// <param name="p"></param>
        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string sqlStr, params SQLiteParameter[] p)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Clear();
                cmd.Connection = conn;
                cmd.CommandText = sqlStr;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 30;
                if (p != null)
                {
                    foreach (SQLiteParameter parm in p)
                    {
                        cmd.Parameters.AddWithValue(parm.ParameterName, parm.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /// <summary>
        /// 查询dataSet
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static DataSet ExecuteQuery(string sqlStr, params SQLiteParameter[] p)
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {

                    DataSet ds = new DataSet();
                    try
                    {
                        PrepareCommand(command, conn, sqlStr, p);
                        SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                        da.Fill(ds);
                        return ds;
                    }
                    catch (Exception ex)
                    {
                        return ds;
                    }
                }
            }
        }
        /// <summary>
        /// 事务处理
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="p"></param>
        public static bool ExecSQL(string sqlStr, params SQLiteParameter[] p)
        {
            bool result = true;
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    PrepareCommand(command, conn, sqlStr, p);
                    SQLiteTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result = false;
                    }
                }
            }
            return result;
        }

        public static int ExecuteNonQuery(string sqlStr, params SQLiteParameter[] p)
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(command, conn, sqlStr, p);
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        return -99;
                    }
                }
            }
        }

        public static object ExecuteScalar(string sqlStr, params SQLiteParameter[] p)
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(command, conn, sqlStr, p);
                        return command.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        return -99;
                    }
                }
            }
        }
    }
}