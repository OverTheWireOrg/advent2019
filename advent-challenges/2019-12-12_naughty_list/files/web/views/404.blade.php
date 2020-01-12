@extends('main')

@section('content')
<div>
  <h2>File not found.</h2>
  <p style="">The file you requested could not be found on the server. If
    you believe this error is not your fault, please <a href="/?page={{ encrypt('contact') }}">contact us</a>.</p>
</div>
@endsection