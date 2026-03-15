package xyz.huable.dawn.ui.compose.theme

import android.app.Activity
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.dynamicDarkColorScheme
import androidx.compose.material3.dynamicLightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

/**
 * Dawn app theme.
 *
 * Changes from original baseline:
 *  - Dynamic color always enabled (minSdk=36, no SDK guard needed).
 *  - Static fallback upgraded to full M3 tonal palette (see Color.kt).
 *  - Expressive typography scale (see Type.kt).
 *  - Removed deprecated window.statusBarColor (redundant with enableEdgeToEdge()).
 *
 * Note: MotionScheme.expressive() and material3-expressive are still @InternalMaterial3Api
 * in the current stable BOM. Re-enable once they are promoted to public API.
 */
@Composable
fun DawnTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    dynamicColor: Boolean = true,
    content: @Composable () -> Unit
) {
    // minSdk = 36 (Android 16): dynamic color always available — no SDK guard needed.
    val context = LocalContext.current
    val colorScheme = if (dynamicColor) {
        if (darkTheme) dynamicDarkColorScheme(context) else dynamicLightColorScheme(context)
    } else {
        if (darkTheme) DarkColorScheme else LightColorScheme
    }

    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            // window.statusBarColor is deprecated (API 35+) and a no-op on API 36 when
            // enableEdgeToEdge() is called in the Activity — so it is intentionally omitted.
            WindowCompat.getInsetsController(window, view).isAppearanceLightStatusBars = !darkTheme
        }
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = DawnTypography,
        content = content
    )
}

