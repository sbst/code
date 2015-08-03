<?php
$object = array();

//exec("ping -w 1 -i 0.2 -c 1 ".$_GET['jsonData'], $out, $status);

$config = ssh2Auth($_POST['jsonData']);
if($config === false) break;

$object = parsData($config,$_POST['jsonData']);

for($ix = 0; $ix < count($object); ++$ix){
	if($object[$ix]["int"] == "lo" || $object[$ix]["addr"] == "127.0.0.1"){
		unset($object[$ix]);
		$object = array_values($object);
	}elseif($object[$ix]["addr"] == "unassigned"){
		unset($object[$ix]);
       		$object = array_values($object);
	}
}

jsonEcho($object);


function ssh2Auth($sendData){
        if(!$connection = ssh2_connect($sendData, "22")){
                return false;
        }
        if(!ssh2_auth_password($connection, "administrator", "Spb78sts!")){
                return false;
        }
        if(!$stream = ssh2_exec($connection, 'ifconfig')){
                return false;
        }else{
                stream_set_blocking($stream, true);
                $stream_out = ssh2_fetch_stream($stream, SSH2_STREAM_STDIO);
                $config = stream_get_contents($stream_out);
        }

        fclose($stream);
        return $config;
}



function parsData($config, $name){
        $config = explode("\n", $config);
        $interfaces = array();

	for ($i=0; $i!=count($config); ++$i) {
                if (strpos($config[$i], "Link encap"))  {
                        list($cur_int, $config[$i]) = explode("Link encap", $config[$i]);
                        $cur_int = trim($cur_int);

                        list($config[$i], $cur_mac) = explode("addr", $config[$i]);
                        $cur_mac = ($cur_mac) ?trim($cur_mac) :"unassigned";

                        list($config[$i+1], $cur_mask) = explode("Mask:", $config[$i+1]);
                        $cur_mask = ($cur_mask) ?trim($cur_mask) :"unassigned";

                        list($config[$i+1], $cur_bcast) = explode("Bcast:", $config[$i+1]);
                        $cur_bcast = ($cur_bcast) ?trim($cur_bcast) :"unassigned";

                        list($config[$i+1], $cur_addr) = explode("inet addr:", $config[$i+1]);
                        $cur_addr = ($cur_addr) ?trim($cur_addr) :"unassigned";

                        while (!strpos($config[++$i], "MTU"));

                        $cur_status = (strpos($config[$i], "UP") && strpos($config[$i], "RUNNING")) ?1
                        : (strpos($config[$i], "RUNNING") ?0 :3);

                        if (!strcmp($cur_mask, "255.0.0.0")) $cur_status = 2;
                                $newint = array(
                                        	"int" => $cur_int,
                                        	"mac" => $cur_mac,
                                        	"mask" => $cur_mask,
                                        	"addr" => $cur_addr,
                                        	"status" => $cur_status);
                                array_push($interfaces, $newint);
                        }
        }
        return $interfaces;
}

function jsonEcho($interfaces){
        echo json_encode($interfaces);
        return false;
}

?>
