# DataLoader

DataLoader is a .NET application designed to ingest mock data files into a CRM system. It connects to the CRM system using a provided connection string and processes mock data to create accounts, contacts, tasks, and appointments.

## Features

- Connects to a CRM system using a connection string.
- Loads new accounts into the CRM system.
- Adds contacts to accounts from mock data.
- Creates tasks and appointments for the added contacts.

## Requirements

- .NET 8.0
- Visual Studio 2022
- CRM system connection string

## Installation

1. Clone the repository:
2. Open the solution in Visual Studio 2022.

3. Restore the NuGet packages:

## Configuration

The application requires a connection string to connect to the CRM system. This connection string should be provided as a command line argument when running the application.

## Usage

1. Build the solution in Visual Studio 2022.

2. Run the application with the CRM connection string as a command line argument:
## Code Overview

### Program.cs

The `Program` class is the entry point of the application. It reads the connection string from the command line arguments and initializes the `DataIngestor` class to execute the data ingestion process.
### DataIngestor.cs

The `DataIngestor` class is responsible for the data ingestion process. It includes methods to load new accounts, add contacts to accounts from mock data, and create tasks and appointments for the contacts.

#### Properties

- `ConnectionString`: The connection string for the CRM system.
- `MockFilePath`: The path to the mock data files.
- `MaxRecords`: The maximum number of records to process.

#### Methods

- `Execute()`: Executes the data ingestion process.
- `GetRandomAccount()`: Retrieves a random account from the CRM system.
- `AddContactsToAccountsFromMockData()`: Adds contacts to accounts from the mock data.
- `LoadNewAccounts()`: Loads new accounts into the CRM system.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
