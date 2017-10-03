TARGET = PDFReport.dll
SRCS = PDFReport.cs
LIBDIR = ..
LOCALLIBS = $(LIBDIR)/MACS.dll
IMPORTLIBS = itextsharp.dll
LIBS = $(LOCALLIBS) $(IMPORTLIBS)
DOXFILES = Index.dox examples/Example.dox
DOCDIR = ../doc/PDFReport
DOCFILE = $(DOCDIR)/index.html

MCS = mcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf
MKDIR = mkdir -p

DOXYGEN = doxygen


.PHONY: all clean release doc


all: $(TARGET) PDFReport.exe PDFConcat.exe

doc: $(DOCFILE)

$(TARGET): $(SRCS) Makefile $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$@
	cp $(TARGET) $(TARGET).mdb $(IMPORTLIBS) $(LIBDIR)

PDFReport.exe: Main.cs $(SRCS) $(LIBS) Makefile
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) Main.cs $(SRCS) -out:$@

PDFConcat.exe: PDFConcat.cs $(SRCS) $(LIBS) Makefile
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) PDFConcat.cs $(SRCS) -out:$@

$(DOCFILE): $(SRCS) $(DOXFILES) Doxyfile Makefile
	$(MKDIR) $(DOCDIR)
	$(DOXYGEN)

clean:
	$(RM) $(TARGET) *.mdb *.exe $(LIBDIR)/$(TARGET) $(LIBDIR)/$(TARGET).mdb $(IMPORTLIBS:%=$(LIBDIR)/%) $(DOCDIR)

release:
	$(MCS) $(MCSFLAGS_RELEASE) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$(TARGET)
