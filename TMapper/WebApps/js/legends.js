//Legend Javascript

var LegendVisible = false;

function toggleLegend()
{
	var dlegend = document.getElementById('legend');
	var btoggle = document.getElementById('legendtoggle');

	if(LegendVisible)
	{
		dlegend.style.visibility = "hidden";
		btoggle.innerHTML = 'Show Legend';
		LegendVisible = false;
	}
	else
	{
		dlegend.style.visibility = "visible";
		btoggle.innerHTML = 'Hide Legend';
		LegendVisible = true;
	}
}

function getEmptyUL(id)
{
	var ul = document.createElement('ul');
	ul.setAttribute('id', id);
	
	return ul;
}

function toggleGems()
{
	var dlegend = document.getElementById('ullegend');
	var gems = document.getElementById('lgems');
	var tgems = document.getElementById('tgems');
	
	if(gems && gems.childNodes.length != 0)
	{
		//hidding

		dlegend.replaceChild(getEmptyUL('lgems'),gems);
	}
	else
	{
		//showing
		var gems2 = GenerateGems();
		dlegend.replaceChild(gems2, gems);
	}
}

function toggleObjs()
{
	var dlegend = document.getElementById('ullegend');
	var objs = document.getElementById('lobjs');
	var tobjs = document.getElementById('tobjs');
	
	if(objs && objs.childNodes.length != 0)
	{
		//hidding

		dlegend.replaceChild(getEmptyUL('lobjs'),objs);
	}
	else
	{
		//showing
		var objs2 = GenerateObjs();
		dlegend.replaceChild(objs2, objs);
	}
}

function toggleOres()
{
	var dlegend = document.getElementById('ullegend');
	var ores = document.getElementById('lores');
	var tores = document.getElementById('tores');
	
	if(ores && ores.childNodes.length != 0)
	{
		//hidding

		dlegend.replaceChild(getEmptyUL('lores'),ores);
	}
	else
	{
		//showing
		var ores2 = GenerateOres();
		dlegend.replaceChild(ores2, ores);
	}
}

function GenerateGems()
{
	var ul = document.createElement('ul');
	ul.setAttribute('id','lobjs');
	
	ul.innerHTML = '<li>Amethyst <img src="img/gem_a.png" class="legendicon" /></li>'
						+ '<li>Topaz <img src="img/gem_t.png" class="legendicon" /></li>'
						+ '<li>Sapphire <img src="img/gem_s.png" class="legendicon" /></li>'
						+ '<li>Ruby <img src="img/gem_r.png" class="legendicon" /></li>'
						+ '<li>Diamond <img src="img/gem_d.png" class="legendicon" /></li>';
						
	return ul;
}

function GenerateObjs()
{
	var ul = document.createElement('ul');
	ul.setAttribute('id','lobjs');
	
	ul.innerHTML = '<li>Altar <img src="img/objects_altar.png" class="legendicon" /></li>'
						+ '<li>Heart <img src="img/objects_heart.png" class="legendicon" /></li>'
						+ '<li>Hellforge <img src="img/objects_hellforge.png" class="legendicon" /></li>'
						+ '<li>Shadow Orb <img src="img/objects_shadow.png" class="legendicon" /></li>'
						+ '<li>Statue <img src="img/objects_statue.png" class="legendicon" /></li>'
						+ '<li>Torch <img src="img/objects_torch.png" class="legendicon" /></li>';
					
	return ul;
}

function GenerateOres()
{
	var ul = document.createElement('ul');
	ul.setAttribute('id','lores');
	
	ul.innerHTML = '<li>Copper <img src="img/ore_c.png" class="legendicon" /></li>'
						+ '<li>Iron <img src="img/ore_i.png" class="legendicon" /></li>'
						+ '<li>Silver <img src="img/ore_s.png" class="legendicon" /></li>'
						+ '<li>Gold <img src="img/ore_g.png" class="legendicon" /></li>'
						+ '<li>Demonite <img src="img/ore_d.png" class="legendicon" /></li>'
						+ '<li>Meteorite <img src="img/ore_m.png" class="legendicon" /></li>'
						+ '<li>Hellstone <img src="img/ore_h.png" class="legendicon" /></li>'
						+ '<li>Cobalt <img src="img/ore_cob.png" class="legendicon" /></li>'
						+ '<li>Mythril <img src="img/ore_myth.png" class="legendicon" /></li>'
						+ '<li>Adamantite <img src="img/ore_a.png" class="legendicon" /></li>';
						
	return ul;
}