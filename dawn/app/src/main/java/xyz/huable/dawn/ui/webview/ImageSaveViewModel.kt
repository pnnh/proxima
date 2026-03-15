package xyz.huable.dawn.ui.webview

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.File
import java.net.URL

class ImageSaveViewModel(application: Application) : AndroidViewModel(application) {

    val rootDir: File = application.filesDir

    /** Returns rootDir itself followed by all immediate subdirectories, sorted by name. */
    fun listSaveDirectories(): List<File> {
        val subs = rootDir.listFiles()
            ?.filter { it.isDirectory }
            ?.sortedBy { it.name.lowercase() }
            ?: emptyList()
        return listOf(rootDir) + subs
    }

    /**
     * Creates a new directory named [name] under [parent].
     * [parent] must be within [rootDir] (security check).
     * Returns the created [File], or null on failure.
     */
    fun createDirectory(parent: File, name: String): File? {
        if (name.isBlank()) return null
        if (!parent.canonicalPath.startsWith(rootDir.canonicalPath)) return null
        val dir = File(parent, name.trim())
        return if (dir.mkdirs() || (dir.exists() && dir.isDirectory)) dir else null
    }

    /**
     * Downloads [imageUrl] into [targetDir].
     * Extension is determined from the server's Content-Type header first, then the URL path.
     * Must be called from a coroutine. Returns true on success.
     */
    suspend fun saveImageFromUrl(imageUrl: String, targetDir: File): Boolean = try {
        withContext(Dispatchers.IO) {
            val connection = URL(imageUrl).openConnection()
            connection.connect()
            val contentType = connection.contentType?.lowercase() ?: ""

            val urlSegment = imageUrl.substringBefore('?').substringAfterLast('/')
            val urlExt = urlSegment.substringAfterLast('.', "").lowercase()
            val knownExts = setOf("jpg", "jpeg", "png", "gif", "webp", "bmp", "svg", "tif", "tiff", "ico")

            // Content-Type takes priority over URL extension (server is authoritative)
            val ext = when {
                "jpeg" in contentType || "jpg" in contentType -> "jpg"
                "png" in contentType -> "png"
                "gif" in contentType -> "gif"
                "webp" in contentType -> "webp"
                "bmp" in contentType -> "bmp"
                "svg" in contentType -> "svg"
                urlExt in knownExts -> if (urlExt == "jpeg") "jpg" else urlExt
                else -> "jpg"
            }

            val baseName = urlSegment
                .substringBeforeLast('.')
                .replace(Regex("[^a-zA-Z0-9._-]"), "_")
                .take(80)
                .ifBlank { "image_${System.currentTimeMillis()}" }

            val dest = uniqueFile(targetDir, "$baseName.$ext")
            connection.getInputStream().use { input ->
                dest.outputStream().use { output -> input.copyTo(output) }
            }
            true
        }
    } catch (_: Exception) {
        false
    }

    /** Returns a non-colliding [File] path inside [dir], appending (1), (2)… if needed. */
    private fun uniqueFile(dir: File, name: String): File {
        var candidate = File(dir, name)
        if (!candidate.exists()) return candidate
        val base = name.substringBeforeLast('.')
        val ext = name.substringAfterLast('.', missingDelimiterValue = "")
        var i = 1
        while (candidate.exists()) {
            candidate = if (ext.isNotEmpty()) File(dir, "$base($i).$ext")
                        else File(dir, "$base($i)")
            i++
        }
        return candidate
    }
}
