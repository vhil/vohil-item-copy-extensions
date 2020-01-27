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
	using System.Security.Cryptography;
	using System.Text;
	using Sitecore.Layouts;
	using Sitecore.Rules;
	using Sitecore.Rules.ConditionalRenderings;

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

		#region configuration

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

		#endregion

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

				var originalDataSourses = this.GetDataSoursesForLayout(original, layoutFieldId);
				var layoutRawField = copy[layoutFieldId];
				var datasourceMap = this.GetRelativeDatasourceMap(original, copy);
				foreach (var originalDatasource in originalDataSourses)
				{
					if (this.IsRelativeDatasource(originalDatasource, original))
					{
						var copiedRelativeDatasource = this.GetCopiedRelativeDatasource(originalDatasource, original, copy, datasourceMap);
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

		protected virtual string GetCopiedRelativeDatasource(string originalDatasource, Item original, Item copy, IDictionary<Guid, Item> childMap)
		{
			var dsItem = original.Database.GetItem(originalDatasource, original.Language);
			if (dsItem != null && childMap.ContainsKey(dsItem.ID.Guid))
			{
				var newDsItem = childMap[dsItem.ID.Guid];

				if (newDsItem != null)
				{
					return newDsItem.ID.ToString();
				}
			}

			return string.Empty;
		}

		protected IDictionary<Guid, Item> GetRelativeDatasourceMap(Item original, Item copy)
		{
			var map = new Dictionary<Guid, Item>();

			var originalChildren = original.GetChildrenReccursively();
			var copiedChildren = new Dictionary<string, Item>();

			foreach (var copiedChild in copy.GetChildrenReccursively())
			{
				var hash = this.GetItemHash(copiedChild);
				if (!copiedChildren.ContainsKey(hash))
				{
					copiedChildren.Add(hash, copiedChild);
				}
				else
				{
					var duplicate1 = copiedChildren[hash];
					var duplicate2 = copiedChild;
				}
			}

			foreach (var child in originalChildren)
			{
				var childHash = this.GetItemHash(child);

				if (copiedChildren.ContainsKey(childHash) && !map.ContainsKey(child.ID.Guid))
				{
					map.Add(child.ID.Guid, copiedChildren[childHash]);
				}
			}

			return map;
		}

		protected virtual string GetItemHash(Item item)
		{
			var hash = new StringBuilder(item.Name);
			hash.Append(item.TemplateID.Guid.ToString());
			hash.Append(item.Version.Number);
			hash.Append(item.Appearance.Sortorder);
			hash.Append(item.Statistics.Created.ToString("s"));

			foreach (var field in item.Fields.Where(x => !x.Name.StartsWith("__")).OrderBy(x => x.Name))
			{
				hash.Append(field.Value);
			}

			using (var algo = new MD5CryptoServiceProvider())
			{
				var hashString = GenerateHashString(algo, hash.ToString());
				return hashString;
			}
		}

		private static string GenerateHashString(HashAlgorithm algo, string text)
		{
			algo.ComputeHash(Encoding.UTF8.GetBytes(text));
			var result = algo.Hash;
			return string.Join(
				string.Empty,
				result.Select(x => x.ToString("x2")));
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

		protected virtual IEnumerable<string> GetDataSoursesForLayout(Item pageItem, ID layoutFieldId)
		{
			if (pageItem == null) return Enumerable.Empty<string>();

			var datasourses = new List<string>();

			var renderings = pageItem
				                 .GetLayoutDefinition(layoutFieldId)?
				                 .Devices?
				                 .ToArray()?
				                 .Select(x => (DeviceDefinition)x)?
				                 .SelectMany(d => d.Renderings.Cast<RenderingDefinition>())?
				                 .ToArray()
			                 ?? new RenderingDefinition[0];

			foreach (var rendering in renderings)
			{
				datasourses.Add(rendering.Datasource);

				if (rendering.Rules != null && rendering.Rules.HasElements)
				{
					var rules = RuleFactory.ParseRules<ConditionalRenderingsRuleContext>(pageItem.Database, rendering.Rules);
					var actions = rules.Rules?
						.SelectMany(x => x.Actions)
						.Select(x => x as SetDataSourceAction<ConditionalRenderingsRuleContext>)
						.Where(x => x != null)
						.ToArray();

					foreach (var action in actions ?? Enumerable.Empty<SetDataSourceAction<ConditionalRenderingsRuleContext>>())
					{
						datasourses.Add(action.DataSource);
					}
				}
			}

			return datasourses
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToList();
		}

	}
}