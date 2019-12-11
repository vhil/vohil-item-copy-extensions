namespace Pintle.ItemCopyExtensions.Events
{
	using Sitecore.Data.Items;
	using Sitecore.Events;

	public class ItemEventHandler : AbstractItemEventListener
	{
		protected override void OnItemCopied(SitecoreEventArgs eventArgs, Item item, Item itemCopy)
		{
			ItemCopyingService.ConfiguredInstance.RemapToRelativeDataSources(item, itemCopy);
		}
	}
}