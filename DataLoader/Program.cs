// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;

Console.WriteLine("Data Loader (Connected Demo)");

string CONNECTION_STRING = "AuthType=ClientSecret;ClientId=70dbbbb5-71bd-45d5-a23b-f23149d30c73;ClientSecret=1F58Q~F8aLc1gZ0MUnCNxypvUUm7fPNhQcq5JbT1;Url=https://ljr-dev1.crm9.dynamics.com/";

string MOCK_FILE_PATH = "mockdata";

DataLoader.DataIngestor dataIngestor = new DataLoader.DataIngestor(CONNECTION_STRING, MOCK_FILE_PATH);
dataIngestor.Execute();


Console.WriteLine("Data Loader (Connected Demo) - Done");
