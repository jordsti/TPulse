var WaypointPlaced = false;

var WaypointX = 0;
var WaypointY = 0;

function mouseclick(event)
{
	var mx = event.clientX;
	var my = event.clientY - 60;
	
	//calculate tile pos
	
	var col = mx / (TileWidth*ZoomRatio);
	var row = my / (TileHeight*ZoomRatio);
	
}