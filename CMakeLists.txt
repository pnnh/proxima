cmake_minimum_required(VERSION 3.28...3.31)

project(Proxima VERSION 0.1.0 LANGUAGES C CXX)

if (APPLE)
    enable_language(Swift)
endif ()

# 设置语言标准
set(CMAKE_C_STANDARD 17)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
if (WIN32)
    # if (NOT CMAKE_SIZEOF_VOID_P)
    set(CMAKE_SIZEOF_VOID_P 8) # 不添加的话cmake会配置失败
    # endif ()
endif ()

# 设置开启显示详细的构建过程
set(CMAKE_VERBOSE_MAKEFILE ON)

# 启用测试
enable_testing()

include_directories(${CMAKE_CURRENT_SOURCE_DIR})
include_directories(${PROJECT_BINARY_DIR})

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

add_subdirectory(quark)

if (VCPKG_TARGET_TRIPLET STREQUAL "x64-windows")
    #    add_subdirectory(Gliese)
    #    add_subdirectory(proxima)
    #    add_subdirectory(pulsar)
elseif (VCPKG_TARGET_TRIPLET STREQUAL "arm64-osx")
    #    add_subdirectory(kepler)
#    add_subdirectory(proxima)
#    add_subdirectory(pulsar)
elseif (EMSCRIPTEN)
#    add_subdirectory(polaris)
elseif (LINUX)
#    add_subdirectory(proxima)
#    add_subdirectory(pulsar)
endif ()

set(APP_NAME MTProxima)

# 查找并引用Qt库
find_package(
        Qt6
        COMPONENTS Core
        Qml
        Quick
        Gui
        QuickControls2
        Sql
        Widgets
        Network
        Concurrent
        ShaderTools
        Sql
        REQUIRED)

qt_standard_project_setup()

set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)

qt_policy(SET QTP0001 NEW)
qt_policy(SET QTP0004 NEW)

file(GLOB_RECURSE APP_SOURCES
        services/*.cpp)

qt6_add_executable(${APP_NAME} MANUAL_FINALIZATION)
target_sources(${APP_NAME} PUBLIC main.cpp ${APP_SOURCES})

add_dependencies(${APP_NAME} MTQuark)
target_link_libraries(${APP_NAME} PRIVATE MTQuark)

# 查找并引用Boost库
find_package(Boost COMPONENTS system thread chrono filesystem url REQUIRED)
target_include_directories(${APP_NAME} PRIVATE ${Boost_INCLUDE_DIRS})
target_link_libraries(${APP_NAME} PRIVATE ${Boost_LIBRARIES})

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE spdlog::spdlog)

# 查找并引用Markdown解析库
find_package(cmark CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE cmark::cmark)

# 查找并引用Sqlite解析库
find_package(unofficial-sqlite3 CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE unofficial::sqlite3::sqlite3)

# 查找并引用yaml-cpp解析库
find_package(yaml-cpp CONFIG REQUIRED)
target_link_libraries(${APP_NAME} PRIVATE yaml-cpp::yaml-cpp)

target_link_libraries(
        ${APP_NAME}
        PRIVATE Qt6::Core
        Qt6::Qml
        Qt6::Quick
        Qt6::Gui
        Qt6::QuickControls2
        Qt6::Widgets
        Qt6::Sql)

include_directories(${CMAKE_CURRENT_SOURCE_DIR}/content/sources)

file(GLOB_RECURSE ASSETS_SOURCES
        RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
        content/*.png
        content/*.svg
        content/*.jpeg
        content/*.webp)

file(GLOB_RECURSE MODEL_SOURCES
        RELATIVE ${CMAKE_CURRENT_SOURCE_DIR}
        content/*.cpp)

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