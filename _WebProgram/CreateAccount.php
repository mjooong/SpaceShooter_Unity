<?php
	
	$u_id = $_POST["Input_user"];
	$u_pw = $_POST["Input_pass"];
	$nick = $_POST["Input_nick"];

	if( empty($u_id) )
		die("u_id is empty. \n");
	if( empty($u_pw) )
		die("u_pw is empty. \n");
	if( empty($nick) )
		die("nick is empty. \n");

	$con = mysqli_connect("localhost", "minjong0712", "so12djfrnf!", "minjong0712");
	//"localhost" <-- 같은 서버 내

	if(!$con)
		die("Could not Connet" . mysqli_connect_error());
	//연결 실패 했을 경우 이 스크립트를 닫아주겠다는 뜻

	$check = mysqli_query($con, "SELECT user_id FROM MyDBStudy WHERE user_id = '". $u_id ."' ");
	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{   // 즉 0 이 아니라는 뜻은 내가 생성하려고 하는 ID 값이 존재 한다는 뜻이다.
		// (누군가 이미 사용하고 있다는 뜻)
		die("ID does exist. \n");
	}

	$check = mysqli_query($con, "SELECT nick_name FROM MyDBStudy WHERE nick_name = '". $nick ."' ");
	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{	// 즉 0 이 아니라는 뜻은 내가 생성하려고 하는 NickName 값이 존재 한다는 뜻이다.
		// (누군가 이미 사용하고 있다는 뜻)
		die("Nickname does exist. \n");
	}

	$Result = mysqli_query($con,
	"INSERT INTO MyDBStudy (user_id, user_pw, nick_name) VALUES 
	( '". $u_id ."', '". $u_pw ."', '". $nick ."' );" );

	if($Result)
		echo "Create Success. \n";
	else
		echo "Create error. \n";

	mysqli_close($con);
?>