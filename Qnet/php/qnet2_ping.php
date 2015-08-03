<?php
	//set_time_limit(500);
	//$time_start = getmicrotime();
	exec("ping -i 0.2 -c 1 ".$_GET["ping"], $out, $status);
	//$time_end = getmicrotime();
	//$time = $time_end - $time_start;
	//echo $time;
	if($status === 0){
		echo "1";
	}else{ echo "0"; }

	function getmicrotime(){
    		list($usec, $sec) = explode(" ", microtime());
    		return ((float)$usec + (float)$sec);
	}
?>
