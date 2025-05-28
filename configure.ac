AC_INIT(homeip-sharp, 0.9.1)

AM_INIT_AUTOMAKE

AC_CANONICAL_HOST

dnl Check for pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR(['pkg-config' is not in your PATH.])
fi

dnl Check for Mono
PKG_CHECK_MODULES(MONO_DEPENDENCY, mono >= 1.0, has_mono=true, has_mono=false)
PKG_CHECK_MODULES(MONO_DEPENDENCY, mint >= 1.0, has_mint=true, has_mint=false)

if test "x$has_mono" = "xtrue"; then
	AC_PATH_PROG(RUNTIME, mono, no)
    AC_PATH_PROG(CSC, gmcs, no)
    LIB_PREFIX=.so
    LIB_SUFFIX=
else
    AC_MSG_ERROR([You need to install mono])
fi

CS="C#"
if test "x$CSC" = "xno" ; then
    AC_MSG_ERROR([No $CS compiler found])
fi
                                     
AC_SUBST(RUNTIME)
AC_SUBST(CSC)
AC_SUBST(LIB_PREFIX)
AC_SUBST(LIB_SUFFIX)

dnl Check for libhildondesktop-sharp
PKG_CHECK_MODULES(LIBHILDONDESKTOP_SHARP, libhildondesktop-sharp >= 0.0.1)
AC_SUBST(LIBHILDONDESKTOP_SHARP_LIBS)

dnl Check for glade-sharp
PKG_CHECK_MODULES(GLADE_SHARP, glade-sharp-2.0 >= 2.10.4)
AC_SUBST(GLADE_SHARP_LIBS)

hildondesktoplibdir=`pkg-config hildon-desktop --variable=hildondesktoplibdir`
AC_SUBST(hildondesktoplibdir)
homedesktopentrydir=`pkg-config osso-af-settings --variable=homedesktopentrydir`
AC_SUBST(homedesktopentrydir)

dnl API version
API_VERSION="1.0.0.0"
AC_SUBST(API_VERSION)

AC_OUTPUT([
Makefile
debian/homeip-sharp.install
homeip-sharp/Makefile
])

echo ""
echo "Configuration summary"
echo "---------------------"
echo ""
echo "   * Installation prefix: $prefix"
echo "   * compiler: $CSC"
echo ""
