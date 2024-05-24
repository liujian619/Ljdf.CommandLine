using System.Collections.Generic;
using System.IO;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 处理程序上下文。
	/// </summary>
	public sealed class HandleContext
	{
		internal HandleContext(TextWriter output, IEnumerable<CommandDescriptor> commands, 
			ParserConfig parserConfig, HelpBuilder helpBuilder)
		{
			Output = output;
			Commands = commands;
			_parserConfig = parserConfig;
			HelpBuilder = helpBuilder;
		}

		private readonly ParserConfig _parserConfig;

		/// <summary>
		/// 获取一个值，指示在解析过程中是否忽略大小写。
		/// </summary>
		public bool IgnoreCase => _parserConfig.IgnoreCase;	

		/// <summary>
		/// 获取输出流。
		/// </summary>
		public TextWriter Output { get; }

		/// <summary>
		/// 获取命令集合。
		/// </summary>
		public IEnumerable<CommandDescriptor> Commands { get; }
		
		/// <summary>
		/// 获取 <see cref="CommandLine.HelpBuilder"/> 对象。
		/// </summary>
		public HelpBuilder HelpBuilder { get; }


		internal bool AutoTipWhenDone { get; private set; } = true;

		/// <summary>
		/// 阻止默认行为。
		/// </summary>
		public void PreventDefault() 
		{
			AutoTipWhenDone = false;
		}
	}
}
