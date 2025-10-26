cmake_minimum_required(VERSION 3.29...3.31)

project(Proxima VERSION 0.1.0 LANGUAGES C CXX)

if (APPLE)
    enable_language(OBJC OBJCXX Swift)
    set(CMAKE_Swift_LANGUAGE_VERSION 6.1)
endif ()

# 设置语言标准
if (WIN32)
    set(CMAKE_C_STANDARD 23)    # Windows 10 1809 以上版本支持C23
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
    set(CMAKE_XCODE_ATTRIBUTE_XCODE_VERSION "16.0")
endif ()

# 避免警告
if (POLICY CMP0012)
    cmake_policy(SET CMP0012 NEW)
endif ()
# 针对CMake 3.30版本Boost查找相关警告
if (POLICY CMP0167)
    cmake_policy(SET CMP0167 NEW)
endif ()
set(Boost_USE_STATIC_LIBS ON)
set(Boost_USE_MULTITHREADED ON)

set(APP_NAME MTProxima)

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
    include_directories(${CMAKE_CURRENT_BINARY_DIR}/${APP_NAME}.build/Debug/DerivedSources)
elseif (WIN32)
    file(GLOB_RECURSE WINDOWS_SOURCES
            proxima/windows/*.cpp)
    list(APPEND PROXIMA_SOURCES ${WINDOWS_SOURCES})
endif ()

qt_add_executable(${APP_NAME} MACOSX_BUNDLE WIN32 MANUAL_FINALIZATION)
target_sources(${APP_NAME} PUBLIC ${PROXIMA_SOURCES})

if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    # 添加xcode生成的Swift头文件路径
    include_directories(${CMAKE_CURRENT_BINARY_DIR}/${APP_NAME}.build/Debug/DerivedSources)
    set_target_properties(${APP_NAME} PROPERTIES
            MACOSX_BUNDLE_BUNDLE_IDENTIFIER "xyz.huable.${APP_NAME}"
            MACOSX_BUNDLE_BUNDLE_NAME ${APP_NAME}
            MACOSX_BUNDLE_BUNDLE_VERSION "${PROJECT_VERSION}")
elseif (WIN32)
    target_compile_definitions(${APP_NAME} PUBLIC UNICODE _UNICODE)
    target_link_libraries(${APP_NAME} PUBLIC dwmapi UxTheme)
endif ()

#file(GLOB_RECURSE PROXIMA_MODULES
#        proxima/*.cxx
#        proxima/*.ixx
#        quark/tests/*.cxx
#        quark/tests/*.ixx
#        quark/business/*.cxx
#        quark/business/*.ixx
#        quark/core/*.cxx
#        quark/core/*.ixx
#        quark/infra/*.cxx
#        quark/infra/*.ixx
#        quark/services/*.cxx
#        quark/services/*.ixx)
#
#target_sources(${APP_NAME}
#        PUBLIC
#        FILE_SET CXX_MODULES FILES
#        ${PROXIMA_MODULES}
#)

target_link_libraries(
        ${APP_NAME}
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
        content/macos/sources/*.cpp)
    list(APPEND ASSETS_SOURCES ${APPLE_MODEL_SOURCES})
endif ()

file(GLOB_RECURSE QUICK_SOURCES
        RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
        content/*.qml)

qt_add_qml_module(
        ${APP_NAME}
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
        ${APP_NAME}
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
target_link_libraries(${APP_NAME} PRIVATE nlohmann_json::nlohmann_json)

# 查找并链接模板库
find_package(inja CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE pantor::inja)

# 查找并引用env解析库
find_package(libenvpp CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE libenvpp::libenvpp)

# 查找并引用日期库
find_package(date CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE date::date-tz)

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PUBLIC spdlog::spdlog)

# 查找并引用Boost库
find_package(Boost COMPONENTS system thread chrono filesystem url REQUIRED)
target_include_directories(${APP_NAME} PUBLIC ${Boost_INCLUDE_DIRS})
target_link_libraries(${APP_NAME} PUBLIC ${Boost_LIBRARIES})

# 查找并引用Markdown解析库
find_package(cmark CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PUBLIC cmark::cmark)

# 查找并引用Sqlite解析库
find_package(unofficial-sqlite3 CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PUBLIC unofficial::sqlite3::sqlite3)

# 查找并引用yaml-cpp解析库
find_package(yaml-cpp CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PUBLIC yaml-cpp::yaml-cpp)

# 完成 Qt 目标配置
qt_finalize_target(${APP_NAME})