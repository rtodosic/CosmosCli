# CosmosCli

One of the limitations of Azure Cosmos DB for NoSQL is that it isn't easy to update documents 
as you can in a relational database. You may have to write your own application to update your 
documents in Cosmos DB. The idea behind this tool is the following:

Select -> Transform -> Upsert

1. Select your json documents from Cosmos DB
1. Transform the json documents using [jq](https://jqlang.github.io/jq)
1. Upsert the json documents back into Cosmos DB


## Installation
Coming soon


## Compiling

This is a .Net 8 application. You will need to download the [SDK](https://dotnet.microsoft.com/en-us/download). You will also need to have an internet connection to download required nuget packages during the build. 

1. Clone this repository to your local drive.
```bash
git clone https://github.com/rtodosic/CosmosCli.git
```
1. Change into the repository directory.
```bash
cd CosmosCli
```
1. Build the project (optional) 
```bash
dotnet build
```
1. Create the NuGet package  
```bash
dotnet pack -c Release
```
1. Install the command-line tool 
```bash
dotnet tool install --global --add-source Cosmos/Package/ cosmos
```

## Usage

```bash
cosmos --help
```

```text
Usage: CosmosCli [command]

CosmosCli

Commands:
  account
  database
  container

Options:
  -h, --help    Show help message
  --version     Show version
```

```bash
cosmos container --help
```

```text
Usage: CosmosCli container [command]

CosmosCli

Commands:
  new
  delete
  index
  select
  upsert

Options:
  -h, --help    Show help message
```

```bash
cosmos container upsert --help
```

```text
Usage: CosmosCli container upsert [--database <String>] [--container <String>] [--compress-json] [--show-response-stats] [--partition-key <String>] [--partition-key-value <String>] [--hide-results] [--consistency-level <String>] [--exclude-region <String>...] [--post-triggers <String>...] [--pre-triggers <String>...] [--session-token <String>] [--endpoint <String>] [--key <String>] [--verbose] [--help] json-items

Arguments:
  0: json-items

Options:
  -d, --database <String>               The name of the database that you are connecting to in Cosmos DB.
  -c, --container <String>              The name of the container that you are connecting to in Cosmos DB.
  -j, --compress-json                   Compress the json output by removing whitespace and carriage returns.
  -s, --show-response-stats             Show response statistics (including RUs, StatusCode, ContinuationToken).
  -p, --partition-key <String>          Specify the name of the partition key which is provided in the input json.
  -a, --partition-key-value <String>    Specify the partition key value which will be used during the upsert.
  -i, --hide-results                    After upsert don't display the saved items.
  -l, --consistency-level <String>      Consistency Level (Eventual, ConsistentPrefix, Session, BoundedStaleness, Strong)
  -r, --exclude-region <String>...      Exclude region(s) while querying
  --post-triggers <String>...           Set the names of the post trigger to be invoked after the upsert.
  --pre-triggers <String>...            Set the names of the pre trigger to be invoked after the upsert.
  -t, --session-token <String>          Set a session token in the query request.
  -e, --endpoint <String>               The endpointUri used to connect to the Cosmos DB service
  -k, --key <String>                    The primary or secondary key used to connect to the Cosmos DB service.
  -v, --verbose                         Display more details of what is happening.
  -h, --help                            Show help message
```
