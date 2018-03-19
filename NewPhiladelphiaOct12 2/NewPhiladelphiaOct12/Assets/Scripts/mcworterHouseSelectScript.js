var titleTime: float=10;
var showTitle= false;
var customSkin : GUISkin;

function Update () {
//This timer limits the time in which the title is displayed
titleTime -= Time.deltaTime;
 if( titleTime <= 0){
showTitle= false;
  }

}

function OnMouseDown() {
titleTime= 10;
showTitle= true;
}

function OnGUI() {
GUI.skin = customSkin;

if(showTitle){
GUI.Box(Rect(Screen.width/2 -Screen.width/4,Screen.height -75,Screen.width/2,Screen.height/4),"The Louisa McWorter House", "title");
}
}