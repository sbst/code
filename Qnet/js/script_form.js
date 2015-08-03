	var machine = {};
	var matrixSm = [];
	var comp_adr = [];

	var gNum; var gCount; var num_of_mach;

	$(document).ready(function(){
		var options = {
			success: function(retData){
				//comp_adr = retData;
				//console.log(JSON.parse(retData));
				menuSend(JSON.parse(retData));
			},
			error: function(){
				console.log("Internal server error 500");
			}
		};
		$("#myForm").ajaxForm(options);
	});
	
	function create_Quagga(){
		var adrMas = {};
		adrMas["adr"] = [];		
		adrMas["type"] = [];

		adrMas["adr"][0] = {};
		adrMas["adr"][0] = "172.16.117.171";
		adrMas["type"][0] = "rt";
		adrMas["adr"][1] = {};
                adrMas["adr"][1] = "172.16.117.172";
                adrMas["type"][1] = "rt";
		adrMas["adr"][2] = {};
                adrMas["adr"][2] = "172.16.117.173";
                adrMas["type"][2] = "rt";
		adrMas["adr"][3] = {};
                adrMas["adr"][3] = "172.16.117.174";
                adrMas["type"][3] = "rt";
		adrMas["adr"][4] = {};
                adrMas["adr"][4] = "172.16.117.175";
                adrMas["type"][4] = "rt";
		adrMas["adr"][5] = {};
                adrMas["adr"][5] = "172.16.117.176";
                adrMas["type"][5] = "rt";
		adrMas["adr"][6] = {};
                adrMas["adr"][6] = "172.16.117.177";
                adrMas["type"][6] = "rt";
		adrMas["adr"][7] = {};
                adrMas["adr"][7] = "172.16.117.177";
                adrMas["type"][7] = "rt";
		adrMas["adr"][8] = {};
                adrMas["adr"][8] = "172.16.117.178";
                adrMas["type"][8] = "rt";
		adrMas["adr"][9] = {};
                adrMas["adr"][9] = "172.16.117.179";
                adrMas["type"][9] = "rt";
		adrMas["adr"][10] = {};
                adrMas["adr"][10] = "172.16.117.170";
                adrMas["type"][10] = "pc";
		adrMas["adr"][11] = {};
                adrMas["adr"][11] = "172.16.117.169";
                adrMas["type"][11] = "pc";
		adrMas["adr"][12] = {};
                adrMas["adr"][12] = "172.16.117.168";
                adrMas["type"][12] = "pc";
		adrMas["adr"][13] = {};
                adrMas["adr"][13] = "172.16.117.167";
                adrMas["type"][13]= "pc";

		menuSend(adrMas);
	}
	
	function menuSend(adrMas){
		gNum = 0; gCount = 0; num_of_mach = 0;
		for(var ix in adrMas["adr"]){
			++gNum;
			++num_of_mach;
			machine[adrMas["adr"][ix]] = {};
                        machine[adrMas["adr"][ix]]["addr"] = adrMas["adr"][ix];
                        machine[adrMas["adr"][ix]]["type"] = adrMas["type"][ix];
		}

		for(var ix in adrMas["adr"]){
			ajaxSend(adrMas["adr"][ix]);
		}
	}

/*
	function ajaxPing(addr){
		$.ajax({
			url:"php/qnet2_ping.php",
			type:"GET",
			data:{ "ping": addr},
			success: function(answer){
				console.log(answer);
				//if(answer === "0"){ ajaxSend(addr); }
			}
		});
		return false;
	}
*/

	function ajaxSend(addr){
			$.ajax({
				url:"php/qnet2_par.php",
				type:"POST",
				data:"jsonData=" + addr,
				success: function(html){
					++gCount;
					//console.log(html);
					var htmlJson = JSON.parse(html);
					workProcess(htmlJson,addr);
					if(gNum === gCount){
                                		addConnect();
                                		console.log(machine);//JSON.stringify(machine));
						//create_div();
						begin_draw2();
                        			//begin_draw();
					}
				},
				error: function(){
					gNum = gNum - 1;
					if(gNum === gCount){
                                                addConnect();
                                                console.log(machine);
						//create_div();
						begin_draw2();
						//begin_draw();
                                        }

				}
			});
			return false;
	}

	function workProcess(retData,addr){
		//machine[addr] = {};
		//machine[addr]["addr"] = addr;
		machine[addr]["comp"] = [];
        	machine[addr]["comp"] = retData;
		get_netAdr_into_obj(machine,addr);
	}

	function get_netAdr_into_obj(machine, adr){
		var ix;
		for(ix in machine[adr]["comp"]){
			machine[adr]["comp"][ix]["net"] = get_netAdr(machine, adr, ix);
		}
	}

	function get_netAdr(machine, adr, ix){
		var netAdr = "";
		var netIP = [];
		if(machine[adr]["comp"][ix]["addr"] && machine[adr]["comp"][ix]["mask"]){	//[ix]
			var adrN = machine[adr]["comp"][ix]["addr"].split(".");
			var maskN = machine[adr]["comp"][ix]["mask"].split(".");
			for(var iy = 0; iy < 4; iy++){
				netIP[iy] = (adrN[iy] & maskN[iy]).toString();
				if(iy < 3){
					netAdr = netAdr + netIP[iy] + '.';
				}else{
					netAdr = netAdr + netIP[iy];
				}
			}
		}else{
			return false;
		}
		return netAdr;
	}

	function addConnect(){
		var elem = [];
		var ig = 0, iv = 0;
		for(var iq in machine){
			for(var ix in machine){
				if(machine[iq] === machine[ix]){ 
					continue; 
				}else{
					for(var iy in machine[iq]["comp"]){
						elem[ig] = {};
						elem[ig]["comp"] = iq;
						elem[ig]["inter"] = machine[iq]["comp"][iy]["addr"];
						elem[ig]["dat"] = [];
						machine[iq]["comp"][iy]["connect"] = [];
						for(var iz in machine[ix]["comp"]){
							if(machine[iq]["comp"][iy]["net"] === machine[ix]["comp"][iz]["net"]){
								if(machine[iq]["comp"][iy]["net"] === machine[iq]["comp"][0]["net"]){	
									continue;	
								}else{
									//machine[iq]["comp"][iy]["connect"][ig] = {};
									//machine[iq]["comp"][iy]["connect"][ig]["conn_int"] = machine[ix]["comp"][iz]["addr"];
									//machine[iq]["comp"][iy]["connect"][ig]["conn_net"] = machine[ix]["comp"][iz]["net"];
									//machine[iq]["comp"][iy]["connect"][ig]["conn_id"] = machine[ix]["addr"];
									elem[ig]["dat"][iv] = {};
									elem[ig]["dat"][iv]["conn_int"] = machine[ix]["comp"][iz]["addr"];
									elem[ig]["dat"][iv]["conn_net"] = machine[ix]["comp"][iz]["net"];
                                					elem[ig]["dat"][iv]["conn_id"] = machine[ix]["addr"];
									//machine[iq]["comp"][iy]["connect"].push(elem[ig]);
									//console.log(machine[iq]["comp"][iy]["connect"]);
									//console.log(elem);
									++iv; //++ie;
								}
							}
						}
						++ig; iv=0;
					}
				}
			}
		}

		var iu = 0;
                for(var ix in elem){
                        for(var iy in elem[ix]["dat"]){
                        	//console.log(elem[ix]);
				//++iu;
				for(var iq in machine){
					for(var iz in machine[iq]["comp"]){
						if(machine[iq]["addr"] == elem[ix]["comp"] && machine[iq]["comp"][iz]["addr"] == elem[ix]["inter"]){
							 machine[iq]["comp"][iz]["connect"].push(elem[ix]["dat"][iy]);
						}
					}
				}
                        }
                }

		//console.log(machine);
		return false;
	}


	function create_div(){
		check_exist("ch_menu");

		var fp = document.createElement('div');
		fp.className = "ch_menu_css";
		fp.id = "ch_menu";
		fp.innerHTML = '<form><input type="button" id="old" value="Old" onclick="begin_draw()"><br> \
				<input type="button" id="new" value="New" onclick="begin_draw2()"></form>';
		document.body.appendChild(fp);
	}

	function check_exist(elemId){
        	if(document.getElementById(elemId)){
                	document.getElementById(elemId).parentNode.removeChild(document.getElementById(elemId));
        	}
	}

