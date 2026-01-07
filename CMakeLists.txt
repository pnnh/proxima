cmake_minimum_required(VERSION 3.29...3.31)

project(Proxima VERSION 0.1.0 LANGUAGES C CXX)

if (APPLE)
    enable_language(OBJC OBJCXX Swift)
    set(CMAKE_Swift_LANGUAGE_VERSION 6.1)
    # 强制禁用已过时的 AGL 框架，防止其进入链接列表 (针对新版 Xcode SDK)
    set(OPENGL_agl_LIBRARY "OPENGL_agl_LIBRARY-NOTFOUND" CACHE STRING "" FORCE)
endif ()

# 设置语言标准
if (WIN32)
    set(CMAKE_C_STANDARD 23)
else ()
    set(CMAKE_C_STANDARD 23)
endif ()
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 设置开启显示详细的构建过程
set(CMAKE_VERBOSE_MAKEFILE ON)

# 启用测试
enable_testing()

include_directories(${CMAKE_CURRENT_SOURCE_DIR})
include_directories(${PROJECT_BINARY_DIR})
link_directories(${PROJECT_BINARY_DIR})

if (APPLE)
    set(CMAKE_MACOSX_RPATH 1)
    link_directories(${SWIFT_LIBRARY_SEARCH_PATHS})
    set(CMAKE_Swift_FLAGS "${CMAKE_Swift_FLAGS} -cxx-interoperability-mode=default")
    set(CMAKE_XCODE_ATTRIBUTE_XCODE_VERSION "26.0")

endif ()

# 避免警告
if (POLICY CMP0012)
    cmake_policy(SET CMP0012 NEW)
endif ()
# 针对CMake 3.30版本Boost查找相关警告
if (POLICY CMP0167)
    cmake_policy(SET CMP0167 NEW)
endif ()


#=====================PROXIMA APP CONFIG=========================#


set(PROXIMA_APP_NAME MTProxima)

# 查找并引用Qt库
find_package(
        Qt6
        COMPONENTS Core
        Qml
        Quick
        Gui
        QuickControls2
        Widgets
        ShaderTools
        QuickDialogs2
        REQUIRED)

set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)

qt_policy(SET QTP0001 NEW)
qt_policy(SET QTP0004 NEW)

qt_standard_project_setup()

file(GLOB_RECURSE PROXIMA_SOURCES
        proxima/main.cpp
        proxima/services/*.cpp
        proxima/tests/*.cpp
        quark/tests/*.c
        quark/tests/*.cpp
        quark/business/*.cpp
        quark/core/*.cpp
        quark/infra/*.cpp
        quark/services/*.cpp)

if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    file(GLOB_RECURSE DARWIN_SOURCES
            proxima/macos/*.cpp
            proxima/macos/*.mm
            proxima/macos/*.swift
            module.modulemap)
    list(APPEND PROXIMA_SOURCES ${DARWIN_SOURCES})

    # 添加xcode生成的Swift头文件路径
    include_directories(${CMAKE_CURRENT_BINARY_DIR}/${PROXIMA_APP_NAME}.build/Debug/DerivedSources)
elseif (WIN32)
    file(GLOB_RECURSE WINDOWS_SOURCES
            proxima/windows/*.cpp)
    list(APPEND PROXIMA_SOURCES ${WINDOWS_SOURCES})
endif ()

qt_add_executable(${PROXIMA_APP_NAME} MACOSX_BUNDLE WIN32 MANUAL_FINALIZATION)
target_sources(${PROXIMA_APP_NAME} PUBLIC ${PROXIMA_SOURCES})

if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    # 添加xcode生成的Swift头文件路径
    include_directories(${CMAKE_CURRENT_BINARY_DIR}/${PROXIMA_APP_NAME}.build/Debug/DerivedSources)
    set_target_properties(${PROXIMA_APP_NAME} PROPERTIES
            MACOSX_BUNDLE_BUNDLE_IDENTIFIER "xyz.huable.${PROXIMA_APP_NAME}"
            MACOSX_BUNDLE_BUNDLE_NAME ${PROXIMA_APP_NAME}
            MACOSX_BUNDLE_BUNDLE_VERSION "${PROJECT_VERSION}")
    set_target_properties(${PROXIMA_APP_NAME} PROPERTIES XCODE_ATTRIBUTE_LINK_WITH_STANDARD_LIBRARIES "YES")
    
elseif (WIN32)
    target_compile_definitions(${PROXIMA_APP_NAME} PUBLIC UNICODE _UNICODE)
    target_link_libraries(${PROXIMA_APP_NAME} PUBLIC dwmapi UxTheme)
endif ()

target_link_libraries(
        ${PROXIMA_APP_NAME}
        PRIVATE Qt6::Core
        Qt6::Qml
        Qt6::Quick
        Qt6::Gui
        Qt6::QuickControls2
        Qt6::Widgets
        Qt6::QuickDialogs2)

include_directories(${CMAKE_CURRENT_SOURCE_DIR}/content/sources)

file(GLOB_RECURSE ASSETS_SOURCES
        RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
        content/*.png
        content/*.svg
        content/*.jpeg
        content/*.webp)

file(GLOB_RECURSE MODEL_SOURCES
        RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
        content/sources/*.cpp)

if (APPLE)
    file(GLOB_RECURSE APPLE_MODEL_SOURCES
            RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
            content/macos/sources/*.cpp)
    target_include_directories(${PROXIMA_APP_NAME} PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/content/macos/sources)
    list(APPEND MODEL_SOURCES ${APPLE_MODEL_SOURCES})
endif ()

file(GLOB_RECURSE QUICK_SOURCES
        RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
        content/*.qml)

qt_add_qml_module(
        ${PROXIMA_APP_NAME}
        URI
        quick
        VERSION
        1.0
        RESOURCES
        ${ASSETS_SOURCES}
        SOURCES
        ${MODEL_SOURCES}
        QML_FILES
        ${QUICK_SOURCES})

qt_add_shaders(
        ${PROXIMA_APP_NAME}
        "shaders"
        BATCHABLE
        PRECOMPILE
        OPTIMIZED
        PREFIX
        "/"
        FILES
        "content/shaders/rounded.frag"
        "content/shaders/rounded.vert")

# 查找JSON库
find_package(nlohmann_json CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PRIVATE nlohmann_json::nlohmann_json)

# 查找并链接模板库
find_package(inja CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PRIVATE pantor::inja)

# 查找并引用env解析库
find_package(libenvpp CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PRIVATE libenvpp::libenvpp)

# 查找并引用日期库
find_package(date CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PRIVATE date::date-tz)

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PUBLIC spdlog::spdlog)

# 查找并引用Boost库
set(Boost_USE_STATIC_LIBS ON)
set(Boost_USE_MULTITHREADED ON)
find_package(Boost COMPONENTS system thread chrono filesystem url REQUIRED)
target_include_directories(${PROXIMA_APP_NAME} PUBLIC ${Boost_INCLUDE_DIRS})
target_link_libraries(${PROXIMA_APP_NAME} PUBLIC ${Boost_LIBRARIES})

# 查找并引用Markdown解析库
find_package(cmark CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PUBLIC cmark::cmark)

# 查找并引用Sqlite解析库
find_package(unofficial-sqlite3 CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PUBLIC unofficial::sqlite3::sqlite3)

# 查找并引用yaml-cpp解析库
find_package(yaml-cpp CONFIG REQUIRED)
target_link_libraries(${PROXIMA_APP_NAME} PUBLIC yaml-cpp::yaml-cpp)

# 完成 Qt 目标配置
qt_finalize_target(${PROXIMA_APP_NAME})


#=====================PULSAR APP CONFIG=========================#

set(PULSAR_APP_NAME MTPulsar)

add_executable(${PULSAR_APP_NAME})

file(GLOB_RECURSE APP_SOURCES pulsar/*.cpp
        quark/core/*.cpp
        quark/infra/*.cpp
        quark/services/*.cpp)

target_sources(${PULSAR_APP_NAME} PUBLIC ${APP_SOURCES})

if (NOT APPLE)

    file(GLOB_RECURSE APP_MODULES pulsar/*.cxx quark/*.cxx)

    target_sources(${PULSAR_APP_NAME}
            PUBLIC
            FILE_SET CXX_MODULES FILES
            ${APP_MODULES}
    )
endif ()


# 查找并引用Boost库
set(Boost_NO_BOOST_CMAKE ON)
set(Boost_USE_STATIC_LIBS ON)
set(Boost_USE_MULTITHREADED ON)
find_package(Boost COMPONENTS system thread chrono filesystem url REQUIRED)
target_include_directories(${PULSAR_APP_NAME} PRIVATE ${Boost_INCLUDE_DIRS})
target_link_libraries(${PULSAR_APP_NAME} PRIVATE ${Boost_LIBRARIES})

# 查找Workflow库，实现HTTP服务
set(WORKFLOW_BUILD_STATIC_RUNTIME ON)
find_library(workflow_lib workflow)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE ${workflow_lib})

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE spdlog::spdlog spdlog::spdlog_header_only)

# 查找JSON库
find_package(nlohmann_json CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE nlohmann_json::nlohmann_json)

# 查找并链接模板库
find_package(inja CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE pantor::inja)

# 查找并引用env解析库
find_package(libenvpp CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE libenvpp::libenvpp)

# 查找并引用日期库
find_package(date CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE date::date-tz)

# 查找并引用Sqlite解析库
find_package(unofficial-sqlite3 CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE unofficial::sqlite3::sqlite3)

# 查找并引用yaml-cpp解析库
find_package(yaml-cpp CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE yaml-cpp::yaml-cpp)

# 查找并引用Markdown解析库
find_package(cmark CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE cmark::cmark)

# 查找并引用PostgreSQL解析库
find_package(libpqxx CONFIG REQUIRED)
target_link_libraries(${PULSAR_APP_NAME} PRIVATE libpqxx::pqxx)

# 拷贝资源文件到二进制目录，方便调试
file(COPY assets DESTINATION ${CMAKE_CURRENT_BINARY_DIR})

# 配置安装内容
install(TARGETS ${PULSAR_APP_NAME}
        RUNTIME_DEPENDENCY_SET ${PULSAR_APP_NAME}-dependencies
        DESTINATION lib)
install(RUNTIME_DEPENDENCY_SET ${PULSAR_APP_NAME}-dependencies)