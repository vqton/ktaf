index.blade.php
edit.blade.php
create.blade.php
 ("index","edit", "create")|foreach { New-Item -Name "$"}



  ("index","edit", "create")|foreach { New-Item -Name "$_.blade.php" -Value ("@extends('admin.layouts.master')`r`n@section('content')`r`n`r`n@endsection")}
