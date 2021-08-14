@extends('admin.layouts.master')
@section('content')
<div class="row">
    <div class="col-lg-12 margin-tb">

        <h2>Edit Meta Tags</h2>

        <a class="btn btn-primary" href="{{ route('meta.index') }}" enctype="multipart/form-data"> Back</a>
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
<form action="{{ route('meta.update',$meta->id) }}" method="POST" enctype="multipart/form-data">
    @csrf
    @method('PUT')
    <div class="form-group">
        <label for="title">Title:</label>
        <input type="text" name="meta_title" class="form-control" id="title" value="{{ $meta->meta_title }}">
    </div>
    <div class="form-group">
        <label for="content">Description:</label>
        <textarea class="ckeditor form-control"  name="meta_description">{{ $meta->meta_description}}</textarea>
      </div>
     <div class=" form-group">
        <label for="keywords">Keywords:</label>
        <input type="text" name="meta_keywords" class="form-control" id="keywords" value="{{ $meta->meta_keywords }}">
    </div>
    <div class=" form-group">
        <label for="author">Author:</label>
        <input type="text" class="form-control" name="author" id="author" value="{{ $meta->author}}">
    </div>
     <div class="form-group">
        <label for="robot">Robots: </label>
        <select class="form-control" name="robot" id="robot">
            @foreach($enumRobot as $item)
            @if ($item === $meta->robots)
            <option value={{ $item }} selected="check">{{$item}}</option>
            @endif
            <option value={{ $item }}>{{$item}}</option>
            @endforeach

        </select>
    </div>
    <button type=" submit" class="btn btn-primary">Submit</button>
</form>

@endsection

{{-- <div class="row">
        <div class="col-xs-12 col-sm-12 col-md-12">
            <div class="form-group">
                <strong>Title:</strong>
                <input type="text" name="name" value="{{ $meta->meta_title }}" class="form-control"
placeholder="Title">
@error('name')
<div class="mt-1 mb-1 alert alert-danger">{{ $message }}</div>
@enderror
</div>
</div>
<div class="col-xs-12 col-sm-12 col-md-12">
    <div class="form-group">
        <strong>Keywords:</strong>
        <input type="text" name="email" class="form-control" placeholder="Keywords" value="{{ $meta->meta_keywords }}">
        @error('email')
        <div class="mt-1 mb-1 alert alert-danger">{{ $message }}</div>
        @enderror
    </div>
</div>
<div class="col-xs-12 col-sm-12 col-md-12">
    <div class="form-group">
        <strong>Content:</strong>
        <input type="text" name="address" value="{{ $meta->content }}" class="form-control" placeholder="Content">
        @error('Content')
        <div class="mt-1 mb-1 alert alert-danger">{{ $message }}</div>
        @enderror
    </div>
</div>
<button type="submit" class="btn btn-primary">Submit</button>
</div> --}}
