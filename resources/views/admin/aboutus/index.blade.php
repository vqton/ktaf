@extends('admin.layouts.master')

@section('content')
<div class="card-header">
    <div class="col-sm-12">

        @if(session()->get('success'))
        <div class="alert alert-success">
            {{ session()->get('success') }}
        </div>
        @endif
    </div>
    <div class="pull-right mb-2">
        <i class="fas fa-table me-1"></i>
        <a class="btn btn-success" href="{{ route('aboutus.create') }}"> Create Topic</a>
    </div>



    @if(session('status'))
    <div class="alert alert-success mb-1 mt-1">
        {{ session('status') }}
    </div>
    @endif
    <table class="table table-bordered table-responsive-lg">
        <thead class="thead-dark">
            <tr>
                <th scope="col">Id</th>
                <th scope="col">Content</th>
                <th scope="col">Created At</th>
                <th scope="col">Updated At</th>
                <th scope="col">Action</th>
            </tr>
        </thead>
        <tbody>
            @foreach ($items as $item )
            <tr>
                <td>{{ $item->id }}</td>
                <td>{!! $item->content !!}</td>
                <td>{{ $item->updated_at }}</td>
                <td>{{ $item->created_at }}</td>
                <td>
                    <form action="{{ route('aboutus.destroy',$item->id) }}" method="POST">

                        <a href="" title="show">
                            <i class="fas fa-eye text-success  fa-lg"></i>
                        </a>

                        <a href="{{ route('aboutus.edit',$item->id) }}">
                            <i class="fas fa-edit  fa-lg"></i>
                        </a>
                        @csrf
                        @method('DELETE')
                        <button type="submit" title="delete" style="border: none; background-color:transparent;">
                            <i class="fas fa-trash fa-lg text-danger"></i>
                        </button>
                    </form>
                </td>
            </tr>
            @endforeach
    </table>
</div>

@endsection
