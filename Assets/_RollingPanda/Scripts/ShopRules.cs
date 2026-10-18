using System;
using System.Collections.Generic;
namespace RollingPanda
{
    public enum CosmeticSlot { Coat,Headband,Belt,Outfit,Hat }
    public enum ShopResult { Invalid,NotEnough,Purchased,Equipped }
    public sealed class CosmeticItem
    {
        public readonly string Id,Name,Rarity;public readonly CosmeticSlot Slot;public readonly int Price,Variant;
        public CosmeticItem(string id,string name,CosmeticSlot slot,string rarity,int price,int variant){Id=id;Name=name;Slot=slot;Rarity=rarity;Price=price;Variant=variant;}
    }
    public static class ShopCatalog
    {
        public static readonly CosmeticItem[] Items={
            new CosmeticItem("coat.classic","Classic panda",CosmeticSlot.Coat,"Starter",0,0),
            new CosmeticItem("coat.moon","Moonlit blue coat",CosmeticSlot.Coat,"Uncommon",15,1),
            new CosmeticItem("coat.plum","Plum blossom coat",CosmeticSlot.Coat,"Rare",35,2),
            new CosmeticItem("coat.copper","Copper coat",CosmeticSlot.Coat,"Rare",35,3),
            new CosmeticItem("band.red","Red headband",CosmeticSlot.Headband,"Starter",0,0),
            new CosmeticItem("band.blue","Blue headband",CosmeticSlot.Headband,"Common",5,1),
            new CosmeticItem("band.jade","Jade headband",CosmeticSlot.Headband,"Uncommon",15,2),
            new CosmeticItem("band.gold","Golden headband",CosmeticSlot.Headband,"Rare",35,3),
            new CosmeticItem("belt.none","No karate belt",CosmeticSlot.Belt,"Starter",0,0),
            new CosmeticItem("belt.white","White karate belt",CosmeticSlot.Belt,"Common",10,1),
            new CosmeticItem("belt.black","Black karate belt",CosmeticSlot.Belt,"Rare",40,2),
            new CosmeticItem("outfit.none","No outfit",CosmeticSlot.Outfit,"Starter",0,0),
            new CosmeticItem("outfit.kimono","Blossom kimono",CosmeticSlot.Outfit,"Rare",100,1),
            new CosmeticItem("outfit.samurai","Forest samurai",CosmeticSlot.Outfit,"Epic",250,2),
            new CosmeticItem("hat.none","No hat",CosmeticSlot.Hat,"Starter",0,0),
            new CosmeticItem("hat.straw","Bamboo straw hat",CosmeticSlot.Hat,"Uncommon",25,1)
        };
        public static CosmeticItem Find(string id){foreach(var item in Items)if(item.Id==id)return item;return null;}
    }
    // Pure economy logic: purchases are permanent unlocks, equipment occupies one item per slot.
    public sealed class ShopInventory
    {
        public int Green {get;private set;}
        readonly HashSet<string> owned=new HashSet<string>();
        readonly string[] equipped=new string[5];
        public ShopInventory(int green=0,IEnumerable<string> unlocks=null,string[] selection=null)
        {
            Green=Math.Max(0,green);
            foreach(var item in ShopCatalog.Items)if(item.Price==0){owned.Add(item.Id);equipped[(int)item.Slot]=item.Id;}
            if(unlocks!=null)foreach(var id in unlocks)if(ShopCatalog.Find(id)!=null)owned.Add(id);
            if(selection!=null)for(int i=0;i<Math.Min(5,selection.Length);i++){var item=ShopCatalog.Find(selection[i]);if(item!=null && (int)item.Slot==i && owned.Contains(item.Id))equipped[i]=item.Id;}
        }
        public bool Owns(string id)=>id!=null && owned.Contains(id);
        public string Equipped(CosmeticSlot slot)=>equipped[(int)slot];
        public string[] Selection()=>(string[])equipped.Clone();
        public string[] Unlocks(){var result=new List<string>();foreach(var item in ShopCatalog.Items)if(Owns(item.Id))result.Add(item.Id);return result.ToArray();}
        public void Earn(int amount){if(amount>0)Green=(int)Math.Min(int.MaxValue,(long)Green+amount);}
        public ShopResult BuyOrEquip(string id)
        {
            var item=ShopCatalog.Find(id);if(item==null)return ShopResult.Invalid;
            bool purchased=!Owns(id);
            if(purchased){if(Green<item.Price)return ShopResult.NotEnough;Green-=item.Price;owned.Add(id);}
            equipped[(int)item.Slot]=id;return purchased ? ShopResult.Purchased : ShopResult.Equipped;
        }
    }
}
