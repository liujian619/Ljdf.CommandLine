using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 用于创建命令行帮助信息。
	/// </summary>
	public sealed class HelpBuilder
	{
		internal HelpBuilder(ParserConfig config)
		{
			_config = config;
		}

		private readonly ParserConfig _config;
		private const string Space = " ";
		private static readonly string NL = Environment.NewLine;

		private static Func<ParserConfig, string> _preBuild;
		private static Func<ParserConfig, string> _postBuild;

		private static readonly IDictionary<Type, string> _typeStrings = new Dictionary<Type, string> 
		{
			{ typeof(string), "string" },
			{ typeof(bool), "boolean" },
			{ typeof(float), "real" },
			{ typeof(double), "real" },
			{ typeof(decimal), "real" },
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(FileInfo), "FILE" },
			{ typeof(DirectoryInfo), "DIR" },
			{ typeof(Switch), "on|off" }
		};

		/// <summary>
		/// 注册类型对应的描述文本。
		/// </summary>
		/// <typeparam name="T">类型。</typeparam>
		/// <param name="text">描述文本。</param>
		public static void RegisterTypeString<T>(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				_typeStrings[typeof(T)] = text;
			}
		}

		/// <summary>
		/// 设置前置委托，该委托返回一个字符串，该字符串会显示在构建的帮助文档核心内容的前面。
		/// </summary>
		/// <param name="preBuild">一个委托。</param>
		public static void SetPreBuild(Func<ParserConfig, string> preBuild)
		{
			_preBuild = preBuild;
		}

		/// <summary>
		/// 设置后置委托，该委托返回一个字符串，该字符串会显示在构建的帮助文档核心内容的后面。
		/// </summary>
		/// <param name="postBuild">一个委托。</param>
		public static void SetPostBuild(Func<ParserConfig, string> postBuild)
		{
			_postBuild = postBuild;
		}


		private static string _assemblyName;
		/// <summary>
		/// 获取程序名称。
		/// </summary>
		public static string AssemblyName
		{
			get
			{
				if (_assemblyName is null)
				{
					_assemblyName = GetAssembly().GetName().Name ?? string.Empty;
				}
				return _assemblyName;	
			}
		}

		private static string _copyright;
		/// <summary>
		/// 获取版权信息。
		/// </summary>
		public static string Copyright
		{
			get
			{
				if (_copyright is null)
				{
					var assembly = GetAssembly();
					var attr = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
					_copyright = attr?.Copyright ?? string.Empty;
				}
				return _copyright;
			}
		}

		private static string _version;
		/// <summary>
		/// 获取版本号。
		/// </summary>
		public static string Version
		{
			get
			{
				if (_version is null)
				{
					var assembly = GetAssembly();
					var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
					_version = attr != null ? attr.InformationalVersion : assembly.GetName().Version.ToString();
				}
				return _version;
			}
		}

		private static string _description;
		/// <summary>
		/// 获取描述信息。
		/// </summary>
		public static string Description
		{
			get
			{
				if (_description is null)
				{
					var assembly = GetAssembly();
					var attr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
					_description = attr?.Description ?? string.Empty;
				}
				return _description;
			}
		}



		/// <summary>
		/// 构建帮助文档。
		/// </summary>
		/// <param name="commands">命令选项集合。</param>
		public string Build(IEnumerable<CommandDescriptor> commands)
		{
			if (commands is null || !commands.Any())
			{
				return FormatContent(null, false);
			}

			commands = commands.OrderByDescending(c => c.IsTopCommand).ThenBy(c => c.Order);
			var sb = new StringBuilder();

			// 1. 描述
			if (!string.IsNullOrEmpty(Description))
			{
				sb.AppendLine(Resource.Help_Description + Resource.Help_Colon);
				sb.AppendLine(Tab(Description));
			}

			// 2. 使用
			sb.AppendLine();
			sb.AppendLine(Resource.Help_Usage + Resource.Help_Colon);
			string app = AssemblyName.ToLower();
			var usages = Usages(app, commands);
			int count = usages.Count();
			for (int i = 0; i < count; i++)
			{
				sb.AppendLine(usages.ElementAt(i));
				if (i < count - 1)
				{
					sb.AppendLine();
				}
			}

			// 3. 选项
			sb.AppendLine();
			sb.AppendLine(Resource.Help_Option + Resource.Help_Colon);
			var options = Options(commands.SelectMany(c => c.Options));
			foreach (var opt in options)
			{
				sb.AppendLine(opt);
			}

			return FormatContent(sb.ToString().TrimEnd(), true);
		}

		/// <summary>
		/// 格式化内容。
		/// </summary>
		/// <param name="content">内容。</param>
		public string FormatContent(string content)
		{
			return FormatContent(content ?? string.Empty, false);
		}


		/// <summary>
		/// 构建错误信息。
		/// </summary>
		/// <param name="errorMessage">错误信息。</param>
		public string Error(string errorMessage)
		{
			return FormatContent(Resource.Help_Error + Resource.Help_Colon + errorMessage ?? string.Empty, false);
		}

		
		/************************************************************************
		 * 格式：
		 * {程序名} {版本号}
		 * {版权信息}
		 * 
		 * {自定义前置内容}
		 * 
		 * {内容}
		 * 
		 * {自定义后置内容}
		 * 
		 * {帮助提示}
		 ************************************************************************/
		private string FormatContent(string content, bool needPreOrPostContent)
		{
			var sb = new StringBuilder();

			// {程序名} {版本号}
			sb.AppendLine($"{AssemblyName} {Version}");

			// {版权信息}
			string cr = Copyright;
			if (!string.IsNullOrEmpty(cr))
			{
				sb.AppendLine(cr);
			}

			// {自定义前置内容}
			if (needPreOrPostContent && _preBuild != null)
			{
				string text = _preBuild.Invoke(_config);
				if (!string.IsNullOrEmpty(text))
				{
					sb.AppendLine();
					sb.AppendLine(text);
				}
			}

			// {内容}
			if (!string.IsNullOrEmpty(content))
			{
				sb.AppendLine();
				sb.AppendLine(content);
			}

			// {自定义后置内容}
			if (needPreOrPostContent && _postBuild != null)
			{
				string text = _postBuild.Invoke(_config);
				if (!string.IsNullOrEmpty(text))
				{
					sb.AppendLine();
					sb.AppendLine(text);
				}
			}

			// {帮助提示}
			if (!_config.PreventDefaultHelp)
			{
				sb.AppendLine();
				sb.Append(Resource.SeeHelp);
			}

			return sb.ToString();
		}

		private static IEnumerable<string> Usages(string app, IEnumerable<CommandDescriptor> commands) 
		{
			foreach (var cmd in commands)
			{
				var list = new List<string> { app };

				if (!cmd.IsTopCommand)
				{
					list.Add(cmd.Name);
				}

				var options = Order(cmd.Options);
				int total = options.Count();
				int spaces = Tab(string.Join(Space, list)).Length;
				for (int i = 0; i < total; i++)
				{
					list.Add(GetOptionInUsage(options.ElementAt(i), total));
					if (i < total - 1)
					{
						list.Add(NL + new string(' ', spaces));
					}
				}

				string result = Tab(string.Join(Space, list));
				if (!string.IsNullOrEmpty(cmd.Description))
				{
					result = Tab(cmd.Description) + NL + result;
				}

				yield return result;
			}
		}

		private static IEnumerable<string> Options(IEnumerable<OptionDescriptor> options)
		{
			options = Order(options);

			var list = new List<KeyValuePair<string, string>>();
			var names = new HashSet<string>(); // 相同的选项名称只显示一个
			foreach (var opt in options)
			{
				string shortName = opt.ShortName?.ToString();
				string longName = opt.LongName;
				if (opt.Hidden || (shortName != null && names.Contains(shortName)) || (longName != null && names.Contains(longName)))
				{
					continue;
				}

				if (shortName != null) { names.Add(shortName); }
				if (longName != null) { names.Add(longName); }

				string key = string.Join(", ", GetOptionNames(opt)) + $" {GetTypeString(opt)}";
				string value = "";
				if (!string.IsNullOrEmpty(opt.Description))
				{
					value += opt.Description;
				}
				if (opt.Optional)
				{
					value += Resource.Help_Comma + Resource.Help_Optional;
					if (opt.OptionalValue != null)
					{
						value += $"{Resource.Help_Comma}{Resource.Help_OptionalValue}{Resource.Help_Colon}{opt.OptionalValue}";
					}
				}
				
				list.Add(new KeyValuePair<string, string>(key, value));
			}

			// 选项最大长度
			int maxLength = list.Max(o => o.Key.Length);
			foreach (var item in list)
			{
				yield return Tab(item.Key.PadRight(maxLength, ' ') + Tab(item.Value));
			}
		}

		private static IEnumerable<OptionDescriptor> Order(IEnumerable<OptionDescriptor> options)
		{
			return options.OrderByDescending(p => p.IsDefault).ThenBy(p => p.Optional).ThenBy(p => p.Order);
		}

		private static string GetOptionInUsage(OptionDescriptor opt, int totalOptions)
		{
			string[] names = GetOptionNames(opt);
			string result = string.Join(" | ", names);

			if (names.Length > 1 && (!opt.IsNoneValueOption || totalOptions > 1))
			{
				result = $"({result})";
			}

			result += $" {GetTypeString(opt)}";

			if (opt.Optional)
			{
				result = $"[{result}]";
			}

			return result;
		}

		private static string[] GetOptionNames(OptionDescriptor opt)
		{
			var names = new List<string>();
			if (opt.ShortName.HasValue)
				names.Add(Parser.ShortHypen + opt.ShortName.Value);
			if (opt.LongName != null)
				names.Add(Parser.LongHypen + opt.LongName);
			return names.ToArray();
		}

		// 获取命令或选项的参数值类型说明
		private static string GetTypeString(OptionDescriptor opt)
		{
			_typeStrings.TryGetValue(opt.Type, out string name);
			name = name ?? opt.Type.Name;

			if (opt.IsSingleValueOption)
				return $"<{name}>";
			else if (opt.IsMultipleValueOption)
				return $"<{name}>{DescribeCount(opt.MinCount.Value, opt.MaxCount.Value)}";
			else // 无参选项，没有参数值，自然不需要参数类型
				return string.Empty;
		}

		// 描述数目
		private static string DescribeCount(int min, int max)
		{
			var sb = new StringBuilder();
			sb.Append('{');
			sb.Append(min);
			if (min < max)
			{
				sb.Append(',');
			}
			if (max < int.MaxValue)
			{
				sb.Append(max);
			}
			sb.Append('}');

			return sb.ToString();	
		}

		// 在字符串开头添加 4 个空格
		private static string Tab(string text)
		{
			return $"    {text}";
		}


		private static Assembly GetAssembly()
		{
			return Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
		}
	}
}
