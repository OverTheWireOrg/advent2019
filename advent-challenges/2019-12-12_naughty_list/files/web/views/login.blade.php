@extends('main')

@section('content')
<div class="form-box">
  <h1>Login</h1>
  @if (!empty($data->error))
    <p style="font-weight: bold; color: red">{{ $data->error }}</p>
  @else
    <p>Or if you don't yet have an account you can <a href="/?page={{ encrypt('register') }}">register</a> one.</p>
  @endif
  <p>
  <form action="/?page={{ encrypt('login') }}" method="post">
      <label for="username">User Name</label>
      <input id="username" type="text" name="username">
    
      <label for="password">Password</label>
      <input id="password" type="password" name="password">

      <input class="button-primary" type="submit" value="Submit" />
  </form>
  </p>
</div>
@endsection