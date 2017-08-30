project(VISP_WRAPPER)

cmake_minimum_required(VERSION 2.6)

find_package(VISP REQUIRED visp_core visp_blob visp_vision)

include_directories(${VISP_INCLUDE_DIRS})

add_library(visp-plugin-frame SHARED visp-plugin-frame.cpp)
target_link_libraries(visp-plugin-frame ${VISP_LIBRARIES})

add_library(visp-plugin-pose-estimation SHARED visp-plugin-pose-estimation.cpp)
target_link_libraries(visp-plugin-pose-estimation ${VISP_LIBRARIES})

set(VISP_WRAPPER_DOC_DIR "${VISP_WRAPPER_BINARY_DIR}/doc")


#----------------------------------------------------------------------
# Try to find doxygen for documentation generation
# Use "make visp_wrapper_doc" target to generate the documentation
#----------------------------------------------------------------------
find_package(Doxygen)
if(DOXYGEN_FOUND)
  ## we need latex for doxygen because of the formulas
  find_package(LATEX)
  if(NOT LATEX_COMPILER)
    message(STATUS "latex command LATEX_COMPILER not found but usually required. You will probably get warnings and user interaction on doxy run.")
  endif()
  if(NOT MAKEINDEX_COMPILER)
    message(STATUS "makeindex command MAKEINDEX_COMPILER not found but usually required.")
  endif()
  if(NOT DVIPS_CONVERTER)
    message(STATUS "dvips command DVIPS_CONVERTER not found but usually required.")
  endif()

  # set vars used in doxygen config file
  # - DOXYGEN_STRIP_FROM_INC_PATH corresponding to STRIP_FROM_INC_PATH in the doxy file
  set(DOXYGEN_STRIP_FROM_INC_PATH "")

  file(GLOB_RECURSE __path "${VISP_WRAPPER_SOURCE_DIR}")

  configure_file(${VISP_WRAPPER_SOURCE_DIR}/doc/config-doxygen.in
    ${VISP_WRAPPER_DOC_DIR}/config-doxygen
    @ONLY )

  # set vars used in mainpage.doc.in
  set(mainpage_ext_file_ "${VISP_WRAPPER_SOURCE_DIR}/doc/mainpage_extension.doc")
  if(EXISTS ${mainpage_ext_file_})
    file(READ ${mainpage_ext_file_} mainpage_ext_content_)
    set(VISP_MAINPAGE_EXTENSION "${VISP_MAINPAGE_EXTENSION}\n${mainpage_ext_content_}")
  endif()

  configure_file(${VISP_WRAPPER_SOURCE_DIR}/doc/mainpage.doc.in
    ${VISP_WRAPPER_DOC_DIR}/mainpage.doc
    @ONLY )
endif()

# ----------------------------------------------------------------------------
#   Doxygen documentation target, for "make visp_wrapper_doc"
# ----------------------------------------------------------------------------
if(DOXYGEN_FOUND)
  add_custom_target(visp_wrapper_doc ${DOXYGEN_EXECUTABLE} ${VISP_WRAPPER_DOC_DIR}/config-doxygen)
endif()
