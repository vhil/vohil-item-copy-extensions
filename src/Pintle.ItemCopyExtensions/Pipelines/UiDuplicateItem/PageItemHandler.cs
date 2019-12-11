namespace Pintle.ItemCopyExtensions.Pipelines.UiDuplicateItem
{
	using Sitecore;
	using Sitecore.Configuration;
	using Sitecore.Data;
	using Sitecore.Diagnostics;
	using Sitecore.Globalization;
	using Sitecore.Web.UI.Sheer;

	public class PageItemHandler
	{
		public void RemapToRelativeDataSourses(ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull((object)args, nameof(args));
			var database = Factory.GetDatabase(args.Parameters["database"]);
			var id = args.Parameters["id"];
			if (!Language.TryParse(args.Parameters["language"], out var language)) language = Context.Language;
			var copyName = args.Parameters["name"];
			var original = database.GetItem(ID.Parse(id), language);
			
			if (original == null)
			{
				SheerResponse.Alert("Item not found.");
				args.AbortPipeline();

			}
			else
			{
				var parent = original.Parent;
				if (parent == null)
				{
					args.AbortPipeline();
				}
				else
				{
					var copy = database.GetItem(parent.Paths.FullPath + "/" + copyName, language);

					if (copy == null)
					{
						args.AbortPipeline();
					}
					else
					{
						ItemCopyingService.ConfiguredInstance.RemapToRelativeDataSources(original, copy);
					}
				}
			
			}
		}

	}
}