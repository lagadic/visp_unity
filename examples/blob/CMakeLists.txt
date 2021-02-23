project(test-blob)

cmake_minimum_required(VERSION 3.0)

find_package(VISP REQUIRED visp_core visp_blob visp_sensor visp_gui visp_vision)

include_directories(${VISP_INCLUDE_DIRS})

add_executable(test-blob-opencv test-blob-opencv.cpp)
target_link_libraries(test-blob-opencv ${VISP_LIBRARIES})

add_executable(test-blob-v4l2 test-blob-v4l2.cpp)
target_link_libraries(test-blob-v4l2 ${VISP_LIBRARIES})

