var builder = DistributedApplication.CreateBuilder(args);

var macPath = "/Users/cohen/Dockers";

#region 密钥
var postgresPassword = builder.AddParameter("PostgresPassword", secret: true);
var rabbitMqUserName = builder.AddParameter("RabbitMqUserName", secret: false);
var rabbitMqPassword = builder.AddParameter("RabbitMqPassword", secret: true);
#endregion

#region 基础设施
var postgres = builder.AddPostgres("dedsinative-postgres", password: postgresPassword, port: 10812)
    .WithDataBindMount(source: macPath + "/PostgreSql/DedsiNativeDB")
    .WithLifetime(ContainerLifetime.Persistent);

var dedsiNativeDB = postgres.AddDatabase("DedsiNativeDB");

// RabbitMQ 作为 ABP 分布式事件总线的传输层。持久容器保留本地开发期间的队列与消息。
var rabbitMq = builder.AddRabbitMQ("DedsiNativeRabbitMQ", rabbitMqUserName, rabbitMqPassword, port: 11424)
    .WithManagementPlugin(port: 15672)
    .WithLifetime(ContainerLifetime.Persistent);
#endregion

builder
    .AddProject<Projects.DedsiNative_Host>("dedsinative-host")
    .WithReference(dedsiNativeDB)
    .WithReference(rabbitMq)
    .WaitFor(dedsiNativeDB)
    .WaitFor(rabbitMq);

builder.Build().Run();
