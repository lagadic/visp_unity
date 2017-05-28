project(test)

cmake_minimum_required(VERSION 2.6)

find_package(VISP REQUIRED visp_core)

include_directories(${VISP_INCLUDE_DIRS})

add_library(visp-demo SHARED visp-demo.cpp)
target_link_libraries(visp-demo ${VISP_LIBRARIES})
