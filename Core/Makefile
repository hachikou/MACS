TARGET = MACS.dll
SRCS = UnixTime.cs StringUtil.cs MathUtil.cs NetworkByteOrder.cs ByteUtil.cs\
       SimpleCipher.cs ComplexCipher.cs\
       FileUtil.cs IniFile.cs SectionIniFile.cs ConfDict.cs\
       Ipaddr.cs IpaddrRange.cs Hwaddr.cs HwaddrRange.cs\
       OpeLog.cs Loggable.cs StaticLoggable.cs ObjectDictionary.cs\
       TempStream.cs TinyChunkStream.cs TelnetStream.cs\
       HtmlTool.cs TableFormatter.cs JsonTool.cs NDJson.cs\
       SendMail.cs Translator.cs Translatable.cs\
       XmlFile.cs CascadedAttribute.cs NumberList.cs ExTimeSpan.cs NumberRange.cs\
       LinedStringBuilder.cs CSVFile.cs SJISWriter.cs SJISReader.cs\
       ProcUtil.cs ThreadBase.cs DataArray.cs SJISDictionary.cs\
       SocStream.cs SocStream_Server.cs SocError.cs\
       FtpClient.cs Syslog.cs\
       ImageFileUtil.cs\
       RWLock.cs NThread.cs\
       DebugTool.cs
LIBDIR = ..
LIBS = System.Web.dll System.Drawing.dll
DOXFILES = Index.dox
DOCDIR = ../doc/Core
DOCFILE = $(DOCDIR)/index.html

MCS = mcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf
MKDIR = mkdir -p

DOXYGEN = doxygen


.PHONY: all clean release doc


all: $(TARGET)

doc: $(DOCFILE)

$(TARGET): $(SRCS) Makefile
	$(MCS) $(MCSFLAGS) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$@
	cp $(TARGET) $(TARGET).mdb $(LIBDIR)

$(DOCFILE): $(SRCS) $(DOXFILES) Doxyfile
	$(MKDIR) $(DOCDIR)
	$(DOXYGEN)

clean:
	$(RM) *.dll *.mdb $(LIBDIR)/$(TARGET) $(LIBDIR)/$(TARGET).mdb $(DOCDIR)

release:
	$(MCS) $(MCSFLAGS_RELEASE) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$(TARGET)

SJISDictionary.cs: makeSJISDictionary.exe SJISDictionary.cs.in
	mono --debug makeSJISDictionary.exe

makeSJISDictionary.exe: makeSJISDictionary.cs FileUtil.cs
	$(MCS) $(MCSFLAGS) makeSJISDictionary.cs FileUtil.cs -out:$@

SocError.cs: makeSocError SocError.txt
	./makeSocError

NDJsonTest.exe: NDJson.cs StringUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST NDJson.cs StringUtil.cs -out:$@

IpaddrTest.exe: Ipaddr.cs StringUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST -r:System.Web.dll Ipaddr.cs StringUtil.cs -out:$@

CascadedAttributeTest.exe: CascadedAttributeTest.cs CascadedAttribute.cs StringUtil.cs XmlFile.cs FileUtil.cs
	$(MCS) $(MCSFLAGS) $^ -out:$@

ObjectDictionaryTest.exe: ObjectDictionary.cs StringUtil.cs DataArray.cs FileUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST $^ -out:$@

IniFileTest.exe: IniFile.cs StringUtil.cs FileUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST IniFile.cs StringUtil.cs FileUtil.cs -out:$@

SectionIniFileTest.exe: SectionIniFile.cs IniFile.cs StringUtil.cs FileUtil.cs
	$(MCS) $(MCSFLAGS) -define:SECTIONSELFTEST SectionIniFile.cs IniFile.cs StringUtil.cs FileUtil.cs -out:$@

NumberListTest.exe: NumberList.cs StringUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST NumberList.cs StringUtil.cs -out:$@

NumberRangeTest.exe: NumberRange.cs StringUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST NumberRange.cs StringUtil.cs -out:$@

ConfDictTest.exe: ConfDict.cs StringUtil.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST ConfDict.cs StringUtil.cs -out:$@

ExTimeSpanTest.exe: ExTimeSpan.cs
	$(MCS) $(MCSFLAGS) -define:SELFTEST ExTimeSpan.cs -out:$@
