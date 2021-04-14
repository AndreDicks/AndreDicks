using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Core.Data.DataConnectorInterface
{
    public abstract class BaseDataConnector
    {
        protected string m_strConnection;
        protected DbConnection m_Connection;
        protected bool connectionOpen = false;

        protected DbTransaction dbTrans = null;

        protected char parameterDelimiter = '@';
        protected bool useParameterNames = true;
        protected int transactionStack = 0;

        public char ParameterDelimiter
        {
            get
            {
                return parameterDelimiter;
            }
        }

        public bool BeginTransaction()
        {
            try
            {
                if(dbTrans != null)
                    transactionStack++;
                else
                    dbTrans = m_Connection.BeginTransaction();
                return dbTrans != null;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.BaseDataConnector BeginTransaction");
                throw new Exception("Core.Data.BaseDataConnector BeginTransaction", ex);
            }
        }

        public void EndTransaction(bool commit)
        {
            try
            {
                if(dbTrans == null)
                    return;
                transactionStack--;
                if(transactionStack <= 0)
                {
                    transactionStack = 0;
                    if(commit == true)
                    {
                        dbTrans.Commit();
                    }
                    else
                    {
                        dbTrans.Rollback();
                    }

                    dbTrans = null;
                }
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.BaseDataConnector EndTransaction");
                throw new Exception("Core.Data.BaseDataConnector EndTransaction", ex);
            }
        }

        public abstract void EndTranactionRetaining(bool commit);

        // Implementation for IDataConnector IF
        public bool IsConnected
        {
            get
            {
                return ((this.m_Connection.State == ConnectionState.Open) ? true : false);
            }
        }

        public abstract void AddDBSpecificParameter<T>(DbCommand pCmd, string pParamName, T pValue);

        public abstract DataSet GetDataSet(DbCommand pCmd);

        public bool Connect()
        {
            try
            {
                if(m_Connection.State == ConnectionState.Closed)
                {
                    m_Connection.Open();
                }
                connectionOpen = true;
                return true;
            }
            catch(Exception ex /*e*/)
            {
                this.SetErrorMessage(ex, "Core.Data.BaseDataConnector Connect");
                connectionOpen = false;
                return false;
            }
        }

        public virtual void CloseConnection()
        {
            try
            {
                m_Connection.Close();
                connectionOpen = false;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.BaseDataConnector CloseConnection");
                throw new Exception("Core.Data.BaseDataConnector CloseConnection", ex);
            }
        }

        public bool ValidateRecordExistance(string pTableName, string[] pFieldName, object[] pValueToValidate)
        {
            string _sql = "Select ";

            foreach(string fld in pFieldName)
            {
                _sql = _sql + " " + fld + ", ";
            }
            _sql = _sql.Substring(0, _sql.Length - 2);

            _sql = _sql + " from " + pTableName + " where ";

            foreach(string fld in pFieldName)
            {
                _sql = _sql + fld + " = " + parameterDelimiter + (useParameterNames == true ? fld : "") + " and ";
            }
            _sql = _sql.Substring(0, _sql.Length - 5);

            //using(DbDataReader _DtReader = null)
            //{
            try
            {
                using(DbCommand _dbcmd = m_Connection.CreateCommand())// (sSQLStringLocal, ConStr);
                {
                    _dbcmd.CommandType = CommandType.Text;
                    _dbcmd.CommandText = _sql;
                    if(dbTrans != null)
                        _dbcmd.Transaction = dbTrans;

                    for(int _i = 0; _i < pFieldName.Length; _i++)
                    {
                        //_dbcmd.Parameters.Add(pValueToValidate[_i]);
                        AddDBSpecificParameter(_dbcmd, parameterDelimiter + pFieldName[_i], (object)pValueToValidate[_i]);
                    }

                    Connect();
                    using(DataSet _ds = GetDataSet(_dbcmd))
                    {
                        if((_ds != null) && ((_ds.Tables[0] != null) && (_ds.Tables[0].Rows.Count > 0)))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch(Exception E)
            {
                this.SetErrorMessage(E, "Core.Data.BaseDataConnector ValidateRecordExistance");
                throw E;
            }
            finally
            {
                //CloseConnection();
            }
            //}
        }

        public DataSet RetrieveAllDataForTable(string pTableName, string[] pFilterFieldList, object[] pFilterValues)
        {
            string _sql = "Select * ";

            //foreach (string fld in pFilterFieldList)
            //{
            //    _sql = _sql + " " + fld + ", ";
            //}
            //_sql = _sql.Substring(0, _sql.Length - 2);

            _sql = _sql + " from " + pTableName;

            if(pFilterFieldList != null)
            {
                _sql = _sql + " where ";

                foreach(string fld in pFilterFieldList)
                {
                    _sql = _sql + fld + " = " + parameterDelimiter + (useParameterNames == true ? fld : "") + " and ";
                }
                _sql = _sql.Substring(0, _sql.Length - 5);
            }

            try
            {
                using(DbCommand _dbcmd = m_Connection.CreateCommand())// (sSQLStringLocal, ConStr);
                {
                    _dbcmd.CommandType = CommandType.Text;
                    _dbcmd.CommandText = _sql;
                    if(dbTrans != null)
                        _dbcmd.Transaction = dbTrans;

                    if(pFilterFieldList != null)
                        for(int _i = 0; _i < pFilterFieldList.Length; _i++)
                        {
                            AddDBSpecificParameter(_dbcmd, parameterDelimiter + pFilterFieldList[_i], pFilterValues[_i]);
                        }

                    DataSet _ds = GetDataSet(_dbcmd);

                    if((_ds != null) && (_ds.CreateDataReader().HasRows))
                    {
                        return _ds;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch(Exception E)
            {
                this.SetErrorMessage(E, "Core.Data.BaseDataConnector RetrieveAllDataForTable");
                //CloseConnection();
                //return null;
                throw E;
            }
            finally
            {
                //CloseConnection();
            }
        }

        public bool InsertRecord(string pTableName, string[] pFieldName, object[] pValueToValidate)
        {
            string _sql = "Insert into " + pTableName + " (";

            foreach(string fld in pFieldName)
            {
                _sql = _sql + " " + fld + ", ";
            }
            _sql = _sql.Substring(0, _sql.Length - 2);

            _sql = _sql + ") VALUES ( ";

            foreach(string fld in pFieldName)
            {
                _sql = _sql + " " + parameterDelimiter + (useParameterNames == true ? fld : "") + ", ";
            }
            _sql = _sql.Substring(0, _sql.Length - 2);
            _sql = _sql + " ) ";

            try
            {
                using(DbCommand _dbcmd = m_Connection.CreateCommand())// (sSQLStringLocal, ConStr);
                {
                    _dbcmd.CommandType = CommandType.Text;
                    _dbcmd.CommandText = _sql;
                    if(dbTrans != null)
                        _dbcmd.Transaction = dbTrans;

                    for(int _i = 0; _i < pFieldName.Length; _i++)
                    {
                        //_dbcmd.Parameters.Add(pValueToValidate[_i]);
                        AddDBSpecificParameter(_dbcmd, parameterDelimiter + pFieldName[_i], pValueToValidate[_i] == null ? DBNull.Value : (object)pValueToValidate[_i]);
                    }
                    Connect();
                    int result = _dbcmd.ExecuteNonQuery();

                    return result == 1;
                }
            }
            catch(Exception E)
            {
                this.SetErrorMessage(E, "Core.Data.BaseDataConnector InsertRecord");
                //CloseConnection();
                //return false;
                throw E;
            }
            finally
            {
                //CloseConnection();
            }
        }

        public bool DeleteRecord(string pTableName, string[] pFilterFieldList, object[] pFilterValues)
        {
            string _sql = "DELETE FROM " + pTableName;

            if(pFilterFieldList != null)
            {
                _sql = _sql + " where ";

                foreach(string fld in pFilterFieldList)
                {
                    _sql = _sql + fld + " = " + parameterDelimiter + (useParameterNames == true ? "where" + fld : "") + " and ";
                }
                _sql = _sql.Substring(0, _sql.Length - 5);
            }

            try
            {
                using(DbCommand _dbcmd = m_Connection.CreateCommand())// (sSQLStringLocal, ConStr);
                {
                    _dbcmd.CommandType = CommandType.Text;
                    _dbcmd.CommandText = _sql;
                    if(dbTrans != null)
                        _dbcmd.Transaction = dbTrans;

                    if(pFilterFieldList != null)
                        for(int _i = 0; _i < pFilterFieldList.Length; _i++)
                        {
                            AddDBSpecificParameter(_dbcmd, parameterDelimiter + "where" + pFilterFieldList[_i], pFilterValues[_i]);
                        }
                    Connect();
                    int result = _dbcmd.ExecuteNonQuery();

                    return result >= 1;
                }
            }
            catch(Exception E)
            {
                this.SetErrorMessage(E, "Core.Data.BaseDataConnector DeleteRecord");
                //CloseConnection();
                //return false;
                throw E;
            }
            finally
            {
                //CloseConnection();
            }
        }

        public bool UpdateRecord(string pTableName, string[] pFieldName, object[] pValues,
                                string[] pFilterFieldList, object[] pFilterValues)
        {
            string _sql = "Update " + pTableName + " set ";

            foreach(string fld in pFieldName)
            {
                _sql = _sql + " " + fld + " = " + parameterDelimiter + (useParameterNames == true ? "set" + fld : "") + ", ";
            }
            _sql = _sql.Substring(0, _sql.Length - 2);

            if(pFilterFieldList != null)
            {
                _sql = _sql + " where ";

                foreach(string fld in pFilterFieldList)
                {
                    _sql = _sql + fld + " = " + parameterDelimiter + (useParameterNames == true ? "where" + fld : "") + " and ";
                }
                _sql = _sql.Substring(0, _sql.Length - 5);
            }

            try
            {
                using(DbCommand _dbcmd = m_Connection.CreateCommand())// (sSQLStringLocal, ConStr);
                {
                    _dbcmd.CommandType = CommandType.Text;
                    _dbcmd.CommandText = _sql;
                    if(dbTrans != null)
                        _dbcmd.Transaction = dbTrans;

                    if(pFilterFieldList != null)
                        for(int _i = 0; _i < pFilterFieldList.Length; _i++)
                        {
                            AddDBSpecificParameter(_dbcmd, parameterDelimiter + "where" + pFilterFieldList[_i], pFilterValues[_i]);
                        }

                    for(int _i = 0; _i < pFieldName.Length; _i++)
                    {
                        AddDBSpecificParameter(_dbcmd, parameterDelimiter + "set" + pFieldName[_i], pValues[_i] == null ? DBNull.Value : (object)pValues[_i]);
                    }
                    Connect();
                    int result = _dbcmd.ExecuteNonQuery();

                    return result >= 1;
                }
            }
            catch(Exception E)
            {
                this.SetErrorMessage(E, "Core.Data.BaseDataConnector UpdateRecord");
                //CloseConnection();
                //return false;
                throw E;
            }
            finally
            {
                //CloseConnection();
            }
        }
        public bool UpdateRecord(string pTableName, string[] pFieldName, object[] pValues,
                                string[] pFilterFieldList, object[] pFilterValues, string[] pColTypes, string strDBtype)
        {
            string _sql = "Update " + pTableName + " set ";

            foreach (string fld in pFieldName)
            {
                _sql = _sql + " " + fld + " = " + parameterDelimiter + (useParameterNames == true ? "set" + fld : "") + ", ";
            }
            _sql = _sql.Substring(0, _sql.Length - 2);

            if (pFilterFieldList != null)
            {
                _sql = _sql + " where ";

                foreach (string fld in pFilterFieldList)
                {
                    _sql = _sql + fld + " = " + parameterDelimiter + (useParameterNames == true ? "where" + fld : "") + " and ";
                }
                _sql = _sql.Substring(0, _sql.Length - 5);
            }

            try
            {
                using (DbCommand _dbcmd = m_Connection.CreateCommand())// (sSQLStringLocal, ConStr);
                {
                    _dbcmd.CommandType = CommandType.Text;
                    _dbcmd.CommandText = _sql;
                    if (dbTrans != null)
                        _dbcmd.Transaction = dbTrans;

                    if (pFilterFieldList != null)
                        for (int _i = 0; _i < pFilterFieldList.Length; _i++)
                        {
                            AddDBSpecificParameter(_dbcmd, parameterDelimiter + "where" + pFilterFieldList[_i], pFilterValues[_i]);
                        }

                    for (int _i = 0; _i < pFieldName.Length; _i++)
                    {
                        if (pColTypes[_i] == "Byte[]" && strDBtype == "MSSQL")
                        {
                            AddDBSpecificParameter(_dbcmd, parameterDelimiter + "set" + pFieldName[_i], pValues[_i] == DBNull.Value ? System.Data.SqlTypes.SqlBinary.Null : (object)pValues[_i]);
                        }
                        else
                        {
                            AddDBSpecificParameter(_dbcmd, parameterDelimiter + "set" + pFieldName[_i], pValues[_i] == DBNull.Value ? DBNull.Value : (object)pValues[_i]);
                        }
                    }
                    Connect();
                    int result = _dbcmd.ExecuteNonQuery();

                    return result >= 1;
                }
            }
            catch (Exception E)
            {
                this.SetErrorMessage(E, "Core.Data.BaseDataConnector UpdateRecord");
                //CloseConnection();
                //return false;
                throw E;
            }
            finally
            {
                //CloseConnection();
            }
        }
        private void SetErrorMessage(Exception ex, string methodName)
        {
        }
    }
}