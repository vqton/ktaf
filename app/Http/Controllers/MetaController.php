<?php

namespace App\Http\Controllers;

use App\Models\Meta;
use Illuminate\Http\Request;
use Illuminate\Support\Arr;
use Illuminate\Support\Facades\DB;

class MetaController extends Controller
{
    /**
     * Display a listing of the resource.
     *
     * @return \Illuminate\Http\Response
     */
    public function index()
    {
        //
        $metas = Meta::all();
        return view('admin.meta.index', compact('metas'));
    }

    /**
     * Show the form for creating a new resource.
     *
     * @return \Illuminate\Http\Response
     */
    public function create()
    {
        //
    }

    /**
     * Store a newly created resource in storage.
     *
     * @param  \Illuminate\Http\Request  $request
     * @return \Illuminate\Http\Response
     */
    public function store(Request $request)
    {
        //
    }

    /**
     * Display the specified resource.
     *
     * @param  \App\Models\Meta  $meta
     * @return \Illuminate\Http\Response
     */
    public function show(Meta $meta)
    {
        //
    }

    /**
     * Show the form for editing the specified resource.
     *
     * @param  \App\Models\Meta  $meta
     * @return \Illuminate\Http\Response
     */
    public function edit($id)
    {
        $meta = Meta::findOrFail($id);
        $enumRobot = $this->getEnumValues('metas', 'robots');
        return view('admin.meta.edit', compact('meta', 'enumRobot'));
    }

    /**
     * Update the specified resource in storage.
     *
     * @param  \Illuminate\Http\Request  $request
     * @param  \App\Models\Meta  $meta
     * @return \Illuminate\Http\Response
     */
    public function update(Request $request, $id)
    {
        //\
        $request->validate([
            'meta_title' => 'required',
            'meta_description' => 'required|max:60',
            'author' => 'required',
            'meta_keywords' => 'required'
        ]);
// dd($request);

        $meta = Meta::findOrFail($id);

        $meta->update([
            'meta_title' => $request->input('meta_title'),
            'author' => $request->author,
            'meta_description' => $request->input('meta_description'),
            'meta_keywords' => $request->meta_keywords,
            'robots' => $request->robot,
        ]);
        return redirect()->route('meta.index')
            ->with('success', 'Meta tag updated successfully');
    }

    /**
     * Remove the specified resource from storage.
     *
     * @param  \App\Models\Meta  $meta
     * @return \Illuminate\Http\Response
     */
    public function destroy(Meta $meta)
    {
        //
    }

    public static function getEnumValues($table, $column)
    {
        $type = DB::select(DB::raw("SHOW COLUMNS FROM $table WHERE Field = '{$column}'"))[0]->Type;
        preg_match('/^enum\((.*)\)$/', $type, $matches);
        $enum = array();
        foreach (explode(',', $matches[1]) as $value) {
            $v = trim($value, "'");
            $enum = Arr::add($enum, $v, $v);
        }
        return $enum;
    }
    public function __construct()
    {
        $this->middleware('auth');
    }
}
