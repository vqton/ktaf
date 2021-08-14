<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />

    @include('admin.layouts.partials.metatags')

    <link href="{{ asset('backend/css/styles.css') }}" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/v/bs5/dt-1.10.25/datatables.min.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js" crossorigin="anonymous">
    </script>



</head>

<body class="sb-nav-fixed">
    @include('admin.layouts.partials.navbar')
    <div id="layoutSidenav">
        <div id="layoutSidenav_nav">
            <nav class="sb-sidenav accordion sb-sidenav-light" id="sidenavAccordion">
                @include('admin.layouts.partials.siderbar')
                @include('admin.layouts.partials.status')
            </nav>
        </div>
        <div id="layoutSidenav_content">
            <main>
                <div class="px-4 container-fluid">
                    <h1 class="mt-4">Sidenav Light</h1>
                    <ol class="mb-4 breadcrumb">
                        <li class="breadcrumb-item"><a href="index.html">Dashboard</a></li>
                        <li class="breadcrumb-item active">Sidenav Light</li>
                    </ol>
                    <div class="mb-4 card">
                        <div class="card-body">
                            @yield('content')
                        </div>
                    </div>
                </div>
            </main>
            @include('admin.layouts.partials.footer')
        </div>
    </div>
    {{-- <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js" crossorigin="anonymous">
    </script> --}}



</body>
<script src="{{asset('backend/ckeditor/ckeditor.js')}}"></script>
<script type="text/javascript">
  CKEDITOR.replace( 'content', {
    filebrowserBrowseUrl: '/backend/ckfinder/ckfinder.html',
    filebrowserUploadUrl: '/backend/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Files&type=Files'
} );
</script>
<script src="{{ asset('backend/js/scripts.js') }}js/scripts.js"></script>

</html>
