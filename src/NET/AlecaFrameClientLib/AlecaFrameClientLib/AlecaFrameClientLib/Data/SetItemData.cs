using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFrameClientLib.Data
{
	public class SetItemData : InventoryItemData
	{
		public List<FoundryItemComponent> components = new List<FoundryItemComponent>();

		public bool isReadyToSell;

		public SetItemData()
		{
			type = "set";
		}

		public void InitializeSetComponents(BigItem isPartOf)
		{
			components = (from p in isPartOf.components?.Where(delegate(ItemComponent p)
				{
					string uniqueName = p.uniqueName;
					if (uniqueName != null && !uniqueName.Contains("/MiscItems/"))
					{
						string uniqueName2 = p.uniqueName;
						if (uniqueName2 != null && !uniqueName2.Contains("/Research/") && p.isPartOf != null && p.reloadTime == 0f && p.fireRate == 0f && !p.name.Contains("Embolos") && !p.name.Contains("Xenorhast") && !p.name.Contains("Cranial Foremount") && (p.tradable || p.isPartOf?.name?.Contains("Prime") == true) && (!p.IsLandingCraftPart() || p.name != "Blueprint"))
						{
							return p.name != "Thrax Plasm";
						}
					}
					return false;
				})
				select new FoundryItemComponent(p)).ToList();
			components.RemoveAll((FoundryItemComponent p) => p.name == "Parallax Blueprint");
			isFav = FavouriteHelper.IsFavourite(isPartOf.uniqueName);
			foreach (FoundryItemComponent component in components)
			{
				component.quantity = 0;
				component.quantityOwned = "0";
				component.recipeNeccessaryComponents = false;
			}
			vaulted = true;
		}

		public void AddSetComponent(ItemComponent part, int amountToAdd)
		{
			FoundryItemComponent foundryItemComponent = components.FirstOrDefault((FoundryItemComponent p) => p.uniqueName == part.uniqueName);
			if (foundryItemComponent != null)
			{
				foundryItemComponent.quantity += amountToAdd;
				foundryItemComponent.UpdateVisibleFields();
			}
		}

		internal void UpdateDucats()
		{
			ducats = components?.Sum((FoundryItemComponent p) => p.ducats * p.neccessaryAmount) ?? 0;
		}

		public int CountHowManyAreReady()
		{
			return components.Min((FoundryItemComponent p) => p.quantity / p.neccessaryAmount);
		}
	}
}
