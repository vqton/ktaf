@extends('admin.layouts.master')
@section('title',$metas[0]->meta_title)
@section('meta_keywords',$metas[0]->meta_keywords)
@section('meta_content',$metas[0]->meta_description)
@section('author',$metas[0]->author)
@section('robots',$metas[0]->robots)
@section('content')
<script>
    $(document).ready(function() {
    $('#example').DataTable();
} );
</script>
<div class="card-header">

    <div class="mb-2 pull-right">
        <i class="fas fa-table me-1"></i>
        <a class="btn btn-success" href="{{ route('meta.create') }}"> Create Tag</a>
    </div>
</div>

<div class="card-body">
    <table class="table table-bordered table-responsive-lg">
        <thead class="thead-dark">
            <tr>
                <th scope="col">Title</th>
                <th scope="col">Keywords</th>
                <th scope="col">Description</th>
                <th scope="col">Robots</th>
                <th scope="col">Author</th>
                <th scope="col">Created At</th>
                <th scope="col">Action</th>
            </tr>
        </thead>
        <tbody>
            @foreach ($metas as $meta )
            <tr>
                <td>{{ $meta->meta_title }}</td>
                <td>{{ $meta->meta_keywords }}</td>
                <td>{!! $meta->meta_description !!}</td>
                <td>{{ $meta->robots }}</td>
                <td>{{ $meta->author }}</td>
                <td>{{ $meta->created_at }}</td>
                <td>
                    <form action="" method="POST">

                        <a href="" title="show">
                            <i class="fas fa-eye text-success fa-lg"></i>
                        </a>

                        <a href="{{ route('meta.edit',$meta->id) }}">
                            <i class="fas fa-edit fa-lg"></i>
                        </a>

                        {{-- @csrf
                        @method('DELETE') --}}

                        {{-- <button type="submit" title="delete" style="border: none; background-color:transparent;">
                            <i class="fas fa-trash fa-lg text-danger"></i>
                        </button> --}}
                    </form>
                </td>
            </tr>
            @endforeach
    </table>

</div>
@if ($message = Session::get('success'))
<div class="alert alert-success">
    <p></p>
</div>
@endif
@endsection
