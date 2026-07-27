var builder = DistributedApplication.CreateBuilder(args);

var macPath = "/Users/cohen/Dockers";

#region 密钥
var postgresPassword = builder.AddParameter("PostgresPassword", secret: true);
#endregion

#region 基础设施
var postgres = builder.AddPostgres("dedsinative-postgres", password: postgresPassword, port: 10812)
    .WithDataBindMount(source: macPath + "/PostgreSql/DedsiNativeDB")
    .WithLifetime(ContainerLifetime.Persistent);

var dedsiNativeDB = postgres.AddDatabase("DedsiNativeDB");
#endregion

builder
    .AddProject<Projects.DedsiNative_Host>("dedsinative-host")
    .WithReference(dedsiNativeDB)
    .WaitFor(dedsiNativeDB);

builder.Build().Run();
