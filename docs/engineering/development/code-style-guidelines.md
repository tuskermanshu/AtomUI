# AtomUI 代码风格规范

本文定义 AtomUI 仓库中跨模块通用的代码风格约束。命名规则仍以仓库根目录的
[`.editorconfig`](../../../.editorconfig) 为机器可执行事实来源；本文补充必须由开发者和 AI agent 在编写、
修改与审查代码时主动遵守的结构性规则。

## C# 控制流块体

所有 C# 控制流语句只要语法允许使用块体，就必须显式写出 `{}`，即使主体只有一行也不得省略。

适用范围包括但不限于：

- `if`、`else if`、`else`
- `for`、`foreach`、`while`、`do`
- `using` 语句、`lock`、`fixed`
- `checked`、`unchecked`、`unsafe`

正确写法：

```csharp
if (isEnabled)
{
    UpdateState();
}
```

禁止写法：

```csharp
if (isEnabled)
    UpdateState();
```

## Agent 完成门禁

AI agent 在任何任务中新增或修改 C# 代码时，都必须满足本规范。即使本次只新增或修改一行代码，也不得省略控制流
块体的大括号。

如果 agent 触及的代码片段、相邻上下文或同一编辑区域中存在可见的省略大括号控制流语句，必须在本次改动中一并
修正。未完成修正、未说明更大范围存量问题，或在新增代码中继续省略 `{}`，均不算任务完成。

表达式主体成员、switch expression、lambda expression、`using` declaration 等语法上不是控制流语句块体的形式，不受
本条约束；不得以这些例外为理由省略普通控制流语句的大括号。
