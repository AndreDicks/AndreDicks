using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Core.Data.DataConnectorInterface
{
    public struct SP_Parameter
    {
        public string name;
        public object value;
        public double size;
        public int type;  // This will be the numerical value of the enum for the specific database data type.
        public ParameterDirection direction;
    }

    public interface IDataConnection
    {
        /// <summary>
        /// Initializes the settings for the DB Connection, such as the connection string, or
        /// Server/port/user/password settings.
        /// </summary>
        void Initialize(string strConnection);

        /// <summary>
        /// Attempts to open a connection to the database with the settings given through initialization
        /// </summary>
        /// <returns>True if a connection was successfully established</returns>
        bool Connect();

        /// <summary>
        /// Executes a query on the Database
        /// </summary>
        /// <param name="sQueryString">The SQL query or Stored Procedure to Execute</param>
        /// <returns>A dataset filled with the data returned from the DB by executing the query</returns>
        DataSet ExecuteQuery(string sQueryString);

        /// <summary>
        /// Executes a query on the Database and returns the data reader.
        /// </summary>
        /// <param name="sQueryString">The SQL query or Stored Procedure to Execute</param>
        /// <returns>A data reader to the data returned from the DB by executing the query</returns>
        //DbDataReader ExecuteReader(string sQueryString);

        /// <summary>
        /// Executes a query on the Database
        /// </summary>
        /// <param name="sQueryString">The SQL query or Stored Procedure to Execute</param>
        /// <param name="htParams">
        /// A HashTable containing all the parameters names and values for the query
        /// </param>
        /// <returns>A dataset filled with the data returned from the DB by executing the query</returns>
        DataSet ExecuteQuery(string sQueryString, Hashtable htParams);

        /// <summary>
        /// Executes a non-query on the Database. Eg. Updates and deletes. Can return an Error code
        /// </summary>
        /// <param name="sQueryString">The SQL querys string or Stored Procedure to Execute</param>
        /// <param name="htParams">
        /// A HashTable containing all the parameters names and values for the query
        /// </param>
        /// <returns>An integer which can be used to return RowIDs or Error codes</returns>
        int ExecuteNonQuery(string sQueryString, Hashtable htParams);

        /// <summary>
        /// Executes a non-query on the Database. Eg. Updates and deletes.
        /// </summary>
        /// <param name="sQueryString">The SQL querys string or Stored Procedure to Execute</param>
        int ExecuteNonQuery(string sQueryString);

        /// <summary>
        /// Executes a procedure on the Database. This is function is used when stored procedure has
        /// a return values else ExecuteNonQuery can be used.
        /// </summary>
        /// <param name="sQueryString">Stored Procedure to Execute</param>
        /// <param name="htParams">
        /// A HashTable containing all the parameters names and values for the query as spParameter
        /// </param>
        /// <returns>None</returns>
        void ExecuteStoredProc(string sQueryString, ref SP_Parameter[] htParams);

        /// <summary>
        /// Gets the server time.
        /// </summary>
        /// <returns></returns>
        DateTime GetServerTime();

        /// <summary>
        /// Closes the Connection to the DB
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Releases the Connection resources
        /// </summary>
        void ReleaseResources();

        bool ValidateRecordExistance(string pTableName, string[] pFieldName, object[] pValueToValidate);

        bool InsertRecord(string pTableName, string[] pFieldName, object[] pValueToValidate);

        bool UpdateRecord(string pTableName, string[] pFieldName, object[] pValueToValidate, string[] pFilterFieldList, object[] pFilterValues);

        bool UpdateRecord(string pTableName, string[] pFieldName, object[] pValueToValidate, string[] pFilterFieldList, object[] pFilterValues, string[] pColTypes, string strDBtype);

        DataSet RetrieveAllDataForTable(string pTableName, string[] pFilterFieldList, object[] pFilterValues);

        bool DeleteRecord(string pTableName, string[] pFilterFieldList, object[] pFilterValues);

        string GetDBID();

        /// <summary>
        /// Return the connection status
        /// </summary>
        bool IsConnected
        {
            get;
        }

        IList<string> GetTableNames();

        DataColumnCollection GetTableColumns(string tableName);

        bool BeginTransaction();

        void EndTransaction(bool commit);

        void EndTranactionRetaining(bool commit);
    }
}