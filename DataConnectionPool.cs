using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Core.Data.DataConnectorInterface
{
    public class DataConnectionPool : IDisposable
    {
        private List<IDataConnection> m_availableConnections = new List<IDataConnection>();
        private List<IDataConnection> m_usedConnections = new List<IDataConnection>();
        private int maxConnections = 0;

        public DataConnectionPool(string strConnection,
                                  string strLibName,
                                  string strClassName,
                                  int max)
        {
            try
            {
                maxConnections = max;
                while((m_availableConnections.Count + m_usedConnections.Count) < maxConnections)
                {
                    IDataConnection thisConnection = DataConnectionFactory.CreateConnection(strLibName, strClassName);
                    thisConnection.Initialize(strConnection);                    
                    thisConnection.Connect();

                    m_availableConnections.Add(thisConnection);
                }
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DataConnectionPool DataConnectionPool");
                throw new Exception("Core.Data.DataConnectionPool DataConnectionPool", ex);
            }
        
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCalledFromMethod()
        {
            string returnThis = "";
            try
            {
                StackTrace st = new StackTrace();
                StackFrame sf0 = st.GetFrame(0);
                StackFrame sf1 = st.GetFrame(1);
                StackFrame sf2 = st.GetFrame(2);

                var method0 = sf0.GetMethod();
                var fullMethod0 = System.Reflection.MethodBase.GetMethodFromHandle(method0.MethodHandle);

                var method1 = sf1.GetMethod();
                var fullMethod1 = System.Reflection.MethodBase.GetMethodFromHandle(method1.MethodHandle);

                var method2 = sf2.GetMethod();
                var fullMethod2 = System.Reflection.MethodBase.GetMethodFromHandle(method2.MethodHandle);

                if(fullMethod0.Name == "GetCalledFromMethod")
                {
                    returnThis = "[{ " + fullMethod1.ToString() + " - " + fullMethod2.ToString() + " }]";
                }
                else
                {
                    returnThis = "[{ " + fullMethod0.ToString() + " - " + fullMethod1.ToString() + " - " + fullMethod2.ToString() + " }]";
                }

                return returnThis;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DataConnectionPool GetCalledFromMethod");
                return "[{}]";
            }
        }

        public IDataConnection GetConnection()
        {
            try
            {
                IDataConnection thisConnection = null;
                lock(this)
                {
                    if(m_availableConnections.Count > 0)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            thisConnection = m_availableConnections[0];
                            thisConnection.Connect();
                            if (thisConnection == null && m_availableConnections.Count > 0)
                            {
                                ReleaseConnection(thisConnection);
                                m_availableConnections.Remove(thisConnection);
                                i = -1;
                            }

                            m_availableConnections.Remove(thisConnection);
                            m_usedConnections.Add(thisConnection);
                        }
                        
                    }
                }
                return thisConnection;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DataConnectionPool GetConnection");
                throw new Exception("Core.Data.DataConnectionPool GetConnection", ex);
            }
        }

        public void ReleaseConnection(IDataConnection connection)
        {
            try
            {
                lock(this)
                {
                    connection.CloseConnection();
                    m_usedConnections.Remove(connection);
                    m_availableConnections.Add(connection);
                }
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DataConnectionPool ReleaseConnection");
                throw new Exception("Core.Data.DataConnectionPool ReleaseConnection", ex);
            }
        }

        public bool CloseAllConnections()
        {
            try
            {
                foreach(IDataConnection _conn in m_availableConnections)
                {
                    _conn.CloseConnection();
                }
                return true;
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DataConnectionPool CloseAllConnections");
                throw new Exception("Core.Data.DataConnectionPool CloseAllConnections", ex);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                foreach(IDataConnection conn in m_usedConnections)
                {
                    m_availableConnections.Add(conn);
                }
                m_usedConnections.Clear();
                foreach(IDataConnection conn in m_availableConnections)
                {
                    conn.CloseConnection();
                    conn.ReleaseResources();
                }
                m_availableConnections.Clear();
                GC.SuppressFinalize(this);
            }
            catch(Exception ex)
            {
                this.SetErrorMessage(ex, "Core.Data.DataConnectionPool Dispose");
                throw new Exception("Core.Data.DataConnectionPool Dispose", ex);
            }
        }

        #endregion IDisposable Members

        private void SetErrorMessage(Exception ex, string methodName)
        {
            
        }
    }
}