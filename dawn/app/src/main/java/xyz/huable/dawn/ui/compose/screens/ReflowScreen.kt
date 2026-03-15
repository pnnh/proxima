package xyz.huable.dawn.ui.compose.screens

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.lifecycle.viewmodel.compose.viewModel
import xyz.huable.dawn.R
import xyz.huable.dawn.ui.reflow.ReflowViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReflowScreen(viewModel: ReflowViewModel = viewModel()) {
    val text by viewModel.text.observeAsState("")

    Scaffold(
        topBar = {
            TopAppBar(title = { Text(stringResource(R.string.menu_reflow)) })
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = text,
                style = MaterialTheme.typography.bodyLarge
            )
        }
    }
}
