using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    public sealed class GameSession : MonoBehaviour
    {
        readonly List<BlueGem> gems = new List<BlueGem>();
        struct PickupToast { public Vector2 Viewport;public float Age;public int Kind;public bool Active; }
        readonly PickupToast[] toasts=new PickupToast[32];
        int nextToast;
        RunnerMagnet magnet;
        RunnerShield shield;
        public bool ShieldActive => shield!=null && ShieldRules.Protects(shield.Remaining);
        GUIStyle toastStyle;
        RunnerMotor runner;
        double distance;
        bool leaving;
#if !UNITY_EDITOR && !UNITY_STANDALONE
        bool menuPressed;
        Vector2 menuPressPosition;
#endif
        GameArt hudArt;
        Font hudFont;
        GUIStyle counterStyle, metricStyle, panelStyle, buttonStyle;
        public bool IsRunning => !RunEntryTransition.Active && runner!=null && !runner.Paused && !runner.Finished && !Defeated;
        public EndlessTrack Track { get; set; }
#if UNITY_EDITOR || UNITY_STANDALONE
        bool previewSafeArea;
#endif
        public bool Defeated { get; private set; }
        readonly RunPickups pickups = new RunPickups();
        public int GemCount => pickups.Blue;
        public long Score => (long)distance + pickups.Points;
        public string DefeatReason { get; private set; }

        public void Initialize(RunnerMotor player)
        {
            runner = player; player.Session = this;

            hudArt=Resources.Load<GameArt>("Presentation/GameArt");
            hudFont=Resources.Load<Font>("Presentation/alagard");
            RunnerInput.BlocksPointer = BlocksPointer;
            RunnerInput.TouchButtonAt = TouchButtonAt;
        }
        void OnDestroy()
        {
            if (RunnerInput.BlocksPointer == BlocksPointer) RunnerInput.BlocksPointer = null;
            if (RunnerInput.TouchButtonAt == TouchButtonAt) RunnerInput.TouchButtonAt = null;
        }
        int TouchButtonAt(Vector2 screenPosition)
        {
            Rect safe = SafeArea();
            float scale = UiScale(safe);
            if (scale <= 0) return 0;
            float width = safe.width / scale, height = safe.height / scale;
            Vector2 point = new Vector2((screenPosition.x - safe.x) / scale, (safe.yMax - screenPosition.y) / scale);
            if (new Rect(16, height - 68, (width - 40) / 2, 52).Contains(point)) return 1;
            if (new Rect(24 + (width - 40) / 2, height - 68, (width - 40) / 2, 52).Contains(point)) return 2;
            return 0;
        }

        bool CanReturn => runner != null && (Defeated || runner.Paused || runner.Finished);
        Rect MenuRect(float width,float height)
        {
            float panelWidth=Mathf.Min(430,width-32);
            return new Rect((width-panelWidth)/2+12,Mathf.Max(82,height*.31f)+88,panelWidth-24,48);
        }
        void SaveBest()
        {
            long best;
            long.TryParse(PlayerPrefs.GetString("RollingPanda.BestScore","0"),out best);
            if(Score>best) { PlayerPrefs.SetString("RollingPanda.BestScore",Score.ToString()); PlayerPrefs.Save(); }
        }
        void ReturnToMenu()
        {
            if(leaving || !Application.CanStreamedLevelBeLoaded("StartMenu")) return;
            leaving=true;SaveBest();RunnerInput.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("StartMenu");
        }
        void Update()
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            var touch=UnityEngine.InputSystem.Touchscreen.current;
            if(CanReturn && touch!=null)
            {
                var t=touch.primaryTouch;Rect safe=SafeArea();float scale=UiScale(safe);
                if(scale>0)
                {
                    Vector2 p=new Vector2((t.position.ReadValue().x-safe.x)/scale,(safe.yMax-t.position.ReadValue().y)/scale);
                    Rect r=MenuRect(safe.width/scale,safe.height/scale);
                    if(t.press.wasPressedThisFrame) { menuPressed=r.Contains(p);menuPressPosition=p; }
                    if(t.press.wasReleasedThisFrame)
                    {
                        if(menuPressed && r.Contains(p) && Vector2.Distance(p,menuPressPosition)<24) ReturnToMenu();
                        menuPressed=false;
                    }
                }
            }
            else menuPressed=false;
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f8Key.wasPressedThisFrame)
            { previewSafeArea = !previewSafeArea; RunnerInput.Reset(); }
#endif
        }
        Rect SafeArea()
        {
            Rect safe = Screen.safeArea;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (previewSafeArea)
            {
                float unit = Mathf.Min(Screen.width, Screen.height);
                safe = new Rect(0, unit * 0.04f, Screen.width, Screen.height - unit * 0.12f);
            }
#endif
            return safe;
        }
        float UiScale(Rect safe) => Mathf.Min(safe.width / 390f, safe.height / 500f);
        bool BlocksPointer(Vector2 screenPosition)
        {
            Rect safe = SafeArea();
            if (!safe.Contains(screenPosition)) return true;
            float scale = UiScale(safe);
            float yFromTop = safe.yMax - screenPosition.y;
            if(CanReturn && MenuRect(safe.width/scale,safe.height/scale).Contains(new Vector2((screenPosition.x-safe.x)/scale,yFromTop/scale))) return true;
            return yFromTop < 70f * scale || yFromTop > safe.height - 80f * scale;
        }
        public void RegisterGem(BlueGem gem) { gems.Add(gem); gem.Initialize(this); }
        public void CollectGem(PickupKind kind,Vector3 worldPosition)
        {
            if(!IsRunning) return;
            pickups.Collect(kind);
            var camera=Camera.main;
            if(camera!=null)
            {
                Vector3 p=camera.WorldToViewportPoint(worldPosition);
                if(p.z>0) {toasts[nextToast]=new PickupToast{Viewport=p,Kind=(int)kind,Active=true};nextToast=(nextToast+1)%toasts.Length;}
            }
        }

        void LateUpdate()
        {
            if(shield==null && Track!=null) {shield=gameObject.AddComponent<RunnerShield>();shield.Initialize(runner,this);}
            if(magnet==null && Track!=null) {magnet=gameObject.AddComponent<RunnerMagnet>();magnet.Initialize(runner,this,gems);}
            if(IsRunning) for(int i=0;i<toasts.Length;i++) if(toasts[i].Active)
            {toasts[i].Age+=Time.deltaTime;if(toasts[i].Age>=.7f) toasts[i].Active=false;}
            if (runner != null && !runner.Paused)
                distance = System.Math.Max(distance, runner.DistanceTravelled);
        }

        public void HitObstacle(string reason) { if(!ShieldActive) Fail(reason); }

        public void Fail(string reason)
        {
            if (Defeated) return;
            distance = System.Math.Max(distance, runner.DistanceTravelled);
            Defeated = true;
            DefeatReason = reason;
            SaveBest();
        }

        public void ResetRun()
        {
            Defeated = false;
            System.Array.Clear(toasts,0,toasts.Length);nextToast=0;if(shield!=null) shield.ResetPowerup();if(magnet!=null) magnet.ResetPowerup();
            DefeatReason = null;
            pickups.Reset();
            distance = 0f;
            foreach (var gem in gems) gem.ResetGem();
            if (Track != null) Track.ResetTrack();
        }

        void OnGUI()
        {
            if(runner==null || RunEntryTransition.Active) return;
            Rect safe=SafeArea();float scale=UiScale(safe);if(scale<=0) return;
            if(counterStyle==null)
            {
                counterStyle=new GUIStyle(GUI.skin.label){font=hudFont,fontSize=18,alignment=TextAnchor.MiddleLeft,wordWrap=false};
                metricStyle=new GUIStyle(counterStyle){fontSize=18,alignment=TextAnchor.MiddleRight};
                panelStyle=new GUIStyle(counterStyle){fontSize=19,alignment=TextAnchor.MiddleCenter,wordWrap=true};
                toastStyle=new GUIStyle(counterStyle){fontSize=14,alignment=TextAnchor.MiddleCenter};
                buttonStyle=new GUIStyle(GUI.skin.button){font=hudFont,fontSize=22};
            }
            Matrix4x4 previous=GUI.matrix;
            float width=safe.width/scale,height=safe.height/scale;
            GUI.matrix=Matrix4x4.TRS(new Vector3(safe.x,Screen.height-safe.yMax,0),Quaternion.identity,new Vector3(scale,scale,1));
            GUI.Box(new Rect(10,10,204,44),"");
            DrawCounter(18,pickups.Blue,0);DrawCounter(82,pickups.Red,1);DrawCounter(146,pickups.Pouches,2);
            bool wide=width>=600;
            float metricsWidth=wide ? 240 : 150;
            float right=width-12;
            GUI.Box(new Rect(right-metricsWidth,10,metricsWidth,wide ? 44 : 56),"");
            if(wide)
            {
                GUI.Label(new Rect(right-230,15,130,32),$"Score {Score}",metricStyle);
                GUI.Label(new Rect(right-94,15,84,32),$"{(long)distance} m",metricStyle);
            }
            else
            {
                GUI.Label(new Rect(right-144,10,136,28),$"Score {Score}",metricStyle);
                GUI.Label(new Rect(right-144,34,136,28),$"{(long)distance} m",metricStyle);
            }
            if(Defeated || runner.Paused || runner.Finished)
            {
                float panelWidth=Mathf.Min(430,width-32),left=(width-panelWidth)/2,top=Mathf.Max(82,height*.31f);
                GUI.Box(new Rect(left,top,panelWidth,150),"");
                string state=Defeated ? "Run ended" : runner.Finished ? "Course complete!" : "Paused";
                GUI.Label(new Rect(left+12,top+10,panelWidth-24,30),state,panelStyle);
                GUI.Label(new Rect(left+12,top+44,panelWidth-24,38),Defeated ? DefeatReason : "Swipe left/right: lanes   Up: jump   Down: slide",panelStyle);
                bool menuClicked=GUI.Button(MenuRect(width,height),"Main menu",buttonStyle);
#if UNITY_EDITOR || UNITY_STANDALONE
                if(menuClicked) ReturnToMenu();
#endif
            }
            bool restartClicked=GUI.Button(new Rect(16,height-68,(width-40)/2,52),"Restart",buttonStyle);
            bool pauseClicked=GUI.Button(new Rect(24+(width-40)/2,height-68,(width-40)/2,52),runner.Paused ? "Resume" : "Pause",buttonStyle);
#if UNITY_EDITOR || UNITY_STANDALONE
            if(restartClicked) runner.Restart();
            if(pauseClicked) runner.TogglePause();
#endif
            if(magnet!=null && magnet.Remaining>0)
                GUI.Label(new Rect(18,57,205,25),"Magnet  "+Mathf.CeilToInt(magnet.Remaining)+"s",counterStyle);
            if(shield!=null && shield.Remaining>0)
                GUI.Label(new Rect(18,magnet!=null && magnet.Remaining>0 ? 82 : 57,205,25),"Shield  "+Mathf.CeilToInt(shield.Remaining)+"s",counterStyle);
            DrawToasts(safe,scale);
            GUI.matrix=previous;
        }
        void DrawToasts(Rect safe,float scale)
        {
            Color before=GUI.color;
            for(int i=0;i<toasts.Length;i++)
            {
                var t=toasts[i];if(!t.Active) continue;
                Vector2 start=new Vector2((t.Viewport.x*Screen.width-safe.x)/scale,(safe.yMax-t.Viewport.y*Screen.height)/scale);
                Vector2 lift=start+Vector2.up*22;
                Vector2 target=new Vector2(t.Kind==0 ? 32 : t.Kind==1 ? 96 : 160,32);
                Vector2 p=t.Age<.16f ? Vector2.Lerp(start,lift,t.Age/.16f) : Vector2.Lerp(lift,target,Mathf.SmoothStep(0,1,(t.Age-.16f)/.54f));
                Color color=t.Kind==0 ? new Color(.35f,.85f,1) : t.Kind==1 ? new Color(1,.35f,.4f) : new Color(.4f,1,.5f);
                color.a=1-Mathf.InverseLerp(.55f,.7f,t.Age);GUI.color=color;
                GUI.Label(new Rect(p.x-15,p.y-10,30,20),"+1",toastStyle);
            }
            GUI.color=before;
        }
        void DrawCounter(float x,int count,int kind)
        {
            Texture2D icon=hudArt==null ? null : hudArt.Icon(kind);
            if(icon!=null) GUI.DrawTexture(new Rect(x,16,28,32),icon,ScaleMode.ScaleToFit,true);
            else
            {
                Color previous=GUI.color;GUI.color=kind==0 ? Color.cyan : kind==1 ? Color.red : Color.green;
                GUI.Label(new Rect(x,17,28,30),"*",counterStyle);GUI.color=previous;
            }
            string display=count>=10000 ? (count/1000f).ToString("0.#")+"k" : count.ToString();
            GUI.Label(new Rect(x+29,16,35,32),new GUIContent(display,count.ToString()),counterStyle);
        }
    }
}



