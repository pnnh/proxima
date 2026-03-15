package xyz.huable.dawn.ui.compose

import android.net.Uri
import androidx.annotation.DrawableRes
import androidx.annotation.StringRes
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.material3.adaptive.navigationsuite.NavigationSuiteScaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import xyz.huable.dawn.R
import androidx.compose.animation.EnterTransition
import androidx.compose.animation.ExitTransition
import xyz.huable.dawn.ui.compose.screens.FileListScreen
import xyz.huable.dawn.ui.compose.screens.FilePreviewScreen
import xyz.huable.dawn.ui.compose.screens.WebViewScreen
import xyz.huable.dawn.ui.compose.screens.ReflowScreen
import xyz.huable.dawn.ui.compose.screens.SettingsScreen
import xyz.huable.dawn.ui.compose.screens.SlideshowScreen
import xyz.huable.dawn.ui.compose.theme.DawnTheme

private enum class TopLevelDestination(
    val route: String,
    @StringRes val labelRes: Int,
    @DrawableRes val iconRes: Int
) {
    Files("files", R.string.menu_files, R.drawable.ic_folder),
    Reflow("reflow", R.string.menu_reflow, R.drawable.ic_gallery_black_24dp),
    Slideshow("slideshow", R.string.menu_slideshow, R.drawable.ic_slideshow_black_24dp),
    Settings("settings", R.string.menu_settings, R.drawable.ic_settings_black_24dp),
}

private const val FILE_PREVIEW_ROUTE = "files/preview/{path}"
private const val WEB_VIEW_ROUTE = "files/webview/{url}"

@Composable
fun DawnApp() {
    DawnTheme {
        val navController = rememberNavController()
        val navBackStackEntry by navController.currentBackStackEntryAsState()
        val currentRoute = navBackStackEntry?.destination?.route

        // Determine which top-level tab is currently active
        val selectedDestination = TopLevelDestination.entries.find { dest ->
            currentRoute == dest.route || currentRoute?.startsWith("${dest.route}/") == true
        } ?: TopLevelDestination.Files

        NavigationSuiteScaffold(
            navigationSuiteItems = {
                TopLevelDestination.entries.forEach { dest ->
                    item(
                        icon = {
                            Icon(
                                painter = painterResource(dest.iconRes),
                                contentDescription = stringResource(dest.labelRes)
                            )
                        },
                        label = { Text(stringResource(dest.labelRes)) },
                        selected = selectedDestination == dest,
                        onClick = {
                            navController.navigate(dest.route) {
                                // Pop up to the start destination so the back stack
                                // doesn't grow unboundedly across tab switches
                                popUpTo(navController.graph.findStartDestination().id) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        }
                    )
                }
            }
        ) {
            NavHost(
                navController = navController,
                startDestination = TopLevelDestination.Files.route
            ) {
                composable(TopLevelDestination.Files.route) {
                    FileListScreen(
                        onNavigateToPreview = { path ->
                            navController.navigate("files/preview/${Uri.encode(path)}")
                        },
                        onNavigateToUrl = { url ->
                            navController.navigate("files/webview/${Uri.encode(url)}")
                        }
                    )
                }

                composable(
                    route = FILE_PREVIEW_ROUTE,
                    arguments = listOf(navArgument("path") { type = NavType.StringType })
                ) { backStackEntry ->
                    val encodedPath = backStackEntry.arguments?.getString("path").orEmpty()
                    FilePreviewScreen(
                        filePath = Uri.decode(encodedPath),
                        onNavigateUp = { navController.popBackStack() }
                    )
                }

                composable(TopLevelDestination.Reflow.route) { ReflowScreen() }
                composable(TopLevelDestination.Slideshow.route) { SlideshowScreen() }
                composable(TopLevelDestination.Settings.route) { SettingsScreen() }

                composable(
                    route = WEB_VIEW_ROUTE,
                    arguments = listOf(navArgument("url") { type = NavType.StringType }),
                    enterTransition = { EnterTransition.None },
                    exitTransition = { ExitTransition.None },
                    popEnterTransition = { EnterTransition.None },
                    popExitTransition = { ExitTransition.None },
                ) { backStackEntry ->
                    val encodedUrl = backStackEntry.arguments?.getString("url").orEmpty()
                    WebViewScreen(
                        url = Uri.decode(encodedUrl),
                        onNavigateUp = { navController.popBackStack() }
                    )
                }
            }
        }
    }
}
