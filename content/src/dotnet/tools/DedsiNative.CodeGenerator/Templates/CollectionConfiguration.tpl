        {{Builder}}.HasMany(x => x.{{Name}}).WithOne()
            .HasForeignKey("{{ForeignKey}}").IsRequired().OnDelete(DeleteBehavior.Restrict);
        // 聚合仓储读取时一并加载明细；只做投影的查询可显式 IgnoreAutoIncludes。
        {{Builder}}.Navigation(x => x.{{Name}}).AutoInclude();
