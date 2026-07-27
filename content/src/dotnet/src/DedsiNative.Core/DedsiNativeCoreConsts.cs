using Dedsi.CleanArchitecture.Domain;

namespace DedsiNative;

/// <summary>
/// DedsiNative 核心常量定义，继承自领域层公共常量基类。
/// </summary>
public class DedsiNativeCoreConsts: DedsiCleanArchitectureDomainConsts
{
    /// <summary>
    /// 应用程序名称。
    /// </summary>
    public const string ApplicationName = "DedsiNative";
    
    /// <summary>
    /// 移动端应用程序名称。
    /// </summary>
    public const string MobileApplicationName = "DedsiNative.Mobile";
    
    /// <summary>
    /// 数据库连接字符串名称，用于从配置中读取对应的连接字符串。
    /// </summary>
    public const string ConnectionStringName = "DedsiNativeDB";
    
    /// <summary>
    /// 数据库 Schema 名称。
    /// </summary>
    public const string DbSchemaName  = "DedsiNative";

    /// <summary>
    /// 数据库表名前缀。
    /// </summary>
    public const string DbTablePrefix = "DedsiNative";
}