add_executable(QMTests main.c cases/cases.c)
add_dependencies(QMTests MTQuantum)
target_link_libraries(QMTests PUBLIC MTQuantum)
add_custom_command(TARGET QMTests
        POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:MTQuantum> "${CMAKE_CURRENT_BINARY_DIR}/Debug"
        COMMENT "Copy dll file to ${CMAKE_CURRENT_BINARY_DIR} directory" VERBATIM
)

add_executable(QMXXTests main.cpp cases/cases.cpp)
add_dependencies(QMXXTests MTQuantum)
target_link_libraries(QMXXTests PUBLIC MTQuantum)
add_custom_command(TARGET QMXXTests
        POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:MTQuantum> "${CMAKE_CURRENT_BINARY_DIR}/Debug"
        COMMENT "Copy dll file to ${CMAKE_CURRENT_BINARY_DIR} directory" VERBATIM
)