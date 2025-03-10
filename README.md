# CosmosCli

One of the limitations of Azure Cosmos DB for NoSQL is that it isn't easy to update documents 
as you can in a relational database. You may have to write your own application to update your 
documents in Cosmos DB. The idea behind this tool is the following:

Select -> Transform -> Upsert

1. Select your json documents from Cosmos DB using [select-item](#container-select-item)
1. Transform the json documents using [jq](https://jqlang.github.io/jq)
1. Upsert the json documents back into Cosmos DB using [upsert-item](#container-upsert-item)

Sample Scripts
* [Add a property](#sample-script-to-add-a-property)
* [Update properties](#sample-script-to-update-properties)
* [Remove a property](#sample-script-to-remove-a-property)

## Installation
Make sure you have [.Net 8 or higher installed](https://dotnet.microsoft.com/en-us/download). 
Open a terminal and run the following:

```bash
dotnet tool install --global CosmosCli
```

To make sure it is installed run the following:
```bash
cosmos --version
```

*Note*: If you already have it installed, you can upgrade to the latest version
by running the following: 
```bash
dotnet tool update --global CosmosCli
```

## Usage

To view a list of **commands** enter the following at the command prompt:

```bash
cosmos --help
```

```text
Usage: cosmos [command]

cosmos

Commands:
  account
  database
  container

Options:
  -h, --help    Show help message
  --version     Show version
```

Each of the commands have **sub-commands**. To view more details, use **--help** on the commands.  

```bash
cosmos container --help
```

```text
Usage: cosmos container [command]

cosmos

Commands:
  show
  new
  delete
  index
  select-item
  upsert-item
  delete-item

Options:
  -h, --help    Show help message
```

To get details on a **sub-command**, enter the **command** followed by the **sub-command** along with **--help** to get details of how to use the sub-command:

```bash
cosmos container upsert-item --help
```

```text
Usage: cosmos container upsert-item [--compress-json] [--show-stats] [--partition-key <String>] [--consistency-level <String>] [--exclude-region <String>...] [--post-triggers <String>...] [--pre-triggers <String>...] [--session-token <String>] [--container <String>] [--database <String>] [--endpoint <String>] [--key <String>] [--verbose] [--help] json-items

Arguments:
  0: json-items

Options:
  -j, --compress-json                 Compress the json output by removing whitespace and carriage returns.
  -s, --show-stats                    Show response statistics (including RUs, StatusCode, ContinuationToken).
  -p, --partition-key <String>        Specify the name of the partition key which is provided in the input json.
  -l, --consistency-level <String>    Consistency Level (Eventual, ConsistentPrefix, Session, BoundedStaleness, Strong)
  -r, --exclude-region <String>...    Exclude region(s) while querying
  --post-triggers <String>...         Set the names of the post trigger to be invoked after the upsert.
  --pre-triggers <String>...          Set the names of the pre trigger to be invoked after the upsert.
  --session-token <String>            Set a session token in the query request.
  -c, --container <String>            The name of the container that you are connecting to in Cosmos DB.
  -d, --database <String>             The name of the database that you are creating.
  -e, --endpoint <String>             The endpointUri used to connect to the Cosmos DB service
  -k, --key <String>                  The primary or secondary key used to connect to the Cosmos DB service.
  -v, --verbose                       Display more details of what is happening.
  -h, --help                          Show help message
```
## Environment Variables
You can specify the **endpoint**, **key**, **database** and **container** as environment variables. If you specify them as part of the command then the environment variables will be ignored.

You can set the following environment variables:
```
COSMOSDB_ENDPOINT
COSMOSDB_KEY
COSMOSDB_DATABASE
COSMOSDB_CONTAINER
```

## Config File
You can specify the **endpoint**, **key**, **database** and **container** in a **config.json** file. If environment variables are specified or if you specify them as part of the command, the config values will be ignored. 

The **config.json** file can exist in the current directory or globally in your HOME directory inside of a **.cosmos** directory.

Sample **config.json** file:

```json
{
	"Endpoint": "https://localhost:8081",
	"Key": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
	"Database": "TestDB",
	"Container": "Test"
}
```

## Commands
- account
    - [show](#account-show)
- database
    - [show](#database-show)
    - [new](#database-new)
    - [update](#database-update)
    - [delete](#database-delete)
- container
    - [show](#container-show)
    - [new](#container-new)
    - [delete](#container-delete)
    - [index](#container-index)
    - [select-item](#container-select-item)
    - [upsert-item](#container-upsert-item)
    - [delete-item](#container-delete-item)

## Account Show
This returns information about the specified Cosmos Account. This includes information about the databases and containers that are in the account.

```bash
cosmos account show
```
### Tips

**jq** can be used to pull out the account name
```bash
cosmos account show | jq .id
```

Or, the databases
```bash
cosmos account show | jq .databases[].id
```

Or, the containers
```bash
cosmos account show | jq .databases[].containers[].id
```

Or, JSON with just the names of the account, databases and containers.
```bash
cosmos account show | jq '{accountId: .id, databases: [{databaseId: .databases[].id, containers: [{containerId: .databases[].containers[].id}]}] }'
```

## Database Show
This returns information about a specified Cosmos database. This includes information about the database and containers.

```bash
cosmos database show -d TestDB
```

### Tips
JSON with just the database and containers.

```bash
cosmos database show -d TestDB | jq '{databaseId: .id, containers: [{containerId: .containers[].id}]}'
```

## Database New
This creates a database in a Cosmos account. The database can be created with manual or autoscaled RU/s. 

The following creates a database named **TestDB** without specifying any throughput RU/s. It is assumed that throughput RU/s will be specified on the containers. 

```bash
cosmos database new -d TestDB
```

The following creates a database with autoscaling RU/s. The value must be between 1000 and 1,000,000 RU/s and the output message will say that the RU/s is 10% of what you specified.

```bash
cosmos database new -d TestDB --autoscale 1000
```
 
The following creates a database with manual RU/s. The value must be between 400 and 1,000,000 RU/s and an increment of 100.

```bash
cosmos database new -d TestDB --manual 400 
```

## Database Update
This updates the RU/s on an existing database. If the database was not created with manual or autoscaled RU/s this command will fail. The database must have RU/s specified.


The following will change the autoscaled throughput to 2000. The value must be between 1000 and 1,000,000 RU/s and the output message will say that the RU/s is 10% of what you specified.
```bash
cosmos database new -d TestDB --autoscale 2000 
```

The following will change the manual throughput to 500. The value must be between 400 and 1,000,000 RU/s and an increment of 100.
```bash
cosmos database new -d TestDB --manual 500 
```

### Tips
Currently you can not change from manual to autoscale or autoscale to manual. This is a limitation of the Cosmos SDK.


## Database Delete
This removes a database. This will delete all containers created in the database, so use with caution. 

The following will delete the specified TestDB database.
```bash
cosmos database delete -d TestDB 
```

## Tips
Use with caution. All containers for the specified database will also be removed.

## Container Show
This returns information about a specified Cosmos container.

```bash
cosmos container show -d TestDB -c Container1
```

### Tips
**jq** can be use to get just the container name.

```bash
cosmos container show -d TestDB -c | jq '.id'
```

## Container New
This creates a new container in a Cosmos database. The partition key must be specified.

```bash
cosmos container new -d TestDB -c Container1 -p id
```

The following creates a container with autoscaling RU/s. The value must be between 1000 and 1,000,000 RU/s and the output message will say that the RU/s is 10% of what you specified.

```bash
cosmos container new -d TestDB -c Container1 -p id --autoscale 1000 
```

The following creates a container with manual RU/s. The value must be between 400 and 1,000,000 RU/s and an increment of 100.

```bash
cosmos container new -d TestDB -c Container1 -p id --manual 400 
```

## Container Delete

This removes a container. 

The following will delete the specified **Container1** in a **TestDB** database.

```bash
cosmos container delete -d TestDB -c Container1
```

## Container Index

This gets or sets an index on a container in a Cosmos database. 

The following will get the current index from a specified container in a specified database. 

```bash
cosmos container index -d TestDB -c Container1
```

The following will set the index for a specified container in a specified database. 

```bash
cosmos container index -d TestDB -c Container1 "{\"automatic\":true,\"indexingMode\":\"Consistent\",\"includedPaths\":[{\"path\":\"/*\"}],\"excludedPaths\":[{\"path\":\"/name/?\"}],\"compositeIndexes\":[[{\"path\":\"/familyname\",\"order\":\"ascending\"},{\"path\":\"/children/[]/age\",\"order\":\"descending\"}]],\"spatialIndexes\":[],\"vectorIndexes\": [],\"fullTextIndexes\":[]}"
```

The following will set the index for a specified container in a specified database for a giving index file. 

```bash
cosmos container index -d TestDB -c Container1 "c:\index.json"
```
### Tips

You can also pipe the index into the container index command. 

```bash
cat c:\index.json | cosmos container index -d TestDB -c Container1
```

## Container Select Item
This is used to select data from a container in a Cosmos database.

The following selects data from a container. Like the Azure portal, by default, only the first 100 documents will be returned.

```bash
cosmos container select-item -d TestDB -c Container1 "Select * from c"
```

The default max items returned is 100 with 1 enumeration. However, the max items returned can be changed along with the number of  enumerations. The following returns 200 items at a time and allows for 5 enumerations which will return up to 1000 items.

```bash
cosmos container select-item -d TestDB -c Container1 -n 200 -i 5 "Select * from c"
```

The following displays the RU charges, count, status code, activity id and continuation token per enumeration.

```bash
cosmos container select-item -d TestDB -c Container1 -s "Select * from c"
```

The following displays information about utilized indexes as well as suggests potential indexes that should be added. 

```bash
cosmos container select-item -d TestDB -c Container1 -m "Select * from c order by c.name"
```

The system generated properties (_rid, _self, _etag, _attachments, _ts and _lsn) can be removed by doing the following:

```bash
cosmos container select-item -d TestDB -c Container1 -_ "Select * from c order by c.name"
```

The query can also be piped into the command as follows:

```bash
cat query.txt | cosmos container select-item -d TestDB -c Container1 
```

When a query returns more data than can be processed at once, a continuation token file can be used to run a query again 
and return items for where it left off. The following will return the first 100 items it the first command and the
next 100 items in the second command. The specified file should not exist on the first call. The last call will set the
value in the file to an empty string. Running the command once the file contains a empty value will return and 
empty JSON array. Delete the file or specify a different file name to start querying from the beginning. 

```bash
cosmos container select-item -d TestDB -c Container1 -t continuationToken.txt "Select * from c"
cosmos container select-item -d TestDB -c Container1 -t continuationToken.txt "Select * from c"
```

## Container Upsert Item
This is used to update or insert documents into a container in a Cosmos database.


The following will upsert a single document in a container. Note that the partition key property must be specified and be in the JSON.
```bash
cosmos container upsert-item -d TestDB -c Container1 -p id '{\"id\":\"7\",\"name\":\"Bob\"}'   
```

The following will upsert multiple documents in a container. Note that the partition key property must be specified and be in the JSON.
```bash
cosmos container upsert-item -d TestDB -c Container1 -p id '[{\"id\":\"7\",\"name\":\"Bob\"},{\"id\":\"8\",\"name\":\"Bill\"}]'   
```

The following displays RU charges, status code, activity id, and etag of the items.
```bash
cosmos container upsert-item -d TestDB -c Container1 -s -p id '[{\"id\":\"7\",\"name\":\"Bob\"},{\"id\":\"8\",\"name\":\"Bill\"}]'   
```


The documents can also be piped into the command as follows:

```bash
cat docs.txt | cosmos container upsert-item -d TestDB -c Container1 -p id
```

## Container Delete Item
This is used to delete documents in a container in a Cosmos database.


The following will delete a single document in a container. Note that the partition key property must be specified and be in the JSON.
```bash
cosmos container delete-item -d TestDB -c Container1 -p id '{\"id\":\"8\"}'   
```

The following will delete multiple documents from  a container. Note that the partition key property must be specified and be in the JSON.
```bash
cosmos container delete-item -d TestDB -c Container1 -p id '[{\"id\":\"7\",\"name\":\"Bob\"},{\"id\":\"8\",\"name\":\"Bill\"}]'   
```

If the partition key is not **id**, both the **id** property and **partition key** must be specified in the JSON.
```bash
cosmos container delete-item -d TestDB -c Container2 -p pkey '{\"id\":\"11111\",\"pkey\":\"AAAAA\"}'
``` 

The following displays RU charges, status code, activity id, and etag of the items.
```bash
cosmos container delete-item -d TestDB -c Container1 -s -p id '[{\"id\":\"7\",\"name\":\"Bob\"},{\"id\":\"8\",\"name\":\"Bill\"}]'   
```

## Sample script to add a property

The following is a PowerShell script to add a **discount** property with a value of **0.02** to all the documents in a container. Note that the endpoint and key have been removed to make it shorter.   

```powershell
while (-not (Test-Path .\continuationToken.txt) -or (Get-Item .\continuationToken.txt).Length -ne 0) {
    $r = cosmos container select-item -d cosmicworks -c products -_ -t continuationToken.txt "Select * from c" |  jq '[.[] | .discount = 0.02]' | cosmos container upsert-item -d cosmicworks -c products -p categoryId
    Write-Output "Working..."
} 
Write-Output "Done"
```

This script loops until there is a **continuationToken.txt** file that is empty. The **continuationToken.txt** is created as part of the **cosmos container select-item** command by specifying the continuation token file. The main part of the script does the following:
   select -> transform -> upsert

The **select-item** is used to query the items in a container. 

```bash
cosmos container select-item -d cosmicworks -c products -_ -t continuationToken.txt "Select * from c"
```

The results of the **select-item** are piped to **jq** which is destructed and the **discount** property is added with a hardcoded value and the items are then wrapped into an array. 
```bash
jq '[.[] | .discount = 0.02]'
```

The results of **jq** are piped into the **upsert-item**. The database and container are the same as was used in the **select-item** command which will result in the same documents being updated. In this example, the container has a **categoryId** partition key.
```bash
 cosmos container upsert-item -d cosmicworks -c products -p categoryId
```

The result of the upsert are captured in the **$r** variable which could be used to output the updated documents to the console.


## Sample script to update properties


The following is a PowerShell script to update a **price** property and the **name** property of all the documents in a container. Note that the endpoint and key have been removed to make it shorter.   

```powershell
while (-not (Test-Path .\continuationToken.txt) -or (Get-Item .\continuationToken.txt).Length -ne 0) {
    $r = cosmos container select-item -d cosmicworks -c products -_ -t continuationToken.txt "Select * from c" | jq '. | map(.price*=0.02) | map(.name+=\" - test\")' | cosmos container upsert-item -d cosmicworks -c products -p categoryId
    Write-Output "Working..."
} 
Write-Output "Done"
```

This script loops until there is a **continuationToken.txt** file that is empty. The **continuationToken.txt** is created as part of the **cosmos container select-item** command by specifying the continuation token file. The main part of the script does the following:
   select -> transform -> upsert

The **select-item** is used to query the items in a container. 

```bash
cosmos container select-item -d cosmicworks -c products -_ -t continuationToken.txt "Select * from c"
```

The results of the **select-item** are piped to **jq** which is destructed and the **price** is multiplied by 0.02 and **" - test"** is appended to the end of the **name** property. 
```bash
jq '. | map(.price*=0.02) | map(.name+=\" - test\")'
```

The results of **jq** are piped into the **upsert-item**. The database and container are the same as was used in the **select-item** command which will result in the same documents being updated. In this example, the container has a **categoryId** partition key.
```bash
 cosmos container upsert-item -d cosmicworks -c products -p categoryId
```

The result of the upsert are captured in the **$r** variable which could be used to output the updated documents to the console.


## Sample Script to Remove a Property

The following is a PowerShell script which removes a **discount** property from all the documents in a container. Note that the endpoint and key have been removed to make it shorter.   

```powershell
while (-not (Test-Path .\continuationToken.txt) -or (Get-Item .\continuationToken.txt).Length -ne 0) {
    $r = cosmos container select-item -d cosmicworks -c products -_ -t continuationToken.txt "Select * from c" |  jq '. | map(del(.discount))' | cosmos container upsert-item -d cosmicworks -c products -p categoryId
    Write-Output "Working..."
} 
Write-Output "Done"
```

This script loops until there is a **continuationToken.txt** file that is empty. The **continuationToken.txt** is created as part of the **cosmos container select-item** command by specifying the continuation token file. The main part of the script does the following:
   select -> transform -> upsert

The **select-item** is used to query the items in a container. 

```bash
cosmos container select-item -d cosmicworks -c products -_ -t continuationToken.txt "Select * from c"
```

The results of the **select-item** are piped to **jq** which deletes the **discount** property from each of the JSON items in the array. 
```bash
 jq '. | map(del(.discount))'
```

The results of **jq** are piped into the **upsert-item**. The database and container are the same as was used in the **select-item** command which will result in the same documents being updated. In this example, the container has a **categoryId** partition key.
```bash
 cosmos container upsert-item -d cosmicworks -c products -p categoryId
```

The result of the upsert are captured in the **$r** variable which could be used to output the updated documents to the console.

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