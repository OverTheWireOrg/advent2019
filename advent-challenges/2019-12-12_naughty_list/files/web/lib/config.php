<?php

$conf = new stdClass;

$conf->debug  = false;

$conf->cipher = 'aes-128-gcm';
$conf->iv_len = openssl_cipher_iv_length($conf->cipher);
$conf->key    = "\xb0\x37\xaa\x39\x51\xbe\xad\xfe\xb0\x5d\xe5\xcf\xa1\x93\x9b\xa4";

$conf->db = 'chall';
$conf->host = 'mysql';
$conf->port = 3306;
$conf->user = 'challdb';
$conf->pass = 'b6a1fd593b92228f8e993b30d0649afa';
$conf->sleep  = 1000000;

$conf->flag   = 'AOTW{S4n7A_c4nT_hAv3_3lF-cOnTroL_wi7H0uT_eLf-d1sCipl1N3}';
$conf->goal   = 5000;
$conf->max    = 15;
$conf->ips    = '/tmp/ips.json';
