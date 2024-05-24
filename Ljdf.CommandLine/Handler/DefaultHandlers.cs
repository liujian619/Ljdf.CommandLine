namespace Ljdf.CommandLine
{
	[Command(Order = int.MinValue, ResourceType = typeof(Resource))]
	internal sealed class HelpHandler : Handler
	{
		[NoneValueOption('h', "help", "PrintHelp")]
		public bool IsHelp { get; set; }

		public override void Handle(HandleContext context)
		{
			if (IsHelp)
			{
				context.Output.WriteLine(context.HelpBuilder.Build(context.Commands));
				context.PreventDefault();
			}
		}
	}

	[Command(Order = int.MinValue, ResourceType = typeof(Resource))]
	internal sealed class VersionHandler : Handler
	{
		[NoneValueOption('v', "version", "PrintVersion")]
		public bool IsVersion { get; set; }

		public override void Handle(HandleContext context)
		{
			if (IsVersion)
			{
				context.Output.WriteLine(HelpBuilder.Version);
				context.PreventDefault();
			}
		}
	}
}
