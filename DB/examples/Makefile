TARGET = Example1.exe Example2.exe Example3.exe Example4.exe Example5.exe\
         Example6.exe Example7.exe Example8.exe Example9.exe Example10.exe\
         Example11.exe Example12.exe Example13.exe Example14.exe Example15.exe\
         Example16.exe Example17.exe
LOCALLIBS = ../../SCSLib/SCSLib.dll ../DB.dll
LIBS = $(LOCALLIBS)

MCS = dmcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf


.PHONY: all doc clean


all: $(TARGET)

%.exe: %.cs $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) $< -out:$@

doc:
	$(MAKE) -C .. doc

clean:
	$(RM) $(TARGET) *.mdb
