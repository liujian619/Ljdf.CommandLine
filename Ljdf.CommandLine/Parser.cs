using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 解析器。
	/// </summary>
	public class Parser
	{
		private Parser(ParserConfig config)
		{
			_config = config;
			_helpBuilder = new HelpBuilder(config);
			_optionNames = new HashSet<string>(Helpers.GetComparer(config.IgnoreCase));
		}

		/// <summary>
		/// 创建 <see cref="Parser"/> 对象。
		/// </summary>
		/// <returns>返回 <see cref="Parser"/> 对象。</returns>
		public static Parser Create()
		{
			return Create(null);
		}

		/// <summary>
		/// 创建 <see cref="Parser"/> 对象。
		/// </summary>
		/// <param name="configAction">一个委托，用于配置 <see cref="ParserConfig"/> 对象中各属性的值。</param>
		/// <returns>返回 <see cref="Parser"/> 对象。</returns>
		public static Parser Create(Action<ParserConfig> configAction)
		{
			var cfg = new ParserConfig();
			configAction?.Invoke(cfg);

			return new Parser(cfg);
		}


		internal const string ShortHypen = "-";
		internal const string LongHypen = "--";

		private static readonly IDictionary<Type, Type> _typeConverters = new Dictionary<Type, Type>
		{
			{ typeof(FileInfo), typeof(FileInfoConverter) },
			{ typeof(DirectoryInfo), typeof(DirectoryInfoConverter) },
			{ typeof(Encoding), typeof(EncodingConverter) },
		};

		private readonly ParserConfig _config;
		private readonly HelpBuilder _helpBuilder;
		private readonly List<HandlerMetadata> _handlerMetadatas = new List<HandlerMetadata>();
		private readonly HashSet<string> _optionNames;
		private TextWriter _output;
		private Action<HandleContext> _matchFailedCallback;

		private TextWriter Output => _output ?? Console.Out;
		private ParserConfig Config => _config;
		
		/// <summary>
		/// 获取 <see cref="CommandLine.HelpBuilder"/> 对象。
		/// </summary>
		public HelpBuilder HelpBuilder => _helpBuilder;


		/// <summary>
		/// 注册指定类型的类型转换器。
		/// </summary>
		/// <remarks>该转换器用于将值从 <see cref="string"/> 类型转换到 <typeparamref name="T"/> 类型。</remarks>
		/// <typeparam name="T">经转换器转换之后的目标类型。</typeparam>
		/// <typeparam name="C">转换器类型。</typeparam>
		public static void RegisterTypeConverter<T, C>() where C : TypeConverter, new()
		{
			_typeConverters[typeof(T)] = typeof(C);
		}

		internal static TypeConverter GetTypeConverter(Type type)
		{
			if (_typeConverters.TryGetValue(type, out Type converterType))
				return Activator.CreateInstance(converterType) as TypeConverter;
			else
				return null;
		}


		/// <summary>
		/// 添加处理程序。
		/// </summary>
		/// <typeparam name="T">处理程序的类型。</typeparam>
		/// <returns>返回 <see cref="Parser"/> 对象。</returns>
		public Parser AddHandler<T>() where T : Handler, new()
		{
			var type = typeof(T);
			if (_handlerMetadatas.All(o => o.HandlerType != type))
			{
				CheckHandler(type, _optionNames);
				_handlerMetadatas.Add(HandlerMetadata.Create<T>());
			}
			else
			{
				throw new InvalidOperationException(Resource.HandlerDuplicated.CCFormat(type));
			}

			return this;
		}

		/// <summary>
		/// 设置输出流。
		/// </summary>
		/// <param name="output">输出流。</param>
		/// <returns>返回 <see cref="Parser"/> 对象。</returns>
		public Parser SetOutput(TextWriter output)
		{
			_output = output;
			return this;
		}

		/// <summary>
		/// 自定义匹配失败的处理方法。
		/// </summary>
		/// <param name="callback">一个委托，用于匹配失败后的处理。</param>
		/// <returns>返回 <see cref="Parser"/> 对象。</returns>
		public Parser MatchFailed(Action<HandleContext> callback)
		{
			_matchFailedCallback = callback;
			return this;
		}

		/// <summary>
		/// 解析命令行参数，并执行相应的处理程序。
		/// </summary>
		/// <param name="args">命令行参数。</param>
		public void Run(string[] args) 
		{
			var metadatas = new List<HandlerMetadata>();
			if (!Config.DisableHelpHandler)
			{
				metadatas.Add(HandlerMetadata.Create<HelpHandler>());
			}
			if (!Config.DisableVersionHandler)
			{
				metadatas.Add(HandlerMetadata.Create<VersionHandler>());
			}
			metadatas.AddRange(_handlerMetadatas);


			if (!Config.PreventDefaultHelp && args.Length == 1 && args[0] == "-?")
			{
				Output.WriteLine(HelpBuilder.Build(GetHandleContext(metadatas).Commands));
				return;
			}


			var commandNames = _handlerMetadatas.Where(o => !o.IsTopCommand).Select(o => o.CommandName);
			var tokens = Lex(args, commandNames, Config.IgnoreCase);
			bool matched = false;
			Handler handler = null;
			foreach (var item in metadatas)
			{
				// 每次需要一个新的 List，不然前一次的移除了会导致后面的缺失 Token
				if (item.TryMatch(new List<Token>(tokens), Config, out handler))
				{
					matched = true; 
					break; 
				}
			}

			if (matched)
			{
				var context = GetHandleContext(metadatas);
				try 
				{
					handler?.Handle(context); 
				}
				catch (Exception ex)
				{
					Output.WriteLine(HelpBuilder.Error(ex.Message));
					context.PreventDefault();
				}
				
				if (context.AutoTipWhenDone && !Config.DisableDoneTip)
				{
					Output.WriteLine(HelpBuilder.FormatContent(Resource.Help_Done));
				}
			}
			else
			{
				if (_matchFailedCallback is null)
					Output.WriteLine(HelpBuilder.Error(Resource.UnknownCommandOrOption));
				else
					_matchFailedCallback.Invoke(GetHandleContext(metadatas));
			}
		}


		private HandleContext GetHandleContext(IEnumerable<HandlerMetadata> handlerMetadatas)
		{
			Type defaultResourceType = _config.DefaultResourceType;
			var commands = handlerMetadatas.Select(m => 
			{
				var attr = m.HandlerType.GetCustomAttribute<CommandAttribute>(true);
				var opts = m.Options.Select(a => new OptionDescriptor(a, attr.ResourceType ?? defaultResourceType));
				return new CommandDescriptor(attr, opts, defaultResourceType);
			});

			return new HandleContext(Output, commands, Config, HelpBuilder);
		}


		/*********************************************************************************************
		 * 一个有效的处理程序需要满足：
		 * 1. 处理程序类必须应用了 CommandAttribute 特性
		 * 2. 类中的属性需要应用 (NoneValue|SingleValue|MultipleValue)OptionAttribute 特性
		 * 3. 只能有一个属性所应用的 OptionAttribute 特性的 IsDefault 属性为 true
		 * 4. NoneValueOptionAttribute 特性只能应用到 bool 类型的属性
		 * 5. MultipleValueOptionAttribute 特性只能应用到 IEnumerable<> 类型的属性
		 * 6. MultipleValueOptionAttribute 特性的 MaxCount 属性值必须大于等于 1
		 * 7. MultipleValueOptionAttribute 特性的 MinCount 属性值必须大于等于 1
		 * 8. MultipleValueOptionAttribute 特性的 MinCount 属性值不能大于 MaxCount 属性值
		 * 9. 对于同一个命令，短名称与短名称之间、短名称与长名称之间、长名称与长名称之间 均不能重复
		 *********************************************************************************************/
		private static void CheckHandler(Type handlerType, HashSet<string> optionNames)
		{
			var cmdAttr = handlerType.GetCustomAttribute<CommandAttribute>(true);
			if (cmdAttr is null) // 1.
			{
				throw new InvalidOperationException(Resource.HandlerLackOfCmdAttr.CCFormat(handlerType, typeof(CommandAttribute)));
			}
			string commandName = cmdAttr.Name;

			bool hasSetIsDefault = false;
			var propInfos = handlerType.GetProperties().Where(p => p.IsDefined(typeof(OptionAttribute), true)); // 2.
			foreach (var pi in propInfos)
			{
				var optAttr = pi.GetCustomAttribute<OptionAttribute>(true);
				if (optAttr.IsDefault)
				{
					if (hasSetIsDefault) // 3.
					{
						throw new InvalidOperationException(Resource.OnlyOneDefaultProp.CCFormat(handlerType));
					}
					hasSetIsDefault = true;
				}

				if (optAttr is NoneValueOptionAttribute)
				{
					if (pi.PropertyType != typeof(bool)) // 4.
					{
						throw new InvalidOperationException(Resource.AttrMustApplyToPropOfType.CCFormat(
							typeof(NoneValueOptionAttribute), typeof(bool)));
					}
				}
				else if (optAttr is MultipleValueOptionAttribute m)
				{
					Type t = typeof(MultipleValueOptionAttribute);
					// 5.
					if (!(pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
					{
						throw new InvalidOperationException(Resource.AttrMustApplyToPropOfType.CCFormat(t, typeof(IEnumerable<>)));
					}

					// 6. 
					if (m.MaxCount < 1)
					{
						throw new InvalidOperationException(
							Resource.PropOfAttrMustEqOrGtOne.CCFormat(t, nameof(m.MaxCount)));
					}

					// 7. 
					if (m.MinCount < 1)
					{
						throw new InvalidOperationException(
							Resource.PropOfAttrMustEqOrGtOne.CCFormat(t, nameof(m.MinCount)));
					}

					// 8. 
					if (m.MinCount > m.MaxCount)
					{
						throw new InvalidOperationException(Resource.MinCannotGtMax.CCFormat(
							t, nameof(m.MinCount), nameof(m.MaxCount)));
					}
				}

				AddOptionName(optionNames, commandName, optAttr.ShortName, optAttr.LongName); // 9.
			}
		}

		private static void AddOptionName(HashSet<string> optionNames, string commandName, char? shortName, string longName)
		{
			string name;

			if (shortName.HasValue && !string.IsNullOrEmpty(shortName.Value.ToString()))
			{
				name = FormatOptionName(commandName, shortName.Value.ToString());
				if (!optionNames.Contains(name))
					optionNames.Add(name);
				else
					throw new InvalidOperationException(Resource.OptionNameDuplicated.CCFormat(shortName.Value));
			}

			if (!string.IsNullOrEmpty(longName))
			{
				name = FormatOptionName(commandName, longName);
				if (!optionNames.Contains(name))
					optionNames.Add(name);
				else
					throw new InvalidOperationException(Resource.OptionNameDuplicated.CCFormat(longName));
			}
		}

		
		// 词法分析得到的标记序列集合，只有第一项可能为 TopCommand 或 SubCommand，其余项都是参数选项
		private static IEnumerable<Token> Lex(string[] args, IEnumerable<string> commandNames, bool ignoreCase)
		{
			var tokens = new List<Token>();
			if (args is null || args.Length <= 0)
			{
				return tokens;
			}

			Token token = null;
			for (int i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				if (i == 0 && !IsOption(arg, out _)) // 命令只能在第一项
				{
					if (commandNames.Contains(arg, Helpers.GetComparer(ignoreCase)))
					{
						token = new Token { Name = arg, Type = TokenType.SubCommand };
					}
					else
					{
						token = new Token { Type = TokenType.TopCommand };
						token.Values.Add(arg);
					}
				}
				else
				{
					if (IsOption(arg, out bool? isShortName))
					{
						if (token != null)
						{
							tokens.Add(token);
						}

						if (isShortName == true)
						{
							var chs = arg.Substring(ShortHypen.Length);
							for (int k = 0; k < chs.Length; k++)
							{
								token = new Token { Type = TokenType.ShortOption, Name = chs[k].ToString() };
								if (k < chs.Length - 1)
								{
									tokens.Add(token);
								}
							}
						}
						else if (isShortName == false)
						{
							token = new Token { Type = TokenType.LongOption, Name = arg.Substring(LongHypen.Length) };
						}
					}
					else
					{
						token.Values.Add(arg);
					}
				}
			}

			if (token != null)
			{
				tokens.Add(token);
			}

			return tokens;
		}

		/*********************************************************************************************
		 * 参数选项可能为：
		 * 1. 一个连接符(-)后接一个字母：-f  -n
		 * 2. 两个连接符(--)后接长度至少为 2 的字符串：--func  --name
		 * 3.  一个连接符(-)后接多个字母：-abc <=> -a -b -c
		 *********************************************************************************************/
		private static bool IsOption(string arg, out bool? isShortName)
		{
			if (arg.StartsWith(LongHypen))
			{
				bool b = OptionAttribute.ValidateLongName(arg.Substring(LongHypen.Length));
				isShortName = b ? false : (bool?)null;
				return b;
			}
			else if (arg.StartsWith(ShortHypen))
			{
				string name = arg.Substring(ShortHypen.Length);
				bool b = name.All(c => OptionAttribute.ValidateShortName(c));
				isShortName = b ? true : (bool?)null;	
				return b;
			}
			else
			{
				isShortName = null;
				return false;
			}
		}

		private static string FormatOptionName(string commandName, string optionName)
		{
			return $"{commandName}.{optionName}";
		}
	}
}
