<?php

$db = connectdb();

function connectdb(){

	global $conf;

	$db = new mysqli($conf->host, $conf->user, $conf->pass, $conf->db, $conf->port);
	if ($db->connect_errno){
		echo 'Could not connect to the database, please contact an admin.<br />';
		exit(0);
	}

	return $db;
}

function getUser($username){

	global $db;

	$sql = 'SELECT * FROM `users` WHERE `username` = "'.mysqli_real_escape_string($db, $username).'" LIMIT 1';
	if ($result = $db->query($sql)){

		if ($result->num_rows === 1){

			return (object) $result->fetch_assoc();
		}
	}

	return false;
}

function addUser($username, $password){

	global $db;

	$sql = 'INSERT INTO `users` (`username`, `password`) VALUES ("'.mysqli_real_escape_string($db, $username).'", "'.mysqli_real_escape_string($db, md5($password)).'")';
	if ($result = $db->query($sql)){

		return true;
	}

	return false;
}

function validUser($username){

	return ctype_alnum($username);
}


function encrypt($data){

	global $conf;
	if (!is_object($conf))
		return false;

	$iv    = openssl_random_pseudo_bytes($conf->iv_len);
	$tag   = 0;
	$ctext = openssl_encrypt($data, $conf->cipher, $conf->key, 0, $iv, $tag);

	if ($ctext)
		return b64_encode($iv.$ctext.$tag);

	return false;
}

function decrypt($data){

	global $conf;
	if (!is_object($conf))
		return false;

	$data = b64_decode($data);
	if (!$data)
		return false;

	$iv    = substr($data, 0, $conf->iv_len);
	$tag   = substr($data, -16);
	$ctext = substr($data, $conf->iv_len, -16);
	$ptext = openssl_decrypt($ctext, $conf->cipher, $conf->key, 0, $iv, $tag);

	return $ptext;
}

function b64_encode($data){

	return rtrim(strtr(base64_encode($data), '+/', '-_'), '=');
}

function b64_decode($data){

	return base64_decode(str_pad(strtr($data, '-_', '+/'), strlen($data) % 4, '=', STR_PAD_RIGHT));
}

