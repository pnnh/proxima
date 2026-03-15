package xyz.huable.dawn.ui.compose.screens

import android.annotation.SuppressLint
import android.graphics.Bitmap
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.activity.compose.BackHandler
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.selection.selectable
import androidx.compose.foundation.selection.selectableGroup
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.ArrowDropDown
import androidx.compose.material.icons.filled.Close
import androidx.compose.ui.draw.rotate
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.RadioButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.lifecycle.viewmodel.compose.viewModel
import kotlinx.coroutines.launch
import xyz.huable.dawn.R
import xyz.huable.dawn.ui.webview.ImageSaveViewModel
import java.io.File

private data class DirNode(
    val dir: File,
    val depth: Int,
    val expanded: Boolean,
    val hasChildren: Boolean,
)

@SuppressLint("SetJavaScriptEnabled")
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun WebViewScreen(
    url: String,
    onNavigateUp: () -> Unit,
    saveViewModel: ImageSaveViewModel = viewModel(),
) {
    var webViewRef by remember { mutableStateOf<WebView?>(null) }
    var currentUrl by remember { mutableStateOf(url) }
    var hasLoadError by remember { mutableStateOf(false) }
    // non-null when user long-pressed an image; drives the context-menu dialog
    var pendingImageUrl by remember { mutableStateOf<String?>(null) }
    var showSavePicker by remember { mutableStateOf(false) }
    val snackbarHostState = remember { SnackbarHostState() }
    val scope = rememberCoroutineScope()

    // Pre-read strings so they can be used inside coroutine lambdas
    val strSaved = stringResource(R.string.success_save_image)
    val strFailed = stringResource(R.string.error_save_image_failed)

    BackHandler {
        if (webViewRef?.canGoBack() == true) webViewRef?.goBack()
        else onNavigateUp()
    }

    Scaffold(
        snackbarHost = { SnackbarHost(snackbarHostState) },
        topBar = {
            TopAppBar(
                navigationIcon = {
                    IconButton(onClick = {
                        if (webViewRef?.canGoBack() == true) webViewRef?.goBack()
                        else onNavigateUp()
                    }) {
                        Icon(
                            imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = stringResource(R.string.action_cancel)
                        )
                    }
                },
                title = {
                    Text(
                        text = currentUrl,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        style = MaterialTheme.typography.bodyMedium,
                    )
                },
                actions = {
                    // Close button: immediately pops back to the file list
                    IconButton(onClick = onNavigateUp) {
                        Icon(
                            imageVector = Icons.Default.Close,
                            contentDescription = stringResource(R.string.action_close_webview)
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
            AndroidView(
                factory = { context ->
                    WebView(context).apply {
                        webViewClient = object : WebViewClient() {
                            override fun onPageStarted(view: WebView, url: String?, favicon: Bitmap?) {
                                url?.let { currentUrl = it }
                                hasLoadError = false
                            }

                            override fun onPageFinished(view: WebView, url: String?) {
                                url?.let { currentUrl = it }
                            }

                            override fun onReceivedError(
                                view: WebView,
                                request: WebResourceRequest,
                                error: WebResourceError,
                            ) {
                                if (request.isForMainFrame) hasLoadError = true
                            }
                        }
                        // Long-press to detect image elements; return true to suppress the
                        // default WebView context menu.
                        setOnLongClickListener {
                            val result = hitTestResult
                            if (result.type == WebView.HitTestResult.IMAGE_TYPE ||
                                result.type == WebView.HitTestResult.SRC_IMAGE_ANCHOR_TYPE
                            ) {
                                result.extra?.let { pendingImageUrl = it }
                            }
                            true
                        }
                        settings.javaScriptEnabled = true
                        settings.domStorageEnabled = true
                        loadUrl(url)
                        webViewRef = this
                    }
                },
                update = { webViewRef = it },
                onRelease = { it.destroy() },
                modifier = Modifier.fillMaxSize(),
            )

            if (hasLoadError) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = stringResource(R.string.error_page_load_failed),
                        color = MaterialTheme.colorScheme.error,
                        style = MaterialTheme.typography.bodyLarge,
                    )
                }
            }
        }
    }

    // Step 1 — image context menu (shown immediately after long-press)
    if (pendingImageUrl != null && !showSavePicker) {
        AlertDialog(
            onDismissRequest = { pendingImageUrl = null },
            title = { Text(stringResource(R.string.dialog_image_options)) },
            text = {
                Text(
                    text = pendingImageUrl ?: "",
                    style = MaterialTheme.typography.bodySmall,
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis,
                )
            },
            confirmButton = {
                TextButton(onClick = { showSavePicker = true }) {
                    Text(stringResource(R.string.action_save_image))
                }
            },
            dismissButton = {
                TextButton(onClick = { pendingImageUrl = null }) {
                    Text(stringResource(R.string.action_cancel))
                }
            }
        )
    }

    // Step 2 — directory picker
    if (showSavePicker && pendingImageUrl != null) {
        val imageUrl = pendingImageUrl!!
        SaveDirectoryDialog(
            saveViewModel = saveViewModel,
            onSave = { targetDir ->
                showSavePicker = false
                pendingImageUrl = null
                scope.launch {
                    val ok = saveViewModel.saveImageFromUrl(imageUrl, targetDir)
                    snackbarHostState.showSnackbar(if (ok) strSaved else strFailed)
                }
            },
            onDismiss = {
                showSavePicker = false
                pendingImageUrl = null
            }
        )
    }
}

@Composable
private fun SaveDirectoryDialog(
    saveViewModel: ImageSaveViewModel,
    onSave: (File) -> Unit,
    onDismiss: () -> Unit,
) {
    val rootDir = saveViewModel.rootDir

    var treeNodes by remember {
        val hasChildren = rootDir.listFiles()?.any { it.isDirectory } == true
        mutableStateOf(listOf(DirNode(rootDir, depth = 0, expanded = false, hasChildren = hasChildren)))
    }
    var selectedDir by remember { mutableStateOf<File?>(rootDir) }
    var newDirName by remember { mutableStateOf("") }
    var createDirError by remember { mutableStateOf(false) }

    fun toggleNode(index: Int) {
        val node = treeNodes[index]
        val list = treeNodes.toMutableList()
        if (node.expanded) {
            list[index] = node.copy(expanded = false)
            var i = index + 1
            while (i < list.size && list[i].depth > node.depth) list.removeAt(i)
        } else {
            list[index] = node.copy(expanded = true)
            val children = node.dir.listFiles()
                ?.filter { it.isDirectory }
                ?.sortedBy { it.name.lowercase() }
                ?.map { child ->
                    DirNode(child, node.depth + 1, expanded = false,
                        hasChildren = child.listFiles()?.any { it.isDirectory } == true)
                } ?: emptyList()
            list.addAll(index + 1, children)
        }
        treeNodes = list
    }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(stringResource(R.string.dialog_save_directory)) },
        text = {
            Column {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(max = 280.dp)
                ) {
                    itemsIndexed(treeNodes) { index, node ->
                        val isSelected = node.dir == selectedDir
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(start = (node.depth * 20).dp)
                                .selectable(
                                    selected = isSelected,
                                    role = Role.RadioButton,
                                    onClick = { selectedDir = node.dir }
                                )
                                .padding(vertical = 4.dp, horizontal = 4.dp)
                        ) {
                            if (node.hasChildren) {
                                IconButton(
                                    onClick = { toggleNode(index) },
                                    modifier = Modifier.size(24.dp)
                                ) {
                                    Icon(
                                        imageVector = Icons.Default.ArrowDropDown,
                                        contentDescription = null,
                                        modifier = Modifier.rotate(if (node.expanded) 0f else -90f)
                                    )
                                }
                            } else {
                                Spacer(Modifier.size(24.dp))
                            }
                            Spacer(Modifier.width(4.dp))
                            RadioButton(selected = isSelected, onClick = null)
                            Spacer(Modifier.width(8.dp))
                            val label = if (node.dir.absolutePath == rootDir.absolutePath)
                                stringResource(R.string.label_internal_storage)
                            else node.dir.name
                            Text(
                                text = label,
                                style = MaterialTheme.typography.bodyMedium,
                            )
                        }
                    }
                }

                HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp))

                // Create new subdirectory under the currently selected directory
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    OutlinedTextField(
                        value = newDirName,
                        onValueChange = { newDirName = it; createDirError = false },
                        label = { Text(stringResource(R.string.hint_folder_name)) },
                        singleLine = true,
                        isError = createDirError,
                        modifier = Modifier.weight(1f)
                    )
                    Spacer(Modifier.width(8.dp))
                    TextButton(
                        onClick = {
                            val parent = selectedDir ?: rootDir
                            val created = saveViewModel.createDirectory(parent, newDirName.trim())
                            if (created != null) {
                                // Collapse + re-expand parent to show fresh filesystem state
                                val parentIdx = treeNodes.indexOfFirst { it.dir == parent }
                                val list = treeNodes.toMutableList()
                                if (parentIdx >= 0) {
                                    val parentNode = list[parentIdx]
                                    list[parentIdx] = parentNode.copy(hasChildren = true, expanded = false)
                                    var i = parentIdx + 1
                                    while (i < list.size && list[i].depth > parentNode.depth) list.removeAt(i)
                                    list[parentIdx] = list[parentIdx].copy(expanded = true)
                                    val fresh = parent.listFiles()
                                        ?.filter { it.isDirectory }
                                        ?.sortedBy { it.name.lowercase() }
                                        ?.map { child ->
                                            DirNode(child, parentNode.depth + 1, expanded = false,
                                                hasChildren = child.listFiles()?.any { it.isDirectory } == true)
                                        } ?: emptyList()
                                    list.addAll(parentIdx + 1, fresh)
                                }
                                treeNodes = list
                                selectedDir = created
                                newDirName = ""
                            } else {
                                createDirError = true
                            }
                        },
                        enabled = newDirName.isNotBlank()
                    ) {
                        Text(stringResource(R.string.action_create))
                    }
                }
            }
        },
        confirmButton = {
            TextButton(
                onClick = { selectedDir?.let { onSave(it) } },
                enabled = selectedDir != null
            ) {
                Text(stringResource(R.string.action_save_image))
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text(stringResource(R.string.action_cancel))
            }
        }
    )
}
