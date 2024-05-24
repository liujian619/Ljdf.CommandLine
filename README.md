# Ljdf.CommandLine

主要用于命令行参数解析，同时可以生成命令行参数的帮助文档。 

Mainly used for command-line argument parsing, while also capable of generating help documentation for command-line arguments.

<<<<<<< HEAD
可以通过 nuget 使用：[![1.0.1](https://img.shields.io/nuget/v/Ljdf.CommandLine?style=flat)](https://www.nuget.org/packages/Ljdf.CommandLine)
=======
可以通过 nuget 使用：![1.0.1](https://www.nuget.org/packages/Ljdf.CommandLine?style=flat)
>>>>>>> 3c6f22659736bb8f84b7707ac7133a706e6bc849



## 示例（Example）

创建一个处理器“JoinHandler”（Create a Handler "JoinHandler"）

```cs
[Command("join", "合并字符串")]
class JoinHandler : Handler 
{
    [MultipleValueOption('i', "一个或多个输入字符串", Order = 1)]
    public IEnumerable<string> InputStrings { get; set; }

    [SingleValueOption('separator', "分隔字符", Order = 2)]
    public char Separator { get; set; }

    [NoneValueOption("newline", "输入字符串之间插入换行符", Order = 3)]
    public bool IsDtsDoc { get; set; }
}
```

使用“JoinHandler”（Use the "JoinHandler"）

```cs
internal class Program
{
    static void Main(string[] args)
    {
        Ljdf.CommandLine.Parser.Create()
            .AddHanlder<JoinHandler>()
            .Run(args);
    }
}
```

## 处理器约束

一个有效的处理程序需要满足：

1. 处理程序类必须应用了 `CommandAttribute` 特性
2. 类中的属性需要应用 `(NoneValue|SingleValue|MultipleValue)OptionAttribute` 特性
3. 只能有一个属性所应用的 `OptionAttribute` 特性的 `IsDefault` 属性为 true
4. `NoneValueOptionAttribute` 特性只能应用到 bool 类型的属性
5. `MultipleValueOptionAttribute` 特性只能应用到 `IEnumerable<>` 类型的属性
6. `MultipleValueOptionAttribute` 特性的 `MaxCount` 属性值必须大于等于 1
7. `MultipleValueOptionAttribute` 特性的 `MinCount` 属性值必须大于等于 1
8. `MultipleValueOptionAttribute` 特性的 `MinCount` 属性值不能大于 `MaxCount` 属性值
9. 对于同一个命令，短名称与短名称之间、短名称与长名称之间、长名称与长名称之间 均不能重复

## 命名约束

### 子命令名称

1. 必须是字母、数字、连接符(-)
2. 长度大于等于 1
3. 第一个字符必须是字母

### 选项短名称

必须是单独的一个字母

### 选项长名称

1. 必须是字母、数字、连接符(-)
2. 长度大于等于 2
3. 第一个字符必须是字母


## 命令或选项描述

可以通过 `XXXAttribte` 中的 `Description` 属性来设置命令或选项的描述文本。

该属性也可以设置为资源键的值，只需要指定 `ResourceType`，便可通过资源键来获取对应的文本。这么做的目的是为了多语言的本地化。

有三个地方可以指定 `ResourceType`，优先级由高到低分别为：

* 通过选项对应的属性上的特性中的 `ResourceType` 属性指定；
* 通过选项对应的属性所在的类上的特性中的 `ResourceType` 属性指定；
* 通过 `ParseConfig` 中的 `DefaultResourceType` 属性指定。

## 帮助文档

默认实现了命令及选项得帮助说明文档，通过选项 `-?` 或 `-h` 或 `--help` 可以查看，大致格式如下：

```
{程序名} {版本号}
{版权信息}

{自定义前置内容}

{内容}

{自定义后置内容}

{帮助提示}
```

* {程序名}、{版本号}、{版权信息} 在项目信息中指定（如：`Microsoft.NET.Sdk` 风格下的 `csproj` 文件或 `Assembly.cs` 类）
* {自定义前置内容} 和 {自定义后置内容} 通过 `HelpBuilder.SetPreBuild` 和 `HelpBuilder.SetPostBuild` 方法指定
* {内容} 就是将添加的各个 `Handler` 及其属性的信息按一定格式展现出来，可以通过 `Hidden` 隐藏，通过 `Order` 设置显示顺序，值越小越靠前
* {帮助提示} 是提示使用“-?”选项查看帮助文档


## 选项说明

参数选项可能为：
1. 一个连接符(-)后接一个字母：-f  -n
2. 两个连接符(--)后接长度至少为 2 的字符串：--func  --name
3.  一个连接符(-)后接多个字母：-abc <=> -a -b -c

## 本地化

提供了中英两种资源，默认采用中文。可以提供下面方法切换

```cs
System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us", false);
System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us", false);
```

或者

```cs
CultureInfo enUSCulture = new CultureInfo("en-us");
CultureInfo.DefaultThreadCurrentCulture = enUSCulture;
CultureInfo.DefaultThreadCurrentUICulture = enUSCulture;
```

**更多更细的功能请查看源码**
