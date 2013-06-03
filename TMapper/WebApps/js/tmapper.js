var TileWidth = 100;
var TileHeight = 100;

var CurRow = 0;
var CurCol = 0;

var MapRows = 18
var MapCols = 64

var mapwidth = 0;
var mapheight = 0;

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
		if(CurCol + 1 < MapCols - (mapwidth/TileWidth))
		{
			CurCol++;
			drawMap();
		}
	}
	else if(event.keyCode == 40)
	{
		//down arrow
		if(CurRow + 1 < MapRows - (mapheight/TileHeight))
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

function onLoadMapper()
{
	//Getting the container
	var mapdiv = document.getElementById("mapview");
	
var viewportWidth  = document.documentElement.clientWidth
  , viewportHeight = document.documentElement.clientHeight;
	
	mapheight = viewportHeight - 200;
	mapwidth = viewportWidth - 350;
	
	mapdiv.style.width = mapwidth;
	mapdiv.style.height = mapheight;
	
	document.getElementById('leftcursor').style.minHeight = mapheight;
	document.getElementById('rightcursor').style.minHeight = mapheight;
	
	var rows = mapheight / TileHeight;
	var cols = mapwidth / TileWidth;
	
	for(var ir = 0; ir < rows; ir++)
	{
		var tr = ir + CurRow;
		for(var ic = 0; ic < cols; ic++)
		{
			var tc = ic + CurCol;
			
			var element = document.createElement('img');
			element.src = "images/"+ pad(tc, 2) + "_" + pad(tr, 2) + ".png";
			element.setAttribute('class','imgmap');
			element.style.position = "relative";
			element.top = ir*TileHeight + "px";
			element.left = ic*TileWidth + "px";
			
			mapdiv.appendChild(element);
		}
		
		var ebr = document.createElement('br');
		
		mapdiv.appendChild(ebr);
	}
	
}
