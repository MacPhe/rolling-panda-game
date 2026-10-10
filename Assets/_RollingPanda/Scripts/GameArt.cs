using UnityEngine;
namespace RollingPanda
{
    public sealed class GameArt : ScriptableObject
    {
        public Mesh Diamond;
        public Texture2D BlueIcon, RedIcon, PouchIcon;
        public Texture2D Icon(int kind) => kind==0 ? BlueIcon : kind==1 ? RedIcon : PouchIcon;
    }
}

