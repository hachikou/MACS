TARGET =Example1_1.pdf Example1_1_2.pdf Example1_2.pdf Example1_3.pdf \
	Example1_4.pdf Example1_5.pdf Example1_6.pdf Example1_7.pdf\
	Example1_8.pdf Example1_8_2.pdf Example1_9.pdf Example1_10.pdf\
	Example1_11.pdf Example1_12.pdf Example1_12_2.pdf Example1_13.pdf\
	Example1_14.pdf Example1_15.pdf Example1_16.pdf Example1_17.pdf
LOCALLIBS = ../../SCSLib/SCSLib.dll ../../iText/itextsharp.dll ../PDFReport.dll
LIBS = $(LOCALLIBS)

MCS = dmcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf


.PHONY: all doc clean


all: $(TARGET) doc

%.exe: %.cs $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) $(LIBS:%=-r:%) $< -out:$@

%.pdf: %.xml example.ini example.csv $(LOCALLIBS)
	../PDFReport $< example.ini example.csv -o $@

Example1_1_2.pdf: Example1_1.xml $(LOCALLIBS)
	../PDFReport -f $< -o $@

Example1_8_2.pdf: Example1_8.xml $(LOCALLIBS)
	../PDFReport -r $< -o $@

Example1_10.pdf: Example1_10.xml example.txt $(LOCALLIBS)
	../PDFReport $< example.txt -o $@

Example1_11.pdf: Example1_11.xml example2.ini $(LOCALLIBS)
	../PDFReport $< example2.ini -o $@

Example1_16.pdf: Example1_16.xml example.ini example.csv example2.csv $(LOCALLIBS)
	../PDFReport $< example.ini example.csv example2.csv -o $@

Example1_17.pdf: Example1_17.xml example.ini example2.csv $(LOCALLIBS)
	../PDFReport $< example.ini example2.csv -o $@

doc:
	$(MAKE) -C .. doc

clean:
	$(RM) $(TARGET) *.mdb
