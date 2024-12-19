
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

if (MSVC)
    set(MTAPI_EXPORT "__declspec (dllexport)")
endif ()

configure_file("build.h.in" "${PROJECT_BINARY_DIR}/build.h")

file(GLOB_RECURSE QUARK_SOURCES
        business/*.cpp
        models/*.cpp
        services/*.cpp
        types/*.cpp
        utils/*.cpp
        media/*.cpp)

file(GLOB_RECURSE QUARK_HEADERS
        business/*.h
        models/*.h
        services/*.h
        types/*.h
        utils/*.h
        media/*.h)

if (APPLE)
    file(GLOB_RECURSE MODULEMAP_SOURCES quark.modulemap)
    list(APPEND QUARK_SOURCES ${MODULEMAP_SOURCES})
endif ()
set(GLOBAL_QUARK_SOURCES ${QUARK_SOURCES} CACHE INTERNAL "GLOBAL_QUARK_SOURCES")

set(LIB_NAME MTQuark)

add_library(${LIB_NAME} SHARED ${LIB_SOURCES} ${GLOBAL_QUARK_SOURCES})

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${LIB_NAME} PRIVATE spdlog::spdlog)

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${LIB_NAME} PRIVATE spdlog::spdlog)

# 查找并引用Boost库
find_package(Boost COMPONENTS system thread chrono filesystem url REQUIRED)
target_include_directories(${LIB_NAME} PRIVATE ${Boost_INCLUDE_DIRS})
target_link_libraries(${LIB_NAME} PRIVATE ${Boost_LIBRARIES})

# 查找并引用日志库
find_package(spdlog CONFIG REQUIRED)
target_link_libraries(${LIB_NAME} PRIVATE spdlog::spdlog)

# 查找并引用Markdown解析库
find_package(cmark CONFIG REQUIRED)
target_link_libraries(${LIB_NAME} PRIVATE cmark::cmark)

# 查找并引用Sqlite解析库
find_package(unofficial-sqlite3 CONFIG REQUIRED)
target_link_libraries(${LIB_NAME} PRIVATE unofficial::sqlite3::sqlite3)

# 查找并引用yaml-cpp解析库
find_package(yaml-cpp CONFIG REQUIRED)
target_link_libraries(${LIB_NAME} PRIVATE yaml-cpp::yaml-cpp)
