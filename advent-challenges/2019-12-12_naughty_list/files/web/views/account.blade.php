@extends('main')

@section('content')
<div>
  <h2>Your Account</h2>
  <div class="infobox">
    <h4>{{ $user->username}}</h4>
    <div class="row">
      <div class="six columns"><span class="bold">Credits:</span></div>
      <div class="six columns"><span>{{ $user->credits }} / {{ $conf->goal }}</span></div>
    </div>
    <div class="row">
      <div class="six columns"><span class="bold">Next Credit:</span></div>
      <div class="six columns"><span id="timer"></span></div>
    </div>
  </div>
  <div class="row" style="background: none;">
    <div class="four columns buybox buy10"><a href="javascript:alert('no privs');" title="Purchase 10 credits">buy 10</a></div>
    <div class="four columns buybox buy25"><a href="javascript:alert('no privs');" title="Purchase 25 credits">buy 25</a></div>
    <div class="four columns buybox buy99"><a href="javascript:alert('no privs');" title="Purchase 99 credits">buy 99</a></div>
  </div>

  <div class="infobox">
    <h4>Transfer Credits</h4>
    @if (!empty($data->error))
      <p style="color: red">{{ $data->error }}</span>.</p>
    @endif
    @if (!empty($data->message))
      <p style="color: green">{{ $data->message }}</span>.</p>
    @endif
    <p>You can transfer your credits to another elf by using a destination
       code eg.<br /><span class="bold">{{ encrypt('sendto:santa') }}</span></p>
    <form method="post" action="/?page={{ encrypt('account') }}">
      <div class="row">
        <div style="line-height: 40px" class="three columns"><span class="bold">Credits:</span></div>
        <div class="nine columns"><span><input  name="credits" type="number" min="1" max="{{ $user->credits }}" step="1" value="1" width="100%" /></span></div>
      </div>
      <div class="row">
        <div style="line-height: 40px"  class="three columns"><span class="bold">Destination Code:</span></div>
        <div class="nine columns"><input name="destination" type="text" /></div>
      </div>
      <div class="row">
        <div style="line-height: 40px"  class="three columns">&nbsp;</div>
        <div class="nine columns"><input class="button-primary" type="submit" value="Transfer Credits" /></div>
      </div>
    </form>
  </div>
</div>
<script>
var countDownDate=new Date("Dec 25, 2019 00:00:00").getTime(),x=setInterval(function(){var e=(new Date).getTime(),t=countDownDate-e,n=Math.floor(t/864e5),o=Math.floor(t%864e5/36e5),a=Math.floor(t%36e5/6e4),r=Math.floor(t%6e4/1e3);document.getElementById("timer").innerHTML=n+"d "+o+"h "+a+"m "+r+"s ",t<0&&(clearInterval(x),document.getElementById("timer").innerHTML="EXPIRED")},1e3);
</script>
@endsection