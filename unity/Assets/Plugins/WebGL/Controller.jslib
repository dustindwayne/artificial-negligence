mergeInto(LibraryManager.library, {
 AN_ReadPad: function(ptr) {
  var pads = navigator.getGamepads ? navigator.getGamepads() : [];
  var pad = null;
  for(var i=0;i<pads.length;i++) if(pads[i] && pads[i].connected && pads[i].mapping === 'standard') {pad=pads[i];break;}
  if(!pad) return 0;
  function held(i){return pad.buttons[i] && (pad.buttons[i].pressed || pad.buttons[i].value > .4);}
  var x=held(14)?-1:held(15)?1:(pad.axes[0]||0);
  var y=held(12)?1:held(13)?-1:-(pad.axes[1]||0);
  var b=0, ids=[0,1,2,3,4,5,8,9,6,7];
  for(var j=0;j<ids.length;j++)if(held(ids[j]))b|=1<<j;
  HEAPF32[ptr>>2]=x;HEAPF32[(ptr+4)>>2]=y;HEAPF32[(ptr+8)>>2]=b;return 1;
 }
});
