TARGET = MACS.HttpServer.dll
SRCS = 	HttpServer.cs HttpPage.cs HttpBuiltinContentsSupport.cs HttpNlsSupport.cs\
	HttpStaticPage.cs HttpTemplatePage.cs HttpValidationPage.cs\
	WebControl.cs TemplateFlag.cs Literal.cs TextBox.cs PasswordBox.cs HiddenText.cs\
	CheckBox.cs RadioButton.cs Button.cs SubmitButton.cs DropDown.cs TextArea.cs\
        FileInputBox.cs HttpPostedFile.cs HtmlCalender.cs TranslatableWebControl.cs\
	RadioSelector.cs DropDownSelector.cs YesNoSelector.cs DateSelector.cs\
	TimeSelector.cs\
	HttpStackTracePage.cs HttpEmergencyPage.cs\
	HttpProxy.cs
LIBDIR = ..
LOCALLIBS = $(LIBDIR)/MACS.dll
LIBS = $(LOCALLIBS) System.Web.dll System.Drawing.dll
DOXFILES = Index.dox examples/Example.dox
DOCDIR = ../doc/HttpServer
DOCFILE = $(DOCDIR)/index.html

MCS = mcs
MCSFLAGS = -warn:4 -debug+
MCSFLAGS_RELEASE = -optimize+

RM = rm -rf
MKDIR = mkdir -p

DOXYGEN = doxygen


.PHONY: all clean release doc


all: $(TARGET) doc

doc: $(DOCFILE)

$(TARGET): $(SRCS) Makefile $(LOCALLIBS)
	$(MCS) $(MCSFLAGS) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$@
	cp $(TARGET) $(TARGET).mdb $(LIBDIR)

$(DOCFILE): $(SRCS) $(DOXFILES) Doxyfile Makefile
	$(MKDIR) $(DOCDIR)
	$(DOXYGEN)

clean:
	$(RM) *.dll *.mdb $(LIBDIR)/$(TARGET) $(LIBDIR)/$(TARGET).mdb $(DOCDIR)

release:
	$(MCS) $(MCSFLAGS_RELEASE) -t:library $(LIBS:%=-r:%) $(SRCS) -out:$(TARGET)
