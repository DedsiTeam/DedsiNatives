# EF Core 迁移流程

## 生成迁移

从包含 `DedsiIdentity.slnx` 的 .NET 根目录执行：

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/DedsiIdentity.Infrastructure `
  --startup-project src/DedsiIdentity.Host `
  --context DedsiIdentityDbContext `
  --output-dir EntityFrameworkCore/Migrations
```

## 更新数据库

仅在用户明确要求时执行：

```powershell
dotnet ef database update `
  --project src/DedsiIdentity.Infrastructure `
  --startup-project src/DedsiIdentity.Host `
  --context DedsiIdentityDbContext
```

## 检查生成结果

- 确认 Up 和 Down 互相对应。
- 确认表名、Schema、字段类型、长度、可空性和默认值符合模型。
- 确认没有意外删除表或字段。
- 确认没有因为重命名识别失败而生成“删除后重建”。
- 确认 ModelSnapshot 只包含预期模型变化。
- 不直接编辑生成文件来掩盖错误；返回实体映射修正后重新生成迁移。
