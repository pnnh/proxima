package xyz.huable.dawn.ui.files

import android.graphics.BitmapFactory
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import androidx.lifecycle.lifecycleScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import xyz.huable.dawn.R
import xyz.huable.dawn.databinding.FragmentFilePreviewBinding
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

class FilePreviewFragment : Fragment() {

    private var _binding: FragmentFilePreviewBinding? = null
    private val binding get() = _binding!!

    private val dateFmt = SimpleDateFormat("yyyy-MM-dd HH:mm", Locale.getDefault())

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View {
        _binding = FragmentFilePreviewBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        val filePath = arguments?.getString("filePath") ?: return
        val file = File(filePath)

        // Set toolbar title to the file name
        (activity as? AppCompatActivity)?.supportActionBar?.title = file.name

        val ext = file.extension.lowercase()
        when {
            ext in IMAGE_EXTENSIONS -> showImagePreview(file)
            ext in TEXT_EXTENSIONS  -> showTextPreview(file)
            else                    -> showFileInfo(file)
        }
    }

    // ---- Image ----

    private fun showImagePreview(file: File) {
        binding.previewLoading.visibility = View.VISIBLE
        viewLifecycleOwner.lifecycleScope.launch {
            val bitmap = withContext(Dispatchers.IO) {
                runCatching { BitmapFactory.decodeFile(file.absolutePath) }.getOrNull()
            }
            binding.previewLoading.visibility = View.GONE
            if (bitmap != null) {
                binding.previewImage.setImageBitmap(bitmap)
                binding.previewImage.visibility = View.VISIBLE
            } else {
                // Fall back to info view if image decoding fails
                showFileInfo(file)
            }
        }
    }

    // ---- Text ----

    private fun showTextPreview(file: File) {
        binding.previewLoading.visibility = View.VISIBLE
        viewLifecycleOwner.lifecycleScope.launch {
            val content = withContext(Dispatchers.IO) {
                runCatching { file.readText(Charsets.UTF_8) }.getOrElse { "（无法读取文件内容）" }
            }
            binding.previewLoading.visibility = View.GONE
            binding.previewText.text = content
            binding.previewTextScroll.visibility = View.VISIBLE
        }
    }

    // ---- Binary / unknown ----

    private fun showFileInfo(file: File) {
        val ext = file.extension.lowercase()
        val iconRes = when {
            ext in IMAGE_EXTENSIONS -> R.drawable.ic_file_image
            ext in TEXT_EXTENSIONS  -> R.drawable.ic_file_text
            else                    -> R.drawable.ic_file_binary
        }
        binding.previewIcon.setImageResource(iconRes)
        binding.previewFileName.text = file.name
        binding.previewFileSize.text = formatSize(file.length())
        binding.previewFileDate.text = dateFmt.format(Date(file.lastModified()))
        binding.previewInfo.visibility = View.VISIBLE
    }

    // ---- Helpers ----

    private fun formatSize(bytes: Long): String = when {
        bytes < 1024            -> "$bytes B"
        bytes < 1024 * 1024     -> "%.1f KB".format(bytes / 1024f)
        bytes < 1024 * 1024 * 1024 -> "%.1f MB".format(bytes / (1024f * 1024f))
        else                    -> "%.1f GB".format(bytes / (1024f * 1024f * 1024f))
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }

    companion object {
        private val IMAGE_EXTENSIONS = setOf("jpg", "jpeg", "png", "gif", "bmp", "webp")
        private val TEXT_EXTENSIONS  = setOf("txt", "md", "log", "csv", "xml", "json", "html", "htm", "kt", "java", "py")
    }
}

