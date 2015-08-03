var count_machine_with_rt = 0;
var addr_rt = [];
var count_machine_with_pc = 0;
var addr_pc = [];

function create_canvas(){
	if(document.getElementById("myCanvas")){
		document.getElementById("myCanvas").parentNode.removeChild(document.getElementById("myCanvas"));
	}
	var canvas = document.createElement("canvas");
	canvas.className = "coorMyCanvas";
	canvas.id = "myCanvas";
	document.body.appendChild(canvas);
	return canvas;
}

function begin_draw2(){
        count_machine_with_rt = 0; addr_rt = [];
        count_machine_with_pc = 0; addr_pc = [];
	countOfMachine();

	var canvas = create_canvas();
	paper.setup(canvas);
	with(paper){
		draw_erorr_comp();		
		var circle_rt = new Path.Circle(new Point(420,450),200);
		
		var points_rt = draw_rt(circle_rt,count_machine_with_rt);
		var points_pc = draw_pc(450,420,points_rt);
                var points_cloud = draw_cloud(450,points_rt);
		var lines = draw_lines(points_rt,points_pc,points_cloud);
		
		circle_rt.remove();		
		
		var interfaces = draw_interfaces(lines, points_rt);
		
		console.log(interfaces);
		view.draw();
		
		var DDTool = new Tool();
		
		DDTool.onMouseMove = function(event){
                        deleteMenu3();
                        project.activeLayer.selected = false;
                        if(event.item && event.item.data){
                                event.item.selected = true;
                                createMenu3(event.item.name, event.item.data, event.point.x, event.point.y); //event.item.position.x, event.item.position.y);
                        }
                }
		
		DDTool.onMouseDown = function(event){
                        deleteMenu2();
                        project.activeLayer.selected = false;
			 if(event.item){
                                event.item.selected = true;
                                createMenu2(event.item.name, event.item.data);
					
                        }
                }
		DDTool.onMouseDrag = function(event){		
			if(!(event.item==null))
			if(!(event.item.data.type === "line")){ 
				//maxDistance = 1;
				deleteMenu3();
				event.item.position = event.point;
				for(var ix in lines){
					if(event.item.name == lines[ix].data.to)
					lines[ix].lastSegment.point = event.point;
					if(event.item.name == lines[ix].data.from)
					lines[ix].firstSegment.point = event.point;
					}
				for(var i in interfaces)
				interfaces[i].remove();
				interfaces = draw_interfaces(lines, points_rt);
			}
		}
	}
}

function createMenu3(name,data,positionX,positionY){
	with(paper){
		var menu = document.createElement('div');
		menu.className = "divInfoMenu3";
		menu.id = "idMenu03";
		var menu2 = document.createElement('div');
		menu2.className = "divRasporka";
		menu2.id = "idDivRasporka";
		document.body.appendChild(menu2);
		if(data.type === "line"){
			menu.innerHTML = "Interface net address: " + name + "<br>"+"Connected:<br>From: " + data.from + "<br>To: " + data.to;
			menu.style.left=positionX-140;
			menu.style.top=positionY+125;
			document.body.appendChild(menu);
			if (menu2.offsetTop-menu.offsetTop-menu.offsetHeight<0){
				menu.style.top=parseInt(menu.style.top)-140;
				document.body.appendChild(menu);
				}
			return;
		}
		if(data.type === "cloud"){
			menu.innerHTML = "Unknown network: " + name + "<br>Tag: " + data.tag;
			//menu.innerHTML = "Unknown net: " + name;
			menu.style.left=positionX-140;
                        menu.style.top=positionY+125;
                        document.body.appendChild(menu);
			if(menu2.offsetTop-menu.offsetTop-menu.offsetHeight<0){
				menu.style.top=parseInt(menu.style.top)-75;
				document.body.appendChild(menu);
				}
                        return;
		}
		//menu.innerHTML = "Comp address: " + name + "<br>";
		menu.innerHTML = "Comp address: " + name + " Tag: " + data.tag + "<br>";
		for(var iy in data.connect)
			menu.innerHTML = menu.innerHTML + "---------------------<br>" + "Comp connect with computers: "+data.connect[iy]["conn_id"]+"<br>Comp connect using by interface: "+data.connect[iy]["conn_int"]+"<br>Comp include in interface's net: "+data.connect[iy]["conn_net"] + "<br>";
		menu.style.left=positionX-140;
		menu.style.top=positionY+125;
		document.body.appendChild(menu);
		if(data.type === "router"){
			if(menu2.offsetTop-menu.offsetTop-menu.offsetHeight<0){
				menu.style.top=parseInt(menu.style.top)-495;//375
				document.body.appendChild(menu);
				}
		}
		if(data.type === "PC"){
			if(menu2.offsetTop-menu.offsetTop-menu.offsetHeight<0){
				menu.style.top=parseInt(menu.style.top)-210;
				document.body.appendChild(menu);
				}
		}
		if((data.type === "redrouter")||(data.type === "redPC")){
			if(menu2.offsetTop-menu.offsetTop-menu.offsetHeight<0){
				menu.style.top=parseInt(menu.style.top)-75;
				document.body.appendChild(menu);
				}
		}
	}
}

function deleteMenu3(){
	if(document.getElementById("idMenu03"))
		document.getElementById("idMenu03").parentNode.removeChild(document.getElementById("idMenu03"));
}

function createMenu2(name, data){
   with(paper){
	var menu = document.createElement('div');
	menu.className = "divInfoMenu2";
	menu.id = "idMenu02";
	if(data.type === "line"){
		menu.innerHTML = "Interface net address: " + name + "<br>"+"Connected:<br>From: " + data.from + "<br>To: " + data.to;
		document.body.appendChild(menu);
		return;
	}
	if(data.type === "cloud"){
        	menu.innerHTML = "Unknown network: " + name + "<br>Tag: " + data.tag;
        	document.body.appendChild(menu);
        	return;
        }
	menu.innerHTML = "Comp address: " + name + " Tag: " + data.tag + "<br>"; 
	for(var iy in data.connect)
		menu.innerHTML = menu.innerHTML + "---------------------<br>" + "Comp connect with computers: "+data.connect[iy]["conn_id"]+"<br>Comp connect using by interface: "+data.connect[iy]["conn_int"]+"<br>Comp include in interface's net: "+data.connect[iy]["conn_net"] + "<br>";
	document.body.appendChild(menu);
   }
}

function deleteMenu2(){
	if(document.getElementById("idMenu02")){
		document.getElementById("idMenu02").parentNode.removeChild(document.getElementById("idMenu02"));
	}
}


function draw_rt(circle, count_machine){
	var points = []; var count_rt = 0;
	with(paper){
		for(var iq = 0; iq < count_machine; ++iq){
			var offset = iq / count_machine * circle.length;

			var point = circle.getPointAt(offset);
		
			points[iq] = new Raster({
                                source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icort.ico',
				position: point
                        });

			points[iq].name = addr_rt[iq];
			points[iq].data.type = "router";
			points[iq].data.tag = "Rt"+iq;
			points[iq].data.connect = [];
			for(var ix in machine){
				if(points[iq].name === machine[ix]["addr"]){
					for(var iy in machine[ix]["comp"]){
						for(var iz in machine[ix]["comp"][iy]["connect"]){
							points[iq].data.connect[count_rt] = {};
							points[iq].data.connect[count_rt] = machine[ix]["comp"][iy]["connect"][iz];
							count_rt++;
						}
					}
				}
			}
		}
	}
	return points;
}

function draw_cloud(coor_y, rt_p){
	var koof = (coor_y + 100) / coor_y;
	var points = []; var countCl = 0;
	with(paper){
		for(var iq in rt_p){
			for(var ix in machine){
                        	if(rt_p[iq].name === machine[ix]["addr"]){
					for(var iy in machine[ix]["comp"]){
						if(machine[ix]["comp"][iy]["net"] === machine[ix]["comp"][0]["net"]) continue;
						if(machine[ix]["comp"][iy]["connect"].length <= 0){
							if(rt_p[iq].position.y > coor_y){
							 points[countCl] = new Raster({
                                                         	source: 'http://sotsbi.spb.ru:8161/qnet2/lib/cloud.png',
                                                         	position: new Point(koof * rt_p[iq].position.x, koof * rt_p[iq].position.y)
                                                         });
                                                         points[countCl].name = machine[ix]["comp"][iy]["net"];
                                                         points[countCl].data.type = "cloud";
                                                   	 points[countCl].data.tag = "Int"+countCl;
							 ++countCl;
							}
							if(rt_p[iq].position.y < coor_y){
                                                         points[countCl] = new Raster({
                                                                source: 'http://sotsbi.spb.ru:8161/qnet2/lib/cloud.png',
                                                                position: new Point(koof * rt_p[iq].position.x, koof * rt_p[iq].position.y - 60)
                                                         });
                                                         points[countCl].name = machine[ix]["comp"][iy]["net"];
                                                         points[countCl].data.type = "cloud";
							 points[countCl].data.tag = "Int"+countCl;
                                                         ++countCl;
                                                        }
						}
					}
				}
			}	
		}
	}
	return points;
}

function draw_pc(coor_y,coor_x,rt_p){
	var koof = (coor_y + 50) / coor_y; var spec = 0;
	var points = []; var countP = 0;
	with(paper){
		for(var iz in addr_pc){
			for(var ix in rt_p){
				for(var iy in rt_p[ix].data.connect){
					if(addr_pc[iz] === rt_p[ix].data.connect[iy]["conn_id"]){
						if(rt_p[ix].position.y >= coor_y){
							for(var ir in points){
                                                                if((points[ir].position.x === koof * rt_p[ix].position.x + spec) && (points[ir].position.y === koof * rt_p[ix].position.y + spec)) spec += 20;
                                                        }
							if(rt_p[ix].position.x >= coor_x){
								points[countP] = new Raster({
                                                        		source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icopc.png',
                                                                	position: new Point(koof * rt_p[ix].position.x + spec, koof * rt_p[ix].position.y + spec)
                                                        	});
							}
							if(rt_p[ix].position.x < coor_x){
								points[countP] = new Raster({
                                                                        source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icopc.png',
                                                                        position: new Point((rt_p[ix].position.x * koof - rt_p[ix].position.x) + 50 + spec,rt_p[ix].position.y + spec)
                                                                });
							}
                                                        points[countP].name = addr_pc[iz];
                                                       	points[countP].data.type = "PC";
							points[countP].data.tag = "Comp"+countP;
                                                        points[countP].data.connect = [];
                                                        for(var iw1 in machine){
                                                          if(machine[iw1]["addr"] === points[countP].name){
                                                            for(var iw2 in machine[iw1]["comp"]){
                                                              points[countP].data.connect = machine[iw1]["comp"][iw2]["connect"];
                                                            }
                                                          }
                                                        }
							spec = 0;
                                                        ++countP;
						}
						if(rt_p[ix].position.y < coor_y){
							for(var ir in points){
                                                                if((points[ir].position.x === koof * rt_p[ix].position.x) && (points[ir].position.y === koof * rt_p[ix].position.y)) spec += 20;
                                                        }
                                                        points[countP] = new Raster({
                                                                source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icopc.png',
                                                                position: new Point(koof * rt_p[ix].position.x + spec, koof * rt_p[ix].position.y + spec - 100)
                                                        });
                                                        points[countP].name = addr_pc[iz];
							points[countP].data.type = "PC";
							points[countP].data.tag = "Comp"+countP;
                                                        points[countP].data.connect = [];
                                                        for(var iw1 in machine){
                                                          if(machine[iw1]["addr"] === points[countP].name){
                                                            for(var iw2 in machine[iw1]["comp"]){
                                                              points[countP].data.connect = machine[iw1]["comp"][iw2]["connect"];
                                                            }
                                                          }
                                                        }
                                                        ++countP;
                                                }
					}
				}
			}
		}
	}
	return points;
}

function draw_lines(rt_p,pc_p,cloud_p){
	var lines = []; var countL = 0;
	with(paper){
		for(var ix in rt_p){
			for(var iy in rt_p[ix].data.connect){
				for(var iz in rt_p){
					if(rt_p[ix].data.connect[iy]["conn_id"] === rt_p[iz].name){
						lines[countL] = new Path.Line(rt_p[ix].position,rt_p[iz].position);
						lines[countL].strokeColor = "black";
						lines[countL].name = rt_p[ix].data.connect[iy]["conn_net"];
						lines[countL].data.type = "line";
						lines[countL].data.from = rt_p[ix].name;
						lines[countL].data.to = rt_p[iz].name;
						for(var iq in lines){
							if(iq == countL) continue;
							if((lines[countL].data.from == lines[iq].data.to) && (lines[countL].data.to == lines[iq].data.from)){
								lines[countL].remove();
								lines.splice(countL,1);
								countL = countL - 1;
							}
										
						}
						++countL;
					}
				}
				for(var iz in pc_p){
                                        if(rt_p[ix].data.connect[iy]["conn_id"] === pc_p[iz].name){
                                                lines[countL] = new Path.Line(rt_p[ix].position,pc_p[iz].position);
                                                lines[countL].strokeColor = "black";
                                                lines[countL].name = rt_p[ix].data.connect[iy]["conn_net"];
                                                lines[countL].data.type = "line";
                                                lines[countL].data.from = rt_p[ix].name;
                                                lines[countL].data.to = pc_p[iz].name;
                                                for(var iq in lines){
                                                        if(iq == countL) continue;
                                                        if((lines[countL].data.from == lines[iq].data.to) && (lines[countL].data.to == lines[iq].data.from)){
                                                                lines[countL].remove();
                                                                lines.splice(countL,1);
                                                                countL = countL - 1;
                                                        }
                                                }
                                                ++countL;
                                        }
                                }
				for(var iz in cloud_p){
					for(var ik in machine){
						if(rt_p[ix].name === machine[ik]["addr"]){
							for(var ij in machine[ik]["comp"]){
                                        			if((machine[ik]["comp"][ij]["connect"].length <= 0) && (machine[ik]["comp"][ij]["net"] === cloud_p[iz].name)){
                                                			lines[countL] = new Path.Line(rt_p[ix].position,cloud_p[iz].position);
                                                			lines[countL].strokeColor = "black";
                                                			lines[countL].name = rt_p[ix].data.connect[iy]["conn_net"];
                                                			lines[countL].data.type = "line";
                                                			lines[countL].data.from = rt_p[ix].name;
                                                			lines[countL].data.to = cloud_p[iz].name;
                                                			for(var iq in lines){
                                                        			if(iq == countL) continue;
                                                        			if((lines[countL].data.from == lines[iq].data.to) && (lines[countL].data.to == lines[iq].data.from)){
                                                                			lines[countL].remove();
                                                                			lines.splice(countL,1);
                                                                			countL = countL - 1;
                                                        			}
                                                			}
                                                			++countL;
								}
							}
						}
                                        }
                                }
			}
		}
	}
	return lines;
}





/*
function draw_inter(rt_p, pc_p){
	var inter = []; var countI = 0;
	
	with(paper){
	var countRt = 0;
	var countPc = 0;
	while(countRt != rt_p.length && countPc != pc_p.length){
		for(var ix in rt_p){
			for(var iy in rt_p[ix].data.connect){
				if((ix === countRt) === false){
					for(var iz in rt_p[countRt].data.connect){
						if(rt_p[countRt].data.connect[iz]["conn_net"] === rt_p[ix].data.connect[iy]["conn_net"]){
							inter[countI] = solve_coor_inter(rt_p[countRt],rt_p[ix]);
							inter[countI].name = rt_p[countRt].data.connect[iz]["conn_int"];
							++countI;
						}
					}
					iz = 0;
				}
				for(var iz in pc_p[countPc].data.connect){
					if(pc_p[countPc].data.connect[ix]["conn_net"] === rt_p[ix].data.connect[iy]["conn_net"]){
						inter[countI] = solve_coor_inter(pc_p[countPc],rt_p[ix]);
						inter[countI].name = pc_p[countPc].data.connect[iz]["conn_int"];
						++countI;
					}
					iz = 0;
				}
			}
		}
		++countRt; ++countPc;
	}
	countRt = 0; countPc = 0;
	while(countPc != pc_p.length && countRt != rt_p.length){	
		for(var ix in rt_p){
                        for(var iy in rt_p[ix].data.connect){
                                for(var iz in pc_p[countPc].data.connect){
                                        if(pc_p[countPc].data.connect[ix]["conn_net"] === rt_p[ix].data.connect[iy]["conn_net"]){
                                                inter[countI] = solve_coor_inter(pc_p[countPc],rt_p[ix]);
                                                inter[countI].name = pc_p[countPc].data.connect[iz]["conn_int"];
                                                ++countI;
                                        }
                                        iz = 0;
                                }
                        }
		}
		++countRt; ++countPc;
	}
	return inter;
}
*/

function solve_coor_inter(comp1, comp2){
	with(paper){
		var obj_comp1 = JSON.stringify(comp1.position);
		var obj_comp2 = JSON.stringify(comp2.position);
		obj_comp1 = obj_comp1.replace('[', '');		
		obj_comp1 = obj_comp1.replace(']', '');
		obj_comp1 = obj_comp1.replace('"Point",', '');
		obj_comp2 = obj_comp2.replace('[', '');
                obj_comp2 = obj_comp2.replace(']', '');
                obj_comp2 = obj_comp2.replace('"Point",', '');
		obj_comp1 = obj_comp1.split(",");		
		obj_comp2 = obj_comp2.split(",");

		var point = new Point(0,0);
		point.x = obj_comp1[0];
		point.y = obj_comp2[1];
		//console.log(point.position);


		var lengthA = obj_comp2[1] - obj_comp1[1];
		if(lengthA<0) lengthA = lengthA * (-1);
		var lengthB = obj_comp1[0] - obj_comp2[0];
		if(lengthB<0) lengthB = lengthB * (-1);
		var lengthC = Math.sqrt(lengthA*lengthA + lengthB*lengthB);
		var Alph = Math.acos((lengthA*lengthA + lengthB*lengthB - lengthC*lengthC)/(2*lengthA*lengthB));

		var new_x = obj_comp1[0] + comp1.width/2 * Math.cos(Alph);
		var new_y = obj_comp1[1] + comp1.width/2 * Math.sin(Alph);
		
		var new_point = new Point(0,0);
		new_point.x = new_x;
                new_point.y = new_y;
		//console.log(point.position);

		var circle = new Path.Circle({
			point: new_point,
			radius: 5,
			fillColor: "blue"
		})
		
		//console.log(circle);
	}
	return circle;
}

function draw_erorr_comp(){
	with(paper){
		var count = 0;
		var cor_x = 875;
		for(var ix in machine){
			for(var iy in machine[ix]["comp"]){
				for(var iz in machine[ix]["comp"][iy]["connect"]){
					++count;
				}
			}
			if(count <= 0){
				if (machine[ix]["type"] === "pc"){
                        		var ico = new Raster({
                                		source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icopcred.ico',
                                		position: new Point(cor_x,870),
						color: "red"
                        		});
					for(var it in addr_rt){
	                                        if(addr_rt[it] === machine[ix]["addr"]){
        	                                        addr_rt.splice(it,1);
                	                        }
                	                }
					cor_x = cor_x-50;
					ico.name = machine[ix]["addr"];
					ico.data.type = "redPC";
                                	addr_rt.sort();
                        	}
                        	if (machine[ix]["type"] === "rt"){
                        		var ico = new Raster({
                                		source: 'http://sotsbi.spb.ru:8161/qnet2/lib/icortred.ico',
                                		position: new Point(cor_x,870),
						color: "red"
                        		});
					for(var it in addr_rt){
                                       		if(addr_rt[it] === machine[ix]["addr"]){
                                               		addr_rt.splice(it,1);
                                       		}
                               		}
					cor_x = cor_x-50;
					count_machine_with_rt = count_machine_with_rt - 1;
					ico.name = machine[ix]["addr"];
					ico.data.type = "redrouter";
                                	addr_rt.sort();
                        	}
			}
			count = 0;
		}
	}
}

function countOfMachine(){
	for(var ix in machine){
		if(machine[ix]["type"] == "rt"){
			++count_machine_with_rt;
			addr_rt[count_machine_with_rt] = machine[ix]["addr"];
		} else if(machine[ix]["type"] == "pc"){
                        ++count_machine_with_pc;
                        addr_pc[count_machine_with_pc] = machine[ix]["addr"];
                }
	}
} 

function draw_interfaces(lines, points_rt){
	with(paper){
		var copyLines = [];
		var rect = [];
		var interface = [];

		for (var i in lines)
			copyLines[i] = lines[i].clone();
		var linesGroup = new CompoundPath(copyLines);//linii
		for (i in points_rt)
			rect[i] = new Path.Rectangle({position: points_rt[i].position, size: points_rt[i].size}); 
		var rectGroup = new CompoundPath(rect);//kvadrati - ramki dlya rasterov
		
		var intersections = linesGroup.getIntersections(rectGroup);
		for(i=0;i<intersections.length;i++)
			 interface[i] = new Path.Circle({ center: intersections[i].point, radius: 4, fillColor: 'red'});
		
		linesGroup.remove();
		rectGroup.remove();

	}
	return interface;

}
