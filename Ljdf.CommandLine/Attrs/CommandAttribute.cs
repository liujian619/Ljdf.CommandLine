using System;
using System.Linq;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 用于命令选项的特性。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandAttribute : BaseAttribute
	{
		/// <summary>
		/// 构造 <see cref="CommandAttribute"/> 特性。
		/// </summary>
		/// <remarks>此构造函数一般用于顶级命令。</remarks>
		public CommandAttribute() : base(null)
		{
		}

		/// <summary>
		/// 构造 <see cref="CommandAttribute"/> 特性。
		/// </summary>
		/// <param name="description">描述文本。</param>
		/// <remarks>此构造函数一般用于顶级命令。</remarks>
		public CommandAttribute(string description) : base(description)
		{
		}

		/// <summary>
		/// 构造 <see cref="CommandAttribute"/> 特性。
		/// </summary>
		/// <param name="name">命令名称。（值为 <see langword="null"/> 时代表顶级命令，否则为子命令。）</param>
		/// <param name="description">描述文本。</param>
		/// <remarks>此构造函数一般用于子命令。</remarks>
		public CommandAttribute(string name, string description) : base(description)
		{
			if (name != null)
			{
				CheckName(name);
			}
			Name = name;
		}

		/// <summary>
		/// 获取命令名称，如果是顶级命令，则为 <see langword="null"/>。
		/// </summary>
		public string Name { get; }

		private static void CheckName(string name)
		{
			// 命令名称：长度大于等于1  必须是字母、数字、连接符(-)，同时第一个字符必须是字母
			if (!(name.Length >= 1 && char.IsLetter(name[0])
				&& name.All(c => char.IsNumber(c) || char.IsLetter(c) || c == '-')))
			{
				throw new FormatException(Resource.InvalidCommandName.CCFormat(name));
			}
		}
	}
}
