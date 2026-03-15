package xyz.huable.dawn.ui.compose.screens

import android.graphics.BitmapFactory
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.unit.dp
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import xyz.huable.dawn.R
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

private val PREVIEW_DATE_FORMAT = SimpleDateFormat("yyyy-MM-dd HH:mm", Locale.getDefault())
private val IMAGE_EXTENSIONS = setOf("jpg", "jpeg", "png", "gif", "bmp", "webp")
private val TEXT_EXTENSIONS = setOf("txt", "md", "log", "csv", "xml", "json", "html", "htm", "kt", "java", "py")

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FilePreviewScreen(
    filePath: String,
    onNavigateUp: () -> Unit
) {
    val file = remember(filePath) { File(filePath) }
    val ext = remember(filePath) { file.extension.lowercase() }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(
                        text = file.name,
                        maxLines = 1
                    )
                },
                navigationIcon = {
                    IconButton(onClick = onNavigateUp) {
                        Icon(
                            imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = "返回"
                        )
                    }
                }
            )
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            when {
                ext in IMAGE_EXTENSIONS -> ImagePreview(file)
                ext in TEXT_EXTENSIONS -> TextPreview(file)
                else -> FileInfoView(file)
            }
        }
    }
}

@Composable
private fun ImagePreview(file: File) {
    var bitmap by remember { mutableStateOf<android.graphics.Bitmap?>(null) }
    var loading by remember { mutableStateOf(true) }

    LaunchedEffect(file.absolutePath) {
        bitmap = withContext(Dispatchers.IO) {
            runCatching { BitmapFactory.decodeFile(file.absolutePath) }.getOrNull()
        }
        loading = false
    }

    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
        when {
            loading -> CircularProgressIndicator()
            bitmap != null -> Image(
                bitmap = bitmap!!.asImageBitmap(),
                contentDescription = file.name,
                contentScale = ContentScale.Fit,
                modifier = Modifier.fillMaxSize()
            )
            else -> FileInfoView(file)
        }
    }
}

@Composable
private fun TextPreview(file: File) {
    var content by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(file.absolutePath) {
        content = withContext(Dispatchers.IO) {
            runCatching { file.readText(Charsets.UTF_8) }.getOrElse { "（无法读取文件内容）" }
        }
    }

    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
        if (content == null) {
            CircularProgressIndicator()
        } else {
            Text(
                text = content!!,
                style = MaterialTheme.typography.bodyMedium,
                fontFamily = FontFamily.Monospace,
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(16.dp)
            )
        }
    }
}

@Composable
private fun FileInfoView(file: File) {
    val ext = file.extension.lowercase()
    val iconRes = when {
        ext in IMAGE_EXTENSIONS -> R.drawable.ic_file_image
        ext in TEXT_EXTENSIONS -> R.drawable.ic_file_text
        else -> R.drawable.ic_file_binary
    }

    Column(
        modifier = Modifier.fillMaxSize(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Icon(
            painter = painterResource(iconRes),
            contentDescription = null,
            modifier = Modifier.size(72.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = file.name,
            style = MaterialTheme.typography.titleMedium
        )
        Spacer(modifier = Modifier.height(8.dp))
        Text(
            text = formatPreviewSize(file.length()),
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = PREVIEW_DATE_FORMAT.format(Date(file.lastModified())),
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

private fun formatPreviewSize(bytes: Long): String = when {
    bytes < 1024L -> "$bytes B"
    bytes < 1024L * 1024 -> "%.1f KB".format(bytes / 1024f)
    bytes < 1024L * 1024 * 1024 -> "%.1f MB".format(bytes / (1024f * 1024f))
    else -> "%.1f GB".format(bytes / (1024f * 1024f * 1024f))
}
