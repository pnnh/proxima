package xyz.huable.dawn.ui.files

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import xyz.huable.dawn.model.FileItem
import java.io.File

class FileListViewModel(application: Application) : AndroidViewModel(application) {

    private val rootDir: File = application.filesDir

    private val _currentDir = MutableLiveData<File>(rootDir)
    val currentDir: LiveData<File> = _currentDir

    private val _files = MutableLiveData<List<FileItem>>()
    val files: LiveData<List<FileItem>> = _files

    private val _title = MutableLiveData<String>("文件")
    val title: LiveData<String> = _title

    init {
        loadFiles(rootDir)
    }

    fun navigateTo(dir: File) {
        _currentDir.value = dir
        loadFiles(dir)
    }

    /** Returns true if navigated up, false if already at root */
    fun navigateUp(): Boolean {
        val current = _currentDir.value ?: return false
        if (current.canonicalPath == rootDir.canonicalPath) return false
        navigateTo(current.parentFile ?: return false)
        return true
    }

    fun isAtRoot(): Boolean =
        _currentDir.value?.canonicalPath == rootDir.canonicalPath

    fun createDirectory(name: String): Boolean {
        val dir = File(_currentDir.value, name)
        return dir.mkdirs().also { if (it) refresh() }
    }

    fun createTextFile(name: String): Boolean {
        val fileName = if (name.endsWith(".txt")) name else "$name.txt"
        val file = File(_currentDir.value, fileName)
        return try {
            file.createNewFile()
            refresh()
            true
        } catch (e: Exception) {
            false
        }
    }

    fun refresh() {
        _currentDir.value?.let { loadFiles(it) }
    }

    private fun loadFiles(dir: File) {
        _title.value = if (dir.canonicalPath == rootDir.canonicalPath) "文件" else dir.name
        val items = dir.listFiles()
            ?.sortedWith(compareBy({ !it.isDirectory }, { it.name.lowercase() }))
            ?.map { file ->
                FileItem(
                    name = file.name,
                    path = file.absolutePath,
                    isDirectory = file.isDirectory,
                    size = if (file.isDirectory) (file.listFiles()?.size ?: 0).toLong()
                           else file.length(),
                    lastModified = file.lastModified(),
                    extension = if (file.isDirectory) "" else file.extension.lowercase()
                )
            } ?: emptyList()
        _files.value = items
    }
}

