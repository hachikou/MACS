TARGET = DB.dll
SRCS = DBTableDef.cs DBColumnDef.cs DBIndexDef.cs DBCon.cs DBConPool.cs DBTable.cs DBCondition.cs DBReader.cs DBDump.cs \
	A5erToDBTableDef.cs DBTableJson.cs
LIBDIR = ../../lib
LIBS = System.Data $(LIBDIR)/SCSLib.dll $(LIBDIR)/Npgsql.dll $(LIBDIR)/Ionic.Zip.dll
DOXFILES = Index.dox examples/Example.dox
DOCDIR = ../../doc/DB
DOCFILE = $(DOCDIR)/index.html

MCS = mcs
MCSFLAGS = -warn:4 -debug+ -d:USE_POSTGRESQL
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf
MKDIR = mkdir -p

DOXYGEN = doxygen


.PHONY: all clean release doc


all: $(TARGET)

doc: $(DOCFILE)

$(TARGET): $(SRCS) Makefile
	$(MCS) $(MCSFLAGS) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$@
	cp $(TARGET) $(LIBDIR)

$(DOCFILE): $(SRCS) $(DOXFILES) Doxyfile Makefile
	$(MKDIR) $(DOCDIR)
	$(DOXYGEN)

clean:
	$(RM) $(TARGET) *.mdb

release:
	$(MCS) $(MCSFLAGS_RELEASE) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$(TARGET)
