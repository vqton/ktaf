@extends('admin.layouts.master')
@section('content')
<div class="row">
    <div class="col-lg-12 margin-tb">

        <h2>Create Topic</h2>

        <a class="btn btn-primary" href="{{ route('aboutus.index') }}" enctype="multipart/form-data"> Back</a>
    </div>
</div>
@if ($errors->any())
<div class="alert alert-danger">
    <ul>
        @foreach ($errors->all() as $error)
        <li>{{ $error }}</li>
        @endforeach
    </ul>
</div>
@endif
<form method="POST" action="{{ route('aboutus.store') }}">
    @csrf
    <div class="form-group">
        <label for="content">Content:</label>
        {{-- <input type="text" class="form-control" name="content" id="content"> --}}
        <textarea class="ckeditor form-control" name="content"></textarea>
    </div>
    <button type=" submit" class="btn btn-primary">Submit</button>
</form>

@endsection
