var TileWidth = 100;
var TileHeight = 100;

var CurRow = 0;
var CurCol = 0;

var MapRows = 18
var MapCols = 63

var mapwidth = 0;
var mapheight = 0;

var ZoomRatio = 1.0;

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
						element.src = "images/"+ pad(tc, 2) + "_" + pad(tr, 2) + ".png";
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
	
}
