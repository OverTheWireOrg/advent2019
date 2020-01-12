@extends('main')

@section('content')
<div class="form-box">
  <h1>Contact Us</h1>
  @if (!empty($data->message))
    <p style="font-weight: bold; color: green">Thanks for filling up our spam folder.</p>
  @else
    <p>Fill in the form below and we will get back to you.</p>
  @endif
  <p>
  <form action="/?page={{ encrypt('contact') }}" method="post">
      <label for="name">Name</label>
      <input id="name" type="text" name="name" required />
    
      <label for="email">Email</label>
      <input id="email" type="email" name="email" required />
    
      <label for="message">Message</label>
      <textarea id="message" name="message"></textarea>
      <input class="button-primary" type="submit" value="Submit" required />
  </form>
  </p>
</div>
@endsection