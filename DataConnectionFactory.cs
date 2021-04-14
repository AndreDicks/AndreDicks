using Core.Libraries;
using System;
using System.IO;

namespace Core.Data.DataConnectorInterface
{
    public class DataConnectionFactory
    {
        public static IDataConnection CreateConnection(string sDLLName, string sClassName)
        {
            try
            {
                /* use the library manager to load and get and instance of the class */
                LibraryManager.Instance.DefaultPath = new FileInfo(sDLLName).DirectoryName;
                return LibraryManager.Instance.LoadLibrary(sDLLName).GetObject<IDataConnection>(sClassName);
            }
            catch(Exception ex)
            {
                SetErrorMessage(ex, "Core.Data.DataConnectionFactory CreateConnection");
                throw ex;
            }
        }

        public static void SetErrorMessage(Exception ex, string methodName)
        {
        }
    }
}