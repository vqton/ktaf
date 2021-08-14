<?php

namespace App\Http\Controllers;

use App\Http\Requests\CreateAboutUsRequest;
use App\Models\AboutUs;
use Illuminate\Http\Request;

class AboutUsController extends Controller
{
    public function __construct()
    {
        $this->middleware('auth');
    }
    /**
     * Display a listing of the resource.
     *
     * @return \Illuminate\Http\Response
     */
    public function index()
    {
        //
        $items = AboutUs::all();
        return view('admin.aboutus.index', compact('items'));
    }

    /**
     * Show the form for creating a new resource.
     *
     * @return \Illuminate\Http\Response
     */
    public function create()
    {
        //
        return view('admin.aboutus.create');
    }

    /**
     * Store a newly created resource in storage.
     *
     * @param  \Illuminate\Http\Request  $request
     * @return \Illuminate\Http\Response
     */
    public function store(CreateAboutUsRequest $request)
    {
        $request->validate([
            'content' => 'required',

        ]);

        // $oRec = new AboutUs([
        //     'content' => $request->input('content'),
        // ]);

        // $oRec.create();
        // dd( $request);

        $ob = new AboutUs(['content' => $request->input('content')]);
        $ob->save();
        return redirect()->route('aboutus.index')
            ->with('success', 'Item created successfully.');
    }

    /**
     * Display the specified resource.
     *
     * @param  \App\Models\AboutUs  $aboutUs
     * @return \Illuminate\Http\Response
     */
    public function show(AboutUs $aboutUs)
    {
        //
    }

    /**
     * Show the form for editing the specified resource.
     *
     * @param  \App\Models\AboutUs  $aboutUs
     * @return \Illuminate\Http\Response
     */
    public function edit($id)
    {
        //
        $item = AboutUs::find($id);
        return view('admin.aboutus.edit', compact('item'));
    }

    /**
     * Update the specified resource in storage.
     *
     * @param  \Illuminate\Http\Request  $request
     * @param  \App\Models\AboutUs  $aboutUs
     * @return \Illuminate\Http\Response
     */
    public function update($id, Request $request)
    {
        //
        $item = AboutUs::find($id);
        $item->update(['content' => $request->input('content'),
        ]);
        return redirect()->route('aboutus.index')
            ->with('success', 'Item updated successfully');
    }

    /**
     * Remove the specified resource from storage.
     *
     * @param  \App\Models\AboutUs  $aboutUs
     * @return \Illuminate\Http\Response
     */
    public function destroy($id)
    {
        //

        $aboutUs = AboutUs::find($id);
        $aboutUs->delete();

        return redirect()->route('aboutus.index')
            ->with('success', 'Item created successfully.');
    }
}
