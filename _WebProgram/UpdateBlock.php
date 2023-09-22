<?php
	
	$u_id = $_POST["Input_user"];
	$Info_data = $_POST["Info_data"];

	if( empty($u_id) )
		die("u_id is empty. \n");
	//if( empty($Info_data) )
	//	die("Bscore is empty. \n");

	$con = mysqli_connect("localhost", "minjong0712", "so12djfrnf!", "minjong0712");
	//"localhost" <-- 같은 서버 내

	if(!$con)
		die("Could not Connet" . mysqli_connect_error());
	//연결 실패 했을 경우 이 스크립트를 닫아주겠다는 뜻

	$check = mysqli_query($con, "SELECT user_id FROM MyDBStudy WHERE user_id = '". $u_id ."' ");

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{   
		die("ID does not exist. \n");
	}

	if( $row = mysqli_fetch_assoc($check) ) //user 이름에 해당하는 행을 찾아준다.
	{
		mysqli_query($con,
			"UPDATE MyDBStudy SET block_info = '". $Info_data ."' WHERE user_id = '". $u_id ."' ");
		//user_id 를 찾아서 block_info = '$Info_data' 로 변경하라는 뜻 
		echo("UpDateSuccess~");
	}

	mysqli_close($con);
?>