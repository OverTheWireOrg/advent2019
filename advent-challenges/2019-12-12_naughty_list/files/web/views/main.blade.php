<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Elf Naughty List</title>

  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link href="//fonts.googleapis.com/css?family=Raleway:400,300,600" rel="stylesheet" type="text/css">
  <link rel="stylesheet" href="css/normalize.css">
  <link rel="stylesheet" href="css/skeleton.css">

  <link rel="icon" type="image/png" href="images/favicon.png">

  <style>

    .bold {

      font-weight: bold;
    }

    html, body {
      height: 100%;
    }

    body {
      background: rgb(175,0,0);
      background: linear-gradient(315deg, rgba(175,0,0,1) 0%, rgba(196,0,0,1) 100%);

      background-attachment: fixed;
    }

    @font-face {
      font-family: 'heartbeat_in_christmasregular';
      src: url('/fonts/xmas-webfont.woff2') format('woff2'),
      url('/fonts/xmas-webfont.woff') format('woff');
      font-weight: normal;
      font-style: normal;
    }

    .container .row {
      background-color: #a60000;
    }

    .navbar {
      height: 50px;
      background-color: #af0000;
    }

    .navbar .container {
      height: 50px;
      background-color: #af0000;
      border-left: 1px solid #8c0000;
      border-right: 1px solid #8c0000;
      border-bottom: 1px solid #8c0000;
    }

    .navbar .container .row {
      background-color: #830000;
    }

    .nav {
      margin: 0;
      padding: 0;
      float: right;
      list-style-type: none;
    }

    .nav .container {

    }

    .nav li {
      display: inline;
      padding: 0;
      margin: 0;
    }

    .nav li a {
      display: inline-block;
      height: 48px;
      line-height: 48px;
      padding: 0 15px;
      font-weight: bold;
      color: #fff;
      text-decoration: none;
      border-radius: 5px 5px 0px 0px;
      -moz-border-radius: 5px 5px 0px 0px;
      -webkit-border-radius: 5px 5px 0px 0px;
      margin-top: 2px;
    }

    .nav li a.active {
      background-color: #a60000;
    }

    .nav li a:hover, .nav li a:hover {
      background-color: #c60000;
    }

    .header h1 {
      font-family: heartbeat_in_christmasregular;
      font-size: 15em;
      color: #c40000;
      text-shadow: 0px 0px 7px rgba(217,203,182,1);
      margin-top: 17%;
      margin-bottom: 0;
    }

    .header {
      background: url(/images/header.png) no-repeat;
      background-size: 100% auto;
      height: 403px;
    }

    .main {
      background: url(/images/bg.png) repeat-y black;
      background-size: 100% auto;
      padding-bottom: 200px;
    }

    .footer {
      background: url(/images/footer.png) no-repeat;
      background-size: 100% auto;
      height: 200px;
      margin-top: -200px;
    }

    .page {
      min-height: 100%;
    }

    .content {
      padding: 0 15px;
      max-width: 80%;
      margin: auto;
      padding: 0 50px;
    }

    .form-box {
      max-width: 80%;
      margin: auto;
      padding: 0 50px;
    }

    .form-box h1, .form-box p {
      text-align: center;
    }

    .form-box input, .form-box textarea {
      width: 100%;
    }

    .infobox {
      margin-bottom: 15px;
      padding: 15px;
      border: 1px solid #ffb04c;
      -webkit-border-radius: 10px;
      -moz-border-radius: 10px;
      border-radius: 10px;
      background: #ffdfad;
    }

    .infobox .row {
      background: none !important;
    }

    .infobox input, .infobox textarea {
      width: 100%;
    }

    .buybox {
      height: 180px;
      margin-bottom: 15px;
      -webkit-border-radius: 7px;
      -moz-border-radius: 7px;
      border-radius: 7px;
      overflow: hidden;
      opacity: 0.7;
    }

    .buybox:hover {

      opacity: 1;
    }

    .buybox a {

      display: block;
      width: 100%;
      height: 100%;
      overflow: hidden;
      text-indent: -1000px;
    }

    .buy10 {
      background-image: url(/images/10.png);
      background-size: 100% auto;
    }

    .buy25 {
      background-image: url(/images/25.png);
      background-size: 100% auto;
    }

    .buy99 {
      background-image: url(/images/99.png);
      background-size: 100% auto;
    }

  </style>

</head>
<body>
  <div class="section navbar">
    <div class="container">
      <div class="row">
        <div class="twelve columns">
          <ul class="nav">
            <li><a href="/"{!!  $active == 'home' ? ' class="active"': '' !!}>Home</a></li>
            <li><a href="/?page={{ encrypt('contact') }}"{!!  $active == 'contact' ? ' class="active"': '' !!}>Contact</a></li>
            @if ($logged_in == false)
            <li><a href="/?page={{ encrypt('login') }}"{!!  $active == 'login' ? ' class="active"': '' !!}>Login</a></li>
            @else
            <li><a href="/?page={{ encrypt('account') }}"{!!  $active == 'account' ? ' class="active"': '' !!}>Account</a></li>
            <li><a href="/?page={{ encrypt('logout') }}">Logout</a></li>
            @endif
          </ul>
        </div>
      </div>
    </div>
  </div>

  <div class="page container">
    <div class="row header">
      <div class="twelve columns">
        <h1 align="center">Naughty List</h1>
      </div>
    </div>
    <div class="row main">
      <div class="one column">&nbsp;</div>
      <div class="ten columns">
        <div class="content">
          @yield('content')
        </div>
      </div>
      <div class="one column">&nbsp;</div>
    </div>

    <div class="row footer">
      <div class="one column">
        &nbsp;
      </div>
    </div>
  </div>

</body>
</html>
