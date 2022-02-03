using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using _Game.Scripts.UI.HUD;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Tools
{
#if UNITY_EDITOR
	public static class TestTools
	{
		private static GameModel Game => Models.Get<GameModel>();

		public static void RunMagicTest()
		{
			// var enemy = Models.Get<GameModel>().EnemyContainer.GetAllAlive();
			// foreach (var en in enemy)
			// {
			// 	en.GetCameraPosition();
			// }
			

			// Models.Get<ItemsModel>().Add(ItemClass.Weapon, ItemType.AutomaticRifle);
			// Models.Get<ItemsModel>().Add(ItemClass.Weapon, ItemType.Shotgun);
			// Models.Get<ItemsModel>().Add(ItemClass.Weapon, ItemType.Uzi);
			// Models.Get<ItemsModel>().Add(ItemClass.Weapon, ItemType.Magnum);
			// Models.Get<ItemsModel>().Add(ItemClass.MeleeWeapon, ItemType.BaseballBat);
			// Models.Get<ItemsModel>().Add(ItemClass.MeleeWeapon, ItemType.Baton);
			// Models.Get<ItemsModel>().Add(ItemClass.MeleeWeapon, ItemType.Crowbar);
			// Models.Get<ItemsModel>().Add(ItemClass.MeleeWeapon, ItemType.Katana);
			// Models.Get<ItemsModel>().Add(ItemClass.MeleeWeapon, ItemType.Cleaver);
			// Models.Get<ItemsModel>().Add(ItemClass.MeleeWeapon, ItemType.Machete);
			Models.Get<ItemsModel>().Add(ItemClass.Grenade, ItemType.FragGrenade);
		}
	}
#endif
}