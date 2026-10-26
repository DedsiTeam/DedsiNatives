        if (id == default)
            throw new ArgumentException("主键不能为 Guid.Empty。", nameof(id));
