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
    internal class DataIngestor
    {
        #region "Properties"

        public string ConnectionString { get; set; }
        public string MockFilePath { get; set; }
        public int MaxRecords { get; set; } = 5;

        public List<MockContact> MockContacts { get; set; }
        public List<MockAccount> MockAccounts { get; set; } = new List<MockAccount>();


        #endregion

        public DataIngestor(string connectionString, string mockFilePath)
        {
            ConnectionString = connectionString;
            MockFilePath = mockFilePath;
        }

        public void Execute()
        {
            AddContactsToAccountsFromMockData();
        }

        #region "Private routines"
        public void LoadMockData()
        {
            LoadCsvData();
            // Use the contacts array to create contacts in the CRM system
        }

        private void LoadCsvData()
        {
            // Load the CSV data into a list of mock contacts
            var contacts = new List<MockContact>();
            string contactMockFile = Path.Combine(MockFilePath + @"\contacts1.csv");
            string accountMockFile = Path.Combine(MockFilePath + @"\accounts1.csv");

            if (File.Exists(contactMockFile) == false)
            {
                Console.WriteLine("File not found: " + contactMockFile);
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };

            using (var reader = new StreamReader(contactMockFile))
            using (var csv = new CsvReader(reader, config))
            {
                contacts = csv.GetRecords<MockContact>().ToList();
            }
            MockContacts = contacts;

            // Load the CSV data into a list of mock accounts
            List<MockAccount> accounts = new List<MockAccount>();

            if (File.Exists(Path.Combine(accountMockFile)) == false)
            {
                Console.WriteLine("File not found: " + accountMockFile);
            }

            config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };

            using (var reader = new StreamReader(accountMockFile))
            using (var csv = new CsvReader(reader, config))
            {
                accounts = csv.GetRecords<MockAccount>().ToList();
            }
            MockAccounts = accounts;
        }

        public Entity GetRandomAccount()
        {
            Entity randomAccount = null;
            using (ServiceClient serviceClient = new ServiceClient(ConnectionString))
            {
                if (serviceClient.IsReady)
                {

                    // Query to retrieve all accounts
                    QueryExpression query = new QueryExpression("account")
                    {
                        ColumnSet = new ColumnSet(true) // Retrieve all columns
                    };

                    EntityCollection accounts = serviceClient.RetrieveMultiple(query);

                    if (accounts.Entities.Count > 0)
                    {
                        // Get a random account
                        Random random = new Random();
                        int randomIndex = random.Next(accounts.Entities.Count);
                        return accounts.Entities[randomIndex];
                    }
                    else
                    {
                        Console.WriteLine("No accounts found.");
                        return randomAccount;
                    }
                }
                else
                {
                    Console.WriteLine("Service client is not ready.");
                    return randomAccount;
                }
            }
        }


        public void AddContactsToAccountsFromMockData()
        {
            Console.WriteLine("AddContactsToAccountsFromMockData is running");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Load the CSV data into a list of mock accounts / contacts properties
            LoadCsvData();

            ServiceClient serviceClient;

            //// Load accounts from Mock Data
            
            //using (serviceClient = new ServiceClient(ConnectionString))
            //{
            //    if (serviceClient.IsReady)
            //    {
            //        for (int i = 0; i < MockAccounts.Count; i++)
            //        {
            //            MockAccount ma = MockAccounts[i];
            //            Entity account = new Entity("account");
            //            account["name"] = ma.organization_name;
            //            Guid actId = serviceClient.Create(account);
            //        }
            //    }
            //}


            // Load a random account
            Entity randomAccount = GetRandomAccount();

            using (serviceClient = new ServiceClient(ConnectionString))
            {
                if (serviceClient.IsReady)
                {
                    // save 5 contacts to the random account
                    for (int i = 0; i < MaxRecords; i++)
                    {
                        // Get a random MockContact
                        Random random = new Random();
                        int randomIndex = random.Next(MockContacts.Count);
                        MockContact mc = MockContacts[randomIndex];

                        Entity contact = new Entity("contact");
                        contact["firstname"] = mc.first_name;
                        contact["lastname"] = mc.last_name;
                        contact["emailaddress1"] = mc.email;
                        //contact["city"] = mc.city;
                        //contact["country"] = mc.country_code;
                        contact["parentcustomerid"] = randomAccount.ToEntityReference();
                        Guid contactId = serviceClient.Create(contact);
                        // Optionally, retrieve the created contact
                        contact = serviceClient.Retrieve("contact", contactId, new ColumnSet(true));

                        // add a task and appointment to the contact
                        Entity task = new Entity("task");
                        task["subject"] = "Follow up with " + mc.first_name + " " + mc.last_name;
                        task["description"] = "Follow up with " + mc.first_name + " " + mc.last_name;
                        task["scheduledstart"] = DateTime.Now.AddDays(1);
                        task["scheduledend"] = DateTime.Now.AddDays(2);
                        task["regardingobjectid"] = contact.ToEntityReference();  
                        Guid taskId = serviceClient.Create(task);
                    }
                }
            }
        }


        public void LoadNewAccounts()
        {
            Console.WriteLine("LoadNewAccounts is running");
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
                    act["ljr_accounttype"] = new OptionSetValue(100000001); // 100000001 = Customer
                    act["ljr_accountcategory"] = new OptionSetValue(100000002); // 100000002 = Business
                    act["ljr_accountsubtype"] = new OptionSetValue(100000003); // 100000003 = Corporate
                    act["ljr_accountstatus"] = new OptionSetValue(100000004); // 100000004 = Active

                    Guid actId = serviceClient.Create(act);

                    act = serviceClient.Retrieve("account", actId, new ColumnSet(true));
                }
            }
        }
        #endregion


        // id,first_name,last_name,email,gender,city,country_code
        public class MockContact
        {
            public string id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string email { get; set; }
            public string gender { get; set; }
            public string City { get; set; }
            public string city { get; set; }
            public string country_code { get; set; }
        }

        //organization_name,founded_year,hq_city,revenue,employee_count,ceo_name,stock_symbol
        public class MockAccount
        {
            public string organization_name { get; set; }
            public int founded_year { get; set; }
            public string hq_city { get; set; }
            public int employee_count { get; set; }

            public string ceo_name { get; set; }

            public string stock_symbol { get; set; }
        }



    }
}
