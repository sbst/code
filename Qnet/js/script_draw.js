//Text of programm is already here. If you don't see it - you'll just crazy.

function begin_draw(){
	var adrMas = [];
	var countMas = 0;
	var line = [];
	var perem = 0;

	for(var ix in machine){
		adrMas[countMas] = {};
		adrMas[countMas]["addr"] = machine[ix]["addr"];
		adrMas[countMas]["type"] = machine[ix]["type"];
		++countMas;
	}


	var canvas = document.getElementById('myCanvas');
	paper.setup(canvas);
	with(paper){
		var circle = new Path.Circle(new Point(300,300), 200);
		var countLine = 0;	

		for(var ix = 0; ix < countMas; ++ix){
			var offset = ix / countMas * circle.length;

			adrMas[ix]["coorPoint"] = circle.getPointAt(offset);			
		}
		circle.remove();

		for(var ix in machine){
			for(var iy in machine[ix]["comp"]){
				for(var iz in machine[ix]["comp"][iy]["connect"]){
					for(var iq in adrMas){
						if(machine[ix]["comp"][iy]["connect"][iz]["conn_id"] === adrMas[iq]["addr"]){
							for(var ig in adrMas){ 
								if(machine[ix]["addr"] === adrMas[ig]["addr"]){
									line[perem] = {};
									line[perem]["from"] = adrMas[ig]["coorPoint"];
									line[perem]["to"] = adrMas[iq]["coorPoint"];
									line[perem]["line"] = new Path.Line(adrMas[ig]["coorPoint"],adrMas[iq]["coorPoint"]);
									line[perem]["line"].strokeColor = "black";
									line[perem]["line"].name = "interfaceNet";
									for(var im in line){
										if(im == perem) continue;
										if((line[perem]["from"] == line[im]["to"]) && (line[perem]["to"] == line[im]["from"])){
											line[perem]["line"].remove();
											line.splice(perem,1);
											perem = perem - 1;
										}
									}

									++perem;
									break;
								}
							}
						}
					}
				}
			}
		}

		for(var ix = 0; ix < countMas; ++ix){
			if (adrMas[ix]["type"]==="pc")
			{
			var ico = new Raster({
				source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icopc.png',
				position: adrMas[ix]["coorPoint"]
			});
			}
			if (adrMas[ix]["type"]==="rt")
			{
			var ico = new Raster({
				source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icort.ico',
				position: adrMas[ix]["coorPoint"]
			}); 
			}
			ico.name = adrMas[ix]["addr"];
			
		}
		view.draw();


		var DDTool = new Tool();				
	/*	DDTool.onMouseDrag = function(event){
			if(!(event.item.name == "interfaceNet") && event.item){
				event.item.position = event.point;
			}
		}
	*/
		DDTool.onMouseMove = function(event){
			deleteMenu();
                        project.activeLayer.selected = false;
			if(event.item){
                        	event.item.selected = true;
				console.log(event.item.name);
				createMenu(event.item.name);
			}
                }

	}
}

function createMenu(data){
	var menu = document.createElement('div');
	menu.className = "divInfoMenu";
	menu.id = "idMenu01";
	document.body.appendChild(menu);
	menu.innerHTML = data;
}

function deleteMenu(){
	if(document.getElementById("idMenu01")){
		document.getElementById("idMenu01").parentNode.removeChild(document.getElementById("idMenu01"));
	}
}
