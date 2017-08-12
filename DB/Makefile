TARGET = MACS.DB.dll
SRCS = DBTableDef.cs DBColumnDef.cs DBIndexDef.cs DBCon.cs DBConPool.cs DBTable.cs DBCondition.cs DBReader.cs DBDump.cs \
	A5erToDBTableDef.cs DBTableJson.cs
LIBDIR = ..
LOCALLIBS = $(LIBDIR)/MACS.dll
IMPORTLIBS = MySql.Data.dll Npgsql.dll Ionic.Zip.dll
LIBS = $(LOCALLIBS) System.Data $(IMPORTLIBS)
DOXFILES = Index.dox examples/Example.dox
DOCDIR = ../doc/DB
DOCFILE = $(DOCDIR)/index.html

MCS = mcs
MCSFLAGS = -warn:4 -debug+ -d:USE_MYSQL -d:USE_POSTGRESQL
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf
MKDIR = mkdir -p

DOXYGEN = doxygen


.PHONY: all clean release doc


all: $(TARGET)

doc: $(DOCFILE)

$(TARGET): $(SRCS) Makefile $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$@
	cp $(TARGET) $(TARGET).mdb $(IMPORTLIBS) $(LIBDIR)

$(DOCFILE): $(SRCS) $(DOXFILES) Doxyfile Makefile
	$(MKDIR) $(DOCDIR)
	$(DOXYGEN)

clean:
	$(RM) $(TARGET) *.mdb $(LIBDIR)/$(TARGET) $(LIBDIR)/$(TARGET).mdb $(IMPORTLIBS:%=$(LIBDIR)/%) $(DOCDIR)

release:
	$(MCS) $(MCSFLAGS_RELEASE) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$(TARGET)
