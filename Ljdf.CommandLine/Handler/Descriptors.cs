using System;
using System.Collections.Generic;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 命令信息。
	/// </summary>
	public sealed class CommandDescriptor
	{
		internal CommandDescriptor(CommandAttribute commandAttribute, IEnumerable<OptionDescriptor> options, Type inheritedResourceType)
		{
			_commandAttribute = commandAttribute;
			_inheritedResourceType = inheritedResourceType;
			Options = options;
		}
		
		private readonly CommandAttribute _commandAttribute;
		private readonly Type _inheritedResourceType;


		/// <summary>
		/// 获取命令名称，如果是顶级命令，则为 <see langword="null"/>。
		/// </summary>
		public string Name => _commandAttribute.Name;

		private string _description;
		/// <summary>
		/// 获取描述文本。
		/// </summary>
		public string Description
		{
			get
			{
				if (_description is null)
					_description = _commandAttribute.GetActualDescription(_inheritedResourceType);
			
				return _description;
			}
		}

		/// <summary>
		/// 获取一个值，指示是否不显示命令信息。
		/// </summary>
		public bool Hidden => _commandAttribute.Hidden;

		/// <summary>
		/// 获取或设置序号。
		/// </summary>
		/// <remarks>该序号仅用于辅助打印帮助信息。</remarks>
		public int Order => _commandAttribute.Order;

		/// <summary>
		/// 获取一个值，指示是否是顶级命令。
		/// </summary>
		public bool IsTopCommand => Name is null;

		/// <summary>
		/// 获取该命令的选项集合。
		/// </summary>
		public IEnumerable<OptionDescriptor> Options { get; }
	}


	/// <summary>
	/// 选项信息。
	/// </summary>
	public sealed class OptionDescriptor
	{
		internal OptionDescriptor(OptionAttribute optionAttribute, Type inheritedResourceType)
		{
			_optionAttribute = optionAttribute;
			_inheritedResourceType = inheritedResourceType;

			if (IsNoneValueOption)
				Type = typeof(bool);
			else if (IsSingleValueOption)
				Type = _optionAttribute.PropertyInfo.PropertyType;
			else if (IsMultipleValueOption)
				Type = _optionAttribute.PropertyInfo.PropertyType.GetGenericArguments()[0];
		}

		private readonly OptionAttribute _optionAttribute;
		private readonly Type _inheritedResourceType;

		/// <summary>
		/// 获取类型。
		/// </summary>
		/// <remarks>
		/// <para>1. 若 <see cref="IsNoneValueOption"/> 为 <see langword="true"/>，该类型为 <see cref="bool"/>；</para>
		/// <para>2. 若 <see cref="IsSingleValueOption"/> 为 <see langword="true"/>，该类型为属性类型；</para>
		/// <para>3. 若 <see cref="IsMultipleValueOption"/> 为 <see langword="true"/>，该类型为泛型类型的参数类型；</para>
		/// </remarks>
		public Type Type { get; }

		/// <summary>
		/// 获取短名称。
		/// </summary>
		public char? ShortName => _optionAttribute.ShortName;

		/// <summary>
		/// 获取长名称。
		/// </summary>
		public string LongName => _optionAttribute.LongName;

		private string _description;
		/// <summary>
		/// 获取描述文本。
		/// </summary>
		public string Description
		{
			get
			{
				if (_description is null)
					_description = _optionAttribute.GetActualDescription(_inheritedResourceType);

				return _description;
			}
		}

		/// <summary>
		/// 获取序号。
		/// </summary>
		/// <remarks>该序号仅用于辅助打印帮助信息。</remarks>
		public int Order => _optionAttribute.Order;	

		/// <summary>
		/// 获取一个值，指示是否不显示选项信息。
		/// </summary>
		public bool Hidden => _optionAttribute.Hidden;

		/// <summary>
		/// 获取是否可选。
		/// </summary>
		public bool Optional => _optionAttribute.Optional;

		/// <summary>
		/// 获取可选时的默认值。
		/// </summary>
		/// <remarks><b>注意：</b>该值只是用于辅助生成帮助文档，不会实际赋值给选项。</remarks>
		public object OptionalValue => _optionAttribute.OptionalValue;

		/// <summary>
		/// 获取一个值，指示是否是默认选项。
		/// </summary>
		public bool IsDefault => _optionAttribute.IsDefault;

		/// <summary>
		/// 获取一个值，指示该选项是否是无参选项。
		/// </summary>
		public bool IsNoneValueOption => _optionAttribute is NoneValueOptionAttribute;

		/// <summary>
		/// 获取一个值，指示该选项是否是独参选项。
		/// </summary>
		public bool IsSingleValueOption => _optionAttribute is SingleValueOptionAttribute;

		/// <summary>
		/// 获取一个值，指示该选项是否是多参选项。
		/// </summary>
		public bool IsMultipleValueOption => _optionAttribute is MultipleValueOptionAttribute;

		/// <summary>
		/// 获取参数值最多个数。
		/// </summary>
		/// <remarks>当且仅当 <see cref="IsMultipleValueOption"/> 为 <see langword="true"/> 时有效。</remarks>
		public int? MaxCount => IsMultipleValueOption ? (_optionAttribute as MultipleValueOptionAttribute).MaxCount : (int?)null;

		/// <summary>
		/// 获取参数值最少个数。
		/// </summary>
		/// <remarks>当且仅当 <see cref="IsMultipleValueOption"/> 为 <see langword="true"/> 时有效。</remarks>
		public int? MinCount => IsMultipleValueOption ? (_optionAttribute as MultipleValueOptionAttribute).MinCount : (int?)null;
	}
}
