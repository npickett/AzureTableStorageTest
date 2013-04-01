using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Web
{
    public partial class Default : System.Web.UI.Page
    {
        protected PageData pageData;
        private const int DEFAULT_SIZE = 1;
        private const int DEFAULT_ENTERIES = 100;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PageData"] != null)
                pageData = Session["PageData"] as PageData;
            else
                pageData = new PageData();

            if (!this.IsPostBack)
            {
                txtSize.Text = DEFAULT_SIZE.ToString();
                txtEnteries.Text = DEFAULT_ENTERIES.ToString();
            }
        }

        protected void Page_PreRenderComplete()
        {
            // bind session data to the Repeater control
            InProgress.DataSource = pageData.ResultsList;
            InProgress.DataBind();

            // update the labels
            litNoProgress.Visible = InProgress.Items.Count == 0;
        }

        protected void Page_Unload()
        {
            Session["PageData"] = pageData;
        }

        protected IEnumerable<TableStorageOperationState> BeginInsertBatch()
        {
            var ds = new TableDataSource<TableEntry>();

            int enteries = DEFAULT_ENTERIES;
            if (txtEnteries.Text != "")
                enteries = Convert.ToInt32(txtEnteries.Text);

            int size = DEFAULT_SIZE;
            if (txtSize.Text != "")
                size = Convert.ToInt32(txtSize.Text);

            byte[] data = new byte[size];

            var list = new List<TableEntry>();

            for (int i = 0; i < enteries; i++)
            {
                // Spread partition key into groups with the max entity transaction count
                string partitionKey = Math.Ceiling((double)(i+1) / Data.Constants.MAX_ENTITY_TRANSACTION_COUNT).ToString();

                list.Add(new TableEntry(partitionKey, data));
            }

            return ds.AddTableEntryAsync(list);
        }

        public void OnTimeout(IAsyncResult ar)
        {
            if (!this.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "changeColor"))
                ClientScript.RegisterClientScriptBlock(this.GetType(), "changeColor",
                    "<script>document.body.style.backgroundColor = \"#FF0000\"</script>");
        }


        protected void btnRun_Click(object sender, EventArgs e)
        {
            var stateData = this.BeginInsertBatch();

            if (stateData != null)
            {
                pageData.ResultsList.AddRange(stateData);
            }
        }

        // session state class
        protected class PageData
        {
            public List<TableStorageOperationState> ResultsList = new List<TableStorageOperationState>();
            public Boolean QueryResultsComplete = false;
        }

        protected void btnRecreate_Click(object sender, EventArgs e)
        {
            var ds = new TableDataSource<TableEntry>();

            ds.DeleteTestTable();
            System.Threading.Thread.Sleep(40000);
            ds.CreateTestTable();
        }

        protected void btnClearSession_Click(object sender, EventArgs e)
        {
            pageData = new PageData();
            Session["PageData"] = pageData;
        }
    }
}