
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Core.Data.DataConnectorInterface
{
    public class DBManager
    {
        private static volatile DBManager instance;
        private static Object syncRoot = new Object();
        private static bool bInitialized = false;
        private Dictionary<string, DataConnectionPool> pools = new Dictionary<string, DataConnectionPool>();
        public List<string> databaseIDs = new List<string>();

        public static DBManager Instance
        {
            get
            {
                // Create the singleton
                if(instance == null)
                {
                    lock(syncRoot)
                    {
                        if(instance == null)
                        {
                            instance = new DBManager();
                        }
                    }
                }

                // Make sure that we return an initialized instance
                //if (!bInitialized)
                //    instance.Initialize();

                return instance;
            }
        }

        private DBManager()
        {
        }

        public IDictionary<string, object> GetXMLNodeGroup(XmlDocument doc, string sGroupName)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach(XmlNode xn in doc.DocumentElement.ChildNodes)
                {
                    if(xn.NodeType != XmlNodeType.Comment)
                    {
                        if(xn.Attributes["Group"].Value == sGroupName)
                        {
                            dic.Add(xn.Attributes["Name"].Value, xn.InnerText);
                        }
                    }
                }
                return dic;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DBManager IDictionary");
                throw new Exception("Core.Data.DBManager IDictionary", ex);
            }
        }

        

        public void Uninitialize(string strDBID)
        {
            try
            {
                if(pools.ContainsKey(strDBID))
                {
                    DataConnectionPool pool = pools[strDBID];
                    pool.CloseAllConnections();
                    pools.Remove(strDBID);
                    databaseIDs.Remove(strDBID);
                }
            }
            catch(Exception)
            {
            }
        }

        
        

        

        public void Initialize(string strConnection, string strLibraryName, string strClassName, int iNumConnections, string strDBID)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                if (!pools.ContainsKey(strDBID))
                {
                  pools.Add(strDBID, new DataConnectionPool(strConnection, strLibraryName, strClassName, iNumConnections));
                  databaseIDs.Add(strDBID);
                }
                  
                bInitialized = true;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DBManager Initialize");
                throw new Exception("Core.Data.DBManager Initialize", ex);
            }
        }

        public IDataConnection GetDataConnector()
        {
            try
            {
                return pools[databaseIDs[0]].GetConnection();
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DBManager GetDataConnector");
                throw new Exception("Core.Data.DBManager GetDataConnector", ex);
            }
        }

        public IDataConnection GetDataConnector(string dbID)
        {
            try
            {
                return pools[dbID].GetConnection();
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DBManager GetDataConnector");
                throw new Exception("Core.Data.DBManager GetDataConnector", ex);
            }
        }

        public void ReturnDataConnector(IDataConnection dataConnector)
        {
            try
            {
                pools[dataConnector.GetDBID()].ReleaseConnection(dataConnector);
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DBManager ReturnDataConnector");
                throw new Exception("Core.Data.DBManager ReturnDataConnector", ex);
            }
        }

        private void SetErrorMessage(Exception ex, string methodName)
        {
            
        }
    }
}