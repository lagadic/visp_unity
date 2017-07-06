project(test-blob)

cmake_minimum_required(VERSION 2.6)

find_package(VISP REQUIRED visp_core visp_blob visp_sensor visp_gui visp_vision)

include_directories(${VISP_INCLUDE_DIRS})

add_executable(test-blob test-blob.cpp)
target_link_libraries(test-blob ${VISP_LIBRARIES})

