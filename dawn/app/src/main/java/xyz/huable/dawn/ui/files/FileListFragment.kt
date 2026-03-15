package xyz.huable.dawn.ui.files

import android.os.Bundle
import android.view.LayoutInflater
import android.view.Menu
import android.view.MenuInflater
import android.view.MenuItem
import android.view.View
import android.view.ViewGroup
import android.widget.EditText
import android.widget.ImageView
import android.widget.TextView
import android.widget.Toast
import androidx.activity.OnBackPressedCallback
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.MenuProvider
import androidx.fragment.app.Fragment
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.ViewModelProvider
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import xyz.huable.dawn.R
import xyz.huable.dawn.databinding.FragmentFileListBinding
import xyz.huable.dawn.databinding.ItemFileBinding
import xyz.huable.dawn.model.FileItem
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

class FileListFragment : Fragment() {

    private var _binding: FragmentFileListBinding? = null
    private val binding get() = _binding!!

    private lateinit var viewModel: FileListViewModel
    private lateinit var adapter: FileItemAdapter

    private val backCallback = object : OnBackPressedCallback(false) {
        override fun handleOnBackPressed() {
            if (!viewModel.navigateUp()) {
                isEnabled = false
                requireActivity().onBackPressedDispatcher.onBackPressed()
            }
        }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View {
        viewModel = ViewModelProvider(this)[FileListViewModel::class.java]
        _binding = FragmentFileListBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        adapter = FileItemAdapter { item ->
            if (item.isDirectory) {
                viewModel.navigateTo(File(item.path))
            }
        }
        binding.recyclerviewFileList.layoutManager = LinearLayoutManager(requireContext())
        binding.recyclerviewFileList.adapter = adapter

        viewModel.files.observe(viewLifecycleOwner) { files ->
            adapter.submitList(files)
            binding.emptyView.visibility = if (files.isEmpty()) View.VISIBLE else View.GONE
        }

        viewModel.currentDir.observe(viewLifecycleOwner) {
            backCallback.isEnabled = !viewModel.isAtRoot()
        }

        viewModel.title.observe(viewLifecycleOwner) { t ->
            (activity as? AppCompatActivity)?.supportActionBar?.title = t
        }

        requireActivity().onBackPressedDispatcher.addCallback(viewLifecycleOwner, backCallback)

        // Add create folder / create text file to toolbar overflow menu
        requireActivity().addMenuProvider(object : MenuProvider {
            override fun onCreateMenu(menu: Menu, menuInflater: MenuInflater) {
                menuInflater.inflate(R.menu.menu_file_actions, menu)
            }

            override fun onMenuItemSelected(menuItem: MenuItem): Boolean {
                return when (menuItem.itemId) {
                    R.id.action_create_folder -> {
                        showCreateFolderDialog(); true
                    }
                    R.id.action_create_text_file -> {
                        showCreateTextFileDialog(); true
                    }
                    else -> false
                }
            }
        }, viewLifecycleOwner, Lifecycle.State.RESUMED)
    }

    private fun showCreateFolderDialog() {
        val input = EditText(requireContext()).apply {
            hint = getString(R.string.hint_folder_name)
            setPadding(48, 24, 48, 8)
        }
        MaterialAlertDialogBuilder(requireContext())
            .setTitle(R.string.dialog_new_folder)
            .setView(input)
            .setPositiveButton(R.string.action_create) { _, _ ->
                val name = input.text.toString().trim()
                if (name.isNotEmpty()) {
                    if (!viewModel.createDirectory(name)) {
                        Toast.makeText(context, R.string.error_create_folder, Toast.LENGTH_SHORT).show()
                    }
                }
            }
            .setNegativeButton(R.string.action_cancel, null)
            .show()
    }

    private fun showCreateTextFileDialog() {
        val input = EditText(requireContext()).apply {
            hint = getString(R.string.hint_file_name)
            setPadding(48, 24, 48, 8)
        }
        MaterialAlertDialogBuilder(requireContext())
            .setTitle(R.string.dialog_new_text_file)
            .setView(input)
            .setPositiveButton(R.string.action_create) { _, _ ->
                val name = input.text.toString().trim()
                if (name.isNotEmpty()) {
                    if (!viewModel.createTextFile(name)) {
                        Toast.makeText(context, R.string.error_create_file, Toast.LENGTH_SHORT).show()
                    }
                }
            }
            .setNegativeButton(R.string.action_cancel, null)
            .show()
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }

    // ---- Adapter ----

    class FileItemAdapter(
        private val onClick: (FileItem) -> Unit
    ) : ListAdapter<FileItem, FileItemAdapter.ViewHolder>(DIFF) {

        override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
            val binding = ItemFileBinding.inflate(LayoutInflater.from(parent.context), parent, false)
            return ViewHolder(binding)
        }

        override fun onBindViewHolder(holder: ViewHolder, position: Int) {
            holder.bind(getItem(position), onClick)
        }

        class ViewHolder(private val binding: ItemFileBinding) :
            RecyclerView.ViewHolder(binding.root) {

            private val dateFmt = SimpleDateFormat("yyyy-MM-dd HH:mm", Locale.getDefault())

            fun bind(item: FileItem, onClick: (FileItem) -> Unit) {
                binding.fileName.text = item.name

                // Icon
                val iconRes = when {
                    item.isDirectory -> R.drawable.ic_folder
                    item.extension in listOf("jpg", "jpeg", "png", "gif", "bmp", "webp") ->
                        R.drawable.ic_file_image
                    item.extension == "txt" || item.extension == "md" || item.extension == "log" ->
                        R.drawable.ic_file_text
                    else -> R.drawable.ic_file_binary
                }
                binding.fileIcon.setImageResource(iconRes)

                // Secondary info
                binding.fileInfo.text = if (item.isDirectory) {
                    val cnt = item.size
                    if (cnt == 0L) "空文件夹" else "$cnt 项"
                } else {
                    val sizeStr = formatSize(item.size)
                    val dateStr = dateFmt.format(Date(item.lastModified))
                    "$sizeStr  ·  $dateStr"
                }

                // Chevron only for directories
                binding.fileChevron.visibility =
                    if (item.isDirectory) View.VISIBLE else View.INVISIBLE

                binding.root.setOnClickListener { onClick(item) }
            }

            private fun formatSize(bytes: Long): String = when {
                bytes < 1024 -> "$bytes B"
                bytes < 1024 * 1024 -> "%.1f KB".format(bytes / 1024f)
                bytes < 1024 * 1024 * 1024 -> "%.1f MB".format(bytes / (1024f * 1024f))
                else -> "%.1f GB".format(bytes / (1024f * 1024f * 1024f))
            }
        }

        companion object {
            val DIFF = object : DiffUtil.ItemCallback<FileItem>() {
                override fun areItemsTheSame(a: FileItem, b: FileItem) = a.path == b.path
                override fun areContentsTheSame(a: FileItem, b: FileItem) = a == b
            }
        }
    }
}

