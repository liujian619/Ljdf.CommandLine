using System.Collections.Generic;

namespace Ljdf.CommandLine
{
	internal sealed class Token
	{
		// 记录短名称 or 长名称 or 子命令名称；若 Type 为 TopCommand，此属性为 null
		public string Name { get; set; }

		public TokenType Type { get; set; }

		// 记录选项的值 or 命令的默认选项的值
		public IList<string> Values { get; } = new List<string>();
	}

	enum TokenType
	{
		ShortOption, // 短名称选项

		LongOption, // 长名称选项
		
		TopCommand, // 顶级命令
		
		SubCommand, // 子命令
	}
}
