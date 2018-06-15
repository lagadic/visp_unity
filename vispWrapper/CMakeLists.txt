project(ViSPUnity)

cmake_minimum_required(VERSION 2.6)

find_package(VISP REQUIRED visp_core visp_io visp_detection)

include_directories(${VISP_INCLUDE_DIRS})

add_library(ViSPUnity SHARED ViSPUnity.cpp)
target_link_libraries(ViSPUnity ${VISP_LIBRARIES})
