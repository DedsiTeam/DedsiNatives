using Dedsi.CleanArchitecture.Domain;
using Volo.Abp.Modularity;

namespace DedsiNative;

/// <summary>
/// DedsiNative 核心层模块，负责注册领域层所需的基础依赖。
/// </summary>
[DependsOn(
    typeof(DedsiCleanArchitectureDomainModule)    
)]
public class DedsiNativeCoreModule : AbpModule;