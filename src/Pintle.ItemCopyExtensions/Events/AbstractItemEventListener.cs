namespace Pintle.ItemCopyExtensions.Events
{
	using System;
	using Sitecore.Data;
	using Sitecore.Data.Events;
	using Sitecore.Data.Items;
	using Sitecore.Data.Templates;
	using Sitecore.Diagnostics;
	using Sitecore.Events;

	public abstract class AbstractItemEventListener
	{
		#region event subscribers

		public void OnItemAdded(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemAdded(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemCreated(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var itemCreated = this.ExtractArgument<ItemCreatedEventArgs>(args, 0);
				var item = itemCreated.Item;
				this.OnItemCreated(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemRenamed(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemRenamed(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemCopied(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				var itemCopy = this.ExtractItem(args, 1);
				this.OnItemCopied(eventArgs, item, itemCopy);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemCopying(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				var targetItem = this.ExtractArgument<Item>(args, 1);
				var copyName = this.ExtractArgument<string>(args, 2);
				var copyId = this.ExtractArgument<ID>(args, 3);

				this.OnItemCopying(eventArgs, item, targetItem, copyName, copyId);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemCloneAdded(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemCloneAdded(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemCreating(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var itemCreating = this.ExtractArgument<ItemCreatingEventArgs>(args, 0);
				this.OnItemCreating(sender, eventArgs, itemCreating);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemDeleting(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemDeleting(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemMoved(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				var targetParent = this.ExtractArgument<ID>(args, 1);
				this.OnItemMoved(eventArgs, item, targetParent);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemMoving(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				var oldParent = this.ExtractArgument<ID>(args, 1);
				var newParent = this.ExtractArgument<ID>(args, 2);

				this.OnItemMoving(eventArgs, item, oldParent, newParent);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemSaved(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				var itemChanges = this.ExtractArgument<ItemChanges>(args, 1);
				this.OnItemSaved(eventArgs, item, itemChanges);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemSaving(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemSaving(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemTemplateChanged(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				var templateChange = this.ExtractArgument<TemplateChangeList>(args, 1);
				var oldTemplate = templateChange.Source;
				var newTemplate = templateChange.Target;

				this.OnItemTemplateChanged(eventArgs, item, oldTemplate, newTemplate);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemTransferred(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemTransferred(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemVersionAdding(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemVersionAdding(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		public void OnItemVersionRemoving(object sender, EventArgs args)
		{
			var eventArgs = args as SitecoreEventArgs;

			try
			{
				var item = this.ExtractItem(args, 0);
				this.OnItemVersionRemoving(eventArgs, item);
			}
			catch (Exception ex)
			{
				this.LogException(eventArgs?.EventName, ex);
			}
		}

		#endregion

		#region virtual

		protected virtual void OnItemAdded(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemCreated(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemRenamed(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemCopied(SitecoreEventArgs eventArgs, Item item, Item itemCopy)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemCopying(SitecoreEventArgs eventArgs, Item item, Item targetItem, string copyName, ID copyId)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemCloneAdded(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemCreating(object sender, SitecoreEventArgs eventArgs, ItemCreatingEventArgs itemCreating)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemDeleting(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemMoved(SitecoreEventArgs eventArgs, Item item, ID targetParent)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemVersionRemoving(SitecoreEventArgs eventArgs, Item itemVersion)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemVersionAdding(SitecoreEventArgs eventArgs, Item itemVersion)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemTransferred(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemTemplateChanged(SitecoreEventArgs eventArgs, Item item, Template oldTemplate, Template newTemplate)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemSaving(SitecoreEventArgs eventArgs, Item item)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemSaved(SitecoreEventArgs eventArgs, Item item, ItemChanges itemChanges)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnItemMoving(SitecoreEventArgs eventArgs, Item item, ID oldParent, ID newParent)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region protected

		protected void LogException(string eventName, Exception ex)
		{
			Log.Error($"Error while executing {this.GetType().FullName} event handler of {eventName} event. {ex.Message}", ex, this);
		}

		protected Item ExtractItem(EventArgs args, int itemParamIndex)
		{
			return Event.ExtractParameter(args, itemParamIndex) as Item;
		}

		protected T ExtractArgument<T>(EventArgs args, int itemParamIndex)
		{
			return (T)Event.ExtractParameter(args, itemParamIndex);
		}

		#endregion
	}
}