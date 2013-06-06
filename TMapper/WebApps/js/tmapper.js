var TileWidth = 100;
var TileHeight = 100;

var CurRow = 0;
var CurCol = 0;

var MapRows = 18
var MapCols = 63

var mapwidth = 0;
var mapheight = 0;

var ZoomRatio = 1.0;


var WaypointPlaced = false;

var WaypointX = 0;
var WaypointY = 0;

function mouseclick(event)
{
	var mx = event.clientX;
	var my = event.clientY - 400;
	
	//calculate tile pos
	
	var col = mx / (TileWidth*ZoomRatio);
	var row = my / (TileHeight*ZoomRatio);
	
	WaypointX = Math.floor(col + CurCol);
	WaypointY = Math.floor(row + CurRow);
	
	console.log(WaypointX + ";"+ WaypointY);
	
	WaypointPlaced = true;
	
	drawMap();
}

function initTMapper()
{
	var title = document.getElementById('worldname');
	
	title.innerHTML = WorldName;
	onLoadMapper();
	
	var date = document.getElementById('generatedate');
	
	date.innerHTML = 'Generated on ' + GeneratedOn;
}

function pad(num, size) {
    var s = num+"";
    while (s.length < size) s = "0" + s;
    return s;
}

function keyDown(event)
{
	if(event.keyCode == 37)
	{
		//left arrow
		if(CurCol - 1 >= 0)
		{
			CurCol--;
			drawMap();
		}
	}
	else if(event.keyCode == 38)
	{
		//up arrow
		if(CurRow - 1 >= 0)
		{
			CurRow--;
			drawMap();
		}
	}
	else if(event.keyCode == 39)
	{
		//right arrow
		if(CurCol + 1 < MapCols - ( mapwidth / (TileWidth*ZoomRatio) ) )
		{
			CurCol++;
			drawMap();
		}
	}
	else if(event.keyCode == 40)
	{
		//down arrow
		if(CurRow + 1 < MapRows - ( mapheight / (TileHeight*ZoomRatio) ) )
		{
			CurRow++;
			drawMap();
		}
	}
}

function drawMap()
{
	cleanImgs();
	onLoadMapper();
}

function cleanImgs()
{
	var mapdiv = document.getElementById("mapview");
	
	while(mapdiv.hasChildNodes())
	{
		mapdiv.removeChild(mapdiv.firstChild);
	}
}


function updateZoomForm()
{
	var tzoom = document.getElementById("tzoomratio");
	tzoom.value = ZoomRatio;
}

function onmouseWheel(event)
{
	if(event.wheelDelta == -120 && ZoomRatio > 0.5)
	{
		ZoomRatio = ZoomRatio - 0.1;
		drawMap();
		updateZoomForm();
	}
	else if(event.wheelDelta == 120 && ZoomRatio < 5)
	{
		ZoomRatio = ZoomRatio + 0.1;
		drawMap();
		updateZoomForm();
	}
}

function onLoadMapper()
{
	//Getting the container
	var mapdiv = document.getElementById("mapview");
	
var viewportWidth  = document.documentElement.clientWidth
  , viewportHeight = document.documentElement.clientHeight;
	
	mapheight = viewportHeight - 150;
	mapwidth = viewportWidth - 20;
	
	
	mapdiv.style.width = mapwidth + "px";
	mapdiv.style.height = mapheight + "px";

	var rows = mapheight / (TileHeight * ZoomRatio);
	var cols = mapwidth / (TileWidth * ZoomRatio);
	
	for(var ir = 0; ir < rows; ir++)
	{
		var tr = ir + CurRow;
		if(tr < MapRows)
		{
			for(var ic = 0; ic < cols; ic++)
			{
				var tc = ic + CurCol;
				if(tc < MapCols)
				{
					var imgid = pad(tc, 2) + "_" + pad(tr, 2);
					
					var element = document.getElementById(imgid);
					
					
					if(!element)
					{
						element = document.createElement('img');
						element.setAttribute('id',imgid);
						element.src = "maps/"+ pad(tc, 2) + "_" + pad(tr, 2) + ".png";
						element.setAttribute('class','imgmap');
						element.style.position = "relative";
						mapdiv.appendChild(element);
					}
					
					element.top = (ir*TileHeight * ZoomRatio) + "px";
					element.left = (ic*TileWidth * ZoomRatio) + "px";
					element.width = TileWidth * ZoomRatio;
					element.height = TileHeight * ZoomRatio;
				}
			}
			
			var ebr = document.createElement('br');
			
			mapdiv.appendChild(ebr);
		}
	}
	
	//draw waypoint
	
	if(WaypointPlaced)
	{
		var waypoint = document.getElementById('waypoint');
		if(!waypoint)
		{
			waypoint = document.createElement('img');
			waypoint.setAttribute('id', 'waypoint');
			waypoint.src = 'img/waypoint.png';
			
			mapdiv.appendChild(waypoint);
		}
		var relrow = (WaypointY-CurRow);
		var relcol = (WaypointX-CurCol);
		waypoint.style.top = (relrow*TileHeight*ZoomRatio) + "px";
		waypoint.style.left = (relcol*TileWidth*ZoomRatio) + "px";
	
		//if(relrow < 0 || relrow > MapRows || relcol < 0 || relcol > MapsCols)
		//{
		//	waypoint.style.visibility = "hidden";
		//}
		//else
		//{
		//	waypoint.style.visibility = "visible";
		//}
	}
	
}
