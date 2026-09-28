var builder = DistributedApplication.CreateBuilder(args);

var macPath = "/Users/cohen/Dockers";

#region 密钥
var postgresUserName = builder.AddParameter("DedsiCohenPostgresUserName", secret: false);
var postgresPassword = builder.AddParameter("DedsiCohenPostgresPassword", secret: true);

var rabbitMqUserName = builder.AddParameter("DedsiCohenRabbitMqUserName", secret: false);
var rabbitMqPassword = builder.AddParameter("DedsiCohenRabbitMqPassword", secret: true);

var minioUserName = builder.AddParameter("DedsiCohenMinioUserName", secret: false);
var minioPassword = builder.AddParameter("DedsiCohenMinioPassword", secret: true);

var redisPassword = builder.AddParameter("DedsiCohenRedisPassword", secret: true);
var seqAdminPassword = builder.AddParameter("SeqAdminPassword", secret: true);
#endregion

#region 基础设施
var postgres = builder.AddPostgres("DedsiCohenPostgres", userName: postgresUserName, password: postgresPassword, port: 10812)
    .WithEnvironment("TZ", "Asia/Shanghai")
    .WithArgs("-c", "timezone=Asia/Shanghai")
    .WithDataBindMount(source: macPath + "/PostgreSql/DedsiNativeDB")
    .WithLifetime(ContainerLifetime.Persistent);

var dedsiNativeDB = postgres.AddDatabase("DedsiNativeDB");

// RabbitMQ 作为 ABP 分布式事件总线的传输层。持久容器保留本地开发期间的队列与消息。
var rabbitMq = builder.AddRabbitMQ("DedsiCohenRabbitMQ", userName: rabbitMqUserName, password: rabbitMqPassword, port: 14321)
    .WithManagementPlugin(port: 15672)
    .WithLifetime(ContainerLifetime.Persistent);

// MinIO 作为对象存储驱动
var minio = builder.AddMinioContainer("DedsiCohenMinio", rootUser: minioUserName, rootPassword: minioPassword, port: 11629)
    .WithDataBindMount(source: macPath + "/Minio/Data")
    .WithLifetime(ContainerLifetime.Persistent);

// Redis 为业务服务和认证服务提供共享的分布式缓存。
var redis = builder.AddRedis("DedsiCohenRedis", password: redisPassword, port: 16379)
    .WithDataBindMount(source: macPath + "/Redis/Data")
    .WithLifetime(ContainerLifetime.Persistent);

// Seq 持久化结构化日志；生产环境使用独立部署的日志服务。
var seq = builder.AddSeq("seq", seqAdminPassword, port: 15341)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .ExcludeFromManifest();
#endregion

builder
    .AddProject<Projects.DedsiNative_AuthServer>("dedsinative-authserver")
    .WithEnvironment("TZ", "Asia/Shanghai")
    .WithReference(dedsiNativeDB)
    .WithReference(rabbitMq)
    .WithReference(minio)
    .WithReference(redis)
    .WithReference(seq)
    .WaitFor(dedsiNativeDB)
    .WaitFor(rabbitMq)
    .WaitFor(minio)
    .WaitFor(redis)
    .WaitFor(seq);

builder
    .AddProject<Projects.DedsiNative_Host>("dedsinative-host")
    .WithEnvironment("TZ", "Asia/Shanghai")
    .WithReference(dedsiNativeDB)
    .WithReference(rabbitMq)
    .WithReference(minio)
    .WithReference(redis)
    .WithReference(seq)
    .WaitFor(dedsiNativeDB)
    .WaitFor(rabbitMq)
    .WaitFor(minio)
    .WaitFor(redis)
    .WaitFor(seq);

builder.Build().Run();
