﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace CJBItemSpawner
{
    internal class ItemMenu : ItemMenuWithInventory
    {
        /*********
        ** Properties
        *********/
        private ItemInventoryMenu ItemsToGrabMenu;
        private TemporaryAnimatedSprite Poof;
        private static bool ItemsLoaded;

        private ClickableComponent Title;
        private ClickableComponent SortButton;
        private ClickableComponent QualityButton;
        private List<ClickableComponent> Tabs = new List<ClickableComponent>();
        private static int TabIndex;
        private static int SortID;
        private static ItemQuality Quality = ItemQuality.Normal;

        private TextBox Textbox;
        private Rectangle TextboxBounds;
        private List<Item> InventoryItems;
        private ClickableTextureComponent UpArrow;
        private ClickableTextureComponent DownArrow;

        private bool ShowReceivingMenu = true;
        private bool CanExitOnKey = true;
        private bool AllowRightClick;
        private static List<Item> ItemList;
        private static string TempText = "";


        /*********
        ** Public methods
        *********/
        public ItemMenu(List<Item> inventory)
          : base(null, true, true, 0, -50)
        {
            this.InventoryItems = inventory;
            this.MovePosition(110, Game1.viewport.Height / 2 - (650 + IClickableMenu.borderWidth * 2) / 2);

            this.Textbox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
            this.Textbox.X = Game1.viewport.Width / 2 - Game1.tileSize * 3;
            this.Textbox.Y = Game1.viewport.Height / 2;
            this.Textbox.Width = Game1.tileSize * 8;
            this.Textbox.Height = Game1.tileSize * 3;
            this.Textbox.X = this.xPositionOnScreen + (width / 2) - (this.Textbox.Width / 2) - Game1.tileSize + 32;
            this.Textbox.Y = this.yPositionOnScreen + (height / 2) + Game1.tileSize * 2 + 40;
            this.Textbox.Selected = false;
            this.Textbox.Text = ItemMenu.TempText;
            Game1.keyboardDispatcher.Subscriber = this.Textbox;
            this.TextboxBounds = new Rectangle(this.Textbox.X, this.Textbox.Y, this.Textbox.Width, this.Textbox.Height / 3);

            this.Title = new ClickableComponent(new Rectangle(this.xPositionOnScreen + width - Game1.tileSize, this.yPositionOnScreen - Game1.tileSize * 2, Game1.tileSize * 4, Game1.tileSize), "CJB Item Spawner");
            this.SortButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen - Game1.tileSize * 2 + 10, Game1.tileSize * 4, Game1.tileSize), "Sort By: Name");
            this.QualityButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 4, this.yPositionOnScreen - Game1.tileSize * 2 + 10, Game1.tileSize * 4, Game1.tileSize), "Quality");
            this.UpArrow = new ClickableTextureComponent("up-arrow", new Rectangle(this.xPositionOnScreen + width - Game1.tileSize / 2, this.yPositionOnScreen - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            this.DownArrow = new ClickableTextureComponent("down-arrow", new Rectangle(this.xPositionOnScreen + width - Game1.tileSize / 2, this.yPositionOnScreen + height / 2 - Game1.tileSize * 2, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);

            {
                int i = -1;

                int x = (int)(this.xPositionOnScreen - Game1.tileSize * 5.3f);
                int y = this.yPositionOnScreen + 10;
                int lblHeight = (int)(Game1.tileSize * 0.9F);

                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "All"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Tools & Equipment"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Seeds & Crops"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Fish & Bait & Trash"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Forage & Fruits"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Artifacts & Minerals"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Resources & Crafting"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Artisan & Cooking"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Animal & Monster"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i++, Game1.tileSize * 5, Game1.tileSize), "Decorating"));
                this.Tabs.Add(new ClickableComponent(new Rectangle(x, y + lblHeight * i, Game1.tileSize * 5, Game1.tileSize), "Misc"));
            }

            if (!ItemMenu.ItemsLoaded)
                ItemMenu.LoadItems();

            this.AllowRightClick = true;
            this.Inventory.ShowGrayedOutSlots = true;

            switch (ItemMenu.SortID)
            {
                case 0:
                    this.SortButton.name = "Sort By: Name";
                    break;
                case 1:
                    this.SortButton.name = "Sort By: Category";
                    break;
                case 2:
                    this.SortButton.name = "Sort By: ID";
                    break;
            }

            this.LoadInventory();
        }

        private void LoadInventory()
        {
            List<Item> items = ItemMenu.ItemList.OrderBy(o => o.Name).ToList();

            switch (ItemMenu.SortID)
            {
                case 1:
                    items = ItemMenu.ItemList.OrderBy(o => o.category).ToList();
                    break;
                case 2:
                    items = ItemMenu.ItemList.OrderBy(o => o.parentSheetIndex).ToList();
                    break;
            }

            this.InventoryItems.Clear();
            foreach (Item item in items)
            {
                item.Stack = item.maximumStackSize();

                if (item is Object obj)
                    obj.quality = (int)ItemMenu.Quality;

                if (this.IsCategoryAllowed(item) && item.Name.ToLower().Contains(this.Textbox.Text.ToLower()))
                    this.InventoryItems.Add(item);
            }

            this.ItemsToGrabMenu = new ItemInventoryMenu(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen, false, this.InventoryItems);
        }
        private bool IsCategoryAllowed(Item item)
        {
            switch (ItemMenu.TabIndex)
            {
                case 0:
                    return true;
                case 1:
                    return (item is Tool || item.getCategoryName().Equals("Ring") || item is Hat || item is Boots);
                case 2:
                    return (item.getCategoryName().Equals("Seed") || item.getCategoryName().Equals("Vegetable") || item.getCategoryName().Equals("Fertilizer") || item.getCategoryName().Equals("Flower"));
                case 3:
                    return (item.getCategoryName().Equals("Fish") || item.getCategoryName().Equals("Bait") || item.getCategoryName().Equals("Trash") || item.getCategoryName().Equals("Fishing Tackle"));
                case 4:
                    return (item.getCategoryName().Equals("Forage") || item.getCategoryName().Equals("Fruit"));
                case 5:
                    return (item.getCategoryName().Equals("Artifact") || item.getCategoryName().Equals("Mineral"));
                case 6:
                    return (item.getCategoryName().Equals("Resource") || item.getCategoryName().Equals("Crafting") || item.category == -8 || item.category == -9);
                case 7:
                    return (item.getCategoryName().Equals("Artisan Goods") || item.getCategoryName().Equals("Cooking"));
                case 8:
                    return (item.getCategoryName().Equals("Animal Product") || item.getCategoryName().Equals("Monster Loot"));
                case 9:
                    return (item.getCategoryName().Equals("Furniture") || item.getCategoryName().Equals("Decor"));
                case 10:
                    return (item.getCategoryName().Trim().Equals(""));
                default:
                    return false;
            }
        }

        private static void LoadItems()
        {
            ItemMenu.ItemsLoaded = true;
            ItemMenu.ItemList = new List<Item>
            {
                ToolFactory.getToolFromDescription(0, 0),
                ToolFactory.getToolFromDescription(0, 1),
                ToolFactory.getToolFromDescription(0, 2),
                ToolFactory.getToolFromDescription(0, 3),
                ToolFactory.getToolFromDescription(0, 4),
                ToolFactory.getToolFromDescription(1, 0),
                ToolFactory.getToolFromDescription(1, 1),
                ToolFactory.getToolFromDescription(1, 2),
                ToolFactory.getToolFromDescription(1, 3),
                ToolFactory.getToolFromDescription(1, 4),
                ToolFactory.getToolFromDescription(2, 0),
                ToolFactory.getToolFromDescription(2, 1),
                ToolFactory.getToolFromDescription(2, 2),
                ToolFactory.getToolFromDescription(2, 3),
                ToolFactory.getToolFromDescription(3, 0),
                ToolFactory.getToolFromDescription(3, 1),
                ToolFactory.getToolFromDescription(3, 2),
                ToolFactory.getToolFromDescription(3, 3),
                ToolFactory.getToolFromDescription(3, 4),
                ToolFactory.getToolFromDescription(4, 0),
                ToolFactory.getToolFromDescription(4, 1),
                ToolFactory.getToolFromDescription(4, 2),
                ToolFactory.getToolFromDescription(4, 3),
                ToolFactory.getToolFromDescription(4, 4),
                new MilkPail(),
                new Shears(),
                new Pan()
            };

            foreach (KeyValuePair<string, string> o in CraftingRecipe.craftingRecipes)
            {
                CraftingRecipe rec = new CraftingRecipe(o.Key, false);
                Item item = rec.createItem();
                if (item != null)
                    ItemMenu.ItemList.Add(item);
            }

            for (int i = 0; i < 112; i++)
                ItemMenu.ItemList.Add(new Wallpaper(i) { category = -24 });

            for (int i = 0; i < 40; i++)
                ItemMenu.ItemList.Add(new Wallpaper(i, true) { category = -24 });

            foreach (KeyValuePair<int, string> o in Game1.content.Load<Dictionary<int, string>>("Data\\Boots"))
            {
                Item item = new Boots(o.Key);
                ItemMenu.ItemList.Add(item);
            }

            foreach (KeyValuePair<int, string> o in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
            {
                Item item = new Hat(o.Key);
                ItemMenu.ItemList.Add(item);
            }

            foreach (KeyValuePair<int, string> o in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture"))
            {
                Item item = new Furniture(o.Key, Vector2.Zero);

                if (o.Key == 1466 || o.Key == 1468)
                    item = new TV(o.Key, Vector2.Zero);
                ItemMenu.ItemList.Add(item);
            }

            foreach (KeyValuePair<int, string> o in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
            {
                Item item = new MeleeWeapon(o.Key);

                if (o.Key >= 32 && o.Key <= 34)
                    item = new Slingshot(o.Key);

                ItemMenu.ItemList.Add(item);
            }

            foreach (KeyValuePair<int, string> o in Game1.content.Load<Dictionary<int, string>>("Data\\Fish"))
            {
                Item item = new Object(o.Key, 999);
                item.category = -4;
                ItemMenu.ItemList.Add(item);
            }

            foreach (KeyValuePair<int, string> o in Game1.bigCraftablesInformation)
            {
                if (ItemMenu.HasItem(o.Key, o.Value.Split('/')[0]))
                    continue;

                ItemMenu.ItemList.Add(new Object(Vector2.Zero, o.Key));
            }

            foreach (KeyValuePair<int, string> o in Game1.objectInformation)
            {
                if (ItemMenu.HasItem(o.Key, o.Value.Split('/')[0]))
                    continue;

                string[] info = o.Value.Split('/');
                if (info.Length >= 3)
                {
                    if (info[3].StartsWith("Ring"))
                    {
                        ItemMenu.ItemList.Add(new Ring(o.Key));
                        continue;
                    }
                    Object item = new Object(o.Key, 1);
                    ItemMenu.ItemList.Add(item);

                    if (item.category == -79)
                    {
                        ItemMenu.ItemList.Add(new Object(Vector2.Zero, 348, item.Name + " Wine", false, true, false, false)
                        {
                            name = item.Name + " Wine",
                            price = item.price * 3
                        });
                    }
                    if (item.category == -75)
                    {
                        ItemMenu.ItemList.Add(new Object(Vector2.Zero, 350, item.Name + " Juice", false, true, false, false)
                        {
                            name = item.Name + " Juice",
                            price = (int)(item.price * 2.25d)
                        });
                    }

                    if (item.category == -79)
                    {
                        ItemMenu.ItemList.Add(new Object(Vector2.Zero, 344, item.Name + " Jelly", false, true, false, false)
                        {
                            name = item.Name + " Jelly",
                            price = 50 + item.Price * 2
                        });
                    }
                    if (item.category == -75)
                    {
                        ItemMenu.ItemList.Add(new Object(Vector2.Zero, 342, "Pickled " + item.Name, false, true, false, false)
                        {
                            name = "Pickled " + item.Name,
                            price = 50 + item.Price * 2
                        });
                    }
                }
            }
        }

        private static bool HasItem(int itemID, string name)
        {
            foreach (Item item in ItemMenu.ItemList)
            {
                if (item.parentSheetIndex == itemID && item.Name.Equals(name))
                    return true;
            }
            return false;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (this.TextboxBounds.Contains(x, y))
            {
                this.Textbox.Text = "";
                return;
            }
            if (!this.AllowRightClick)
                return;
            base.receiveRightClick(x, y, false);
            if (this.HeldItem == null && this.ShowReceivingMenu)
            {
                this.HeldItem = this.ItemsToGrabMenu.RightClick(x, y, this.HeldItem, false);
                if (this.HeldItem is Object obj && obj.parentSheetIndex == 326)
                {
                    this.HeldItem = null;
                    Game1.player.canUnderstandDwarves = true;
                    this.Poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % Game1.tileSize + Game1.tileSize / 4, y - y % Game1.tileSize + Game1.tileSize / 4), false, false);
                    Game1.playSound("fireball");
                }
                else if (this.HeldItem is Object recipe && recipe.isRecipe)
                {
                    string key = this.HeldItem.Name.Substring(0, recipe.Name.IndexOf("Recipe") - 1);
                    try
                    {
                        if (recipe.category == -7)
                            Game1.player.cookingRecipes.Add(key, 0);
                        else
                            Game1.player.craftingRecipes.Add(key, 0);
                        this.Poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % Game1.tileSize + Game1.tileSize / 4, y - y % Game1.tileSize + Game1.tileSize / 4), false, false);
                        Game1.playSound("newRecipe");
                    }
                    catch { }
                    this.HeldItem = null;
                }
                else
                {
                    if (!Game1.player.addItemToInventoryBool(this.HeldItem))
                        return;
                    this.HeldItem = null;
                    Game1.playSound("coin");
                }
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            ItemMenu.Open();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this.HeldItem == null)
            {
                for (int i = 0; i < this.Tabs.Count; i++)
                {
                    ClickableComponent tab = this.Tabs[i];
                    if (tab.bounds.Contains(x, y))
                    {
                        ItemMenu.TabIndex = i;
                        Game1.exitActiveMenu();
                        ItemInventoryMenu.ScrollIndex = 0;
                        ItemMenu.Open();
                        break;
                    }
                }

                if (this.SortButton.bounds.Contains(x, y))
                {
                    ItemMenu.SortID++;
                    if (ItemMenu.SortID > 2)
                        ItemMenu.SortID = 0;
                    ItemMenu.Open();
                }

                if (this.QualityButton.bounds.Contains(x, y))
                {
                    ItemMenu.Quality = ItemMenu.Quality != ItemMenu.Quality.GetNext()
                        ? ItemMenu.Quality.GetNext()
                        : ItemQuality.Normal;
                    ItemMenu.Open();
                }

                if (this.UpArrow.bounds.Contains(x, y))
                    this.ItemsToGrabMenu?.receiveScrollWheelAction(1);

                if (this.DownArrow.bounds.Contains(x, y))
                    this.ItemsToGrabMenu?.receiveScrollWheelAction(-1);
            }

            if (this.HeldItem == null && this.ShowReceivingMenu)
            {
                this.HeldItem = this.ItemsToGrabMenu.LeftClick(x, y, this.HeldItem, false);
                if (this.HeldItem is Object obj && obj.parentSheetIndex == 326)
                {
                    this.HeldItem = null;
                    Game1.player.canUnderstandDwarves = true;
                    this.Poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % Game1.tileSize + Game1.tileSize / 4, y - y % Game1.tileSize + Game1.tileSize / 4), false, false);
                    Game1.playSound("fireball");
                }
                else if (this.HeldItem is Object && (this.HeldItem as Object).parentSheetIndex == 102)
                {
                    this.HeldItem = null;
                    Game1.player.foundArtifact(102, 1);
                    this.Poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % Game1.tileSize + Game1.tileSize / 4, y - y % Game1.tileSize + Game1.tileSize / 4), false, false);
                    Game1.playSound("fireball");
                }
                else if (this.HeldItem is Object recipe && recipe.isRecipe)
                {
                    string key = recipe.Name.Substring(0, recipe.Name.IndexOf("Recipe") - 1);
                    try
                    {
                        if (recipe.category == -7)
                            Game1.player.cookingRecipes.Add(key, 0);
                        else
                            Game1.player.craftingRecipes.Add(key, 0);
                        this.Poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % Game1.tileSize + Game1.tileSize / 4, y - y % Game1.tileSize + Game1.tileSize / 4), false, false);
                        Game1.playSound("newRecipe");
                    }
                    catch { }
                    this.HeldItem = null;
                }
                else if (Game1.player.addItemToInventoryBool(this.HeldItem))
                {
                    this.HeldItem = null;
                    Game1.playSound("coin");
                }
            }
            if (this.HeldItem == null || this.isWithinBounds(x, y) || !this.HeldItem.canBeTrashed())
                return;
            Game1.playSound("throwDownITem");
            Game1.createItemDebris(this.HeldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
            this.HeldItem = null;

        }

        public static void OrganizeItemsInList(List<Item> items)
        {
            items.Sort();
            items.Reverse();
        }

        public bool AreAllItemsTaken()
        {
            return this.ItemsToGrabMenu.ActualInventory.All(t => t == null);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.Textbox.Selected)
                return;

            if ((this.CanExitOnKey || this.AreAllItemsTaken()) && (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose()))
            {
                this.exitThisMenu();
                if (Game1.currentLocation.currentEvent != null)
                    ++Game1.currentLocation.currentEvent.CurrentCommand;
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.HeldItem != null)
                Game1.setMousePosition(this.TrashCan.bounds.Center);
            if (key != Keys.Delete || this.HeldItem == null || !this.HeldItem.canBeTrashed())
                return;
            if (this.HeldItem is Object obj && Game1.player.specialItems.Contains(obj.parentSheetIndex))
                Game1.player.specialItems.Remove(obj.parentSheetIndex);
            this.HeldItem = null;
            Game1.playSound("trashcan");
        }

        public override void update(GameTime time)
        {
            if (!ItemMenu.TempText.Equals(this.Textbox.Text))
            {
                ItemMenu.TempText = this.Textbox.Text;
                ItemInventoryMenu.ScrollIndex = 0;
                this.LoadInventory();
            }

            base.update(time);
            if (this.Poof == null || !this.Poof.update(time))
                return;
            this.Poof = null;
        }

        public override void performHoverAction(int x, int y)
        {
            if (this.ItemsToGrabMenu.isWithinBounds(x, y) && this.ShowReceivingMenu)
                this.HoveredItem = this.ItemsToGrabMenu.Hover(x, y, this.HeldItem);
            else
                base.performHoverAction(x, y);

            this.Textbox.Selected = this.TextboxBounds.Contains(x, y);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
                return;

            if (this.HeldItem == null && this.HoveredItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
            {
                try
                {
                    Object obj = (Object)this.HoveredItem;
                    obj.quality = direction > 0
                        ? (int)((ItemQuality)obj.quality).GetNext()
                        : (int)((ItemQuality)obj.quality).GetPrevious();
                }
                catch { }
            }
            else
                this.ItemsToGrabMenu?.receiveScrollWheelAction(direction);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (!Game1.options.showMenuBackground)
                spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
            this.Textbox.Draw(spriteBatch);
            this.Draw(spriteBatch, false, false);
            if (this.ShowReceivingMenu)
            {
                CJB.DrawTextBox(this.Title.bounds.X, this.Title.bounds.Y, Game1.borderFont, this.Title.name, true, 2);
                Game1.drawDialogueBox(this.ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder, this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, this.ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2, false, true);
                this.ItemsToGrabMenu.draw(spriteBatch);
                for (int i = 0; i < this.Tabs.Count; i++)
                {
                    ClickableComponent current = this.Tabs[i];
                    CJB.DrawTextBox(current.bounds.X + current.bounds.Width, current.bounds.Y, Game1.smallFont, current.name, true, 2, ItemMenu.TabIndex == i ? 1F : 0.7F);
                }

                CJB.DrawTextBox(this.SortButton.bounds.X, this.SortButton.bounds.Y, Game1.smallFont, this.SortButton.name, true);
                CJB.DrawTextBox(this.QualityButton.bounds.X, this.QualityButton.bounds.Y, Game1.smallFont, this.QualityButton.name, true);

                this.UpArrow.draw(spriteBatch);
                this.DownArrow.draw(spriteBatch);
            }

            this.Poof?.draw(spriteBatch, true);
            if (this.HoverText != null && (this.HoveredItem == null || this.ItemsToGrabMenu == null))
                IClickableMenu.drawHoverText(spriteBatch, this.HoverText, Game1.smallFont);
            if (this.HoveredItem != null)
                IClickableMenu.drawToolTip(spriteBatch, this.HoveredItem.getDescription(), this.HoveredItem.Name, this.HoveredItem, this.HeldItem != null);
            else if (this.HoveredItem != null && this.ItemsToGrabMenu != null)
                IClickableMenu.drawToolTip(spriteBatch, this.ItemsToGrabMenu.DescriptionText, this.ItemsToGrabMenu.DescriptionTitle, this.HoveredItem, this.HeldItem != null);
            this.HeldItem?.drawInMenu(spriteBatch, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

            if (this.HoveredItem is Object obj)
            {
                if (obj.stack > 1)
                    this.DrawHoverTextBox(spriteBatch, Game1.smallFont, obj.sellToStorePrice(), obj.stack);
                else
                    this.DrawHoverTextBox(spriteBatch, Game1.smallFont, obj.sellToStorePrice());
            }
            else if (this.HoveredItem != null)
            {
                if (this.HoveredItem.Stack > 1)
                    this.DrawHoverTextBox(spriteBatch, Game1.smallFont, (this.HoveredItem.salePrice() / 2), this.HoveredItem.Stack);
                else
                    this.DrawHoverTextBox(spriteBatch, Game1.smallFont, this.HoveredItem.salePrice());
            }
            if (!Game1.options.hardwareCursor)
                spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }

        public static void Open()
        {
            Game1.activeClickableMenu = new ItemMenu(new List<Item>());
        }

        private void DrawHoverTextBox(SpriteBatch spriteBatch, SpriteFont font, int price, int stack = -1)
        {
            if (price < 1)
                return;

            string priceString = price.ToString();
            string stackPriceString = Environment.NewLine + (price * stack);

            string message = "Single: " + price;
            string message1 = "Single: ";

            if (stack > 1)
            {
                message += Environment.NewLine + "Stack: " + price * stack;
                message1 += Environment.NewLine + "Stack: ";
            }

            Vector2 bounds = font.MeasureString(message);
            int width = (int)bounds.X + Game1.tileSize / 2 + 40;
            int height = (int)font.MeasureString(message).Y + Game1.tileSize / 3 + 5;
            int x = Game1.getOldMouseX() - Game1.tileSize / 2 - width;
            int y = Game1.getOldMouseY() + Game1.tileSize / 2;

            if (x < 0)
                x = 0;
            if (y + height > Game1.graphics.GraphicsDevice.Viewport.Height)
                y = Game1.graphics.GraphicsDevice.Viewport.Height - height;
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White);
            spriteBatch.Draw(Game1.debrisSpriteSheet, new Vector2(x + Game1.tileSize / 4 + font.MeasureString(message + "   ").X, y + Game1.tileSize / 4 + 10), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16), Color.White, 0.0f, new Vector2(8f, 8f), Game1.pixelZoom, SpriteEffects.None, 1f);
            if (stack > 1)
                spriteBatch.Draw(Game1.debrisSpriteSheet, new Vector2(x + Game1.tileSize / 4 + font.MeasureString(message + "   ").X, y + Game1.tileSize / 4 + 38), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16), Color.White, 0.0f, new Vector2(8f, 8f), Game1.pixelZoom, SpriteEffects.None, 0.95f);
            Utility.drawTextWithShadow(spriteBatch, message1, font, new Vector2(x + Game1.tileSize / 4, y + Game1.tileSize / 4), Game1.textColor);

            Utility.drawTextWithShadow(spriteBatch, priceString, font, new Vector2(x + width + Game1.tileSize / 4 - 60 - font.MeasureString(priceString).X, y + Game1.tileSize / 4), Game1.textColor);
            if (stack > 1)
                Utility.drawTextWithShadow(spriteBatch, stackPriceString, font, new Vector2(x + width + Game1.tileSize / 4 - 60 - font.MeasureString(stackPriceString).X, y + Game1.tileSize / 4), Game1.textColor);
        }
    }
}
