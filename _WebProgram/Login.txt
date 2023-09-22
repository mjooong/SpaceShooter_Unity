<?php
	
	$u_id = $_POST["Input_user"];
	$u_pw = $_POST["Input_pass"];

	//echo $u_id."<br>";
	//echo $u_pw."<br>";

	$con = mysqli_connect("localhost", "minjong0712", "so12djfrnf!", "minjong0712");
	//"localhost" <-- 같은 서버 내

	if(!$con)
		die("Could not Connet" . mysqli_connect_error());
	//연결 실패 했을 경우 이 스크립트를 닫아주겠다는 뜻

	$check = mysqli_query($con, "SELECT * FROM MyDBStudy WHERE user_id = '". $u_id ."' ");
	
	$numrows = mysqli_num_rows($check);

	if($numrows == 0)
	{	//mysqli_num_rows() 함수는 데이터베이스에서 쿼리를 보내서 나온 레코드의 
		//개수를 알아낼  때 쓰임, 즉 0 이라는 뜻은 해당 조건을 못 찾았다는 뜻

		die("ID does not exist. \n");
	}

	$row = mysqli_fetch_assoc($check); //user_id 에 해당되는 행의 내용을 가져온다.
	if($row)
	{
		if($u_pw == $row["user_pw"])
		{
			//echo $row["nick_name"] . " : " . $row["myinfo"];
			// PHP에서의 JSON 생성 코드
			$RowDatas = array();
			$RowDatas["nick_name"]	= $row["nick_name"];
			$RowDatas["best_score"]	= $row["best_score"];
			$RowDatas["game_gold"]	= $row["game_gold"];
			$RowDatas["block_info"]	= $row["block_info"];
			$RowDatas["Item_list"]	= $row["info"];
			$output = json_encode($RowDatas, JSON_UNESCAPED_UNICODE);
			//PHP 5.4 이상 JSON 형식 생성

			echo $output;
			echo "\n";
			echo "Login-Success!!";
		}
		else 
		{
			die("PassWord does not Match. \n");
		}
	}

	mysqli_close($con);

?>