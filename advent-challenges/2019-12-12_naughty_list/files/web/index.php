<?php

/**
 * Really shitty code, it turns out there are several things in PHP
 * that can really hamper the ability to have multiple connections
 * from the same session.
 */

require_once(__DIR__.'/lib/BladeOne.php');
require_once(__DIR__.'/lib/config.php');
require_once(__DIR__.'/lib/common.php');

ini_set('display_errors', ($conf->debug)? 1: 0);

require_once(__DIR__.'/lib/flag.php');

$views = __DIR__.'/views';
$cache = __DIR__.'/cache';
$mode  = ($conf->debug)? \eftec\bladeone\BladeOne::MODE_DEBUG: \eftec\bladeone\BladeOne::MODE_AUTO;
$blade = new \eftec\bladeone\BladeOne($views, $cache, $mode);

$data = (object) [];
$page = (!empty($_REQUEST['page']))? (($_REQUEST['page'] == '404')? '404': decrypt($_REQUEST['page'])): 'home';
$user = false;
switch (strtolower($page)){

	case 'home':

		$show = 'home';
		break;

	case 'contact':

		if (!empty($_POST))
			$data->message = 'Thank you for filling our spam folder!';

		$show = 'contact';
		break;

	case 'login':

		session_start();
		if (!empty($_SESSION['logged_in'])){
			header('Location: /?page='.encrypt('account'));
			exit;
		}

		if (count($_POST) > 0){

			$name = $_POST['username'];
			$pass = $_POST['password'];
			$user = getUser($name);
			$isin = false;

			if (is_object($user) && !empty($user->username)){

				if ($user->username == strtolower($name) && $user->password === md5($pass)){
					$isin = true;
				}
			}

			if ($isin){

				$_SESSION['logged_in'] = true;
				$_SESSION['username'] = $user->username;
				$_SESSION['credits'] = $user->credits;

				session_write_close();

				header('Location: /?page='.encrypt('account'));
				exit;

			} else {

				$data->error = 'Access denied.';
			}
		}

		$show = 'login';
		break;

	case 'logout':

		session_start();
		unset($_SESSION['loggin_in']);
		unset($_SESSION['username']);
	    session_unset();
	    session_destroy();
	    session_write_close();
	    setcookie(session_name(), '', 0, '/');
	    session_regenerate_id(true);
	    $_SESSION = [];

		header('Location: /?page='.encrypt('login'));
		exit;
		break;

	case 'register':

		if (empty($_POST) && empty($_GET['redirect']) && !empty($_SERVER['HTTP_REFERER'])){
			header('Location: /?page='.encrypt('register').'&redirect='.encrypt($_SERVER['HTTP_REFERER']));
			exit();
		}

		$ip  = $_SERVER['REMOTE_ADDR'];
		$ips = [];
		if (file_exists($conf->ips))
			$ips = (array) json_decode(file_get_contents($conf->ips));

		$data->redirect = (!empty($_GET['redirect']))? decrypt($_GET['redirect']): '';
		if (strstr($data->redirect, "\n") || strstr(urldecode($data->redirect), "\n"))
			$data->redirect = '';

		if (count($_POST) > 0){

			if (!empty($ips[$ip]) && ($ips[$ip] > $conf->max)){

				$data->error = sprintf('Sorry little elf, only %d accounts per hour', $conf->max);

			} else {

				if (empty($_POST['username'])){

					$data->error = 'Please provide a username';

				} else {

					if (!validUser($_POST['username'])){

						$data->error = 'User name must be alphanumeric only.';

					} else {

						if (empty($_POST['password']) || empty($_POST['confirm']) || $_POST['password'] != $_POST['confirm']){

							$data->error = 'You did not confirm the password correctly';

						} else {

							$test = getUser($_POST['username']);
							if (is_object($test) && !empty($test->username)){

								$data->error = 'That username is already taken.';

							} else {

								if (addUser($_POST['username'], $_POST['password'])){

									if (empty($ips[$ip])){
										$ips[$ip] = 1;
									} else {
										$ips[$ip] += 1;
									}

									file_put_contents($conf->ips, json_encode($ips), LOCK_EX);

									if (!empty($data->redirect)){
										header('Location: '.$data->redirect);
										exit();
									} else {
										$data->message = 'You registered successfully.';
									}

								} else {

									$data->error = 'There was an error registering, please contact an admin.';
								}
							}
						}
					}
				}
			}
		}

		$show = 'register';
		break;

	case 'account':

		session_start();
		if (empty($_SESSION['logged_in'])){
			header('Location: /?page='.encrypt('login'));
			exit;
		}
		session_write_close();

		if (count($_POST) > 0){

			if (empty($_POST['destination']) || empty($_POST['credits'])){

				$data->error = 'Please fill out all fields in the form';

			} else {

				session_start();
				$user = getUser($_SESSION['username']);
				$dest = decrypt($_POST['destination']);
				session_write_close();
				if (!$dest || strstr($dest, ':') == false){

					$data->error = 'Invalid destination code';

				} else {

					$dest = substr($dest, strpos($dest, ':')+1);
					$sendTo = getUser($dest);

					if (!is_object($sendTo) || empty($sendTo->username)){

						$data->error = 'That user does not exist in our system';

					} else {

						if ($user->id == $sendTo->id){

							$data->error = 'You cannot transfer to yourself silly';

						} else {

							$credits = (int) $_POST['credits'];
							if ($credits <= 0){

								$data->error = 'Please provide a sane amount of credits';

							} else {

								if ($user->credits < $credits){

									$data->error = 'You do not have enough credits';

								} else {

									$sql = 'UPDATE `users` SET `credits` = `credits` + '.$credits.' WHERE `id` = '.$sendTo->id;
									$res = $db->query($sql);

									usleep($conf->sleep);

									$sql = 'UPDATE `users` SET `credits` = `credits` - '.$credits.' WHERE `id` = '.$user->id;
									$db->query($sql);

									$data->message = sprintf('Successfully transferred %d credits to %s', $credits, $sendTo->username);
								}
							}
						}
					}
				}
			}
		}

		session_start();
		$user = getUser($_SESSION['username']);
		$_SESSION['credits'] = $user->credits;
		session_write_close();
		$show = 'account';
		break;

	case '404':

		header('HTTP/1.0 404 Not Found');
		$from = (!empty($_REQUEST['from']))? decrypt($_REQUEST['from']): 'home';
		$show = '404';
		break;

	default:
		
		$query = '';
		if (!empty($_REQUEST['page']))
			$query = '&from_page='.encrypt($_REQUEST['page']);
		header('Location: /?page=404'.$query);
		exit;

		break;
}

if (!empty($user) && is_object($user)){
	if ($user->credits >= $conf->goal){
		header('Location: /?page='.encrypt('home'));
		exit(0);
	}
}

require_once(__DIR__.'/lib/flag.php');
session_start();
echo $blade->run($show, [
	'content'   => 'sdfsdf',
	'active'    => $show,
	'data'      => $data,
	'user'      => $user,
	'conf'      => $conf,
	'logged_in' => (empty($_SESSION['logged_in']))? false: true
]);
session_write_close();
