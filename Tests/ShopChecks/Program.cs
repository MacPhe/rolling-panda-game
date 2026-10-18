using System;
using RollingPanda;
static class Program
{
 static void Check(bool test,string message){if(!test)throw new Exception(message);}
 static void Main()
 {
  var empty=new ShopInventory();Check(empty.BuyOrEquip("outfit.kimono")==ShopResult.NotEnough && empty.Green==0,"No overdraft");
  Check(empty.BuyOrEquip("missing")==ShopResult.Invalid,"Unknown item");
  empty.Earn(500);Check(empty.BuyOrEquip("outfit.samurai")==ShopResult.Purchased && empty.Green==250,"Purchase exact price");
  Check(empty.BuyOrEquip("outfit.samurai")==ShopResult.Equipped && empty.Green==250,"No duplicate charge");
  Check(empty.BuyOrEquip("outfit.none")==ShopResult.Equipped && empty.Green==250,"Unequip remains owned");
  var restored=new ShopInventory(empty.Green,empty.Unlocks(),empty.Selection());Check(restored.Owns("outfit.samurai") && restored.Equipped(CosmeticSlot.Outfit)=="outfit.none","Restore unlocks and selection");
  var invalid=new ShopInventory(-2,new[]{"missing"},new[]{"outfit.samurai","band.gold"});Check(invalid.Green==0 && invalid.Equipped(CosmeticSlot.Coat)=="coat.classic" && invalid.Equipped(CosmeticSlot.Headband)=="band.red","Reject unowned or mismatched selection");
  empty.Earn(int.MaxValue);empty.Earn(int.MaxValue);Check(empty.Green==int.MaxValue,"Wallet cannot overflow");
  var random=new Random(18);var state=new ShopInventory();long earned=0,spent=0;
  for(int n=0;n<100000;n++)
  {
   if(random.Next(3)==0){int amount=random.Next(1,51);state.Earn(amount);earned+=amount;}
   else{var item=ShopCatalog.Items[random.Next(ShopCatalog.Items.Length)];int before=state.Green;bool owned=state.Owns(item.Id);var result=state.BuyOrEquip(item.Id);if(result==ShopResult.Purchased){spent+=item.Price;Check(!owned && before-state.Green==item.Price,"Charge once");}else Check(before==state.Green,"Failed/equipped transaction preserves wallet");}
   Check(state.Green==earned-spent,"Conservation of green gems");
   if(n%100==0)state=new ShopInventory(state.Green,state.Unlocks(),state.Selection());
   for(int slot=0;slot<5;slot++){var item=ShopCatalog.Find(state.Equipped((CosmeticSlot)slot));Check(item!=null && (int)item.Slot==slot && state.Owns(item.Id),"Only owned items in correct slots");}
  }
  Console.WriteLine("Shop checks passed: insufficient funds, duplicate charge, unequip, restore validation, overflow; 100,000 earning/purchase/equip actions with save round trips.");
 }
}
