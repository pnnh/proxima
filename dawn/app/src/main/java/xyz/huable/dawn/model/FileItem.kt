package xyz.huable.dawn.model

data class FileItem(
    val name: String,
    val path: String,
    val isDirectory: Boolean,
    val size: Long,           // bytes for file, item count for directory
    val lastModified: Long,
    val extension: String
)

