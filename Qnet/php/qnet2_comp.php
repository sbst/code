<?php
$data = array();
$data["adr"] = array();
$data["type"] = array();
if(isset($_FILES["myfile"])) {
        if ($_FILES["myfile"]["error"] > 0) {
        	echo "Error: " . $_FILES["file"]["error"] . "<br>";
        } else {
		//$data = array();
       	 	$f = file($_FILES['myfile']['tmp_name']);
        	for($ix = 0; $ix != count($f); $ix++){
			$arr=explode("|",trim($f[$ix]));
			array_push($data["adr"],trim($arr[0]));
			array_push($data["type"],trim($arr[1]));
                	//array_push($data,trim($f[$ix]));
		}
	}
	echo json_encode($data);
}

?>




