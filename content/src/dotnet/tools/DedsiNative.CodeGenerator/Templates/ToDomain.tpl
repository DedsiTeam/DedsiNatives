    /// <summary>
    /// 从请求构造领域对象；实体标识由服务端生成。
    /// </summary>
    public static {{Type}} ToDomain({{RequestType}} request{{IdParameter}})
    {
        ArgumentNullException.ThrowIfNull(request);
        return new {{Type}}({{Arguments}});
    }
