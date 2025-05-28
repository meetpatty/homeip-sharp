MCS=$(CSC)

ASSEMBLY_NAME= homeip-sharp
ASSEMBLY= $(ASSEMBLY_NAME).dll

sources = *.cs

CLEANFILES = $(ASSEMBLY)

noinst_DATA = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(sources)
	$(MCS) -target:library $(LIBHILDONDESKTOP_SHARP_LIBS) \
    $(GLADE_SHARP_LIBS) -r:Mono.Cairo \
	$(sources) -out:$(ASSEMBLY)

install:
	mkdir -p $(DESTDIR)@hildondesktoplibdir@
	cp -a $(ASSEMBLY) $(DESTDIR)@hildondesktoplibdir@
	mkdir -p $(DESTDIR)@homedesktopentrydir@
	cp -a homeip-sharp.desktop $(DESTDIR)@homedesktopentrydir@
	mkdir -p "$(DESTDIR)/usr/lib/homeip-sharp/"
	cp -a "accept.png" "$(DESTDIR)/usr/lib/homeip-sharp/accept.png"
	cp -a "back.png" "$(DESTDIR)/usr/lib/homeip-sharp/back.png"
	cp -a "prefs.glade" "$(DESTDIR)/usr/lib/homeip-sharp/prefs.glade"

EXTRA_DIST = $(sources)
