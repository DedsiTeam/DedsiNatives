        Options(builder => builder.RequireAuthorization(
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("permission", "{{Permission}}")
                .Build()));
