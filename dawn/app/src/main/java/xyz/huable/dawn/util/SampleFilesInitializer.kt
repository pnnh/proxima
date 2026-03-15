package xyz.huable.dawn.util

import android.content.Context
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.LinearGradient
import android.graphics.Paint
import android.graphics.RectF
import android.graphics.Shader
import java.io.File
import java.io.FileOutputStream

object SampleFilesInitializer {

    private const val PREF_NAME = "dawn_prefs"
    private const val KEY_INITIALIZED = "sample_files_initialized"

    fun initIfNeeded(context: Context) {
        val prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE)
        if (prefs.getBoolean(KEY_INITIALIZED, false)) return
        createSampleFiles(context.filesDir)
        prefs.edit().putBoolean(KEY_INITIALIZED, true).apply()
    }

    private fun createSampleFiles(baseDir: File) {
        File(baseDir, "README.txt").writeText(
            "欢迎使用 Dawn 文件管理器！\n\n功能包括：\n· 浏览文件与文件夹\n· 新建文件夹\n· 新建文本文件\n"
        )
        File(baseDir, "notes.txt").writeText(
            "我的笔记\n========\n1. 学习 Android 开发\n2. 创建文件管理器\n3. 实现文件操作功能\n"
        )

        val docsDir = File(baseDir, "文档").also { it.mkdirs() }
        File(docsDir, "报告.txt").writeText(
            "工作报告\n日期：2026-03-15\n\n项目进展顺利，各模块按计划推进。\n"
        )
        File(docsDir, "计划.txt").writeText(
            "项目计划\n1. 需求分析 ✓\n2. 设计阶段 ✓\n3. 开发阶段 进行中\n4. 测试阶段 待开始\n"
        )

        val imagesDir = File(baseDir, "图片").also { it.mkdirs() }
        createColorBlockImage(File(imagesDir, "color_block.png"))
        createGradientImage(File(imagesDir, "gradient.png"))
        createCheckerImage(File(imagesDir, "checker.png"))

        File(baseDir, "data.bin").writeBytes(ByteArray(256) { it.toByte() })

        val downloadsDir = File(baseDir, "下载").also { it.mkdirs() }
        File(downloadsDir, "archive.bin").writeBytes(ByteArray(1024) { (it % 256).toByte() })
        File(downloadsDir, "来源.txt").writeText("该目录存放从网络下载的文件。\n")
    }

    private fun createColorBlockImage(file: File) {
        val bmp = Bitmap.createBitmap(200, 200, Bitmap.Config.ARGB_8888)
        val canvas = Canvas(bmp)
        val paint = Paint(Paint.ANTI_ALIAS_FLAG)
        paint.color = Color.rgb(100, 149, 237)
        canvas.drawRect(0f, 0f, 200f, 200f, paint)
        paint.color = Color.WHITE
        paint.textSize = 36f
        paint.textAlign = Paint.Align.CENTER
        canvas.drawText("Dawn", 100f, 110f, paint)
        FileOutputStream(file).use { bmp.compress(Bitmap.CompressFormat.PNG, 100, it) }
        bmp.recycle()
    }

    private fun createGradientImage(file: File) {
        val bmp = Bitmap.createBitmap(200, 200, Bitmap.Config.ARGB_8888)
        val canvas = Canvas(bmp)
        val paint = Paint()
        paint.shader = LinearGradient(
            0f, 0f, 200f, 200f,
            intArrayOf(Color.rgb(255, 100, 100), Color.rgb(100, 100, 255)),
            null,
            Shader.TileMode.CLAMP
        )
        canvas.drawRect(0f, 0f, 200f, 200f, paint)
        FileOutputStream(file).use { bmp.compress(Bitmap.CompressFormat.PNG, 100, it) }
        bmp.recycle()
    }

    private fun createCheckerImage(file: File) {
        val bmp = Bitmap.createBitmap(200, 200, Bitmap.Config.ARGB_8888)
        val canvas = Canvas(bmp)
        val paintA = Paint().apply { color = Color.WHITE }
        val paintB = Paint().apply { color = Color.LTGRAY }
        val cell = 20f
        var row = 0; var y = 0f
        while (y < 200f) {
            var col = 0; var x = 0f
            while (x < 200f) {
                canvas.drawRect(RectF(x, y, x + cell, y + cell),
                    if ((row + col) % 2 == 0) paintA else paintB)
                x += cell; col++
            }
            y += cell; row++
        }
        FileOutputStream(file).use { bmp.compress(Bitmap.CompressFormat.PNG, 100, it) }
        bmp.recycle()
    }
}

