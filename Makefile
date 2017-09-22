SUBDIRS := Core HttpServer DB

.PHONY: all doc clean

all:
	for d in $(SUBDIRS); do $(MAKE) -C $$d; done

doc:
	for d in $(SUBDIRS); do $(MAKE) -C $$d doc; done

clean:
	for d in $(SUBDIRS); do $(MAKE) -C $$d clean; done
