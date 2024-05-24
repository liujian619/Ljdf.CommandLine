using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ljdf.CommandLine
{
	internal sealed class HandlerMetadata
	{
		private HandlerMetadata(Type handlerType, string commandName, IList<OptionAttribute> options)
		{
			HandlerType = handlerType;
			CommandName = commandName;
			Options = options;
		}

		public static HandlerMetadata Create<T>() where T : Handler, new()
		{
			Type handlerType = typeof(T);

			List<OptionAttribute> optionAttributes = new List<OptionAttribute>();
			var propInfos = handlerType.GetProperties();
			foreach (var pi in propInfos)
			{
				var optAttr = pi.GetCustomAttribute<OptionAttribute>(true);
				if (optAttr != null)
				{
					optAttr.PropertyInfo = pi;
					optionAttributes.Add(optAttr);
				}
			}

			var cmdAttr = handlerType.GetCustomAttribute<CommandAttribute>(true);
			return new HandlerMetadata(handlerType, cmdAttr?.Name, optionAttributes);
		}

		/// <summary>
		/// 处理程序的类型。
		/// </summary>
		public Type HandlerType { get; }

		/// <summary>
		/// 命令名称，如果是顶级命令，则为 <see langword="null"/>。
		/// </summary>
		public string CommandName { get; }

		/// <summary>
		/// 选项集合。
		/// </summary>
		public IEnumerable<OptionAttribute> Options { get; }

		/// <summary>
		/// 获取一个值，指示是否是顶级命令。
		/// </summary>
		public bool IsTopCommand => CommandName is null;


		/*****************************************************************************************
		 * 匹配过程：
		 * 由于只有第一项 token 可能代表命令，因此需要单独判断第一项 token。
		 * 如果第一项 token 代表的是命令，则先匹配相应的命令。
		 * 如果命令匹配成功，去掉第一项 token，其余按选项去匹配。
		 * 如果第一项 token 不是命令，则全部按选项去匹配。
		 *****************************************************************************************/
		public bool TryMatch(IList<Token> tokens, ParserConfig config, out Handler handler)
		{
			handler = Activator.CreateInstance(HandlerType) as Handler;
			if (handler is null || tokens is null || !tokens.Any())
			{
				return false;
			}

			try
			{
				var firstToken = tokens[0];
				if (firstToken.Type == TokenType.TopCommand || firstToken.Type == TokenType.SubCommand)
				{
					if (TryMatchCommand(handler, firstToken, config, out OptionAttribute defaultOption))
					{
						tokens.Remove(firstToken);
						return TryMatchOptions(handler, tokens, defaultOption, config);
					}
					else { return false; }
				}
				else
				{
					// 如果第一个 token 不代表命令，这说明这些选项都是用于顶级命令的
					return IsTopCommand && TryMatchOptions(handler, tokens, null, config);
				}
			}
			catch 
			{	
				return false; // 匹配时不需要知道具体异常，凡是有异常的就说明不匹配
			}
		}

		private bool TryMatchCommand(Handler handler, Token token, ParserConfig config, out OptionAttribute defaultOption)
		{
			if (token.Type == TokenType.TopCommand && IsTopCommand
				|| (token.Type == TokenType.SubCommand && !IsTopCommand && Helpers.Eq(CommandName, token.Name, config.IgnoreCase)))
			{
				defaultOption = Options.FirstOrDefault(o => o.IsDefault);
				if (defaultOption != null)
				{
					SetValue(handler, defaultOption.PropertyInfo, defaultOption is SingleValueOptionAttribute, token.Values);
				}

				return true;
			}
			else
			{
				defaultOption = null;
				return false;
			}
		}

		private bool TryMatchOptions(Handler handler, IList<Token> tokens, OptionAttribute defaultOption, ParserConfig config)
		{
			var opts = defaultOption != null ? Options.Except(new[] { defaultOption }) : Options;
			foreach (var opt in opts)
			{
				var token = tokens.FirstOrDefault(a =>
					(a.Type == TokenType.ShortOption && Helpers.Eq(a.Name, opt.ShortName?.ToString(), config.IgnoreCase))
					|| (a.Type == TokenType.LongOption && Helpers.Eq(a.Name, opt.LongName, config.IgnoreCase)));

				if (token != null)
				{
					int count = token.Values.Count;
					if (opt is MultipleValueOptionAttribute m && (count < m.MinCount || count > m.MaxCount))
					{
						return false;
					}

					if (opt is NoneValueOptionAttribute)
						opt.PropertyInfo.SetValue(handler, true);
					else
						SetValue(handler, opt.PropertyInfo, opt is SingleValueOptionAttribute, token.Values);

					tokens.Remove(token);
				}
				else // 用户未设置值
				{
					// 属性不是可选的
					if (!opt.Optional) { return false; }
				}
			}

			return tokens.Count == 0; // tokens 与属性完全一一对应（如果tokens 中不存在对应属性，但是该属性可选，则也算对应了）
		}


		private static void SetValue(Handler handler, PropertyInfo pi, bool isSingleValue, IEnumerable<string> values)
		{
			if (isSingleValue)
			{
				Type targetType = pi.PropertyType;
				string value = values.First();
				if (TryConvert(value, targetType, Parser.GetTypeConverter(targetType), out object convertedValue))
					pi.SetValue(handler, convertedValue);
				else
					throw new InvalidCastException(Resource.CannotConvertFrom.CCFormat(value, targetType));
			}
			else
			{
				Type targetType = pi.PropertyType.GetGenericArguments()[0];
				object list = Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType), values.Count());
				Delegate add = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(targetType), list, "Add", false);
				foreach (var v in values)
				{
					if (TryConvert(v, targetType, Parser.GetTypeConverter(targetType), out object convertedValue))
						add.DynamicInvoke(convertedValue);
					else
						throw new InvalidCastException(Resource.CannotConvertFrom.CCFormat(v, targetType));
				}

				pi.SetValue(handler, list);
			}
		}

		private static bool TryConvert(string value, Type targetType, TypeConverter typeConverter, out object convertedValue)
		{
			var sourceType = typeof(string);
			if (targetType == sourceType)
			{
				convertedValue = value;
				return true;
			}

			try
			{
				TypeConverter converter = typeConverter ?? TypeDescriptor.GetConverter(targetType);
				if (converter.CanConvertFrom(sourceType))
				{
					convertedValue = converter.ConvertFrom(value);
					return true;
				}

				converter = TypeDescriptor.GetConverter(sourceType);
				if (converter.CanConvertTo(targetType))
				{
					convertedValue = converter.ConvertTo(value, targetType);
					return true;
				}
			}
			catch { }

			convertedValue = null;
			return false;
		}
	}
}
