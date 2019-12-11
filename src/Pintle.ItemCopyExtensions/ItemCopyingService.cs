namespace Pintle.ItemCopyExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using Sitecore;
	using Sitecore.Configuration;
	using Sitecore.Data;
	using Sitecore.Data.Items;
	using Sitecore.Diagnostics;

	public class ItemCopyingService
	{
		protected bool RemapSharedLayout;
		protected bool RemapFinalLayout;

		public ItemCopyingService(string remapSharedLayout, string remapFinalLayout)
		{
			Assert.IsNotNull(remapSharedLayout, nameof(remapSharedLayout));
			Assert.IsNotNull(remapFinalLayout, nameof(remapFinalLayout));

			this.RemapSharedLayout = remapSharedLayout.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			this.RemapFinalLayout = remapFinalLayout.Equals("true", StringComparison.InvariantCultureIgnoreCase);
		}

		public void AddModuleTemplate(string key, System.Xml.XmlNode node)
		{
			this.AddModuleTemplate(node);
		}

		public void AddModuleTemplate(System.Xml.XmlNode node)
		{
			var idStr = Sitecore.Xml.XmlUtil.GetValue(node);
			if (ID.IsID(idStr))
			{
				this.PageModuleTemplates.Add(new Guid(idStr));
			}
		}

		protected ICollection<Guid> PageModuleTemplates = new List<Guid>();

		public static ItemCopyingService ConfiguredInstance => Factory.CreateObject("pintle/feature/itemCopyExtensions/itemCopyingService", true) as ItemCopyingService;

		public void RemapToRelativeDataSources(Item original, Item copy)
		{
			try
			{
				this.RemapDatasourcesForAllLanguages(original, copy);

				var childrenMap = this.GetChildOriginalToCopyMap(original, copy);

				foreach (var pair in childrenMap)
				{
					var childOriginal = pair.Key;
					var childCopy = pair.Value;

					this.RemapDatasourcesForAllLanguages(childOriginal, childCopy);
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Error re-mapping relative data sources for item '{copy?.Paths?.FullPath}' from its original item {original?.Paths?.FullPath}", ex, typeof(ItemCopyingService));
			}
		}

		protected virtual IEnumerable<KeyValuePair<Item, Item>> GetChildOriginalToCopyMap(Item original, Item copy)
		{
			if (original == null || copy == null) return Enumerable.Empty<KeyValuePair<Item, Item>>();

			try
			{
				var map = new Dictionary<Item, Item>();

				var copyChildren = copy.GetChildrenReccursively(reccursionStoperTemplates: this.PageModuleTemplates.ToArray())
					.Where(x => this.PageModuleTemplates.All(t => t != x.TemplateID.Guid))
					.ToArray();

				var originalChildren = original.GetChildrenReccursively(reccursionStoperTemplates: this.PageModuleTemplates.ToArray())
					.Where(x => this.PageModuleTemplates.All(t => t != x.TemplateID.Guid))
					.ToArray();

				var copyMap = new Dictionary<string, Item>();
				
				foreach (var copyChild in copyChildren)
				{
					if (!copyMap.ContainsKey(copyChild.Paths.FullPath))
					{
						copyMap.Add(copyChild.Paths.FullPath, copyChild);
					}
				}

				foreach (var originalChild in originalChildren)
				{
					var copyChildPath = originalChild.Paths.FullPath.Replace(original.Paths.FullPath, copy.Paths.FullPath);
					if (copyMap.ContainsKey(copyChildPath))
					{
						map.Add(originalChild, copyMap[copyChildPath]);
					}
				}

				return map;
			}
			catch (Exception ex)
			{
				Log.Error("Error getting children original to copy items map", ex, typeof(ItemCopyingService));
				return Enumerable.Empty<KeyValuePair<Item, Item>>();
			}
		}

		protected virtual void RemapDatasourcesForAllLanguages(Item original, Item copy)
		{
			var languages = original.Database.GetLanguages();
			foreach (var lang in languages)
			{
				var originalLangVersion = original.Database.GetItem(original.ID, lang);
				var copiedLangVersion = copy.Database.GetItem(copy.ID, lang);
				
				if (this.RemapFinalLayout)
				{
					this.RemapItemRelativeDatasource(originalLangVersion, copiedLangVersion, FieldIDs.FinalLayoutField);
				}

				if (this.RemapSharedLayout)
				{
					this.RemapItemRelativeDatasource(originalLangVersion, copiedLangVersion, FieldIDs.LayoutField);
				}
			}
		}

		protected virtual void RemapItemRelativeDatasource(Item original, Item copy, ID layoutFieldId)
		{
			try
			{
				if (original == null || copy == null) return;
				if (original.Versions.Count == 0 || copy.Versions.Count == 0) return;

				var originalDataSourses = original.GetDataSoursesForLayout(layoutFieldId);
				var layoutRawField = copy[layoutFieldId];

				foreach (var originalDatasource in originalDataSourses)
				{
					if (this.IsRelativeDatasource(originalDatasource, original))
					{
						var copiedRelativeDatasource = this.GetCopiedRelativeDatasource(originalDatasource, original, copy);
						if (!string.IsNullOrWhiteSpace(copiedRelativeDatasource))
						{
							layoutRawField = layoutRawField.Replace(originalDatasource, copiedRelativeDatasource);
						}
					}
				}

				using (new EditContext(copy))
				{
					copy[layoutFieldId] = layoutRawField;
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Error re-mapping relative data sources for item '{copy?.Paths?.FullPath}' from its original item {original?.Paths?.FullPath}", ex, typeof(ItemCopyingService));
			}
		}

		protected virtual string GetCopiedRelativeDatasource(string originalDatasource, Item original, Item copy)
		{
			var dsItem = original.Database.GetItem(originalDatasource, original.Language);
			if (dsItem != null)
			{
				var newDsItem = original.Database.GetItem(dsItem.Paths.FullPath.Replace(original.Paths.FullPath, copy.Paths.FullPath), original.Language);

				if (newDsItem != null)
				{
					return newDsItem.ID.ToString();
				}
			}

			return string.Empty;
		}

		protected virtual bool IsRelativeDatasource(string originalDatasource, Item original)
		{
			var dsItem = original.Database.GetItem(originalDatasource, original.Language);
			if (dsItem != null)
			{
				return dsItem.Paths.FullPath.StartsWith(original.Paths.FullPath);
			}

			return false;
		}
	}
}