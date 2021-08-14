<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

class CreateMetasTable extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('metas', function (Blueprint $table) {
            $table->id()->unique();
            $table->string('meta_title')->nullable(true)->default('Accounting Firm');
            $table->string('meta_keywords')->nullable(true)->default('Accounting Firm');
            $table->string('meta_description')->nullable(true)->default('Accounting Firm');
            $table->enum('robots', ['noindex ', 'index', 'nofollow', 'follow', 'none', 'noodp'])->default('index');
            $table->string('author')->nullable(true)->default('Quang Tôn');
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('metas');
    }
}
