TARGET = PDFReport.dll
SRCS = PDFReport.cs
LIBS = ../SCSLib/SCSLib.dll ../iText/itextsharp.dll
DOXFILES = Index.dox examples/Example.dox
DOCDIR = ../../doc/html/PDFReport
DOCFILE = $(DOCDIR)/index.html

MCS = dmcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf
MKDIR = mkdir -p

DOXYGEN = doxygen


.PHONY: all clean release doc


all: $(TARGET) PDFReport.exe PDFConcat.exe

doc: $(DOCFILE)

$(TARGET): $(SRCS) $(LIBS) Makefile
	$(MCS) $(MCSFLAGS) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$@

PDFReport.exe: Main.cs $(SRCS) $(LIBS) Makefile
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) Main.cs $(SRCS) -out:$@

PDFConcat.exe: PDFConcat.cs $(SRCS) $(LIBS) Makefile
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) PDFConcat.cs $(SRCS) -out:$@

clean:
	$(RM) $(TARGET) *.mdb $(DOCDIR)

$(DOCFILE):	$(SRCS) $(DOXFILES) Doxyfile Makefile
	$(MKDIR) $(DOCDIR)
	$(DOXYGEN)

release:
	$(MCS) $(MCSFLAGS_RELEASE) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$(TARGET)
