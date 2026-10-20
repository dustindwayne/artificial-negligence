using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace ArtificialNegligence {
 [Flags] public enum PadButton {A=1,B=2,X=4,Y=8,LB=16,RB=32,Back=64,Start=128,LT=256,RT=512}
 public struct PadFrame {public bool connected;public float x,y;public PadButton buttons;public bool Held(PadButton b){return (buttons&b)!=0;}}
 public static class ControllerDevice {
  public static float Axis(float v){return Mathf.Abs(v)<.22f?0:Mathf.Sign(v)*Mathf.Clamp01((Mathf.Abs(v)-.22f)/.78f);}
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] static extern int AN_ReadPad(float[] data);
  static readonly float[] web=new float[4];
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
  [StructLayout(LayoutKind.Sequential)] struct State {public uint packet;public ushort buttons;public byte lt,rt;public short lx,ly,rx,ry;}
  [DllImport("xinput1_4.dll",EntryPoint="XInputGetState")] static extern uint Read4(uint index,out State state);
  [DllImport("xinput1_3.dll",EntryPoint="XInputGetState")] static extern uint Read3(uint index,out State state);
  static int slot=-1;static bool oldLibrary;
  static bool Read(int index,out State state){try{return (oldLibrary?Read3((uint)index,out state):Read4((uint)index,out state))==0;}catch(DllNotFoundException){oldLibrary=true;try{return Read3((uint)index,out state)==0;}catch{state=default;return false;}}}
#endif
  public static void VerifyPlatformAdapter(){
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
   UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
   var device=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();device.MakeCurrent();
   try{
    var state=new UnityEngine.InputSystem.LowLevel.GamepadState{leftStick=new Vector2(-1,1),leftTrigger=1,rightTrigger=1};
    foreach(var key in new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.South,UnityEngine.InputSystem.LowLevel.GamepadButton.East,UnityEngine.InputSystem.LowLevel.GamepadButton.West,UnityEngine.InputSystem.LowLevel.GamepadButton.North,UnityEngine.InputSystem.LowLevel.GamepadButton.LeftShoulder,UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder,UnityEngine.InputSystem.LowLevel.GamepadButton.Select,UnityEngine.InputSystem.LowLevel.GamepadButton.Start,UnityEngine.InputSystem.LowLevel.GamepadButton.DpadRight,UnityEngine.InputSystem.LowLevel.GamepadButton.DpadDown})state=state.WithButton(key);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(device,state);UnityEngine.InputSystem.InputSystem.Update();var frame=Poll();
    if(!frame.connected||frame.x!=1||frame.y!=-1||(int)frame.buttons!=1023)throw new Exception("Linux gamepad adapter mapping failed / "+frame.connected+" / "+frame.x+" / "+frame.y+" / "+(int)frame.buttons);
    UnityEngine.InputSystem.InputSystem.RemoveDevice(device);if(Poll().connected)throw new Exception("Linux virtual controller disconnect failed");
    Debug.Log("AN_LINUX_CONTROLLER_ADAPTER_PASS / all ten buttons, sticks, D-pad priority, disconnect using a virtual Input System gamepad");
   }finally{if(device.added)UnityEngine.InputSystem.InputSystem.RemoveDevice(device);}
#endif
  }
  public static PadFrame Poll(){
#if UNITY_WEBGL && !UNITY_EDITOR
   if(AN_ReadPad(web)==0)return default;return new PadFrame{connected=true,x=Axis(web[0]),y=Axis(web[1]),buttons=(PadButton)(int)web[2]};
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
   State s=default;bool found=slot>=0&&Read(slot,out s);if(!found){slot=-1;for(int i=0;i<4;i++)if(Read(i,out s)){slot=i;found=true;break;}}if(!found)return default;
   PadButton b=0;ushort n=s.buttons;if((n&0x1000)!=0)b|=PadButton.A;if((n&0x2000)!=0)b|=PadButton.B;if((n&0x4000)!=0)b|=PadButton.X;if((n&0x8000)!=0)b|=PadButton.Y;if((n&0x100)!=0)b|=PadButton.LB;if((n&0x200)!=0)b|=PadButton.RB;if((n&0x20)!=0)b|=PadButton.Back;if((n&0x10)!=0)b|=PadButton.Start;if(s.lt>100)b|=PadButton.LT;if(s.rt>100)b|=PadButton.RT;
   float x=(n&4)!=0?-1:(n&8)!=0?1:s.lx/32767f,y=(n&1)!=0?1:(n&2)!=0?-1:s.ly/32767f;
   return new PadFrame{connected=true,x=Axis(x),y=Axis(y),buttons=b};
#elif UNITY_STANDALONE_LINUX
   var gamepad=UnityEngine.InputSystem.Gamepad.current;if(gamepad==null||!gamepad.added||!gamepad.enabled)return default;
   var stick=gamepad.leftStick.ReadUnprocessedValue();var cross=gamepad.dpad.ReadValue();PadButton buttons=0;
   if(gamepad.buttonSouth.isPressed)buttons|=PadButton.A;if(gamepad.buttonEast.isPressed)buttons|=PadButton.B;if(gamepad.buttonWest.isPressed)buttons|=PadButton.X;if(gamepad.buttonNorth.isPressed)buttons|=PadButton.Y;
   if(gamepad.leftShoulder.isPressed)buttons|=PadButton.LB;if(gamepad.rightShoulder.isPressed)buttons|=PadButton.RB;if(gamepad.selectButton.isPressed)buttons|=PadButton.Back;if(gamepad.startButton.isPressed)buttons|=PadButton.Start;
   if(gamepad.leftTrigger.isPressed)buttons|=PadButton.LT;if(gamepad.rightTrigger.isPressed)buttons|=PadButton.RT;
   return new PadFrame{connected=true,x=cross.x!=0?Mathf.Sign(cross.x):Axis(stick.x),y=cross.y!=0?Mathf.Sign(cross.y):Axis(stick.y),buttons=buttons};
#else
   return default;
#endif
  }
 }
}
