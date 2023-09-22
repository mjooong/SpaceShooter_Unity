<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
</head>

<?php
	$u_id = $_POST["Input_user"];
	$u_pw = $_POST["Input_pass"];
	
	//echo "$u_id<br/>";
	//echo "$u_pw<br/>";

	$con = mysqli_connect("localhost", "minjong0712", "so12djfrnf!", "minjong0712");
	// "localhost" <-- 같은 서버 내

	if(!$con)
		die( "Could not connect" . mysqli_connect_error() );

	$check = mysqli_query($con, "SELECT * FROM MyDBStudy WHERE user_id = '". $u_id ."'" );	

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{    //mysqli_num_rows() 함수는 데이터베이스에서 쿼리를 보내서 나온 행의 개수를 알아낼 때 쓰임
		//즉 0이라는 뜻은 해당 조건을 못 찾았다는 뜻 
		die("ID does not exist.");
	}

	$row = mysqli_fetch_assoc($check);	//user_id 이름에 해당하는 행의 내용을 가져온다.
	if($row)
	{
		if($u_pw == $row["user_pw"])
		{
			echo $row["nick_name"];
			echo "<br/>";
			echo "$u_id<br/>";
		}
		else
		{
			die("Pass does not Match. \n");
		}
	}

	mysqli_close($con);
?>