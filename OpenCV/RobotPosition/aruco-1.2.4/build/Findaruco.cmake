# ===================================================================================
#  aruco CMake configuration file
#
#             ** File generated automatically, do not modify **
#
#  Usage from an external project:
#    In your CMakeLists.txt, add these lines:
#
#    FIND_PACKAGE(aruco REQUIRED )
#    TARGET_LINK_LIBRARIES(MY_TARGET_NAME )
#
#    This file will define the following variables:
#      - aruco_LIBS          : The list of libraries to links against.
#      - aruco_LIB_DIR       : The directory where lib files are. Calling LINK_DIRECTORIES
#                                with this path is NOT needed.
#      - aruco_VERSION       : The  version of this PROJECT_NAME build. Example: "1.2.0"
#      - aruco_VERSION_MAJOR : Major version part of VERSION. Example: "1"
#      - aruco_VERSION_MINOR : Minor version part of VERSION. Example: "2"
#      - aruco_VERSION_PATCH : Patch version part of VERSION. Example: "0"
#
# ===================================================================================
INCLUDE_DIRECTORIES()
SET(aruco_INCLUDE_DIRS )

LINK_DIRECTORIES("C:/Users/Brian/Downloads/aruco-1.2.4/aruco-1.2.4/aruco-1.2.4/build/lib")
#SET(aruco_LIB_DIR "")

SET(aruco_LIBS debug;opencv_core220d;optimized;opencv_core220;debug;opencv_imgproc220d;optimized;opencv_imgproc220;debug;opencv_features2d220d;optimized;opencv_features2d220;debug;opencv_gpu220d;optimized;opencv_gpu220;debug;opencv_calib3d220d;optimized;opencv_calib3d220;debug;opencv_objdetect220d;optimized;opencv_objdetect220;debug;opencv_video220d;optimized;opencv_video220;debug;opencv_highgui220d;optimized;opencv_highgui220;debug;opencv_ml220d;optimized;opencv_ml220;debug;opencv_legacy220d;optimized;opencv_legacy220;debug;opencv_contrib220d;optimized;opencv_contrib220;debug;opencv_flann220d;optimized;opencv_flann220 aruco124) 

SET(aruco_FOUND YES)
SET(aruco_FOUND "YES")
SET(aruco_VERSION        1.2.4)
SET(aruco_VERSION_MAJOR  1)
SET(aruco_VERSION_MINOR  2)
SET(aruco_VERSION_PATCH  4)
