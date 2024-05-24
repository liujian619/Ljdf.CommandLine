using System;
using System.Reflection;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 用于选项的特性基类。
	/// </summary>
	public abstract class BaseAttribute : Attribute
	{
		/// <summary>
		/// 构造 <see cref="BaseAttribute"/> 对象。
		/// </summary>
		/// <param name="description">描述文本。</param>
		protected BaseAttribute(string description)
		{
			Description = description;
		}

		/// <summary>
		/// 获取描述文本。
		/// </summary>
		/// <remarks>
		/// <para>可以通过该属性实现多语言的本地化。</para>
		/// <para>若 <see cref="ResourceType"/> 不为 <see langword="null"/>，则该属性代表的是资源键（区分大小写），通过资源键在资源类中获取描述文本。</para>
		/// </remarks>
		public string Description { get; }

		/// <summary>
		/// 获取或设置一个值，指示是否不显示命令或选项信息。
		/// </summary>
		public bool Hidden { get; set; }

		/// <summary>
		/// 获取或设置序号。
		/// </summary>
		/// <remarks>该序号仅用于辅助打印帮助信息。</remarks>
		public int Order { get; set; }

		/// <summary>
		/// 获取或设置资源类的类型。
		/// </summary>
		public Type ResourceType { get; set; }


		/// <summary>
		/// 获取真实的描述文本。
		/// </summary>
		/// <param name="inheritedResourceType">继承的资源类的类型。</param>
		internal string GetActualDescription(Type inheritedResourceType)
		{
			var resxType = ResourceType ?? inheritedResourceType;
			if (resxType != null && !string.IsNullOrEmpty(Description))
			{
				var pi = resxType.GetProperty(Description, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (pi != null)
				{
					string s = pi.GetValue(null)?.ToString();
					if (s != null)
					{
						return s;
					}
				}
			}

			return Description;
		}
	}
}
