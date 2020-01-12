@extends('main')

@section('content')
<div class="form-box">
  <h1>Register</h1>
  @if (!empty($data->error))
    <p style="font-weight: bold; color: red">{{ $data->error }}</p>
  @else
    @if (!empty($data->message))
      <p style="font-weight: bold; color: green">{{ $data->message }}</p>
    @else
      <p>You can register for an account below.</p>
    @endif
  @endif
  <p>
  <form action="/?page={{ encrypt('register') }}" method="post">
      <label for="username">User Name</label>
      <input id="username" type="text" name="username" />
    
      <label for="password">Password</label>
      <input id="password" type="password" name="password" />
    
      <label for="password">Confirm Password</label>
      <input id="confirm" type="password" name="confirm" />

      <input type="hidden" name="redirect" value="{{ $data->redirect }}" />
      <input class="button-primary" type="submit" value="Register" />
  </form>
  </p>
</div>
@endsection