// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Data Loader (Connected Demo)");

        if (args.Length < 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Please provide the Dataverse connection string as a command line argument.");
            Console.ResetColor();
            return;
        }

        string CONNECTION_STRING = args[0];

        string MOCK_FILE_PATH = "mockdata";

        DataLoader.DataIngestor dataIngestor = new DataLoader.DataIngestor(CONNECTION_STRING, MOCK_FILE_PATH);
        dataIngestor.Execute();

        Console.WriteLine("Data Loader (Connected Demo) - Done");
    }
}
