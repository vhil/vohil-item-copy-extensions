namespace Pintle.ItemCopyExtensions.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Sitecore.Collections;
	using Sitecore.Data;
	using Sitecore.Data.Items;
	using Sitecore.Data.Managers;
	using Sitecore.Data.Fields;
	using Sitecore.Layouts;
	using Sitecore.Rules;
	using Sitecore.Rules.ConditionalRenderings;

	internal static class ItemExtensions
	{
		internal static IEnumerable<Item> GetChildrenReccursively(
			this Item item,
			Guid? templateId = null,
			params Guid[] reccursionStoperTemplates)
		{
			if (item == null) return Enumerable.Empty<Item>();
			if (reccursionStoperTemplates == null) reccursionStoperTemplates = new Guid[0];

			var result = new List<Item>();

			foreach (var child in item.GetChildren(ChildListOptions.IgnoreSecurity).Where(x => x != null))
			{
				if (!templateId.HasValue || child.IsDerived(templateId.Value))
				{
					result.Add(child);
				}

				if (!reccursionStoperTemplates.Contains(child.TemplateID.Guid))
				{
					result.AddRange(child.GetChildrenReccursively(templateId, reccursionStoperTemplates));
				}
			}

			return result;
		}

		internal static bool IsDerived(this Item item, Guid templateId)
		{
			if (item == null) return false;

			return item.IsDerived(new ID(templateId));
		}

		internal static bool IsDerived(this Item item, ID templateId)
		{
			if (item == null)
			{
				return false;
			}

			return !templateId.IsNull && item.IsDerived(item.Database.Templates[templateId]);
		}

		internal static bool IsDerived(this Item item, Item templateItem)
		{
			if (item == null)
			{
				return false;
			}

			if (templateItem == null)
			{
				return false;
			}

			var itemTemplate = TemplateManager.GetTemplate(item);
			return itemTemplate != null && (itemTemplate.ID == templateItem.ID || itemTemplate.DescendsFrom(templateItem.ID));
		}

		internal static IEnumerable<string> GetDataSoursesForLayout(this Item pageItem, ID layoutFieldId)
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
		
		internal static LayoutDefinition GetLayoutDefinition(this Item item, ID layoutFieldId)
		{
			var layoutField = LayoutField.GetFieldValue(item.Fields[layoutFieldId]);
			return LayoutDefinition.Parse(layoutField);
		}
	}
}