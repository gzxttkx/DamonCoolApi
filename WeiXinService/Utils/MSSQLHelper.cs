using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Text;
using System.Reflection;

namespace WeiXinService.Utils
{
    /// <summary>
    /// MSSQL数据库操作类
    /// </summary>
    public class MSSQLHelper
    {
        #region 变量
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        #endregion

        #region 事务的SqlConnection
        #region 数据库连接对象
        /// <summary>
        /// 获取打开的数据库连接对象
        /// </summary>
        public static SqlConnection GetOpenConnection()
        {
            SqlConnection connection = null;

            string key = "Simpo_FQD_OracleConnection";

            if (HttpContext.Current.Items[key] == null)
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                HttpContext.Current.Items[key] = connection;
            }
            else
            {
                connection = (SqlConnection)HttpContext.Current.Items[key];
            }

            return connection;
        }
        #endregion

        #region 事务对象
        /// <summary>
        /// 获取事务对象
        /// </summary>
        public static SqlTransaction GetTran()
        {
            SqlTransaction tran = null;

            string key = "Simpo_FQD_OracleTransaction";

            if (HttpContext.Current.Items[key] == null)
            {
                tran = GetOpenConnection().BeginTransaction();
                HttpContext.Current.Items[key] = tran;
            }
            else
            {
                tran = (SqlTransaction)HttpContext.Current.Items[key];
            }

            return tran;
        }
        #endregion

        #region 开起事务标志
        /// <summary>
        /// 事务标志
        /// </summary>
        private static string tranFlagKey = "Simpo_FQD_OracleTransaction_Flag";
        /// <summary>
        /// 添加事务标志
        /// </summary>
        public static void AddTranFlag()
        {
            HttpContext.Current.Items[tranFlagKey] = true;
        }
        /// <summary>
        /// 移除事务标志
        /// </summary>
        public static void RemoveTranFlag()
        {
            HttpContext.Current.Items[tranFlagKey] = false;
        }
        /// <summary>
        /// 事务标志
        /// </summary>
        public static bool TranFlag
        {
            get
            {
                bool tranFlag = false;

                if (HttpContext.Current.Items[tranFlagKey] != null)
                {
                    tranFlag = (bool)HttpContext.Current.Items[tranFlagKey];
                }

                return tranFlag;
            }
        }
        #endregion
        #endregion

        #region 基础方法
        #region 公用方法
        #region GetMaxID
        /// <summary>
        /// 不支持多用户并发，慎用，请使用GetNextID方法
        /// </summary>
        private static int GetMaxID(string fieldName, string tableName)
        {
            string strsql = "select max(" + fieldName + ")+1 from " + tableName;
            object obj = MSSQLHelper.GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }
        #endregion

        #region Exists
        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            object obj = MSSQLHelper.GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
        #endregion

        #region  执行简单SQL语句
        #region Exists
        public static bool Exists(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }
        #endregion

        #region 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            SqlConnection connection = GetOpenConnection();
            using (SqlCommand cmd = new SqlCommand(SQLString, connection))
            {
                try
                {
                    if (TranFlag) cmd.Transaction = GetTran();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    connection.Close();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    if (!TranFlag) connection.Close();
                }
            }
        }
        #endregion

        #region 执行多条SQL语句，实现数据库事务
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>  
        public static bool ExecuteSqlTran(ArrayList SQLStringList)
        {
            bool re = false;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                SqlTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    re = true;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    re = false;
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return re;

        }
        #endregion

        #region 执行带一个存储过程参数的的SQL语句
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@content", SqlDbType.NVarChar);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        #endregion

        #region 向数据库里插入图像格式的字段
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(strSQL, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        #endregion

        #region 执行一条计算查询结果语句，返回查询结果
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }
        #endregion

        #region 执行查询语句，返回SqlDataReader
        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string strSQL)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }


        }
        #endregion

        #region 执行查询语句，返回DataSet
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }
        #endregion
        #endregion

        #region 执行带参数的SQL语句
        #region 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = GetOpenConnection();
            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    if (TranFlag) cmd.Transaction = GetTran();
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    if (!TranFlag) connection.Close();
                }
            }
        }
        #endregion

        #region 执行多条SQL语句，实现数据库事务
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region 执行一条计算查询结果语句，返回查询结果
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }
        #endregion

        #region 执行查询语句，返回SqlDataReader
        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }
        #endregion

        #region 执行查询语句，返回DataSet
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                    return ds;
                }
            }
        }
        #endregion

        #region PrepareCommand
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        #endregion
        #endregion

        #region 存储过程操作
        #region 执行存储过程 返回SqlDataReader
        /// <summary>
        /// 执行存储过程 返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader RunProcedureReader(string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataReader returnReader;
            connection.Open();
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return returnReader;
        }
        #endregion

        #region 执行存储过程，返回影响的行数
        /// <summary>
        /// 执行存储过程，返回影响的行数  
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                //Connection.Close();
                return result;
            }
        }
        #endregion

        #region 执行存储过程，什么值也不返回
        /// <summary>
        /// 执行存储过程，什么值也不返回 
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        public static void RunProcedure(string storedProcName, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                System.Data.SqlClient.SqlCommand cmd = BuildQueryCommand(connection, storedProcName, parameters);

                try
                {
                    if (connection.State != ConnectionState.Open)
                    {

                        connection.Open();
                    }

                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                    throw new Exception(ex.Message);
                    // Console.WriteLine(ex.Message.ToString());
                }
                finally
                {
                    connection.Close();

                }
            }
        }
        #endregion

        #region 执行存储过程,返回数据集
        /// <summary>
        /// 执行存储过程,返回数据集
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedureGetDataSet(string storedProcName, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, "dt");
                connection.Close();
                return dataSet;
            }
        }
        #endregion

        #region 构建 SqlCommand 对象
        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (SqlParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }
        #endregion

        #region 执行存储过程，返回影响的行数
        /// <summary>
        /// 执行存储过程，返回影响的行数  
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public static int RunProcedure_rowsAffected(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                //Connection.Close();
                return result;
            }
        }
        #endregion

        #region 创建 SqlCommand 对象实例
        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值) 
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                         SqlDbType.Int, 4, ParameterDirection.ReturnValue,
             false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        #endregion
        #endregion
        #endregion

        #region 扩展方法
        #region 执行返回一行一列的数据库操作
        /// <summary>
        /// 执行返回一行一列的数据库操作
        /// </summary>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <param name="param">Oracle命令参数数组</param>
        /// <returns>第一行第一列的记录</returns>
        public static int ExecuteScalar(string commandText, CommandType commandType, params SqlParameter[] param)
        {
            int count = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = commandType;
                        cmd.Parameters.AddRange(param);
                        connection.Open();
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        count = 0;
                    }
                }
            }
            return count;
        }
        #endregion

        #region 执行不查询的数据库操作
        /// <summary>
        /// 执行不查询的数据库操作
        /// </summary>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <param name="param">Oracle命令参数数组</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string commandText, CommandType commandType, params SqlParameter[] param)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = commandType;
                        cmd.Parameters.AddRange(param);
                        connection.Open();
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                    }
                }
            }
            return result;
        }
        #endregion

        #region 执行返回一条记录的泛型对象
        /// <summary>
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="reader">只进只读对象</param>
        /// <returns>泛型对象</returns>
        private static T ExecuteDataReader<T>(IDataReader reader)
        {
            T obj = default(T);
            try
            {
                Type type = typeof(T);
                obj = (T)Activator.CreateInstance(type);//从当前程序集里面通过反射的方式创建指定类型的对象   
                //obj = (T)Assembly.Load(MSSQLHelper._assemblyName).CreateInstance(MSSQLHelper._assemblyName + "." + type.Name);//从另一个程序集里面通过反射的方式创建指定类型的对象 
                PropertyInfo[] propertyInfos = type.GetProperties();//获取指定类型里面的所有属性
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string fieldName = reader.GetName(i);
                        if (fieldName.ToLower() == propertyInfo.Name.ToLower())
                        {
                            object val = reader[propertyInfo.Name];//读取表中某一条记录里面的某一列信息
                            if (val != null && val != DBNull.Value)
                                propertyInfo.SetValue(obj, val, null);//给对象的某一个属性赋值
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return obj;
        }
        #endregion

        #region 执行返回一条记录的泛型对象
        /// <summary>
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <param name="param">Oracle命令参数数组</param>
        /// <returns>实体对象</returns>
        public static T ExecuteEntity<T>(string commandText, CommandType commandType, params SqlParameter[] param)
        {
            T obj = default(T);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(param);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        obj = MSSQLHelper.ExecuteDataReader<T>(reader);
                    }
                }
            }
            return obj;
        }
        #endregion

        #region 执行返回多条记录的泛型集合对象
        /// <summary>
        /// 执行返回多条记录的泛型集合对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <param name="param">Oracle命令参数数组</param>
        /// <returns>泛型集合对象</returns>
        public static List<T> ExecuteList<T>(string commandText, CommandType commandType, params SqlParameter[] param)
        {
            List<T> list = new List<T>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = commandType;
                        cmd.Parameters.AddRange(param);
                        connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        while (reader.Read())
                        {
                            T obj = MSSQLHelper.ExecuteDataReader<T>(reader);
                            list.Add(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        list = null;
                    }
                }
            }
            return list;
        }
        #endregion
        #endregion

        #region 增删改查
        #region 根据sequence名称获取下一个ID
        /// <summary>
        /// 根据sequence名称获取下一个ID
        /// </summary>
        public static int GetNextID(string sequenceName)
        {
            string sql = string.Format("select {0}.Nextval from dual", sequenceName);
            DataTable dt = Query(sql).Tables[0];
            return int.Parse(dt.Rows[0][0].ToString());
        }
        #endregion

        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        public static void Insert(object obj)
        {
            StringBuilder strSql = new StringBuilder();
            Type type = obj.GetType();
            strSql.Append(string.Format("insert into {0}(", type.Name));

            PropertyInfo[] propertyInfoList = type.GetProperties();
            List<string> propertyNameList = new List<string>();
            int savedCount = 0;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                if (propertyInfo.GetCustomAttributes(typeof(NotSaveAttribute), false).Length == 0)
                {
                    if (propertyInfo.GetCustomAttributes(typeof(IsEntityAttribute), false).Length == 0)
                    {
                        object val = propertyInfo.GetValue(obj, null);
                        if (val != null)
                        {
                            propertyNameList.Add(propertyInfo.Name);
                            savedCount++;
                        }
                    }
                    else
                    {
                        object val = propertyInfo.GetValue(obj, null);
                        if (val != null)
                        {
                            propertyNameList.Add(propertyInfo.Name + "Id");
                            savedCount++;
                        }
                    }
                }
            }

            strSql.Append(string.Format("{0})", string.Join(",", propertyNameList.ToArray())));
            strSql.Append(string.Format(" values ({0})", string.Join(",", propertyNameList.ConvertAll<string>(a => ":" + a).ToArray())));
            SqlParameter[] parameters = new SqlParameter[savedCount];
            int i = 0;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                if (propertyInfo.GetCustomAttributes(typeof(NotSaveAttribute), false).Length == 0)
                {
                    if (propertyInfo.GetCustomAttributes(typeof(IsEntityAttribute), false).Length == 0)
                    {
                        object val = propertyInfo.GetValue(obj, null);
                        if (val != null)
                        {
                            SqlParameter oracleParameter = new SqlParameter(":" + propertyInfo.Name, propertyInfo.GetValue(obj, null));
                            parameters[i++] = oracleParameter;
                        }
                    }
                    else
                    {
                        object val = propertyInfo.GetValue(obj, null);
                        if (val != null)
                        {
                            object valProVal = val.GetType().GetProperty("Id").GetValue(val, null);
                            SqlParameter oracleParameter = new SqlParameter(":" + propertyInfo.Name + "Id", valProVal);
                            parameters[i++] = oracleParameter;
                        }
                    }
                }
            }

            ExecuteSql(strSql.ToString(), parameters);
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        public static void Update(object obj)
        {
            object oldObj = Find(obj);
            if (oldObj == null) throw new Exception("无法获取到旧数据");

            StringBuilder strSql = new StringBuilder();
            Type type = obj.GetType();
            strSql.Append(string.Format("update {0} ", type.Name));

            PropertyInfo[] propertyInfoList = type.GetProperties();
            List<string> propertyNameList = new List<string>();
            int savedCount = 0;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                if (propertyInfo.GetCustomAttributes(typeof(NotSaveAttribute), false).Length == 0)
                {
                    if (propertyInfo.GetCustomAttributes(typeof(IsEntityAttribute), false).Length == 0)
                    {
                        object oldVal = propertyInfo.GetValue(oldObj, null);
                        object val = propertyInfo.GetValue(obj, null);
                        if (!object.Equals(oldVal, val))
                        {
                            propertyNameList.Add(propertyInfo.Name);
                            savedCount++;
                        }
                    }
                    else
                    {
                        object val = propertyInfo.GetValue(obj, null);
                        object oldVal = propertyInfo.GetValue(oldObj, null);
                        object oldValProVal = oldVal == null ? null : GetIdVal(oldVal);
                        object valProVal = val == null ? null : GetIdVal(val);
                        if (!object.Equals(oldValProVal, valProVal))
                        {
                            propertyNameList.Add(propertyInfo.Name);
                            savedCount++;
                        }
                    }
                }
            }

            strSql.Append(string.Format(" set "));
            SqlParameter[] parameters = new SqlParameter[savedCount];
            int i = 0;
            StringBuilder sbPros = new StringBuilder();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                if (propertyInfo.GetCustomAttributes(typeof(NotSaveAttribute), false).Length == 0)
                {
                    if (propertyInfo.GetCustomAttributes(typeof(IsEntityAttribute), false).Length == 0)
                    {
                        object oldVal = propertyInfo.GetValue(oldObj, null);
                        object val = propertyInfo.GetValue(obj, null);
                        if (!object.Equals(oldVal, val))
                        {
                            sbPros.Append(string.Format(" {0}=:{0},", propertyInfo.Name));
                            SqlParameter oracleParameter = new SqlParameter(":" + propertyInfo.Name, val == null ? DBNull.Value : val);
                            parameters[i++] = oracleParameter;
                        }
                    }
                    else
                    {
                        object oldVal = propertyInfo.GetValue(oldObj, null);
                        object val = propertyInfo.GetValue(obj, null);
                        object oldValProVal = oldVal == null ? null : GetIdVal(oldVal);
                        object valProVal = val == null ? null : GetIdVal(val);
                        if (!object.Equals(oldValProVal, valProVal))
                        {
                            sbPros.Append(string.Format(" {0}=:{0},", propertyInfo.Name + "Id"));

                            SqlParameter oracleParameter = new SqlParameter(":" + propertyInfo.Name + "Id", valProVal == null ? DBNull.Value : valProVal);
                            parameters[i++] = oracleParameter;
                        }
                    }
                }

                if (propertyInfo.GetCustomAttributes(typeof(IsEntityAttribute), false).Length > 0)
                {
                    object val = propertyInfo.GetValue(obj, null);
                    if (val != null && Find(val) != null)
                    {
                        Update(val);
                    }
                }
            }
            if (sbPros.Length > 0)
            {
                strSql.Append(sbPros.ToString(0, sbPros.Length - 1));
            }
            strSql.Append(string.Format(" where {0}={1}", GetIdName(obj.GetType()), int.Parse(GetIdVal(obj).ToString())));

            if (savedCount > 0)
            {
                ExecuteSql(strSql.ToString(), parameters);
            }
        }
        #endregion

        #region 删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public static void Delete<T>(int id)
        {
            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(string.Format("delete from {0} where {2}={1}", type.Name, id, GetIdName(type)));

            ExecuteSql(sbSql.ToString());
        }
        /// <summary>
        /// 根据Id集合删除
        /// </summary>
        public static void BatchDelete<T>(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return;

            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(string.Format("delete from {0} where {2} in ({1})", type.Name, ids, GetIdName(type)));

            ExecuteSql(sbSql.ToString());
        }
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public static void Delete<T>(string conditions)
        {
            if (string.IsNullOrWhiteSpace(conditions)) return;

            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(string.Format("delete from {0} where {1}", type.Name, conditions));

            ExecuteSql(sbSql.ToString());
        }
        #endregion

        #region 获取实体
        #region 根据实体获取实体
        /// <summary>
        /// 根据实体获取实体
        /// </summary>
        private static object Find(object obj)
        {
            Type type = obj.GetType();

            object result = Activator.CreateInstance(type);
            bool hasValue = false;
            IDataReader rd = null;

            string sql = string.Format("select * from {0} where {2}={1}", type.Name, GetIdVal(obj), GetIdName(obj.GetType()));

            try
            {
                rd = ExecuteReader(sql);

                PropertyInfo[] propertyInfoList = type.GetProperties();

                int fcnt = rd.FieldCount;
                List<string> fileds = new List<string>();
                for (int i = 0; i < fcnt; i++)
                {
                    fileds.Add(rd.GetName(i).ToUpper());
                }

                while (rd.Read())
                {
                    hasValue = true;
                    IDataRecord record = rd;

                    foreach (PropertyInfo pro in propertyInfoList)
                    {
                        if (pro.PropertyType.IsClass)
                        {
                            object[] objArray = pro.GetCustomAttributes(typeof(IsEntityAttribute), false);
                            if (objArray.Length > 0)
                            {
                                if (record[pro.Name + "Id"].GetType() == typeof(int))
                                {
                                    pro.SetValue(result, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, (int)record[pro.Name + "Id"]), null);
                                }
                                if (record[pro.Name + "Id"].GetType() == typeof(decimal))
                                {
                                    pro.SetValue(result, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, int.Parse(((decimal)record[pro.Name + "Id"]).ToString())), null);
                                }
                                continue;
                            }
                        }

                        if (!fileds.Contains(pro.Name.ToUpper()) || record[pro.Name] == DBNull.Value)
                        {
                            continue;
                        }

                        pro.SetValue(result, record[pro.Name] == DBNull.Value ? null : getReaderValue(record[pro.Name], pro.PropertyType), null);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (rd != null && !rd.IsClosed)
                {
                    rd.Close();
                    rd.Dispose();
                }
            }

            if (hasValue)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 根据Id获取实体
        /// <summary>
        /// 根据Id获取实体
        /// </summary>
        private static object FindById(Type type, int id)
        {
            object result = Activator.CreateInstance(type);
            IDataReader rd = null;
            bool hasValue = false;

            string sql = string.Format("select * from {0} where {2}={1}", type.Name, id, GetIdName(type));

            try
            {
                rd = ExecuteReader(sql);

                PropertyInfo[] propertyInfoList = type.GetProperties();

                int fcnt = rd.FieldCount;
                List<string> fileds = new List<string>();
                for (int i = 0; i < fcnt; i++)
                {
                    fileds.Add(rd.GetName(i).ToUpper());
                }

                while (rd.Read())
                {
                    hasValue = true;
                    IDataRecord record = rd;

                    foreach (PropertyInfo pro in propertyInfoList)
                    {
                        if (pro.PropertyType.IsClass)
                        {
                            object[] objArray = pro.GetCustomAttributes(typeof(IsEntityAttribute), false);
                            if (objArray.Length > 0)
                            {
                                if (record[pro.Name + "Id"].GetType() == typeof(int))
                                {
                                    pro.SetValue(result, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, (int)record[pro.Name + "Id"]), null);
                                }
                                if (record[pro.Name + "Id"].GetType() == typeof(decimal))
                                {
                                    pro.SetValue(result, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, int.Parse(((decimal)record[pro.Name + "Id"]).ToString())), null);
                                }
                                continue;
                            }
                        }

                        if (!fileds.Contains(pro.Name.ToUpper()) || record[pro.Name] == DBNull.Value)
                        {
                            continue;
                        }

                        pro.SetValue(result, record[pro.Name] == DBNull.Value ? null : getReaderValue(record[pro.Name], pro.PropertyType), null);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (rd != null && !rd.IsClosed)
                {
                    rd.Close();
                    rd.Dispose();
                }
            }

            if (hasValue)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 根据Id获取实体
        /// <summary>
        /// 根据Id获取实体
        /// </summary>
        public static T FindById<T>(int id) where T : new()
        {
            Type type = typeof(T);
            T result = (T)Activator.CreateInstance(type);
            IDataReader rd = null;
            bool hasValue = false;

            string sql = string.Format("select * from {0} where {2}={1}", type.Name, id, GetIdName(type));

            try
            {
                rd = ExecuteReader(sql);

                PropertyInfo[] propertyInfoList = type.GetProperties();

                int fcnt = rd.FieldCount;
                List<string> fileds = new List<string>();
                for (int i = 0; i < fcnt; i++)
                {
                    fileds.Add(rd.GetName(i).ToUpper());
                }

                while (rd.Read())
                {
                    hasValue = true;
                    IDataRecord record = rd;

                    foreach (PropertyInfo pro in propertyInfoList)
                    {
                        if (pro.PropertyType.IsClass)
                        {
                            object[] objArray = pro.GetCustomAttributes(typeof(IsEntityAttribute), false);
                            if (objArray.Length > 0)
                            {
                                if (record[pro.Name + "Id"].GetType() == typeof(int))
                                {
                                    pro.SetValue(result, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, (int)record[pro.Name + "Id"]), null);
                                }
                                if (record[pro.Name + "Id"].GetType() == typeof(decimal))
                                {
                                    pro.SetValue(result, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, int.Parse(((decimal)record[pro.Name + "Id"]).ToString())), null);
                                }
                                continue;
                            }
                        }

                        if (!fileds.Contains(pro.Name.ToUpper()) || record[pro.Name] == DBNull.Value)
                        {
                            continue;
                        }

                        pro.SetValue(result, record[pro.Name] == DBNull.Value ? null : getReaderValue(record[pro.Name], pro.PropertyType), null);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (rd != null && !rd.IsClosed)
                {
                    rd.Close();
                    rd.Dispose();
                }
            }

            if (hasValue)
            {
                return result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion
        #endregion

        #region 获取列表
        /// <summary>
        /// 获取列表
        /// </summary>
        public static List<T> FindListBySql<T>(string sql) where T : new()
        {
            List<T> list = new List<T>();
            object obj;
            IDataReader rd = null;

            try
            {
                rd = ExecuteReader(sql);

                if (typeof(T) == typeof(int))
                {
                    while (rd.Read())
                    {
                        list.Add((T)rd[0]);
                    }
                }
                else if (typeof(T) == typeof(string))
                {
                    while (rd.Read())
                    {
                        list.Add((T)rd[0]);
                    }
                }
                else
                {
                    PropertyInfo[] propertyInfoList = (typeof(T)).GetProperties();

                    int fcnt = rd.FieldCount;
                    List<string> fileds = new List<string>();
                    for (int i = 0; i < fcnt; i++)
                    {
                        fileds.Add(rd.GetName(i).ToUpper());
                    }

                    while (rd.Read())
                    {
                        IDataRecord record = rd;
                        obj = new T();


                        foreach (PropertyInfo pro in propertyInfoList)
                        {
                            if (pro.PropertyType.IsClass)
                            {
                                if (pro.GetCustomAttributes(typeof(IsEntityAttribute), false).Length > 0)
                                {
                                    if (record[pro.Name + "Id"].GetType() == typeof(int))
                                    {
                                        pro.SetValue(obj, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, (int)record[pro.Name + "Id"]), null);
                                    }
                                    if (record[pro.Name + "Id"].GetType() == typeof(decimal))
                                    {
                                        pro.SetValue(obj, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, int.Parse(((decimal)record[pro.Name + "Id"]).ToString())), null);
                                    }
                                    continue;
                                }
                            }

                            if (!fileds.Contains(pro.Name.ToUpper()) || record[pro.Name] == DBNull.Value)
                            {
                                continue;
                            }

                            pro.SetValue(obj, record[pro.Name] == DBNull.Value ? null : getReaderValue(record[pro.Name], pro.PropertyType), null);
                        }
                        list.Add((T)obj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (rd != null && !rd.IsClosed)
                {
                    rd.Close();
                    rd.Dispose();
                }
            }

            return list;
        }
        #endregion

        #region 获取列表
        /// <summary>
        /// 获取列表
        /// </summary>
        public static List<T> FindListBySql<T>(string sql, params SqlParameter[] cmdParms) where T : new()
        {
            List<T> list = new List<T>();
            object obj;
            IDataReader rd = null;

            try
            {
                rd = ExecuteReader(sql, cmdParms);

                if (typeof(T) == typeof(int))
                {
                    while (rd.Read())
                    {
                        list.Add((T)rd[0]);
                    }
                }
                else if (typeof(T) == typeof(string))
                {
                    while (rd.Read())
                    {
                        list.Add((T)rd[0]);
                    }
                }
                else
                {
                    PropertyInfo[] propertyInfoList = (typeof(T)).GetProperties();

                    int fcnt = rd.FieldCount;
                    List<string> fileds = new List<string>();
                    for (int i = 0; i < fcnt; i++)
                    {
                        fileds.Add(rd.GetName(i).ToUpper());
                    }

                    while (rd.Read())
                    {
                        IDataRecord record = rd;
                        obj = new T();


                        foreach (PropertyInfo pro in propertyInfoList)
                        {
                            if (pro.PropertyType.IsClass)
                            {
                                if (pro.GetCustomAttributes(typeof(IsEntityAttribute), false).Length > 0)
                                {
                                    if (record[pro.Name + "Id"].GetType() == typeof(int))
                                    {
                                        pro.SetValue(obj, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, (int)record[pro.Name + "Id"]), null);
                                    }
                                    if (record[pro.Name + "Id"].GetType() == typeof(decimal))
                                    {
                                        pro.SetValue(obj, record[pro.Name + "Id"] == DBNull.Value ? null : FindById(pro.PropertyType, int.Parse(((decimal)record[pro.Name + "Id"]).ToString())), null);
                                    }
                                    continue;
                                }
                            }

                            if (!fileds.Contains(pro.Name.ToUpper()) || record[pro.Name] == DBNull.Value)
                            {
                                continue;
                            }

                            pro.SetValue(obj, record[pro.Name] == DBNull.Value ? null : getReaderValue(record[pro.Name], pro.PropertyType), null);
                        }
                        list.Add((T)obj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (rd != null && !rd.IsClosed)
                {
                    rd.Close();
                    rd.Dispose();
                }
            }

            return list;
        }
        #endregion

        #region 分页获取列表
        /// <summary>
        /// 分页(任意entity，尽量少的字段)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static PageViewModel FindPageBySql<T>(string sql, string orderby, int pageSize, int currentPage) where T : new()
        {
            PageViewModel pageViewModel = new PageViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string commandText = string.Format("select count(*) from ({0}) T", sql);
                IDbCommand cmd = new SqlCommand(commandText, connection);
                pageViewModel.total = int.Parse(cmd.ExecuteScalar().ToString());

                int startRow = pageSize * (currentPage - 1) + 1;
                int endRow = startRow + pageSize;

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"
                    select * from 
                    (select ROW_NUMBER() over({1}) as rowNumber, t.* from ({0}) t) tempTable
                    where rowNumber between {2} and {3} ", sql, orderby, startRow, endRow));

                List<T> list = FindListBySql<T>(sb.ToString());
                pageViewModel.rows = list;
            }

            return pageViewModel;
        }
        #endregion

        #region 分页获取列表
        /// <summary>
        /// 分页(任意entity，尽量少的字段)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static PageViewModel FindPageBySql<T>(string sql, string orderby, int pageSize, int currentPage, params SqlParameter[] cmdParms) where T : new()
        {
            PageViewModel pageViewModel = new PageViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string commandText = string.Format("select count(*) from ({0}) T", sql);
                SqlCommand cmd = new SqlCommand(commandText, connection);
                PrepareCommand(cmd, connection, null, commandText, cmdParms);
                pageViewModel.total = int.Parse(cmd.ExecuteScalar().ToString());
                cmd.Parameters.Clear();

                int startRow = pageSize * (currentPage - 1) + 1;
                int endRow = startRow + pageSize;

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"
                    select * from 
                    (select ROW_NUMBER() over({1}) as rowNumber, t.* from ({0}) t) tempTable
                    where rowNumber between {2} and {3} ", sql, orderby, startRow, endRow));

                List<T> list = FindListBySql<T>(sb.ToString(), cmdParms);
                pageViewModel.rows = list;
            }

            return pageViewModel;
        }


        #endregion

        #region 分页获取列表
        /// <summary>
        /// 分页(任意entity，尽量少的字段)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataSet FindPageBySql(string sql, string orderby, int pageSize, int currentPage, out int resultCount, params SqlParameter[] cmdParms)
        {
            DataSet ds = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string commandText = string.Format("select count(*) from ({0}) T", sql);
                IDbCommand cmd = new SqlCommand(commandText, connection);
                resultCount = int.Parse(cmd.ExecuteScalar().ToString());

                int startRow = pageSize * (currentPage - 1) + 1;
                int endRow = startRow + pageSize;

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"
                    select * from 
                    (select ROW_NUMBER() over({1}) as rowNumber, t.* from ({0}) t) tempTable
                    where rowNumber between {2} and {3} ", sql, orderby, startRow, endRow));

                ds = Query(sql, cmdParms);
            }

            return ds;
        }
        #endregion

        #region getReaderValue 转换数据
        /// <summary>
        /// 转换数据
        /// </summary>
        private static Object getReaderValue(Object rdValue, Type ptype)
        {

            if (ptype == typeof(decimal))
                return Convert.ToDecimal(rdValue);

            if (ptype == typeof(int))
                return Convert.ToInt32(rdValue);

            if (ptype == typeof(long))
                return Convert.ToInt64(rdValue);

            return rdValue;
        }
        #endregion

        #region 获取主键名称
        /// <summary>
        /// 获取主键名称
        /// </summary>
        public static string GetIdName(Type type)
        {
            PropertyInfo[] propertyInfoList = type.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                if (propertyInfo.GetCustomAttributes(typeof(IsIdAttribute), false).Length > 0)
                {
                    return propertyInfo.Name;
                }
            }
            return "Id";
        }
        #endregion

        #region 获取主键值
        /// <summary>
        /// 获取主键名称
        /// </summary>
        public static object GetIdVal(object val)
        {
            string idName = GetIdName(val.GetType());
            if (!string.IsNullOrWhiteSpace(idName))
            {
                return val.GetType().GetProperty(idName).GetValue(val, null);
            }
            return 0;
        }
        #endregion
        #endregion

        #region 事务
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        public static void BeginTransaction()
        {
            GetTran();
            AddTranFlag();
        }
        #endregion

        #region 结束事务(正常结束)
        /// <summary>
        /// 结束事务(正常结束)
        /// </summary>
        public static void EndTransaction()
        {
            try
            {
                GetTran().Commit();
                RemoveTranFlag();
            }
            catch (Exception ex)
            {
                GetTran().Rollback();
                RemoveTranFlag();
            }
            finally
            {
                GetOpenConnection().Close();
            }
        }
        #endregion

        #region 回滚事务(出错时调用该方法回滚)
        /// <summary>
        /// 回滚事务(出错时调用该方法回滚)
        /// </summary>
        public static void RollbackTransaction()
        {
            GetTran().Rollback();
            RemoveTranFlag();
            GetOpenConnection().Close();
        }
        #endregion
        #endregion

    }
}