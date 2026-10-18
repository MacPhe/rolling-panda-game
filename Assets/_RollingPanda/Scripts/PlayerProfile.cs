using System;
using UnityEngine;
namespace RollingPanda
{
    public static class PlayerProfile
    {
        public const string SaveKey="RollingPanda.Profile.v1";
        public const string BackupKey="RollingPanda.Profile.backup.v1";
        [Serializable] sealed class SaveData {public int version=1,green;public string[] owned,equipped;}
        static ShopInventory inventory;static string validSave;static bool dirty;
        public static int Revision {get;private set;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reload(){inventory=null;validSave=null;dirty=false;Revision++;}
        public static ShopInventory Inventory
        {
            get
            {
                if(inventory==null)
                {
                    string raw=PlayerPrefs.GetString(SaveKey,"");
                    if(!TryRead(raw) && !TryRead(PlayerPrefs.GetString(BackupKey,"")))inventory=new ShopInventory();
                }
                return inventory;
            }
        }
        static bool TryRead(string raw)
        {
            if(string.IsNullOrEmpty(raw))return false;
            try{var data=JsonUtility.FromJson<SaveData>(raw);if(data==null || data.version!=1 || data.green<0 || data.owned==null || data.equipped==null)return false;inventory=new ShopInventory(data.green,data.owned,data.equipped);validSave=raw;return true;}catch(ArgumentException){return false;}
        }
        public static void EarnGreen(int amount){if(amount<=0)return;Inventory.Earn(amount);dirty=true;}
        public static ShopResult BuyOrEquip(string id)
        {
            var result=Inventory.BuyOrEquip(id);
            if(result==ShopResult.Purchased || result==ShopResult.Equipped){Revision++;dirty=true;Save();}
            return result;
        }
        public static void Save()
        {
            if(!dirty)return;
            string raw=JsonUtility.ToJson(new SaveData{green=Inventory.Green,owned=Inventory.Unlocks(),equipped=Inventory.Selection()});
            if(!string.IsNullOrEmpty(validSave))PlayerPrefs.SetString(BackupKey,validSave);
            PlayerPrefs.SetString(SaveKey,raw);PlayerPrefs.Save();validSave=raw;dirty=false;
        }
    }
}
