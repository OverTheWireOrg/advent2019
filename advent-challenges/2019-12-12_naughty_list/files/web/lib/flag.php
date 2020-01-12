<?php

session_start();
if (!empty($_SESSION['credits'])){
	$amount = (int) $_SESSION['credits'];
	if ($amount >= $conf->goal){
		printf("%s\n", $conf->flag);
		exit(0);
	}
}
session_commit();