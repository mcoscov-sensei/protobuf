prefix=@CMAKE_INSTALL_PREFIX@
exec_prefix=@CMAKE_INSTALL_PREFIX@
libdir=@CMAKE_INSTALL_FULL_LIBDIR@
includedir=@CMAKE_INSTALL_FULL_INCLUDEDIR@

Name: Protocol Buffers
Description: Google's Data Interchange Format
Version: @protobuf_VERSION@
Requires: @_protobuf_PC_REQUIRES@
Requires.private: @_protobuf_PC_REQUIRES_PRIVATE@
Libs: -L${libdir} -lprotobuf-lite @CMAKE_THREAD_LIBS_INIT@
Cflags: -I${includedir}
Conflicts: protobuf
