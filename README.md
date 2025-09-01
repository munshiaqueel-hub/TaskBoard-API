# TaskBoard-API
TaskBoard-API

Please run these commands before running app:

dotnet user-secrets init

dotnet user-secrets set "Azure:Blob:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=taskgroupstorage;AccountKey=Dfe8mdzavvMwfsgIn2zGSt4IVST4Ml+HOhf2WvkIqrHMA148/bo2GA7Kmnww8QcFNyZVQM5fHqEX+AStAWjI0w==;EndpointSuffix=core.windows.net"

dotnet user-secrets set "Azure:DB:ConnectionString" "Server=tcp:taskboard-db-server.database.windows.net,1433;Initial Catalog=taskboard-db;Persist Security Info=False;User ID=sa1;Password=saUser@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

dotnet user-secrets set "Jwt:Key" "c4vdkg8OtND/KqVwTs5blIoKs54ryidV0EMdrk2gQU8="
