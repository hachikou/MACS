TARGET = Example1.exe Example2.exe Example3.exe Example4.exe Example5.exe\
	Example6.exe Example7.exe Example8.exe Example9.exe Example10.exe\
	Example11.exe Example12.exe Example13.exe Example14.exe Example15.exe
LOCALLIBS = ../../SCSLib/SCSLib.dll ../HttpServer.dll
LIBS = $(LOCALLIBS) System.Web.dll

MCS = gmcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf


.PHONY: all clean


all: $(TARGET)

Example10.exe: Example10.cs html.cs $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) Example10.cs html.cs -out:$@

%.exe: %.cs $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) $< -out:$@

clean:
	$(RM) $(TARGET) *.mdb
