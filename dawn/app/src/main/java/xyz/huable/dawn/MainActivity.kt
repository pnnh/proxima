package xyz.huable.dawn

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import xyz.huable.dawn.ui.compose.DawnApp
import xyz.huable.dawn.util.SampleFilesInitializer

class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        SampleFilesInitializer.initIfNeeded(this)
        setContent {
            DawnApp()
        }
    }
}
