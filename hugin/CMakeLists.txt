
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

include(InitializeSwift)
include(AddSwift)

_swift_generate_cxx_header_target(
        HuginHeader
        HuginLibrary
        "${CMAKE_CURRENT_BINARY_DIR}/MessagesImpl-swift.h"
        SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/MessagesImpl.swift"
        SEARCH_PATHS "${CMAKE_CURRENT_SOURCE_DIR}")

file(GLOB_RECURSE HUGIN_SOURCES *.swift)

add_library(HuginLibrary STATIC ${HUGIN_SOURCES})
add_dependencies(HuginLibrary HuginHeader)
#    target_include_directories(ProximaSwift PUBLIC "${CMAKE_CURRENT_BINARY_DIR}/include")
set_target_properties(HuginLibrary PROPERTIES Swift_MODULE_NAME "HuginLibrary")
target_compile_options(HuginLibrary PUBLIC "$<$<COMPILE_LANGUAGE:Swift>:-cxx-interoperability-mode=default>")