using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Data
{
    public class TableDataContext<T> : TableServiceContext
    {
        public TableDataContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }

        public string ContextTableName
        {
            get { return "TestTable"; }
        }

        public IQueryable<T> TableName
        {
            get
            {
                return CreateQuery<T>(ContextTableName);
            }
        }
    }
}
