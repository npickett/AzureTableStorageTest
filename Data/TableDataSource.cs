using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;
using System.Data.Services.Client;
using System.Text.RegularExpressions;
using Microsoft.Samples.ServiceHosting.AspProviders;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Data
{
    public class TableDataSource<T> where T : TableServiceEntity
    {
        private CloudStorageAccount storageAccount;
        private TableDataContext<T> context;
        private const int ASYNC_TIMEOUT = 6000000;
        private const short WAIT_HANDLE_MAX = 63;

        public TableDataSource()
        {
            storageAccount = CloudConfiguration.GetStorageAccount("DataConnectionString");

            CreateDataServiceContext();
        }

        public void CreateTestTable()
        {
            storageAccount.CreateCloudTableClient().CreateTableIfNotExist(context.ContextTableName);
        }

        public void DeleteTestTable()
        {
            storageAccount.CreateCloudTableClient().DeleteTableIfExist(context.ContextTableName);
        }

        public IEnumerable<T> Select()
        {
            var results = from g in context.TableName
                          where g.PartitionKey == "1"
                          select g;
            return results;
        }

        public T SelectEntry(string partitionKey, string rowKey)
        {
            var results = from g in context.TableName
                          where g.PartitionKey == partitionKey && g.RowKey == rowKey
                          select g;

            return results.FirstOrDefault<T>();
        }

        public void AddTestEntry(T newItem)
        {
            try
            {
                context.AddObject(context.ContextTableName, newItem);
                context.SaveChangesWithRetries();
            }
            catch (DataServiceRequestException e)
            {
                var inner = e.InnerException as DataServiceClientException;
                if (inner != null)
                {
                    var innerMessage = inner.Message;
                    var statusCode = inner.StatusCode;
                }
            }
            finally
            {
                context.Detach(newItem);
            }
        }

        public void UpdateTableEntry(T entry)
        {
            context.UpdateObject(entry);
            context.SaveChanges();
        }

        public void DeleteTableEntry(T entry)
        {
            context.DeleteObject(entry);
            context.SaveChanges();
        }

        public TableDataContext<T> CreateDataServiceContext()
        {
            context = new TableDataContext<T>(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            return context;
        }

        public async Task<TableStorageOperationState> AddTableEntryRequestAsync(IList<T> transactionGroup)
        {
            var context = CreateDataServiceContext();
            context.MergeOption = MergeOption.AppendOnly;

            foreach (var entity in transactionGroup)
            {
                context.AddObject(context.ContextTableName, entity);
            }

            var saveChangesOption = transactionGroup.Count > 1 ? SaveChangesOptions.Batch : SaveChangesOptions.None;

            var state = new TableStorageOperationState();
            var firstEntity = transactionGroup.First();

            state.OperationType = "AddObject";
            state.IsBatch = saveChangesOption == SaveChangesOptions.Batch ? true : false;

            state.BatchCount = transactionGroup.Count;
            state.PartitionKey = firstEntity.PartitionKey;

            state.StartDate = DateTime.UtcNow;
            state.StatusCode = (int)HttpStatusCode.InternalServerError;

            try
            {
                DataServiceResponse response = await Task<DataServiceResponse>.Factory
                    .FromAsync(context.BeginSaveChangesWithRetries, context.EndSaveChangesWithRetries, state);

                state.IsSuccess = true;
                state.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (DataServiceRequestException e)
            {
                var inner = e.InnerException as DataServiceClientException;
                if (inner != null)
                {
                    state.IsSuccess = false;
                    state.ErrorCode = GetErrorCode(e);
                    state.StatusCode = inner.StatusCode;
                }
            }
            catch (Exception ex)
            {
                state.IsSuccess = false;
                state.ErrorCode = ex.Message;
            }
            finally
            {
                state.EndDate = DateTime.UtcNow;
                state.ElapsedTime = (state.EndDate - state.StartDate).TotalMilliseconds.ToString();
                state.IsComplete = true;
            }

            return state;

        }

        public async void AddTableEntryAsync(List<T> entities, Action<IEnumerable<TableStorageOperationState>> resultCallback)
        {
            var inputList = Enumerable.Empty<IList<T>>();

            inputList = entities.GroupAndSlice(Constants.MAX_ENTITY_TRANSACTION_COUNT, delegate(T entry1, T entry2)
            {
                if (entry1.PartitionKey == entry2.PartitionKey)
                {
                    return true;
                }

                return false;
            });

            IEnumerable<Task<TableStorageOperationState>> tasks = inputList.Select(AddTableEntryRequestAsync);
            Task<TableStorageOperationState[]> allTasks = Task.WhenAll(tasks);

            TableStorageOperationState[] taskResults = await allTasks;
            resultCallback(taskResults);
        }

        public IEnumerable<TableStorageOperationState> AddTableEntryAsync(List<T> entities)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<List<T>> dataByPartitionList = entities.GroupBy(item => item.PartitionKey)
                    .Select(group => new List<T>(group))
                    .ToList();

            var resultList = new List<IAsyncResult>();
            var waitHandles = new List<WaitHandle>();
            var stateList = new List<TableStorageOperationState>();

            foreach (var entityGroup in dataByPartitionList)
            {
                foreach (List<T> transactionGroup in Slice<T>(entityGroup, Constants.MAX_ENTITY_TRANSACTION_COUNT))
                {
                    var context = CreateDataServiceContext();
                    context.MergeOption = MergeOption.AppendOnly;

                    foreach (var entity in transactionGroup)
                    {
                        context.AddObject(context.ContextTableName, entity);
                    }

                    var saveChangesOption = entities.Count > 1 ? SaveChangesOptions.Batch : SaveChangesOptions.None;

                    var state = new TableStorageOperationState();
                    var firstEntity = transactionGroup.First();
                    //state.Entries = new List<object>();

                    state.OperationType = "AddObject";
                    state.IsBatch = saveChangesOption == SaveChangesOptions.Batch ? true : false;
                    //state.Entries.Add(transactionGroup);
                    state.BatchCount = transactionGroup.Count;
                    state.PartitionKey = firstEntity.PartitionKey;

                    state.StartDate = DateTime.UtcNow;
                    state.StatusCode = (int)HttpStatusCode.InternalServerError;

                    var response = context.BeginSaveChangesWithRetries(saveChangesOption, (result) =>
                    {
                        CallbackData callbackData = null;

                        try
                        {
                            callbackData = (CallbackData)result.AsyncState;
                            callbackData.Context.EndSaveChangesWithRetries(result);

                            callbackData.State.IsSuccess = true;
                            callbackData.State.StatusCode = (int)HttpStatusCode.OK;
                        }
                        catch (DataServiceRequestException e)
                        {

                            var inner = e.InnerException as DataServiceClientException;
                            if (inner != null)
                            {
                                callbackData.State.IsSuccess = false;
                                callbackData.State.ErrorCode = GetErrorCode(e);
                                callbackData.State.StatusCode = inner.StatusCode;
                            }
                        }
                        catch (Exception ex)
                        {
                            callbackData.State.IsSuccess = false;
                            callbackData.State.ErrorCode = ex.Message;
                        }
                        finally
                        {
                            callbackData.State.EndDate = DateTime.UtcNow;
                            callbackData.State.ElapsedTime = (callbackData.State.EndDate - callbackData.State.StartDate).TotalMilliseconds.ToString();
                            callbackData.State.IsComplete = true;
                        }
                    }
                    , new CallbackData { Context = context, State = state });

                    resultList.Add(response);
                    waitHandles.Add(response.AsyncWaitHandle);

                    if (waitHandles.Count >= WAIT_HANDLE_MAX)
                    {
                        if (WaitHandle.WaitAll(waitHandles.ToArray(), ASYNC_TIMEOUT, false))
                        {
                            foreach (IAsyncResult result in resultList)
                            {
                                var callbackState = result.AsyncState as CallbackData;
                                stateList.Add(callbackState.State);
                            }
                        }
                        else
                        {
                            stateList.Add(new TableStorageOperationState() { IsSuccess = false, ErrorCode = "Timeout" });
                        }

                        resultList.Clear();
                        waitHandles.Clear();
                    }

                }
            }

            if (waitHandles.Count > 0)
            {
                if (WaitHandle.WaitAll(waitHandles.ToArray(), ASYNC_TIMEOUT, false))
                {
                    foreach (IAsyncResult result in resultList)
                    {
                        var callbackState = result.AsyncState as CallbackData;
                        stateList.Add(callbackState.State);
                    }
                }
                else
                {
                    stateList.Add(new TableStorageOperationState() { IsSuccess = false, ErrorCode = "Timeout" });
                }

                resultList.Clear();
                waitHandles.Clear();
            }

            sw.Stop();

            return stateList;
        }

        public static string GetErrorCode(DataServiceRequestException ex)
        {
            var r = new Regex(@"<code>(\w+)</code>", RegexOptions.IgnoreCase);
            var match = r.Match(ex.InnerException.Message);
            return match.Groups[1].Value;
        }
    }
}
