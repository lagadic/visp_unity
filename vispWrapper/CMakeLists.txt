#############################################################################
#
# ViSP, open source Visual Servoing Platform software.
# Copyright (C) 2005 - 2019 by Inria. All rights reserved.
#
# This software is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
# See the file LICENSE.txt at the root directory of this source
# distribution for additional information about the GNU GPL.
#
# For using ViSP with software that can not be combined with the GNU
# GPL, please contact Inria about acquiring a ViSP Professional
# Edition License.
#
# See http://visp.inria.fr for more information.
#
# This software was developed at:
# Inria Rennes - Bretagne Atlantique
# Campus Universitaire de Beaulieu
# 35042 Rennes Cedex
# France
#
# If you have questions regarding the use of this file, please contact
# Inria at visp@inria.fr
#
# This file is provided AS IS with NO WARRANTY OF ANY KIND, INCLUDING THE
# WARRANTY OF DESIGN, MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
#
# Description:
# Unity plugin that wraps some ViSP functionalities.
#
#############################################################################

project(ViSPUnity)

cmake_minimum_required(VERSION 3.0)

#----------------------------------------------------------------------
# Project version number.
#----------------------------------------------------------------------
set(PROJECT_VERSION_MAJOR 1)
set(PROJECT_VERSION_MINOR 0)
set(PROJECT_VERSION_PATCH 0)
set(PROJECT_VERSION "${PROJECT_VERSION_MAJOR}.${PROJECT_VERSION_MINOR}")
set(PROJECT_VERSION_FULL "${PROJECT_VERSION}.${PROJECT_VERSION_PATCH}")

# By default set release configuration
if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE "Release" CACHE STRING "Choose the type of build, options are: None Debug Release RelWithDebInfo MinSizeRel." FORCE)
endif()

if(APPLE)
  set(CMAKE_MACOSX_RPATH ON)
endif()

find_package(VISP REQUIRED visp_core visp_io visp_detection visp_mbt)

include_directories(${VISP_INCLUDE_DIRS})

add_library(ViSPUnity SHARED ViSPUnity.cpp ViSPUnity.h)
target_link_libraries(ViSPUnity ${VISP_LIBRARIES})

if(APPLE)
  set_target_properties(ViSPUnity PROPERTIES BUNDLE TRUE)
  set_target_properties(ViSPUnity PROPERTIES PREFIX "")
  set_target_properties(ViSPUnity PROPERTIES SUFFIX ".bundle")
elseif(UNIX)
  set_target_properties(ViSPUnity PROPERTIES PREFIX "")
endif()

# ----------------------------------------------------------------------------
#   Install target, for "make install"
# ----------------------------------------------------------------------------
install(FILES $<TARGET_FILE:ViSPUnity> DESTINATION ${PROJECT_SOURCE_DIR}/../unityProject/Assets)

# ----------------------------------------------------------------------------
#   Uninstall target, for "make uninstall"
# ----------------------------------------------------------------------------
configure_file(
  cmake/cmake_uninstall.cmake.in
  "${CMAKE_CURRENT_BINARY_DIR}/cmake_uninstall.cmake"
  IMMEDIATE @ONLY)

add_custom_target(uninstall
  "${CMAKE_COMMAND}" -P "${CMAKE_CURRENT_BINARY_DIR}/cmake_uninstall.cmake")

#----------------------------------------------------------------------
# add "doc" target for doxygen documentation building
#----------------------------------------------------------------------
find_package(Doxygen)
if (DOXYGEN_FOUND)
  if(DOXYGEN_DOT_EXECUTABLE)
    set(PROJECT_HAVE_DOT "yes")
  else()
    set(PROJECT_HAVE_DOT "no")
  endif()
  ## we need latex for doxygen because of the formulas
  find_package(LATEX)
  if(NOT LATEX_COMPILER)
    message(STATUS "latex command LATEX_COMPILER not found but usually required. You will probably get warnings and user interaction on doxy run.")
  endif ()
  if(NOT MAKEINDEX_COMPILER)
    message(STATUS "makeindex command MAKEINDEX_COMPILER not found but usually required.")
  endif ()
  if(NOT DVIPS_CONVERTER)
    message(STATUS "dvips command DVIPS_CONVERTER not found but usually required.")
  endif ()

  configure_file(${PROJECT_SOURCE_DIR}/doc/config-doxygen.cmake
    ${PROJECT_BINARY_DIR}/doc/config-doxygen
    @ONLY )

  add_custom_target(doc ${DOXYGEN_EXECUTABLE} ${PROJECT_BINARY_DIR}/doc/config-doxygen
    DEPENDS ${PROJECT_BINARY_DIR}/doc/config-doxygen)

endif()
