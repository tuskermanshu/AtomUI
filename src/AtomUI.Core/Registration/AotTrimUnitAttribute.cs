using System;

namespace AtomUI.Registration;

/// <summary>
/// 声明类型及其所在文件的注册代码归属的 AOT 裁剪单元，覆盖 Directory 粒度下按目录推导的单元归属。
/// 仅对声明了 <c>AtomUIRegistrationGranularity=Directory</c> 的控件包生效；被注解文件内的调用不再按目录
/// 归入控件族 Unit。与 <c>AtomUIRegistrationUnit</c> 编译项元数据同时存在且声明不一致时构建期报告冲突。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AotTrimUnitAttribute : Attribute
{
    /// <param name="unitName">目标 Registration Unit 名称，由包 id 限定；通用单元使用 <see cref="AotTrimGeneralUnits"/> 常量。</param>
    public AotTrimUnitAttribute(string unitName)
    {
        UnitName = unitName;
    }

    /// <summary>目标 Registration Unit 名称。</summary>
    public string UnitName { get; }
}

/// <summary>
/// 跨控件族的通用 AOT 裁剪单元名称常量。控件族 Unit 由稳定控件族目录派生，不在此登记。
/// </summary>
public static class AotTrimGeneralUnits
{
    /// <summary>包级共享核心：不归属任何控件族 Unit，随 Package 入口保留。</summary>
    public const string Core = "Core";
}
