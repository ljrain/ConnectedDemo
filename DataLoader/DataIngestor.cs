using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace DataLoader
{
    /// <summary>
    /// Class responsible for data ingestion from mock data files to the CRM system.
    /// </summary>
    internal class DataIngestor
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the connection string for the CRM system.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the path to the mock data files.
        /// </summary>
        public string MockFilePath { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records to process.
        /// </summary>
        public int MaxRecords { get; set; } = 50;

        /// <summary>
        /// Gets or sets the list of mock contacts.
        /// </summary>
        //public List<MockContact> MockContacts { get; set; } = new List<MockContact>();

        /// <summary>
        /// Gets or sets the list of mock accounts.
        /// </summary>
        //public List<MockAccount> MockAccounts { get; set; } = new List<MockAccount>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DataIngestor"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string for the CRM system.</param>
        /// <param name="mockFilePath">The path to the mock data files.</param>
        public DataIngestor(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Executes the data ingestion process.
        /// </summary>
        public void Execute()
        {
            AddContactsToAccountsFromMockData();
        }

        #region "Private routines"

        /// <summary>
        /// Writes a message to the console with the specified color.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="color">The color to use.</param>
        /// <param name="overwrite">Whether to overwrite the current line.</param>
        private void WriteMessage(string message, bool overwrite = false)
        {
            //if (overwrite)
            //{
            //    Console.SetCursorPosition(0, Console.CursorTop);
            //    Console.Write(new string(' ', Console.WindowWidth)); // Clear the line
            //    Console.SetCursorPosition(0, Console.CursorTop - 1); // Move cursor back to the start of the line
            //}
            Console.WriteLine(message);
        }

        /// <summary>
        /// Retrieves a random account from the CRM system.
        /// </summary>
        /// <returns>A random account entity.</returns>
        public Entity? GetRandomAccount()
        {
            using (ServiceClient serviceClient = new ServiceClient(ConnectionString))
            {
                if (serviceClient.IsReady)
                {
                    QueryExpression query = new QueryExpression("account")
                    {
                        ColumnSet = new ColumnSet(true)
                    };

                    EntityCollection accounts = serviceClient.RetrieveMultiple(query);

                    if (accounts.Entities.Count > 0)
                    {
                        Random random = new Random();
                        int randomIndex = random.Next(accounts.Entities.Count);
                        return accounts.Entities[randomIndex];
                    }
                    else
                    {
                        WriteMessage("No accounts found.");
                        return null;
                    }
                }
                else
                {
                    WriteMessage("Service client is not ready.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds contacts to accounts from the mock data.
        /// </summary>
        public void AddContactsToAccountsFromMockData()
        {
            WriteMessage("AddContactsToAccountsFromMockData is running");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ServiceClient serviceClient;
            Entity? randomAccount = GetRandomAccount();
            if (randomAccount == null)
            {
                WriteMessage("No random account available.");
                return;
            }
            WriteMessage("Random account: " + randomAccount["name"], true);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            using (serviceClient = new ServiceClient(ConnectionString))
            {
                if (serviceClient.IsReady)
                {
                    WriteMessage("Adding contacts to account: " + randomAccount["name"], true);
                    for (int i = 0; i < MaxRecords; i++)
                    {
                        Entity contact = new Entity("contact");
                        contact["firstname"] = "First " + GetRandomNumberAsString();
                        contact["lastname"] = "Last " + GetRandomNumberAsString();
                        contact["emailaddress1"] = GetRandomNumberAsString() + "user@example.com";
                        contact["parentcustomerid"] = randomAccount.ToEntityReference();
                        Guid contactId = serviceClient.Create(contact);
                        contact = serviceClient.Retrieve("contact", contactId, new ColumnSet(true));
                        WriteMessage("Added contact: " + contact["fullname"], true);

                        Entity task = new Entity("task");
                        task["subject"] = "Follow up with " + contact.Attributes["fullname"];
                        task["description"] = "Follow up with " + contact.Attributes["fullname"];
                        task["scheduledstart"] = DateTime.Now.AddDays(1);
                        task["scheduledend"] = DateTime.Now.AddDays(2);
                        task["regardingobjectid"] = contact.ToEntityReference();
                        Guid taskId = serviceClient.Create(task);
                        WriteMessage("Added task: " + task["subject"], true);

                        Entity appointment = new Entity("appointment");
                        appointment["subject"] = "Meeting with " + contact.Attributes["fullname"];
                        appointment["description"] = "Meeting with " + contact.Attributes["fullname"];
                        appointment["scheduledstart"] = DateTime.Now.AddDays(3);
                        appointment["scheduledend"] = DateTime.Now.AddDays(4);
                        appointment["regardingobjectid"] = contact.ToEntityReference();
                        Guid appointmentId = serviceClient.Create(appointment);
                        WriteMessage("Added appointment: " + appointment["subject"], true);

                        WriteMessage("Adding 5 service requests for " + contact.Attributes["fullname"], true);
                        for (int x = 0; x < 5; x++)
                        {
                            Entity serviceReq = new Entity("ljr_servicerequest");
                            serviceReq["ljr_name"] = "Service Request for " + contact.Attributes["fullname"];
                            serviceReq["ljr_userstory"] = "As a Data Anaylst I need to be able to work with the data in a fast manner SO i can get this done.";
                            serviceReq["ljr_customer"] = randomAccount.ToEntityReference();
                            Guid serviceReqId = serviceClient.Create(serviceReq);
                        }
                        
                        WriteMessage("Adding 5 e-activities for " + contact.Attributes["fullname"], true);
                        for (int x = 0; x < 5; x++)
                        {
                            Entity eActivity = new Entity("ljr_eactvitity");
                            eActivity["ljr_name"] = "Activity for " + contact.Attributes["fullname"];
                            eActivity["ljr_json"] = GenerateEActivityJson(contact["fullname"].ToString(), randomAccount["name"].ToString());
                            eActivity["ljr_account"] = randomAccount.ToEntityReference();
                            eActivity["ttlinseconds"] = 86400 * 1; // time to live is 7 days
                            Guid eActivityId = serviceClient.Create(eActivity);
                        }
                    }
                }
            }
            timer.Stop();
            WriteMessage("AddContactsToAccountsFromMockData took " + timer.Elapsed.TotalMilliseconds + " milliseconds.");
        }

        private string GenerateEActivityJson(string contactName, string accountName)
        {
            return $"{{ \"contact\": \"{contactName}\", \"account\": \"{accountName}\", \"data\": {{ \"timestamp\": \"880\", \"duration\": \"123\" }} }}";
        }


        private string GetRandomNumberAsString()
        {
            Random random = new Random();
            return random.Next(10000, 99999).ToString();
        }

        /// <summary>
        /// Loads new accounts into the CRM system.
        /// </summary>
        public void LoadNewAccounts()
        {
            WriteMessage("LoadNewAccounts is running");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int i = 0;
            using (ServiceClient serviceClient = new ServiceClient(ConnectionString))
            {
                if (serviceClient.IsReady)
                {
                    Entity act = new Entity("account");
                    act["name"] = "Test Account " + i.ToString();
                    act["telephone1"] = "1234567890";
                    act["emailaddress1"] = "testaccount" + i.ToString() + "@example.com";
                    act["address1_line1"] = "123 Test St";
                    act["address1_city"] = "Test City";
                    act["address1_stateorprovince"] = "Test State";
                    act["address1_postalcode"] = "12345";
                    act["address1_country"] = "Test Country";
                    act["address1_latitude"] = 12.345678;
                    act["address1_longitude"] = 98.765432;
                    act["ljr_accounttype"] = new OptionSetValue(100000001);
                    act["ljr_accountcategory"] = new OptionSetValue(100000002);
                    act["ljr_accountsubtype"] = new OptionSetValue(100000003);
                    act["ljr_accountstatus"] = new OptionSetValue(100000004);

                    Guid actId = serviceClient.Create(act);
                    act = serviceClient.Retrieve("account", actId, new ColumnSet(true));
                }
            }
        }

        #endregion

    }
}
