        builder.Property(x => x.CreationTime).HasColumnType("timestamp without time zone").IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatorName).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(40).IsRequired(false).IsConcurrencyToken();
