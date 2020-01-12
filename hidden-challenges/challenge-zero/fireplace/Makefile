cn := challenge-zero
build:
	docker build -t $(cn) .

run:
	docker run --rm -ti -p 10000:10000 $(cn)

shell:
	docker run --rm -ti -p 10000:10000 -v $$PWD:/data $(cn) /bin/bash
