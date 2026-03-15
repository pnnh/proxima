package xyz.huable.dawn.ui.compose.screens

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.MoreVert
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import xyz.huable.dawn.R
import xyz.huable.dawn.model.FileItem
import xyz.huable.dawn.ui.files.FileListViewModel
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

private val DATE_FORMAT = SimpleDateFormat("yyyy-MM-dd HH:mm", Locale.getDefault())

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FileListScreen(
    onNavigateToPreview: (String) -> Unit,
    viewModel: FileListViewModel = viewModel()
) {
    val files by viewModel.files.observeAsState(emptyList())
    val breadcrumbs by viewModel.breadcrumbs.observeAsState(emptyList())
    val currentDir by viewModel.currentDir.observeAsState()

    var showCreateFolderDialog by remember { mutableStateOf(false) }
    var showCreateFileDialog by remember { mutableStateOf(false) }
    var showOverflowMenu by remember { mutableStateOf(false) }

    BackHandler(enabled = currentDir != null && !viewModel.isAtRoot()) {
        viewModel.navigateUp()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(stringResource(R.string.app_name)) },
                actions = {
                    Box {
                        IconButton(onClick = { showOverflowMenu = true }) {
                            Icon(Icons.Default.MoreVert, contentDescription = null)
                        }
                        DropdownMenu(
                            expanded = showOverflowMenu,
                            onDismissRequest = { showOverflowMenu = false }
                        ) {
                            DropdownMenuItem(
                                text = { Text(stringResource(R.string.action_new_folder)) },
                                leadingIcon = {
                                    Icon(
                                        painter = painterResource(R.drawable.ic_create_folder),
                                        contentDescription = null
                                    )
                                },
                                onClick = {
                                    showOverflowMenu = false
                                    showCreateFolderDialog = true
                                }
                            )
                            DropdownMenuItem(
                                text = { Text(stringResource(R.string.action_new_text_file)) },
                                leadingIcon = {
                                    Icon(
                                        painter = painterResource(R.drawable.ic_file_text),
                                        contentDescription = null
                                    )
                                },
                                onClick = {
                                    showOverflowMenu = false
                                    showCreateFileDialog = true
                                }
                            )
                        }
                    }
                }
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            // Breadcrumb row
            if (!breadcrumbs.isNullOrEmpty()) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .horizontalScroll(rememberScrollState(Int.MAX_VALUE))
                        .padding(horizontal = 8.dp, vertical = 4.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    breadcrumbs!!.forEachIndexed { index, crumb ->
                        val isLast = index == breadcrumbs!!.lastIndex
                        Text(
                            text = crumb.name,
                            style = MaterialTheme.typography.labelMedium,
                            fontWeight = if (isLast) FontWeight.Bold else FontWeight.Normal,
                            color = if (isLast)
                                MaterialTheme.colorScheme.onSurface
                            else
                                MaterialTheme.colorScheme.onSurfaceVariant,
                            modifier = Modifier
                                .padding(horizontal = 4.dp, vertical = 2.dp)
                                .then(
                                    if (!isLast) Modifier.clickable { viewModel.navigateTo(crumb.dir) }
                                    else Modifier
                                )
                        )
                        if (!isLast) {
                            Icon(
                                painter = painterResource(R.drawable.ic_chevron_right),
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.onSurfaceVariant,
                                modifier = Modifier.size(16.dp)
                            )
                        }
                    }
                }
                HorizontalDivider()
            }

            // File list or empty state
            if (files.isEmpty()) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = stringResource(R.string.empty_folder),
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            } else {
                LazyColumn(modifier = Modifier.fillMaxSize()) {
                    items(files, key = { it.path }) { item ->
                        FileItemRow(
                            item = item,
                            onClick = {
                                if (item.isDirectory) viewModel.navigateTo(File(item.path))
                                else onNavigateToPreview(item.path)
                            }
                        )
                        HorizontalDivider(modifier = Modifier.padding(start = 56.dp))
                    }
                }
            }
        }
    }

    if (showCreateFolderDialog) {
        NameInputDialog(
            title = stringResource(R.string.dialog_new_folder),
            hint = stringResource(R.string.hint_folder_name),
            onConfirm = { name ->
                if (name.isNotEmpty()) viewModel.createDirectory(name)
                showCreateFolderDialog = false
            },
            onDismiss = { showCreateFolderDialog = false }
        )
    }

    if (showCreateFileDialog) {
        NameInputDialog(
            title = stringResource(R.string.dialog_new_text_file),
            hint = stringResource(R.string.hint_file_name),
            onConfirm = { name ->
                if (name.isNotEmpty()) viewModel.createTextFile(name)
                showCreateFileDialog = false
            },
            onDismiss = { showCreateFileDialog = false }
        )
    }
}

@Composable
private fun FileItemRow(item: FileItem, onClick: () -> Unit) {
    val iconRes = when {
        item.isDirectory -> R.drawable.ic_folder
        item.extension in setOf("jpg", "jpeg", "png", "gif", "bmp", "webp") -> R.drawable.ic_file_image
        item.extension in setOf("txt", "md", "log", "csv", "xml", "json") -> R.drawable.ic_file_text
        else -> R.drawable.ic_file_binary
    }
    val metaText = if (item.isDirectory) {
        "${item.size} 项"
    } else {
        "${formatSize(item.size)} · ${DATE_FORMAT.format(Date(item.lastModified))}"
    }

    ListItem(
        headlineContent = { Text(item.name) },
        supportingContent = {
            Text(
                text = metaText,
                style = MaterialTheme.typography.bodySmall
            )
        },
        leadingContent = {
            Icon(
                painter = painterResource(iconRes),
                contentDescription = null,
                tint = if (item.isDirectory)
                    MaterialTheme.colorScheme.primary
                else
                    MaterialTheme.colorScheme.onSurfaceVariant,
                modifier = Modifier.size(24.dp)
            )
        },
        modifier = Modifier.clickable(onClick = onClick)
    )
}

@Composable
private fun NameInputDialog(
    title: String,
    hint: String,
    onConfirm: (String) -> Unit,
    onDismiss: () -> Unit
) {
    var text by remember { mutableStateOf("") }
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(title) },
        text = {
            OutlinedTextField(
                value = text,
                onValueChange = { text = it },
                placeholder = { Text(hint) },
                singleLine = true,
                modifier = Modifier.fillMaxWidth()
            )
        },
        confirmButton = {
            TextButton(onClick = { onConfirm(text.trim()) }) {
                Text(stringResource(R.string.action_create))
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text(stringResource(R.string.action_cancel))
            }
        }
    )
}

private fun formatSize(bytes: Long): String = when {
    bytes < 1024L -> "$bytes B"
    bytes < 1024L * 1024 -> "%.1f KB".format(bytes / 1024f)
    bytes < 1024L * 1024 * 1024 -> "%.1f MB".format(bytes / (1024f * 1024f))
    else -> "%.1f GB".format(bytes / (1024f * 1024f * 1024f))
}
