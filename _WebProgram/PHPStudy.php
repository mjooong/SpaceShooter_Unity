<html>
	<head>
		
		<title>PHPStudy</title>

	</head>

	<body>
		<h2>PHP 문법 연습</h2>
		
		<?php
			// 1. 출력하기
			echo "Hello PHP<br/>";
			print "print함수로 출력<br/>";

			// 2. 변수 선언하기
			// PHP에서는 변수를 사용하기 위해서 그 변수가 어떤 타입으로 사용될 것인지
			// 명시해 줄 필요가 없다. 
			// PHP에서는 변수를 사용하기 위해서 변수명 앞에 $ 표시를 붙여줘야한다.
			echo "<br/><br/>";

			$name = "드래곤";
			$data1 = 1 + 2;
			$data2 = $data1 / 4;

			echo $name . "<br/>";
			echo $data1 . "<br/>";
			echo "$data2<br/>";

			$name = 123 * 2;
			echo "$name<br/>";

			// 3. 문자열 연결 연산자 도트( . )
			echo "<br/><br/>";

			$dat1 = "천사의 ";
			$data2 = $data1 . "반지";
			echo $data2;

			$name = "솔로몬 ";
			echo "<br/>나의 이름은 " . $name . "입니다";

			// 쌍따움표와 작은따움표 사용하기
			echo "<br/><br/>";
			$AAA = "아기상어";
			$BBB = '어른상어';
			echo $AAA . " : " . $BBB; 

			$a = 111;			// 정수형 저장
			echo "<br/>$a";		// " 는 출력시 변수 안의 내용이 출력되고
			$a = 321;
			echo '<br/>$a';		// ' 는 출력시 변수 이름 자체가 문자열로 출력된다.

			// 4. 변수의 형(타입) 변환
			echo "<br/><br/>";
			$data1 = 1.1f;
			echo $data1 . "<br/>";
			$data2 = (int)$data1;
			echo $data2;

			// 형 변환 타입
			// (int), (interger) --> 정수형으로 변환
			// (float), (double), (real) --> 실수형으로 변환
			// (string) --> 문자열로 변환
			// (array) --> 배열로 변환
			// (object) --> 객체로 변환 

			// 5. 연산자
			echo "<br/><br/>";
			$data1 = 5 + 7;
			$data2 = 32 / (1 + 3);
			$data3 = $data4 = 3;

			echo $data1 . "<br/>";
			echo $data2 . "<br/>";
			echo $data3 . "<br/>";

			// 6. for문 예제
			echo "<br/><br/>";
			$sum = 0;
			for($i = 0; $i <= 10; $i++)
			{
				$sss = $i;
				$sum += $i;
			}

			echo "for 사용 0부터 10까지 합 : $sum : $sss	";

			// 7. break 문을 사용해서 while 반복문을 탈출하는 예제
			echo "<br/><br/>";
			//$k = 0;
			//$x = 0;
			while(1)
			{
				$k++;
				if(10 < $k)
				break;
				$x = $k;
			}

			echo "변수 x에 저장한 값은 $x 입니다.";

			// 8. 배열
			echo "<br/><br/>";
			$AAA = array();
			$AAA[0] = 34;
			$AAA[1] = 50;
			$AAA[2]	= 123;

			$Arr[] = 42;		// $Arr[0] 에 42 대입
			$Arr[] = 73;		// $Arr[1] 에 73 대입
			$Arr[] 100;			// $Arr[2] 에 100 대입
			$Arr[0] = 37;		// $Arr[0] 에 37 대입
			$Arr[1] = 25;		// $Arr[1] 에 25 대입

			$fruit["a"] = "사과";	// $fruit["a"] 에 "사과" 대입
			$fruit["b"] = "배";		// $fruit["b"] 에 "배" 대입

			// html 문법에서 <br/> : line break 강제 줄바꿈 기호
			// html 문법에서 <p>	: 문단 줄바꿈 기호
			// 한글의 경우에는 문단의 첫 글자를 자동으로 들여쓰기 해 주고
			// 영문의 경우에는 문단과 문단 사이에 한줄의 공백을 만들어 줌
			echo $Arr[0] . "<p>";
			echo $Arr[1] . "<p>";
			echo $Arr[2] . "<p>";
			echo $fruit["a"] . "<p>";
			echo $fruit["b"] . "<p>";

			echo "Arr 배열의 개수 : ". count($Arr);
			// count 함수를 이용해서 $Arr 배열의 개수를 구할 수 있다.

			// 9. 배열의 암시적 선언하기
			echo "<br/><br/>";
			$hobby = array("영화감상", "등산", "기타연주");
			// 3개의 값을 배열에 등록하여 hobby 배열 변수 만들기
			echo $hobby[0]. "<br/>";
			echo $hobby[1]. "<br/>";
			echo $hobby[2]. "<br/>";

			// 10. 배열의 예시
			echo "<br/><br/>";
			$brray = array();
			$score = &$brray;	// $brray 라는 배열 변수의 별칭 $score 변수를 만들어 줌...  & : 참조연산자
			$score[0] = 24;
			$score[1] = 83;
			$score[2] = 92;
			$score[3] = 73;
			$score[4] = 29;
			$score[5] = 76;
			$score[6] = 62;
			$score[7] = 53;
			$score[8] = 98;
			$score[9] = 17;

			$sum = 0;
			for($ii = 0; $ii < 10; $ii++)
			{
				$sum = $sum + $brray[$ii];
			}
			echo "배열 합 테스트 : ". $sum;

			// 11. 배열의 암시적 선언, 지정 인덱스
			echo "<br/><br/>";
			$flower = array("장미", "무궁화", "진달래", 2=>"해바라기", "튤립"	);
			echo $flower[0] . "<p>";
			echo $flower[1] . "<p>";
			echo $flower[2] . "<p>";
			echo $flower[3] . "<p>";
			echo $flower[4] . "<p>";
			// <결과>
			// 장미
			// 무궁화
			// 해바라기
			// 튤립
			
			// 진달래는 우선 flower[2] 에 저장되지만 2=>"해바리기" 명령이 있기 때문에
			// flower[2] 에서는 "해바라기" 가 저장된다. [3] 은 튤립이고 , [4] 는 없다.

			// 12. 함수 : 두 변수를 합산해서 출력하는 함수 예제
			function plus($a, $b)		// a, b 변수 값을 받아서 plus 하는 함수 정의
			{
				$c = $a + $b;			// 전달받은 변수 a, b 를 합산해서 변수 c에 대입
				echo $c . "<p>";
			}

			plus(5, 10);
			plus(4, 34);


			function DivRest($a, $b)
			{
				$div = intval($a / $b);		// a / b 의 값을 정수형으로 변환 : 몫
				$rest = $a % $b;			// a / b 의 나눗셈 한 후 나머지 값을 변수 rest 할당함

				return array($div, $rest);
			}

			list($c, $d) = DivRest(30, 7);
			echo "몫 : ". $c . "<br/>";
			echo "나머지 : ". $d . "<br/>";

			$MyArr = DivRest(10, 7);
			echo "몫 : ". $MyArr[0] . "<br/>";
			echo "나머지 : ". $MyArr[1] . "<br/>";

			// 13. 지역변수를 전역변수로 선언하는 global 키워드 함수 예제
			echo "<br/><br/>";
			$data1 = "전역변수";

			function MyFun2()
			{
				global $data1;		// 여기서의 $data1 변수는 글로벌 변수를 의미한다. 를 명시화
				$data1 = "정말 글로벌 변수가 맞나요?";
				echo "$data1 <br/>";
			}

			MyFun2();		// 정말 글로벌 변수가 맞나요?
			echo $data1;	// 정말 글로벌 변수가 맞나요?

			// 14. PHP에서 MySQL 사용을 위한 함수들..
			// die() : PHP 스크립트의 실행을 즉시 중지하는 함수
			// mysqli_connect() : MySQL 데이터베이스에 접속하는 함수
			// mysqli_connect_error() : MySQL 서버에 접근 오류를 반환하는 함수
			// mysqli_query() : SQL 명령어(쿼리)를 실행하는 함수
			// mysqli_num_rows() : 데이터베이스에서 쿼리를 보내서 나온 레코드의 개수를 알아낼 때 쓰임
			// 즉 0이 나왔다는 뜻은 내가 찾는 ID값이 없다는 의미이다.
			// mysqli_fetch_assoc() : MySQL의 실행 결과에서 결과 행을 호출하는 함수
			// mysqli_close($con) : MySQL 서버의 연결을 종료하는 함수

		?>


	</body>


</html>
